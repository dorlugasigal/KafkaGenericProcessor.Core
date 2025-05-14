using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Exceptions;
using KafkaGenericProcessor.Core.Logging;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Middlewares
{
    /// <summary>
    /// Middleware that processes messages using generic processors
    /// </summary>
    /// <typeparam name="TInput">The type of input message</typeparam>
    /// <typeparam name="TOutput">The type of output message</typeparam>
    public class GenericProcessingMiddleware<TInput, TOutput> : IMessageMiddleware
        where TInput : class
        where TOutput : class
    {
        private readonly IMessageProcessor<TInput, TOutput>? _processor;
        private readonly IConsumerOnlyProcessor<TInput>? _consumerOnlyProcessor;
        private readonly IMessageValidator<TInput> _messageValidator;
        private readonly IProducerAccessor _producerAccessor;
        private readonly KafkaProcessorSettings _settings;
        private readonly ILogger<GenericProcessingMiddleware<TInput, TOutput>> _logger;
        private readonly string _inputTypeName = typeof(TInput).Name;
        private readonly string _outputTypeName = typeof(TOutput).Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericProcessingMiddleware{TInput, TOutput}"/> class
        /// </summary>
        /// <param name="processor">The message processor</param>
        /// <param name="consumerOnlyProcessor">The consumer-only processor</param>
        /// <param name="messageValidator">The message validator</param>
        /// <param name="producerAccessor">The producer accessor</param>
        /// <param name="settings">The processor settings</param>
        /// <param name="logger">The logger</param>
        public GenericProcessingMiddleware(
            IMessageProcessor<TInput, TOutput>? processor,
            IConsumerOnlyProcessor<TInput>? consumerOnlyProcessor,
            IMessageValidator<TInput> messageValidator,
            IProducerAccessor producerAccessor,
            KafkaProcessorSettings settings,
            ILogger<GenericProcessingMiddleware<TInput, TOutput>> logger)
        {
            _processor = processor;
            _consumerOnlyProcessor = consumerOnlyProcessor;
            _messageValidator = messageValidator ?? throw new ArgumentNullException(nameof(messageValidator));
            _producerAccessor = producerAccessor ?? throw new ArgumentNullException(nameof(producerAccessor));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes the message
        /// </summary>
        /// <param name="context">The message context</param>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
        {
            // Generate a new correlation ID for this processing
            string correlationId = Guid.NewGuid().ToString();
            
            // Create log context for structured logging
            var logContext = new LogContext(correlationId, _inputTypeName, "ProcessMessage");
            logContext.StartMeasurement();

            try
            {
                _logger.LogStructuredDebug(logContext, "Processing message of type {Type}", _inputTypeName);

                var inputMessage = context.Message.Value as TInput
                    ?? throw new SerializationException(
                        $"Invalid message type. Expected {_inputTypeName}",
                        typeof(TInput),
                        correlationId,
                        false);

                // Validate message
                await ValidateMessageAsync(inputMessage, correlationId, context.ConsumerContext.WorkerId);

                // Get message key for producing
                var messageKey = context.Message.Key ?? correlationId;

                if (_settings.IsConsumerOnly)
                {
                    await ProcessConsumerOnlyMessageAsync(inputMessage, correlationId, logContext);
                }
                else
                {
                    await ProcessAndProduceMessageAsync(inputMessage, messageKey.ToString(), correlationId, logContext);
                }

                // Log performance metrics
                var elapsedMs = logContext.StopMeasurement();
                _logger.LogPerformance(logContext, "ProcessMessage", elapsedMs);
            }
            catch (ValidationException ex)
            {
                logContext.StopMeasurement();
                _logger.LogStructuredWarning(logContext, ex, 
                    "Message validation failed for type {Type}. CorrelationId: {CorrelationId}",
                    _inputTypeName, correlationId);
            }
            catch (ProcessingException ex)
            {
                logContext.StopMeasurement();
                _logger.LogStructuredError(logContext, ex, 
                    "Error processing message of type {Type}. CorrelationId: {CorrelationId}",
                    _inputTypeName, correlationId);
            }
            catch (Exception ex)
            {
                logContext.StopMeasurement();
                _logger.LogStructuredError(logContext, ex, 
                    "Unhandled error processing message with producer '{ProducerName}'. CorrelationId: {CorrelationId}",
                    _settings.ProducerName, correlationId);
            }

            // Always call the next middleware
            await next(context);
        }

        /// <summary>
        /// Validates the input message
        /// </summary>
        /// <param name="inputMessage">The input message to validate</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <param name="workerId">The worker ID for logging</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        private async Task ValidateMessageAsync(TInput inputMessage, string correlationId, int workerId)
        {
            var validationContext = new LogContext(correlationId, _inputTypeName, "ValidateMessage");
            validationContext.AddProperty("WorkerId", workerId);
            validationContext.StartMeasurement();

            try
            {
                var isValid = await _messageValidator.ValidateAsync(inputMessage, correlationId);
                var elapsedMs = validationContext.StopMeasurement();

                if (!isValid)
                {
                    var errors = await _messageValidator.GetValidationErrorsAsync(inputMessage, correlationId);
                    _logger.LogStructuredWarning(validationContext, 
                        "Message validation failed for type {Type}. CorrelationId: {CorrelationId}, WorkerId: {WorkerId}",
                        _inputTypeName, correlationId, workerId);

                    throw new ValidationException($"Validation failed for message of type {_inputTypeName}", errors, correlationId);
                }

                _logger.LogPerformance(validationContext, "ValidateMessage", elapsedMs);
            }
            catch (Exception ex) when (ex is not ValidationException)
            {
                var elapsedMs = validationContext.StopMeasurement();
                _logger.LogStructuredError(validationContext, ex, 
                    "Error during message validation for type {Type}. CorrelationId: {CorrelationId}, WorkerId: {WorkerId}",
                    _inputTypeName, correlationId, workerId);

                throw new ValidationException($"Error validating message of type {_inputTypeName}", ex);
            }
        }

        /// <summary>
        /// Processes a consumer-only message with retries
        /// </summary>
        /// <param name="inputMessage">The input message to process</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <param name="logContext">The log context</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ProcessingException">Thrown when processing fails</exception>
        private async Task ProcessConsumerOnlyMessageAsync(TInput inputMessage, string correlationId, LogContext logContext)
        {
            if (_consumerOnlyProcessor == null)
            {
                _logger.LogStructuredWarning(logContext, 
                    "Consumer-only processor not found for type {Type}. CorrelationId: {CorrelationId}",
                    _inputTypeName, correlationId);
                
                return;
            }

            try
            {
                await _consumerOnlyProcessor.ProcessAsync(inputMessage, correlationId);
                _logger.LogStructuredInformation(logContext, 
                    "Message processed successfully without producing output. CorrelationId: {CorrelationId}",
                    correlationId);
            }
            catch (Exception ex)
            {
                throw new ProcessingException(
                    $"Failed to process consumer-only message of type {_inputTypeName}",
                    typeof(TInput),
                    correlationId,
                    ex);
            }
        }

        /// <summary>
        /// Processes a message and produces output to another topic with retries
        /// </summary>
        /// <param name="inputMessage">The input message to process</param>
        /// <param name="messageKey">The message key</param>
        /// <param name="correlationId">The correlation ID</param>
        /// <param name="logContext">The log context</param>
        /// <returns>A task representing the asynchronous operation</returns>
        /// <exception cref="ProcessingException">Thrown when processing fails</exception>
        private async Task ProcessAndProduceMessageAsync(TInput inputMessage, string messageKey, string correlationId, LogContext logContext)
        {
            if (_processor == null)
            {
                _logger.LogStructuredWarning(logContext, 
                    "Processor not found for type {Type}. CorrelationId: {CorrelationId}",
                    _inputTypeName, correlationId);
                
                return;
            }

            try
            {
                var producer = _producerAccessor.GetProducer(_settings.ProducerName) 
                    ?? throw new KafkaConnectionException(
                        $"Producer '{_settings.ProducerName}' not found",
                        _settings.Brokers,
                        correlationId);

                var outputMessage = await _processor.ProcessAsync(inputMessage, correlationId);

                // Produce the message to the output topic
                await producer.ProduceAsync(_settings.ProducerTopic, messageKey, outputMessage);

                _logger.LogStructuredInformation(logContext, 
                    "Message processed and produced to topic: {Topic} using producer: {ProducerName}. CorrelationId: {CorrelationId}",
                    _settings.ProducerTopic, _settings.ProducerName, correlationId);
            }
            catch (KafkaConnectionException ex)
            {
                // Rethrow connection exceptions as they require infrastructure attention
                throw;
            }
            catch (Exception ex)
            {
                throw new ProcessingException(
                    $"Failed to process message of type {_inputTypeName} and produce {_outputTypeName}",
                    typeof(TInput),
                    correlationId,
                    ex);
            }
        }
    }
}
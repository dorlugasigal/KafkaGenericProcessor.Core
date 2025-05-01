using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace KafkaGenericProcessor.Core.Middlewares;

/// <summary>
/// Generic middleware for processing Kafka messages
/// </summary>
/// <typeparam name="TInput">The input message type</typeparam>
/// <typeparam name="TOutput">The output message type</typeparam>
public class GenericProcessingMiddleware<TInput, TOutput> : IMessageMiddleware
{
    private readonly IMessageProcessor<TInput, TOutput> _processor;
    private readonly IMessageValidator<TInput> _validator;
    private readonly IProducerAccessor _producerAccessor;
    private readonly KafkaProcessorSettings _settings;
    private readonly ILogger<GenericProcessingMiddleware<TInput, TOutput>> _logger;
    private readonly string _inputTypeName;

    /// <summary>
    /// Creates a new instance of GenericProcessingMiddleware
    /// </summary>
    public GenericProcessingMiddleware(
        IMessageProcessor<TInput, TOutput> processor,
        IMessageValidator<TInput> validator,
        IProducerAccessor producerAccessor,
        IOptions<KafkaProcessorSettings> settings,
        ILogger<GenericProcessingMiddleware<TInput, TOutput>> logger)
    {
        _processor = processor ?? throw new ArgumentNullException(nameof(processor));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _producerAccessor = producerAccessor ?? throw new ArgumentNullException(nameof(producerAccessor));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _inputTypeName = typeof(TInput).Name;
        
        _logger.LogInformation(
            "Middleware initialized for processing {InputType} to {OutputType}",
            _inputTypeName,
            typeof(TOutput).Name);
    }

    /// <summary>
    /// Processes a message using the configured processor and validator
    /// </summary>
    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        try
        {
            // Check if the message is of the expected type
            if (context.Message.Value is not TInput inputMessage)
            {
                _logger.LogWarning(
                    "Received message with unsupported format. Expected {ExpectedType}, got {ActualType}",
                    _inputTypeName,
                    context.Message.Value?.GetType().Name ?? "null");
                await next(context);
                return;
            }

            _logger.LogDebug("Processing message of type {Type}", _inputTypeName);
                
            // Validate the message
            if (!await _validator.ValidateAsync(inputMessage))
            {
                _logger.LogWarning("Message validation failed");
                await next(context);
                return;
            }

            var outputMessage = await _processor.ProcessAsync(inputMessage);
            
            var messageKey = context.Message.Key ?? Guid.NewGuid().ToString();

            try
            {
                var producer = _producerAccessor.GetProducer(_settings.ProducerName) 
                    ?? throw new InvalidOperationException($"Producer '{_settings.ProducerName}' not found");
                await producer.ProduceAsync(_settings.ProducerTopic, messageKey, outputMessage);
                
                _logger.LogInformation(
                    "Message processed and produced to topic: {Topic}",
                    _settings.ProducerTopic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error producing message to topic '{Topic}'", _settings.ProducerTopic);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }

        await next(context);
    }
}
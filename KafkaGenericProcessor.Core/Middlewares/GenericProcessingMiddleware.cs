using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
        
        // Log the middleware settings at startup
        _logger.LogInformation(
            "Middleware initialized with settings: ProducerName={ProducerName}, ProducerTopic={ProducerTopic}, ConsumerTopic={ConsumerTopic}",
            _settings.ProducerName,
            _settings.ProducerTopic,
            _settings.ConsumerTopic);
    }

    /// <summary>
    /// Processes a message using the configured processor and validator
    /// </summary>
    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        try
        {
            _logger.LogDebug("Processing message from topic: {Topic}", context.ConsumerContext.Topic);

            // Check if the message is of the expected type
            if (context.Message.Value is TInput inputMessage)
            {
                _logger.LogDebug("Received message of type {Type}: {Message}", 
                    typeof(TInput).Name, 
                    System.Text.Json.JsonSerializer.Serialize(inputMessage));
                
                // Validate the message
                if (await _validator.ValidateAsync(inputMessage))
                {
                    _logger.LogDebug("Message validated successfully");

                    // Process the message
                    var outputMessage = await _processor.ProcessAsync(inputMessage);
                    _logger.LogDebug("Message processed successfully: {OutputMessage}", 
                        System.Text.Json.JsonSerializer.Serialize(outputMessage));

                    // Get message key from context or create a new one
                    var messageKey = context.Message.Key ?? Guid.NewGuid().ToString();
                    _logger.LogDebug("Using message key: {Key}", messageKey);

                    try
                    {
                        // Get the producer
                        _logger.LogDebug("Attempting to get producer with name: {ProducerName}", _settings.ProducerName);
                        var producer = _producerAccessor.GetProducer(_settings.ProducerName);
                        
                        if (producer == null)
                        {
                            _logger.LogError("Producer '{ProducerName}' not found", _settings.ProducerName);
                            throw new InvalidOperationException($"Producer '{_settings.ProducerName}' not found");
                        }

                        _logger.LogDebug(
                            "Producer found. Attempting to produce to topic: {Topic} with key: {Key}", 
                            _settings.ProducerTopic, 
                            messageKey);

                        // Produce the message to the output topic
                        await producer.ProduceAsync(_settings.ProducerTopic, messageKey, outputMessage);
                        
                        _logger.LogInformation(
                            "Message produced to topic: {Topic} with key: {Key}",
                            _settings.ProducerTopic, 
                            messageKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error producing message to topic '{Topic}' with key '{Key}'", 
                            _settings.ProducerTopic, 
                            messageKey);
                    }
                }
                else
                {
                    _logger.LogWarning("Message validation failed");
                }
            }
            else
            {
                _logger.LogWarning(
                    "Received message with unsupported format. Expected {ExpectedType}, got {ActualType}",
                    typeof(TInput).Name,
                    context.Message.Value?.GetType().Name ?? "null");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }

        // Always call next middleware in the pipeline
        await next(context);
    }
}
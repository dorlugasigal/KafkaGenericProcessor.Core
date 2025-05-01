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
public class GenericProcessingMiddleware<TInput, TOutput>(
    IMessageProcessor<TInput, TOutput> processor,
    IMessageValidator<TInput> validator,
    IProducerAccessor producerAccessor,
    IOptions<KafkaProcessorSettings> settings,
    ILogger<GenericProcessingMiddleware<TInput, TOutput>> logger) : IMessageMiddleware
{
    private readonly KafkaProcessorSettings _settings = settings.Value;
    private readonly string _inputTypeName = typeof(TInput).Name;


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
                logger.LogWarning(
                    "Received message with unsupported format. Expected {ExpectedType}, got {ActualType}",
                    _inputTypeName,
                    context.Message.Value?.GetType().Name ?? "null");
                await next(context);
                return;
            }

            logger.LogDebug("Processing message of type {Type}", _inputTypeName);
                
            // Validate the message
            if (!await validator.ValidateAsync(inputMessage))
            {
                logger.LogWarning("Message validation failed");
                await next(context);
                return;
            }

            var outputMessage = await processor.ProcessAsync(inputMessage);
            
            var messageKey = context.Message.Key ?? Guid.NewGuid().ToString();

            try
            {
                var producer = producerAccessor.GetProducer(_settings.ProducerName) 
                    ?? throw new InvalidOperationException($"Producer '{_settings.ProducerName}' not found");
                await producer.ProduceAsync(_settings.ProducerTopic, messageKey, outputMessage);
                
                logger.LogInformation(
                    "Message processed and produced to topic: {Topic}",
                    _settings.ProducerTopic);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error producing message to topic '{Topic}'", _settings.ProducerTopic);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message");
        }

        await next(context);
    }
}
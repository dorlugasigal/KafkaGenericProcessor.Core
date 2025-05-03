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
public class GenericProcessingMiddleware<TInput, TOutput>(
    IMessageProcessor<TInput, TOutput> processor,
    IMessageValidator<TInput> validator,
    IProducerAccessor producerAccessor,
    KafkaProcessorSettings settings,
    ILogger<GenericProcessingMiddleware<TInput, TOutput>> logger,
    string? producerName = null) : IMessageMiddleware
    where TInput : class
    where TOutput : class
{
    private readonly string _effectiveProducerName = producerName ?? settings.ProducerName;
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

            logger.LogDebug("Processing message of type {Type} with producer {ProducerName}", 
                _inputTypeName, _effectiveProducerName);
                
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
                var producer = producerAccessor.GetProducer(_effectiveProducerName) 
                    ?? throw new InvalidOperationException($"Producer '{_effectiveProducerName}' not found");
                await producer.ProduceAsync(settings.ProducerTopic, messageKey, outputMessage);
                
                logger.LogInformation(
                    "Message processed and produced to topic: {Topic} using producer: {ProducerName}",
                    settings.ProducerTopic,
                    _effectiveProducerName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error producing message to topic '{Topic}' with producer '{ProducerName}'", 
                    settings.ProducerTopic, _effectiveProducerName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message with producer '{ProducerName}'", _effectiveProducerName);
        }

        await next(context);
    }
}
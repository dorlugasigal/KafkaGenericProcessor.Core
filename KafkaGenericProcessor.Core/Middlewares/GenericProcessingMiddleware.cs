using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Validation;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Middlewares;

public class GenericProcessingMiddleware<TInput, TOutput>(
    IMessageProcessor<TInput, TOutput>? processor,
    IConsumerOnlyProcessor<TInput>? consumerOnlyProcessor,
    IMessageValidator<TInput> messageValidator,
    IProducerAccessor producerAccessor,
    KafkaProcessorSettings settings,
    ILogger<GenericProcessingMiddleware<TInput, TOutput>> logger) : IMessageMiddleware
    where TInput : class
    where TOutput : class
{
    private readonly string _inputTypeName = typeof(TInput).Name;

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        try
        {
            var inputMessage = context.Message.Value as TInput 
                ?? throw new InvalidOperationException($"Invalid message type. Expected {typeof(TInput).Name}"); 
    
            logger.LogDebug("Processing message of type {Type} ", _inputTypeName);

            if (messageValidator != null)
            {
                var isValid = await messageValidator.ValidateAsync(inputMessage);
                if (!isValid)
                {
                    logger.LogWarning("Message validation failed for type {Type}", _inputTypeName);
                    await next(context);
                    return;
                }
            }
            
            var messageKey = context.Message.Key ?? Guid.NewGuid().ToString();
            
            if (settings.IsConsumerOnly)
            {
                if (consumerOnlyProcessor == null)
                {
                    logger.LogWarning("Consumer-only processor not found for Type {Type}", _inputTypeName);
                    await next(context);
                    return;
                }
                await consumerOnlyProcessor!.ProcessAsync(inputMessage);
                logger.LogInformation("Message processed without producing output");
                await next(context);
                return;
            }

            try
            {
                if (processor == null)
                {
                    logger.LogWarning("Processor not found for Type {Type}", _inputTypeName);
                    await next(context);
                    return;
                }
                var producer = producerAccessor.GetProducer(settings.ProducerName) 
                    ?? throw new InvalidOperationException($"Producer '{settings.ProducerName}' not found");
                var outputMessage = await processor.ProcessAsync(inputMessage);
                await producer.ProduceAsync(settings.ProducerTopic, messageKey, outputMessage);
                
                logger.LogInformation(
                    "Message processed and produced to topic: {Topic} using producer: {ProducerName}",
                    settings.ProducerTopic,
                    settings.ProducerName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error producing message to topic '{Topic}' with producer '{ProducerName}'",
                    settings.ProducerTopic, settings.ProducerName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message with producer '{ProducerName}'", settings.ProducerName);
        }

        await next(context);
    }
}
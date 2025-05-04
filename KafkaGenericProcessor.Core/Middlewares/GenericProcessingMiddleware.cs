using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Middlewares;

public class GenericProcessingMiddleware<TInput, TOutput>(
    IMessageProcessor<TInput, TOutput> processor,
    IProducerAccessor producerAccessor,
    KafkaProcessorSettings settings,
    ILogger<GenericProcessingMiddleware<TInput, TOutput>> logger,
    string? producerName = null) : IMessageMiddleware
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
    
            logger.LogDebug("Processing message of type {Type} with producer {ProducerName}", 
                _inputTypeName, producerName);

            var outputMessage = await processor.ProcessAsync(inputMessage);
            
            var messageKey = context.Message.Key ?? Guid.NewGuid().ToString();

            try
            {
                var producer = producerAccessor.GetProducer(producerName) 
                    ?? throw new InvalidOperationException($"Producer '{producerName}' not found");
                await producer.ProduceAsync(settings.ProducerTopic, messageKey, outputMessage);
                
                logger.LogInformation(
                    "Message processed and produced to topic: {Topic} using producer: {ProducerName}",
                    settings.ProducerTopic,
                    producerName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error producing message to topic '{Topic}' with producer '{ProducerName}'", 
                    settings.ProducerTopic, producerName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message with producer '{ProducerName}'", producerName);
        }

        await next(context);
    }
}
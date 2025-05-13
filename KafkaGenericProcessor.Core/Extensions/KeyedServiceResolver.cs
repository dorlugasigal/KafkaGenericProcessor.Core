using KafkaGenericProcessor.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Extensions;

public class KeyedServiceResolver(IServiceProvider serviceProvider, ILogger<KeyedServiceResolver> logger)
{

    public IMessageProcessor<TInput, TOutput>? GetProcessor<TInput, TOutput>(string key)
        where TInput : class
        where TOutput : class
    {
        var processor = serviceProvider.GetKeyedService<IMessageProcessor<TInput, TOutput>>(key);
                              
        if (processor == null)
        {
            logger.LogInformation("processor with key '{Key}' not found", key);
        }
        return processor;
    }
    
    public IConsumerOnlyProcessor<TInput>? GetConsumerOnlyProcessor<TInput>(string key)
        where TInput : class
    {
        var processor = serviceProvider.GetKeyedService<IConsumerOnlyProcessor<TInput>>(key);
            
        if (processor == null)
        {
            logger.LogInformation("Consumer-only processor with key '{Key}' not found", key);
        }
        return processor;
    }

    public IMessageValidator<TInput>? GetMessageValidator<TInput>(string key)
        where TInput : class
    {
        var validator = serviceProvider.GetKeyedService<IMessageValidator<TInput>>(key);
            
        if (validator == null)
        {
            logger.LogInformation("Message validator with key '{Key}' not found", key);
        }
        return validator;
    }
}
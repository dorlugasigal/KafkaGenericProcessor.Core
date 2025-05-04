using KafkaGenericProcessor.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace KafkaGenericProcessor.Core.Extensions;

public class KeyedServiceResolver<TInput, TOutput>(IServiceProvider serviceProvider)
    where TInput : class
    where TOutput : class
{
    public IMessageProcessor<TInput, TOutput> GetProcessor(string key)
    {
        var processor = serviceProvider.GetKeyedService<IMessageProcessor<TInput, TOutput>>(key)
            ?? throw new InvalidOperationException($"Processor with key '{key}' not found");
        return processor;
    }

}
using KafkaGenericProcessor.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace KafkaGenericProcessor.Core.Extensions;

/// <summary>
/// Helper class to resolve keyed services in KafkaFlow
/// </summary>
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

    public IMessageValidator<TInput> GetValidator(string key)
    {
        var validator = serviceProvider.GetKeyedService<IMessageValidator<TInput>>(key)
            ?? throw new InvalidOperationException($"Validator with key '{key}' not found");
        return validator;
    }
}
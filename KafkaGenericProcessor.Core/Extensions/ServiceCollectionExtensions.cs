using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Middlewares;
using KafkaGenericProcessor.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace KafkaGenericProcessor.Core.Extensions;

/// <summary>
/// Extension methods for service collection to simplify registration of the generic Kafka processor
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a generic Kafka processor with the specified input and output message types
    /// </summary>
    /// <typeparam name="TInput">The input message type</typeparam>
    /// <typeparam name="TOutput">The output message type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure the Kafka processor options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddKafkaGenericProcessor<TInput, TOutput>(
        this IServiceCollection services,
        Action<KafkaProcessorOptions> configureOptions)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        // Create options and apply configuration
        var options = new KafkaProcessorOptions();
        configureOptions(options);

        // Create settings from options
        var settings = new KafkaProcessorSettings
        {
            Brokers = options.Brokers,
            ConsumerTopic = options.ConsumerTopic,
            ProducerTopic = options.ProducerTopic,
            GroupId = options.GroupId,
            WorkersCount = options.WorkersCount,
            BufferSize = options.BufferSize,
            ProducerName = options.ProducerName
        };

        // Register settings in DI
        services.AddSingleton(Options.Create(settings));

        // Register default validator if not registered
        if (!IsServiceRegistered<IMessageValidator<TInput>>(services))
        {
            services.AddTransient<IMessageValidator<TInput>, DefaultMessageValidator<TInput>>();
        }

        // Build Kafka processor
        var builder = new KafkaProcessorBuilder<TInput, TOutput>(services, settings);
        builder.Build();

        return services;
    }

    /// <summary>
    /// Checks if a service is already registered in the service collection
    /// </summary>
    private static bool IsServiceRegistered<TService>(IServiceCollection services)
    {
        return services.Any(s => s.ServiceType == typeof(TService));
    }
}
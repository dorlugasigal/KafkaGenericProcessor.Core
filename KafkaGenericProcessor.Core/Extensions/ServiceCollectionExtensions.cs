using System;
using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Health;
using KafkaGenericProcessor.Core.Middlewares;
using KafkaGenericProcessor.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace KafkaGenericProcessor.Core.Extensions;

/// <summary>
/// Extensions for service collection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add a Kafka Generic Processor with a specific input and output type using configuration from appsettings.json
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The IConfiguration instance</param>
    /// <param name="sectionName">The section name in configuration containing Kafka settings (default: "Kafka")</param>
    /// <typeparam name="TInput">The input message type</typeparam>
    /// <typeparam name="TOutput">The output message type</typeparam>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddKafkaGenericProcessor<TInput, TOutput>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Kafka")
        where TInput : class
        where TOutput : class
    {
        // Load the settings early for validation
        var settingsSection = configuration.GetSection(sectionName);
        var settings = new KafkaProcessorSettings();
        settingsSection.Bind(settings);
        
        // Validate settings
        KafkaConfigurationValidator.ValidateKafkaSettings(settings);
        
        // Register settings with the Options pattern - use the correct overload
        services.Configure<KafkaProcessorSettings>(options => 
            settingsSection.Bind(options));
        
        // Register the default message validator if none is registered
        services.TryAddTransient(typeof(IMessageValidator<TInput>), typeof(DefaultMessageValidator<TInput>));
        
        // Setup KafkaFlow
        services.AddKafka(kafka => 
        {
            kafka.UseConsoleLog();
            
            kafka.AddCluster(cluster =>
            {
                var clusterBuilder = cluster.WithBrokers(settings.Brokers);

                // Create topics if configured to do so
                if (settings.CreateTopicsIfNotExist)
                {
                    clusterBuilder
                        .CreateTopicIfNotExists(settings.ConsumerTopic)
                        .CreateTopicIfNotExists(settings.ProducerTopic)
                        .CreateTopicIfNotExists(settings.HealthCheckTopic);
                }

                // Add producer with the specified name
                clusterBuilder.AddProducer(settings.ProducerName, producer =>
                {
                    producer
                        .DefaultTopic(settings.ProducerTopic)
                        .AddMiddlewares(middlewares =>
                        {
                            middlewares.AddSerializer<JsonCoreSerializer>();
                        });
                });

                // Add consumer with optimized settings
                clusterBuilder.AddConsumer(consumer =>
                {
                    consumer
                        .Topic(settings.ConsumerTopic)
                        .WithGroupId(settings.GroupId)
                        .WithBufferSize(settings.BufferSize)
                        .WithWorkersCount(settings.WorkersCount)
                        .WithAutoCommitIntervalMs((int)settings.AutoCommitInterval.TotalMilliseconds)
                        .AddMiddlewares(middlewares =>
                        {
                            // Add JSON deserialization
                            middlewares.AddDeserializer<JsonCoreDeserializer>();
                            // Add our generic processing middleware
                            middlewares.Add<GenericProcessingMiddleware<TInput, TOutput>>();
                        });
                });
            });
        });
        
        return services;
    }
    
    /// <summary>
    /// Adds Kafka health checks to the service collection using standard configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddKafkaGenericProcessorHealthChecks(
        this IServiceCollection services)
    {
        // Add health checks with fixed configuration
        services.AddKafkaFlowHealthChecks(
            kafkaHealthCheckName: "kafka",
            producerName: "producer",
            healthCheckTopic: "kafka-health-check",
            timeout: TimeSpan.FromSeconds(3));
        
        return services;
    }
}
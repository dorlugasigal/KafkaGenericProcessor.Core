using System;
using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Serializer;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Health;
using KafkaGenericProcessor.Core.Middlewares;
using KafkaGenericProcessor.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        var settings = ConfigureSettings(services, configuration, sectionName);
        
        services.TryAddTransient(typeof(IMessageValidator<TInput>), typeof(DefaultMessageValidator<TInput>));
        
        ConfigureKafkaFlow<TInput, TOutput>(services, settings);
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
        services.AddKafkaFlowHealthChecks(
            kafkaCheckName: "kafka",
            producerName: "producer",
            healthCheckTopic: "kafka-health-check");
        
        return services;
    }

    /// <summary>
    /// Configures Kafka processor settings from configuration
    /// </summary>
    private static KafkaProcessorSettings ConfigureSettings(
        IServiceCollection services,
        IConfiguration configuration,
        string sectionName)
    {
        var settingsSection = configuration.GetSection(sectionName);
        var settings = new KafkaProcessorSettings();
        settingsSection.Bind(settings);
        
        KafkaConfigurationValidator.ValidateKafkaSettings(settings);
        
        services.Configure<KafkaProcessorSettings>(settingsSection.Bind);
        
        return settings;
    }
    
    /// <summary>
    /// Configures KafkaFlow with the specified settings
    /// </summary>
    private static void ConfigureKafkaFlow<TInput, TOutput>(
        IServiceCollection services,
        KafkaProcessorSettings settings)
        where TInput : class
        where TOutput : class
    {
        services.AddKafka(kafka => 
        {
            kafka.UseConsoleLog();
            
            kafka.AddCluster(cluster => 
            {
                var clusterBuilder = cluster.WithBrokers(settings.Brokers);

                if (settings.CreateTopicsIfNotExist)
                {
                    clusterBuilder
                        .CreateTopicIfNotExists(settings.ConsumerTopic)
                        .CreateTopicIfNotExists(settings.ProducerTopic)
                        .CreateTopicIfNotExists(settings.HealthCheckTopic);
                }

                ConfigureProducer(clusterBuilder, settings);
                ConfigureConsumer<TInput, TOutput>(clusterBuilder, settings);
            });
        });
    }

    /// <summary>
    /// Configures the Kafka producer
    /// </summary>
    private static void ConfigureProducer(
        IClusterConfigurationBuilder clusterBuilder,
        KafkaProcessorSettings settings)
    {
        clusterBuilder.AddProducer(settings.ProducerName, producer =>
        {
            producer
                .DefaultTopic(settings.ProducerTopic)
                .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>());
        });
    }

    /// <summary>
    /// Configures the Kafka consumer
    /// </summary>
    private static void ConfigureConsumer<TInput, TOutput>(
        IClusterConfigurationBuilder clusterBuilder,
        KafkaProcessorSettings settings) 
        where TInput : class
        where TOutput : class
    {
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
                    middlewares.AddDeserializer<JsonCoreDeserializer>();
                    middlewares.Add<GenericProcessingMiddleware<TInput, TOutput>>();
                });
        });
    }
}
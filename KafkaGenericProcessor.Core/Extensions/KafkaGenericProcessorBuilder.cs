using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Health;
using KafkaGenericProcessor.Core.Middlewares;
using KafkaGenericProcessor.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaGenericProcessor.Core.Extensions;

/// <summary>
/// Builder for configuring multiple Kafka Generic Processors in a fluent API
/// </summary>
public class KafkaGenericProcessorBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<ProcessorRegistration> _processors = new();
    private bool _addHealthCheck = false;
    
    public KafkaGenericProcessorBuilder(IServiceCollection services)
    {
        _services = services;
    }
    
    /// <summary>
    /// Add a processor with input and output types to the builder
    /// </summary>
    public KafkaGenericProcessorBuilder AddProcessor<TInput, TOutput>(
        IConfiguration configuration,
        string processorKey)
        where TInput : class
        where TOutput : class
    {
        if (string.IsNullOrEmpty(processorKey))
        {
            throw new ArgumentException("Processor key cannot be null or empty", nameof(processorKey));
        }

        var settings = ServiceCollectionExtensions.ConfigureSettings(_services, configuration, processorKey);
        
        // Register default validator if not already registered with the key
        _services.TryAddKeyedTransient(typeof(IMessageValidator<TInput>), processorKey, typeof(DefaultMessageValidator<TInput>));

        // Register a factory class to help resolve keyed services
        _services.AddSingleton<KeyedServiceResolver<TInput, TOutput>>();
        
        // Store processor registration for later configuration
        _processors.Add(new ProcessorRegistration
        {
            ProcessorKey = processorKey,
            Settings = settings,
            ConfigureAction = cluster => ConfigureKeyedServices<TInput, TOutput>(cluster, settings, processorKey)
        });
        
        return this;
    }

    /// <summary>
    /// Add health check configuration
    /// </summary>
    public KafkaGenericProcessorBuilder AddHealthCheck(IConfiguration configuration)
    {
        // Register health check settings from configuration
        _services.Configure<KafkaHealthCheckSettings>(configuration.GetSection("Kafka:Configurations:healthcheck").Bind);
        
        // Add health check registration with KafkaFlow
        _services.AddKafkaFlowHealthChecks();
        
        _addHealthCheck = true;
        return this;
    }

    /// <summary>
    /// Build the Kafka configuration and register it with the service collection
    /// </summary>
    public IServiceCollection Build()
    {
        if (_processors.Count == 0 && !_addHealthCheck)
        {
            return _services;
        }
        
        _services.AddKafka(kafka => 
        {
            kafka.UseConsoleLog();
            
            kafka.AddCluster(cluster => 
            {
                // Get unique set of brokers from all processor settings
                var allBrokers = _processors
                    .SelectMany(p => p.Settings.Brokers)
                    .ToList();
                
                // Add broker addresses from health check settings if needed
                if (_addHealthCheck)
                {
                    var healthSettings = _services.BuildServiceProvider()
                        .GetRequiredService<IOptions<KafkaHealthCheckSettings>>().Value;
                    
                    if (healthSettings.Brokers != null && healthSettings.Brokers.Any())
                    {
                        allBrokers.AddRange(healthSettings.Brokers);
                    }
                }
                
                // Add client configuration to handle version compatibility issues
                var clusterBuilder = cluster.WithBrokers(allBrokers.Distinct().ToArray());

                // Collect all topics that need to be created
                var topics = _processors
                    .SelectMany(p => new[] { p.Settings.ConsumerTopic, p.Settings.ProducerTopic })
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();
                
                // Add health check topic if needed
                if (_addHealthCheck)
                {
                    var healthSettings = _services.BuildServiceProvider()
                        .GetRequiredService<IOptions<KafkaHealthCheckSettings>>().Value;
                    
                    if (!string.IsNullOrEmpty(healthSettings.HealthCheckTopic))
                    {
                        topics.Add(healthSettings.HealthCheckTopic);
                    }
                }

                foreach (var topicName in topics.Distinct())
                {
                    clusterBuilder.CreateTopicIfNotExists(topicName);
                }
                
                // Configure processors
                foreach (var processor in _processors)
                {
                    string producerName = $"{processor.Settings.ProducerName}_{processor.ProcessorKey}";
                    ConfigureProducer(clusterBuilder, processor.Settings, producerName);
                    processor.ConfigureAction(clusterBuilder);
                }
                
                // Configure health check producer if needed
                if (_addHealthCheck)
                {
                    var healthSettings = _services.BuildServiceProvider()
                        .GetRequiredService<IOptions<KafkaHealthCheckSettings>>().Value;
                    
                    clusterBuilder.AddProducer(healthSettings.ProducerName, producer =>
                    {
                        producer
                            .DefaultTopic(healthSettings.HealthCheckTopic)
                            .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>());
                    });
                }
            });
        });
        
        return _services;
    }
    
    /// <summary>
    /// Configures the Kafka producer
    /// </summary>
    private static void ConfigureProducer(
        IClusterConfigurationBuilder clusterBuilder,
        KafkaProcessorSettings settings,
        string producerName)
    {
        clusterBuilder.AddProducer(producerName, producer =>
        {
            producer
                .DefaultTopic(settings.ProducerTopic)
                .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>());
        });
    }
    
    /// <summary>
    /// Configures keyed services for a specific processor
    /// </summary>
    private static void ConfigureKeyedServices<TInput, TOutput>(
        IClusterConfigurationBuilder clusterBuilder,
        KafkaProcessorSettings settings,
        string processorKey)
        where TInput : class
        where TOutput : class
    {
        var consumerName = $"consumer_{processorKey}";
        string producerName = $"{settings.ProducerName}_{processorKey}";
        
        clusterBuilder.AddConsumer(consumer =>
        {
            consumer
                .Topic(settings.ConsumerTopic)
                .WithName(consumerName)
                .WithGroupId($"{settings.GroupId}_{processorKey}")
                .WithBufferSize(settings.BufferSize)
                .WithWorkersCount(settings.WorkersCount)
                .WithAutoCommitIntervalMs((int)settings.AutoCommitInterval.TotalMilliseconds)
                .AddMiddlewares(middlewares =>
                {
                    middlewares.AddDeserializer<JsonCoreDeserializer>();
                    
                    // Create middleware using the KeyedServiceResolver to find the correct services
                    middlewares.Add(resolver => 
                    {
                        var serviceResolver = resolver.Resolve<KeyedServiceResolver<TInput, TOutput>>();
                        
                        var processor = serviceResolver.GetProcessor(processorKey);
                        var validator = serviceResolver.GetValidator(processorKey);
                        
                        return new GenericProcessingMiddleware<TInput, TOutput>(
                            processor,
                            validator,
                            resolver.Resolve<IProducerAccessor>(),
                            settings,
                            resolver.Resolve<ILogger<GenericProcessingMiddleware<TInput, TOutput>>>(),
                            producerName
                        );
                    });
                });
        });
    }
    
    /// <summary>
    /// Private class to store processor registration details
    /// </summary>
    private class ProcessorRegistration
    {
        public required string ProcessorKey { get; set; }
        public required KafkaProcessorSettings Settings { get; set; }
        public required Action<IClusterConfigurationBuilder> ConfigureAction { get; set; }
    }
}
using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Health;
using KafkaGenericProcessor.Core.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KafkaGenericProcessor.Core.Extensions;

public class KafkaGenericProcessorBuilder(IServiceCollection services, IConfiguration configuration)
{
    private readonly List<ProcessorRegistration> _processors = new();
    private bool _addHealthCheck;

    public KafkaGenericProcessorBuilder AddProcessor<TInput, TOutput>(
        string processorKey)
        where TInput : class
        where TOutput : class
    {
        if (string.IsNullOrEmpty(processorKey))
        {
            throw new ArgumentException("Processor key cannot be null or empty", nameof(processorKey));
        }

        var settings = ServiceCollectionExtensions.ConfigureSettings(services, configuration, processorKey);

        services.AddSingleton<KeyedServiceResolver<TInput, TOutput>>();
        
        _processors.Add(new ProcessorRegistration
        {
            ProcessorKey = processorKey,
            Settings = settings,
            ConfigureAction = cluster => ConfigureKeyedServices<TInput, TOutput>(cluster, settings, processorKey)
        });
        
        return this;
    }

    public KafkaGenericProcessorBuilder AddHealthCheck()
    {
        var settingsSection = configuration.GetSection($"Kafka:Configurations:healthcheck");
        var settings = new KafkaHealthCheckSettings();
        settingsSection.Bind(settings);
        
        KafkaConfigurationValidator.ValidateKafkaHealthCheckSettings(settings);
        
        services.Configure<KafkaHealthCheckSettings>("healthcheck", settingsSection.Bind);
        
        services.AddKafkaFlowHealthChecks();
        _addHealthCheck = true;
        return this;
    }

    public IServiceCollection Build()
    {
        if (!ShouldConfigureKafka())
            return services;

        var healthSettings = GetHealthCheckSettings();
        
        services.AddKafka(kafka => 
        {
            kafka.UseConsoleLog();
            
            kafka.AddCluster(cluster => 
            {
                var allBrokers = GetAllUniqueBrokers(healthSettings);
                var clusterBuilder = cluster.WithBrokers(allBrokers);

                ConfigureTopics(clusterBuilder, healthSettings);
                ConfigureProcessors(clusterBuilder);
                ConfigureHealthCheck(clusterBuilder, healthSettings);
            });
        });
        
        return services;
    }

    private bool ShouldConfigureKafka() => _processors.Count > 0 || _addHealthCheck;

    private KafkaHealthCheckSettings? GetHealthCheckSettings()
    {
        if (!_addHealthCheck)
            return null;
            
        return services.BuildServiceProvider()
            .GetRequiredService<IOptions<KafkaHealthCheckSettings>>()
            .Value;
    }

    private string[] GetAllUniqueBrokers(KafkaHealthCheckSettings? healthSettings)
    {
        var brokers = _processors
            .SelectMany(p => p.Settings.Brokers);

        if (healthSettings != null)
            brokers = brokers.Concat(healthSettings.Brokers);

        return brokers.Distinct().ToArray();
    }

    private void ConfigureTopics(IClusterConfigurationBuilder clusterBuilder, KafkaHealthCheckSettings? healthSettings)
    {
        var topics = _processors
            .SelectMany(p => new[] { p.Settings.ConsumerTopic, p.Settings.ProducerTopic })
            .Where(t => !string.IsNullOrEmpty(t));

        if (healthSettings != null)
            topics = topics.Append(healthSettings.ProducerTopic);

        foreach (var topicName in topics.Distinct())
        {
            clusterBuilder.CreateTopicIfNotExists(topicName);
        }
    }

    private void ConfigureProcessors(IClusterConfigurationBuilder clusterBuilder)
    {
        foreach (var processor in _processors)
        {
            string producerName = $"producer-{processor.Settings.ProducerTopic}_{processor.ProcessorKey}";
            ConfigureProducer(clusterBuilder, processor.Settings, producerName);
            processor.ConfigureAction(clusterBuilder);
        }
    }

    private void ConfigureHealthCheck(IClusterConfigurationBuilder clusterBuilder, KafkaHealthCheckSettings? healthSettings)
    {
        if (healthSettings == null)
            return;

        clusterBuilder.AddProducer(healthSettings.ProducerName, producer =>
        {
            producer
                .DefaultTopic(healthSettings.ProducerTopic)
                .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>());
        });
    }
    
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
    
    private static void ConfigureKeyedServices<TInput, TOutput>(
        IClusterConfigurationBuilder clusterBuilder,
        KafkaProcessorSettings settings,
        string processorKey)
        where TInput : class
        where TOutput : class
    {
        var consumerName = $"consumer_{processorKey}";
        string producerName = $"producer-{settings.ProducerTopic}_{processorKey}";
        
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
                    middlewares.AddSingleTypeDeserializer<TInput, JsonCoreDeserializer>();
                    
                    // Create middleware using the KeyedServiceResolver to find the correct services
                    middlewares.Add(resolver => 
                    {
                        var serviceResolver = resolver.Resolve<KeyedServiceResolver<TInput, TOutput>>();
                        
                        var processor = serviceResolver.GetProcessor(processorKey);
                        
                        return new GenericProcessingMiddleware<TInput, TOutput>(
                            processor,
                            resolver.Resolve<IProducerAccessor>(),
                            settings,
                            resolver.Resolve<ILogger<GenericProcessingMiddleware<TInput, TOutput>>>(),
                            producerName
                        );
                    });
                });
        });
    }
    
    private class ProcessorRegistration
    {
        public required string ProcessorKey { get; set; }
        public required KafkaProcessorSettings Settings { get; set; }
        public required Action<IClusterConfigurationBuilder> ConfigureAction { get; set; }
    }
}

using KafkaFlow;
using KafkaFlow.Configuration;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using KafkaGenericProcessor.Core.Health;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AutoOffsetReset = KafkaFlow.AutoOffsetReset;
using KafkaGenericProcessor.Core.Validation;

namespace KafkaGenericProcessor.Core.Extensions;

public class KafkaGenericProcessorBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaGenericProcessorBuilder> _logger;
    private readonly List<ProcessorRegistration> _processors;
    private bool _healthCheckEnabled;

    public KafkaGenericProcessorBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
        
        // Add logging services if not already added
        if (!_services.Any(x => x.ServiceType == typeof(ILoggerFactory)))
        {
            _services.AddLogging();
        }
        
        var serviceProvider = _services.BuildServiceProvider();
        _logger = serviceProvider.GetRequiredService<ILogger<KafkaGenericProcessorBuilder>>();
        _processors = new List<ProcessorRegistration>();
        _healthCheckEnabled = true;
    }

    public KafkaGenericProcessorBuilder AddProducer(string processorKey)
    {
        if (string.IsNullOrEmpty(processorKey))
        {
            throw new ArgumentException("Processor key cannot be null or empty", nameof(processorKey));
        }

        var settings = ServiceCollectionExtensions.ConfigureSettings(_services, _configuration, processorKey);
        
        _processors.Add(new ProcessorRegistration(
            ProcessorKey: processorKey,
            Settings: settings,
            Type: ProcessorType.Producer,
            ConfigureConsumerAction: null
        ));
        
        return this;
    }
    
    
    public KafkaGenericProcessorBuilder AddConsumerProducerProcessor<TInput, TOutput>(
        string processorKey)
        where TInput : class
        where TOutput : class
    {
        if (string.IsNullOrEmpty(processorKey))
        {
            throw new ArgumentException("Processor key cannot be null or empty", nameof(processorKey));
        }

        var settings = ServiceCollectionExtensions.ConfigureSettings(_services, _configuration, processorKey);

        _processors.Add(new ProcessorRegistration(
            ProcessorKey: processorKey,
            Settings: settings,
            Type: ProcessorType.ConsumerProducer,
            ConfigureConsumerAction: cluster => ConfigureProcessingConsumer<TInput, TOutput>(cluster, settings, processorKey)
        ));

        return this;
    }

    public KafkaGenericProcessorBuilder AddConsumerProcessor<TInput>(string processorKey)
        where TInput : class
    {
        if (string.IsNullOrEmpty(processorKey))
        {
            throw new ArgumentException("Processor key cannot be null or empty", nameof(processorKey));
        }

        var settings = ServiceCollectionExtensions.ConfigureSettings(_services, _configuration, processorKey);

        _processors.Add(new ProcessorRegistration(
            ProcessorKey: processorKey,
            Settings: settings,
            Type: ProcessorType.Consumer,
            ConfigureConsumerAction: cluster => ConfigureProcessingConsumer<TInput, object>(cluster, settings, processorKey)
        ));

        return this;
    }

    public KafkaGenericProcessorBuilder DisableHealthCheck()
    {
        _healthCheckEnabled = false;
        return this;
    }

    public IServiceCollection Build()
    {
        if (_processors.Count == 0)
            return _services;

        if (_healthCheckEnabled)
        {
            _services.AddKafkaFlowHealthChecks();
        }
        _services.AddSingleton<KeyedServiceResolver>();

        _services.AddKafka(kafka =>
        {
            kafka.UseConsoleLog();
            
            kafka.AddCluster(cluster => 
            {
                var allBrokers = GetAllUniqueBrokers();
                var clusterBuilder = cluster.WithBrokers(allBrokers);

                ConfigureTopics(clusterBuilder);
                ConfigureProcessors(clusterBuilder);
                
                if (_healthCheckEnabled)
                {
                    ConfigureProducer(clusterBuilder, KafkaConstants.HealthCheck.ProducerTopic, KafkaConstants.HealthCheck.ProducerName);
                }
            });
        });
        
        return _services;
    }

    private string[] GetAllUniqueBrokers()
    {
        return _processors
            .SelectMany(p => p.Settings.Brokers)
            .Distinct()
            .ToArray();
    }

    private void ConfigureTopics(IClusterConfigurationBuilder clusterBuilder)
    {
        var topics = _processors
            .SelectMany(p =>
            {
                return p.Settings.CreateTopicsIfNotExists ? new[] { p.Settings.ConsumerTopic, p.Settings.ProducerTopic } : Array.Empty<string>();
            })
            .Where(t => !string.IsNullOrEmpty(t));

        if (_healthCheckEnabled)
            topics = topics.Append(KafkaConstants.HealthCheck.ProducerTopic);

        _logger.LogInformation("Found topics to create: {@Topics}", topics.Distinct());

        foreach (var topicName in topics.Distinct())
        {
            Console.WriteLine($"Creating topic: {topicName}");
            clusterBuilder.CreateTopicIfNotExists(topicName);
        }
    }

    private void ConfigureProcessors(IClusterConfigurationBuilder clusterBuilder)
    {
        foreach (var processor in _processors)
        {
            switch (processor.Type)
            {
                case ProcessorType.Producer:
                    _logger.LogInformation("Configuring producer: {ProducerName} for topic: {Topic}",
                        processor.Settings.ProducerName, processor.Settings.ProducerTopic);
                    ConfigureProducer(clusterBuilder, processor.Settings.ProducerTopic, processor.Settings.ProducerName);
                    break;
                    
                case ProcessorType.ConsumerProducer:
                    processor.ConfigureConsumerAction!(clusterBuilder);
                    _logger.LogInformation("Configuring producer: {ProducerName} for topic: {Topic}",
                        processor.Settings.ProducerName, processor.Settings.ProducerTopic);
                    ConfigureProducer(clusterBuilder, processor.Settings.ProducerTopic, processor.Settings.ProducerName);
                    break;
                    
                case ProcessorType.Consumer:
                    processor.ConfigureConsumerAction!(clusterBuilder);
                    break;
            }
        }
    }
    
    private static void ConfigureProducer(
        IClusterConfigurationBuilder clusterBuilder,
        string producerTopic,
        string producerName)
    {
        clusterBuilder.AddProducer(producerName, producer =>
        {
            producer
                .DefaultTopic(producerTopic)
                .AddMiddlewares(middlewares => middlewares.AddSerializer<JsonCoreSerializer>());
        });
    }
    
    private static void ConfigureProcessingConsumer<TInput, TOutput>(
        IClusterConfigurationBuilder clusterBuilder,
        KafkaProcessorSettings settings,
        string processorKey)
        where TInput : class
        where TOutput : class
    {
        clusterBuilder.AddConsumer(consumer =>
        {
            consumer
                .Topic(settings.ConsumerTopic)
                .WithName($"consumer_{processorKey}")
                .WithGroupId($"{settings.GroupId}_{processorKey}")
                .WithAutoOffsetReset(AutoOffsetReset.Earliest)
                .WithoutStoringOffsets()
                .WithBufferSize(settings.BufferSize)
                .WithWorkersCount(settings.WorkersCount)
                .WithAutoCommitIntervalMs((int)settings.AutoCommitInterval.TotalMilliseconds)
                .AddMiddlewares(middlewares =>
                {
                    middlewares.AddSingleTypeDeserializer<TInput, JsonCoreDeserializer>();
                    
                    middlewares.Add<IMessageMiddleware>(resolver => 
                    {

                        var serviceResolver = resolver.Resolve<KeyedServiceResolver>();
                        
                        var validator = serviceResolver.GetMessageValidator<TInput>(processorKey) ?? 
                                resolver.Resolve<DefaultMessageValidator<TInput>>();

                        return new GenericProcessingMiddleware<TInput, TOutput>(
                            serviceResolver.GetProcessor<TInput, TOutput>(processorKey),
                            serviceResolver.GetConsumerOnlyProcessor<TInput>(processorKey),
                            validator,
                            resolver.Resolve<IProducerAccessor>(),
                            settings,
                            resolver.Resolve<ILogger<GenericProcessingMiddleware<TInput, TOutput>>>()
                        );
                    });
                });
        });
    }

    private enum ProcessorType
    {
        Producer,
        ConsumerProducer,
        Consumer
    }

    private record ProcessorRegistration(
        string ProcessorKey,
        KafkaProcessorSettings Settings,
        ProcessorType Type,
        Action<IClusterConfigurationBuilder>? ConfigureConsumerAction);
}

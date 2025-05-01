using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaFlow.Configuration;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace KafkaGenericProcessor.Core.Services;

/// <summary>
/// Implementation of IKafkaProcessorBuilder for configuring and building a Kafka processor
/// </summary>
/// <typeparam name="TInput">The input message type</typeparam>
/// <typeparam name="TOutput">The output message type</typeparam>
public class KafkaProcessorBuilder<TInput, TOutput> : IKafkaProcessorBuilder
{
    private readonly IServiceCollection _services;
    private readonly KafkaProcessorSettings _settings;

    /// <summary>
    /// Creates a new instance of KafkaProcessorBuilder
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="options">Options containing Kafka processor settings</param>
    public KafkaProcessorBuilder(IServiceCollection services, IOptions<KafkaProcessorSettings> options)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Sets the brokers for the Kafka processor
    /// </summary>
    public IKafkaProcessorBuilder WithBrokers(string[] brokers)
    {
        _settings.Brokers = brokers;
        return this;
    }

    /// <summary>
    /// Sets the buffer size for the Kafka consumer
    /// </summary>
    public IKafkaProcessorBuilder WithBufferSize(int size)
    {
        _settings.BufferSize = size;
        return this;
    }

    /// <summary>
    /// Sets the consumer topic for the Kafka processor
    /// </summary>
    public IKafkaProcessorBuilder WithConsumerTopic(string topic)
    {
        _settings.ConsumerTopic = topic;
        return this;
    }

    /// <summary>
    /// Sets the consumer group ID for the Kafka processor
    /// </summary>
    public IKafkaProcessorBuilder WithGroupId(string groupId)
    {
        _settings.GroupId = groupId;
        return this;
    }

    /// <summary>
    /// Sets the producer topic for the Kafka processor
    /// </summary>
    public IKafkaProcessorBuilder WithProducerTopic(string topic)
    {
        _settings.ProducerTopic = topic;
        return this;
    }

    /// <summary>
    /// Sets the health check topic for the Kafka processor
    /// </summary>
    public IKafkaProcessorBuilder WithHealthCheckTopic(string topic)
    {
        _settings.HealthCheckTopic = topic;
        return this;
    }

    /// <summary>
    /// Sets the number of worker threads for the Kafka consumer
    /// </summary>
    public IKafkaProcessorBuilder WithWorkers(int count)
    {
        _settings.WorkersCount = count;
        return this;
    }

    /// <summary>
    /// Builds the Kafka processor with the configured settings
    /// </summary>
    public void Build()
    {
        ValidateSettings();

        // Configure KafkaFlow
        _services.AddKafka(kafka =>
        {
            kafka.UseConsoleLog();
            kafka.AddCluster(cluster =>
            {
                var clusterBuilder = cluster.WithBrokers(_settings.Brokers);

                // Create topics if needed
                clusterBuilder
                    .CreateTopicIfNotExists(_settings.ConsumerTopic)
                    .CreateTopicIfNotExists(_settings.ProducerTopic)
                    .CreateTopicIfNotExists(_settings.HealthCheckTopic); // Also create health check topic

                // Add producer
                clusterBuilder.AddProducer(_settings.ProducerName, producer =>
                {
                    producer
                        .DefaultTopic(_settings.ProducerTopic)
                        .AddMiddlewares(middlewares =>
                        {
                            middlewares.AddSerializer<JsonCoreSerializer>();
                        });
                });

                // Add consumer
                clusterBuilder.AddConsumer(consumer =>
                {
                    consumer
                        .Topic(_settings.ConsumerTopic)
                        .WithGroupId(_settings.GroupId)
                        .WithBufferSize(_settings.BufferSize)
                        .WithWorkersCount(_settings.WorkersCount)
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
    }

    /// <summary>
    /// Validates the Kafka processor settings
    /// </summary>
    private void ValidateSettings()
    {
        if (_settings.Brokers == null || _settings.Brokers.Length == 0)
        {
            throw new InvalidOperationException("Kafka broker addresses must be specified");
        }

        if (string.IsNullOrWhiteSpace(_settings.ConsumerTopic))
        {
            throw new InvalidOperationException("Consumer topic must be specified");
        }

        if (string.IsNullOrWhiteSpace(_settings.ProducerTopic))
        {
            throw new InvalidOperationException("Producer topic must be specified");
        }

        if (string.IsNullOrWhiteSpace(_settings.GroupId))
        {
            throw new InvalidOperationException("Consumer group ID must be specified");
        }
        
        if (string.IsNullOrWhiteSpace(_settings.HealthCheckTopic))
        {
            _settings.HealthCheckTopic = "kafka-health-check";
        }
    }
}
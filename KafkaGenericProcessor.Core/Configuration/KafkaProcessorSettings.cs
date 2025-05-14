using System;

namespace KafkaGenericProcessor.Core.Configuration;

/// <summary>
/// Configuration settings for Kafka processors
/// </summary>
public class KafkaProcessorSettings
{
    /// <summary>
    /// Gets or sets the unique key for this processor
    /// </summary>
    public string ProcessorKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Kafka broker addresses
    /// </summary>
    public string[] Brokers { get; set; } = [];
    
    /// <summary>
    /// Gets or sets the output topic for producers
    /// </summary>
    public string ProducerTopic { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the input topic for consumers
    /// </summary>
    public string ConsumerTopic { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets the unique name for the producer based on topic and processor key
    /// </summary>
    public string ProducerName => $"producer-{ProducerTopic}-{ProcessorKey}";
    
    /// <summary>
    /// Gets whether this processor is a consumer-only without producing messages
    /// </summary>
    public bool IsConsumerOnly => string.IsNullOrEmpty(ProducerTopic);
    
    /// <summary>
    /// Gets whether this processor is a producer-only without consuming messages
    /// </summary>
    public bool IsProducerOnly => string.IsNullOrEmpty(ConsumerTopic);
    
    /// <summary>
    /// Gets or sets the consumer group ID
    /// </summary>
    public string GroupId { get; set; } = "kafka-generic-processor-group";
    
    /// <summary>
    /// Gets or sets the number of parallel workers
    /// </summary>
    public int WorkersCount { get; set; } = 20;
    
    /// <summary>
    /// Gets or sets the buffer size for message processing
    /// </summary>
    public int BufferSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the auto-commit interval
    /// </summary>
    public TimeSpan AutoCommitInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    
    /// <summary>
    /// Gets or sets whether to create topics if they don't exist
    /// </summary>
    public bool CreateTopicsIfNotExists { get; set; }
    
}
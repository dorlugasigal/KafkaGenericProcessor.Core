namespace KafkaGenericProcessor.Core.Configuration;

/// <summary>
/// Settings for configuring the Kafka processor
/// </summary>
public class KafkaProcessorSettings
{
    /// <summary>
    /// Array of Kafka broker addresses
    /// </summary>
    public string[] Brokers { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// The topic to consume messages from
    /// </summary>
    public string ConsumerTopic { get; set; } = string.Empty;
    
    /// <summary>
    /// The topic to produce messages to
    /// </summary>
    public string ProducerTopic { get; set; } = string.Empty;
    
    /// <summary>
    /// The consumer group ID
    /// </summary>
    public string GroupId { get; set; } = "kafka-generic-processor-group";
    
    /// <summary>
    /// Number of worker threads (default: 1)
    /// </summary>
    public int WorkersCount { get; set; } = 20;
    
    /// <summary>
    /// Buffer size for the consumer (default: 100)
    /// </summary>
    public int BufferSize { get; set; } = 100;
    
    /// <summary>
    /// Name of the producer (default: "producer")
    /// </summary>
    public string ProducerName { get; set; } = "producer";
    
    /// <summary>
    /// Whether to create topics if they don't exist (default: true)
    /// </summary>
    public bool CreateTopicsIfNotExist { get; set; } = true;
    
    /// <summary>
    /// Auto commit interval for the consumer
    /// </summary>
    public TimeSpan AutoCommitInterval { get; set; } = TimeSpan.FromMilliseconds(500);
}
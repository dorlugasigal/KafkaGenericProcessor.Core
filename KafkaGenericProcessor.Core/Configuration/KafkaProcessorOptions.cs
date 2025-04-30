namespace KafkaGenericProcessor.Core.Configuration;

/// <summary>
/// Options for configuring the Kafka processor during setup
/// </summary>
public class KafkaProcessorOptions
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
    /// Number of worker threads
    /// </summary>
    public int WorkersCount { get; set; } = 1;
    
    /// <summary>
    /// Buffer size for the consumer
    /// </summary>
    public int BufferSize { get; set; } = 100;
    
    /// <summary>
    /// Name of the producer
    /// </summary>
    public string ProducerName { get; set; } = "generic-producer";
    
    /// <summary>
    /// Whether to create topics if they don't exist
    /// </summary>
    public bool CreateTopicsIfNotExist { get; set; } = true;
}
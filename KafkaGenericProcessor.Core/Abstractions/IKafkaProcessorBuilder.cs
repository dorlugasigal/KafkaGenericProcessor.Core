namespace KafkaGenericProcessor.Core.Abstractions;

/// <summary>
/// Builder for configuring and creating a Kafka processor
/// </summary>
public interface IKafkaProcessorBuilder
{
    /// <summary>
    /// Sets the consumer topic
    /// </summary>
    /// <param name="topic">The topic to consume messages from</param>
    /// <returns>The builder instance for method chaining</returns>
    IKafkaProcessorBuilder WithConsumerTopic(string topic);
    
    /// <summary>
    /// Sets the producer topic
    /// </summary>
    /// <param name="topic">The topic to produce messages to</param>
    /// <returns>The builder instance for method chaining</returns>
    IKafkaProcessorBuilder WithProducerTopic(string topic);
    
    /// <summary>
    /// Sets the consumer group ID
    /// </summary>
    /// <param name="groupId">The consumer group ID</param>
    /// <returns>The builder instance for method chaining</returns>
    IKafkaProcessorBuilder WithGroupId(string groupId);
    
    /// <summary>
    /// Sets the Kafka broker addresses
    /// </summary>
    /// <param name="brokers">Array of broker addresses</param>
    /// <returns>The builder instance for method chaining</returns>
    IKafkaProcessorBuilder WithBrokers(string[] brokers);
    
    /// <summary>
    /// Sets the number of worker threads
    /// </summary>
    /// <param name="count">Number of worker threads</param>
    /// <returns>The builder instance for method chaining</returns>
    IKafkaProcessorBuilder WithWorkers(int count);
    
    /// <summary>
    /// Sets the buffer size for the consumer
    /// </summary>
    /// <param name="size">Buffer size</param>
    /// <returns>The builder instance for method chaining</returns>
    IKafkaProcessorBuilder WithBufferSize(int size);
    
    /// <summary>
    /// Builds the Kafka processor with the configured settings
    /// </summary>
    void Build();
}
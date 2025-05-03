using System;

namespace KafkaGenericProcessor.Core.Configuration;

/// <summary>
/// Settings for Kafka health checks
/// </summary>
public record KafkaHealthCheckSettings
{
    /// <summary>
    /// Array of Kafka broker addresses
    /// </summary>
    public string[] Brokers { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// The topic to use for health checks
    /// </summary>
    public string ProducerTopic { get; set; } = "kafka-health-check";
    
    /// <summary>
    /// Name of the producer for health checks
    /// </summary>
    public string ProducerName { get; set; } = "health_producer";
}
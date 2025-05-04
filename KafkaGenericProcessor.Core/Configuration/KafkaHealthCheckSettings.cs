using System;

namespace KafkaGenericProcessor.Core.Configuration;

public record KafkaHealthCheckSettings
{
    public string[] Brokers { get; set; } = [];

    public string ProducerTopic { get; set; } = "kafka-health-check";
    
    public string ProducerName { get; set; } = "health_producer";
}
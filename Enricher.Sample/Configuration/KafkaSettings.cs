namespace Enricher.Sample.Configuration;

public class KafkaSettings
{
    public required string[] Brokers { get; set; }
    public required string ConsumerTopic { get; set; }
    public required string ProducerTopic { get; set; }
}
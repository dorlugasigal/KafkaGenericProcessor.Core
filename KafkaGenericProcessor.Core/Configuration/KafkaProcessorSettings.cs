namespace KafkaGenericProcessor.Core.Configuration;

public class KafkaProcessorSettings
{
    public string[] Brokers { get; set; } = [];
    public string ConsumerTopic { get; set; } = string.Empty;
    
    public string ProducerTopic { get; set; } = string.Empty;
    
    public string GroupId { get; set; } = "kafka-generic-processor-group";
    
    public int WorkersCount { get; set; } = 20;
    
    public int BufferSize { get; set; } = 100;

    public TimeSpan AutoCommitInterval { get; set; } = TimeSpan.FromMilliseconds(500);
}
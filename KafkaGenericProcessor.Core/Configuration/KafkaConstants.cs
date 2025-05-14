namespace KafkaGenericProcessor.Core.Configuration;

internal static class KafkaConstants
{
    public static class HealthCheck
    {
        public const string ProducerTopic = "kafka-health-check";

        public const string ProducerName = "health-check-producer";
    }
}
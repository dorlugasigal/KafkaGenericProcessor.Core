using Enricher.Sample.Configuration;
using Microsoft.Extensions.Options;

namespace Enricher.Sample.Services;

public static class ConfigurationValidator
{
    public static void ValidateKafkaSettings(KafkaSettings settings)
    {
        var errors = new List<string>();

        if (settings.Brokers == null || settings.Brokers.Length == 0)
        {
            errors.Add("Kafka brokers are not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ConsumerTopic))
        {
            errors.Add("Kafka consumer topic is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ProducerTopic))
        {
            errors.Add("Kafka producer topic is not configured.");
        }

        if (errors.Any())
        {
            throw new ApplicationException($"Invalid Kafka configuration: {string.Join(", ", errors)}");
        }
    }
}
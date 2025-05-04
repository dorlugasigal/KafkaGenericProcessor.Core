using System;
using System.Collections.Generic;
using System.Linq;

namespace KafkaGenericProcessor.Core.Configuration;

internal static class KafkaConfigurationValidator
{
    internal static void ValidateKafkaSettings(KafkaProcessorSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings), "Kafka settings cannot be null");
        }

        var errors = new List<string>();

        if (settings.Brokers.Length == 0)
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

        if (string.IsNullOrWhiteSpace(settings.GroupId))
        {
            errors.Add("Consumer group ID is not configured.");
        }

        if (errors.Any())
        {
            throw new ApplicationException($"Invalid Kafka configuration: {string.Join(", ", errors)}");
        }
        
    }

    internal static void ValidateKafkaHealthCheckSettings(KafkaHealthCheckSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings), "Kafka health check settings cannot be null");
        }

        var errors = new List<string>();

        if (settings.Brokers.Length == 0)
        {
            errors.Add("Kafka health check brokers are not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ProducerTopic))
        {
            errors.Add("Kafka health check topic is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.ProducerName))
        {
            errors.Add("Health check producer name is not configured.");
        }

        if (errors.Any())
        {
            throw new ApplicationException($"Invalid Kafka health check configuration: {string.Join(", ", errors)}");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace KafkaGenericProcessor.Core.Configuration;

/// <summary>
/// Validates Kafka configuration settings
/// </summary>
public static class KafkaConfigurationValidator
{
    /// <summary>
    /// Validates the Kafka processor settings and throws exceptions for any invalid settings
    /// </summary>
    /// <param name="settings">The settings to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null</exception>
    /// <exception cref="ApplicationException">Thrown if any settings are invalid</exception>
    public static void ValidateKafkaSettings(KafkaProcessorSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings), "Kafka settings cannot be null");
        }

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

        if (string.IsNullOrWhiteSpace(settings.GroupId))
        {
            errors.Add("Consumer group ID is not configured.");
        }

        if (errors.Any())
        {
            throw new ApplicationException($"Invalid Kafka configuration: {string.Join(", ", errors)}");
        }
        
        // Apply defaults for optional settings if not set
        if (string.IsNullOrWhiteSpace(settings.HealthCheckTopic))
        {
            settings.HealthCheckTopic = "kafka-health-check";
        }
        
        if (string.IsNullOrWhiteSpace(settings.ProducerName))
        {
            settings.ProducerName = "producer";
        }
    }
}
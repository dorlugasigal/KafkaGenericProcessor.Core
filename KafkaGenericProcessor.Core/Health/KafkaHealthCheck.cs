using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Health;

/// <summary>
/// A health check for Kafka that verifies connectivity by sending a test message.
/// </summary>
public class KafkaHealthCheck : IHealthCheck
{
    private readonly IProducerAccessor _producerAccessor;
    private readonly ILogger<KafkaHealthCheck> _logger;
    private readonly string _producerName;
    private readonly string _healthCheckTopic;
    private DateTime _lastSuccessfulCheck = DateTime.MinValue;

    /// <summary>
    /// Creates a new instance of KafkaHealthCheck
    /// </summary>
    /// <param name="producerAccessor">The producer accessor</param>
    /// <param name="logger">The logger</param>
    /// <param name="producerName">The name of the producer to use for health checks</param>
    /// <param name="healthCheckTopic">The topic to send health check messages to</param>
    public KafkaHealthCheck(
        IProducerAccessor producerAccessor,
        ILogger<KafkaHealthCheck> logger,
        string producerName = "producer",
        string healthCheckTopic = "kafka-health-check")
    {
        _producerAccessor = producerAccessor ?? throw new ArgumentNullException(nameof(producerAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _producerName = producerName ?? throw new ArgumentNullException(nameof(producerName));
        _healthCheckTopic = healthCheckTopic ?? throw new ArgumentNullException(nameof(healthCheckTopic));
    }

    /// <summary>
    /// Performs a health check by sending a test message to Kafka
    /// </summary>
    /// <param name="context">The health check context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The health check result</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var producer = _producerAccessor.GetProducer(_producerName);
            
            if (producer == null)
            {
                var message = $"Producer '{_producerName}' not found";
                _logger.LogWarning(message);
                return new HealthCheckResult(context.Registration.FailureStatus, message);
            }

            // Create a health check message with timestamp
            var healthMessage = new HealthCheckMessage
            {
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            };

            // Send the health check message
            await producer.ProduceAsync(_healthCheckTopic, healthMessage.Id, healthMessage);
            
            _lastSuccessfulCheck = DateTime.UtcNow;
            
            return HealthCheckResult.Healthy("Kafka connection is healthy", 
                new Dictionary<string, object>
                {
                    { "LastSuccessfulCheck", _lastSuccessfulCheck },
                    { "HealthCheckTopic", _healthCheckTopic },
                    { "MessageId", healthMessage.Id }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Kafka health check");
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                "Failed to connect to Kafka",
                ex,
                new Dictionary<string, object>
                {
                    { "LastSuccessfulCheck", _lastSuccessfulCheck },
                    { "Exception", ex.Message }
                });
        }
    }

    /// <summary>
    /// Class representing a health check message sent to Kafka
    /// </summary>
    private class HealthCheckMessage
    {
        public string Id { get; set; } = default!;
        public DateTime Timestamp { get; set; }
    }
}
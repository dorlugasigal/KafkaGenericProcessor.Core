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
public class KafkaHealthCheck(
    IProducerAccessor producerAccessor,
    ILogger<KafkaHealthCheck> logger,
    string producerName,
    string healthCheckTopic) : IHealthCheck
{
    private DateTime _lastSuccessfulCheck = DateTime.MinValue;

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
            var producer = producerAccessor.GetProducer(producerName);
            
            if (producer == null)
            {
                var message = $"Producer '{producerName}' not found";
                logger.LogWarning(message);
                return new HealthCheckResult(context.Registration.FailureStatus, message);
            }

            // Create a health check message with timestamp
            var healthMessage = new HealthCheckMessage
            {
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            };

            // Send the health check message
            await producer.ProduceAsync(healthCheckTopic, healthMessage.Id, healthMessage);
            
            _lastSuccessfulCheck = DateTime.UtcNow;
            
            return HealthCheckResult.Healthy("Kafka connection is healthy", 
                new Dictionary<string, object>
                {
                    { "LastSuccessfulCheck", _lastSuccessfulCheck },
                    { "HealthCheckTopic", healthCheckTopic },
                    { "MessageId", healthMessage.Id }
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Kafka health check");
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
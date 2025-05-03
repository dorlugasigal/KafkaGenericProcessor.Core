using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Health;

/// <summary>
/// A health check for Kafka that verifies connectivity by sending a test message.
/// </summary>
public class KafkaHealthCheck(
    IProducerAccessor producerAccessor,
    ILogger<KafkaHealthCheck> logger,
    KafkaHealthCheckSettings settings)
    : IHealthCheck
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
            var producer = producerAccessor.GetProducer(settings.ProducerName);
            
            if (producer == null)
            {
                var message = $"Producer '{settings.ProducerName}' not found";
                logger.LogWarning(message);
                return new HealthCheckResult(context.Registration.FailureStatus, message);
            }

            var healthMessage = new
            {
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            };

            await producer.ProduceAsync(settings.ProducerTopic, healthMessage.Id, healthMessage);
            
            _lastSuccessfulCheck = DateTime.UtcNow;
            
            return HealthCheckResult.Healthy("Kafka connection is healthy", 
                new Dictionary<string, object>
                {
                    { "LastSuccessfulCheck", _lastSuccessfulCheck },
                    { "HealthCheckTopic", settings.ProducerTopic },
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
}
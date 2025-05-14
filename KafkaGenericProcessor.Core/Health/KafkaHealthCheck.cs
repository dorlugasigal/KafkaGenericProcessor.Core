using KafkaFlow.Producers;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using KafkaGenericProcessor.Core.Configuration;

namespace KafkaGenericProcessor.Core.Health;

internal class KafkaHealthCheck(
    IProducerAccessor producerAccessor,
    ILogger<KafkaHealthCheck> logger)
    : IHealthCheck
{
    private DateTime _lastSuccessfulCheck = DateTime.MinValue;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var producer = producerAccessor.GetProducer(KafkaConstants.HealthCheck.ProducerName);
            
            if (producer == null)
            {
                var message = $"Producer '{KafkaConstants.HealthCheck.ProducerName}' not found";
                logger.LogWarning(message);
                return new HealthCheckResult(context.Registration.FailureStatus, message);
            }

            var healthMessage = new
            {
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            };

            await producer.ProduceAsync(KafkaConstants.HealthCheck.ProducerTopic, healthMessage.Id, healthMessage);
            
            _lastSuccessfulCheck = DateTime.UtcNow;
            
            return HealthCheckResult.Healthy("Kafka connection is healthy", 
                new Dictionary<string, object>
                {
                    { "LastSuccessfulCheck", _lastSuccessfulCheck },
                    { "HealthCheckTopic", KafkaConstants.HealthCheck.ProducerTopic },
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
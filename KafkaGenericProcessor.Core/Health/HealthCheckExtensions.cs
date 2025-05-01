using System;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Health;

/// <summary>
/// Extension methods for health check configuration
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds comprehensive health checks for a KafkaFlow application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="kafkaCheckName">The name of the Kafka health check</param>
    /// <param name="producerName">The name of the producer to check</param>
    /// <param name="healthCheckTopic">The topic to send health check messages to</param>
    /// <returns>The health checks builder for chaining</returns>
    public static IHealthChecksBuilder AddKafkaFlowHealthChecks(
        this IServiceCollection services,
        string kafkaCheckName = "kafka",
        string producerName = "producer",
        string healthCheckTopic = "kafka-health-check")
    {
        return services
            .AddHealthChecks()
            // Liveness check - application is running
            .AddCheck(
                "liveness", 
                () => HealthCheckResult.Healthy("Application is running"), 
                tags: [  "live" ])
            // Readiness check - Kafka is ready
            .Add(new HealthCheckRegistration(
                name: kafkaCheckName,
                factory: sp => new KafkaHealthCheck(
                    sp.GetRequiredService<IProducerAccessor>() ?? throw new ArgumentNullException(nameof(IProducerAccessor)),
                    sp.GetRequiredService<ILogger<KafkaHealthCheck>>() ?? throw new ArgumentNullException(nameof(ILogger<KafkaHealthCheck>)),
                    producerName,
                    healthCheckTopic),
                failureStatus: HealthStatus.Unhealthy,
                tags: [ "ready", "kafka"] ,
                timeout: TimeSpan.FromSeconds(2)));
    }
}
using System;
using System.Collections.Generic;
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
    /// Adds a Kafka health check to the service collection
    /// </summary>
    /// <param name="builder">The health checks builder</param>
    /// <param name="name">The name of the health check</param>
    /// <param name="producerName">The name of the producer to check</param>
    /// <param name="healthCheckTopic">The topic to send health check messages to</param>
    /// <param name="timeout">The timeout for health check operations</param>
    /// <param name="failureStatus">The status to report when the health check fails</param>
    /// <param name="tags">A list of tags that can be used to filter health checks</param>
    /// <returns>The health checks builder for chaining</returns>
    public static IHealthChecksBuilder AddKafkaHealthCheck(
        this IHealthChecksBuilder builder,
        string name = "kafka",
        string producerName = "producer",
        string healthCheckTopic = "kafka-health-check",
        TimeSpan? timeout = null,
        HealthStatus failureStatus = HealthStatus.Unhealthy,
        IEnumerable<string>? tags = null)
    {
        return builder.Add(new HealthCheckRegistration(
            name,
            serviceProvider => new KafkaHealthCheck(
                serviceProvider.GetRequiredService<IProducerAccessor>(),
                serviceProvider.GetRequiredService<ILogger<KafkaHealthCheck>>(),
                producerName,
                healthCheckTopic,
                timeout),
            failureStatus,
            tags ?? new[] { "ready", "kafka" },
            TimeSpan.FromSeconds(2)));
    }

    /// <summary>
    /// Adds comprehensive health checks for a KafkaFlow application
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="kafkaHealthCheckName">The name of the Kafka health check</param>
    /// <param name="producerName">The name of the producer to check</param>
    /// <param name="healthCheckTopic">The topic to send health check messages to</param>
    /// <param name="timeout">The timeout for health check operations</param>
    /// <returns>The health checks builder for chaining</returns>
    public static IHealthChecksBuilder AddKafkaFlowHealthChecks(
        this IServiceCollection services,
        string kafkaHealthCheckName = "kafka",
        string producerName = "producer",
        string healthCheckTopic = "kafka-health-check",
        TimeSpan? timeout = null)
    {
        return services
            .AddHealthChecks()
            // Liveness check - application is running
            .AddCheck("liveness", () => HealthCheckResult.Healthy("Application is running"), 
                tags: new[] { "live" })
            // Readiness check - Kafka is ready
            .AddKafkaHealthCheck(
                kafkaHealthCheckName,
                producerName,
                healthCheckTopic,
                timeout,
                HealthStatus.Unhealthy,
                new[] { "ready", "kafka" });
    }
}
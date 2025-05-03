using System;
using KafkaFlow;
using KafkaFlow.Producers;
using KafkaGenericProcessor.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
    /// <returns>The health checks builder for chaining</returns>
    public static IHealthChecksBuilder AddKafkaFlowHealthChecks(this IServiceCollection services)
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
                name: "kafka",
                factory: sp => new KafkaHealthCheck(
                    sp.GetRequiredService<IProducerAccessor>() ?? throw new ArgumentNullException(nameof(IProducerAccessor)),
                    sp.GetRequiredService<ILogger<KafkaHealthCheck>>() ?? throw new ArgumentNullException(nameof(ILogger<KafkaHealthCheck>)),
                    sp.GetRequiredService<IOptions<KafkaHealthCheckSettings>>().Value),
                failureStatus: HealthStatus.Unhealthy,
                tags: [ "ready", "kafka" ],
                timeout: TimeSpan.FromSeconds(2)));
    }
}
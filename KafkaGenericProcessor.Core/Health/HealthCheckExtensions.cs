using KafkaFlow.Producers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Health;

internal static class HealthCheckExtensions
{
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
                    sp.GetRequiredService<ILogger<KafkaHealthCheck>>() ?? throw new ArgumentNullException(nameof(ILogger<KafkaHealthCheck>))),
                failureStatus: HealthStatus.Unhealthy,
                tags: [ "ready", "kafka" ],
                timeout: TimeSpan.FromSeconds(2)));
    }
}
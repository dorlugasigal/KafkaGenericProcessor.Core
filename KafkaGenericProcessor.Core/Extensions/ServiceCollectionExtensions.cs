using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Health;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaGenericProcessor.Core.Extensions;

/// <summary>
/// Extensions for service collection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add a Kafka Generic Processor builder to configure multiple processors with a fluent API
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>A builder instance to configure processors</returns>
    public static KafkaGenericProcessorBuilder AddKafkaGenericProcessors(this IServiceCollection services)
    {
        return new KafkaGenericProcessorBuilder(services);
    }
    
    /// <summary>
    /// Add a Kafka Generic Processor with a specific input and output type using configuration from appsettings.json
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The IConfiguration instance</param>
    /// <param name="processorKey">The key for keyed services, also used as suffix for section name in configuration</param>
    /// <typeparam name="TInput">The input message type</typeparam>
    /// <typeparam name="TOutput">The output message type</typeparam>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddKafkaGenericProcessor<TInput, TOutput>(
        this IServiceCollection services,
        IConfiguration configuration,
        string processorKey)
        where TInput : class
        where TOutput : class
    {
        // This maintains backward compatibility by using the builder pattern internally
        return services.AddKafkaGenericProcessors()
            .AddProcessor<TInput, TOutput>(configuration, processorKey)
            .Build();
    }

    /// <summary>
    /// Configures Kafka processor settings from configuration
    /// </summary>
    internal static KafkaProcessorSettings ConfigureSettings(
        IServiceCollection services,
        IConfiguration configuration,
        string processorKey)
    {
        // Updated to use the new Kafka.Configurations path
        var settingsSection = configuration.GetSection($"Kafka:Configurations:{processorKey}");
        var settings = new KafkaProcessorSettings();
        settingsSection.Bind(settings);
        
        KafkaConfigurationValidator.ValidateKafkaSettings(settings);
        
        // Register settings with the processor key
        services.Configure<KafkaProcessorSettings>(processorKey, settingsSection.Bind);
        
        return settings;
    }
}
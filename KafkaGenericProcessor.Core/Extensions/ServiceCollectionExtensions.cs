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
    public static KafkaGenericProcessorBuilder AddKafkaGenericProcessors(this IServiceCollection services, IConfiguration configuration)
    {
        return new KafkaGenericProcessorBuilder(services, configuration);
    }
    
    /// <summary>
    /// Configures Kafka processor settings from configuration
    /// </summary>
    internal static KafkaProcessorSettings ConfigureSettings(
        IServiceCollection services,
        IConfiguration configuration,
        string processorKey)
    {
        var settingsSection = configuration.GetSection($"Kafka:Configurations:{processorKey}");
        var settings = new KafkaProcessorSettings();
        settingsSection.Bind(settings);
        
        KafkaConfigurationValidator.ValidateKafkaSettings(settings);
        
        // Register settings with the processor key
        services.Configure<KafkaProcessorSettings>(processorKey, settingsSection.Bind);
        
        return settings;
    }
}
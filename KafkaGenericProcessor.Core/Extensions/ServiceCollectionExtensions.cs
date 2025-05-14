using KafkaGenericProcessor.Core.Configuration;
using KafkaGenericProcessor.Core.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KafkaGenericProcessor.Core.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to configure Kafka Generic Processor services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Kafka generic processor services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>A builder for configuring Kafka generic processors</returns>
    public static KafkaGenericProcessorBuilder AddKafkaGenericProcessors(this IServiceCollection services, IConfiguration configuration)
    {
        // Register default validator
        services.AddTransient(typeof(DefaultMessageValidator<>));
        
        return new KafkaGenericProcessorBuilder(services, configuration);
    }
    
    /// <summary>
    /// Configures processor settings from configuration
    /// </summary>
    internal static KafkaProcessorSettings ConfigureSettings(
        IServiceCollection services,
        IConfiguration configuration,
        string processorKey)
    {
        var settingsSection = configuration.GetSection($"Kafka:Configurations:{processorKey}");
        var settings = new KafkaProcessorSettings { ProcessorKey = processorKey };
        settingsSection.Bind(settings);
        
        services.Configure<KafkaProcessorSettings>(processorKey, settingsSection.Bind);
        
        return settings;
    }
}
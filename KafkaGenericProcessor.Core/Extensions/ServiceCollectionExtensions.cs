using KafkaGenericProcessor.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaGenericProcessor.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static KafkaGenericProcessorBuilder AddKafkaGenericProcessors(this IServiceCollection services, IConfiguration configuration)
    {
        return new KafkaGenericProcessorBuilder(services, configuration);
    }
    
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
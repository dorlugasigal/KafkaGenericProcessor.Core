using KafkaFlow;
using KafkaFlow.Producers;
using Enricher.Sample.Models;
using Enricher.Sample.Processors;
using Enricher.Sample.Validators;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Extensions;
using KafkaGenericProcessor.Core.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Net.Mime;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

// Register our custom message processor and validator
builder.Services.AddTransient<IMessageProcessor<MyInput, MyOutput>, MyInputProcessor>();
builder.Services.AddTransient<IMessageValidator<MyInput>, MyInputValidator>();

// Add the Kafka Generic Processor using the centralized configuration from appsettings.json
builder.Services.AddKafkaGenericProcessor<MyInput, MyOutput>(builder.Configuration);
builder.Services.AddKafkaGenericProcessorHealthChecks();

var app = builder.Build();

// Basic status endpoint
app.MapGet("/", () => "KafkaFlow Processor running");

// Configure health check endpoints using HealthChecks.UI.Client
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/test", async (IProducerAccessor producerAccessor, IOptions<KafkaProcessorSettings> options, ILogger<Program> logger) =>
{
    var kafkaSettings = options.Value;
    var testMessage = new MyInput(Guid.NewGuid().ToString(), $"Test message created at {DateTime.Now}");
    return await ProduceMessageAsync(producerAccessor, kafkaSettings, kafkaSettings.ConsumerTopic, testMessage.Id, testMessage, logger);
});

app.MapGet("/test-output", async (IProducerAccessor producerAccessor, IOptions<KafkaProcessorSettings> options, ILogger<Program> logger) =>
{
    var kafkaSettings = options.Value;
    var testOutput = new MyOutput(ProducedBy: $"Direct test from endpoint at {DateTime.Now}", ProcessedAt: DateTime.UtcNow);
    return await ProduceMessageAsync(producerAccessor, kafkaSettings, kafkaSettings.ProducerTopic, Guid.NewGuid().ToString(), testOutput, logger);
});

async Task<IResult> ProduceMessageAsync<T>(
    IProducerAccessor producerAccessor, 
    KafkaProcessorSettings settings, 
    string topic, 
    string messageKey, 
    T message, 
    ILogger? logger = null)
{
    try
    {
        var producer = producerAccessor.GetProducer(settings.ProducerName);
        if (producer == null)
        {
            return Results.Problem($"Producer '{settings.ProducerName}' not found. Check your Kafka configuration.");
        }
        var result = await producer.ProduceAsync(topic, messageKey, message);
        return Results.Ok(new { Result = result, Topic = topic });
    }
    catch (Exception ex)
    {
        logger?.LogError(ex, "Error producing message to topic: {Topic}", topic);
        return Results.Problem($"Error sending message: {ex.Message}");
    }
}

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();

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
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

// Register keyed services for both processors and validators
builder.Services.AddKeyedTransient<IMessageProcessor<MyInput, MyOutput>, MyInputProcessor>("enrich1");
builder.Services.AddKeyedTransient<IMessageValidator<MyInput>, MyInputValidator>("enrich1");

builder.Services.AddKeyedTransient<IMessageProcessor<MyInput, MyOutput>, MyInputProcessor2>("enrich2");
builder.Services.AddKeyedTransient<IMessageValidator<MyInput>, MyInputValidator2>("enrich2");

// Use the builder pattern to register processors and health check in a simple fluent API
builder.Services
    .AddKafkaGenericProcessors(builder.Configuration)
    .AddProcessor<MyInput, MyOutput>("enrich1")
    .AddProcessor<MyInput, MyOutput>("enrich2")
    .AddHealthCheck()
    .Build();

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

// Test endpoints for enrich1 processor
app.MapGet("/test-input-enrich1", async (IProducerAccessor producerAccessor, IOptionsMonitor<KafkaProcessorSettings> optionsMonitor, ILogger<Program> logger) =>
{
    var kafkaSettings = optionsMonitor.Get("enrich1");
    var testMessage = new MyInput("A123", $"This message is handled by enrich1 processor at {DateTime.Now}");
    
    // Use the named producer based on the processor key
    string producerName = $"{kafkaSettings.ProducerName}_enrich1";
    var producer = producerAccessor.GetProducer(producerName);
    
    if (producer == null)
    {
        return Results.Problem($"Producer '{producerName}' not found. Check your Kafka configuration.");
    }
    var result = await producer.ProduceAsync(kafkaSettings.ConsumerTopic, Guid.NewGuid().ToString(), testMessage);
    return Results.Ok(new { Result = result, Topic = kafkaSettings.ConsumerTopic, Producer = producerName });
});

// Test endpoints for enrich2 processor with validation that requires min length and letter ID
app.MapGet("/test-input-enrich2", async (IProducerAccessor producerAccessor, IOptionsMonitor<KafkaProcessorSettings> optionsMonitor, ILogger<Program> logger) =>
{
    var kafkaSettings = optionsMonitor.Get("enrich2");
    var testMessage = new MyInput("B456", $"This message with more than 10 characters is handled by enrich2 processor at {DateTime.Now}");
    
    // Use the named producer based on the processor key
    string producerName = $"{kafkaSettings.ProducerName}_enrich2";
    var producer = producerAccessor.GetProducer(producerName);
    
    if (producer == null)
    {
        return Results.Problem($"Producer '{producerName}' not found. Check your Kafka configuration.");
    }
    var result = await producer.ProduceAsync(kafkaSettings.ConsumerTopic, Guid.NewGuid().ToString(), testMessage);
    return Results.Ok(new { Result = result, Topic = kafkaSettings.ConsumerTopic, Producer = producerName });
});

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();

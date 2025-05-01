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

// Add an endpoint to generate and produce a test message
app.MapGet("/test", async (IProducerAccessor producerAccessor, IOptions<KafkaProcessorSettings> options) =>
{
    try
    {
        var kafkaSettings = options.Value;
        var testMessage = new MyInput(Guid.NewGuid().ToString(),$"Test message created at {DateTime.Now}");
        
        var producer = producerAccessor.GetProducer(kafkaSettings.ProducerName);
        if (producer == null)
        {
            return Results.Problem($"Producer '{kafkaSettings.ProducerName}' not found. Check your Kafka configuration.");
        }
        
        var result = await producer.ProduceAsync(kafkaSettings.ConsumerTopic, testMessage.Id, testMessage);
            
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error sending test message: {ex.Message}");
    }
});

// Add a diagnostic endpoint that tests producing directly to the output topic
app.MapGet("/test-output", async (IProducerAccessor producerAccessor, IOptions<KafkaProcessorSettings> options, ILogger<Program> logger) =>
{
    try
    {
        var kafkaSettings = options.Value;
        
        // Log the producer settings
        logger.LogInformation("Producer topic from settings: {ProducerTopic}", kafkaSettings.ProducerTopic);
        
        // Get the producer
        var producer = producerAccessor.GetProducer(kafkaSettings.ProducerName);
        if (producer == null)
        {         
            return Results.Problem($"Producer '{kafkaSettings.ProducerName}' not found. Check your Kafka configuration.");
        }
        
        // Create a test output message
        var testOutput = new MyOutput(
            ProducedBy: $"Direct test from endpoint at {DateTime.Now}",
            ProcessedAt: DateTime.UtcNow
        );
        
        // Try to produce directly to the output topic
        logger.LogInformation("Attempting to produce directly to output topic: {Topic}", kafkaSettings.ProducerTopic);
        var result = await producer.ProduceAsync(kafkaSettings.ProducerTopic, Guid.NewGuid().ToString(), testOutput);
        
        return Results.Ok(new { 
            Message = "Message sent directly to output topic",
            Result = result,
            OutputTopic = kafkaSettings.ProducerTopic
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error in direct output topic test");
        return Results.Problem($"Error sending test message to output topic: {ex.Message}");
    }
});

// Start the KafkaBus
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();

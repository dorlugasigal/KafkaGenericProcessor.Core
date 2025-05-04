using Enricher.Sample.Models;
using Enricher.Sample.Processors;
using HealthChecks.UI.Client;
using KafkaFlow;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Register keyed services for both processors and validators
builder.Services.AddKeyedTransient<IMessageProcessor<MyInput, MyOutput>, GlobalTrackConverterProcessor>(nameof(GlobalTrackConverterProcessor));

// Use the builder pattern to register processors and health check in a simple fluent API
builder.Services
    .AddKafkaGenericProcessors(builder.Configuration)
    .AddProcessor<MyInput, MyOutput>(nameof(GlobalTrackConverterProcessor))
    .AddHealthCheck()
    .Build();

var app = builder.Build();

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

var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();

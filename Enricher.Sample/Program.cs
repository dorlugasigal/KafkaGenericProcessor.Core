using KafkaFlow;
using KafkaFlow.Producers;
using Enricher.Sample.Configuration;
using Enricher.Sample.Models;
using Enricher.Sample.Services;
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Bind and validate Kafka settings
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));
var kafkaSettings = builder.Configuration.GetSection("Kafka").Get<KafkaSettings>() ?? throw new ApplicationException("Kafka configuration is missing in appsettings.json");

ConfigurationValidator.ValidateKafkaSettings(kafkaSettings);

// Register our custom message processor and validator
builder.Services.AddTransient<IMessageProcessor<MyInput, MyOutput>, MyInputProcessor>();
builder.Services.AddTransient<IMessageValidator<MyInput>, MyInputValidator>();

// Register the generic Kafka processor
builder.Services.AddKafkaGenericProcessor<MyInput, MyOutput>(options => 
{
    options.Brokers = kafkaSettings.Brokers;
    options.ConsumerTopic = kafkaSettings.ConsumerTopic;
    options.ProducerTopic = kafkaSettings.ProducerTopic;
    options.GroupId = "kafkaflow-processor-group"; 
    options.WorkersCount = 1;
    options.BufferSize = 100;
    options.ProducerName = "producer";
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Basic status endpoint
app.MapGet("/", () => "KafkaFlow Processor running");

// Health check endpoint
app.MapHealthChecks("/health");

// Add an endpoint to generate and produce a test message
app.MapGet("/test", async (IProducerAccessor producerAccessor) =>
{
    try
    {
        var testMessage = new MyInput(Guid.NewGuid().ToString(),$"Test message created at {DateTime.Now}");

        var producer = producerAccessor.GetProducer("producer");
        
        var result = await producer.ProduceAsync(kafkaSettings.ConsumerTopic, testMessage.Id, testMessage);
            
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error sending test message: {ex.Message}");
    }
});

// Start the KafkaBus per the official documentation pattern
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();

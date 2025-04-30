using KafkaFlow;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Bind Kafka settings
var kafkaSettings = builder.Configuration.GetSection("Kafka").Get<KafkaSettings>() ?? new KafkaSettings();
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("Kafka"));

// Register KafkaFlow
builder.Services.AddKafka(kafka =>
{
    kafka.UseConsoleLog();
    kafka.AddCluster(cluster =>
    {
        cluster
        .CreateTopicIfNotExists(kafkaSettings.ProducerTopic)
        .CreateTopicIfNotExists(kafkaSettings.ConsumerTopic)
        .WithBrokers(kafkaSettings.Brokers)
            .AddProducer("producer", producer =>
            {
                producer
                    .DefaultTopic(kafkaSettings.ProducerTopic)
                    .AddMiddlewares(middlewares =>
                    {
                        middlewares.AddSerializer<JsonCoreSerializer>();
                    });
            })
            .AddConsumer(consumer =>
            {
                consumer
                    .Topic(kafkaSettings.ConsumerTopic)
                    .WithGroupId("kafkaflow-processor-group")
                    .WithBufferSize(100)
                    .WithWorkersCount(1)
                    .AddMiddlewares(middlewares =>
                    {
                        // Add JSON deserialization for incoming messages
                        middlewares.AddDeserializer<JsonCoreDeserializer>();
                        middlewares.Add<ProcessingMiddleware>();
                    });
            });
    });
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Basic status endpoint
app.MapGet("/", () => "KafkaFlow Processor running");

// Health check endpoint
app.MapHealthChecks("/health");

// Add an endpoint to generate and produce a test message
app.MapGet("/test", async (IProducerAccessor producerAccessor, IOptions<KafkaSettings> settings) =>
{
    try
    {
        var producer = producerAccessor.GetProducer("producer");
        
        if (producer == null)
            return Results.Problem("Producer not found");
            
        // Create a test message with sample data
        var testMessage = new MyMessage
        {
            Id = Guid.NewGuid().ToString(),
            Content = $"Test message created at {DateTime.Now}"
        };
            
        // Produce the test message to the input topic
        var result = await producer.ProduceAsync(
            settings.Value.ConsumerTopic ?? "input-topic", 
            testMessage.Id, 
            testMessage);
            
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error sending test message: {ex.Message}");
    }
});

// Add an endpoint to manually produce messages for testing
app.MapPost("/produce", async (IProducerAccessor producerAccessor, IOptions<KafkaSettings> settings, MyMessage message) =>
{
    try
    {
        var producer = producerAccessor.GetProducer("producer");
        
        if (producer == null)
            return Results.Problem("Producer not found");
            
        if (string.IsNullOrEmpty(message.Id))
            message.Id = Guid.NewGuid().ToString();
            
        // Produce the message to the input topic
        var result = await producer.ProduceAsync(
            settings.Value.ConsumerTopic ?? "input-topic", 
            message.Id, 
            message);
            
        return Results.Ok(new { 
            Status = "Message sent", 
            Topic = settings.Value.ConsumerTopic, 
            MessageId = message.Id, 
            Content = message.Content 
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error sending message: {ex.Message}");
    }
});

// Start the KafkaBus per the official documentation pattern
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.Run();

// Kafka settings POCO
public class KafkaSettings
{
    public string[]? Brokers { get; set; }
    public string? ConsumerTopic { get; set; }
    public string? ProducerTopic { get; set; }
}

// Message type T
public class MyMessage
{
    public string? Id { get; set; }
    public string? Content { get; set; }
    // The field that gets added during processing
    public string? ProcessedBy { get; set; }
    // Timestamp for tracking when the message was processed
    public DateTime? ProcessedAt { get; set; }
}

// Middleware for processing and producing
public class ProcessingMiddleware : IMessageMiddleware
{
    private readonly IOptions<KafkaSettings> _settings;
    private readonly IProducerAccessor _producerAccessor;
    private readonly ILogger<ProcessingMiddleware> _logger;

    public ProcessingMiddleware(
        IOptions<KafkaSettings> settings,
        IProducerAccessor producerAccessor,
        ILogger<ProcessingMiddleware> logger)
    {
        _settings = settings;
        _producerAccessor = producerAccessor;
        _logger = logger;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        try
        {
            _logger.LogInformation("Processing message from topic: {Topic}", context.ConsumerContext.Topic);
            
            var message = context.Message.Value as MyMessage;
            if (message is not null)
            {
                // Process the message - add our processing metadata
                message.ProcessedBy = "KafkaFlowProcessor";
                message.ProcessedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Processing message with ID: {MessageId}", message.Id);

                // Get the producer and send the processed message to the output topic
                var producer = _producerAccessor.GetProducer("producer");
                if (producer != null)
                {
                    var outputTopic = _settings.Value.ProducerTopic ?? "output-topic";
                    await producer.ProduceAsync(outputTopic, message.Id, message);
                    _logger.LogInformation("Message produced to topic: {Topic}", outputTopic);
                }
                else
                {
                    _logger.LogError("Producer not found, cannot send message to output topic");
                }
            }
            else
            {
                _logger.LogWarning("Received message with unsupported format");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
        }
        
        // Always call next to allow other middleware to process
        await next(context);
    }
}

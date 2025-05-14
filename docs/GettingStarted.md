# Getting Started with KafkaGenericProcessor.Core

This guide will walk you through the steps to set up and use KafkaGenericProcessor.Core in your .NET application.

## Prerequisites

- .NET 6.0 or later
- A Kafka cluster (or Docker for local development)
- Basic understanding of Kafka concepts

## Installation

Install the package via NuGet:

```bash
dotnet add package KafkaGenericProcessor.Core
```

Or using the Package Manager Console:

```
Install-Package KafkaGenericProcessor.Core
```

## Basic Setup

### 1. Configure appsettings.json

Add Kafka configuration to your `appsettings.json`:

```json
{
  "Kafka": {
    "Configurations": {
      "my-processor": {
        "Brokers": ["localhost:9092"],
        "ConsumerTopic": "input-topic",
        "ProducerTopic": "output-topic",
        "GroupId": "my-consumer-group",
        "WorkersCount": 5,
        "BufferSize": 100,
        "EnableDeadLetterQueue": true,
        "MaxRetryCount": 3,
        "RetryInitialDelayMs": 100,
        "RetryMaxDelayMs": 5000,
        "CreateTopicsIfNotExists": true
      }
    }
  }
}
```

### 2. Define Message Types

Create classes for your input and output messages:

```csharp
public class InputMessage
{
    public string Id { get; set; }
    public string Data { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OutputMessage
{
    public string Id { get; set; }
    public string ProcessedData { get; set; }
    public DateTime ProcessedAt { get; set; }
}
```

### 3. Create a Message Processor

Implement the `IMessageProcessor<TInput, TOutput>` interface:

```csharp
using KafkaGenericProcessor.Core.Abstractions;
using Microsoft.Extensions.Logging;

public class MyMessageProcessor : IMessageProcessor<InputMessage, OutputMessage>
{
    private readonly ILogger<MyMessageProcessor> _logger;
    
    public MyMessageProcessor(ILogger<MyMessageProcessor> logger)
    {
        _logger = logger;
    }
    
    public async Task<OutputMessage> ProcessAsync(
        InputMessage input, 
        string correlationId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing message {MessageId}. CorrelationId: {CorrelationId}", 
            input.Id, correlationId);
            
        // Your business logic here
        
        return new OutputMessage
        {
            Id = input.Id,
            ProcessedData = $"Processed: {input.Data}",
            ProcessedAt = DateTime.UtcNow
        };
    }
}
```

### 4. Create a Message Validator (Optional)

Implement the `IMessageValidator<TInput>` interface:

```csharp
using KafkaGenericProcessor.Core.Abstractions;
using KafkaGenericProcessor.Core.Validation;

public class MyMessageValidator : IMessageValidator<InputMessage>
{
    public async Task<bool> ValidateAsync(
        InputMessage message, 
        string correlationId, 
        CancellationToken cancellationToken = default)
    {
        var errors = await GetValidationErrorsAsync(message, correlationId, cancellationToken);
        return !errors.Any();
    }
    
    public Task<IReadOnlyList<ValidationError>> GetValidationErrorsAsync(
        InputMessage message, 
        string correlationId, 
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrEmpty(message.Id))
        {
            errors.Add(new ValidationError("Id", "ID is required"));
        }
        
        if (string.IsNullOrEmpty(message.Data))
        {
            errors.Add(new ValidationError("Data", "Data is required"));
        }
        
        return Task.FromResult<IReadOnlyList<ValidationError>>(errors);
    }
}
```

### 5. Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
using KafkaGenericProcessor.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

// ...

public void ConfigureServices(IServiceCollection services)
{
    // Add required services
    services.AddLogging();
    
    // Register your message processor and validator
    services.AddKeyedTransient<IMessageProcessor<InputMessage, OutputMessage>, MyMessageProcessor>("my-processor");
    services.AddKeyedTransient<IMessageValidator<InputMessage>, MyMessageValidator>("my-processor");
    
    // Configure Kafka Generic Processor
    services
        .AddKafkaGenericProcessors(Configuration)
        .AddConsumerProducerProcessor<InputMessage, OutputMessage>("my-processor")
        .Build();
    
    // Add health checks (optional)
    services.AddHealthChecks()
        .AddKafkaCheck();
}
```

## Usage Patterns

### Consumer-Producer Pattern

The example above demonstrates the consumer-producer pattern, where messages are consumed from one topic, processed, and then produced to another topic.

### Consumer-Only Pattern

For processing messages without producing output:

```csharp
// Define a consumer-only processor
public class MyConsumerOnlyProcessor : IConsumerOnlyProcessor<InputMessage>
{
    private readonly ILogger<MyConsumerOnlyProcessor> _logger;
    
    public MyConsumerOnlyProcessor(ILogger<MyConsumerOnlyProcessor> logger)
    {
        _logger = logger;
    }
    
    public async Task ProcessAsync(
        InputMessage input, 
        string correlationId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Consuming message {MessageId}. CorrelationId: {CorrelationId}", 
            input.Id, correlationId);
            
        // Your business logic here
    }
}

// Register in services
services.AddKeyedTransient<IConsumerOnlyProcessor<InputMessage>, MyConsumerOnlyProcessor>("my-consumer");

// Configure in builder
services
    .AddKafkaGenericProcessors(Configuration)
    .AddConsumerProcessor<InputMessage>("my-consumer")
    .Build();
```

### Producer-Only Pattern

For only producing messages:

```csharp
// Configure in builder
services
    .AddKafkaGenericProcessors(Configuration)
    .AddProducer("my-producer")
    .Build();

// Inject and use the producer
public class MyService
{
    private readonly IProducerAccessor _producerAccessor;
    
    public MyService(IProducerAccessor producerAccessor)
    {
        _producerAccessor = producerAccessor;
    }
    
    public async Task PublishMessageAsync(OutputMessage message)
    {
        var producer = _producerAccessor.GetProducer("my-producer");
        var correlationId = Guid.NewGuid().ToString();
        
        await producer.ProduceAsync("output-topic", message.Id, message);
    }
}
```

## Testing Your Configuration

Once configured, your application will automatically start consuming and processing messages when it runs. You can monitor the processing with logs:

```
info: MyNamespace.MyMessageProcessor[0]
      Processing message 123456. CorrelationId: 7890abcd-ef12-3456-7890-abcdef123456
```

## Next Steps

- Explore [Advanced Scenarios](AdvancedScenarios.md) for retry policies and dead letter queues
- Learn about [Middleware](Middleware.md) for custom processing pipelines
- Set up [Health Checks](HealthChecks.md) for monitoring
- Implement [Error Handling](ErrorHandling.md) strategies

For more information on configuration options and advanced features, refer to the [API Reference](ApiReference.md).
# KafkaGenericProcessor.Core

> NOTE: This library is 100% vibe code material, the requirements were wrapping the KafkaFlow library to provide a simplified consume + consumeproduce + produce abilities with an easy simple fluent pattern. this library is NOT production ready, do NOT use it in production, contributions are welcomed, but please vibe code them 🤟

[![NuGet](https://img.shields.io/nuget/v/KafkaGenericProcessor.Core.svg)](https://www.nuget.org/packages/KafkaGenericProcessor.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A robust, type-safe framework for building Kafka message processing applications in .NET. Simplify your Kafka consumer and producer implementation with a comprehensive set of features designed for enterprise applications.

## Features

- **Type-Safe Processing**: Handle Kafka messages with strongly-typed processors and serializers
- **Middleware Pipeline**: Process messages through configurable middleware chains
- **Validation**: Built-in support for message validation before processing
- **Error Handling**: Comprehensive exception hierarchy with automatic handling
- **Retry Policies**: Exponential backoff retry mechanism with configurable parameters
- **Health Checks**: Built-in health monitoring for Kafka connectivity
- **Structured Logging**: Consistent, correlation-based logging throughout the processing pipeline
- **Performance Metrics**: Track processing times and throughput
- **Correlation IDs**: Trace messages through the entire processing pipeline

## Quick Start

### Installation

```bash
dotnet add package KafkaGenericProcessor.Core
```

### Basic Configuration

Add the following to your `appsettings.json`:

```json
{
  "Kafka": {
    "Configurations": {
      "order-processor": {
        "Brokers": ["kafka-broker:9092"],
        "ConsumerTopic": "incoming-orders",
        "ProducerTopic": "processed-orders",
        "GroupId": "order-processing-group",
        "WorkersCount": 10,
        "BufferSize": 100,
        "CreateTopicsIfNotExists": true
      }
    }
  }
}
```

### Register Services

```csharp
// In Program.cs or Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add required services
    services.AddLogging();
    
    // Register your message processors and validators
    services.AddKeyedTransient<IMessageProcessor<OrderMessage, ProcessedOrderMessage>, OrderProcessor>("order-processor");
    services.AddKeyedTransient<IMessageValidator<OrderMessage>, OrderValidator>("order-processor");
    
    // Configure Kafka Generic Processor
    services
        .AddKafkaGenericProcessors(Configuration)
        .AddConsumerProducerProcessor<OrderMessage, ProcessedOrderMessage>("order-processor")
        .Build();
}
```

### Create Message Types

```csharp
public class OrderMessage
{
    public string OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal Amount { get; set; }
}

public class ProcessedOrderMessage
{
    public string OrderId { get; set; }
    public string Status { get; set; }
    public DateTimeOffset ProcessedAt { get; set; }
}
```

### Implement Processor

```csharp
public class OrderProcessor : IMessageProcessor<OrderMessage, ProcessedOrderMessage>
{
    private readonly ILogger<OrderProcessor> _logger;
    
    public OrderProcessor(ILogger<OrderProcessor> logger)
    {
        _logger = logger;
    }
    
    public async Task<ProcessedOrderMessage> ProcessAsync(
        OrderMessage input, 
        string correlationId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing order {OrderId} for customer {CustomerName}. CorrelationId: {CorrelationId}", 
            input.OrderId, input.CustomerName, correlationId);
            
        // Process the order (your business logic here)
        await Task.Delay(100, cancellationToken);
        
        return new ProcessedOrderMessage
        {
            OrderId = input.OrderId,
            Status = "Processed",
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }
}
```

### Implement Validator

```csharp
public class OrderValidator : IMessageValidator<OrderMessage>
{
    public async Task<bool> ValidateAsync(
        OrderMessage message, 
        string correlationId, 
        CancellationToken cancellationToken = default)
    {
        var errors = await GetValidationErrorsAsync(message, correlationId, cancellationToken);
        return !errors.Any();
    }
    
    public Task<IReadOnlyList<ValidationError>> GetValidationErrorsAsync(
        OrderMessage message, 
        string correlationId, 
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();
        
        if (string.IsNullOrEmpty(message.OrderId))
        {
            errors.Add(new ValidationError("OrderId", "Order ID is required"));
        }
        
        if (string.IsNullOrEmpty(message.CustomerName))
        {
            errors.Add(new ValidationError("CustomerName", "Customer name is required"));
        }
        
        if (message.Amount <= 0)
        {
            errors.Add(new ValidationError("Amount", "Amount must be greater than zero"));
        }
        
        return Task.FromResult<IReadOnlyList<ValidationError>>(errors);
    }
}
```

## Documentation

For more detailed documentation, please refer to:

- [Getting Started Guide](docs/GettingStarted.md)
- 
## Contributing

Contributions are welcome! Please see our [contributing guidelines](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

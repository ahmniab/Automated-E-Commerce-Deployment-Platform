# eShop.EventBusRabbitMQ

A production-ready RabbitMQ implementation of the **eShop EventBus** pattern for event-driven microservices communication. Built with **.NET 10**, designed for high performance, resilience, and observability.

> **No Aspire Dependencies** — Uses standard Microsoft.Extensions patterns for maximum compatibility and control.

## Features

✅ **Event Publishing & Subscription** — Seamless pub/sub pattern for microservice communication  
✅ **Automatic Retry with Polly** — Configurable exponential backoff resilience  
✅ **OpenTelemetry Integration** — Full tracing and observability support  
✅ **AOT Compatible** — Ready for Ahead-of-Time compilation  
✅ **.NET Configuration Binding** — Uses standard `appsettings.json` patterns  
✅ **Null Safety** — Full nullable reference types support  
✅ **Type-Safe DI** — Microsoft.Extensions.DependencyInjection integration

## Installation

```bash
dotnet add package eShop.EventBusRabbitMQ
```

## Quick Start

### 1. Configure appsettings.json

```json
{
  "EventBus": {
    "SubscriptionClientName": "my-service-consumer",
    "RetryCount": 10
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/"
  }
}
```

### 2. Register in Dependency Injection

```csharp
using eShop.EventBusRabbitMQ;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add RabbitMQ EventBus
builder.Services.AddRabbitMqEventBus(builder.Configuration);

// Add OpenTelemetry tracing
builder.Services.AddOpenTelemetry()
    .WithTracing();

var app = builder.Build();
app.Run();
```

### 3. Subscribe to Events

```csharp
public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    public Task Handle(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Order created: {@event.OrderId}");
        return Task.CompletedTask;
    }
}

// Register handler
services.AddIntegrationEventHandler<OrderCreatedIntegrationEvent, OrderCreatedEventHandler>();
```

### 4. Publish Events

```csharp
public class CreateOrderService
{
    private readonly IEventBus _eventBus;

    public CreateOrderService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task CreateOrderAsync(Order order, CancellationToken cancellationToken)
    {
        // Create order logic...

        // Publish integration event
        var @event = new OrderCreatedIntegrationEvent(order.Id, order.CustomerId);
        await _eventBus.PublishAsync(@event, cancellationToken);
    }
}
```

## Configuration Options

### EventBusOptions

| Property                 | Type     | Default  | Description                                       |
| ------------------------ | -------- | -------- | ------------------------------------------------- |
| `SubscriptionClientName` | `string` | Required | Consumer group name for RabbitMQ subscriptions    |
| `RetryCount`             | `int`    | 10       | Number of retry attempts with exponential backoff |

### RabbitMQOptions (ConnectionFactory)

| Property      | Type     | Default     | Description              |
| ------------- | -------- | ----------- | ------------------------ |
| `Host`        | `string` | `localhost` | RabbitMQ server hostname |
| `Port`        | `int`    | `5672`      | RabbitMQ server port     |
| `UserName`    | `string` | `guest`     | Authentication username  |
| `Password`    | `string` | `guest`     | Authentication password  |
| `VirtualHost` | `string` | `/`         | RabbitMQ virtual host    |

## Architecture

### Components

- **RabbitMQEventBus** — Core event pub/sub engine
- **RabbitMQConnectionFactory** — Manages RabbitMQ connections with automatic reconnection
- **RabbitMqDependencyInjectionExtensions** — DI configuration helpers
- **EventBusOptions** — Configuration model with validation
- **RabbitMQTelemetry** — OpenTelemetry integration for observability
- **ActivityExtensions** — Activity and tracing support

### Event Flow

```
Publisher
    ↓
IEventBus.PublishAsync()
    ↓
RabbitMQ Exchange (fanout)
    ↓
Queue per Subscriber
    ↓
IIntegrationEventHandler<TEvent>
    ↓
Consumer Success
```

## Resilience & Retry Logic

The library includes built-in retry logic powered by **Polly.Core**:

- **Exponential Backoff** — Configurable retry count with increasing delays
- **Automatic Recovery** — Connection auto-reconnection on failure
- **Activity Tracing** — Full observability of retry attempts

Configuration:

```json
{
  "EventBus": {
    "RetryCount": 10 // Max 10 retry attempts
  }
}
```

## OpenTelemetry & Observability

Fully integrated with OpenTelemetry for production observability:

```csharp
// Enable with your tracing setup
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddOtlpExporter()
        .AddSource("eShop.EventBusRabbitMQ"));
```

Activities are created for:

- Event publishing
- Event consumption
- RabbitMQ connection operations
- Retry attempts

## AOT Compatibility

This library is **AOT-compatible** with full C# feature support:

```xml
<IsAotCompatible>true</IsAotCompatible>
<EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>
```

Perfect for cloud-native deployments with minimal startup time and memory footprint.

## Requirements

- **.NET 10** or later
- **RabbitMQ 3.8+**
- `eShop.EventBus` (core contracts)

## Dependencies

| Package                  | Version | Purpose                    |
| ------------------------ | ------- | -------------------------- |
| `RabbitMQ.Client`        | 7.2.0   | RabbitMQ protocol client   |
| `eShop.EventBus`         | 1.0.0   | Event bus contracts        |
| `Polly.Core`             | 8.5.0   | Resilience policies        |
| `OpenTelemetry.Api`      | 1.12.0  | Observability API          |
| `Microsoft.Extensions.*` | 10.0.0  | DI, configuration, logging |

## Error Handling

The library provides detailed error handling:

```csharp
try
{
    await eventBus.PublishAsync(@event, cancellationToken);
}
catch (InvalidOperationException ex)
{
    // Configuration validation errors
    _logger.LogError(ex, "EventBus configuration failed");
}
catch (RabbitMQClientException ex)
{
    // RabbitMQ connection/protocol errors (after retries)
    _logger.LogError(ex, "RabbitMQ operation failed");
}
```

## Best Practices

1. **Health Checks** — Monitor RabbitMQ connection status
2. **Dead Letter Queues** — Configure DLQs for failed events
3. **Event Versioning** — Version integration events for backward compatibility
4. **Async All The Way** — Use `async/await` patterns throughout
5. **Structured Logging** — Integrate with your observability platform
6. **Configuration Validation** — The library validates on startup

## Troubleshooting

### Connection Refused

```
System.Net.Sockets.SocketException: Connection refused
```

✅ Verify RabbitMQ is running:

```bash
docker run -d --name rabbitmq -p 5672:15672 rabbitmq:latest
```

### Invalid Configuration

```
ArgumentException: SubscriptionClientName cannot be null or empty
```

✅ Ensure `EventBus:SubscriptionClientName` is set in `appsettings.json`

### No Handlers Registered

Verify handlers are registered:

```csharp
services.AddIntegrationEventHandler<MyEvent, MyHandler>();
```

## Performance Considerations

- **Connection Pooling** — Automatic connection reuse
- **Batch Publishing** — Design events efficiently (publish many small events, not few large ones)
- **Memory** — Events are deserialized on-demand; use lightweight event contracts
- **Threading** — Safe for concurrent use across multiple threads

## License

MIT — See LICENSE file for details

## Contributing

Contributions welcome! Please follow:

- Nullable reference types enabled
- AOT compatibility requirements
- Standard .NET conventions
- Unit test coverage for changes

## Support

For issues and feature requests, visit the [eShop repository](https://github.com/dotnet/eshop).

---

**Version:** 1.0.0  
**Framework:** .NET 10  
**Status:** Production Ready ✅

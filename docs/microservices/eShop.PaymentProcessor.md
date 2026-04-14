# `eShop.PaymentProcessor` Microservice

## Overview

### **Responsibility:**

- Subscribe to `OrderStatusChangedToStockConfirmedIntegrationEvent` events from the event bus.
- Simulate payment processing and decide success/failure based on configuration.
- Publish either `OrderPaymentSucceededIntegrationEvent` or `OrderPaymentFailedIntegrationEvent` for downstream services.

### Dependence to other services

- **RabbitMQ** for consuming stock-confirmed events and publishing payment-result events.
- **Ordering.API** as the primary consumer of emitted payment result events.
- **No direct database dependency** in the current implementation.

### Architecture & Tech Stack

- **Framework:** .NET 10 / ASP.NET Core hosted worker
- **Database:** None
- **Messaging:** RabbitMQ (`eShop.EventBusRabbitMQ`)
- **Caching:** None configured

## Build & Deploy

```bash
# Build & publish (from repo root)
dotnet publish src/microservices/eShop.PaymentProcessor/PaymentProcessor/PaymentProcessor.csproj -c Release -o ./publish

# Container build (from repo root)
docker build -t eshop/paymentprocessor:latest -f src/microservices/eShop.PaymentProcessor/Dockerfile src/microservices/eShop.PaymentProcessor
```

### Environment Variables

| Variable | Purpose | Example | Required |
| -------- | ------- | ------- | -------- |
| `ASPNETCORE_ENVIRONMENT` | Hosting environment | `Production` | Yes |
| `ASPNETCORE_URLS` | Bind addresses | `http://0.0.0.0:8080` | Yes |
| `ConnectionStrings__EventBus` | RabbitMQ AMQP connection string | `amqp://guest:guest@rabbitmq:5672` | Yes* |
| `EventBus__SubscriptionClientName` | Event bus subscription/client name | `PaymentProcessor` | No |
| `PaymentOptions__PaymentSucceeded` | Simulated payment result switch | `true` | Yes |
| `RabbitMQ__Host` | Broker host (alternative config shape) | `rabbitmq` | No* |
| `RabbitMQ__Port` | Broker port (alternative config shape) | `5672` | No* |
| `RabbitMQ__UserName` | Broker username (alternative config shape) | `guest` | No* |
| `RabbitMQ__Password` | Broker password (alternative config shape) | `guest` | No* |
| `RabbitMQ__VirtualHost` | Broker virtual host (alternative config shape) | `/` | No* |

\* RabbitMQ connectivity is required for this service to function. Use either `ConnectionStrings__EventBus` or the `RabbitMQ__*` configuration shape.

## Testing

```bash
dotnet test src/microservices/eShop.PaymentProcessor/eShop.PaymentProcessor.sln
```

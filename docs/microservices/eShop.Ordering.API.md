# `eShop.Ordering.API` Microservice

## Overview

### **Responsibility:**

- Expose versioned HTTP APIs to create, query, cancel, and ship customer orders.
- Coordinate order lifecycle transitions through commands, domain events, and integration events.
- Consume integration events (grace-period confirmed, stock confirmed/rejected, payment succeeded/failed) and update order state accordingly.
- Persist ordering aggregates in PostgreSQL and publish integration events via the event bus.

### Dependence to other services

- **PostgreSQL** (`OrderingDB`) for order aggregate persistence and queries.
- **RabbitMQ** for publishing and consuming ordering-related integration events.
- **Identity service** for bearer token validation (`Identity:Audience`, `Identity:Url`).
- **OrderProcessor** and **PaymentProcessor** as event-driven collaborators in the order workflow.

### Architecture & Tech Stack

- **Framework:** .NET 10 / ASP.NET Core Web API
- **Database:** PostgreSQL (Entity Framework Core + Npgsql)
- **Messaging:** RabbitMQ (`eShop.EventBusRabbitMQ` + integration event log)
- **Caching:** None configured

## Build & Deploy

```bash
# Build & publish (from repo root)
dotnet publish src/microservices/eShop.Ordering.API/Ordering.API/Ordering.API.csproj -c Release -o ./publish

# Container build (from repo root)
docker build -t eshop/ordering-api:latest -f src/microservices/eShop.Ordering.API/Dockerfile src/microservices/eShop.Ordering.API
```

### Environment Variables

| Variable | Purpose | Example | Required |
| -------- | ------- | ------- | -------- |
| `ASPNETCORE_ENVIRONMENT` | Hosting environment | `Production` | Yes |
| `ASPNETCORE_URLS` | Bind addresses | `http://0.0.0.0:8080` | Yes |
| `ConnectionStrings__orderingdb` | Ordering PostgreSQL connection string | `Host=postgres;Database=OrderingDB;Username=postgres;Password=...` | Yes |
| `ConnectionStrings__EventBus` | RabbitMQ AMQP connection string | `amqp://guest:guest@rabbitmq:5672` | Yes* |
| `EventBus__SubscriptionClientName` | Event bus subscription/client name | `Ordering` | No |
| `Identity__Audience` | JWT audience for API authorization | `orders` | Yes |
| `Identity__Url` | Identity authority URL | `http://identity-api:8080` | Yes |
| `OpenApi__Auth__ClientId` | Swagger OAuth client id | `orderingswaggerui` | No |
| `OpenApi__Auth__AppName` | Swagger OAuth application name | `Ordering Swagger UI` | No |
| `RabbitMQ__Host` | Broker host (alternative config shape) | `rabbitmq` | No* |
| `RabbitMQ__Port` | Broker port (alternative config shape) | `5672` | No* |
| `RabbitMQ__UserName` | Broker username (alternative config shape) | `guest` | No* |
| `RabbitMQ__Password` | Broker password (alternative config shape) | `guest` | No* |
| `RabbitMQ__VirtualHost` | Broker virtual host (alternative config shape) | `/` | No* |

\* RabbitMQ connectivity is required in practice for integration-event-based order processing. Use either `ConnectionStrings__EventBus` or the `RabbitMQ__*` configuration shape.

## Testing

```bash
dotnet test src/microservices/eShop.Ordering.API/eShop.Ordering.API.sln
```

# `eShop.Basket.API` Microservice

## Overview

### **Responsibility:**

- Handle the **shopping basket** (cart) operations for customers.
- Act as a fast **in-memory data store** mapping user sessions to their current cart items and quantities.
- Expose endpoints over **gRPC** for high-performance communication with the frontend and backend services (like the `eShop.WebApp` BFF).
- **Subscribe to RabbitMQ integration events** (e.g., `OrderStartedIntegrationEvent`) to clear out the user's basket as soon as their checkout process is successfully initiated.
- **Authenticate calls** via **gRPC Identity interceptors** against the eShop **Identity** service to verify the user owns the basket they are trying to modify.

### Dependence to other services

- **Redis** — Fast, distributed cache used for persisting basket state.
- **Identity (OpenID Connect authority)** — Validates JWT tokens and claims during gRPC calls.
- **RabbitMQ** — **Consumer** for integration events (like wiping a basket when an order starts).

### Architecture & Tech Stack

- **Framework:** .NET 10 / ASP.NET Core (gRPC services).
- **Database:** Redis (via `StackExchange.Redis`).
- **Messaging:** RabbitMQ (via `eShop.EventBusRabbitMQ` / `AddRabbitMqEventBus`).
- **Caching:** Handled inherently by Redis as the data store.

---

## Build & Deploy

```bash
# Build & publish (eShop.Basket.API workspace — project under Basket.API/)
dotnet publish Basket.API/Basket.API.csproj -c Release -o ./publish

# Build & publish (full dotnet/eShop repo layout)
dotnet publish src/microservices/eShop.Basket.API/Basket.API/Basket.API.csproj -c Release -o ./publish

# Run published output
dotnet ./publish/Basket.API.dll
```

```bash
# Container build
docker build -t eshop/basket.api:latest -f src/microservices/eShop.Basket.API/Dockerfile .

# Kubernetes (illustrative — align names/labels with your manifests)
kubectl apply -f deploy/basket-api.yaml
# Ensure env vars / ConfigMaps supply Redis connection string, Identity audience, ConnectionStrings__EventBus, etc.
```

The service utilizes a multi-stage `Dockerfile` that runs `dotnet publish` on `Basket.API/Basket.API.csproj` and creates a lightweight production container.

---

### Environment Variables

.NET binds hierarchical configuration to environment variables using `__` (e.g. `EventBus__SubscriptionClientName`). Keys below are those used or implied by the **eShop.Basket.API** codebase plus **shared** hosting/telemetry patterns.

| Variable | Purpose | Example | Required |
| -------- | ------- | ------- | -------- |
| `ASPNETCORE_ENVIRONMENT` | Hosting environment (`Development`, `Production`, …) | `Production` | Yes |
| `ASPNETCORE_URLS` | Kestrel bind URLs | `http://0.0.0.0:8080` | Yes (typical in containers) |
| `ConnectionStrings__Redis` | Redis connection string used for basket storage | `localhost:6379,abortConnect=false` | Yes |
| `Identity__Audience` | Required token audience expected from Identity | `basket` | Yes |
| `EventBus__SubscriptionClientName` | RabbitMQ consumer subscription name | `Basket` | No* |
| `ConnectionStrings__EventBus` | AMQP connection string for the event bus | `amqp://guest:pass@rabbitmq:5672/` | Yes* |
| `RabbitMQ__Host` | Broker host (when using discrete RabbitMQ options) | `rabbitmq` | No* |
| `RabbitMQ__Port` | Broker port | `5672` | No* |
| `RabbitMQ__UserName` | Broker user | `guest` | No* |
| `RabbitMQ__Password` | Broker password | `guest` | No* |
| `RabbitMQ__VirtualHost` | Virtual host | `/` | No* |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry OTLP exporter (enabled when set) | `http://otel-collector:4317` | No |
| `Logging__LogLevel__Default` | Default log level | `Information` | No |
| `Logging__LogLevel__Microsoft.AspNetCore` | ASP.NET Core log level | `Warning` | No |

\* **Event bus:** Required to listen for checkout-completion events (which clear the basket). Depending on configuration, either `ConnectionStrings__EventBus` or individual `RabbitMQ__*` values can be used.

---

## Testing

```bash
# From the microservice root
dotnet test Basket.UnitTests/Basket.UnitTests.csproj

# Or test entire solution
dotnet test
```

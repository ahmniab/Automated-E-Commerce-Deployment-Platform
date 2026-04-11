# eShop.ServiceDefaults

Shared defaults for ASP.NET Core microservices used in eShop-style distributed systems.

This package centralizes common service setup so each microservice can start with consistent observability, health checks, and authentication behavior.

## Features

- OpenTelemetry logging, metrics, and tracing setup
- OTLP exporter support via configuration
- Default health checks (`/health`, `/alive` in development)
- Standard HTTP resilience handlers for `HttpClient`
- JWT bearer authentication setup from configuration
- Outgoing `HttpClient` bearer token forwarding from current HTTP context
- Small helpers for claim access and required configuration values

## Target Framework

- `.NET 10` (`net10.0`)

## Install

```bash
dotnet add package eShop.ServiceDefaults
```

## Quick Start

In your service startup (for example `Program.cs`):

```csharp
using eShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaultAuthentication();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
```

## Configuration

### Authentication

`AddDefaultAuthentication` is enabled only when the `Identity` section exists.

```json
{
  "Identity": {
    "Url": "http://identity",
    "Audience": "basket"
  }
}
```

Notes:

- `sub` claim remapping is disabled to preserve the original subject claim.
- `RequireHttpsMetadata` is set to `false`.
- Audience validation is disabled (`ValidateAudience = false`).

### OpenTelemetry Export

OTLP exporting is enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is set.

Example:

```bash
export OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
```

## Provided Extensions

### `IHostApplicationBuilder`

- `AddServiceDefaults()`
  - Calls `AddBasicServiceDefaults()`
  - Configures default `HttpClient` resilience handlers
  - Includes commented hooks for service discovery

- `AddBasicServiceDefaults()`
  - Adds default health checks
  - Configures OpenTelemetry

- `ConfigureOpenTelemetry()`
  - Adds OpenTelemetry logging
  - Adds metrics: ASP.NET Core, HTTP client, runtime, and `Experimental.Microsoft.Extensions.AI` meter
  - Adds tracing: ASP.NET Core, gRPC client, HTTP client, and `Experimental.Microsoft.Extensions.AI` source
  - Uses `AlwaysOnSampler` in development

- `AddDefaultHealthChecks()`
  - Registers `self` health check tagged with `live`

- `AddDefaultAuthentication()`
  - Adds JWT bearer authentication and authorization when `Identity` configuration exists

### `WebApplication`

- `MapDefaultEndpoints()`
  - In development only:
    - Maps `/health` for readiness
    - Maps `/alive` for liveness checks (`live` tag)

### `IHttpClientBuilder`

- `AddAuthToken()`
  - Adds a delegating handler that reads `access_token` from the current request context
  - Forwards token as `Authorization: Bearer <token>` on outgoing requests

### `ClaimsPrincipal`

- `GetUserId()` -> reads `sub`
- `GetUserName()` -> reads `ClaimTypes.Name`

### `IConfiguration`

- `GetRequiredValue(name)` -> throws if value is missing

## Development Notes

- Health endpoints are intentionally mapped only in development by default.
- Service discovery hooks are present in code but currently commented out.

## License

MIT

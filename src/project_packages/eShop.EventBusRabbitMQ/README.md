# eShop.EventBusRabbitMQ — Project Repository

Welcome to the **eShop.EventBusRabbitMQ** project! This repository contains a production-ready RabbitMQ implementation of the eShop EventBus pattern for building event-driven microservices.

This is a **pre-built NuGet package** ready for consumption by other services in the eShop ecosystem.

## 📋 Project Structure

```
eShop.EventBusRabbitMQ/
├── EventBusRabbitMQ/              # Main library project
│   ├── ActivityExtensions.cs       # OpenTelemetry activity helpers
│   ├── EventBusOptions.cs          # Configuration model
│   ├── EventBusRabbitMQ.cs         # Core event bus implementation
│   ├── RabbitMqDependencyInjectionExtensions.cs   # DI setup
│   ├── RabbitMQConnectionFactory.cs # Connection management
│   ├── RabbitMQTelemetry.cs        # Observability integration
│   ├── GlobalUsings.cs             # Global namespace imports
│   ├── EventBusRabbitMQ.csproj     # Project file
│   └── bin/, obj/                  # Build artifacts
├── docs/
│   └── README.md                   # Package documentation & usage guide
└── eShop.EventBusRabbitMQ.slnx     # Solution file
```

## 🚀 Quick Overview

| Aspect           | Details                                                 |
| ---------------- | ------------------------------------------------------- |
| **Purpose**      | Production RabbitMQ event bus for microservices         |
| **Framework**    | .NET 10                                                 |
| **Package ID**   | `eShop.EventBusRabbitMQ` v1.0.0                         |
| **License**      | MIT                                                     |
| **Key Features** | Pub/Sub, retry logic, OpenTelemetry, AOT compatible     |
| **Dependencies** | RabbitMQ.Client 7.2.0, Polly.Core 8.5.0, MSE.\* v10.0.0 |

## 📦 Building the Package

### Prerequisites

- **.NET 10 SDK** or later
- **RabbitMQ** (for testing — installation optional for building)

### Build the Project

Navigate to the project directory and run:

```bash
cd EventBusRabbitMQ
dotnet build
```

**Expected Output:**

```
✅ Build succeeded.
📊 0 Warning(s)
```

### Create NuGet Package

Generate the distributable NuGet package:

```bash
dotnet pack
```

**Generated Files:**

- `bin/Release/eShop.EventBusRabbitMQ.1.0.0.nupkg` — Main package
- `bin/Release/eShop.EventBusRabbitMQ.1.0.0.snupkg` — Symbols package
- `docs/README.md` — Packaged documentation

## 🔧 Development Setup

### Visual Studio / VS Code

```bash
# Open the solution
code .

# Or open in Visual Studio
# File → Open Folder → Select this directory
```

### Project Configuration

Key `.csproj` settings:

```xml
<TargetFramework>net10.0</TargetFramework>
<IsAotCompatible>true</IsAotCompatible>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<GenerateDocumentationFile>true</GenerateDocumentationFile>
```

## 📚 Documentation

For **detailed usage**, **configuration**, **API reference**, and **examples**, see:

👉 **[Package Documentation](docs/README.md)**

Topics covered:

- Installation & setup
- Quick start guide
- Configuration options
- Architecture overview
- Event publishing & subscription
- Retry and resilience patterns
- OpenTelemetry integration
- Troubleshooting
- Best practices

## 🧪 Testing

### Manual Testing with Docker

**Start RabbitMQ:**

```bash
docker run -d --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:latest-management
```

**Access RabbitMQ Management UI:**

```
http://localhost:15672
Default credentials: guest / guest
```

### Build & Verify

```bash
dotnet build --configuration Release
dotnet test  # (if test projects added)
```

## 🏗️ Architecture Highlights

### Component Diagram

```
┌─────────────────┐
│  Your Service   │
│  (Consumer)     │
└────────┬────────┘
         │
    IEventBus
         │
┌────────▼────────────────────────────────┐
│  RabbitMQEventBus (Core Implementation)  │
├─────────────────────────────────────────┤
│ • Publish event to exchange              │
│ • Subscribe to queue                     │
│ • Handle consumer acknowledgment          │
│ • Retry with exponential backoff         │
└────────┬────────────────────────────────┘
         │
┌────────▼────────────────────────────────┐
│    RabbitMQ (Message Broker)            │
│  • Exchange (fanout)                     │
│  • Queues (per subscriber)               │
└─────────────────────────────────────────┘
         │
    ┌────▼────┐  ┌─────────┐
    │ Service │  │ Service │
    │    A    │  │    B    │
    └─────────┘  └─────────┘
```

### Key Classes

| Class                                   | Purpose                      |
| --------------------------------------- | ---------------------------- |
| `RabbitMQEventBus`                      | Core pub/sub engine          |
| `RabbitMQConnectionFactory`             | Manages RabbitMQ connections |
| `EventBusOptions`                       | Configuration container      |
| `RabbitMqDependencyInjectionExtensions` | DI setup                     |
| `RabbitMQTelemetry`                     | OpenTelemetry integration    |

## 🔐 Security & Compliance

- ✅ **Nullable Reference Types** — Full null-safety
- ✅ **AOT Compatible** — Minimal memory footprint
- ✅ **OpenTelemetry Ready** — Production observability
- ✅ **No External Secrets** — Config-driven credentials
- ✅ **MIT Licensed** — Open source

## 📊 Versioning

| Component       | Version |
| --------------- | ------- |
| Package         | 1.0.0   |
| .NET Target     | 10.0    |
| RabbitMQ.Client | 7.2.0   |
| Polly.Core      | 8.5.0   |
| OpenTelemetry   | 1.12.0  |

## 🚧 Building for Production

### Pre-Release Checklist

- [ ] All tests passing
- [ ] Documentation reviewed
- [ ] NuGet package created and tested
- [ ] Version bumped in `.csproj`
- [ ] Package published to NuGet feed

### Package Metadata

```xml
<PackageId>eShop.EventBusRabbitMQ</PackageId>
<Version>1.0.0</Version>
<Authors>Microsoft</Authors>
<Description>RabbitMQ implementation of the eShop EventBus...</Description>
<PackageTags>eshop;eventbus;rabbitmq;messaging;microservices</PackageTags>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
```

## 🔄 Continuous Integration

Recommended CI/CD checks:

```yaml
- Build: dotnet build
- Pack: dotnet pack
- Publish: NuGet push (when tagged)
```

## 🐛 Troubleshooting

### Build Errors

**Error:** `NU1301: Unable to load service index`

- **Cause:** NuGet feed authentication
- **Fix:** Ensure `eShop.EventBus` package source is configured

**Error:** `CS0246: The type or namespace name 'eShop' could not be found`

- **Cause:** Missing `eShop.EventBus` package
- **Fix:** `dotnet restore`

### Runtime Issues

See [docs/README.md#troubleshooting](docs/README.md#troubleshooting) for runtime troubleshooting.

## 🤝 Contributing

Requirements for contributions:

1. **Nullable Safety** — Enable `<Nullable>enable</Nullable>`
2. **AOT Compatibility** — Test with `<IsAotCompatible>true</IsAotCompatible>`
3. **Documentation** — Update `docs/README.md`
4. **Unit Tests** — Add tests for new functionality
5. **Follow Conventions** — Match existing code style

## 📖 Learning Resources

- [RabbitMQ Tutorials](https://www.rabbitmq.com/tutorials)
- [eShop Reference Application](https://github.com/dotnet/eshop)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Polly Resilience Library](https://www.pollydocs.org/)
- [Microsoft.Extensions Packages](https://github.com/dotnet/runtime/tree/main/src/libraries/Microsoft.Extensions)

## 📞 Support

- **Issues:** Report bugs and feature requests in the eShop repository
- **Documentation:** See [docs/README.md](docs/README.md)
- **Examples:** Review the quick start guide in the package docs

## 📄 License

```
MIT License

Copyright (c) Microsoft Corporation.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

---

## 🚀 Next Steps

1. **Understand the Package** — Read [docs/README.md](docs/README.md)
2. **Build Locally** — Run `dotnet build` in `EventBusRabbitMQ/`
3. **Create Package** — Run `dotnet pack`
4. **Use in Services** — Add via `dotnet add package eShop.EventBusRabbitMQ`

---

**Status:** ✅ Production Ready  
**Last Updated:** April 2026  
**Maintainer:** Microsoft — eShop Team

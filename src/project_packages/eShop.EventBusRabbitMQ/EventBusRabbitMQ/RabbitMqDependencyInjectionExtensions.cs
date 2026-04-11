using eShop.EventBusRabbitMQ;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for RabbitMQ event bus.
/// Provides standard .NET 10 configuration without Aspire dependencies.
/// </summary>
/// <remarks>
/// Configuration example (appsettings.json):
/// {
///   "EventBus": {
///     "SubscriptionClientName": "my-service-consumer",
///     "RetryCount": 10
///   },
///   "RabbitMQ": {
///     "Host": "localhost",
///     "Port": 5672,
///     "UserName": "guest",
///     "Password": "guest",
///     "VirtualHost": "/"
///   }
/// }
/// </remarks>
public static class RabbitMqDependencyInjectionExtensions
{
    private const string EventBusSectionName = "EventBus";
    private const string RabbitMQSectionName = "RabbitMQ";
    private const string DefaultHostName = "localhost";
    private const int DefaultPort = 5672;

    /// <summary>
    /// Adds RabbitMQ event bus to the service collection using standard .NET configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The IConfiguration to bind options from.</param>
    /// <returns>An IEventBusBuilder for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    public static IEventBusBuilder AddRabbitMqEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // 1. Configure EventBusOptions from configuration section
        services.Configure<EventBusOptions>(configuration.GetSection(EventBusSectionName));
        ValidateEventBusOptions(configuration);

        // 2. Register RabbitMQ connection factory and connection
        RegisterRabbitMQConnection(services, configuration);

        // 3. Register OpenTelemetry for RabbitMQ tracing
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource(RabbitMQTelemetry.ActivitySourceName);
            });

        // 4. Register core event bus abstractions
        services.AddSingleton<RabbitMQTelemetry>();
        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        // 5. Register RabbitMQEventBus as IHostedService for automatic message consumption
        services.AddSingleton<IHostedService>(sp =>
            (RabbitMQEventBus)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(services);
    }

    /// <summary>
    /// Adds RabbitMQ event bus with manual connection parameters.
    /// </summary>
    public static IEventBusBuilder AddRabbitMqEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        string hostName,
        int port = 5672,
        string? userName = null,
        string? password = null,
        string? virtualHost = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostName);

        // Configure EventBusOptions
        services.Configure<EventBusOptions>(configuration.GetSection(EventBusSectionName));
        ValidateEventBusOptions(configuration);

        // Register connection factory with explicit parameters
        services.AddSingleton(sp =>
            new RabbitMQConnectionFactory(
                sp.GetRequiredService<ILogger<RabbitMQConnectionFactory>>(),
                hostName,
                port,
                userName,
                password,
                virtualHost));

        // Register RabbitMQ connection as singleton
        services.AddSingleton(sp =>
            sp.GetRequiredService<RabbitMQConnectionFactory>().CreateConnection());

        // Register OpenTelemetry
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddSource(RabbitMQTelemetry.ActivitySourceName);
            });

        // Register event bus abstractions
        services.AddSingleton<RabbitMQTelemetry>();
        services.AddSingleton<IEventBus, RabbitMQEventBus>();
        services.AddSingleton<IHostedService>(sp =>
            (RabbitMQEventBus)sp.GetRequiredService<IEventBus>());

        return new EventBusBuilder(services);
    }

    /// <summary>
    /// Registers RabbitMQ connection from configuration.
    /// </summary>
    private static void RegisterRabbitMQConnection(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbitMqSection = configuration.GetSection(RabbitMQSectionName);
        var hostName = rabbitMqSection.GetValue("Host", DefaultHostName);
        var port = rabbitMqSection.GetValue("Port", DefaultPort);
        var userName = rabbitMqSection.GetValue<string?>("UserName");
        var password = rabbitMqSection.GetValue<string?>("Password");
        var virtualHost = rabbitMqSection.GetValue<string?>("VirtualHost");

        services.AddSingleton(sp =>
            new RabbitMQConnectionFactory(
                sp.GetRequiredService<ILogger<RabbitMQConnectionFactory>>(),
                hostName,
                port,
                userName,
                password,
                virtualHost));

        services.AddSingleton(sp =>
            sp.GetRequiredService<RabbitMQConnectionFactory>().CreateConnection());
    }

    /// <summary>
    /// Validates that required EventBusOptions are configured.
    /// </summary>
    private static void ValidateEventBusOptions(IConfiguration configuration)
    {
        var eventBusSection = configuration.GetSection(EventBusSectionName);
        var subscriptionClientName = eventBusSection.GetValue<string?>("SubscriptionClientName");

        if (string.IsNullOrWhiteSpace(subscriptionClientName))
        {
            throw new InvalidOperationException(
                $"EventBus configuration is missing required 'SubscriptionClientName' setting. " +
                $"Configure it in appsettings.json under '{EventBusSectionName}' section.");
        }
    }

    /// <summary>
    /// Builder for fluent configuration of event bus.
    /// </summary>
    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}


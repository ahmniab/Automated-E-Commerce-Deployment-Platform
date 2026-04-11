namespace eShop.EventBusRabbitMQ;

/// <summary>
/// Factory for creating and managing RabbitMQ connections.
/// Replaces Aspire.RabbitMQ.Client functionality with standard .NET patterns.
/// </summary>
public class RabbitMQConnectionFactory
{
    private readonly ILogger<RabbitMQConnectionFactory> _logger;
    private readonly string _hostName;
    private readonly int _port;
    private readonly string? _userName;
    private readonly string? _password;
    private readonly string? _virtualHost;

    public RabbitMQConnectionFactory(
        ILogger<RabbitMQConnectionFactory> logger,
        string hostName,
        int port = 5672,
        string? userName = null,
        string? password = null,
        string? virtualHost = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
        _port = port;
        _userName = userName ?? "guest";
        _password = password ?? "guest";
        _virtualHost = virtualHost ?? "/";
    }

    /// <summary>
    /// Creates a RabbitMQ connection with automatic retry logic.
    /// </summary>
    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName ?? "guest",
            Password = _password ?? "guest",
            VirtualHost = _virtualHost ?? "/",
            // Connection resilience
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            // Socket/network timeouts
            RequestedConnectionTimeout = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
        };

        _logger.LogInformation(
            "Creating RabbitMQ connection to {HostName}:{Port}/{VirtualHost}",
            _hostName, _port, _virtualHost ?? "/");

        try
        {
            var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _logger.LogInformation("RabbitMQ connection established successfully");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ connection");
            throw;
        }
    }

    /// <summary>
    /// Creates a RabbitMQ connection asynchronously.
    /// </summary>
    public async Task<IConnection> CreateConnectionAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostName,
            Port = _port,
            UserName = _userName ?? "guest",
            Password = _password ?? "guest",
            VirtualHost = _virtualHost ?? "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            RequestedConnectionTimeout = TimeSpan.FromSeconds(10),
            RequestedHeartbeat = TimeSpan.FromSeconds(30),
        };

        _logger.LogInformation(
            "Creating RabbitMQ connection to {HostName}:{Port}/{VirtualHost}",
            _hostName, _port, _virtualHost ?? "/");

        try
        {
            var connection = await factory.CreateConnectionAsync();
            _logger.LogInformation("RabbitMQ connection established successfully");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RabbitMQ connection");
            throw;
        }
    }
}

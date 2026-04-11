namespace eShop.EventBusRabbitMQ;

/// <summary>
/// Configuration options for RabbitMQ event bus.
/// Expected configuration section: "EventBus"
/// </summary>
public class EventBusOptions
{
    /// <summary>
    /// Name of the subscription queue/consumer group.
    /// Required property that must be configured via IOptions.
    /// </summary>
    public required string SubscriptionClientName { get; set; }

    /// <summary>
    /// Number of retry attempts for transient failures (default: 10).
    /// </summary>
    public int RetryCount { get; set; } = 10;
}

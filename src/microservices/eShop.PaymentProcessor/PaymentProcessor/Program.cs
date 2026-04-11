var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRabbitMqEventBus(builder.Configuration)
    .AddSubscription<OrderStatusChangedToStockConfirmedIntegrationEvent, OrderStatusChangedToStockConfirmedIntegrationEventHandler>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration(nameof(PaymentOptions));

var app = builder.Build();

app.MapDefaultEndpoints();

await app.RunAsync();

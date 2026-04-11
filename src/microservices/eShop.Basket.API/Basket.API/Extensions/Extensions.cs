using eShop.Basket.API.Repositories;
using eShop.Basket.API.IntegrationEvents.EventHandling;
using eShop.Basket.API.IntegrationEvents.EventHandling.Events;

namespace eShop.Basket.API.Extensions;

public static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultAuthentication();

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

        builder.Services.AddSingleton<IBasketRepository, RedisBasketRepository>();

        builder.Services.AddRabbitMqEventBus(builder.Configuration)
                        .AddSubscription<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();
    }
}

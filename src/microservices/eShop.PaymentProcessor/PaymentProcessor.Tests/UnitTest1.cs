using eShop.EventBus.Abstractions;
using eShop.EventBus.Events;
using eShop.PaymentProcessor;
using eShop.PaymentProcessor.IntegrationEvents.EventHandling;
using eShop.PaymentProcessor.IntegrationEvents.Events;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace PaymentProcessor.Tests;

public class OrderStatusChangedToStockConfirmedIntegrationEventHandlerTests
{
    [Fact]
    public async Task Handle_WhenPaymentSucceeds_PublishesOrderPaymentSucceededEvent()
    {
        var eventBusMock = new Mock<IEventBus>();
        IntegrationEvent? publishedEvent = null;

        eventBusMock
            .Setup(x => x.PublishAsync(It.IsAny<IntegrationEvent>()))
            .Callback<IntegrationEvent>(integrationEvent => publishedEvent = integrationEvent)
            .Returns(Task.CompletedTask);

        var optionsMock = new Mock<IOptionsMonitor<PaymentOptions>>();
        optionsMock.Setup(x => x.CurrentValue).Returns(new PaymentOptions { PaymentSucceeded = true });

        var sut = new OrderStatusChangedToStockConfirmedIntegrationEventHandler(
            eventBusMock.Object,
            optionsMock.Object,
            NullLogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler>.Instance);

        var inputEvent = new OrderStatusChangedToStockConfirmedIntegrationEvent(1234);

        await sut.Handle(inputEvent);

        eventBusMock.Verify(
            x => x.PublishAsync(It.IsAny<IntegrationEvent>()),
            Times.Once);

        var successEvent = Assert.IsType<OrderPaymentSucceededIntegrationEvent>(publishedEvent);
        Assert.Equal(inputEvent.OrderId, successEvent.OrderId);
    }

    [Fact]
    public async Task Handle_WhenPaymentFails_PublishesOrderPaymentFailedEvent()
    {
        var eventBusMock = new Mock<IEventBus>();
        IntegrationEvent? publishedEvent = null;

        eventBusMock
            .Setup(x => x.PublishAsync(It.IsAny<IntegrationEvent>()))
            .Callback<IntegrationEvent>(integrationEvent => publishedEvent = integrationEvent)
            .Returns(Task.CompletedTask);

        var optionsMock = new Mock<IOptionsMonitor<PaymentOptions>>();
        optionsMock.Setup(x => x.CurrentValue).Returns(new PaymentOptions { PaymentSucceeded = false });

        var sut = new OrderStatusChangedToStockConfirmedIntegrationEventHandler(
            eventBusMock.Object,
            optionsMock.Object,
            NullLogger<OrderStatusChangedToStockConfirmedIntegrationEventHandler>.Instance);

        var inputEvent = new OrderStatusChangedToStockConfirmedIntegrationEvent(5678);

        await sut.Handle(inputEvent);

        eventBusMock.Verify(
            x => x.PublishAsync(It.IsAny<IntegrationEvent>()),
            Times.Once);

        var failedEvent = Assert.IsType<OrderPaymentFailedIntegrationEvent>(publishedEvent);
        Assert.Equal(inputEvent.OrderId, failedEvent.OrderId);
    }
}

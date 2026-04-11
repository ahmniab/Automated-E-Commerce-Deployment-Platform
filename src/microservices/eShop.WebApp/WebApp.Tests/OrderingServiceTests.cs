using eShop.WebApp.Services;
using Xunit;

namespace WebApp.Tests;

/// <summary>
/// Tests for OrderingService and related records
/// </summary>
public class OrderingServiceTests
{
    [Fact]
    public void OrderRecord_CanBeCreatedWithValidData()
    {
        // Arrange
        var orderDate = DateTime.UtcNow;

        // Act
        var order = new OrderRecord(
            1001,
            orderDate,
            "Completed",
            150.00m
        );

        // Assert
        Assert.Equal(1001, order.OrderNumber);
        Assert.Equal(orderDate, order.Date);
        Assert.Equal("Completed", order.Status);
        Assert.Equal(150.00m, order.Total);
    }

    [Fact]
    public void OrderRecord_SupportsMultipleStatuses()
    {
        // Arrange
        var statuses = new[] { "Pending", "Completed", "Cancelled", "Shipped" };

        // Act & Assert
        foreach (var status in statuses)
        {
            var order = new OrderRecord(1001, DateTime.UtcNow, status, 100.00m);
            Assert.Equal(status, order.Status);
        }
    }

    [Fact]
    public void CreateOrderRequest_ContainsRequiredAddressFields()
    {
        // Arrange
        var items = new List<BasketItem>
        {
            new() { Id = "1", ProductId = 1, ProductName = "Product 1", UnitPrice = 10.00m, Quantity = 2 }
        };

        // Act
        var request = new CreateOrderRequest(
            "user-123",
            "John Doe",
            "New York",
            "123 Main St",
            "NY",
            "USA",
            "10001",
            "4111111111111111",
            "John Doe",
            DateTime.UtcNow.AddYears(1),
            "123",
            1,
            "John Doe",
            items
        );

        // Assert
        Assert.Equal("user-123", request.UserId);
        Assert.Equal("John Doe", request.UserName);
        Assert.Equal("New York", request.City);
        Assert.Equal("123 Main St", request.Street);
        Assert.Equal("USA", request.Country);
    }

    [Theory]
    [InlineData(100.00)]
    [InlineData(999.99)]
    [InlineData(50.50)]
    public void OrderRecord_AcceptsVariousTotalAmounts(decimal total)
    {
        // Act
        var order = new OrderRecord(1001, DateTime.UtcNow, "Completed", total);

        // Assert
        Assert.Equal(total, order.Total);
    }

    [Fact]
    public void BasketItem_CanRepresentOrderLineItem()
    {
        // Act
        var item = new BasketItem
        {
            Id = "item-1",
            ProductId = 5,
            ProductName = "Test Product",
            UnitPrice = 29.99m,
            OldUnitPrice = 39.99m,
            Quantity = 3
        };

        // Assert
        Assert.Equal("item-1", item.Id);
        Assert.Equal(5, item.ProductId);
        Assert.Equal("Test Product", item.ProductName);
        Assert.Equal(29.99m, item.UnitPrice);
        Assert.Equal(39.99m, item.OldUnitPrice);
        Assert.Equal(3, item.Quantity);
    }
}

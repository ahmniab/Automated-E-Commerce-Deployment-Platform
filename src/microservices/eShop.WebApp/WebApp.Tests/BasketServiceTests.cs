using eShop.WebApp.Services;
using Xunit;

namespace WebApp.Tests;

/// <summary>
/// Tests for BasketService
/// Note: BasketService uses gRPC which requires integration testing.
/// These tests validate the BasketQuantity record and service structure.
/// </summary>
public class BasketServiceTests
{
    [Fact]
    public void BasketQuantity_CanBeCreatedWithValidValues()
    {
        // Arrange & Act
        var basketItem = new BasketQuantity(123, 5);

        // Assert
        Assert.Equal(123, basketItem.ProductId);
        Assert.Equal(5, basketItem.Quantity);
    }

    [Fact]
    public void BasketQuantity_SupportsEquality()
    {
        // Arrange
        var item1 = new BasketQuantity(1, 5);
        var item2 = new BasketQuantity(1, 5);
        var item3 = new BasketQuantity(2, 5);

        // Assert
        Assert.Equal(item1, item2);
        Assert.NotEqual(item1, item3);
    }

    [Fact]
    public void BasketQuantity_MultipleItems_CanBeCollected()
    {
        // Arrange
        var items = new List<BasketQuantity>
        {
            new(1, 5),
            new(2, 10),
            new(3, 3)
        };

        // Act & Assert
        Assert.Equal(3, items.Count);
        Assert.Equal(1, items[0].ProductId);
        Assert.Equal(10, items[1].Quantity);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(100, 999)]
    [InlineData(50, 0)]
    public void BasketQuantity_AcceptsVariousValues(int productId, int quantity)
    {
        // Act
        var item = new BasketQuantity(productId, quantity);

        // Assert
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
    }
}

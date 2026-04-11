using eShop.Catalog.API.Infrastructure.Exceptions;
using eShop.Catalog.API.Model;

namespace Catalog.API.Tests;

public class CatalogItemTests
{
    private static CatalogItem CreateItem(int availableStock = 10, int maxStock = 20, int restockThreshold = 5)
    {
        return new CatalogItem("Test Item")
        {
            AvailableStock = availableStock,
            MaxStockThreshold = maxStock,
            RestockThreshold = restockThreshold,
            Price = 9.99m
        };
    }

    // ── RemoveStock ──────────────────────────────────────────

    [Fact]
    public void RemoveStock_WithSufficientStock_ReturnsDesiredQuantity()
    {
        var item = CreateItem(availableStock: 10);

        int removed = item.RemoveStock(5);

        Assert.Equal(5, removed);
        Assert.Equal(5, item.AvailableStock);
    }

    [Fact]
    public void RemoveStock_WithInsufficientStock_ReturnsOnlyAvailable()
    {
        var item = CreateItem(availableStock: 3);

        int removed = item.RemoveStock(10);

        Assert.Equal(3, removed);
        Assert.Equal(0, item.AvailableStock);
    }

    [Fact]
    public void RemoveStock_WhenStockIsZero_ThrowsDomainException()
    {
        var item = CreateItem(availableStock: 0);

        var ex = Assert.Throws<CatalogDomainException>(() => item.RemoveStock(1));
        Assert.Contains("sold out", ex.Message);
    }

    [Fact]
    public void RemoveStock_WithZeroQuantity_ThrowsDomainException()
    {
        var item = CreateItem(availableStock: 10);

        var ex = Assert.Throws<CatalogDomainException>(() => item.RemoveStock(0));
        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void RemoveStock_WithNegativeQuantity_ThrowsDomainException()
    {
        var item = CreateItem(availableStock: 10);

        var ex = Assert.Throws<CatalogDomainException>(() => item.RemoveStock(-5));
        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void RemoveStock_ExactlyAllStock_ReturnsAllAndLeavesZero()
    {
        var item = CreateItem(availableStock: 7);

        int removed = item.RemoveStock(7);

        Assert.Equal(7, removed);
        Assert.Equal(0, item.AvailableStock);
    }

    // ── AddStock ─────────────────────────────────────────────

    [Fact]
    public void AddStock_BelowMax_AddsFullQuantity()
    {
        var item = CreateItem(availableStock: 5, maxStock: 20);

        int added = item.AddStock(10);

        Assert.Equal(10, added);
        Assert.Equal(15, item.AvailableStock);
    }

    [Fact]
    public void AddStock_ExceedingMax_CapsAtMaxThreshold()
    {
        var item = CreateItem(availableStock: 15, maxStock: 20);

        int added = item.AddStock(10);

        Assert.Equal(5, added);
        Assert.Equal(20, item.AvailableStock);
    }

    [Fact]
    public void AddStock_ExactlyToMax_AddsFullQuantity()
    {
        var item = CreateItem(availableStock: 10, maxStock: 20);

        int added = item.AddStock(10);

        Assert.Equal(10, added);
        Assert.Equal(20, item.AvailableStock);
    }

    [Fact]
    public void AddStock_SetsOnReorderToFalse()
    {
        var item = CreateItem(availableStock: 2, maxStock: 20);
        item.OnReorder = true;

        item.AddStock(5);

        Assert.False(item.OnReorder);
    }

    [Fact]
    public void AddStock_WhenAlreadyAtMax_AddsZero()
    {
        var item = CreateItem(availableStock: 20, maxStock: 20);

        int added = item.AddStock(5);

        Assert.Equal(0, added);
        Assert.Equal(20, item.AvailableStock);
    }
}

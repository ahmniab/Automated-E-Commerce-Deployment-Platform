using eShop.WebAppComponents.Catalog;
using eShop.WebAppComponents.Services;
using Xunit;

namespace WebApp.Tests;

/// <summary>
/// Tests for CatalogService HTTP operations
/// </summary>
public class CatalogServiceTests
{
    [Fact]
    public void CatalogResult_CanBeCreatedWithValidData()
    {
        // Arrange
        var brand = new CatalogBrand(1, "Brand1");
        var type = new CatalogItemType(1, "Type1");
        var items = new List<CatalogItem>
        {
            new(1, "Product 1", "Description", 10.00m, "url1", 1, brand, 1, type),
            new(2, "Product 2", "Description", 20.00m, "url2", 1, brand, 1, type)
        };

        // Act
        var result = new CatalogResult(0, 10, 2, items);

        // Assert
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public void CatalogItem_ContainsRequiredProperties()
    {
        // Arrange & Act
        var item = new CatalogItem(
            1,
            "Test Product",
            "Test Description",
            99.99m,
            "http://example.com/image.jpg",
            1,
            new CatalogBrand(1, "TestBrand"),
            1,
            new CatalogItemType(1, "TestType")
        );

        // Assert
        Assert.Equal(1, item.Id);
        Assert.Equal("Test Product", item.Name);
        Assert.Equal("Test Description", item.Description);
        Assert.Equal(99.99m, item.Price);
        Assert.Equal("http://example.com/image.jpg", item.PictureUrl);
        Assert.Equal("TestBrand", item.CatalogBrand.Brand);
    }

    [Fact]
    public void CatalogBrand_CanBeCreatedWithIdAndName()
    {
        // Act
        var brand = new CatalogBrand(5, "Nike");

        // Assert
        Assert.Equal(5, brand.Id);
        Assert.Equal("Nike", brand.Brand);
    }

    [Fact]
    public void CatalogItemType_CanBeCreatedWithIdAndType()
    {
        // Act
        var type = new CatalogItemType(3, "Electronics");

        // Assert
        Assert.Equal(3, type.Id);
        Assert.Equal("Electronics", type.Type);
    }

    [Theory]
    [InlineData(0, 10, 5)]
    [InlineData(1, 20, 15)]
    [InlineData(2, 50, 0)]
    public void CatalogResult_SupportsPaginationValues(int pageIndex, int pageSize, int count)
    {
        // Act
        var result = new CatalogResult(pageIndex, pageSize, count, new List<CatalogItem>());

        // Assert
        Assert.Equal(pageIndex, result.PageIndex);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(count, result.Count);
    }
}

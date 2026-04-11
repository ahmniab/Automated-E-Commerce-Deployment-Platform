using eShop.Catalog.API.Model;

namespace Catalog.API.Tests;

public class PaginatedItemsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var data = new[] { new CatalogItem("A"), new CatalogItem("B") };

        var page = new PaginatedItems<CatalogItem>(pageIndex: 2, pageSize: 10, count: 50, data: data);

        Assert.Equal(2, page.PageIndex);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(50, page.Count);
        Assert.Equal(2, page.Data.Count());
    }

    [Fact]
    public void Data_PreservesItemOrder()
    {
        var items = new[] { new CatalogItem("First"), new CatalogItem("Second"), new CatalogItem("Third") };

        var page = new PaginatedItems<CatalogItem>(0, 10, 3, items);

        var names = page.Data.Select(i => i.Name).ToList();
        Assert.Equal(["First", "Second", "Third"], names);
    }

    [Fact]
    public void EmptyData_ReturnsZeroItems()
    {
        var page = new PaginatedItems<CatalogItem>(0, 10, 0, Enumerable.Empty<CatalogItem>());

        Assert.Empty(page.Data);
        Assert.Equal(0, page.Count);
    }
}

using eShop.Catalog.API;

namespace Catalog.API.Tests;

public class CatalogApiTests
{
    [Fact]
    public void GetFullPath_CombinesRootAndPicsFolder()
    {
        var result = CatalogApi.GetFullPath("/app", "item-1.webp");

        Assert.Equal(Path.Combine("/app", "Pics", "item-1.webp"), result);
    }

    [Fact]
    public void GetFullPath_HandlesSubdirectoryInFileName()
    {
        var result = CatalogApi.GetFullPath("/srv/catalog", "products/shoe.png");

        Assert.Equal(Path.Combine("/srv/catalog", "Pics", "products/shoe.png"), result);
    }

    [Fact]
    public void GetFullPath_WithEmptyFileName_StillReturnsValidPath()
    {
        var result = CatalogApi.GetFullPath("/app", "");

        Assert.Equal(Path.Combine("/app", "Pics", ""), result);
    }
}

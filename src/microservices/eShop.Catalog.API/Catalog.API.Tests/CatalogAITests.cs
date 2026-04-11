using eShop.Catalog.API.Model;
using eShop.Catalog.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Catalog.API.Tests;

public class CatalogAITests
{
    private static CatalogAI CreateDisabledAI()
    {
        var env = Substitute.For<IWebHostEnvironment>();
        var logger = NullLogger<CatalogAI>.Instance;
        return new CatalogAI(env, logger, embeddingGenerator: null);
    }

    [Fact]
    public void IsEnabled_WhenNoEmbeddingGenerator_ReturnsFalse()
    {
        var ai = CreateDisabledAI();

        Assert.False(ai.IsEnabled);
    }

    [Fact]
    public async Task GetEmbeddingAsync_ForText_WhenDisabled_ReturnsNull()
    {
        var ai = CreateDisabledAI();

        var result = await ai.GetEmbeddingAsync("some search text");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEmbeddingAsync_ForItem_WhenDisabled_ReturnsNull()
    {
        var ai = CreateDisabledAI();
        var item = new CatalogItem("Test") { Description = "A test item" };

        var result = await ai.GetEmbeddingAsync(item);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEmbeddingsAsync_WhenDisabled_ReturnsNull()
    {
        var ai = CreateDisabledAI();
        var items = new[]
        {
            new CatalogItem("Item1") { Description = "First" },
            new CatalogItem("Item2") { Description = "Second" }
        };

        var result = await ai.GetEmbeddingsAsync(items);

        Assert.Null(result);
    }
}

using eShop.Basket.API.Grpc;
using Grpc.Core;
using Microsoft.AspNetCore.Components.Authorization;
using GrpcBasketItem = eShop.Basket.API.Grpc.BasketItem;
using GrpcBasketClient = eShop.Basket.API.Grpc.Basket.BasketClient;

namespace eShop.WebApp.Services;

public class BasketService(GrpcBasketClient basketClient, AuthenticationStateProvider authenticationStateProvider)
{
    public async Task<IReadOnlyCollection<BasketQuantity>> GetBasketAsync()
    {
        var result = await basketClient.GetBasketAsync(new(), headers: await CreateUserHeadersAsync());
        return MapToBasket(result);
    }

    public async Task DeleteBasketAsync()
    {
        await basketClient.DeleteBasketAsync(new DeleteBasketRequest(), headers: await CreateUserHeadersAsync());
    }

    public async Task UpdateBasketAsync(IReadOnlyCollection<BasketQuantity> basket)
    {
        var updatePayload = new UpdateBasketRequest();

        foreach (var item in basket)
        {
            var updateItem = new GrpcBasketItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            };
            updatePayload.Items.Add(updateItem);
        }

        await basketClient.UpdateBasketAsync(updatePayload, headers: await CreateUserHeadersAsync());
    }

    private async Task<Metadata?> CreateUserHeadersAsync()
    {
        var buyerId = await authenticationStateProvider.GetBuyerIdAsync();
        if (string.IsNullOrWhiteSpace(buyerId))
        {
            return null;
        }

        return new Metadata { { "x-user-id", buyerId } };
    }

    private static List<BasketQuantity> MapToBasket(CustomerBasketResponse response)
    {
        var result = new List<BasketQuantity>();
        foreach (var item in response.Items)
        {
            result.Add(new BasketQuantity(item.ProductId, item.Quantity));
        }

        return result;
    }
}

public record BasketQuantity(int ProductId, int Quantity);

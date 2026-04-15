#nullable enable

using System;
using System.Linq;

namespace eShop.Basket.API.Extensions;

internal static class ServerCallContextIdentityExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context)
    {
        var userId = context.GetHttpContext().User.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            return userId;
        }

        return context.RequestHeaders.GetValue("x-user-id");
    }

    public static string? GetUserName(this ServerCallContext context) => context.GetHttpContext().User.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;

    private static string? GetValue(this Metadata metadata, string key)
        => metadata.FirstOrDefault(header => string.Equals(header.Key, key, StringComparison.OrdinalIgnoreCase))?.Value;
}

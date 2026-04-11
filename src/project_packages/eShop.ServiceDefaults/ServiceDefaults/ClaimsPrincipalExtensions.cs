using System.Security.Claims;

namespace eShop.ServiceDefaults;

/// <summary>
/// Helper methods for extracting common user claims.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user identifier from the <c>sub</c> claim.
    /// </summary>
    /// <param name="principal">The user principal.</param>
    /// <returns>The user identifier, or <see langword="null"/> if unavailable.</returns>
    public static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirst("sub")?.Value;

    /// <summary>
    /// Gets the user name from the <see cref="ClaimTypes.Name"/> claim.
    /// </summary>
    /// <param name="principal">The user principal.</param>
    /// <returns>The user name, or <see langword="null"/> if unavailable.</returns>
    public static string? GetUserName(this ClaimsPrincipal principal) =>
        principal.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
}

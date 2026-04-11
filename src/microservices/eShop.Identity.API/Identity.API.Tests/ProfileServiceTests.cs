using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using eShop.Identity.API.Services;

namespace eShop.Identity.API.Tests;

public class ProfileServiceTests
{
    [Fact]
    public async Task GetProfileDataAsync_populates_issued_claims_for_valid_subject()
    {
        using var fixture = new IdentityTestFixture();
        var user = IdentityTestFixture.CreateTestUser("profile-user-1", "profile1", "profile1@test.local");
        var create = await fixture.UserManager.CreateAsync(user, "Pass123$");
        Assert.True(create.Succeeded);

        var sut = new ProfileService(fixture.UserManager);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", user.Id) }));
        var context = new ProfileDataRequestContext { Subject = principal };

        await sut.GetProfileDataAsync(context);

        Assert.NotNull(context.IssuedClaims);
        var types = context.IssuedClaims!.Select(c => c.Type).ToHashSet();
        Assert.Contains(JwtClaimTypes.Subject, types);
        Assert.Contains(JwtClaimTypes.PreferredUserName, types);
        Assert.Contains("name", types);
    }

    [Fact]
    public async Task GetProfileDataAsync_throws_when_user_not_found()
    {
        using var fixture = new IdentityTestFixture();
        var sut = new ProfileService(fixture.UserManager);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "nonexistent-id") }));
        var context = new ProfileDataRequestContext { Subject = principal };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.GetProfileDataAsync(context));
        Assert.Contains("Invalid subject", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task IsActiveAsync_sets_active_for_unlocked_user_without_stamp_mismatch()
    {
        using var fixture = new IdentityTestFixture();
        var user = IdentityTestFixture.CreateTestUser("active-user-1", "active1", "active1@test.local");
        Assert.True((await fixture.UserManager.CreateAsync(user, "Pass123$")).Succeeded);

        var sut = new ProfileService(fixture.UserManager);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", user.Id) }));
        var client = new Client { ClientId = "test-client" };
        var context = new IsActiveContext(principal, client, caller: "test");

        await sut.IsActiveAsync(context);

        Assert.True(context.IsActive);
    }
}

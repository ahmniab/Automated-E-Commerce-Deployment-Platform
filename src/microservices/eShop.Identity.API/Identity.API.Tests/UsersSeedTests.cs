using eShop.Identity.API.Data;
using Microsoft.Extensions.Logging.Abstractions;

namespace eShop.Identity.API.Tests;

public class UsersSeedTests
{
    [Fact]
    public async Task SeedAsync_creates_alice_and_bob_when_absent()
    {
        using var fixture = new IdentityTestFixture();
        var context = fixture.Scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seed = new UsersSeed(NullLogger<UsersSeed>.Instance, fixture.UserManager);

        await seed.SeedAsync(context);

        var alice = await fixture.UserManager.FindByNameAsync("alice");
        var bob = await fixture.UserManager.FindByNameAsync("bob");
        Assert.NotNull(alice);
        Assert.NotNull(bob);
        Assert.Equal("AliceSmith@email.com", alice!.Email);
        Assert.Equal("BobSmith@email.com", bob!.Email);
    }
}

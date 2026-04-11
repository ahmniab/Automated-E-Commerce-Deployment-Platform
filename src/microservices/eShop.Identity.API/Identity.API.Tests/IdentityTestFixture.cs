using eShop.Identity.API.Data;
using eShop.Identity.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace eShop.Identity.API.Tests;

/// <summary>
/// Builds in-memory Identity + EF Core for testing services that depend on UserManager / SignInManager.
/// </summary>
public sealed class IdentityTestFixture : IDisposable
{
    public IdentityTestFixture()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase($"identity-tests-{Guid.NewGuid():N}"));
        services
            .AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        Provider = services.BuildServiceProvider();
        Scope = Provider.CreateScope();
        UserManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        SignInManager = Scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
    }

    public ServiceProvider Provider { get; }
    public IServiceScope Scope { get; }
    public UserManager<ApplicationUser> UserManager { get; }
    public SignInManager<ApplicationUser> SignInManager { get; }

    public void Dispose()
    {
        Scope.Dispose();
        Provider.Dispose();
    }

    public static ApplicationUser CreateTestUser(string id, string userName, string email)
    {
        return new ApplicationUser
        {
            Id = id,
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            CardNumber = "XXXXXXXXXXXX1881",
            SecurityNumber = "123",
            Expiration = "12/26",
            CardHolderName = "Test User",
            CardType = 1,
            Street = "1 Test St",
            City = "Test City",
            State = "TS",
            Country = "US",
            ZipCode = "12345",
            Name = "Test",
            LastName = "User"
        };
    }
}

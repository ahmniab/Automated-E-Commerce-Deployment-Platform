using eShop.Identity.API.Services;

namespace eShop.Identity.API.Tests;

public class EFLoginServiceTests
{
    [Fact]
    public async Task FindByUsername_resolves_user_by_email()
    {
        using var fixture = new IdentityTestFixture();
        var user = IdentityTestFixture.CreateTestUser("login-user-1", "logintest", "findme@test.local");
        Assert.True((await fixture.UserManager.CreateAsync(user, "Pass123$")).Succeeded);

        var sut = new EFLoginService(fixture.UserManager, fixture.SignInManager);

        var found = await sut.FindByUsername("findme@test.local");

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task ValidateCredentials_returns_true_for_correct_password()
    {
        using var fixture = new IdentityTestFixture();
        var user = IdentityTestFixture.CreateTestUser("login-user-2", "pwdtest", "pwd@test.local");
        const string password = "Pass123$";
        Assert.True((await fixture.UserManager.CreateAsync(user, password)).Succeeded);

        var sut = new EFLoginService(fixture.UserManager, fixture.SignInManager);

        var valid = await sut.ValidateCredentials(user, password);

        Assert.True(valid);
    }
}

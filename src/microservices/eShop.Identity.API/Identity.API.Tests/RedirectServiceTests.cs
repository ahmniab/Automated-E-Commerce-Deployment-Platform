using eShop.Identity.API.Services;

namespace eShop.Identity.API.Tests;

public class RedirectServiceTests
{
    private readonly RedirectService _sut = new();

    [Fact]
    public void ExtractRedirectUriFromReturnUrl_parses_oidc_style_return_url()
    {
        var encoded =
            "/connect/authorize/callback?client_id=x&redirect_uri=https%3A%2F%2Fapp.test%2Fsignin-oidc&scope=openid";

        var result = _sut.ExtractRedirectUriFromReturnUrl(encoded);

        Assert.Equal("https://app.test/", result);
    }

    [Fact]
    public void ExtractRedirectUriFromReturnUrl_returns_empty_when_redirect_uri_missing()
    {
        var result = _sut.ExtractRedirectUriFromReturnUrl("/no-redirect-here");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ExtractRedirectUriFromReturnUrl_uses_scope_split_when_not_signin_oidc()
    {
        var url = "path?redirect_uri=https%3A%2F%2Fapi.test%2Fcallback&scope=basket";

        var result = _sut.ExtractRedirectUriFromReturnUrl(url);

        Assert.Equal("https://api.test/callback", result);
    }
}

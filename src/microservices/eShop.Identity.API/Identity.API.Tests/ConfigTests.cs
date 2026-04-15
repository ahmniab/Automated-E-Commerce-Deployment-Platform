using Duende.IdentityServer;
using eShop.Identity.API.Configuration;
using Microsoft.Extensions.Configuration;

namespace eShop.Identity.API.Tests;

public class ConfigTests
{
    private static IConfiguration CreateTestConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MauiCallback"] = "https://maui.test/callback",
                ["WebAppClient"] = "https://webapp.test",
                ["WebhooksWebClient"] = "https://webhooks-web.test",
                ["BasketApiClient"] = "https://basket-api.test",
                ["OrderingApiClient"] = "https://ordering-api.test",
                ["WebhooksApiClient"] = "https://webhooks-api.test"
            })
            .Build();

    [Fact]
    public void GetApis_returns_expected_api_resources()
    {
        var apis = Config.GetApis().ToList();

        Assert.Equal(3, apis.Count);
        Assert.Contains(apis, a => a.Name == "orders" && a.DisplayName == "Orders Service");
        Assert.Contains(apis, a => a.Name == "basket" && a.DisplayName == "Basket Service");
        Assert.Contains(apis, a => a.Name == "webhooks" && a.DisplayName == "Webhooks registration Service");
    }

    [Fact]
    public void GetApiScopes_returns_matching_scopes()
    {
        var scopes = Config.GetApiScopes().ToList();

        Assert.Equal(3, scopes.Count);
        var names = scopes.Select(s => s.Name).OrderBy(n => n).ToArray();
        Assert.Equal(new[] { "basket", "orders", "webhooks" }, names);
    }

    [Fact]
    public void GetResources_includes_openid_and_profile()
    {
        var resources = Config.GetResources().ToList();

        Assert.Contains(resources, r => r.Name == IdentityServerConstants.StandardScopes.OpenId);
        Assert.Contains(resources, r => r.Name == IdentityServerConstants.StandardScopes.Profile);
    }

    [Fact]
    public void GetClients_registers_expected_clients_with_configuration()
    {
        var config = CreateTestConfiguration();
        var clients = Config.GetClients(config).ToList();

        Assert.Equal(6, clients.Count);
        Assert.Contains(clients, c => c.ClientId == "maui");
        Assert.Contains(clients, c => c.ClientId == "webapp");
        Assert.Contains(clients, c => c.ClientId == "webhooksclient");
        Assert.Contains(clients, c => c.ClientId == "basketswaggerui");
        Assert.Contains(clients, c => c.ClientId == "orderingswaggerui");
        Assert.Contains(clients, c => c.ClientId == "webhooksswaggerui");

        var maui = clients.Single(c => c.ClientId == "maui");
        Assert.Contains("https://maui.test/callback", maui.RedirectUris);
    }

    [Fact]
    public void GetClients_webapp_redirect_uris_include_localhost_and_loopback_aliases()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MauiCallback"] = "https://maui.test/callback",
                ["WebAppClient"] = "http://127.0.0.1:8080",
                ["WebhooksWebClient"] = "https://webhooks-web.test",
                ["BasketApiClient"] = "https://basket-api.test",
                ["OrderingApiClient"] = "https://ordering-api.test",
                ["WebhooksApiClient"] = "https://webhooks-api.test"
            })
            .Build();

        var webappClient = Config.GetClients(config).Single(c => c.ClientId == "webapp");

        Assert.Contains("http://127.0.0.1:8080/signin-oidc", webappClient.RedirectUris);
        Assert.Contains("http://localhost:8080/signin-oidc", webappClient.RedirectUris);
        Assert.Contains("http://127.0.0.1:8080/signout-callback-oidc", webappClient.PostLogoutRedirectUris);
        Assert.Contains("http://localhost:8080/signout-callback-oidc", webappClient.PostLogoutRedirectUris);
    }
}

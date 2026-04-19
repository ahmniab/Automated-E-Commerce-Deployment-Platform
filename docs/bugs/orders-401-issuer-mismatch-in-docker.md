# Orders page 401 in Docker — Identity Server `IssuerUri` does not match Ordering API JwtBearer `ValidIssuers`

## Status

- **Observed environment:** All services running under `docker-compose.yml` (conf/`.env`).
- **Not reproduced:** Running services locally on the host machine.
- **User-facing symptom (WebApp `/user/orders`):**
  `Your session is no longer authorized to view orders. Sign out and sign in again.`
- **Network symptom (WebApp container logs):**
  ```
  HttpClient.OrderingService.ClientHandler - Received HTTP response headers after 194.4929ms - 401
  Polly - Execution attempt ... Result: '401', Handled: 'False', Attempt: '0'
  HttpClient.OrderingService.LogicalHandler - End processing HTTP request after 195.1611ms - 401
  ```
- **Authoritative symptom (Ordering API container logs):**
  ```
  Microsoft.IdentityModel.Tokens.SecurityTokenInvalidIssuerException: IDX10205:
  Issuer validation failed. Issuer: 'identity-api'.
  Did not match: validationParameters.ValidIssuer: 'null'
  or validationParameters.ValidIssuers: 'http://identity-api:5223, , /.well-known/openid-configuration'
  or validationParameters.ConfigurationManager.CurrentConfiguration.Issuer: 'Null'.
  ```

The 401 is therefore not a token lifetime, scope, audience, or signing-key problem. It is strictly an **issuer validation failure** on the Ordering API side.

## TL;DR

- Identity API hard-codes `IdentityServerOptions.IssuerUri = "identity-api"` (a bare string, not a URL).
- Tokens issued to the WebApp carry `iss = "identity-api"`.
- Ordering API builds its JwtBearer `ValidIssuers` from `Identity:Url` (which is `http://identity-api:5223` in Docker), NOT from the literal string `identity-api`.
- OIDC discovery metadata (which would otherwise publish `"identity-api"` as the accepted issuer through `ConfigurationManager`) is not loaded (`ConfigurationManager.CurrentConfiguration.Issuer: 'Null'`), so the handler falls back to the static `ValidIssuers` list.
- The token's `iss` does not appear in any accepted issuer → `IDX10205` → Ordering API returns `401 Unauthorized` → WebApp surfaces its "not authorized to view orders" UI message.

## Components involved

- `src/microservices/eShop.Identity.API/Identity.API/Program.cs` — Identity Server bootstrap (token issuance).
- `src/microservices/eShop.Ordering.API/Ordering.API/Extensions/Extensions.cs` — calls `builder.AddDefaultAuthentication()`.
- `src/project_packages/eShop.ServiceDefaults/ServiceDefaults/artifacts/eShop.ServiceDefaults.Depi.1.0.0-preview1.nupkg` — precompiled package that contains `AddDefaultAuthentication()` (JwtBearer wiring). Source for this method is not in the repo; only the `.nupkg` is shipped.
- `src/microservices/eShop.Ordering.API/Ordering.API/Program.cs` — `orders.MapOrdersApiV1().RequireAuthorization();`.
- `src/microservices/eShop.WebApp/WebApp/Extensions/Extensions.cs` — OIDC setup plus typed `OrderingService` HttpClient with `AddAuthToken()`.
- `src/project_packages/eShop.ServiceDefaults/ServiceDefaults/HttpClientExtensions.cs` — `HttpClientAuthorizationDelegatingHandler` forwards the current cookie `access_token` as `Authorization: Bearer …`.
- `docker-compose.yml` — the environment variables that populate `Identity:Url`, `Identity:MetadataAddress`, etc.

## End-to-end request flow for `/user/orders`

1. User signs in through WebApp. WebApp's OIDC handler performs the code flow against Identity API and stores the resulting `access_token` in the auth cookie (`options.SaveTokens = true`).
2. User navigates to `/user/orders` in WebApp. A Blazor/Razor page asks `OrderingService` for the current user's orders.
3. `OrderingService` is a typed `HttpClient` pointed at `OrderServiceUrl = http://ordering-api:5224`. It has `AddAuthToken()` installed, so the `HttpClientAuthorizationDelegatingHandler` reads `access_token` from the current `HttpContext` and sets:
   ```
   Authorization: Bearer <token>
   ```
4. Ordering API receives the request at `/api/Orders/…`. The endpoint has `RequireAuthorization()`. The default scheme is JwtBearer (registered by `AddDefaultAuthentication()`).
5. JwtBearer handler:
   - Parses the token.
   - Attempts to validate issuer against `TokenValidationParameters.ValidIssuer(s)` and/or `ConfigurationManager.CurrentConfiguration.Issuer`.
   - Token `iss` = `"identity-api"`.
   - Accepted issuers do not include `"identity-api"`.
   - `OnAuthenticationFailed` fires → `SecurityTokenInvalidIssuerException (IDX10205)`.
   - `OnChallenge` writes `401` (`AuthenticationScheme: Bearer was challenged`).
6. WebApp `OrderingService` sees `401`, and the orders page renders the not-authorized message.

## Root cause

### 1. Identity API issues tokens with a non-URL issuer

`src/microservices/eShop.Identity.API/Identity.API/Program.cs`:

```20:32:src/microservices/eShop.Identity.API/Identity.API/Program.cs
builder.Services.AddIdentityServer(options =>
{
    options.IssuerUri = "identity-api";
    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);

    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    // TODO: Remove this line in production.
    options.KeyManagement.Enabled = false;
})
```

Effect of setting `IssuerUri`:

- Duende / IdentityServer puts this literal string in the `iss` claim of every issued token.
- It also publishes the same string in the `issuer` field of the `.well-known/openid-configuration` discovery document.
- Because the value is `"identity-api"` (not `http://identity-api:5223`), the `iss` claim is a bare hostname with no scheme and no port.

There is no reason to pin `IssuerUri` at all: IdentityServer defaults to the request scheme + host + port that the client used to reach it, which is exactly what downstream JwtBearer handlers expect.

### 2. Ordering API's JwtBearer pipeline does not accept that value

Ordering API wires up auth through the ServiceDefaults package:

```1:11:src/microservices/eShop.Ordering.API/Ordering.API/Extensions/Extensions.cs
using FluentValidation;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add the authentication services to DI
        builder.AddDefaultAuthentication();
```

`AddDefaultAuthentication()` is **not** in-source in this repo. It ships as a precompiled package `src/project_packages/eShop.ServiceDefaults/ServiceDefaults/artifacts/eShop.ServiceDefaults.Depi.1.0.0-preview1.nupkg` (its README says "Adds JWT bearer authentication and authorization when `Identity` configuration exists"). Introspecting the assembly shows it uses:

- `AddJwtBearer`
- `set_Authority`
- `set_ValidIssuers`
- `set_ValidateAudience` / `set_Audience`
- `set_RequireHttpsMetadata`

with values derived from the `Identity` configuration section. For Ordering API that section is populated in Docker from `docker-compose.yml`:

```176:189:docker-compose.yml
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:5224
      ConnectionStrings__orderingdb: Host=postgres;Port=5432;Database=orderingdb;Username=eshop;Password=${postgres_password}
      ConnectionStrings__EventBus: amqp://eshop:${rabbitmq_password}@rabbitmq:5672
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Port: 5672
      RabbitMQ__UserName: eshop
      RabbitMQ__Password: ${rabbitmq_password}
      RabbitMQ__VirtualHost: /
      Identity__Url: http://identity-api:5223
      Identity__Audience: orders
      Logging__LogLevel__Microsoft.AspNetCore.Authentication: Debug
      Logging__LogLevel__Microsoft.IdentityModel: Debug
```

The runtime diagnostics in terminal `1.txt` show exactly what the handler ends up with:

```
Issuer: 'identity-api'.
Did not match:
  validationParameters.ValidIssuer: 'null'
  or validationParameters.ValidIssuers: 'http://identity-api:5223, , /.well-known/openid-configuration'
  or validationParameters.ConfigurationManager.CurrentConfiguration.Issuer: 'Null'.
```

Three things to note:

- **Static ValidIssuers = `http://identity-api:5223`, `""`, `/.well-known/openid-configuration`.**
  The literal `"identity-api"` is not among them. (The empty string is `Identity:MetadataAddress` which is unset for Ordering API; the trailing `/.well-known/openid-configuration` is a relative path, so it cannot match a bare hostname either.)
- **`ConfigurationManager.CurrentConfiguration.Issuer = 'Null'`.**
  The JwtBearer handler never successfully pulled `.well-known/openid-configuration` from Identity. If it had, the handler would add the metadata's `issuer` value (which would have been `"identity-api"`, because of the Identity-side `IssuerUri`) to the accepted set, and validation would succeed.
  Why metadata is not loaded in Docker: the log in `1.txt` shows Ordering API initially crashed repeatedly with `The ConnectionString property has not been initialized` (the `depends_on.postgres` conditional fires before env vars / readiness are complete in this service's own startup path), then RabbitMQ was refused on `172.25.0.2:5672` during the next restart cycle, and finally it started. The first OIDC discovery attempt can fail during this race because `identity-api` is started with `condition: service_started`, not `service_healthy`, so Ordering API may issue its initial discovery GET before Identity is actually serving. `ConfigurationManager` caches failure state for a long TTL by default.
- **No mention of `"identity-api"` in the valid set.** That is the actual, decisive mismatch.

### 3. Ordering endpoints require auth

```21:24:src/microservices/eShop.Ordering.API/Ordering.API/Program.cs
var orders = app.NewVersionedApi("Orders");

orders.MapOrdersApiV1()
      .RequireAuthorization();
```

Because `RequireAuthorization()` is applied to the entire orders surface, rejection occurs in middleware before any orders query logic. That is why there is no Ordering-specific error and the 401 body is empty.

### 4. WebApp forwards the token it has

```47:50:src/microservices/eShop.WebApp/WebApp/Extensions/Extensions.cs
        builder.Services.AddHttpClient<OrderingService>(o => o.BaseAddress = new(orderingApiName))
            .AddServiceDiscovery()
            .AddApiVersion(1.0)
            .AddAuthToken();
```

```44:57:src/project_packages/eShop.ServiceDefaults/ServiceDefaults/HttpClientExtensions.cs
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                var accessToken = await context.GetTokenAsync("access_token");

                if (accessToken is not null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
```

The WebApp has the token that Identity issued. It is attaching that token correctly. There is nothing wrong on the WebApp side of the wire. The 401 is generated by the JwtBearer middleware inside Ordering API because it refuses the issuer on the inbound token.

## Why it works locally but fails in Docker

- **Token `iss` is always `"identity-api"`**, regardless of environment, because `IssuerUri` is hard-coded in `Program.cs`.
- **Local (on host machine):**
  - Ordering API uses `appsettings.Development.json` which overrides `Identity:Url = http://localhost:5223`.
  - Identity API is reachable at the same moment Ordering API starts (no orchestration race).
  - Ordering API's JwtBearer `ConfigurationManager` successfully fetches `http://localhost:5223/.well-known/openid-configuration`.
  - That metadata document publishes `"issuer": "identity-api"` (because of `IssuerUri`).
  - JwtBearer now has `ConfigurationManager.CurrentConfiguration.Issuer = "identity-api"`, which **does** match the token's `iss` claim, so validation passes and orders load.
- **Docker:**
  - Service start ordering and healthcheck conditions let Ordering API begin OIDC discovery before Identity is ready (see Ordering API crash loop in `1.txt` for unrelated reasons like DB / RabbitMQ timing — during those restarts, the OIDC discovery attempts contribute to `ConfigurationManager` failure caching).
  - When requests eventually flow, `ConfigurationManager.CurrentConfiguration` is still `Null`.
  - Without metadata, JwtBearer falls back to the static `ValidIssuers` list built from `Identity:Url`, which contains only URL-shaped values and will never match the bare string `"identity-api"`.
  - Every request from WebApp with a valid (but non-URL-issuer) token is rejected as `IDX10205`.

So it is not that the code "does different things" in Docker. It is that one code path (dynamic discovery) masks the misconfiguration on the host machine, and the other code path (static fallback) exposes it in Docker.

## Evidence checklist

| Evidence | Source |
| --- | --- |
| Hard-coded non-URL issuer | `src/microservices/eShop.Identity.API/Identity.API/Program.cs:22` |
| Ordering API uses shared auth defaults | `src/microservices/eShop.Ordering.API/Ordering.API/Extensions/Extensions.cs:10` |
| `AddDefaultAuthentication` is a packaged binary, no in-repo source | `src/project_packages/eShop.ServiceDefaults/ServiceDefaults/artifacts/eShop.ServiceDefaults.Depi.1.0.0-preview1.nupkg` |
| Orders endpoints require auth | `src/microservices/eShop.Ordering.API/Ordering.API/Program.cs:24` |
| WebApp forwards bearer token | `src/project_packages/eShop.ServiceDefaults/ServiceDefaults/HttpClientExtensions.cs:44-57` |
| Docker `Identity:Url` pointed at internal DNS | `docker-compose.yml:186-187` |
| Token `iss` = `"identity-api"`, mismatch against `ValidIssuers` | terminals/1.txt (JwtBearer `IDX10205` block, repeated) |
| Metadata manager never resolved (`CurrentConfiguration.Issuer: 'Null'`) | terminals/1.txt |
| WebApp sees the resulting 401 | WebApp container logs: `HttpClient.OrderingService.ClientHandler ... - 401` |

## Scope

This bug affects every protected Ordering API endpoint called by the WebApp on behalf of the user, including the orders list page. By the same mechanism, the Basket API (`Identity__Audience: basket`, `Identity__Url: http://identity-api:5223`) is vulnerable to the same issuer mismatch — it just has not been hit yet because the initial basket calls in the UI have not surfaced the failure as visibly. Any future service added with the same `AddDefaultAuthentication()` shortcut and protected endpoints will hit the same 401 in Docker.

## Out-of-scope observations (not the cause of this bug)

- Ordering API logs show initial startup crashes with `The ConnectionString property has not been initialized` and RabbitMQ refusal. These are **service-order** problems unrelated to the 401, but they aggravate the OIDC discovery failure by making Ordering API restart before identity-api is stable.
- `libgssapi_krb5.so.2` warnings from Npgsql are cosmetic in this image.
- Data Protection keys stored in container-local filesystem is noisy but unrelated.

## Falsification / confirmation tests

- **Confirm:** exec into the WebApp or Ordering API container, capture the outgoing/incoming bearer token, decode it, and verify `iss == "identity-api"`. Confirm it does not match any value in `ValidIssuers`.
- **Confirm:** `curl -s http://identity-api:5223/.well-known/openid-configuration | jq .issuer` from inside the `eshop-network` returns `"identity-api"`.
- **Confirm:** after restarting only Ordering API once Identity is healthy, Orders starts to work — because `ConfigurationManager` finally loads `issuer: "identity-api"` into the trusted set at boot. This is the exact asymmetry that explains "works on my machine."

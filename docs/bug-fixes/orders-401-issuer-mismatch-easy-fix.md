# Easiest fix — Orders page 401 in Docker (issuer mismatch)

Related bug report: `docs/bugs/orders-401-issuer-mismatch-in-docker.md`.

## Recommended fix (one-line change)

Remove the `IssuerUri` override in Identity API. Identity Server will then issue tokens whose `iss` claim matches the URL each service actually uses to reach it (`http://identity-api:5223` in Docker, `http://localhost:5223` on a host machine), which is exactly what the Ordering API / Basket API `JwtBearer` `ValidIssuers` are built from.

### File

`src/microservices/eShop.Identity.API/Identity.API/Program.cs`

### Change

From:

```csharp
builder.Services.AddIdentityServer(options =>
{
    options.IssuerUri = "identity-api";
    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
    ...
})
```

To:

```csharp
builder.Services.AddIdentityServer(options =>
{
    options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
    ...
})
```

That is the entire code change. One line deleted. No other file needs to be modified for the 401 to go away.

## Why this fix is sufficient

- Today `IssuerUri = "identity-api"` forces every JWT's `iss` claim to be the literal string `"identity-api"`.
- Ordering API's `AddDefaultAuthentication()` builds its `TokenValidationParameters.ValidIssuers` from `Identity:Url` (`http://identity-api:5223` in Docker). Bare string `"identity-api"` is never in that list.
- OIDC discovery would otherwise bridge this gap (by adding metadata's `issuer` to the valid set), but in the Docker startup race it may not be loaded at request time (`ConfigurationManager.CurrentConfiguration.Issuer: 'Null'` in the terminal log).
- Removing `IssuerUri` makes Identity Server fall back to the default behavior: the `iss` claim is built from the request's scheme + host + port.
  - WebApp calls Identity through `Identity__Url: http://localhost:5223` (browser-facing) for the interactive flow and through the back-channel `MetadataAddress: http://identity-api:5223` for token/metadata; the IDP will use whichever host the client reached it on for the token's `iss`.
  - Ordering API's statically-configured `ValidIssuers` list already contains `http://identity-api:5223`, which matches the token's `iss` when the original request path went through that authority.
  - Basket API uses the same shared defaults and the same `Identity__Url: http://identity-api:5223`, so it benefits from the same alignment.

## Ordered step-by-step

1. Open `src/microservices/eShop.Identity.API/Identity.API/Program.cs`.
2. Delete the single line `options.IssuerUri = "identity-api";` inside `AddIdentityServer(options => { ... })`.
3. Rebuild and restart the stack:
   ```
   docker compose --env-file conf/.env up -d --build identity-api ordering-api basket-api webapp
   ```
4. Sign out of the WebApp (or clear site cookies for `localhost:8080`) to force a fresh authorization code flow — existing tokens in the current session still have the old `iss` claim and will keep failing validation until a new token is obtained.
5. Sign back in and open `/user/orders`. The 401 should not appear.

## How to verify the fix

- From inside any service container on `eshop-network`, check discovery metadata:
  ```
  curl -s http://identity-api:5223/.well-known/openid-configuration | jq .issuer
  ```
  Expected after the fix: `"http://identity-api:5223"` (or similar URL), not `"identity-api"`.
- Ordering API logs should now show successful JwtBearer validation for requests from the WebApp (no more repeated `IDX10205` blocks).
- WebApp `OrderingService` logs show `200 OK` for `GET /api/Orders/` instead of `401`.
- `/user/orders` renders the orders list.

## Optional hardening (recommended but not required to stop the 401)

These items are **not** needed to close this bug, but they remove the fragility that made the mis-config invisible on the host and visible in Docker:

1. **Healthcheck-gate Identity API consumers.** Change `depends_on.identity-api` for `ordering-api`, `basket-api`, and `webapp` from `condition: service_started` to `condition: service_healthy`, and add a healthcheck to `identity-api` that probes `/.well-known/openid-configuration`. This guarantees the OIDC `ConfigurationManager` inside JwtBearer will succeed on first fetch.
2. **Point Ordering API at the same metadata address as WebApp.** Add to `ordering-api.environment`:
   ```yaml
   Identity__MetadataAddress: http://identity-api:5223
   ```
   so that JwtBearer explicitly pulls discovery from container DNS and the token's `iss` is locked to metadata rather than to the static `ValidIssuers` fallback.
3. **Remove the debug-only auth logging once validated.**
   ```yaml
   Logging__LogLevel__Microsoft.AspNetCore.Authentication: Debug
   Logging__LogLevel__Microsoft.IdentityModel: Debug
   ```
   These were used to surface `IDX10205`; they can stay during rollout but should not be permanent.

## Rollback plan

Re-add the single line `options.IssuerUri = "identity-api";` and rebuild Identity API. No data migration, no database change, no client-registration change is involved.

## Why not the alternatives

- **Adding `"identity-api"` to Ordering API's `ValidIssuers`:** would paper over the real problem and would need to be repeated for every future protected API. The canonical issuer should be issued by the identity provider, not patched in every consumer.
- **Forcing `MetadataAddress` without removing `IssuerUri`:** would mask the bug in Docker by relying on the discovery document's `issuer` field. Still brittle: any consumer that cannot reach `.well-known` at the exact right moment will fail again.
- **Rewriting `AddDefaultAuthentication` locally:** the method is shipped as a packaged binary (`eShop.ServiceDefaults.Depi.1.0.0-preview1.nupkg`); we would have to vendor and own it. Not needed for this bug.

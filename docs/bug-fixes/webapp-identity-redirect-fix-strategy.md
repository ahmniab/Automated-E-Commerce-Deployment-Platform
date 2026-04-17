# WebApp Login Redirect Host Fix Strategy

## Problem Summary

When logging in from WebApp, the browser is redirected to:

- `http://identity-api:5223/connect/authorize`

instead of:

- `http://localhost:5223/connect/authorize`

This breaks browser navigation because `identity-api` is an internal Docker DNS name, not a host the browser can resolve on the developer machine.

## Root Cause

The OpenID Connect configuration in WebApp sets different hosts for authority and metadata discovery.

Current behavior:

- `Identity:Url` is set to `http://localhost:5223`
- `Identity:MetadataAddress` is set to `http://identity-api:5223`

WebApp auth setup uses both:

- `options.Authority = Identity:Url`
- `options.MetadataAddress = Identity:MetadataAddress + "/.well-known/openid-configuration"`

OIDC middleware uses discovery metadata (`authorization_endpoint`) to build the redirect challenge URL. Because discovery is loaded from `identity-api`, the discovered authorization endpoint host becomes `identity-api`, so browser redirect uses the internal hostname.

## Fix Strategy

Use split endpoints intentionally, but keep browser-facing endpoints external.

### Strategy Choice

Apply a minimal and safe change in WebApp authentication options:

1. Keep internal backchannel metadata retrieval available for container networking.
2. Force browser-facing challenge endpoint to use external authority host.
3. Keep current identity client registration and callback URIs unchanged.

### Planned Implementation

In WebApp OIDC configuration:

- Keep `Authority` as `Identity:Url` (browser-facing base URL).
- Keep `MetadataAddress` for internal discovery.
- Add explicit endpoint override:
  - `options.Events.OnRedirectToIdentityProvider` to set `context.ProtocolMessage.IssuerAddress` to `${Authority}/connect/authorize`.

Why this works:

- Backchannel metadata/token/userinfo can still use internal service address (`identity-api`) for container-to-container communication.
- Browser redirect is guaranteed to use `localhost`, avoiding internal DNS leakage.

## Secondary Hardening (Optional, not in first patch)

If needed later, add explicit issuer/public origin in Identity API so discovery always emits external URLs for public endpoints.

Example direction:

- Configure IdentityServer with explicit issuer origin from configuration.

This is optional in first pass because it can affect token issuer validation across services.

## Validation Plan

After patch:

1. Rebuild and run with Docker Compose.
2. Open WebApp and click login.
3. Confirm first redirect URL host is `localhost:5223`.
4. Confirm signin callback returns to WebApp successfully.
5. Confirm API calls still succeed for authenticated flows.
6. Check no regression in Basket/API JWT validation.

## Risk and Compatibility Notes

- Low-risk change: localized to WebApp challenge redirect behavior.
- No changes to token format, scopes, or client IDs.
- No change to ServiceDefaults JWT bearer behavior.

## Rollback Plan

If any regression appears:

- Revert only the WebApp OIDC event override.
- Keep existing compose and identity settings as-is.

## Planned File Changes (Detailed)

1. `src/microservices/eShop.WebApp/WebApp/Extensions/Extensions.cs`
   - Add OIDC redirect event override to force external authorize endpoint host.
2. `docker-compose.yml` (optional follow-up only if needed)
   - No required change for first fix.
   - May be adjusted later only if we decide to unify metadata and authority hosts.
3. `src/microservices/eShop.Identity.API/Identity.API/Program.cs` (optional hardening)
   - No required change for first fix.
   - Could add explicit issuer/public origin in a second phase if necessary.

## Short File List To Change

- `src/microservices/eShop.WebApp/WebApp/Extensions/Extensions.cs`

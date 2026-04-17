# WebApp Logout Redirect Host Fix Strategy

## Problem Summary

After the login redirect fix, login now correctly goes to:

- http://localhost:5223/connect/authorize

But logout still redirects the browser to:

- http://identity-api:5223/connect/endsession

instead of:

- http://localhost:5223/connect/endsession

This fails in the browser because identity-api is only resolvable inside Docker networking.

## Root Cause

WebApp OpenID Connect configuration currently uses split endpoints:

- Browser-facing authority: Identity:Url = http://localhost:5223
- Backchannel discovery: Identity:MetadataAddress = http://identity-api:5223

The login path was already hardened with:

- OnRedirectToIdentityProvider -> IssuerAddress forced to ${Authority}/connect/authorize

Logout does not have an equivalent override.

When SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme) runs, OIDC middleware reads end_session_endpoint from discovery metadata. Since metadata is loaded from identity-api, logout challenge uses identity-api host and leaks internal DNS to browser.

## Fix Strategy

Use the same split-endpoint approach, and explicitly force the browser-facing logout endpoint.

### Strategy Choice

Apply a minimal and low-risk change in WebApp OIDC events:

1. Keep MetadataAddress for container backchannel traffic.
2. Keep Authority as browser-facing localhost URL.
3. Add logout redirect override event so sign-out challenge always uses external host.

### Planned Implementation

In WebApp OIDC configuration (OpenIdConnectEvents):

1. Keep existing OnRedirectToIdentityProvider override (login).
2. Add OnRedirectToIdentityProviderForSignOut override:
   - Set context.ProtocolMessage.IssuerAddress = ${Authority}/connect/endsession
3. Keep SignedOutRedirectUri as current callback value.
4. Optionally set PostLogoutRedirectUri explicitly from current callback configuration if needed for consistency.

Reference target file:

- src/microservices/eShop.WebApp/WebApp/Extensions/Extensions.cs

## Why This Works

- Browser challenge/logout endpoints are forced to localhost and stay browser-reachable.
- Metadata/token/userinfo backchannel can still use identity-api internal host.
- No change to identity client id, scopes, token format, or existing callback contract.

## Validation Plan

After applying patch:

1. Rebuild and run docker compose.
2. Login from WebApp and verify login still redirects through localhost:5223.
3. Click logout and inspect first redirect URL.
4. Confirm logout endpoint host is localhost:5223 (not identity-api).
5. Confirm post-logout returns to WebApp callback URL.
6. Confirm a fresh protected-page visit triggers a normal login challenge.

## Risk and Compatibility Notes

- Low-risk and localized to WebApp OIDC event handling.
- No dependency on Identity API changes for first pass.
- No expected impact on API JWT bearer validation.

## Rollback Plan

If regression appears:

1. Revert only OnRedirectToIdentityProviderForSignOut override.
2. Keep login override as-is.
3. Re-test login/logout behavior.

## Optional Hardening (Second Phase)

If desired later, configure Identity API public origin/issuer so discovery itself emits browser-safe URLs. This is optional and should be done carefully because it may affect issuer validation across services.

## Short File List To Change

- src/microservices/eShop.WebApp/WebApp/Extensions/Extensions.cs

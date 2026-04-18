# WebApp Orders 401 in Docker: Issuer/Authority Split Hypothesis

## Hypothesis

The 401 on `/user/orders` is caused by an OpenID Connect issuer/authority mismatch between services in Docker.

In this environment, the WebApp and Ordering API do not use the same identity host naming model:

- WebApp is configured with `Identity__Url=http://localhost:5223` (browser-facing)
- WebApp metadata discovery is configured with `Identity__MetadataAddress=http://identity-api:5223` (container DNS)
- Ordering API validates JWTs using `Identity__Url=http://identity-api:5223`

Because IdentityServer issuer is not pinned, the `iss` claim in the token can differ depending on which endpoint/host generated metadata and token flow. When Ordering API validates the JWT, accepted issuers are derived from its own identity configuration, so a host mismatch can make an otherwise valid token fail issuer validation and return 401.

## Why This Fits the Observed Behavior

- The failure appears only in Docker, not local host-only runs.
- `/user/orders` fails with 401 from WebApp `OrderingService` client call.
- WebApp catches 401 and displays:
  `Your session is no longer authorized to view orders. Sign out and sign in again.`
- Ordering API endpoint is protected with `RequireAuthorization()`, so auth rejection happens before order query logic.

## Request Path (Current)

1. User opens `/user/orders` in WebApp.
2. WebApp calls Ordering API at `/api/Orders/`.
3. WebApp forwards the current `access_token` in `Authorization: Bearer ...`.
4. Ordering API validates token using JwtBearer with issuers derived from `Identity` config.
5. If token `iss` does not match accepted issuer values, auth middleware rejects with 401.

## How To Falsify This Hypothesis

The hypothesis is false if we can prove issuer consistency is already correct and 401 still occurs.

### Falsification Test A: Inspect Actual Token Issuer

Goal: verify whether `iss` in the forwarded access token matches Ordering API accepted issuer list.

Steps:

1. Capture one failing access token from WebApp request to Ordering API (development only).
2. Decode token payload and read `iss`, `aud`, `scope`, and `exp`.
3. Compare `iss` with Ordering API expected issuer values (derived from `Identity:Url` and metadata address).
4. Compare token scopes include `orders`.

Expected for hypothesis to hold:

- `iss` is `http://localhost:5223` or another host not accepted by Ordering API configured around `http://identity-api:5223`.

Evidence that falsifies hypothesis:

- `iss` exactly matches accepted issuer(s), scope contains `orders`, token not expired, and yet Ordering still returns 401.

### Falsification Test B: Single-Host Alignment Experiment

Goal: determine whether making identity host naming fully consistent removes the 401.

Steps:

1. Temporarily align WebApp and Ordering API to one identity host model.
2. Re-authenticate user to obtain a fresh token.
3. Retry `/user/orders`.

Expected for hypothesis to hold:

- 401 disappears when host/issuer model is unified.

Evidence that falsifies hypothesis:

- 401 persists even after full host alignment and fresh login.

### Falsification Test C: Auth Pipeline Diagnostics

Goal: identify actual JwtBearer failure reason.

Steps:

1. Enable authentication diagnostics for JwtBearer events (`OnAuthenticationFailed`, `OnTokenValidated`) in development.
2. Trigger `/user/orders`.
3. Read Ordering API logs.

Expected for hypothesis to hold:

- diagnostics show issuer validation failure (for example, invalid issuer).

Evidence that falsifies hypothesis:

- diagnostics indicate different root cause (missing token, expired token, signing key mismatch, or absent scope/policy requirement).

## Fix Design (If Hypothesis Is Confirmed)

### Design Principle

Use one canonical identity authority model for server-to-server validation and token issuance semantics, while keeping browser callback URLs valid for user-facing redirects.

### Proposed Changes

1. Pin IdentityServer issuer explicitly in Identity API so tokens have deterministic `iss`.
2. Stop mixing `localhost` and container DNS for authority/metadata inside service-to-service auth validation paths.
3. Keep WebApp user redirect endpoints browser-safe (`localhost:8080`) but ensure OIDC authority/metadata and downstream JWT validation align on the same issuer model.
4. Add targeted auth diagnostics in development to make future token failures immediately explainable.

### Validation After Fix

1. Rebuild and restart all containers.
2. Sign out and sign in to force new token issuance.
3. Open `/user/orders`.
4. Confirm:
   - no 401 in WebApp outgoing call
   - Ordering API logs show successful token validation
   - orders list loads

## Risks and Notes

- Changing issuer/authority settings can affect Swagger OAuth clients and any existing tokens.
- A clean re-authentication cycle is required after change.
- If issuer is fixed but 401 remains, investigate secondary hypothesis: missing forwarded token in specific Blazor execution context.

## Files To Change For The Fix

1. `src/microservices/eShop.Identity.API/Identity.API/Program.cs`
   - Pin `IssuerUri` and ensure identity issuer is deterministic.
2. `docker-compose.yml`
   - Align identity-related environment settings across WebApp and Ordering API to one authority model.
3. `src/microservices/eShop.WebApp/WebApp/Extensions/Extensions.cs`
   - Ensure OIDC authority/metadata construction is consistent and not producing mixed-host issuer behavior.
4. `src/project_packages/eShop.ServiceDefaults/ServiceDefaults/AuthenticationExtensions.cs`
   - Normalize JwtBearer issuer configuration to match canonical identity issuer rules.
5. `src/microservices/eShop.Ordering.API/Ordering.API/appsettings.json`
   - Keep identity section consistent with canonical issuer/authority values.
6. `src/microservices/eShop.Ordering.API/Ordering.API/appsettings.Development.json`
   - Match development identity settings to the same canonical model.
7. `src/microservices/eShop.WebApp/WebApp/appsettings.json`
   - Align identity URL/metadata settings with designed issuer model.
8. `src/microservices/eShop.WebApp/WebApp/appsettings.Development.json`
   - Match development identity settings to the same model.

Optional (development diagnostics):

9. `src/project_packages/eShop.ServiceDefaults/ServiceDefaults/AuthenticationExtensions.cs`
   - Add temporary JwtBearer event logging for failed validations.

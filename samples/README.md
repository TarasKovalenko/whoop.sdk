# Samples

Four runnable projects, smallest first. All are in `Whoop.Sdk.slnx`, none are packable.

| Sample | Shows |
| --- | --- |
| [`Whoop.Sdk.Samples.QuickStart`](Whoop.Sdk.Samples.QuickStart) | One access token, no DI. Profile, today's cycle, 7 days of recovery, 30 days of workout totals. |
| [`Whoop.Sdk.Samples.Worker`](Whoop.Sdk.Samples.Worker) | Generic Host, `IWhoopClient` injected into a `BackgroundService`, static token *or* auto-refresh, resilience handler. |
| [`Whoop.Sdk.Samples.OAuthWebApp`](Whoop.Sdk.Samples.OAuthWebApp) | Full authorization-code flow, per-request scoped token provider, exception-to-HTTP mapping. |
| [`Whoop.Sdk.Samples.TrustedPartner`](Whoop.Sdk.Samples.TrustedPartner) | Client-credentials partner auth, reading a lab requisition and uploading results. |

## QuickStart

```bash
export WHOOP_ACCESS_TOKEN="..."
dotnet run --project samples/Whoop.Sdk.Samples.QuickStart
```

Tokens come from the [developer dashboard](https://developer-dashboard.whoop.com) or from the OAuth sample below. They expire after about an hour.

## Worker

```bash
dotnet user-secrets set "Whoop:AccessToken" "..." --project samples/Whoop.Sdk.Samples.Worker
dotnet run --project samples/Whoop.Sdk.Samples.Worker
```

To exercise the refreshing provider instead — which is what anything running longer than a token lifetime needs — set `Whoop:ClientId`, `Whoop:ClientSecret` and `Whoop:RefreshToken`. `RefreshTokenStore` stands in for real storage: WHOOP rotates the refresh token on every refresh and invalidates the previous one, so persist each new one or the worker loses access on restart.

## OAuth web app

```bash
dotnet user-secrets set "Whoop:ClientId" "..." --project samples/Whoop.Sdk.Samples.OAuthWebApp
dotnet user-secrets set "Whoop:ClientSecret" "..." --project samples/Whoop.Sdk.Samples.OAuthWebApp
dotnet run --project samples/Whoop.Sdk.Samples.OAuthWebApp
```

Register `http://localhost:5080/callback` as a redirect URI on your WHOOP app, then open <http://localhost:5080/login>.

| Route | Purpose |
| --- | --- |
| `/login` | Redirects to WHOOP for consent, with a CSRF `state`. |
| `/callback` | Validates `state`, exchanges the code, sets a session cookie. |
| `/me` | Profile, via the scoped token provider. |
| `/recovery` | Last 7 days, streamed with `EnumerateAsync`. |

The token provider is **scoped**, so each request resolves the signed-in visitor's own token and refreshes it on demand.

## Trusted partner

```bash
export WHOOP_PARTNER_CLIENT_ID="..."
export WHOOP_PARTNER_CLIENT_SECRET="..."
dotnet run --project samples/Whoop.Sdk.Samples.TrustedPartner -- <requisition-id>
```

Only accounts WHOOP has onboarded as lab partners can call these endpoints. Note that `AddWhoopPartnerAuthentication` fetches tokens over a separate, unauthenticated pipeline so acquiring one cannot recurse back through the auth handler.

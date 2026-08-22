// The full authorization-code flow, end to end, with the client resolved from DI per request.
//
//   dotnet user-secrets set "Whoop:ClientId" "..." --project samples/Whoop.Sdk.Samples.OAuthWebApp
//   dotnet user-secrets set "Whoop:ClientSecret" "..." --project samples/Whoop.Sdk.Samples.OAuthWebApp
//   dotnet run --project samples/Whoop.Sdk.Samples.OAuthWebApp
//
// Register http://localhost:5080/callback as a redirect URI on your WHOOP app, then open
// http://localhost:5080/login.

using Whoop.Sdk;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Extensions.DependencyInjection;
using Whoop.Sdk.Models;
using Whoop.Sdk.Samples.OAuthWebApp;

var builder = WebApplication.CreateBuilder(args);

var clientId = builder.Configuration["Whoop:ClientId"] ?? "";
var clientSecret = builder.Configuration["Whoop:ClientSecret"] ?? "";
var redirectUri = new Uri(builder.Configuration["Whoop:RedirectUri"] ?? "http://localhost:5080/callback");

// Fail at startup rather than with a 500 on the first /login.
if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
{
    Console.Error.WriteLine(
        "Set Whoop:ClientId and Whoop:ClientSecret (user secrets, environment, or appsettings.json).");
    return;
}

builder.Services.AddSingleton<TokenSession>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddWhoopOAuth(clientId, clientSecret);

// One token per signed-in visitor, so the provider is scoped and reads the current request's cookie.
builder.Services.AddScoped<IWhoopTokenProvider>(serviceProvider =>
{
    var context = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext
        ?? throw new InvalidOperationException("No active request.");

    var session = serviceProvider.GetRequiredService<TokenSession>();
    var token = session.Find(context.Request.Cookies["whoop_session"])
        ?? throw new InvalidOperationException("Not connected to WHOOP. Visit /login first.");

    var oauth = serviceProvider.GetRequiredService<WhoopOAuthClient>();
    var sessionId = context.Request.Cookies["whoop_session"]!;

    // Refreshes on demand and writes the rotated token back to the session store.
    return new RefreshingWhoopTokenProvider(
        oauth,
        token.RefreshToken ?? throw new InvalidOperationException("Request the offline scope to enable refresh."),
        token,
        onTokenRefreshed: (refreshed, _) =>
        {
            session.Store(sessionId, refreshed);
            return Task.CompletedTask;
        });
});

builder.Services.AddWhoopClient();

var app = builder.Build();

app.MapGet("/", () => Results.Content(
    """
    <h1>Whoop.Sdk OAuth sample</h1>
    <ul>
      <li><a href="/login">Connect a WHOOP account</a></li>
      <li><a href="/me">Profile</a></li>
      <li><a href="/recovery">Last 7 days of recovery</a></li>
    </ul>
    """,
    "text/html"));

// Step 1: send the user to WHOOP for consent.
app.MapGet("/login", (WhoopOAuthClient oauth, TokenSession session) =>
{
    var url = oauth.CreateAuthorizationUrl(
        redirectUri,
        new[]
        {
            WhoopScopes.ReadProfile,
            WhoopScopes.ReadRecovery,
            WhoopScopes.ReadSleep,
            WhoopScopes.ReadWorkout,
            WhoopScopes.ReadCycles,
            WhoopScopes.Offline, // required if you want a refresh token
        },
        session.CreateState());

    return Results.Redirect(url.AbsoluteUri);
});

// Step 2: WHOOP redirects back with a code; swap it for tokens.
app.MapGet("/callback", async (
    HttpContext context,
    WhoopOAuthClient oauth,
    TokenSession session,
    string? code,
    string? state,
    string? error) =>
{
    if (error is not null)
    {
        return Results.BadRequest($"WHOOP returned '{error}'.");
    }

    if (!session.ConsumeState(state))
    {
        return Results.BadRequest("Unknown or replayed state value.");
    }

    if (string.IsNullOrWhiteSpace(code))
    {
        return Results.BadRequest("No authorization code on the callback.");
    }

    var token = await oauth.ExchangeAuthorizationCodeAsync(code, redirectUri, context.RequestAborted);

    var sessionId = Guid.NewGuid().ToString("N");
    session.Store(sessionId, token);
    context.Response.Cookies.Append("whoop_session", sessionId, new CookieOptions
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
    });

    return Results.Redirect("/me");
});

app.MapGet("/me", async (IWhoopClient whoop, CancellationToken cancellationToken) =>
    Results.Ok(await whoop.User.GetBasicProfileAsync(cancellationToken)));

app.MapGet("/recovery", async (IWhoopClient whoop, CancellationToken cancellationToken) =>
{
    var request = new WhoopCollectionRequest
    {
        Start = DateTimeOffset.UtcNow.AddDays(-7),
        Limit = 25,
    };

    var scored = new List<object>();
    await foreach (var recovery in whoop.Recovery.EnumerateAsync(request, cancellationToken))
    {
        if (recovery.ScoreState == ScoreState.Scored)
        {
            scored.Add(new
            {
                date = recovery.CreatedAt,
                recovery = recovery.Score!.RecoveryScorePercentage,
                hrv = recovery.Score.HrvRmssdMilli,
                restingHeartRate = recovery.Score.RestingHeartRate,
            });
        }
    }

    return Results.Ok(scored);
});

// Turn the library's exceptions into sensible HTTP responses.
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var exception = context.Features
        .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    (var status, var message) = exception switch
    {
        WhoopRateLimitExceededException rateLimited =>
            (StatusCodes.Status429TooManyRequests, $"Rate limited. Retry after {rateLimited.RetryAfter}."),
        WhoopApiException { IsAuthenticationFailure: true } =>
            (StatusCodes.Status401Unauthorized, "WHOOP rejected the token. Visit /login again."),
        WhoopApiException api =>
            ((int)api.StatusCode, api.Message),
        InvalidOperationException invalid =>
            (StatusCodes.Status400BadRequest, invalid.Message),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error."),
    };

    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(new { error = message });
}));

app.Run();

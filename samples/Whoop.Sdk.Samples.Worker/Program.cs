// A long-running host that injects IWhoopClient into a BackgroundService.
//
//   dotnet user-secrets set "Whoop:AccessToken" "..." --project samples/Whoop.Sdk.Samples.Worker
//   dotnet run --project samples/Whoop.Sdk.Samples.Worker
//
// Set ClientId/ClientSecret/RefreshToken instead to exercise the refreshing provider, which is what
// anything running longer than a token lifetime should use.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Whoop.Sdk;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Extensions.DependencyInjection;
using Whoop.Sdk.Samples.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

var options = builder.Configuration.GetSection(WhoopOptions.SectionName).Get<WhoopOptions>() ?? new WhoopOptions();

if (options.CanRefresh)
{
    // Long-lived: mint access tokens from a rotating refresh token and persist each new one.
    builder.Services.AddSingleton(new RefreshTokenStore(options.RefreshToken!));
    builder.Services.AddWhoopOAuth(options.ClientId!, options.ClientSecret!);

    builder.Services.AddSingleton<IWhoopTokenProvider>(serviceProvider =>
    {
        var store = serviceProvider.GetRequiredService<RefreshTokenStore>();

        return new RefreshingWhoopTokenProvider(
            serviceProvider.GetRequiredService<WhoopOAuthClient>(),
            store.Current,
            onTokenRefreshed: (token, cancellationToken) =>
                store.SaveAsync(token.RefreshToken!, cancellationToken));
    });
}
else if (!string.IsNullOrWhiteSpace(options.AccessToken))
{
    // Short-lived: fine for a script or a one-shot job.
    builder.Services.AddWhoopAccessToken(options.AccessToken);
}
else
{
    Console.Error.WriteLine("Configure Whoop:AccessToken, or Whoop:ClientId + ClientSecret + RefreshToken.");
    return 1;
}

builder.Services
    .AddWhoopClient(whoop => whoop.UserAgent = "Whoop.Sdk.Samples.Worker/1.0")
    .AddStandardResilienceHandler();

builder.Services.AddHostedService<DailySummaryService>();

await builder.Build().RunAsync();
return 0;

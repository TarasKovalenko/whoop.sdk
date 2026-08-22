namespace Whoop.Sdk.Samples.Worker;

/// <summary>Credentials bound from the <c>Whoop</c> configuration section.</summary>
public sealed class WhoopOptions
{
    public const string SectionName = "Whoop";

    /// <summary>A ready-made access token. Simplest option, but expires after about an hour.</summary>
    public string? AccessToken { get; set; }

    /// <summary>OAuth client id, required for the refreshing token provider.</summary>
    public string? ClientId { get; set; }

    /// <summary>OAuth client secret, required for the refreshing token provider.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>A refresh token minted with the <c>offline</c> scope.</summary>
    public string? RefreshToken { get; set; }

    /// <summary>True when the refresh flow can be used, which is what a long-running worker wants.</summary>
    public bool CanRefresh =>
        !string.IsNullOrWhiteSpace(ClientId) &&
        !string.IsNullOrWhiteSpace(ClientSecret) &&
        !string.IsNullOrWhiteSpace(RefreshToken);
}

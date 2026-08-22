using System;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Authentication
{
    /// <summary>A token set returned by the WHOOP OAuth token endpoint.</summary>
    public sealed record WhoopOAuthToken
    {
        /// <summary>The bearer token to send on API requests.</summary>
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        /// <summary>The token type, normally <c>Bearer</c>.</summary>
        [JsonPropertyName("token_type")]
        public string? TokenType { get; init; }

        /// <summary>Lifetime of <see cref="AccessToken"/>, in seconds.</summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        /// <summary>
        /// Long-lived token used to obtain a new access token. Only returned when the
        /// <see cref="WhoopScopes.Offline"/> scope was requested.
        /// </summary>
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        /// <summary>Space-separated list of scopes the token was actually granted.</summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; init; }

        /// <summary>
        /// When this token set was received. Set by the library on receipt; it is not part of the wire format.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset ObtainedAt { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>The instant at which <see cref="AccessToken"/> stops being valid.</summary>
        [JsonIgnore]
        public DateTimeOffset ExpiresAt => ObtainedAt.AddSeconds(ExpiresIn);

        /// <summary>
        /// Whether the token is expired, or close enough to expiry that it should be refreshed now.
        /// </summary>
        /// <param name="clockSkew">How far ahead of real expiry the token is considered stale.</param>
        /// <param name="now">The current instant; defaults to <see cref="DateTimeOffset.UtcNow"/>.</param>
        /// <returns><see langword="true"/> when the token should be refreshed.</returns>
        public bool IsExpired(TimeSpan clockSkew, DateTimeOffset? now = null) =>
            (now ?? DateTimeOffset.UtcNow) >= ExpiresAt - clockSkew;
    }
}

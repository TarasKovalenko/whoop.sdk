using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Http;
using Whoop.Sdk.Serialization;

namespace Whoop.Sdk.Authentication
{
    /// <summary>
    /// Implements the WHOOP authorization-code flow: builds the consent URL, exchanges the returned
    /// code for tokens, and refreshes them.
    /// </summary>
    public sealed class WhoopOAuthClient
    {
        /// <summary>The endpoint the user's browser is sent to in order to grant consent.</summary>
        public static readonly Uri DefaultAuthorizationEndpoint = new Uri("https://api.prod.whoop.com/oauth/oauth2/auth");

        /// <summary>The endpoint that issues and refreshes tokens.</summary>
        public static readonly Uri DefaultTokenEndpoint = new Uri("https://api.prod.whoop.com/oauth/oauth2/token");

        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        /// <summary>Creates a client for a registered WHOOP application.</summary>
        /// <param name="httpClient">The client used for token requests.</param>
        /// <param name="clientId">The application's OAuth client identifier.</param>
        /// <param name="clientSecret">The application's OAuth client secret.</param>
        /// <param name="authorizationEndpoint">Overrides <see cref="DefaultAuthorizationEndpoint"/>.</param>
        /// <param name="tokenEndpoint">Overrides <see cref="DefaultTokenEndpoint"/>.</param>
        public WhoopOAuthClient(
            HttpClient httpClient,
            string clientId,
            string clientSecret,
            Uri? authorizationEndpoint = null,
            Uri? tokenEndpoint = null)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("A client id is required.", nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException("A client secret is required.", nameof(clientSecret));
            }

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _clientId = clientId;
            _clientSecret = clientSecret;
            AuthorizationEndpoint = authorizationEndpoint ?? DefaultAuthorizationEndpoint;
            TokenEndpoint = tokenEndpoint ?? DefaultTokenEndpoint;
        }

        /// <summary>The endpoint the user's browser is sent to in order to grant consent.</summary>
        public Uri AuthorizationEndpoint { get; }

        /// <summary>The endpoint that issues and refreshes tokens.</summary>
        public Uri TokenEndpoint { get; }

        /// <summary>
        /// Builds the URL the user must visit to grant consent. WHOOP requires the <c>state</c>
        /// parameter to be at least eight characters long.
        /// </summary>
        /// <param name="redirectUri">The redirect URI registered for the application.</param>
        /// <param name="scopes">The scopes to request. See <see cref="WhoopScopes"/>.</param>
        /// <param name="state">Anti-forgery value echoed back on the redirect. Must be at least eight characters.</param>
        /// <returns>The absolute authorization URL.</returns>
        public Uri CreateAuthorizationUrl(Uri redirectUri, IEnumerable<string> scopes, string state)
        {
            if (redirectUri is null)
            {
                throw new ArgumentNullException(nameof(redirectUri));
            }

            if (scopes is null)
            {
                throw new ArgumentNullException(nameof(scopes));
            }

            if (string.IsNullOrWhiteSpace(state) || state.Length < 8)
            {
                throw new ArgumentException("WHOOP requires a state value of at least eight characters.", nameof(state));
            }

            var scope = string.Join(" ", scopes);
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("At least one scope must be requested.", nameof(scopes));
            }

            var query = QueryString.Build(new[]
            {
                new KeyValuePair<string, string?>("client_id", _clientId),
                new KeyValuePair<string, string?>("redirect_uri", redirectUri.AbsoluteUri),
                new KeyValuePair<string, string?>("response_type", "code"),
                new KeyValuePair<string, string?>("scope", scope),
                new KeyValuePair<string, string?>("state", state),
            });

            return new Uri(AuthorizationEndpoint.AbsoluteUri + query);
        }

        /// <summary>Exchanges an authorization code for a token set.</summary>
        /// <param name="code">The <c>code</c> query parameter from the redirect.</param>
        /// <param name="redirectUri">The same redirect URI used to obtain the code.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The issued token set.</returns>
        public Task<WhoopOAuthToken> ExchangeAuthorizationCodeAsync(
            string code,
            Uri redirectUri,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("An authorization code is required.", nameof(code));
            }

            if (redirectUri is null)
            {
                throw new ArgumentNullException(nameof(redirectUri));
            }

            return RequestTokenAsync(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["grant_type"] = "authorization_code",
                    ["code"] = code,
                    ["redirect_uri"] = redirectUri.AbsoluteUri,
                },
                cancellationToken);
        }

        /// <summary>Exchanges a refresh token for a new token set.</summary>
        /// <param name="refreshToken">The refresh token from a previous token response.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The issued token set, which contains a new refresh token that replaces the old one.</returns>
        public Task<WhoopOAuthToken> RefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("A refresh token is required.", nameof(refreshToken));
            }

            return RequestTokenAsync(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = refreshToken,
                    ["scope"] = WhoopScopes.Offline,
                },
                cancellationToken);
        }

        private async Task<WhoopOAuthToken> RequestTokenAsync(
            Dictionary<string, string> form,
            CancellationToken cancellationToken)
        {
            form["client_id"] = _clientId;
            form["client_secret"] = _clientSecret;

            using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(form),
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
#if NET5_0_OR_GREATER
            var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
            var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif

            if (!response.IsSuccessStatusCode)
            {
                throw new WhoopApiException(
                    response.StatusCode,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The WHOOP token request failed with status {0} ({1}).",
                        (int)response.StatusCode,
                        response.ReasonPhrase),
                    payload,
                    TokenEndpoint,
                    HttpMethod.Post.Method);
            }

            WhoopOAuthToken? token;
            try
            {
                token = JsonSerializer.Deserialize<WhoopOAuthToken>(payload, WhoopJson.Options);
            }
            catch (JsonException exception)
            {
                throw new WhoopApiException(
                    response.StatusCode,
                    "The WHOOP token endpoint returned a body that could not be deserialized.",
                    payload,
                    TokenEndpoint,
                    HttpMethod.Post.Method,
                    exception);
            }

            if (token is null || string.IsNullOrEmpty(token.AccessToken))
            {
                throw new WhoopApiException(
                    response.StatusCode,
                    "The WHOOP token endpoint returned a response without an access token.",
                    payload,
                    TokenEndpoint,
                    HttpMethod.Post.Method);
            }

            return token with { ObtainedAt = DateTimeOffset.UtcNow };
        }
    }
}

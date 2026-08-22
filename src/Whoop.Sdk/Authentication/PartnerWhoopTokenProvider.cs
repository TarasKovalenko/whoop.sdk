using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Endpoints;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Authentication
{
    /// <summary>
    /// Acquires and caches trusted-partner tokens through the client-credentials flow exposed at
    /// <c>POST /v2/partner/token</c>.
    /// </summary>
    public sealed class PartnerWhoopTokenProvider : IWhoopTokenProvider, IDisposable
    {
        private readonly IPartnerClient _partnerClient;
        private readonly PartnerTokenRequest _request;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _clockSkew;

        private string? _accessToken;
        private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;
        private bool _disposed;

        /// <summary>Creates a provider that authenticates with partner credentials.</summary>
        /// <param name="httpClient">
        /// An unauthenticated client whose <see cref="HttpClient.BaseAddress"/> points at the WHOOP
        /// developer API. It must not carry the authentication handler that consumes this provider,
        /// otherwise acquiring a token would recurse.
        /// </param>
        /// <param name="clientId">The partner's OAuth client identifier.</param>
        /// <param name="clientSecret">The partner's OAuth client secret.</param>
        /// <param name="clockSkew">How far ahead of expiry a new token is requested. Defaults to one minute.</param>
        public PartnerWhoopTokenProvider(
            HttpClient httpClient,
            string clientId,
            string clientSecret,
            TimeSpan? clockSkew = null)
            : this(new PartnerClient(new WhoopApiConnection(httpClient)), clientId, clientSecret, clockSkew)
        {
        }

        /// <summary>Creates a provider over an existing partner client. Primarily a testing seam.</summary>
        /// <param name="partnerClient">Used to call the token endpoint.</param>
        /// <param name="clientId">The partner's OAuth client identifier.</param>
        /// <param name="clientSecret">The partner's OAuth client secret.</param>
        /// <param name="clockSkew">How far ahead of expiry a new token is requested. Defaults to one minute.</param>
        public PartnerWhoopTokenProvider(
            IPartnerClient partnerClient,
            string clientId,
            string clientSecret,
            TimeSpan? clockSkew = null)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("A client id is required.", nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(clientSecret))
            {
                throw new ArgumentException("A client secret is required.", nameof(clientSecret));
            }

            _partnerClient = partnerClient ?? throw new ArgumentNullException(nameof(partnerClient));
            _request = new PartnerTokenRequest { ClientId = clientId, ClientSecret = clientSecret };
            _clockSkew = clockSkew ?? TimeSpan.FromMinutes(1);
        }

        /// <inheritdoc />
        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (TryGetCachedToken(out var cached))
            {
                return cached;
            }

            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (TryGetCachedToken(out cached))
                {
                    return cached;
                }

                var response = await _partnerClient
                    .RequestTokenAsync(_request, cancellationToken)
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(response.AccessToken))
                {
                    throw new InvalidOperationException(
                        "The WHOOP partner token endpoint returned a response without an access token.");
                }

                _accessToken = response.AccessToken;
                _expiresAt = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn);
                return response.AccessToken!;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _refreshLock.Dispose();
        }

        private bool TryGetCachedToken(out string token)
        {
            var current = _accessToken;
            if (current is not null && DateTimeOffset.UtcNow < _expiresAt - _clockSkew)
            {
                token = current;
                return true;
            }

            token = string.Empty;
            return false;
        }
    }
}

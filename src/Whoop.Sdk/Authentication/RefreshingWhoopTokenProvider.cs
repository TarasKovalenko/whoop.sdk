using System;
using System.Threading;
using System.Threading.Tasks;

namespace Whoop.Sdk.Authentication
{
    /// <summary>
    /// Keeps a token set fresh: hands out the cached access token until it is about to expire, then
    /// refreshes it exactly once even under concurrent access.
    /// </summary>
    public sealed class RefreshingWhoopTokenProvider : IWhoopTokenProvider, IDisposable
    {
        private readonly WhoopOAuthClient _oauthClient;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _clockSkew;
        private readonly Func<WhoopOAuthToken, CancellationToken, Task>? _onTokenRefreshed;

        private WhoopOAuthToken? _token;
        private string _refreshToken;
        private bool _disposed;

        /// <summary>Creates a provider seeded with a refresh token.</summary>
        /// <param name="oauthClient">Used to perform the refresh.</param>
        /// <param name="refreshToken">A refresh token obtained with the <see cref="WhoopScopes.Offline"/> scope.</param>
        /// <param name="initialToken">An access token already in hand, to avoid an immediate refresh.</param>
        /// <param name="clockSkew">How far ahead of expiry a refresh is triggered. Defaults to one minute.</param>
        /// <param name="onTokenRefreshed">
        /// Invoked after every successful refresh so the caller can persist the rotated refresh token.
        /// WHOOP issues a new refresh token on each refresh and invalidates the previous one.
        /// </param>
        public RefreshingWhoopTokenProvider(
            WhoopOAuthClient oauthClient,
            string refreshToken,
            WhoopOAuthToken? initialToken = null,
            TimeSpan? clockSkew = null,
            Func<WhoopOAuthToken, CancellationToken, Task>? onTokenRefreshed = null)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("A refresh token is required.", nameof(refreshToken));
            }

            _oauthClient = oauthClient ?? throw new ArgumentNullException(nameof(oauthClient));
            _refreshToken = refreshToken;
            _token = initialToken;
            _clockSkew = clockSkew ?? TimeSpan.FromMinutes(1);
            _onTokenRefreshed = onTokenRefreshed;
        }

        /// <summary>The refresh token currently in use. Rotates on every successful refresh.</summary>
        public string CurrentRefreshToken => Volatile.Read(ref _refreshToken);

        /// <inheritdoc />
        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var current = _token;
            if (current is not null && !current.IsExpired(_clockSkew))
            {
                return current.AccessToken;
            }

            await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Another caller may have refreshed while this one waited for the lock.
                current = _token;
                if (current is not null && !current.IsExpired(_clockSkew))
                {
                    return current.AccessToken;
                }

                var refreshed = await _oauthClient
                    .RefreshTokenAsync(_refreshToken, cancellationToken)
                    .ConfigureAwait(false);

                _token = refreshed;
                if (!string.IsNullOrEmpty(refreshed.RefreshToken))
                {
                    Volatile.Write(ref _refreshToken, refreshed.RefreshToken!);
                }

                if (_onTokenRefreshed is not null)
                {
                    await _onTokenRefreshed(refreshed, cancellationToken).ConfigureAwait(false);
                }

                return refreshed.AccessToken;
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
    }
}

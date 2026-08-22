using System;
using System.Threading;
using System.Threading.Tasks;

namespace Whoop.Sdk.Authentication
{
    /// <summary>
    /// Returns a fixed access token. Suitable for short-lived scripts and tests; production callers
    /// should prefer <see cref="RefreshingWhoopTokenProvider"/> so tokens survive expiry.
    /// </summary>
    public sealed class StaticWhoopTokenProvider : IWhoopTokenProvider
    {
        private readonly Task<string> _token;

        /// <summary>Creates a provider around an already-issued token.</summary>
        /// <param name="accessToken">The bearer token, without the <c>Bearer</c> prefix.</param>
        public StaticWhoopTokenProvider(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("An access token is required.", nameof(accessToken));
            }

            _token = Task.FromResult(accessToken);
        }

        /// <inheritdoc />
        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) => _token;
    }

    /// <summary>Adapts an arbitrary callback to <see cref="IWhoopTokenProvider"/>.</summary>
    public sealed class DelegateWhoopTokenProvider : IWhoopTokenProvider
    {
        private readonly Func<CancellationToken, Task<string>> _factory;

        /// <summary>Creates a provider that defers to <paramref name="factory"/> on every request.</summary>
        /// <param name="factory">Returns a currently valid access token.</param>
        public DelegateWhoopTokenProvider(Func<CancellationToken, Task<string>> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <inheritdoc />
        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            _factory(cancellationToken);
    }
}

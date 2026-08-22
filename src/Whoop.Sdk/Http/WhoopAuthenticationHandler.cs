using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Authentication;

namespace Whoop.Sdk.Http
{
    /// <summary>
    /// Attaches a bearer token from an <see cref="IWhoopTokenProvider"/> to every outgoing request.
    /// Requests that already carry an <c>Authorization</c> header are left untouched.
    /// </summary>
    public sealed class WhoopAuthenticationHandler : DelegatingHandler
    {
        private const string BearerScheme = "Bearer";

        private readonly IWhoopTokenProvider _tokenProvider;

        /// <summary>Creates a handler over the supplied token provider.</summary>
        /// <param name="tokenProvider">Supplies the access token.</param>
        public WhoopAuthenticationHandler(IWhoopTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        /// <summary>Creates a handler over the supplied token provider and inner handler.</summary>
        /// <param name="tokenProvider">Supplies the access token.</param>
        /// <param name="innerHandler">The next handler in the pipeline.</param>
        public WhoopAuthenticationHandler(IWhoopTokenProvider tokenProvider, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Headers.Authorization is null)
            {
                var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, token);
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}

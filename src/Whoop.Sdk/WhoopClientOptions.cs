using System;

namespace Whoop.Sdk
{
    /// <summary>Configuration for <see cref="WhoopClient"/>.</summary>
    public sealed class WhoopClientOptions
    {
        /// <summary>The default WHOOP developer API base address.</summary>
        public static readonly Uri DefaultBaseAddress = new Uri("https://api.prod.whoop.com/developer/");

        private Uri _baseAddress = DefaultBaseAddress;

        /// <summary>
        /// Base address every request is resolved against. Must be absolute and must end with a
        /// trailing slash so that relative endpoint paths append rather than replace the last segment.
        /// Defaults to <see cref="DefaultBaseAddress"/>.
        /// </summary>
        public Uri BaseAddress
        {
            get => _baseAddress;
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!value.IsAbsoluteUri)
                {
                    throw new ArgumentException("The base address must be an absolute URI.", nameof(value));
                }

                _baseAddress = value.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
                    ? value
                    : new Uri(value.AbsoluteUri + "/");
            }
        }

        /// <summary>
        /// Value appended to the <c>User-Agent</c> header. Defaults to <c>null</c>, in which case only
        /// the library's own product token is sent.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Token provider used to authenticate outgoing requests when the client creates its own
        /// <see cref="System.Net.Http.HttpClient"/>. Ignored when an externally configured
        /// <see cref="System.Net.Http.HttpClient"/> is supplied, because authentication is then expected
        /// to be handled by a delegating handler on that pipeline.
        /// </summary>
        public Whoop.Sdk.Authentication.IWhoopTokenProvider? TokenProvider { get; set; }

        /// <summary>Request timeout applied to the client-owned <see cref="System.Net.Http.HttpClient"/>. Defaults to 100 seconds.</summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
    }
}

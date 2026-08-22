using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Endpoints;
using Whoop.Sdk.Http;

namespace Whoop.Sdk
{
    /// <summary>
    /// Default <see cref="IWhoopClient"/> implementation.
    /// </summary>
    /// <remarks>
    /// The client is thread safe and designed to be long lived. In applications that already use
    /// <c>IHttpClientFactory</c>, prefer the <c>AddWhoopClient</c> extension from the
    /// <c>Whoop.Sdk.Extensions.DependencyInjection</c> package over constructing instances directly.
    /// </remarks>
    public sealed class WhoopClient : IWhoopClient, IDisposable
    {
        private static readonly ProductInfoHeaderValue LibraryUserAgent = CreateLibraryUserAgent();

        private readonly HttpClient? _ownedHttpClient;
        private bool _disposed;

        /// <summary>Creates a client that authenticates with a fixed access token.</summary>
        /// <param name="accessToken">A WHOOP access token, without the <c>Bearer</c> prefix.</param>
        public WhoopClient(string accessToken)
            : this(new WhoopClientOptions { TokenProvider = new StaticWhoopTokenProvider(accessToken) })
        {
        }

        /// <summary>Creates a client that owns and configures its own <see cref="HttpClient"/>.</summary>
        /// <param name="options">Base address, timeout, user agent, and token provider.</param>
        public WhoopClient(WhoopClientOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _ownedHttpClient = CreateHttpClient(options);
            Connection = new WhoopApiConnection(_ownedHttpClient);
            (Cycles, Recovery, Sleep, Workouts, User, Partner, ActivityMappings) = CreateEndpoints(Connection);
        }

        /// <summary>
        /// Creates a client over an externally managed <see cref="HttpClient"/>, for example one handed
        /// out by <c>IHttpClientFactory</c>. Authentication is expected to be configured on that
        /// client's handler pipeline. The client is not disposed by this instance.
        /// </summary>
        /// <param name="httpClient">
        /// The client to send requests with. When its <see cref="HttpClient.BaseAddress"/> is not set,
        /// <see cref="WhoopClientOptions.DefaultBaseAddress"/> is applied.
        /// </param>
        public WhoopClient(HttpClient httpClient)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            httpClient.BaseAddress ??= WhoopClientOptions.DefaultBaseAddress;
            Connection = new WhoopApiConnection(httpClient);
            (Cycles, Recovery, Sleep, Workouts, User, Partner, ActivityMappings) = CreateEndpoints(Connection);
        }

        /// <summary>Creates a client over a custom connection. Primarily a testing seam.</summary>
        /// <param name="connection">The transport used for every request.</param>
        public WhoopClient(IWhoopApiConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            (Cycles, Recovery, Sleep, Workouts, User, Partner, ActivityMappings) = CreateEndpoints(Connection);
        }

        /// <summary>The underlying transport, exposed for endpoints this library does not wrap yet.</summary>
        public IWhoopApiConnection Connection { get; }

        /// <inheritdoc />
        public ICycleClient Cycles { get; }

        /// <inheritdoc />
        public IRecoveryClient Recovery { get; }

        /// <inheritdoc />
        public ISleepClient Sleep { get; }

        /// <inheritdoc />
        public IWorkoutClient Workouts { get; }

        /// <inheritdoc />
        public IUserClient User { get; }

        /// <inheritdoc />
        public IPartnerClient Partner { get; }

        /// <inheritdoc />
        public IActivityMappingClient ActivityMappings { get; }

        /// <summary>Disposes the <see cref="HttpClient"/> when this instance created it.</summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _ownedHttpClient?.Dispose();
        }

        private static (ICycleClient, IRecoveryClient, ISleepClient, IWorkoutClient, IUserClient, IPartnerClient, IActivityMappingClient)
            CreateEndpoints(IWhoopApiConnection connection) =>
            (
                new CycleClient(connection),
                new RecoveryClient(connection),
                new SleepClient(connection),
                new WorkoutClient(connection),
                new UserClient(connection),
                new PartnerClient(connection),
                new ActivityMappingClient(connection));

        internal static HttpClient CreateHttpClient(WhoopClientOptions options)
        {
            HttpMessageHandler handler = new HttpClientHandler();
            if (options.TokenProvider is not null)
            {
                handler = new WhoopAuthenticationHandler(options.TokenProvider, handler);
            }

            var httpClient = new HttpClient(handler, disposeHandler: true)
            {
                BaseAddress = options.BaseAddress,
                Timeout = options.Timeout,
            };

            httpClient.DefaultRequestHeaders.UserAgent.Add(LibraryUserAgent);
            if (!string.IsNullOrWhiteSpace(options.UserAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            }

            return httpClient;
        }

        private static ProductInfoHeaderValue CreateLibraryUserAgent()
        {
            var version = typeof(WhoopClient).GetTypeInfo().Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            // Strip any source-control metadata the SDK appends, for example "1.0.0+abc1234".
            var plus = version?.IndexOf('+') ?? -1;
            if (plus > 0)
            {
                version = version!.Substring(0, plus);
            }

            return new ProductInfoHeaderValue("Whoop.Sdk", string.IsNullOrWhiteSpace(version) ? "1.0.0" : version);
        }
    }
}

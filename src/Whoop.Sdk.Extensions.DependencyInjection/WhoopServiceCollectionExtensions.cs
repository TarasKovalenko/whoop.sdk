using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Http;

namespace Whoop.Sdk.Extensions.DependencyInjection
{
    /// <summary>Registers Whoop.Sdk with <c>Microsoft.Extensions.DependencyInjection</c>.</summary>
    public static class WhoopServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="IWhoopClient"/> as a typed <c>HttpClient</c>, wiring the base address,
        /// user agent, and the <see cref="WhoopAuthenticationHandler"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Optionally overrides the base address, timeout, or user agent.</param>
        /// <returns>
        /// The <see cref="IHttpClientBuilder"/> for the registered client, so callers can add resilience
        /// handlers, logging, or a custom primary handler.
        /// </returns>
        /// <remarks>
        /// An <see cref="IWhoopTokenProvider"/> must also be registered, for example with
        /// <see cref="AddWhoopAccessToken"/> or <see cref="AddWhoopOAuth"/>.
        /// </remarks>
        public static IHttpClientBuilder AddWhoopClient(
            this IServiceCollection services,
            Action<WhoopClientOptions>? configureOptions = null)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions is not null)
            {
                services.Configure(configureOptions);
            }

            return services
                .AddHttpClient<IWhoopClient, WhoopClient>(ConfigureHttpClient)
                .AddHttpMessageHandler(serviceProvider =>
                    new WhoopAuthenticationHandler(serviceProvider.GetRequiredService<IWhoopTokenProvider>()));
        }

        /// <summary>Authenticates every request with a fixed access token.</summary>
        /// <param name="services">The service collection.</param>
        /// <param name="accessToken">A WHOOP access token, without the <c>Bearer</c> prefix.</param>
        /// <returns>The same service collection, for chaining.</returns>
        public static IServiceCollection AddWhoopAccessToken(this IServiceCollection services, string accessToken)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IWhoopTokenProvider>(new StaticWhoopTokenProvider(accessToken));
            return services;
        }

        /// <summary>
        /// Registers a <see cref="WhoopOAuthClient"/> for the authorization-code flow, along with its own
        /// unauthenticated <c>HttpClient</c>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="clientId">The application's OAuth client identifier.</param>
        /// <param name="clientSecret">The application's OAuth client secret.</param>
        /// <returns>The same service collection, for chaining.</returns>
        /// <remarks>
        /// Token storage is application specific, so no <see cref="IWhoopTokenProvider"/> is registered
        /// here. Register one that reads the current user's refresh token, typically a scoped
        /// <see cref="RefreshingWhoopTokenProvider"/> or a <see cref="DelegateWhoopTokenProvider"/>.
        /// </remarks>
        public static IServiceCollection AddWhoopOAuth(
            this IServiceCollection services,
            string clientId,
            string clientSecret)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHttpClient(nameof(WhoopOAuthClient));
            services.AddSingleton(serviceProvider =>
            {
                var httpClient = serviceProvider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient(nameof(WhoopOAuthClient));

                return new WhoopOAuthClient(httpClient, clientId, clientSecret);
            });

            return services;
        }

        /// <summary>
        /// Authenticates every request with the trusted-partner client-credentials flow. Tokens are
        /// acquired over a separate, unauthenticated <c>HttpClient</c> so that acquisition cannot recurse.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="clientId">The partner's OAuth client identifier.</param>
        /// <param name="clientSecret">The partner's OAuth client secret.</param>
        /// <returns>The same service collection, for chaining.</returns>
        public static IServiceCollection AddWhoopPartnerAuthentication(
            this IServiceCollection services,
            string clientId,
            string clientSecret)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            const string HttpClientName = "Whoop.PartnerToken";
            services.AddHttpClient(HttpClientName, ConfigureHttpClient);

            services.AddSingleton<IWhoopTokenProvider>(serviceProvider =>
            {
                var httpClient = serviceProvider
                    .GetRequiredService<IHttpClientFactory>()
                    .CreateClient(HttpClientName);

                return new PartnerWhoopTokenProvider(httpClient, clientId, clientSecret);
            });

            return services;
        }

        private static void ConfigureHttpClient(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            var options = serviceProvider.GetRequiredService<IOptions<WhoopClientOptions>>().Value;

            httpClient.BaseAddress = options.BaseAddress;
            httpClient.Timeout = options.Timeout;

            if (!string.IsNullOrWhiteSpace(options.UserAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            }
        }
    }
}

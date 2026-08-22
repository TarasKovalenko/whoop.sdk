using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Serialization;

namespace Whoop.Sdk.Http
{
    /// <summary>Default <see cref="IWhoopApiConnection"/>, built on <see cref="HttpClient"/>.</summary>
    public sealed class WhoopApiConnection : IWhoopApiConnection
    {
        private const string JsonMediaType = "application/json";

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>Creates a connection over the supplied client.</summary>
        /// <param name="httpClient">
        /// The client used for every request. Its <see cref="HttpClient.BaseAddress"/> must be set to an
        /// absolute URI ending in a slash, and any authentication must already be configured on its pipeline.
        /// </param>
        /// <param name="jsonOptions">Serializer settings; defaults to <see cref="WhoopJson.Options"/>.</param>
        public WhoopApiConnection(HttpClient httpClient, JsonSerializerOptions? jsonOptions = null)
        {
            if (httpClient is null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (httpClient.BaseAddress is null)
            {
                throw new ArgumentException(
                    "The HttpClient must have a BaseAddress pointing at the WHOOP developer API.",
                    nameof(httpClient));
            }

            _httpClient = httpClient;
            _jsonOptions = jsonOptions ?? WhoopJson.Options;
        }

        /// <inheritdoc />
        public async Task<TResponse> SendAsync<TResponse>(
            HttpMethod method,
            string path,
            IEnumerable<KeyValuePair<string, string?>>? query,
            object? body,
            CancellationToken cancellationToken)
        {
            using var response = await SendCoreAsync(method, path, query, body, cancellationToken).ConfigureAwait(false);

#if NET5_0_OR_GREATER
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

            TResponse? result;
            try
            {
                result = await JsonSerializer
                    .DeserializeAsync<TResponse>(stream, _jsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (JsonException exception)
            {
                throw new WhoopApiException(
                    response.StatusCode,
                    $"The WHOOP API returned a body that could not be deserialized into {typeof(TResponse).Name}.",
                    requestUri: response.RequestMessage?.RequestUri,
                    method: method.Method,
                    innerException: exception);
            }

            if (result is null)
            {
                throw new WhoopApiException(
                    response.StatusCode,
                    $"The WHOOP API returned an empty body where a {typeof(TResponse).Name} was expected.",
                    requestUri: response.RequestMessage?.RequestUri,
                    method: method.Method);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task SendAsync(
            HttpMethod method,
            string path,
            IEnumerable<KeyValuePair<string, string?>>? query,
            object? body,
            CancellationToken cancellationToken)
        {
            using var response = await SendCoreAsync(method, path, query, body, cancellationToken).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendCoreAsync(
            HttpMethod method,
            string path,
            IEnumerable<KeyValuePair<string, string?>>? query,
            object? body,
            CancellationToken cancellationToken)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("The endpoint path must not be empty.", nameof(path));
            }

            using var request = new HttpRequestMessage(method, path + QueryString.Build(query));

            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body, body.GetType(), _jsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, JsonMediaType);
            }

            var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            try
            {
                await ThrowApiExceptionAsync(response, method, request.RequestUri).ConfigureAwait(false);
                return response;
            }
            finally
            {
                response.Dispose();
            }
        }

        private static async Task ThrowApiExceptionAsync(
            HttpResponseMessage response,
            HttpMethod method,
            Uri? requestUri)
        {
            var responseBody = await ReadBodySafelyAsync(response).ConfigureAwait(false);
            requestUri = response.RequestMessage?.RequestUri ?? requestUri;
            var statusCode = (int)response.StatusCode;

            if (statusCode == 429)
            {
                throw new WhoopRateLimitExceededException(
                    "The WHOOP API rate limit was exceeded.",
                    ReadRetryAfter(response),
                    responseBody,
                    requestUri,
                    method.Method);
            }

            var message = string.Format(
                CultureInfo.InvariantCulture,
                "The WHOOP API request '{0} {1}' failed with status {2} ({3}).",
                method.Method,
                requestUri,
                statusCode,
                response.ReasonPhrase);

            throw new WhoopApiException(response.StatusCode, message, responseBody, requestUri, method.Method);
        }

        private static TimeSpan? ReadRetryAfter(HttpResponseMessage response)
        {
            var retryAfter = response.Headers.RetryAfter;
            if (retryAfter is null)
            {
                return null;
            }

            if (retryAfter.Delta is { } delta)
            {
                return delta;
            }

            if (retryAfter.Date is { } date)
            {
                var remaining = date - DateTimeOffset.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }

            return null;
        }

        private static async Task<string?> ReadBodySafelyAsync(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return string.IsNullOrWhiteSpace(content) ? null : content;
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }
    }

    /// <summary>HTTP verbs that are missing from <see cref="HttpMethod"/> on older targets.</summary>
    internal static class WhoopHttpMethods
    {
        /// <summary>The <c>PATCH</c> verb.</summary>
        public static readonly HttpMethod Patch =
#if NETSTANDARD2_0
            new HttpMethod("PATCH");
#else
            HttpMethod.Patch;
#endif
    }
}

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Whoop.Sdk.Http
{
    /// <summary>
    /// The low-level transport used by every endpoint client. Exposed so that callers can reach
    /// endpoints this library does not wrap yet, and so that endpoint clients can be unit tested
    /// without an HTTP stack.
    /// </summary>
    public interface IWhoopApiConnection
    {
        /// <summary>Sends a request and deserializes the JSON response body.</summary>
        /// <typeparam name="TResponse">The type to deserialize the response body into.</typeparam>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">Endpoint path relative to the configured base address, without a leading slash.</param>
        /// <param name="query">Query string parameters. Entries with a <see langword="null"/> value are skipped.</param>
        /// <param name="body">Object serialized as the JSON request body, or <see langword="null"/> for no body.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The deserialized response body.</returns>
        /// <exception cref="WhoopApiException">The API returned a non-success status code.</exception>
        Task<TResponse> SendAsync<TResponse>(
            HttpMethod method,
            string path,
            IEnumerable<KeyValuePair<string, string?>>? query,
            object? body,
            CancellationToken cancellationToken);

        /// <summary>Sends a request and discards the response body.</summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="path">Endpoint path relative to the configured base address, without a leading slash.</param>
        /// <param name="query">Query string parameters. Entries with a <see langword="null"/> value are skipped.</param>
        /// <param name="body">Object serialized as the JSON request body, or <see langword="null"/> for no body.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <exception cref="WhoopApiException">The API returned a non-success status code.</exception>
        Task SendAsync(
            HttpMethod method,
            string path,
            IEnumerable<KeyValuePair<string, string?>>? query,
            object? body,
            CancellationToken cancellationToken);
    }
}

using System;
using System.Net;

namespace Whoop.Sdk
{
    /// <summary>Thrown when the WHOOP API returns a non-success status code.</summary>
#if !NETSTANDARD2_0
    [Serializable]
#endif
    public class WhoopApiException : Exception
    {
        /// <summary>Creates a new instance.</summary>
        /// <param name="statusCode">The HTTP status code returned by the API.</param>
        /// <param name="message">A human readable description of the failure.</param>
        /// <param name="responseBody">The raw response body, when one was returned.</param>
        /// <param name="requestUri">The absolute URI of the failed request.</param>
        /// <param name="method">The HTTP method of the failed request.</param>
        /// <param name="innerException">The exception that caused this one, when there is one.</param>
        public WhoopApiException(
            HttpStatusCode statusCode,
            string message,
            string? responseBody = null,
            Uri? requestUri = null,
            string? method = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
            RequestUri = requestUri;
            Method = method;
        }

        /// <summary>The HTTP status code returned by the API.</summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>The raw response body, when one was returned.</summary>
        public string? ResponseBody { get; }

        /// <summary>The absolute URI of the failed request.</summary>
        public Uri? RequestUri { get; }

        /// <summary>The HTTP method of the failed request.</summary>
        public string? Method { get; }

        /// <summary><see langword="true"/> when the request failed because the caller is unauthenticated or lacks the required scope.</summary>
        public bool IsAuthenticationFailure =>
            StatusCode == HttpStatusCode.Unauthorized || StatusCode == HttpStatusCode.Forbidden;

        /// <summary><see langword="true"/> when the requested resource does not exist.</summary>
        public bool IsNotFound => StatusCode == HttpStatusCode.NotFound;
    }

    /// <summary>Thrown when the WHOOP API rejects a request because the rate limit was exceeded (HTTP 429).</summary>
#if !NETSTANDARD2_0
    [Serializable]
#endif
    public sealed class WhoopRateLimitExceededException : WhoopApiException
    {
        /// <summary>Creates a new instance.</summary>
        /// <param name="message">A human readable description of the failure.</param>
        /// <param name="retryAfter">How long the caller should wait before retrying, when the API said so.</param>
        /// <param name="responseBody">The raw response body, when one was returned.</param>
        /// <param name="requestUri">The absolute URI of the failed request.</param>
        /// <param name="method">The HTTP method of the failed request.</param>
        public WhoopRateLimitExceededException(
            string message,
            TimeSpan? retryAfter = null,
            string? responseBody = null,
            Uri? requestUri = null,
            string? method = null)
            : base((HttpStatusCode)429, message, responseBody, requestUri, method)
        {
            RetryAfter = retryAfter;
        }

        /// <summary>How long the caller should wait before retrying, as advertised by the <c>Retry-After</c> header.</summary>
        public TimeSpan? RetryAfter { get; }
    }
}

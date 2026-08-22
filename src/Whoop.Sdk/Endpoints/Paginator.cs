using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <summary>Turns WHOOP's cursor-paged collection endpoints into a flat asynchronous sequence.</summary>
    internal static class Paginator
    {
        /// <summary>Builds the query string parameters shared by every collection endpoint.</summary>
        public static IEnumerable<KeyValuePair<string, string?>> BuildQuery(WhoopCollectionRequest? request) =>
            new[]
            {
                new KeyValuePair<string, string?>("limit", QueryString.Format(request?.Limit)),
                new KeyValuePair<string, string?>("start", QueryString.Format(request?.Start)),
                new KeyValuePair<string, string?>("end", QueryString.Format(request?.End)),
                new KeyValuePair<string, string?>("nextToken", request?.NextToken),
            };

        /// <summary>
        /// Walks every page, yielding records as they arrive. Stops when the API reports no further
        /// cursor, and guards against a server that keeps echoing the same cursor back.
        /// </summary>
        public static async IAsyncEnumerable<T> EnumerateAsync<T>(
            Func<WhoopCollectionRequest, CancellationToken, Task<PaginatedResponse<T>>> fetchPage,
            WhoopCollectionRequest? request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var current = request ?? new WhoopCollectionRequest();
            string? previousToken = null;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var page = await fetchPage(current, cancellationToken).ConfigureAwait(false);

                foreach (var record in page.Records)
                {
                    yield return record;
                }

                if (string.IsNullOrEmpty(page.NextToken) || string.Equals(page.NextToken, previousToken, StringComparison.Ordinal))
                {
                    yield break;
                }

                previousToken = page.NextToken;
                current = current with { NextToken = page.NextToken };
            }
        }
    }
}

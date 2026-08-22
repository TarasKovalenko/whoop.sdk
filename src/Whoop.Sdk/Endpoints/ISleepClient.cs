using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <summary>The <c>/v2/activity/sleep</c> endpoints. Requires the <c>read:sleep</c> scope.</summary>
    public interface ISleepClient
    {
        /// <summary>Gets a single sleep activity.</summary>
        /// <param name="sleepId">The sleep identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The sleep activity.</returns>
        Task<Sleep> GetAsync(Guid sleepId, CancellationToken cancellationToken = default);

        /// <summary>Gets a single page of sleep activities, most recent first.</summary>
        /// <param name="request">Paging and date filters.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>One page of sleep activities plus the cursor for the next page.</returns>
        Task<PaginatedResponse<Sleep>> ListAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default);

        /// <summary>Streams every sleep activity matching the filters, fetching pages on demand.</summary>
        /// <param name="request">Date filters and page size. Any <c>NextToken</c> is used as the starting cursor.</param>
        /// <param name="cancellationToken">Stops the enumeration.</param>
        /// <returns>An asynchronous sequence over all matching sleep activities.</returns>
        IAsyncEnumerable<Sleep> EnumerateAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default);

        /// <summary>Gets the sleep recorded for a cycle.</summary>
        /// <param name="cycleId">The cycle identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The sleep recorded for the cycle.</returns>
        Task<Sleep> GetForCycleAsync(long cycleId, CancellationToken cancellationToken = default);
    }
}

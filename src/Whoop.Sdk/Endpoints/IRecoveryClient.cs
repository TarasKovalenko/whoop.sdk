using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <summary>The <c>/v2/recovery</c> endpoints. Requires the <c>read:recovery</c> scope.</summary>
    public interface IRecoveryClient
    {
        /// <summary>Gets a single page of recoveries, most recent first.</summary>
        /// <param name="request">Paging and date filters.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>One page of recoveries plus the cursor for the next page.</returns>
        Task<PaginatedResponse<Recovery>> ListAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default);

        /// <summary>Streams every recovery matching the filters, fetching pages on demand.</summary>
        /// <param name="request">Date filters and page size. Any <c>NextToken</c> is used as the starting cursor.</param>
        /// <param name="cancellationToken">Stops the enumeration.</param>
        /// <returns>An asynchronous sequence over all matching recoveries.</returns>
        IAsyncEnumerable<Recovery> EnumerateAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default);

        /// <summary>Gets the recovery scored for a cycle.</summary>
        /// <param name="cycleId">The cycle identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The recovery scored for the cycle.</returns>
        Task<Recovery> GetForCycleAsync(long cycleId, CancellationToken cancellationToken = default);
    }
}

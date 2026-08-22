using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <summary>The <c>/v2/cycle</c> endpoints. Requires the <c>read:cycles</c> scope.</summary>
    public interface ICycleClient
    {
        /// <summary>Gets a single cycle.</summary>
        /// <param name="cycleId">The cycle identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The cycle.</returns>
        Task<Cycle> GetAsync(long cycleId, CancellationToken cancellationToken = default);

        /// <summary>Gets a single page of cycles, most recent first.</summary>
        /// <param name="request">Paging and date filters.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>One page of cycles plus the cursor for the next page.</returns>
        Task<PaginatedResponse<Cycle>> ListAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default);

        /// <summary>Streams every cycle matching the filters, fetching pages on demand.</summary>
        /// <param name="request">Date filters and page size. Any <c>NextToken</c> is used as the starting cursor.</param>
        /// <param name="cancellationToken">Stops the enumeration.</param>
        /// <returns>An asynchronous sequence over all matching cycles.</returns>
        IAsyncEnumerable<Cycle> EnumerateAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default);

        /// <summary>Gets the sleep that belongs to a cycle.</summary>
        /// <param name="cycleId">The cycle identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The sleep recorded for the cycle.</returns>
        Task<Sleep> GetSleepAsync(long cycleId, CancellationToken cancellationToken = default);

        /// <summary>Gets the recovery that belongs to a cycle. Requires the <c>read:recovery</c> scope.</summary>
        /// <param name="cycleId">The cycle identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The recovery scored for the cycle.</returns>
        Task<Recovery> GetRecoveryAsync(long cycleId, CancellationToken cancellationToken = default);
    }
}

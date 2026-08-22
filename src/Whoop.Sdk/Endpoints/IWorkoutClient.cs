using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <summary>The <c>/v2/activity/workout</c> endpoints. Requires the <c>read:workout</c> scope.</summary>
    public interface IWorkoutClient
    {
        /// <summary>Gets a single workout.</summary>
        /// <param name="workoutId">The workout identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The workout.</returns>
        Task<Workout> GetAsync(Guid workoutId, CancellationToken cancellationToken = default);

        /// <summary>Gets a single page of workouts, most recent first.</summary>
        /// <param name="request">Paging and date filters.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>One page of workouts plus the cursor for the next page.</returns>
        Task<PaginatedResponse<Workout>> ListAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default);

        /// <summary>Streams every workout matching the filters, fetching pages on demand.</summary>
        /// <param name="request">Date filters and page size. Any <c>NextToken</c> is used as the starting cursor.</param>
        /// <param name="cancellationToken">Stops the enumeration.</param>
        /// <returns>An asynchronous sequence over all matching workouts.</returns>
        IAsyncEnumerable<Workout> EnumerateAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default);
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <inheritdoc cref="IWorkoutClient" />
    public sealed class WorkoutClient : IWorkoutClient
    {
        private readonly IWhoopApiConnection _connection;

        /// <summary>Creates a client over the supplied connection.</summary>
        /// <param name="connection">The transport used for every request.</param>
        public WorkoutClient(IWhoopApiConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc />
        public Task<Workout> GetAsync(Guid workoutId, CancellationToken cancellationToken = default) =>
            _connection.SendAsync<Workout>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/activity/workout/{workoutId:D}"),
                query: null,
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public Task<PaginatedResponse<Workout>> ListAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            _connection.SendAsync<PaginatedResponse<Workout>>(
                HttpMethod.Get,
                "v2/activity/workout",
                Paginator.BuildQuery(request),
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<Workout> EnumerateAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            Paginator.EnumerateAsync<Workout>(ListAsync, request, cancellationToken);
    }
}

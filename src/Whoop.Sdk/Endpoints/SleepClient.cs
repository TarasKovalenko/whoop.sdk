using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <inheritdoc cref="ISleepClient" />
    public sealed class SleepClient : ISleepClient
    {
        private readonly IWhoopApiConnection _connection;

        /// <summary>Creates a client over the supplied connection.</summary>
        /// <param name="connection">The transport used for every request.</param>
        public SleepClient(IWhoopApiConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc />
        public Task<Sleep> GetAsync(Guid sleepId, CancellationToken cancellationToken = default) =>
            _connection.SendAsync<Sleep>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/activity/sleep/{sleepId:D}"),
                query: null,
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public Task<PaginatedResponse<Sleep>> ListAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            _connection.SendAsync<PaginatedResponse<Sleep>>(
                HttpMethod.Get,
                "v2/activity/sleep",
                Paginator.BuildQuery(request),
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<Sleep> EnumerateAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            Paginator.EnumerateAsync<Sleep>(ListAsync, request, cancellationToken);

        /// <inheritdoc />
        public Task<Sleep> GetForCycleAsync(long cycleId, CancellationToken cancellationToken = default) =>
            _connection.SendAsync<Sleep>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/cycle/{cycleId}/sleep"),
                query: null,
                body: null,
                cancellationToken);
    }
}

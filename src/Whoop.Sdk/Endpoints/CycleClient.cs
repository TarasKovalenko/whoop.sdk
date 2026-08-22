using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <inheritdoc cref="ICycleClient" />
    public sealed class CycleClient : ICycleClient
    {
        private readonly IWhoopApiConnection _connection;

        /// <summary>Creates a client over the supplied connection.</summary>
        /// <param name="connection">The transport used for every request.</param>
        public CycleClient(IWhoopApiConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc />
        public Task<Cycle> GetAsync(long cycleId, CancellationToken cancellationToken = default) =>
            _connection.SendAsync<Cycle>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/cycle/{cycleId}"),
                query: null,
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public Task<PaginatedResponse<Cycle>> ListAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            _connection.SendAsync<PaginatedResponse<Cycle>>(
                HttpMethod.Get,
                "v2/cycle",
                Paginator.BuildQuery(request),
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<Cycle> EnumerateAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            Paginator.EnumerateAsync<Cycle>(ListAsync, request, cancellationToken);

        /// <inheritdoc />
        public Task<Sleep> GetSleepAsync(long cycleId, CancellationToken cancellationToken = default) =>
            _connection.SendAsync<Sleep>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/cycle/{cycleId}/sleep"),
                query: null,
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public Task<Recovery> GetRecoveryAsync(long cycleId, CancellationToken cancellationToken = default) =>
            _connection.SendAsync<Recovery>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/cycle/{cycleId}/recovery"),
                query: null,
                body: null,
                cancellationToken);
    }
}

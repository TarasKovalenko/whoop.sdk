using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <inheritdoc cref="IRecoveryClient" />
    public sealed class RecoveryClient : IRecoveryClient
    {
        private readonly IWhoopApiConnection _connection;

        /// <summary>Creates a client over the supplied connection.</summary>
        /// <param name="connection">The transport used for every request.</param>
        public RecoveryClient(IWhoopApiConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc />
        public Task<PaginatedResponse<Recovery>> ListAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            _connection.SendAsync<PaginatedResponse<Recovery>>(
                HttpMethod.Get,
                "v2/recovery",
                Paginator.BuildQuery(request),
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<Recovery> EnumerateAsync(
            WhoopCollectionRequest? request = null,
            CancellationToken cancellationToken = default) =>
            Paginator.EnumerateAsync<Recovery>(ListAsync, request, cancellationToken);

        /// <inheritdoc />
        public Task<Recovery> GetForCycleAsync(long cycleId, CancellationToken cancellationToken = default) =>
            _connection.SendAsync<Recovery>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/cycle/{cycleId}/recovery"),
                query: null,
                body: null,
                cancellationToken);
    }
}

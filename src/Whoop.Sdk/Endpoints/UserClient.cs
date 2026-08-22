using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <inheritdoc cref="IUserClient" />
    public sealed class UserClient : IUserClient
    {
        private readonly IWhoopApiConnection _connection;

        /// <summary>Creates a client over the supplied connection.</summary>
        /// <param name="connection">The transport used for every request.</param>
        public UserClient(IWhoopApiConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc />
        public Task<UserBasicProfile> GetBasicProfileAsync(CancellationToken cancellationToken = default) =>
            _connection.SendAsync<UserBasicProfile>(
                HttpMethod.Get,
                "v2/user/profile/basic",
                query: null,
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public Task<UserBodyMeasurement> GetBodyMeasurementAsync(CancellationToken cancellationToken = default) =>
            _connection.SendAsync<UserBodyMeasurement>(
                HttpMethod.Get,
                "v2/user/measurement/body",
                query: null,
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public Task RevokeAccessAsync(CancellationToken cancellationToken = default) =>
            _connection.SendAsync(
                HttpMethod.Delete,
                "v2/user/access",
                query: null,
                body: null,
                cancellationToken);
    }

    /// <inheritdoc cref="IActivityMappingClient" />
    public sealed class ActivityMappingClient : IActivityMappingClient
    {
        private readonly IWhoopApiConnection _connection;

        /// <summary>Creates a client over the supplied connection.</summary>
        /// <param name="connection">The transport used for every request.</param>
        public ActivityMappingClient(IWhoopApiConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc />
        public Task<ActivityIdMapping> GetAsync(long activityV1Id, CancellationToken cancellationToken = default) =>
            _connection.SendAsync<ActivityIdMapping>(
                HttpMethod.Get,
                FormattableString.Invariant($"v1/activity-mapping/{activityV1Id}"),
                query: null,
                body: null,
                cancellationToken);
    }
}

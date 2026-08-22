using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <summary>The <c>/v2/user</c> endpoints.</summary>
    public interface IUserClient
    {
        /// <summary>Gets the authenticated user's profile. Requires the <c>read:profile</c> scope.</summary>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The user's basic profile.</returns>
        Task<UserBasicProfile> GetBasicProfileAsync(CancellationToken cancellationToken = default);

        /// <summary>Gets the authenticated user's body measurements. Requires the <c>read:body_measurement</c> scope.</summary>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The user's body measurements.</returns>
        Task<UserBodyMeasurement> GetBodyMeasurementAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the access token being used for this call, along with the user's consent for the
        /// application. Subsequent requests with the same credentials will fail.
        /// </summary>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>A task that completes when the revocation has been accepted.</returns>
        Task RevokeAccessAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>The <c>/v1/activity-mapping</c> endpoint, which maps deprecated v1 identifiers onto v2 ones.</summary>
    public interface IActivityMappingClient
    {
        /// <summary>Looks up the v2 identifier for a v1 activity.</summary>
        /// <param name="activityV1Id">The identifier used by the deprecated v1 API.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The mapping to the v2 identifier.</returns>
        Task<ActivityIdMapping> GetAsync(long activityV1Id, CancellationToken cancellationToken = default);
    }
}

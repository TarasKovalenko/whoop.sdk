using System;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <summary>
    /// The <c>/v2/partner</c> endpoints, available to trusted lab partners authenticated with the
    /// client-credentials flow.
    /// </summary>
    public interface IPartnerClient
    {
        /// <summary>Exchanges partner credentials for an access token. This call is itself unauthenticated.</summary>
        /// <param name="request">The partner's client credentials.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The issued partner token.</returns>
        Task<PartnerTokenResponse> RequestTokenAsync(
            PartnerTokenRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>Gets a lab requisition, including its patient, service requests, and appointments.</summary>
        /// <param name="requisitionId">The requisition identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The requisition.</returns>
        Task<LabRequisition> GetLabRequisitionAsync(Guid requisitionId, CancellationToken cancellationToken = default);

        /// <summary>Gets a single service request.</summary>
        /// <param name="serviceRequestId">The service request identifier.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The service request.</returns>
        Task<ServiceRequest> GetServiceRequestAsync(Guid serviceRequestId, CancellationToken cancellationToken = default);

        /// <summary>Updates the business status of every service request on a requisition.</summary>
        /// <param name="requisitionId">The requisition identifier.</param>
        /// <param name="request">The new status.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>A task that completes when the update has been accepted.</returns>
        Task UpdateLabRequisitionStatusAsync(
            Guid requisitionId,
            ServiceRequestStatusRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>Updates the business status of a single service request.</summary>
        /// <param name="serviceRequestId">The service request identifier.</param>
        /// <param name="request">The new status.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>The updated service request.</returns>
        Task<ServiceRequest> UpdateServiceRequestStatusAsync(
            Guid serviceRequestId,
            ServiceRequestStatusRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>Uploads diagnostic report results against a service request.</summary>
        /// <param name="serviceRequestId">The service request identifier.</param>
        /// <param name="request">The report and its observations.</param>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>A task that completes when the results have been accepted.</returns>
        Task UploadDiagnosticReportResultsAsync(
            Guid serviceRequestId,
            DiagnosticReportCreateRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>Seeds test data into the partner sandbox. Not available in production.</summary>
        /// <param name="cancellationToken">Cancels the request.</param>
        /// <returns>A task that completes when the sandbox data has been created.</returns>
        Task AddTestDataAsync(CancellationToken cancellationToken = default);
    }
}

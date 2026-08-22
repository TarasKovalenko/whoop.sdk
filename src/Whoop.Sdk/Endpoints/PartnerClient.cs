using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;

namespace Whoop.Sdk.Endpoints
{
    /// <inheritdoc cref="IPartnerClient" />
    public sealed class PartnerClient : IPartnerClient
    {
        private readonly IWhoopApiConnection _connection;

        /// <summary>Creates a client over the supplied connection.</summary>
        /// <param name="connection">The transport used for every request.</param>
        public PartnerClient(IWhoopApiConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        /// <inheritdoc />
        public Task<PartnerTokenResponse> RequestTokenAsync(
            PartnerTokenRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _connection.SendAsync<PartnerTokenResponse>(
                HttpMethod.Post,
                "v2/partner/token",
                query: null,
                body: request,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<LabRequisition> GetLabRequisitionAsync(
            Guid requisitionId,
            CancellationToken cancellationToken = default) =>
            _connection.SendAsync<LabRequisition>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/partner/requisition/{requisitionId:D}"),
                query: null,
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public Task<ServiceRequest> GetServiceRequestAsync(
            Guid serviceRequestId,
            CancellationToken cancellationToken = default) =>
            _connection.SendAsync<ServiceRequest>(
                HttpMethod.Get,
                FormattableString.Invariant($"v2/partner/service-request/{serviceRequestId:D}"),
                query: null,
                body: null,
                cancellationToken);

        /// <inheritdoc />
        public Task UpdateLabRequisitionStatusAsync(
            Guid requisitionId,
            ServiceRequestStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _connection.SendAsync(
                WhoopHttpMethods.Patch,
                FormattableString.Invariant($"v2/partner/requisition/{requisitionId:D}/status"),
                query: null,
                body: request,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<ServiceRequest> UpdateServiceRequestStatusAsync(
            Guid serviceRequestId,
            ServiceRequestStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _connection.SendAsync<ServiceRequest>(
                WhoopHttpMethods.Patch,
                FormattableString.Invariant($"v2/partner/service-request/{serviceRequestId:D}/status"),
                query: null,
                body: request,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task UploadDiagnosticReportResultsAsync(
            Guid serviceRequestId,
            DiagnosticReportCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _connection.SendAsync(
                HttpMethod.Post,
                FormattableString.Invariant($"v2/partner/service-request/{serviceRequestId:D}/results"),
                query: null,
                body: request,
                cancellationToken);
        }

        /// <inheritdoc />
        public Task AddTestDataAsync(CancellationToken cancellationToken = default) =>
            _connection.SendAsync(
                HttpMethod.Post,
                "v2/partner/development/add-test-data",
                query: null,
                body: null,
                cancellationToken);
    }
}

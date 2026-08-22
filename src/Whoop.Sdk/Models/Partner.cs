using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Whoop.Sdk.Models
{
    /// <summary>Credentials exchanged for a trusted-partner access token.</summary>
    public sealed record PartnerTokenRequest
    {
        /// <summary>The partner's OAuth client identifier.</summary>
        [JsonPropertyName("client_id")]
        public string ClientId { get; init; } = string.Empty;

        /// <summary>The partner's OAuth client secret.</summary>
        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; init; } = string.Empty;

        /// <summary>Requested scope. Defaults to <c>whoop-partner/token</c>.</summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; init; } = "whoop-partner/token";

        /// <summary>OAuth grant type. Defaults to <c>client_credentials</c>.</summary>
        [JsonPropertyName("grant_type")]
        public string? GrantType { get; init; } = "client_credentials";
    }

    /// <summary>A trusted-partner access token.</summary>
    public sealed record PartnerTokenResponse
    {
        /// <summary>The bearer token to send on subsequent partner requests.</summary>
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        /// <summary>Lifetime of the token, in seconds.</summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        /// <summary>The token type, normally <c>Bearer</c>.</summary>
        [JsonPropertyName("token_type")]
        public string? TokenType { get; init; }
    }

    /// <summary>A lab requisition: the patient, the ordered service requests, and their appointments.</summary>
    public sealed record LabRequisition
    {
        /// <summary>Unique identifier for the requisition.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        /// <summary>When the record was first created.</summary>
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>When the record was last updated.</summary>
        [JsonPropertyName("updated_at")]
        public DateTimeOffset UpdatedAt { get; init; }

        /// <summary>The service requests that make up the requisition.</summary>
        [JsonPropertyName("service_requests")]
        public IReadOnlyList<ServiceRequest> ServiceRequests { get; init; } = new List<ServiceRequest>();

        /// <summary>The patient the requisition was raised for.</summary>
        [JsonPropertyName("patient")]
        public Patient? Patient { get; init; }

        /// <summary>Appointments scheduled to fulfil the requisition.</summary>
        [JsonPropertyName("appointments")]
        public IReadOnlyList<Appointment> Appointments { get; init; } = new List<Appointment>();
    }

    /// <summary>The patient a requisition belongs to.</summary>
    public sealed record Patient
    {
        /// <summary>Unique identifier for the patient.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; init; }
    }

    /// <summary>An appointment at which one or more service requests are fulfilled.</summary>
    public sealed record Appointment
    {
        /// <summary>The service requests associated with this appointment.</summary>
        [JsonPropertyName("service_request_ids")]
        public IReadOnlyList<Guid> ServiceRequestIds { get; init; } = new List<Guid>();

        /// <summary>When the appointment starts.</summary>
        [JsonPropertyName("start_time")]
        public DateTimeOffset StartTime { get; init; }
    }

    /// <summary>A single ordered test or panel within a lab requisition.</summary>
    public sealed record ServiceRequest
    {
        /// <summary>Unique identifier for the service request.</summary>
        [JsonPropertyName("id")]
        public Guid Id { get; init; }

        /// <summary>FHIR status of the service request.</summary>
        [JsonPropertyName("status")]
        public string? Status { get; init; }

        /// <summary>FHIR intent of the service request.</summary>
        [JsonPropertyName("intent")]
        public string? Intent { get; init; }

        /// <summary>Code identifying the ordered test or panel.</summary>
        [JsonPropertyName("code")]
        public string? Code { get; init; }

        /// <summary>Partner-facing business status of the associated task.</summary>
        [JsonPropertyName("task_business_status")]
        public string? TaskBusinessStatus { get; init; }

        /// <summary>Human readable description of the associated task.</summary>
        [JsonPropertyName("task_description")]
        public string? TaskDescription { get; init; }
    }

    /// <summary>Status transition applied to a service request or lab requisition.</summary>
    public sealed record ServiceRequestStatusRequest
    {
        /// <summary>The new business status.</summary>
        [JsonPropertyName("task_business_status")]
        public string? TaskBusinessStatus { get; init; }

        /// <summary>Optional reason for the transition.</summary>
        [JsonPropertyName("reason")]
        public string? Reason { get; init; }
    }

    /// <summary>A diagnostic report uploaded against a service request.</summary>
    public sealed record DiagnosticReportCreateRequest
    {
        /// <summary>FHIR status of the report.</summary>
        [JsonPropertyName("status")]
        public string? Status { get; init; }

        /// <summary>The individual observations contained in the report.</summary>
        [JsonPropertyName("observations")]
        public IReadOnlyList<CreateObservationRequest> Observations { get; init; } = new List<CreateObservationRequest>();
    }

    /// <summary>A single measured value within a diagnostic report.</summary>
    public sealed record CreateObservationRequest
    {
        /// <summary>The numeric result, when the observation is quantitative.</summary>
        [JsonPropertyName("value_numeric")]
        public double? ValueNumeric { get; init; }

        /// <summary>The textual result, when the observation is qualitative.</summary>
        [JsonPropertyName("value_text")]
        public string? ValueText { get; init; }

        /// <summary>Unit of <see cref="ValueNumeric"/>.</summary>
        [JsonPropertyName("unit")]
        public string? Unit { get; init; }

        /// <summary>FHIR status of the observation.</summary>
        [JsonPropertyName("status")]
        public string? Status { get; init; }

        /// <summary>Code identifying what was measured.</summary>
        [JsonPropertyName("code")]
        public string? Code { get; init; }
    }
}

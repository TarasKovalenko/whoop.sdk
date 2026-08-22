using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Whoop.Sdk.Endpoints;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;
using Xunit;

namespace Whoop.Sdk.Tests.Endpoints;

/// <summary>
/// Verifies that every endpoint client maps onto the verb and path published in the WHOOP OpenAPI
/// document. The connection is substituted so these tests describe routing only.
/// </summary>
public sealed class EndpointRoutingTests
{
    private static readonly Guid SampleId = Guid.Parse("ecfc6a15-4661-442f-a9a4-f160dd7afae8");

    private readonly IWhoopApiConnection _connection = Substitute.For<IWhoopApiConnection>();

    [Fact]
    public async Task Cycle_endpoints_use_the_documented_paths()
    {
        var client = new CycleClient(_connection);
        Returns<Cycle>();
        Returns<Sleep>();
        Returns<Recovery>();
        Returns<PaginatedResponse<Cycle>>();

        await client.GetAsync(93845);
        await AssertRoute<Cycle>(HttpMethod.Get, "v2/cycle/93845");

        await client.GetSleepAsync(93845);
        await AssertRoute<Sleep>(HttpMethod.Get, "v2/cycle/93845/sleep");

        await client.GetRecoveryAsync(93845);
        await AssertRoute<Recovery>(HttpMethod.Get, "v2/cycle/93845/recovery");

        await client.ListAsync();
        await AssertRoute<PaginatedResponse<Cycle>>(HttpMethod.Get, "v2/cycle");
    }

    [Fact]
    public async Task Recovery_endpoints_use_the_documented_paths()
    {
        var client = new RecoveryClient(_connection);
        Returns<PaginatedResponse<Recovery>>();
        Returns<Recovery>();

        await client.ListAsync();
        await AssertRoute<PaginatedResponse<Recovery>>(HttpMethod.Get, "v2/recovery");

        await client.GetForCycleAsync(93845);
        await AssertRoute<Recovery>(HttpMethod.Get, "v2/cycle/93845/recovery");
    }

    [Fact]
    public async Task Sleep_endpoints_use_the_documented_paths()
    {
        var client = new SleepClient(_connection);
        Returns<Sleep>();
        Returns<PaginatedResponse<Sleep>>();

        await client.GetAsync(SampleId);
        await AssertRoute<Sleep>(HttpMethod.Get, $"v2/activity/sleep/{SampleId:D}");

        await client.ListAsync();
        await AssertRoute<PaginatedResponse<Sleep>>(HttpMethod.Get, "v2/activity/sleep");

        await client.GetForCycleAsync(1);
        await AssertRoute<Sleep>(HttpMethod.Get, "v2/cycle/1/sleep");
    }

    [Fact]
    public async Task Workout_endpoints_use_the_documented_paths()
    {
        var client = new WorkoutClient(_connection);
        Returns<Workout>();
        Returns<PaginatedResponse<Workout>>();

        await client.GetAsync(SampleId);
        await AssertRoute<Workout>(HttpMethod.Get, $"v2/activity/workout/{SampleId:D}");

        await client.ListAsync();
        await AssertRoute<PaginatedResponse<Workout>>(HttpMethod.Get, "v2/activity/workout");
    }

    [Fact]
    public async Task User_endpoints_use_the_documented_paths()
    {
        var client = new UserClient(_connection);
        Returns<UserBasicProfile>();
        Returns<UserBodyMeasurement>();

        await client.GetBasicProfileAsync();
        await AssertRoute<UserBasicProfile>(HttpMethod.Get, "v2/user/profile/basic");

        await client.GetBodyMeasurementAsync();
        await AssertRoute<UserBodyMeasurement>(HttpMethod.Get, "v2/user/measurement/body");

        await client.RevokeAccessAsync();
        await _connection.Received(1).SendAsync(
            HttpMethod.Delete,
            "v2/user/access",
            Arg.Any<IEnumerable<KeyValuePair<string, string?>>?>(),
            Arg.Is<object?>(body => body == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activity_mapping_uses_the_v1_path()
    {
        var client = new ActivityMappingClient(_connection);
        Returns<ActivityIdMapping>();

        await client.GetAsync(1043);

        await AssertRoute<ActivityIdMapping>(HttpMethod.Get, "v1/activity-mapping/1043");
    }

    [Fact]
    public async Task Partner_endpoints_use_the_documented_verbs_and_paths()
    {
        var client = new PartnerClient(_connection);
        Returns<PartnerTokenResponse>();
        Returns<LabRequisition>();
        Returns<ServiceRequest>();

        var status = new ServiceRequestStatusRequest { TaskBusinessStatus = "DONE" };

        await client.RequestTokenAsync(new PartnerTokenRequest { ClientId = "id", ClientSecret = "secret" });
        await AssertRoute<PartnerTokenResponse>(HttpMethod.Post, "v2/partner/token");

        await client.GetLabRequisitionAsync(SampleId);
        await AssertRoute<LabRequisition>(HttpMethod.Get, $"v2/partner/requisition/{SampleId:D}");

        await client.GetServiceRequestAsync(SampleId);
        await AssertRoute<ServiceRequest>(HttpMethod.Get, $"v2/partner/service-request/{SampleId:D}");

        await client.UpdateServiceRequestStatusAsync(SampleId, status);
        await AssertRoute<ServiceRequest>(new HttpMethod("PATCH"), $"v2/partner/service-request/{SampleId:D}/status");

        await client.UpdateLabRequisitionStatusAsync(SampleId, status);
        await AssertVoidRoute(new HttpMethod("PATCH"), $"v2/partner/requisition/{SampleId:D}/status");

        await client.UploadDiagnosticReportResultsAsync(SampleId, new DiagnosticReportCreateRequest());
        await AssertVoidRoute(HttpMethod.Post, $"v2/partner/service-request/{SampleId:D}/results");

        await client.AddTestDataAsync();
        await AssertVoidRoute(HttpMethod.Post, "v2/partner/development/add-test-data");
    }

    [Fact]
    public async Task Partner_endpoints_send_the_supplied_body()
    {
        var client = new PartnerClient(_connection);
        Returns<PartnerTokenResponse>();
        var request = new PartnerTokenRequest { ClientId = "id", ClientSecret = "secret" };

        await client.RequestTokenAsync(request);

        await _connection.Received(1).SendAsync<PartnerTokenResponse>(
            HttpMethod.Post,
            "v2/partner/token",
            Arg.Any<IEnumerable<KeyValuePair<string, string?>>?>(),
            request,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Partner_endpoints_reject_a_null_body()
    {
        var client = new PartnerClient(_connection);

        await Should.ThrowAsync<ArgumentNullException>(() => client.RequestTokenAsync(null!));
        await Should.ThrowAsync<ArgumentNullException>(() => client.UpdateLabRequisitionStatusAsync(SampleId, null!));
        await Should.ThrowAsync<ArgumentNullException>(() => client.UpdateServiceRequestStatusAsync(SampleId, null!));
        await Should.ThrowAsync<ArgumentNullException>(() => client.UploadDiagnosticReportResultsAsync(SampleId, null!));
    }

    [Fact]
    public async Task Collection_requests_are_translated_into_query_parameters()
    {
        var client = new CycleClient(_connection);
        Returns<PaginatedResponse<Cycle>>();

        var request = new WhoopCollectionRequest
        {
            Limit = 25,
            Start = new DateTimeOffset(2024, 1, 2, 3, 4, 5, 678, TimeSpan.Zero),
            End = new DateTimeOffset(2024, 2, 2, 3, 4, 5, 678, TimeSpan.Zero),
            NextToken = "cursor",
        };

        await client.ListAsync(request);

        var query = (IEnumerable<KeyValuePair<string, string?>>)_connection
            .ReceivedCalls()
            .Last()
            .GetArguments()[2]!;

        query.ShouldBe(new[]
        {
            new KeyValuePair<string, string?>("limit", "25"),
            new KeyValuePair<string, string?>("start", "2024-01-02T03:04:05.678Z"),
            new KeyValuePair<string, string?>("end", "2024-02-02T03:04:05.678Z"),
            new KeyValuePair<string, string?>("nextToken", "cursor"),
        });
    }

    [Fact]
    public void Every_endpoint_client_rejects_a_null_connection()
    {
        Should.Throw<ArgumentNullException>(() => new CycleClient(null!));
        Should.Throw<ArgumentNullException>(() => new RecoveryClient(null!));
        Should.Throw<ArgumentNullException>(() => new SleepClient(null!));
        Should.Throw<ArgumentNullException>(() => new WorkoutClient(null!));
        Should.Throw<ArgumentNullException>(() => new UserClient(null!));
        Should.Throw<ArgumentNullException>(() => new PartnerClient(null!));
        Should.Throw<ArgumentNullException>(() => new ActivityMappingClient(null!));
    }

    private void Returns<T>()
        where T : new() =>
        _connection.SendAsync<T>(
                Arg.Any<HttpMethod>(),
                Arg.Any<string>(),
                Arg.Any<IEnumerable<KeyValuePair<string, string?>>?>(),
                Arg.Any<object?>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new T()));

    private Task<T> AssertRoute<T>(HttpMethod method, string path) =>
        _connection.Received(1).SendAsync<T>(
            method,
            path,
            Arg.Any<IEnumerable<KeyValuePair<string, string?>>?>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());

    private Task AssertVoidRoute(HttpMethod method, string path) =>
        _connection.Received(1).SendAsync(
            method,
            path,
            Arg.Any<IEnumerable<KeyValuePair<string, string?>>?>(),
            Arg.Any<object?>(),
            Arg.Any<CancellationToken>());
}

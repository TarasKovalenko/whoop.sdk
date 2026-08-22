using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Whoop.Sdk.Http;
using Whoop.Sdk.Models;
using Whoop.Sdk.Tests.TestSupport;
using Xunit;

namespace Whoop.Sdk.Tests.Http;

public sealed class WhoopApiConnectionTests
{
    private static readonly Uri BaseAddress = new("https://api.prod.whoop.com/developer/");

    [Fact]
    public void Constructor_rejects_a_client_without_a_base_address()
    {
        using var httpClient = new HttpClient();

        var exception = Should.Throw<ArgumentException>(() => new WhoopApiConnection(httpClient));

        exception.ParamName.ShouldBe("httpClient");
    }

    [Fact]
    public void Constructor_rejects_a_null_client() =>
        Should.Throw<ArgumentNullException>(() => new WhoopApiConnection(null!));

    [Fact]
    public async Task Resolves_relative_paths_against_the_base_address()
    {
        var (connection, handler) = CreateConnection();
        handler.RespondWithJson(SampleJson.Cycle);

        await connection.SendAsync<Cycle>(HttpMethod.Get, "v2/cycle/93845", null, null, CancellationToken.None);

        handler.LastRequest.RequestUri.ShouldBe(new Uri("https://api.prod.whoop.com/developer/v2/cycle/93845"));
    }

    [Fact]
    public async Task Appends_the_query_string_and_skips_null_values()
    {
        var (connection, handler) = CreateConnection();
        handler.RespondWithJson(SampleJson.CyclePage);

        var query = new[]
        {
            new KeyValuePair<string, string?>("limit", "25"),
            new KeyValuePair<string, string?>("start", null),
            new KeyValuePair<string, string?>("nextToken", "abc=="),
        };

        await connection.SendAsync<PaginatedResponse<Cycle>>(HttpMethod.Get, "v2/cycle", query, null, CancellationToken.None);

        handler.LastRequest.Query.ShouldBe("?limit=25&nextToken=abc%3D%3D");
    }

    [Fact]
    public async Task Serializes_the_request_body_as_json()
    {
        var (connection, handler) = CreateConnection();
        handler.RespondWithStatus(HttpStatusCode.NoContent);

        var body = new ServiceRequestStatusRequest { TaskBusinessStatus = "SAMPLE_COLLECTED", Reason = null };

        await connection.SendAsync(WhoopHttpMethodsProbe.Patch, "v2/partner/requisition/x/status", null, body, CancellationToken.None);

        var request = handler.LastRequest;
        request.Method.Method.ShouldBe("PATCH");
        request.ContentType.ShouldBe("application/json");
        // Null members are omitted so that PATCH bodies only carry the fields the caller set.
        request.Body.ShouldBe("""{"task_business_status":"SAMPLE_COLLECTED"}""");
    }

    [Fact]
    public async Task Sends_no_body_when_none_was_supplied()
    {
        var (connection, handler) = CreateConnection();
        handler.RespondWithStatus(HttpStatusCode.NoContent);

        await connection.SendAsync(HttpMethod.Delete, "v2/user/access", null, null, CancellationToken.None);

        handler.LastRequest.Body.ShouldBeNull();
    }

    [Fact]
    public async Task Throws_a_typed_exception_for_error_statuses()
    {
        var (connection, handler) = CreateConnection();
        handler.RespondWithStatus(HttpStatusCode.NotFound, """{"message":"not found"}""");

        var exception = await Should.ThrowAsync<WhoopApiException>(
            connection.SendAsync<Cycle>(HttpMethod.Get, "v2/cycle/1", null, null, CancellationToken.None));

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        exception.IsNotFound.ShouldBeTrue();
        exception.IsAuthenticationFailure.ShouldBeFalse();
        exception.ResponseBody.ShouldBe("""{"message":"not found"}""");
        exception.Method.ShouldBe("GET");
        exception.RequestUri.ShouldBe(new Uri("https://api.prod.whoop.com/developer/v2/cycle/1"));
    }

    [Fact]
    public async Task Flags_unauthorized_responses_as_authentication_failures()
    {
        var (connection, handler) = CreateConnection();
        handler.RespondWithStatus(HttpStatusCode.Unauthorized);

        var exception = await Should.ThrowAsync<WhoopApiException>(
            connection.SendAsync<Cycle>(HttpMethod.Get, "v2/cycle/1", null, null, CancellationToken.None));

        exception.IsAuthenticationFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Maps_429_to_a_rate_limit_exception_and_surfaces_retry_after()
    {
        var (connection, handler) = CreateConnection();
        var response = new HttpResponseMessage((HttpStatusCode)429);
        response.Headers.Add("Retry-After", "30");
        handler.RespondWith(response);

        var exception = await Should.ThrowAsync<WhoopRateLimitExceededException>(
            connection.SendAsync<Cycle>(HttpMethod.Get, "v2/cycle/1", null, null, CancellationToken.None));

        exception.RetryAfter.ShouldBe(TimeSpan.FromSeconds(30));
        exception.ShouldBeAssignableTo<WhoopApiException>();
    }

    [Fact]
    public async Task Reports_an_empty_success_body_as_an_api_error()
    {
        var (connection, handler) = CreateConnection();
        handler.RespondWithJson("null");

        var exception = await Should.ThrowAsync<WhoopApiException>(
            connection.SendAsync<Cycle>(HttpMethod.Get, "v2/cycle/1", null, null, CancellationToken.None));

        exception.Message.ShouldContain("empty body");
    }

    [Fact]
    public async Task Wraps_malformed_json_with_the_underlying_parse_error()
    {
        var (connection, handler) = CreateConnection();
        handler.RespondWithJson("{ not json");

        var exception = await Should.ThrowAsync<WhoopApiException>(
            connection.SendAsync<Cycle>(HttpMethod.Get, "v2/cycle/1", null, null, CancellationToken.None));

        exception.InnerException.ShouldBeOfType<System.Text.Json.JsonException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Rejects_an_empty_path(string path)
    {
        var (connection, _) = CreateConnection();

        var exception = await Should.ThrowAsync<ArgumentException>(
            connection.SendAsync<Cycle>(HttpMethod.Get, path, null, null, CancellationToken.None));

        exception.ParamName.ShouldBe("path");
    }

    private static (WhoopApiConnection Connection, RecordingHttpMessageHandler Handler) CreateConnection()
    {
        var handler = new RecordingHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = BaseAddress };
        return (new WhoopApiConnection(httpClient), handler);
    }
}

/// <summary>The library keeps its PATCH verb internal, so tests declare their own equivalent.</summary>
internal static class WhoopHttpMethodsProbe
{
    public static readonly HttpMethod Patch = new("PATCH");
}

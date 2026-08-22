using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Whoop.Sdk.Http;
using Whoop.Sdk.Tests.TestSupport;
using Xunit;

namespace Whoop.Sdk.Tests;

public sealed class WhoopClientTests
{
    [Fact]
    public void Exposes_a_client_for_every_resource_group()
    {
        using var client = new WhoopClient(Substitute.For<IWhoopApiConnection>());

        client.Cycles.ShouldNotBeNull();
        client.Recovery.ShouldNotBeNull();
        client.Sleep.ShouldNotBeNull();
        client.Workouts.ShouldNotBeNull();
        client.User.ShouldNotBeNull();
        client.Partner.ShouldNotBeNull();
        client.ActivityMappings.ShouldNotBeNull();
    }

    [Fact]
    public async Task Authenticates_requests_through_the_handler_pipeline()
    {
        var recorder = new RecordingHttpMessageHandler().RespondWithJson(SampleJson.BasicProfile);
        var options = new WhoopClientOptions
        {
            TokenProvider = new Whoop.Sdk.Authentication.StaticWhoopTokenProvider("token-abc"),
        };

        // The client owns its handler pipeline, so the recorder is injected through an HttpClient instead.
        using var httpClient = new HttpClient(
            new WhoopAuthenticationHandler(options.TokenProvider, recorder))
        {
            BaseAddress = WhoopClientOptions.DefaultBaseAddress,
        };

        using var client = new WhoopClient(httpClient);
        await client.User.GetBasicProfileAsync();

        recorder.LastRequest.Headers["Authorization"].ShouldBe("Bearer token-abc");
        recorder.LastRequest.RequestUri.AbsoluteUri
            .ShouldBe("https://api.prod.whoop.com/developer/v2/user/profile/basic");
    }

    [Fact]
    public void Applies_the_default_base_address_to_an_external_client()
    {
        using var httpClient = new HttpClient(new RecordingHttpMessageHandler());

        using var client = new WhoopClient(httpClient);

        httpClient.BaseAddress.ShouldBe(WhoopClientOptions.DefaultBaseAddress);
        client.Connection.ShouldBeOfType<WhoopApiConnection>();
    }

    [Fact]
    public void Keeps_a_base_address_that_was_already_configured()
    {
        var custom = new Uri("https://sandbox.example.com/developer/");
        using var httpClient = new HttpClient(new RecordingHttpMessageHandler()) { BaseAddress = custom };

        using var client = new WhoopClient(httpClient);

        httpClient.BaseAddress.ShouldBe(custom);
    }

    [Fact]
    public void Builds_an_owned_http_client_from_the_options()
    {
        var options = new WhoopClientOptions
        {
            BaseAddress = new Uri("https://sandbox.example.com/developer"),
            Timeout = TimeSpan.FromSeconds(7),
            UserAgent = "MyApp/2.1",
            TokenProvider = new Whoop.Sdk.Authentication.StaticWhoopTokenProvider("token"),
        };

        using var httpClient = WhoopClient.CreateHttpClient(options);

        httpClient.BaseAddress.ShouldBe(new Uri("https://sandbox.example.com/developer/"));
        httpClient.Timeout.ShouldBe(TimeSpan.FromSeconds(7));

        var userAgents = httpClient.DefaultRequestHeaders.UserAgent.Select(part => part.ToString()).ToList();
        userAgents[0].ShouldStartWith("Whoop.Sdk/");
        userAgents.ShouldContain("MyApp/2.1");
    }

    [Fact]
    public void Validates_its_arguments()
    {
        Should.Throw<ArgumentException>(() => new WhoopClient(string.Empty));
        Should.Throw<ArgumentNullException>(() => new WhoopClient((WhoopClientOptions)null!));
        Should.Throw<ArgumentNullException>(() => new WhoopClient((HttpClient)null!));
        Should.Throw<ArgumentNullException>(() => new WhoopClient((IWhoopApiConnection)null!));
    }

    [Fact]
    public void Disposing_twice_is_harmless()
    {
        var client = new WhoopClient("token");

        client.Dispose();
        Should.NotThrow(client.Dispose);
    }

    [Fact]
    public void Does_not_dispose_an_externally_owned_http_client()
    {
        var httpClient = new HttpClient(new RecordingHttpMessageHandler())
        {
            BaseAddress = WhoopClientOptions.DefaultBaseAddress,
        };

        using (var client = new WhoopClient(httpClient))
        {
            client.ShouldNotBeNull();
        }

        // Still usable: disposing the WhoopClient must not tear down a caller-managed pipeline.
        Should.NotThrow(() => httpClient.BaseAddress);
        httpClient.Dispose();
    }
}

public sealed class WhoopClientOptionsTests
{
    [Fact]
    public void Defaults_to_the_production_developer_api() =>
        new WhoopClientOptions().BaseAddress.ShouldBe(new Uri("https://api.prod.whoop.com/developer/"));

    [Fact]
    public void Appends_a_trailing_slash_so_relative_paths_are_not_swallowed()
    {
        var options = new WhoopClientOptions { BaseAddress = new Uri("https://example.com/developer") };

        options.BaseAddress.AbsoluteUri.ShouldBe("https://example.com/developer/");
    }

    [Fact]
    public void Rejects_a_relative_base_address() =>
        Should.Throw<ArgumentException>(() =>
            new WhoopClientOptions { BaseAddress = new Uri("/developer", UriKind.Relative) });

    [Fact]
    public void Rejects_a_null_base_address() =>
        Should.Throw<ArgumentNullException>(() => new WhoopClientOptions { BaseAddress = null! });
}

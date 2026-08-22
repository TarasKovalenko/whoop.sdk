using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Http;
using Whoop.Sdk.Tests.TestSupport;
using Xunit;

namespace Whoop.Sdk.Tests.Authentication;

public sealed class WhoopAuthenticationHandlerTests
{
    private static readonly Uri BaseAddress = new("https://api.prod.whoop.com/developer/");

    [Fact]
    public async Task Adds_a_bearer_token_from_the_provider()
    {
        var provider = Substitute.For<IWhoopTokenProvider>();
        provider.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("token-123");

        var (httpClient, recorder) = CreateClient(provider);
        using (httpClient)
        {
            await httpClient.GetAsync("v2/user/profile/basic");
        }

        recorder.LastRequest.Headers["Authorization"].ShouldBe("Bearer token-123");
    }

    [Fact]
    public async Task Asks_the_provider_on_every_request_so_refreshes_are_observed()
    {
        var provider = Substitute.For<IWhoopTokenProvider>();
        provider.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("first", "second");

        var (httpClient, recorder) = CreateClient(provider);
        using (httpClient)
        {
            await httpClient.GetAsync("v2/user/profile/basic");
            await httpClient.GetAsync("v2/user/profile/basic");
        }

        recorder.Requests[0].Headers["Authorization"].ShouldBe("Bearer first");
        recorder.Requests[1].Headers["Authorization"].ShouldBe("Bearer second");
        await provider.Received(2).GetAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Leaves_an_existing_authorization_header_alone()
    {
        var provider = Substitute.For<IWhoopTokenProvider>();
        provider.GetAccessTokenAsync(Arg.Any<CancellationToken>()).Returns("token-123");

        var (httpClient, recorder) = CreateClient(provider);
        using (httpClient)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "v2/user/profile/basic");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "explicit");
            await httpClient.SendAsync(request);
        }

        recorder.LastRequest.Headers["Authorization"].ShouldBe("Bearer explicit");
        await provider.DidNotReceive().GetAccessTokenAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Rejects_a_null_provider() =>
        Should.Throw<ArgumentNullException>(() => new WhoopAuthenticationHandler(null!));

    private static (HttpClient Client, RecordingHttpMessageHandler Recorder) CreateClient(IWhoopTokenProvider provider)
    {
        var recorder = new RecordingHttpMessageHandler();
        var handler = new WhoopAuthenticationHandler(provider, recorder);
        return (new HttpClient(handler) { BaseAddress = BaseAddress }, recorder);
    }
}

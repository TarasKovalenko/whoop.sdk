using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Extensions.DependencyInjection;
using Whoop.Sdk.Tests.TestSupport;
using Xunit;

namespace Whoop.Sdk.Tests.DependencyInjection;

public sealed class WhoopServiceCollectionExtensionsTests
{
    [Fact]
    public void Registers_a_resolvable_client()
    {
        var services = new ServiceCollection();
        services.AddWhoopAccessToken("token");
        services.AddWhoopClient();

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IWhoopClient>();

        client.ShouldBeOfType<WhoopClient>();
        client.Cycles.ShouldNotBeNull();
    }

    [Fact]
    public async Task Wires_the_base_address_and_the_bearer_token()
    {
        var recorder = new RecordingHttpMessageHandler().RespondWithJson(SampleJson.BasicProfile);
        var services = new ServiceCollection();
        services.AddWhoopAccessToken("token-xyz");
        services
            .AddWhoopClient()
            .ConfigurePrimaryHttpMessageHandler(() => recorder);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IWhoopClient>();

        await client.User.GetBasicProfileAsync();

        recorder.LastRequest.RequestUri.AbsoluteUri
            .ShouldBe("https://api.prod.whoop.com/developer/v2/user/profile/basic");
        recorder.LastRequest.Headers["Authorization"].ShouldBe("Bearer token-xyz");
    }

    [Fact]
    public async Task Honours_option_overrides()
    {
        var recorder = new RecordingHttpMessageHandler().RespondWithJson(SampleJson.BasicProfile);
        var services = new ServiceCollection();
        services.AddWhoopAccessToken("token");
        services
            .AddWhoopClient(options =>
            {
                options.BaseAddress = new Uri("https://sandbox.example.com/developer");
                options.UserAgent = "MyApp/3.0";
            })
            .ConfigurePrimaryHttpMessageHandler(() => recorder);

        using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<IWhoopClient>().User.GetBasicProfileAsync();

        recorder.LastRequest.RequestUri.AbsoluteUri
            .ShouldBe("https://sandbox.example.com/developer/v2/user/profile/basic");
        recorder.LastRequest.Headers["User-Agent"].ShouldBe("MyApp/3.0");
    }

    [Fact]
    public void Fails_with_a_clear_error_when_no_token_provider_is_registered()
    {
        var services = new ServiceCollection();
        services.AddWhoopClient();

        using var provider = services.BuildServiceProvider();

        Should.Throw<InvalidOperationException>(() => provider.GetRequiredService<IWhoopClient>())
            .Message.ShouldContain(nameof(IWhoopTokenProvider));
    }

    [Fact]
    public void Registers_the_oauth_client_with_its_own_transport()
    {
        var services = new ServiceCollection();
        services.AddWhoopOAuth("client-id", "client-secret");

        using var provider = services.BuildServiceProvider();
        var oauth = provider.GetRequiredService<WhoopOAuthClient>();

        oauth.TokenEndpoint.ShouldBe(WhoopOAuthClient.DefaultTokenEndpoint);
        oauth.AuthorizationEndpoint.ShouldBe(WhoopOAuthClient.DefaultAuthorizationEndpoint);
    }

    [Fact]
    public async Task Partner_authentication_acquires_tokens_over_a_separate_pipeline()
    {
        var tokenTransport = new RecordingHttpMessageHandler().RespondWithJson(SampleJson.PartnerToken);
        var apiTransport = new RecordingHttpMessageHandler().RespondWithJson(SampleJson.ServiceRequest);

        var services = new ServiceCollection();
        services.AddWhoopPartnerAuthentication("partner-id", "partner-secret");
        services.AddHttpClient("Whoop.PartnerToken").ConfigurePrimaryHttpMessageHandler(() => tokenTransport);
        services.AddWhoopClient().ConfigurePrimaryHttpMessageHandler(() => apiTransport);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IWhoopClient>();

        await client.Partner.GetServiceRequestAsync(Guid.Parse("1a2b3c4d-5e6f-4a7b-8c9d-0e1f2a3b4c5d"));

        tokenTransport.LastRequest.Path.ShouldBe("/developer/v2/partner/token");
        tokenTransport.LastRequest.Body!.ShouldContain("partner-secret");
        apiTransport.LastRequest.Headers["Authorization"].ShouldBe("Bearer partner-token");
    }

    [Fact]
    public void Every_registration_helper_rejects_a_null_collection()
    {
        Should.Throw<ArgumentNullException>(() => ((IServiceCollection)null!).AddWhoopClient());
        Should.Throw<ArgumentNullException>(() => ((IServiceCollection)null!).AddWhoopAccessToken("token"));
        Should.Throw<ArgumentNullException>(() => ((IServiceCollection)null!).AddWhoopOAuth("id", "secret"));
        Should.Throw<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddWhoopPartnerAuthentication("id", "secret"));
    }
}

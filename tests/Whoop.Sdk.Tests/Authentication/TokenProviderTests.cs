using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Endpoints;
using Whoop.Sdk.Models;
using Xunit;

namespace Whoop.Sdk.Tests.Authentication;

public sealed class StaticWhoopTokenProviderTests
{
    [Fact]
    public async Task Returns_the_configured_token() =>
        (await new StaticWhoopTokenProvider("abc").GetAccessTokenAsync()).ShouldBe("abc");

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Rejects_a_blank_token(string token) =>
        Should.Throw<ArgumentException>(() => new StaticWhoopTokenProvider(token));
}

public sealed class DelegateWhoopTokenProviderTests
{
    [Fact]
    public async Task Defers_to_the_callback_on_every_call()
    {
        var calls = 0;
        var provider = new DelegateWhoopTokenProvider(_ =>
        {
            calls++;
            return Task.FromResult($"token-{calls}");
        });

        (await provider.GetAccessTokenAsync()).ShouldBe("token-1");
        (await provider.GetAccessTokenAsync()).ShouldBe("token-2");
    }

    [Fact]
    public void Rejects_a_null_callback() =>
        Should.Throw<ArgumentNullException>(() => new DelegateWhoopTokenProvider(null!));
}

public sealed class PartnerWhoopTokenProviderTests
{
    [Fact]
    public async Task Requests_a_token_once_and_caches_it()
    {
        var partner = Substitute.For<IPartnerClient>();
        partner.RequestTokenAsync(Arg.Any<PartnerTokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PartnerTokenResponse { AccessToken = "partner-token", ExpiresIn = 3600 });

        using var provider = new PartnerWhoopTokenProvider(partner, "id", "secret");

        (await provider.GetAccessTokenAsync()).ShouldBe("partner-token");
        (await provider.GetAccessTokenAsync()).ShouldBe("partner-token");

        await partner.Received(1).RequestTokenAsync(Arg.Any<PartnerTokenRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sends_the_credentials_with_the_client_credentials_grant()
    {
        var partner = Substitute.For<IPartnerClient>();
        partner.RequestTokenAsync(Arg.Any<PartnerTokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PartnerTokenResponse { AccessToken = "partner-token", ExpiresIn = 3600 });

        using var provider = new PartnerWhoopTokenProvider(partner, "id", "secret");
        await provider.GetAccessTokenAsync();

        var request = (PartnerTokenRequest)partner.ReceivedCalls().Single().GetArguments()[0]!;
        request.ClientId.ShouldBe("id");
        request.ClientSecret.ShouldBe("secret");
        request.GrantType.ShouldBe("client_credentials");
        request.Scope.ShouldBe(WhoopScopes.PartnerToken);
    }

    [Fact]
    public async Task Requests_a_new_token_once_the_cached_one_is_within_the_clock_skew()
    {
        var partner = Substitute.For<IPartnerClient>();
        partner.RequestTokenAsync(Arg.Any<PartnerTokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new PartnerTokenResponse { AccessToken = "first", ExpiresIn = 30 },
                new PartnerTokenResponse { AccessToken = "second", ExpiresIn = 3600 });

        using var provider = new PartnerWhoopTokenProvider(partner, "id", "secret", TimeSpan.FromMinutes(1));

        (await provider.GetAccessTokenAsync()).ShouldBe("first");
        (await provider.GetAccessTokenAsync()).ShouldBe("second");
    }

    [Fact]
    public async Task Fails_loudly_when_the_endpoint_returns_no_token()
    {
        var partner = Substitute.For<IPartnerClient>();
        partner.RequestTokenAsync(Arg.Any<PartnerTokenRequest>(), Arg.Any<CancellationToken>())
            .Returns(new PartnerTokenResponse { AccessToken = null, ExpiresIn = 3600 });

        using var provider = new PartnerWhoopTokenProvider(partner, "id", "secret");

        await Should.ThrowAsync<InvalidOperationException>(() => provider.GetAccessTokenAsync());
    }

    [Fact]
    public void Validates_its_arguments()
    {
        var partner = Substitute.For<IPartnerClient>();

        Should.Throw<ArgumentException>(() => new PartnerWhoopTokenProvider(partner, "", "secret"));
        Should.Throw<ArgumentException>(() => new PartnerWhoopTokenProvider(partner, "id", ""));
        Should.Throw<ArgumentNullException>(() => new PartnerWhoopTokenProvider((IPartnerClient)null!, "id", "secret"));
    }
}

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Shouldly;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Tests.TestSupport;
using Xunit;

namespace Whoop.Sdk.Tests.Authentication;

public sealed class RefreshingWhoopTokenProviderTests
{
    [Fact]
    public async Task Refreshes_when_it_has_no_token_yet()
    {
        var (oauth, handler) = CreateOAuthClient();
        handler.RespondWithJson(TokenJson("access-1", "refresh-1", 3600));

        using var provider = new RefreshingWhoopTokenProvider(oauth, "refresh-0");

        (await provider.GetAccessTokenAsync()).ShouldBe("access-1");
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Reuses_a_token_that_is_still_valid()
    {
        var (oauth, handler) = CreateOAuthClient();
        var initial = new WhoopOAuthToken { AccessToken = "seeded", ExpiresIn = 3600 };

        using var provider = new RefreshingWhoopTokenProvider(oauth, "refresh-0", initial);

        (await provider.GetAccessTokenAsync()).ShouldBe("seeded");
        (await provider.GetAccessTokenAsync()).ShouldBe("seeded");
        handler.Requests.ShouldBeEmpty();
    }

    [Fact]
    public async Task Refreshes_a_token_that_is_inside_the_clock_skew()
    {
        var (oauth, handler) = CreateOAuthClient();
        handler.RespondWithJson(TokenJson("access-2", "refresh-2", 3600));
        var nearlyExpired = new WhoopOAuthToken { AccessToken = "stale", ExpiresIn = 30 };

        using var provider = new RefreshingWhoopTokenProvider(
            oauth,
            "refresh-0",
            nearlyExpired,
            clockSkew: TimeSpan.FromMinutes(1));

        (await provider.GetAccessTokenAsync()).ShouldBe("access-2");
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Rotates_the_refresh_token_that_later_refreshes_use()
    {
        var (oauth, handler) = CreateOAuthClient();
        handler.RespondWithJson(TokenJson("access-1", "refresh-1", 3600));

        using var provider = new RefreshingWhoopTokenProvider(oauth, "refresh-0");
        await provider.GetAccessTokenAsync();

        provider.CurrentRefreshToken.ShouldBe("refresh-1");
        handler.LastRequest.Body!.ShouldContain("refresh_token=refresh-0");
    }

    [Fact]
    public async Task Notifies_the_caller_so_the_rotated_token_can_be_persisted()
    {
        var (oauth, handler) = CreateOAuthClient();
        handler.RespondWithJson(TokenJson("access-1", "refresh-1", 3600));

        WhoopOAuthToken? persisted = null;
        using var provider = new RefreshingWhoopTokenProvider(
            oauth,
            "refresh-0",
            onTokenRefreshed: (token, _) =>
            {
                persisted = token;
                return Task.CompletedTask;
            });

        await provider.GetAccessTokenAsync();

        persisted.ShouldNotBeNull();
        persisted!.RefreshToken.ShouldBe("refresh-1");
    }

    [Fact]
    public async Task Refreshes_only_once_when_many_callers_race()
    {
        var (oauth, handler) = CreateOAuthClient();
        handler.RespondWithJson(TokenJson("access-1", "refresh-1", 3600));

        using var provider = new RefreshingWhoopTokenProvider(oauth, "refresh-0");

        var tokens = await Task.WhenAll(Enumerable
            .Range(0, 16)
            .Select(_ => Task.Run(() => provider.GetAccessTokenAsync())));

        tokens.ShouldAllBe(token => token == "access-1");
        handler.Requests.Count.ShouldBe(1);
    }

    [Fact]
    public void Validates_its_arguments()
    {
        var (oauth, _) = CreateOAuthClient();

        Should.Throw<ArgumentException>(() => new RefreshingWhoopTokenProvider(oauth, ""));
        Should.Throw<ArgumentNullException>(() => new RefreshingWhoopTokenProvider(null!, "refresh"));
    }

    [Fact]
    public void Disposing_twice_is_harmless()
    {
        var (oauth, _) = CreateOAuthClient();
        var provider = new RefreshingWhoopTokenProvider(oauth, "refresh-0");

        provider.Dispose();
        Should.NotThrow(() => provider.Dispose());
    }

    private static string TokenJson(string accessToken, string refreshToken, int expiresIn) =>
        $$"""
        {
          "access_token": "{{accessToken}}",
          "refresh_token": "{{refreshToken}}",
          "expires_in": {{expiresIn}},
          "token_type": "bearer"
        }
        """;

    private static (WhoopOAuthClient Client, RecordingHttpMessageHandler Handler) CreateOAuthClient()
    {
        var handler = new RecordingHttpMessageHandler();
        return (new WhoopOAuthClient(new HttpClient(handler), "client-id", "client-secret"), handler);
    }
}

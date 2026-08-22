using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Shouldly;
using Whoop.Sdk.Authentication;
using Whoop.Sdk.Tests.TestSupport;
using Xunit;

namespace Whoop.Sdk.Tests.Authentication;

public sealed class WhoopOAuthClientTests
{
    private static readonly Uri RedirectUri = new("https://example.com/callback");

    private const string TokenJson = """
        {
          "access_token": "access-1",
          "refresh_token": "refresh-1",
          "expires_in": 3600,
          "token_type": "bearer",
          "scope": "offline read:profile"
        }
        """;

    [Fact]
    public void Builds_an_authorization_url_with_every_required_parameter()
    {
        var (client, _) = CreateClient();

        var url = client.CreateAuthorizationUrl(
            RedirectUri,
            new[] { WhoopScopes.ReadProfile, WhoopScopes.Offline },
            "state-value");

        url.GetLeftPart(UriPartial.Path).ShouldBe("https://api.prod.whoop.com/oauth/oauth2/auth");
        url.Query.ShouldBe(
            "?client_id=client-id" +
            "&redirect_uri=https%3A%2F%2Fexample.com%2Fcallback" +
            "&response_type=code" +
            "&scope=read%3Aprofile%20offline" +
            "&state=state-value");
    }

    [Fact]
    public void Rejects_a_state_value_shorter_than_the_documented_minimum()
    {
        var (client, _) = CreateClient();

        Should.Throw<ArgumentException>(() =>
            client.CreateAuthorizationUrl(RedirectUri, new[] { WhoopScopes.ReadProfile }, "short"));
    }

    [Fact]
    public void Rejects_an_empty_scope_list()
    {
        var (client, _) = CreateClient();

        Should.Throw<ArgumentException>(() =>
            client.CreateAuthorizationUrl(RedirectUri, Array.Empty<string>(), "state-value"));
    }

    [Fact]
    public async Task Exchanges_an_authorization_code_for_tokens()
    {
        var (client, handler) = CreateClient();
        handler.RespondWithJson(TokenJson);

        var token = await client.ExchangeAuthorizationCodeAsync("the-code", RedirectUri);

        var request = handler.LastRequest;
        request.Method.ShouldBe(HttpMethod.Post);
        request.RequestUri.ShouldBe(new Uri("https://api.prod.whoop.com/oauth/oauth2/token"));
        request.ContentType.ShouldBe("application/x-www-form-urlencoded");
        ParseForm(request.Body!).ShouldBe(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = "the-code",
            ["redirect_uri"] = "https://example.com/callback",
            ["client_id"] = "client-id",
            ["client_secret"] = "client-secret",
        });

        token.AccessToken.ShouldBe("access-1");
        token.RefreshToken.ShouldBe("refresh-1");
        token.ExpiresIn.ShouldBe(3600);
        token.Scope.ShouldBe("offline read:profile");
        token.ExpiresAt.ShouldBe(token.ObtainedAt.AddHours(1));
    }

    [Fact]
    public async Task Refreshes_with_the_offline_scope()
    {
        var (client, handler) = CreateClient();
        handler.RespondWithJson(TokenJson);

        await client.RefreshTokenAsync("refresh-0");

        ParseForm(handler.LastRequest.Body!).ShouldBe(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = "refresh-0",
            ["scope"] = "offline",
            ["client_id"] = "client-id",
            ["client_secret"] = "client-secret",
        });
    }

    [Fact]
    public async Task Surfaces_token_endpoint_failures_as_api_exceptions()
    {
        var (client, handler) = CreateClient();
        handler.RespondWithStatus(HttpStatusCode.Unauthorized, """{"error":"invalid_client"}""");

        var exception = await Should.ThrowAsync<WhoopApiException>(() => client.RefreshTokenAsync("refresh-0"));

        exception.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        exception.ResponseBody.ShouldBe("""{"error":"invalid_client"}""");
        exception.IsAuthenticationFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Rejects_a_token_response_without_an_access_token()
    {
        var (client, handler) = CreateClient();
        handler.RespondWithJson("""{"expires_in":3600}""");

        var exception = await Should.ThrowAsync<WhoopApiException>(() => client.RefreshTokenAsync("refresh-0"));

        exception.Message.ShouldContain("without an access token");
    }

    [Fact]
    public void Validates_its_arguments()
    {
        using var httpClient = new HttpClient();

        Should.Throw<ArgumentException>(() => new WhoopOAuthClient(httpClient, "", "secret"));
        Should.Throw<ArgumentException>(() => new WhoopOAuthClient(httpClient, "id", ""));
        Should.Throw<ArgumentNullException>(() => new WhoopOAuthClient(null!, "id", "secret"));
    }

    [Fact]
    public async Task Rejects_a_blank_code_or_refresh_token()
    {
        var (client, _) = CreateClient();

        await Should.ThrowAsync<ArgumentException>(() => client.ExchangeAuthorizationCodeAsync(" ", RedirectUri));
        await Should.ThrowAsync<ArgumentException>(() => client.RefreshTokenAsync(" "));
    }

    private static (WhoopOAuthClient Client, RecordingHttpMessageHandler Handler) CreateClient()
    {
        var handler = new RecordingHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        return (new WhoopOAuthClient(httpClient, "client-id", "client-secret"), handler);
    }

    private static Dictionary<string, string> ParseForm(string body)
    {
        var form = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in body.Split('&'))
        {
            var parts = pair.Split('=');
            form[Uri.UnescapeDataString(parts[0])] = Uri.UnescapeDataString(parts[1].Replace('+', ' '));
        }

        return form;
    }
}

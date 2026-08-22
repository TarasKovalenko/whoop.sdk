using System.Collections.Concurrent;
using Whoop.Sdk.Authentication;

namespace Whoop.Sdk.Samples.OAuthWebApp;

/// <summary>
/// In-memory token storage keyed by browser session. A real application would use a database, and
/// would encrypt the refresh token at rest.
/// </summary>
public sealed class TokenSession
{
    private readonly ConcurrentDictionary<string, WhoopOAuthToken> _tokens = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, string> _pendingStates = new(StringComparer.Ordinal);

    public string CreateState()
    {
        // WHOOP requires at least eight characters, and the value guards against CSRF on the callback.
        var state = Guid.NewGuid().ToString("N");
        _pendingStates[state] = state;
        return state;
    }

    public bool ConsumeState(string? state) =>
        state is not null && _pendingStates.TryRemove(state, out _);

    public void Store(string sessionId, WhoopOAuthToken token) => _tokens[sessionId] = token;

    public WhoopOAuthToken? Find(string? sessionId) =>
        sessionId is not null && _tokens.TryGetValue(sessionId, out var token) ? token : null;
}

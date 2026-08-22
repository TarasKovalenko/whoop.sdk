namespace Whoop.Sdk.Samples.Worker;

/// <summary>
/// Stand-in for wherever you really keep refresh tokens. WHOOP rotates the refresh token on every
/// refresh and invalidates the previous one, so a real implementation must persist it durably or
/// the worker loses access on its next restart.
/// </summary>
public sealed class RefreshTokenStore(string initialRefreshToken)
{
    private string _refreshToken = initialRefreshToken;

    public string Current => Volatile.Read(ref _refreshToken);

    public Task SaveAsync(string refreshToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Volatile.Write(ref _refreshToken, refreshToken);
        return Task.CompletedTask;
    }
}

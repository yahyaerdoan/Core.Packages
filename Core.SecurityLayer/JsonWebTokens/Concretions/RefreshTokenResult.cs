namespace Core.SecurityLayer.JsonWebTokens.Concretions;

public class RefreshTokenResult(string rawToken, string hashedToken, DateTime expires)
{
    public string RawToken { get; } = rawToken;

    public string HashedToken { get; } = hashedToken;

    public DateTime Expires { get; } = expires;
}

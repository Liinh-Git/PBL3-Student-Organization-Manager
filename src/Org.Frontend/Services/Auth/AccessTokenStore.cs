namespace Org.Frontend.Services.Auth;

public interface IAccessTokenStore
{
    string? AccessToken { get; set; }
    DateTime? ExpiresAtUtc { get; set; }
}

public sealed class AccessTokenStore : IAccessTokenStore
{
    public string? AccessToken { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}

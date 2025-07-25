namespace Identity.Authentication.Models;

public class AuthenticationTokenResult
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
    public RefreshToken RefreshTokenEntity { get; set; } = default!;
}

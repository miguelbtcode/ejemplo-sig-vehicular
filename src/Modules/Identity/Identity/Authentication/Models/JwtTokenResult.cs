namespace Identity.Authentication.Models;

public class JwtTokenResult
{
    public string Token { get; set; } = default!;
    public DateTime Expiry { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public List<string> Roles { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}

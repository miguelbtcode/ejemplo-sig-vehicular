namespace Identity.Authentication.Models;

public class TokenInfo
{
    public string? JwtId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public TimeSpan TimeUntilExpiry => ExpiresAt.Subtract(DateTime.UtcNow);

    public bool IsNearExpiry(TimeSpan threshold) => TimeUntilExpiry <= threshold;
}

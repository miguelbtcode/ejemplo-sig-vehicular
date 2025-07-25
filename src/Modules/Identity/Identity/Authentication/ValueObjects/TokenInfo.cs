namespace Identity.Authentication.ValueObjects;

public record TokenInfo(string? JwtId, Guid? UserId, DateTime ExpiresAt)
{
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public bool IsNearExpiry(TimeSpan threshold) =>
        ExpiresAt.Subtract(DateTime.UtcNow) <= threshold;
}

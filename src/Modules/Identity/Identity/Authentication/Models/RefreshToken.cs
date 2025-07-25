namespace Identity.Authentication.Models;

public class RefreshToken : Entity<Guid>
{
    public string Token { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public string DeviceId { get; private set; } = default!;
    public string DeviceName { get; private set; } = default!;
    public string Platform { get; private set; } = default!; // "iOS"|"Android"|"web"
    public string? AppVersion { get; private set; }
    public string? UserAgent { get; private set; } // Para web
    public string? IpAddress { get; private set; } // Para web
    public DateTime LastUsed { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? RevokedReason { get; private set; }

    // Navigation property
    public User User { get; private set; } = default!;

    private RefreshToken() { } // EF Constructor

    // Factory method para Mobile
    public static RefreshToken CreateMobile(
        Guid userId,
        string deviceId,
        string deviceName,
        string platform,
        string appVersion,
        DateTime expiresAt
    )
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = GenerateSecureToken(),
            UserId = userId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            Platform = platform,
            AppVersion = appVersion,
            LastUsed = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false,
        };
    }

    // Factory method para Web
    public static RefreshToken CreateWeb(
        Guid userId,
        string deviceId,
        string deviceName,
        string userAgent,
        string? ipAddress,
        DateTime expiresAt
    )
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = GenerateSecureToken(),
            UserId = userId,
            DeviceId = deviceId,
            DeviceName = deviceName,
            Platform = "web",
            UserAgent = userAgent,
            IpAddress = ipAddress,
            LastUsed = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false,
        };
    }

    public void UpdateLastUsed()
    {
        LastUsed = DateTime.UtcNow;
    }

    public void Revoke(string reason = "User logout")
    {
        IsRevoked = true;
        RevokedReason = reason;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsRevoked && !IsExpired;
    public bool IsMobile => Platform != "web";

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

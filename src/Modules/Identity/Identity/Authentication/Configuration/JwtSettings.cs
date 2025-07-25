namespace Identity.Authentication.Configuration;

public class JwtSettings
{
    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiryHours { get; set; } = 1;
    public int RefreshTokenExpiryDays { get; set; } = 30;
    public int RefreshTokenMobileExpiryDays { get; set; } = 60;
}

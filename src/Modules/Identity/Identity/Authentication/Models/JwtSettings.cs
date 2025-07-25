namespace Identity.Authentication.Models;

public class JwtSettings
{
    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiryHours { get; set; } = 1; // Más corto por defecto
    public int RefreshTokenExpiryDays { get; set; } = 30;
    public bool EnableBlacklist { get; set; } = false;
}

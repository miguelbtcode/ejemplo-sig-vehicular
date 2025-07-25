namespace Identity.Authentication.Models;

public record ClientInfo
{
    public bool IsMobile { get; init; }
    public string DeviceId { get; init; } = default!;
    public string DeviceName { get; init; } = default!;
    public string Platform { get; init; } = default!;
    public string AppVersion { get; init; } = default!;
    public string UserAgent { get; init; } = default!;
    public string? IpAddress { get; init; }
}

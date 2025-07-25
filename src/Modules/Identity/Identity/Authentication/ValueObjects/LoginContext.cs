namespace Identity.Authentication.ValueObjects;

public record LoginContext(
    string Email,
    string Password,
    DeviceInfo Device,
    string? IpAddress = null,
    string? UserAgent = null
);

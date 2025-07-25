namespace Identity.Authentication.Dtos;

public record LoginRequestDto(
    string Email,
    string Password,
    string? DeviceId = null,
    string? DeviceName = null,
    string? AppVersion = null,
    string? Platform = null,
    string? UserAgent = null
);

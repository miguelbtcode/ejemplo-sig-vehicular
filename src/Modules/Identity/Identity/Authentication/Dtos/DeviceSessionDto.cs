namespace Identity.Authentication.Dtos;

public record DeviceSessionDto(
    string DeviceId,
    string DeviceName,
    string Platform,
    DateTime LastUsed,
    bool IsCurrent
);

namespace Identity.Authentication.Dtos.Common;

public record DeviceSessionDto(
    string DeviceId,
    string DeviceName,
    string Platform,
    DateTime LastUsed,
    bool IsCurrent
);

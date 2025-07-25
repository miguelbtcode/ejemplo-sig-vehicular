namespace Identity.Authentication.Dtos;

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    DateTime RefreshTokenExpiry,
    UserLoginDto User,
    List<DeviceSessionDto> ActiveSessions,
    string SessionType
);

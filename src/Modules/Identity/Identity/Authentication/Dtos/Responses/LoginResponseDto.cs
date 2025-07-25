using Identity.Authentication.Dtos.Common;

namespace Identity.Authentication.Dtos.Responses;

public record LoginResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    DateTime RefreshTokenExpiry,
    UserLoginDto User,
    List<DeviceSessionDto> ActiveSessions
);

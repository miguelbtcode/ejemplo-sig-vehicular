namespace Identity.Authentication.Dtos.Requests;

public record LogoutRequestDto(string RefreshToken, bool LogoutAllDevices = false);

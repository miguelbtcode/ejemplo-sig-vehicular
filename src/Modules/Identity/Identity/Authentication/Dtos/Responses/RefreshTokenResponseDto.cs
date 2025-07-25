namespace Identity.Authentication.Dtos.Responses;

public record RefreshTokenResponseDto(string AccessToken, DateTime ExpiresAt);

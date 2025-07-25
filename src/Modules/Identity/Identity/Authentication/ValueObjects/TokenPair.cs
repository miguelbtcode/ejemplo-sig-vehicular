namespace Identity.Authentication.ValueObjects;

public record TokenPair(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    DateTime RefreshTokenExpiry
);

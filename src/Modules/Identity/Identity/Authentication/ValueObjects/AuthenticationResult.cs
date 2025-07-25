using Identity.Authentication.Dtos.Common;

namespace Identity.Authentication.ValueObjects;

public class AuthenticationResult
{
    public bool IsSuccess { get; private set; }
    public TokenPair? TokenPair { get; private set; }
    public UserLoginDto? User { get; private set; }
    public List<DeviceSessionDto>? ActiveSessions { get; private set; }
    public Error? Error { get; private set; }

    private AuthenticationResult() { }

    public static AuthenticationResult Success(
        TokenPair tokenPair,
        UserLoginDto user,
        List<DeviceSessionDto> activeSessions
    ) =>
        new()
        {
            IsSuccess = true,
            TokenPair = tokenPair,
            User = user,
            ActiveSessions = activeSessions,
        };

    public static AuthenticationResult Failure(Error error) =>
        new() { IsSuccess = false, Error = error };
}

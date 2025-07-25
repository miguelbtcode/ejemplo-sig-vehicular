namespace Identity.Authentication.Models;

public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public AuthenticationTokenResult? TokenResult { get; set; }

    public static AuthenticationResult Success(AuthenticationTokenResult tokenResult) =>
        new() { IsSuccess = true, TokenResult = tokenResult };

    public static AuthenticationResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

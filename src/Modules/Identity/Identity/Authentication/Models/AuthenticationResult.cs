namespace Identity.Authentication.Models;

public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public JwtTokenResult? TokenResult { get; set; }

    public static AuthenticationResult Success(JwtTokenResult tokenResult) =>
        new() { IsSuccess = true, TokenResult = tokenResult };

    public static AuthenticationResult Failure(string errorMessage) =>
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

using Identity.Authentication.ValueObjects;

namespace Identity.Authentication.Services;

public interface IAuthenticationService
{
    Task<Result<AuthenticationResult>> AuthenticateAsync(LoginContext context);
}

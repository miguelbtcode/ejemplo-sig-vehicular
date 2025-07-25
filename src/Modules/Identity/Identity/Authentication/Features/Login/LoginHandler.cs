using Identity.Authentication.Dtos.Requests;
using Identity.Authentication.Dtos.Responses;
using Identity.Authentication.Services;
using Identity.Authentication.ValueObjects;

namespace Identity.Authentication.Features.Login;

public record LoginCommand(LoginRequestDto Request) : ICommand<Result<LoginResult>>;

public record LoginResult(LoginResponseDto Response);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Password).NotEmpty();
    }
}

internal class LoginHandler(
    IAuthenticationService authenticationService,
    IDeviceService deviceService,
    IHttpContextAccessor httpContextAccessor
) : ICommandHandler<LoginCommand, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Create login context
        var context = CreateLoginContext(command.Request);

        // 2. Authenticate
        var authResult = await authenticationService.AuthenticateAsync(context);
        if (authResult.IsFailure)
            return authResult.Error;

        // 3. Build response
        var response = CreateLoginResponse(authResult.Value);

        return new LoginResult(response);
    }

    private LoginContext CreateLoginContext(LoginRequestDto request)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var device = deviceService.DetectDevice(httpContext, request);
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        return new LoginContext(request.Email, request.Password, device, ipAddress, userAgent);
    }

    private static LoginResponseDto CreateLoginResponse(AuthenticationResult authResult)
    {
        return new LoginResponseDto(
            authResult.TokenPair!.AccessToken,
            authResult.TokenPair.RefreshToken,
            authResult.TokenPair.AccessTokenExpiry,
            authResult.TokenPair.RefreshTokenExpiry,
            authResult.User!,
            authResult.ActiveSessions!
        );
    }
}

namespace Identity.Authentication.Features.Logout;

public record LogoutCommand(string RefreshToken, bool LogoutAllDevices = false)
    : ICommand<Result<LogoutResult>>;

public record LogoutResult(bool Success);

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

internal class LogoutHandler(IdentityDbContext dbContext)
    : ICommandHandler<LogoutCommand, Result<LogoutResult>>
{
    public async Task<Result<LogoutResult>> HandleAsync(
        LogoutCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var refreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.Token == command.RefreshToken,
            cancellationToken
        );

        if (refreshToken == null)
            return new LogoutResult(false);

        if (command.LogoutAllDevices)
        {
            // Revoke all user sessions
            var allUserTokens = await dbContext
                .RefreshTokens.Where(rt => rt.UserId == refreshToken.UserId && !rt.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in allUserTokens)
            {
                token.Revoke("Logout all devices");
            }
        }
        else
        {
            // Revoke only current session
            refreshToken.Revoke("User logout");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new LogoutResult(true);
    }
}

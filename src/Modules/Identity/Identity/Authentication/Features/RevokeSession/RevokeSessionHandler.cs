namespace Identity.Authentication.Features.RevokeSession;

public record RevokeSessionCommand(Guid UserId, string DeviceId)
    : ICommand<Result<RevokeSessionResult>>;

public record RevokeSessionResult(bool Success);

public class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DeviceId).NotEmpty();
    }
}

internal class RevokeSessionHandler(IdentityDbContext dbContext)
    : ICommandHandler<RevokeSessionCommand, Result<RevokeSessionResult>>
{
    public async Task<Result<RevokeSessionResult>> HandleAsync(
        RevokeSessionCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var sessionsToRevoke = await dbContext
            .RefreshTokens.Where(rt =>
                rt.UserId == command.UserId && rt.DeviceId == command.DeviceId && !rt.IsRevoked
            )
            .ToListAsync(cancellationToken);

        if (!sessionsToRevoke.Any())
            return new RevokeSessionResult(false);

        foreach (var session in sessionsToRevoke)
        {
            session.Revoke("Session revoked by user");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new RevokeSessionResult(true);
    }
}

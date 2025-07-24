using Identity.Authentication.Services;
using Identity.Errors;

namespace Identity.Users.Features.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword)
    : ICommand<Result<ChangePasswordResult>>;

public record ChangePasswordResult(bool IsSuccess);

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6);
    }
}

internal class ChangePasswordHandler(IdentityDbContext dbContext, IPasswordHasher passwordHasher)
    : ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResult>>
{
    public async Task<Result<ChangePasswordResult>> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(
            u => u.Id == command.UserId,
            cancellationToken
        );

        if (user == null)
            return UserErrors.NotFound(command.UserId);

        // Verificar contraseña actual
        if (!passwordHasher.VerifyPassword(command.CurrentPassword, user.Password))
            return UserErrors.InvalidCredentials;

        // Cambiar contraseña
        var newHashedPassword = passwordHasher.HashPassword(command.NewPassword);
        user.ChangePassword(newHashedPassword);

        await dbContext.SaveChangesAsync(cancellationToken);
        return new ChangePasswordResult(true);
    }
}

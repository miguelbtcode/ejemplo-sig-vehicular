namespace Identity.Users.Features.DeleteUser;

public record DeleteUserCommand(Guid UserId) : ICommand<Result<DeleteUserResult>>;

public record DeleteUserResult(bool IsSuccess);

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}

internal class DeleteUserHandler(IdentityDbContext dbContext)
    : ICommandHandler<DeleteUserCommand, Result<DeleteUserResult>>
{
    public async Task<Result<DeleteUserResult>> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken
    )
    {
        var usuario = await dbContext.Users.FirstOrDefaultAsync(
            u => u.Id == command.UserId,
            cancellationToken
        );

        if (usuario == null)
            return UserErrors.NotFound(command.UserId);

        // Soft delete - desactivar usuario
        usuario.Deactivate();

        // Agregar evento de dominio
        usuario.AddDomainEvent(new UserDeletedEvent(command.UserId));

        await dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteUserResult(true);
    }
}

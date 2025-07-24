using Identity.Authentication.Services;
using Identity.Users.Dtos;

namespace Identity.Users.Features.CreateUser;

public record CreateUserCommand(CreateUserDto User) : ICommand<Result<CreateUserResult>>;

public record CreateUserResult(Guid Id);

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.User.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.User.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.User.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.User.RoleIds).NotEmpty();
    }
}

internal class CreateUserHandler(IdentityDbContext dbContext, IPasswordHasher passwordHasher)
    : ICommandHandler<CreateUserCommand, Result<CreateUserResult>>
{
    public async Task<Result<CreateUserResult>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // Verificar si el email ya existe
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(
            u => u.Email == command.User.Email.ToLower(),
            cancellationToken
        );

        if (existingUser != null)
            return UserErrors.EmailAlreadyExists(command.User.Email);

        // Crear usuario
        var hashedPassword = passwordHasher.HashPassword(command.User.Password);
        var usuario = User.Create(command.User.Name, command.User.Email, hashedPassword);

        // Asignar roles
        foreach (var roleId in command.User.RoleIds)
        {
            usuario.AssignRole(roleId);
        }

        dbContext.Users.Add(usuario);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateUserResult(usuario.Id);
    }
}

using Identity.Authentication.Models;
using Identity.Authentication.Services;
using Identity.Users.Dtos;

namespace Identity.Users.Features.Login;

public record LoginCommand(LoginDto LoginData) : ICommand<Result<LoginResult>>;

public record LoginResult(JwtTokenResult AuthResult);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.LoginData.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.LoginData.Password).NotEmpty();
    }
}

internal class LoginHandler(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService
) : ICommandHandler<LoginCommand, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default
    )
    {
        // Buscar usuario
        var user = await dbContext
            .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.Module)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.Permissions)
            .ThenInclude(p => p.PermissionType)
            .FirstOrDefaultAsync(
                u => u.Email == command.LoginData.Email.ToLower(),
                cancellationToken
            );

        if (user == null)
            return UserErrors.InvalidCredentials;

        if (!user.Enabled)
            return UserErrors.InactiveUser;

        // Verificar contraseña
        if (!passwordHasher.VerifyPassword(command.LoginData.Password, user.Password))
            return UserErrors.InvalidCredentials;

        // Obtener roles y permisos
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user
            .UserRoles.SelectMany(ur => ur.Role.Permissions)
            .Where(p => p.Module.Enabled)
            .Select(p => $"{p.Module.Name}:{p.PermissionType.Name}")
            .Distinct()
            .ToList();

        // Generar token
        var tokenResult = jwtTokenService.GenerateToken(user, roles, permissions);

        return new LoginResult(tokenResult);
    }
}

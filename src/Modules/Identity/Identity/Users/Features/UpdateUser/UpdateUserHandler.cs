using Identity.Users.Dtos;
using Shared.Exceptions;

namespace Identity.Users.Features.UpdateUser;

public record UpdateUserCommand(UpdateUserDto UserDto) : ICommand<Result<UpdateUserResult>>;

public record UpdateUserResult(bool IsSuccess);

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserDto.Id).NotEmpty();
        RuleFor(x => x.UserDto.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.UserDto.Email).NotEmpty().EmailAddress().MaximumLength(150);
        RuleFor(x => x.UserDto.RoleIds).NotEmpty();
    }
}

internal class UpdateUserHandler(IdentityDbContext dbContext)
    : ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>>
{
    public async Task<Result<UpdateUserResult>> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var user = await dbContext
                .Users.Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == command.UserDto.Id, cancellationToken);

            if (user == null)
                return UserErrors.NotFound(command.UserDto.Id);

            // Verificar si el email ya existe en otro usuario
            var existingUser = await dbContext.Users.FirstOrDefaultAsync(
                u => u.Email == command.UserDto.Email.ToLower() && u.Id != command.UserDto.Id,
                cancellationToken
            );

            if (existingUser != null)
                return UserErrors.EmailAlreadyExists(command.UserDto.Email);

            // Verificar que los roles existen
            var existingRoles = await dbContext
                .Roles.Where(r => command.UserDto.RoleIds.Contains(r.Id) && r.Enabled)
                .CountAsync(cancellationToken);

            if (existingRoles != command.UserDto.RoleIds.Count)
                return RoleErrors.InvalidRoles;

            // Actualizar perfil
            user.UpdateUserInformation(
                command.UserDto.Name,
                command.UserDto.Email,
                command.UserDto.RoleIds
            );

            //TODO: Migrate this to domain event
            // Actualizar roles
            // var currentRoleIds = user.UserRoles.Select(ur => ur.IdRole).ToList();
            // var newRoleIds = command.UserDto.RoleIds;

            // // Eliminar roles que ya no están
            // var rolesToRemove = currentRoleIds.Except(newRoleIds);
            // foreach (var roleId in rolesToRemove)
            // {
            //     user.RemoveRole(roleId);
            // }

            // // Agregar nuevos roles
            // var rolesToAdd = newRoleIds.Except(currentRoleIds);
            // foreach (var roleId in rolesToAdd)
            // {
            //     user.AssignRole(roleId);
            // }

            // SaveChanges maneja automáticamente las transacciones
            await dbContext.SaveChangesAsync(cancellationToken);
            return new UpdateUserResult(true);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Manejar específicamente errores de concurrencia
            throw new BusinessException(
                "User.ConcurrencyError",
                "El usuario fue modificado por otro proceso"
            );
        }
    }
}

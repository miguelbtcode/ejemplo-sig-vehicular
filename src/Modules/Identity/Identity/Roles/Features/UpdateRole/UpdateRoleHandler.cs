using Identity.Roles.Dtos;

namespace Identity.Roles.Features.UpdateRole;

public record UpdateRoleCommand(Guid RoleId, UpdateRoleDto Role)
    : ICommand<Result<UpdateRoleResult>>;

public record UpdateRoleResult(bool IsSuccess);

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty().WithMessage("RoleId es requerido");
        RuleFor(x => x.Role.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Role.Description).MaximumLength(255);
        RuleFor(x => x.Role.Permissions)
            .NotNull()
            .NotEmpty()
            .WithMessage("Debe especificar al menos un permiso");
        RuleForEach(x => x.Role.Permissions)
            .ChildRules(permission =>
            {
                permission.RuleFor(p => p.ModuleId).NotEmpty().WithMessage("ModuleId es requerido");
                permission
                    .RuleFor(p => p.PermissionTypeId)
                    .NotEmpty()
                    .WithMessage("PermissionTypeId es requerido");
            });
    }
}

internal class UpdateRoleHandler(IdentityDbContext dbContext)
    : ICommandHandler<UpdateRoleCommand, Result<UpdateRoleResult>>
{
    public async Task<Result<UpdateRoleResult>> HandleAsync(
        UpdateRoleCommand command,
        CancellationToken cancellationToken
    )
    {
        // 1. Verificar que el rol existe
        var role = await dbContext.Roles.FirstOrDefaultAsync(
            r => r.Id == command.RoleId && r.Enabled,
            cancellationToken
        );

        if (role == null)
            return RoleErrors.NotFound(command.RoleId);

        // 2. Verificar si el nombre ya existe en otro rol
        var existingRoleWithName = await dbContext.Roles.FirstOrDefaultAsync(
            r => r.Name.ToLower() == command.Role.Name.ToLower() && r.Id != command.RoleId,
            cancellationToken
        );

        if (existingRoleWithName != null)
            return RoleErrors.NameAlreadyExists(command.Role.Name);

        // 3. Verificar que todos los módulos existen y están activos
        var moduleIds = command.Role.Permissions.Select(p => p.ModuleId).Distinct().ToList();
        var existingModules = await dbContext
            .Modules.Where(m => moduleIds.Contains(m.Id) && m.Enabled)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (existingModules.Count != moduleIds.Count)
        {
            return new Error(
                "Role.InvalidModules",
                "Uno o más módulos no existen o están inactivos"
            );
        }

        // 4. Verificar que todos los tipos de permiso existen
        var permissionTypeIds = command
            .Role.Permissions.Select(p => p.PermissionTypeId)
            .Distinct()
            .ToList();
        var existingPermissionTypes = await dbContext
            .PermissionTypes.Where(pt => permissionTypeIds.Contains(pt.Id))
            .Select(pt => pt.Id)
            .ToListAsync(cancellationToken);

        if (existingPermissionTypes.Count != permissionTypeIds.Count)
        {
            return new Error(
                "Role.InvalidPermissionTypes",
                "Uno o más tipos de permiso no existen"
            );
        }

        // 5. Actualizar información básica del rol
        role.Update(command.Role.Name, command.Role.Description);

        // Eliminar todos los permisos existentes del rol
        var existingPermissions = await dbContext
            .Permissions.Where(p => p.IdRole == command.RoleId)
            .ToListAsync(cancellationToken);

        if (existingPermissions.Count != 0)
        {
            dbContext.Permissions.RemoveRange(existingPermissions);
        }

        // Crear los nuevos permisos
        var newPermissions = command
            .Role.Permissions.Select(p =>
                Permission.Create(command.RoleId, p.ModuleId, p.PermissionTypeId)
            )
            .ToList();

        if (newPermissions.Count != 0)
        {
            dbContext.Permissions.AddRange(newPermissions);
        }

        // Guardar todos los cambios
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateRoleResult(true);
    }
}

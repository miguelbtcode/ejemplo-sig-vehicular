namespace Identity.Roles.Dtos;

public record CreateRoleDto(string Name, string Description, List<RolePermissionDto> Permissions);

public record RolePermissionDto(Guid ModuleId, Guid PermissionTypeId);

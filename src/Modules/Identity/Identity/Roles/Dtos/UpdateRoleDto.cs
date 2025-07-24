namespace Identity.Roles.Dtos;

public record UpdateRoleDto(string Name, string Description, List<RolePermissionDto> Permissions);

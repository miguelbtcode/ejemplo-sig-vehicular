namespace Identity.Permissions.Dtos;

public record PermisoDto(
    Guid Id,
    string RolName,
    string ModuleName,
    string PermissionType,
    DateTime AssignmentDate
);

namespace Identity.Errors;

public static class RoleErrors
{
    public static Error NotFound(int id) =>
        new("Role.NotFound", $"Rol con ID '{id}' no fue encontrado");

    public static Error InvalidRoles =>
        new("Role.InvalidRoles", "Uno o más roles no existen o están inactivos");

    public static Error NameAlreadyExists(string name) =>
        new("Role.NameAlreadyExists", $"Ya existe un rol con el nombre '{name}'");
}

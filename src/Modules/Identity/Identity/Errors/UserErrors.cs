namespace Identity.Errors;

public static class UserErrors
{
    public static Error NotFound(Guid id) =>
        new("User.NotFound", $"Usuario con ID '{id}' no fue encontrado");

    public static Error EmailAlreadyExists(string email) =>
        new("User.EmailAlreadyExists", $"Ya existe un usuario con el email '{email}'");

    public static Error InvalidCredentials =>
        new("User.InvalidCredentials", "Email o contraseña incorrectos");

    public static Error InactiveUser =>
        new("User.InactiveUser", "La cuenta de usuario está inactiva");

    public static Error InvalidCurrentPassword =>
        new("User.InvalidCurrentPassword", "La contraseña actual es incorrecta");
}

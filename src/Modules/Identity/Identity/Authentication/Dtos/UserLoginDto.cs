namespace Identity.Authentication.Dtos;

public record UserLoginDto(
    Guid Id,
    string Name,
    string Email,
    bool Enabled,
    List<string> Roles,
    List<string> Permissions
);

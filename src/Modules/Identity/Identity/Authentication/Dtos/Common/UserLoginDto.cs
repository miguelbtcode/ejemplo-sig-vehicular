namespace Identity.Authentication.Dtos.Common;

public record UserLoginDto(
    Guid Id,
    string Name,
    string Email,
    bool Enabled,
    List<string> Roles,
    List<string> Permissions
);

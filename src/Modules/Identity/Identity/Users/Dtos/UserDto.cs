namespace Identity.Users.Dtos;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt,
    bool Enabled,
    List<string> Roles
);

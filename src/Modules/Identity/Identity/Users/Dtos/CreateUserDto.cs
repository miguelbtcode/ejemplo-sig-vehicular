namespace Identity.Users.Dtos;

public record CreateUserDto(string Name, string Email, string Password, List<Guid> RoleIds);

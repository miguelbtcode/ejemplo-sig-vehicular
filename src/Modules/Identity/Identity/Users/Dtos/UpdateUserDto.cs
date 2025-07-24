namespace Identity.Users.Dtos;

public record UpdateUserDto(Guid Id, string Name, string Email, List<Guid> RoleIds);

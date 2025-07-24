namespace Identity.Users.Models;

public class UserRole : Entity<Guid>
{
    public Guid IdUser { get; private set; }
    public Guid IdRole { get; private set; }
    public DateTime DateAssigned { get; private set; }

    // Navigation properties
    public User User { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    internal UserRole(Guid idUser, Guid idRole, DateTime dateAssigned)
    {
        IdUser = idUser;
        IdRole = idRole;
        DateAssigned = dateAssigned;
    }

    public static UserRole Create(Guid idUser, Guid idRole)
    {
        if (idUser == Guid.Empty)
            throw new ArgumentException("Invalid user ID.", nameof(idUser));
        if (idRole == Guid.Empty)
            throw new ArgumentException("Invalid role ID.", nameof(idRole));

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            IdUser = idUser,
            IdRole = idRole,
            DateAssigned = DateTime.UtcNow,
        };

        return userRole;
    }

    private UserRole() { } // EF Constructor
}

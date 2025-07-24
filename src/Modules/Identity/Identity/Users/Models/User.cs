namespace Identity.Users.Models;

public class User : Aggregate<Guid>
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Password { get; private set; } = default!;
    public bool Enabled { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    public static User Create(string name, string email, string hashedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(hashedPassword);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email.ToLower(),
            Password = hashedPassword,
            Enabled = true,
        };

        user.AddDomainEvent(new UserCreatedEvent(user));
        return user;
    }

    public void UpdateProfile(string name, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Name = name;
        Email = email.ToLower();

        AddDomainEvent(new UserUpdatedEvent(this));
    }

    public void ChangePassword(string newHashedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newHashedPassword);
        Password = newHashedPassword;
    }

    public void Activate()
    {
        Enabled = true;
    }

    public void Deactivate()
    {
        Enabled = false;
    }

    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(ur => ur.IdRole == roleId))
            return;

        var userRole = UserRole.Create(Id, roleId);
        _userRoles.Add(userRole);
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.IdRole == roleId);
        if (userRole != null)
        {
            _userRoles.Remove(userRole);
        }
    }
}

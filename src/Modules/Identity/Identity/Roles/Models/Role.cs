namespace Identity.Roles.Models;

public class Role : Aggregate<Guid>
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public bool Enabled { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyList<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<Permission> _permissions = [];
    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    public static Role Create(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            Enabled = true,
        };

        return role;
    }

    public void Update(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Description = description ?? string.Empty;
    }

    public void Activate() => Enabled = true;

    public void Deactivate() => Enabled = false;
}

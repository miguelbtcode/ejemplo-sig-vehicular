namespace Identity.Permissions.Models;

public class PermissionType : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    private readonly List<Permission> _permissions = [];
    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    public static PermissionType Create(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new PermissionType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
        };
    }
}

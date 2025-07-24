namespace Identity.Permissions.Models;

public class PermissionType : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    private readonly List<Permission> _permissions = [];
    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    public static PermissionType Create(string name, string code, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return new PermissionType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code.ToUpper(),
            Description = description ?? string.Empty,
        };
    }
}

namespace Identity.Permissions.Models;

public class PermissionType : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string Category { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    private readonly List<Permission> _permissions = [];
    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    public static PermissionType Create(
        string name,
        string code,
        string category,
        string description
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);

        return new PermissionType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code.ToUpper(),
            Category = category.ToUpper(),
            Description = description ?? string.Empty,
        };
    }
}

namespace Identity.Modules.Models;

public class Module : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public bool Enabled { get; private set; }

    private readonly List<Permission> _permissions = [];
    public IReadOnlyList<Permission> Permissions => _permissions.AsReadOnly();

    public static Module Create(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Module
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? string.Empty,
            Enabled = true,
        };
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

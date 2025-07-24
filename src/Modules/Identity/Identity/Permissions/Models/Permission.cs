using Module = Identity.Modules.Models.Module;

namespace Identity.Permissions.Models;

public class Permission : Entity<Guid>
{
    public Guid IdRole { get; private set; }
    public Guid IdModule { get; private set; }
    public Guid IdPermissionType { get; private set; }
    public DateTime DateAssigned { get; private set; }

    // Navigation properties
    public Role Role { get; private set; } = default!;
    public Module Module { get; private set; } = default!;
    public PermissionType PermissionType { get; private set; } = default!;

    private Permission() { }

    public static Permission Create(Guid idRole, Guid idModule, Guid idPermissionType)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            IdRole = idRole,
            IdModule = idModule,
            IdPermissionType = idPermissionType,
            DateAssigned = DateTime.UtcNow,
        };
    }
}

namespace Identity.Data.Seed;

public class IdentityDataSeeder(IdentityDbContext dbContext) : IDataSeeder
{
    public async Task SeedAllAsync()
    {
        await SeedPermissionTypesAsync();
        await SeedModulesAsync();
        await SeedRolesAsync();
        await SeedDefaultUserAsync();
        await SeedDefaultPermissionsAsync();
    }

    private async Task SeedPermissionTypesAsync()
    {
        if (!await dbContext.PermissionTypes.AnyAsync())
        {
            await dbContext.PermissionTypes.AddRangeAsync(InitialData.TiposPermisos);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedModulesAsync()
    {
        if (!await dbContext.Modules.AnyAsync())
        {
            await dbContext.Modules.AddRangeAsync(InitialData.Modules);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedRolesAsync()
    {
        if (!await dbContext.Roles.AnyAsync())
        {
            await dbContext.Roles.AddRangeAsync(InitialData.Roles);
            await dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedDefaultUserAsync()
    {
        if (!await dbContext.Users.AnyAsync())
        {
            // Crear usuario administrador por defecto
            var adminUser = User.Create(
                "Administrador",
                "admin@eshop.com",
                BCrypt.Net.BCrypt.HashPassword("Admin123!")
            );

            dbContext.Users.Add(adminUser);
            await dbContext.SaveChangesAsync();

            // Asignar rol de administrador
            var adminRole = await dbContext.Roles.FirstAsync(r => r.Name == "Administrador");
            adminUser.AssignRole(adminRole.Id);

            await dbContext.SaveChangesAsync();
        }
    }

    private async Task SeedDefaultPermissionsAsync()
    {
        if (!await dbContext.Permissions.AnyAsync())
        {
            var adminRole = await dbContext.Roles.FirstAsync(r => r.Name == "Administrador");
            var modules = await dbContext.Modules.ToListAsync();
            var adminType = await dbContext.PermissionTypes.FirstAsync(tp =>
                tp.Name == "Administrar"
            );

            var permissions = modules.Select(module =>
                Permission.Create(adminRole.Id, module.Id, adminType.Id)
            );

            await dbContext.Permissions.AddRangeAsync(permissions);
            await dbContext.SaveChangesAsync();
        }
    }
}

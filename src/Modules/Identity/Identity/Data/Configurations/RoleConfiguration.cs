namespace Identity.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id_rol");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(50).HasColumnName("nombre_rol");

        builder.Property(e => e.Description).HasMaxLength(255).HasColumnName("descripcion");

        builder.Property(e => e.Enabled).IsRequired().HasColumnName("activo").HasDefaultValue(true);

        builder.HasMany(r => r.Permissions).WithOne(p => p.Role).HasForeignKey(p => p.IdRole);

        builder.ToTable("roles");
    }
}

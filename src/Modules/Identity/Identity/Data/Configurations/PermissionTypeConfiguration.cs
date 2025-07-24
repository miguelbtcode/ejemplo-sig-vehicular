namespace Identity.Data.Configurations;

public class PermissionTypeConfiguration : IEntityTypeConfiguration<PermissionType>
{
    public void Configure(EntityTypeBuilder<PermissionType> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id_tipo_permiso");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(30).HasColumnName("nombre_permiso");

        builder.Property(e => e.Description).HasMaxLength(255).HasColumnName("descripcion");

        builder
            .HasMany(tp => tp.Permissions)
            .WithOne(p => p.PermissionType)
            .HasForeignKey(p => p.IdPermissionType);

        builder.ToTable("tipos_permisos");
    }
}

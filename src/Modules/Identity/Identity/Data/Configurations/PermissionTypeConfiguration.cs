namespace Identity.Data.Configurations;

public class PermissionTypeConfiguration : IEntityTypeConfiguration<PermissionType>
{
    public void Configure(EntityTypeBuilder<PermissionType> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id_tipo_permiso");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(30).HasColumnName("nombre_permiso");

        builder.Property(e => e.Code).IsRequired().HasMaxLength(20).HasColumnName("codigo");

        builder.Property(e => e.Category).IsRequired().HasMaxLength(20).HasColumnName("categoria");

        builder.Property(e => e.Description).HasMaxLength(255).HasColumnName("descripcion");

        builder.HasIndex(e => e.Code).IsUnique();

        builder
            .HasMany(tp => tp.Permissions)
            .WithOne(p => p.PermissionType)
            .HasForeignKey(p => p.IdPermissionType);

        builder.ToTable("tipos_permisos");
    }
}

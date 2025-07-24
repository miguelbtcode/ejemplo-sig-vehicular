namespace Identity.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id_permiso");

        builder.Property(e => e.IdRole).IsRequired().HasColumnName("id_rol");

        builder.Property(e => e.IdModule).IsRequired().HasColumnName("id_modulo");

        builder.Property(e => e.IdPermissionType).IsRequired().HasColumnName("id_tipo_permiso");

        builder
            .Property(e => e.DateAssigned)
            .IsRequired()
            .HasColumnName("fecha_asignacion")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relaciones
        builder
            .HasOne(p => p.Role)
            .WithMany(r => r.Permissions)
            .HasForeignKey(p => p.IdRole)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(p => p.Module)
            .WithMany(m => m.Permissions)
            .HasForeignKey(p => p.IdModule)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(p => p.PermissionType)
            .WithMany(tp => tp.Permissions)
            .HasForeignKey(p => p.IdPermissionType)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único compuesto
        builder
            .HasIndex(p => new
            {
                p.IdRole,
                p.IdModule,
                p.IdPermissionType,
            })
            .IsUnique();

        builder.ToTable("permisos");
    }
}

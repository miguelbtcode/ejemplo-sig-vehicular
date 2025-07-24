using Module = Identity.Modules.Models.Module;

namespace Identity.Data.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id_modulo");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(50).HasColumnName("nombre_modulo");

        builder.Property(e => e.Description).HasMaxLength(255).HasColumnName("descripcion");

        builder.Property(e => e.Enabled).IsRequired().HasColumnName("activo").HasDefaultValue(true);

        builder.HasMany(m => m.Permissions).WithOne(p => p.Module).HasForeignKey(p => p.IdModule);

        builder.ToTable("modulos");
    }
}

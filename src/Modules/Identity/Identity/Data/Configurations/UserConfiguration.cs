namespace Identity.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id_usuario");

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100).HasColumnName("nombre");

        builder.Property(e => e.Email).IsRequired().HasMaxLength(150).HasColumnName("email");

        builder.HasIndex(e => e.Email).IsUnique();

        builder.Property(e => e.Password).IsRequired().HasMaxLength(255).HasColumnName("password");

        builder
            .Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnName("fecha_creacion")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.Enabled).IsRequired().HasColumnName("activo").HasDefaultValue(true);

        builder.HasMany(u => u.UserRoles).WithOne(ur => ur.User).HasForeignKey(ur => ur.IdUser);

        builder.ToTable("usuarios");
    }
}

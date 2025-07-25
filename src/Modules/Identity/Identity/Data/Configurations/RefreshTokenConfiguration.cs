using Identity.Authentication.Models;

namespace Identity.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id_refresh_token");

        builder.Property(e => e.Token).IsRequired().HasMaxLength(500).HasColumnName("token");

        builder.Property(e => e.UserId).IsRequired().HasColumnName("id_usuario");

        builder.Property(e => e.DeviceId).IsRequired().HasMaxLength(100).HasColumnName("device_id");

        builder
            .Property(e => e.DeviceName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("device_name");

        builder.Property(e => e.Platform).IsRequired().HasMaxLength(20).HasColumnName("platform");

        builder.Property(e => e.AppVersion).HasMaxLength(20).HasColumnName("app_version");

        builder.Property(e => e.UserAgent).HasColumnType("text").HasColumnName("user_agent");

        builder
            .Property(e => e.IpAddress)
            .HasMaxLength(45) // IPv6 compatible
            .HasColumnName("ip_address");

        builder.Property(e => e.LastUsed).IsRequired().HasColumnName("last_used");

        builder.Property(e => e.ExpiresAt).IsRequired().HasColumnName("expires_at");

        builder
            .Property(e => e.IsRevoked)
            .IsRequired()
            .HasColumnName("is_revoked")
            .HasDefaultValue(false);

        builder.Property(e => e.RevokedReason).HasMaxLength(255).HasColumnName("revoked_reason");

        // Índices para performance
        builder.HasIndex(e => e.Token).IsUnique().HasDatabaseName("IX_RefreshTokens_Token");

        builder.HasIndex(e => e.UserId).HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasIndex(e => e.DeviceId).HasDatabaseName("IX_RefreshTokens_DeviceId");

        builder.HasIndex(e => e.Platform).HasDatabaseName("IX_RefreshTokens_Platform");

        builder.HasIndex(e => e.ExpiresAt).HasDatabaseName("IX_RefreshTokens_ExpiresAt");

        builder
            .HasIndex(e => new { e.UserId, e.DeviceId })
            .HasDatabaseName("IX_RefreshTokens_UserId_DeviceId");

        // Relaciones
        builder
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("refresh_tokens");
    }
}

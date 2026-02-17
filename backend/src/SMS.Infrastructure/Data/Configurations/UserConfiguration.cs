using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired();

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.RefreshToken)
            .HasColumnName("refresh_token");

        builder.Property(u => u.RefreshTokenExpiryTime)
            .HasColumnName("refresh_token_expiry_time");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by");

        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}

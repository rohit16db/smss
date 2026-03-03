using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration for GradeConfiguration entity
/// Single Responsibility: Define how grade scale is stored
/// </summary>
public class GradeConfigurationConfiguration : IEntityTypeConfiguration<GradeConfiguration>
{
    public void Configure(EntityTypeBuilder<GradeConfiguration> builder)
    {
        builder.ToTable("grade_configuration");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(g => g.GradeName)
            .HasColumnName("grade_name")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(g => g.MinPercentage)
            .HasColumnName("min_percentage")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(g => g.MaxPercentage)
            .HasColumnName("max_percentage")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(g => g.Description)
            .HasColumnName("description")
            .HasMaxLength(255);

        builder.Property(g => g.SchoolId)
            .HasColumnName("school_id")
            .IsRequired();

        builder.Property(g => g.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(g => g.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Unique constraint: School + Grade name combination
        builder.HasIndex(g => new { g.SchoolId, g.GradeName })
            .IsUnique();

        // Index for query optimization
        builder.HasIndex(g => g.SchoolId);
    }
}

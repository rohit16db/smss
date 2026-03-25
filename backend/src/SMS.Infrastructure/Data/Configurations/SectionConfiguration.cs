using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("sections");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.ClassId)
            .HasColumnName("class_id")
            .IsRequired();

        builder.Property(s => s.SectionName)
            .HasColumnName("section_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(s => s.UpdatedBy)
            .HasColumnName("updated_by");

        // Relationships
        builder.HasOne(s => s.Class)
            .WithMany(c => c.Sections)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.Cascade);



        // Indexes
        builder.HasIndex(s => s.ClassId);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => new { s.ClassId, s.SectionName })
            .IsUnique();
    }
}

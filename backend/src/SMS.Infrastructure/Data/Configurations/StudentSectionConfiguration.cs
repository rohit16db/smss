using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class StudentSectionConfiguration : IEntityTypeConfiguration<StudentSection>
{
    public void Configure(EntityTypeBuilder<StudentSection> builder)
    {
        builder.ToTable("student_sections");

        builder.HasKey(ss => ss.Id);

        builder.Property(ss => ss.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ss => ss.StudentId)
            .HasColumnName("student_id")
            .IsRequired();

        builder.Property(ss => ss.SectionId)
            .HasColumnName("section_id")
            .IsRequired();

        builder.Property(ss => ss.JoinedDate)
            .HasColumnName("joined_date")
            .IsRequired();

        builder.Property(ss => ss.LeftDate)
            .HasColumnName("left_date");

        builder.Property(ss => ss.IsCurrent)
            .HasColumnName("is_current")
            .IsRequired();

        builder.Property(ss => ss.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ss => ss.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(ss => ss.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(ss => ss.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(ss => ss.RollNumber)
            .HasColumnName("roll_number")
            .IsRequired(false);

        // Relationships
        builder.HasOne(ss => ss.Section)
            .WithMany(s => s.StudentSections)
            .HasForeignKey(ss => ss.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ss => ss.Student)
            .WithMany(s => s.StudentSections)
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ss => ss.StudentId);
        builder.HasIndex(ss => ss.SectionId);
        builder.HasIndex(ss => ss.IsCurrent);
        builder.HasIndex(ss => new { ss.StudentId, ss.IsCurrent })
            .IsUnique(false);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration for ExamClass junction entity
/// Single Responsibility: Define how class-exam relationship and marks entry status is stored
/// </summary>
public class ExamClassConfiguration : IEntityTypeConfiguration<ExamClass>
{
    public void Configure(EntityTypeBuilder<ExamClass> builder)
    {
        builder.ToTable("exam_classes");

        builder.HasKey(ec => ec.Id);

        builder.Property(ec => ec.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ec => ec.ExamId)
            .HasColumnName("exam_id")
            .IsRequired();

        builder.Property(ec => ec.ClassId)
            .HasColumnName("class_id")
            .IsRequired();

        builder.Property(ec => ec.MarksEntryStatus)
            .HasColumnName("marks_entry_status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(ec => ec.SubmittedAt)
            .HasColumnName("submitted_at");

        builder.Property(ec => ec.SubmittedById)
            .HasColumnName("submitted_by");

        builder.Property(ec => ec.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(ec => ec.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(ec => ec.Exam)
            .WithMany(e => e.ExamClasses)
            .HasForeignKey(ec => ec.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ec => ec.Class)
            .WithMany()
            .HasForeignKey(ec => ec.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ec => ec.SubmittedBy)
            .WithMany()
            .HasForeignKey(ec => ec.SubmittedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique constraint: Exam + Class combination
        builder.HasIndex(ec => new { ec.ExamId, ec.ClassId })
            .IsUnique();

        // Index for common queries
        builder.HasIndex(ec => ec.MarksEntryStatus);
    }
}

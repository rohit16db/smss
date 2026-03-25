using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration for StudentMarks entity
/// Single Responsibility: Define how individual student marks are stored
/// </summary>
public class StudentMarksConfiguration : IEntityTypeConfiguration<StudentMarks>
{
    public void Configure(EntityTypeBuilder<StudentMarks> builder)
    {
        builder.ToTable("student_marks");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(sm => sm.ExamId)
            .HasColumnName("exam_id")
            .IsRequired();

        builder.Property(sm => sm.EnrollmentId)
            .HasColumnName("enrollment_id")
            .IsRequired();

        builder.Property(sm => sm.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();

        builder.Property(sm => sm.MarksObtained)
            .HasColumnName("marks_obtained")
            .HasPrecision(5, 2);

        builder.Property(sm => sm.IsAbsent)
            .HasColumnName("is_absent")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sm => sm.Remarks)
            .HasColumnName("remarks")
            .HasMaxLength(500);

        builder.Property(sm => sm.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(sm => sm.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(sm => sm.Exam)
            .WithMany(e => e.StudentMarks)
            .HasForeignKey(sm => sm.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.Enrollment)
            .WithMany()
            .HasForeignKey(sm => sm.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.ExamSubject)
            .WithMany(es => es.StudentMarks)
            .HasForeignKey(sm => new { sm.ExamId, sm.SubjectId })
            .HasPrincipalKey(es => new { es.ExamId, es.SubjectId })
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: Exam + Enrollment + Subject combination
        builder.HasIndex(sm => new { sm.ExamId, sm.EnrollmentId, sm.SubjectId })
            .IsUnique();

        // Indices for common queries
        builder.HasIndex(sm => sm.ExamId);
        builder.HasIndex(sm => sm.EnrollmentId);
    }
}

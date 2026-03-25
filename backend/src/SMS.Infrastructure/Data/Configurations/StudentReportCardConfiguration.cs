using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration for StudentReportCard entity (denormalized for performance)
/// Single Responsibility: Define how pre-calculated report card data is stored
/// </summary>
public class StudentReportCardConfiguration : IEntityTypeConfiguration<StudentReportCard>
{
    public void Configure(EntityTypeBuilder<StudentReportCard> builder)
    {
        builder.ToTable("student_report_cards");

        builder.HasKey(src => src.Id);

        builder.Property(src => src.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(src => src.ExamId)
            .HasColumnName("exam_id")
            .IsRequired();

        builder.Property(src => src.EnrollmentId)
            .HasColumnName("enrollment_id")
            .IsRequired();

        builder.Property(src => src.TotalMarksObtained)
            .HasColumnName("total_marks_obtained")
            .HasPrecision(7, 2)
            .IsRequired();

        builder.Property(src => src.TotalMarks)
            .HasColumnName("total_marks")
            .HasPrecision(7, 2)
            .IsRequired();

        builder.Property(src => src.Percentage)
            .HasColumnName("percentage")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(src => src.OverallGrade)
            .HasColumnName("overall_grade")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(src => src.ClassPosition)
            .HasColumnName("class_position")
            .IsRequired();

        builder.Property(src => src.Pass)
            .HasColumnName("pass")
            .IsRequired();

        builder.Property(src => src.Remarks)
            .HasColumnName("remarks")
            .HasMaxLength(1000);

        builder.Property(src => src.GeneratedAt)
            .HasColumnName("generated_at");

        builder.Property(src => src.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(src => src.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(src => src.Exam)
            .WithMany(e => e.StudentReportCards)
            .HasForeignKey(src => src.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(src => src.Enrollment)
            .WithMany()
            .HasForeignKey(src => src.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: Exam + Enrollment combination (one report card per enrollment per exam)
        builder.HasIndex(src => new { src.ExamId, src.EnrollmentId })
            .IsUnique();

        // Indices for common queries
        builder.HasIndex(src => src.ExamId);
        builder.HasIndex(src => src.EnrollmentId);
        builder.HasIndex(src => src.Pass);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration for Exam entity following SRP
/// Single Responsibility: Define how Exam is stored and validated at database level
/// </summary>
public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("exams");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(e => e.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(e => e.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(e => e.TotalMarks)
            .HasColumnName("total_marks")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(e => e.PassMarks)
            .HasColumnName("pass_marks")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(e => e.CreatedById)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.AcademicYearId)
            .HasColumnName("academic_year_id")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AcademicYear)
            .WithMany(ay => ay.Exams)
            .HasForeignKey(e => e.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ExamSubjects)
            .WithOne(es => es.Exam)
            .HasForeignKey(es => es.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ExamClasses)
            .WithOne(ec => ec.Exam)
            .HasForeignKey(ec => ec.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.StudentMarks)
            .WithOne(sm => sm.Exam)
            .HasForeignKey(sm => sm.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.StudentReportCards)
            .WithOne(src => src.Exam)
            .HasForeignKey(src => src.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint: Name + StartDate combination (allows same exam on different dates)
        builder.HasIndex(e => new { e.Name, e.StartDate })
            .IsUnique();

        // Index for common queries
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.StartDate);
        builder.HasIndex(e => e.EndDate);
        builder.HasIndex(e => e.AcademicYearId);
    }
}

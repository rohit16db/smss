using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration for ExamSubject junction entity
/// Single Responsibility: Define how subject-exam relationship is stored
/// </summary>
public class ExamSubjectConfiguration : IEntityTypeConfiguration<ExamSubject>
{
    public void Configure(EntityTypeBuilder<ExamSubject> builder)
    {
        builder.ToTable("exam_subjects");

        builder.HasKey(es => es.Id);

        builder.Property(es => es.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(es => es.ExamId)
            .HasColumnName("exam_id")
            .IsRequired();

        builder.Property(es => es.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();

        builder.Property(es => es.MaxMarks)
            .HasColumnName("max_marks")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(es => es.PassMarks)
            .HasColumnName("pass_marks")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(es => es.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(es => es.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(es => es.Exam)
            .WithMany(e => e.ExamSubjects)
            .HasForeignKey(es => es.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(es => es.Subject)
            .WithMany()
            .HasForeignKey(es => es.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint: Exam + Subject combination
        builder.HasIndex(es => new { es.ExamId, es.SubjectId })
            .IsUnique();
    }
}

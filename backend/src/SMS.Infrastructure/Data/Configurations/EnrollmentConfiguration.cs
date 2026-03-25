using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("enrollments");

        builder.HasKey(ss => ss.Id);

        builder.Property(ss => ss.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(ss => ss.StudentId)
            .HasColumnName("student_id")
            .IsRequired();

        builder.Property(ss => ss.AcademicYearId)
            .HasColumnName("academic_year_id")
            .IsRequired();

        builder.Property(ss => ss.ClassId)
            .HasColumnName("class_id")
            .IsRequired();

        builder.Property(ss => ss.SectionId)
            .HasColumnName("section_id")
            .IsRequired(false);

        builder.Property(ss => ss.RollNumber)
            .HasColumnName("roll_number")
            .IsRequired(false);

        builder.Property(ss => ss.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(ss => ss.EnrollmentDate)
            .HasColumnName("enrollment_date")
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

        // Relationships
        builder.HasOne(ss => ss.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ss => ss.AcademicYear)
            .WithMany(ay => ay.Enrollments)
            .HasForeignKey(ss => ss.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ss => ss.Class)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(ss => ss.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ss => ss.Section)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(ss => ss.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ss => ss.StudentId);
        builder.HasIndex(ss => ss.AcademicYearId);
        builder.HasIndex(ss => ss.ClassId);
        builder.HasIndex(ss => ss.SectionId);
        builder.HasIndex(ss => ss.Status);
        
        // A student can only have one active enrollment per academic year
        builder.HasIndex(ss => new { ss.StudentId, ss.AcademicYearId })
            .IsUnique(true)
            .HasFilter("status = 'Enrolled'");
    }
}

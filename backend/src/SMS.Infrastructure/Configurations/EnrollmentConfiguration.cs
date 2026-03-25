using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("enrollments");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.StudentId).HasColumnName("student_id").IsRequired();
        builder.Property(e => e.AcademicYearId).HasColumnName("academic_year_id").IsRequired();
        builder.Property(e => e.ClassId).HasColumnName("class_id").IsRequired();
        builder.Property(e => e.SectionId).HasColumnName("section_id");
        builder.Property(e => e.RollNumber).HasColumnName("roll_number");
        builder.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20);
        builder.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date").IsRequired();
        
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.AcademicYear)
            .WithMany(a => a.Enrollments)
            .HasForeignKey(e => e.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.Class)
            .WithMany()
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.Section)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
            
        // One enrollment per student per academic year
        builder.HasIndex(e => new { e.StudentId, e.AcademicYearId }).IsUnique();
    }
}

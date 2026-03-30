using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class StaffAssignmentConfiguration : IEntityTypeConfiguration<StaffAssignment>
{
    public void Configure(EntityTypeBuilder<StaffAssignment> builder)
    {
        builder.ToTable("staff_assignments");
        
        builder.HasKey(ta => ta.Id);
        
        // Properties
        builder.Property(ta => ta.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(ta => ta.StaffId)
            .HasColumnName("staff_id")
            .IsRequired();
        
        builder.Property(ta => ta.ClassId)
            .HasColumnName("class_id")
            .IsRequired();

        builder.Property(ta => ta.SectionId)
            .HasColumnName("section_id")
            .IsRequired();
            
        builder.Property(ta => ta.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();
            
        builder.Property(ta => ta.AcademicYearId)
            .HasColumnName("academic_year_id")
            .IsRequired();
        
        builder.Property(ta => ta.AssignmentDate)
            .HasColumnName("assignment_date")
            .IsRequired();
        
        builder.Property(ta => ta.RemovalDate)
            .HasColumnName("removal_date");
        
        // Audit trail
        builder.Property(ta => ta.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(ta => ta.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(ta => ta.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(ta => ta.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Foreign key
        builder.HasOne(ta => ta.Staff)
            .WithMany(s => s.Assignments)
            .HasForeignKey(ta => ta.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.AcademicYear)
            .WithMany(ay => ay.StaffAssignments)
            .HasForeignKey(ta => ta.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.Class)
            .WithMany(c => c.StaffAssignments)
            .HasForeignKey(ta => ta.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.Subject)
            .WithMany(s => s.StaffAssignments)
            .HasForeignKey(ta => ta.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.Section)
            .WithMany()
            .HasForeignKey(ta => ta.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(ta => ta.StaffId);
        builder.HasIndex(ta => ta.ClassId);
        builder.HasIndex(ta => ta.SectionId);
        builder.HasIndex(ta => ta.SubjectId);
        builder.HasIndex(ta => ta.AcademicYearId);
        builder.HasIndex(ta => ta.RemovalDate);
        
        // Unique constraint: No duplicate active assignments for same (staff, section, subject)
        builder.HasIndex(ta => new { ta.StaffId, ta.SectionId, ta.SubjectId, ta.RemovalDate })
            .IsUnique()
            .HasFilter("removal_date IS NULL");
    }
}

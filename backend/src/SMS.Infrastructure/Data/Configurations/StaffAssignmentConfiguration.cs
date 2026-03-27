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
            .WithMany()
            .HasForeignKey(ta => ta.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(ta => ta.StaffId);
        builder.HasIndex(ta => ta.ClassId);
        builder.HasIndex(ta => ta.SubjectId);
        builder.HasIndex(ta => ta.AcademicYearId);
        builder.HasIndex(ta => ta.RemovalDate);
        
        // Unique constraint: No duplicate active assignments for same (staff, class, subject)
        builder.HasIndex(ta => new { ta.StaffId, ta.ClassId, ta.SubjectId, ta.RemovalDate })
            .IsUnique()
            .HasFilter("removal_date IS NULL");
    }
}

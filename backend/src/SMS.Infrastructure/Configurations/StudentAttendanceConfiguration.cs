using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Configurations;

public class StudentAttendanceConfiguration : IEntityTypeConfiguration<StudentAttendance>
{
    public void Configure(EntityTypeBuilder<StudentAttendance> builder)
    {
        builder.ToTable("student_attendances");
        
        builder.HasKey(sa => sa.Id);
        
        // Properties
        builder.Property(sa => sa.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(sa => sa.StudentId)
            .HasColumnName("student_id")
            .IsRequired();
        
        builder.Property(sa => sa.ClassId)
            .HasColumnName("class_id")
            .IsRequired();
        
        builder.Property(sa => sa.AttendanceDate)
            .HasColumnName("attendance_date")
            .IsRequired();
        
        builder.Property(sa => sa.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(sa => sa.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);
        
        builder.Property(sa => sa.MarkedByUserId)
            .HasColumnName("marked_by_user_id");
        
        builder.Property(sa => sa.MarkedAt)
            .HasColumnName("marked_at");
        
        // Audit trail
        builder.Property(sa => sa.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(sa => sa.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(sa => sa.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(sa => sa.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Indexes
        builder.HasIndex(sa => sa.StudentId);
        builder.HasIndex(sa => sa.ClassId);
        builder.HasIndex(sa => sa.AttendanceDate);
        builder.HasIndex(sa => sa.Status);
        builder.HasIndex(sa => new { sa.StudentId, sa.AttendanceDate })
            .IncludeProperties(sa => new { sa.Status });
        
        // Unique constraint: No duplicate attendance for same student on same date in same class
        builder.HasIndex(sa => new { sa.StudentId, sa.ClassId, sa.AttendanceDate })
            .IsUnique();
        
        // Check constraint: Status must be valid
        builder.ToTable(tb => tb.HasCheckConstraint("ck_student_attendances_status", 
            "status IN ('present', 'absent', 'leave', 'unexcused')"));
    }
}

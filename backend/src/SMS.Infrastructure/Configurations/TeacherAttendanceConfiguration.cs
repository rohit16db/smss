using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Configurations;

public class TeacherAttendanceConfiguration : IEntityTypeConfiguration<TeacherAttendance>
{
    public void Configure(EntityTypeBuilder<TeacherAttendance> builder)
    {
        builder.ToTable("teacher_attendances");
        
        builder.HasKey(ta => ta.Id);
        
        // Properties
        builder.Property(ta => ta.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(ta => ta.TeacherId)
            .HasColumnName("teacher_id")
            .IsRequired();
        
        builder.Property(ta => ta.AttendanceDate)
            .HasColumnName("attendance_date")
            .IsRequired();
        
        builder.Property(ta => ta.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(ta => ta.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500);
        
        builder.Property(ta => ta.RecordedByUserId)
            .HasColumnName("recorded_by_user_id");
        
        builder.Property(ta => ta.RecordedAt)
            .HasColumnName("recorded_at");
        
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
        builder.HasOne(ta => ta.Teacher)
            .WithMany(t => t.AttendanceRecords)
            .HasForeignKey(ta => ta.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(ta => ta.TeacherId);
        builder.HasIndex(ta => ta.AttendanceDate);
        builder.HasIndex(ta => new { ta.TeacherId, ta.AttendanceDate })
            .IncludeProperties(ta => new { ta.Status });
        
        // Unique constraint: No duplicate attendance for same teacher on same date
        builder.HasIndex(ta => new { ta.TeacherId, ta.AttendanceDate })
            .IsUnique();
        
        // Check constraint: Status must be valid
        builder.ToTable(tb => tb.HasCheckConstraint("ck_teacher_attendances_status", 
            "status IN ('present', 'absent', 'leave')"));
    }
}

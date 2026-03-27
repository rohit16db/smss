using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class StaffAttendanceConfiguration : IEntityTypeConfiguration<StaffAttendance>
{
    public void Configure(EntityTypeBuilder<StaffAttendance> builder)
    {
        builder.ToTable("staff_attendances");
        
        builder.HasKey(ta => ta.Id);
        
        // Properties
        builder.Property(ta => ta.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(ta => ta.StaffId)
            .HasColumnName("staff_id")
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
        builder.HasOne(ta => ta.Staff)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(ta => ta.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(ta => ta.StaffId);
        builder.HasIndex(ta => ta.AttendanceDate);
        
        // Unique constraint: No duplicate attendance for same staff on same date
        builder.HasIndex(ta => new { ta.StaffId, ta.AttendanceDate })
            .IsUnique();
        
        // Check constraint: Status must be valid
        builder.ToTable(tb => tb.HasCheckConstraint("ck_staff_attendances_status", 
            "status IN ('present', 'absent', 'leave')"));
    }
}

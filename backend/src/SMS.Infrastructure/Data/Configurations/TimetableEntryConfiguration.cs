using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class TimetableEntryConfiguration : IEntityTypeConfiguration<TimetableEntry>
{
    public void Configure(EntityTypeBuilder<TimetableEntry> builder)
    {
        builder.ToTable("timetable_entries");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.TimeSlotId)
            .HasColumnName("time_slot_id")
            .IsRequired();
            
        builder.Property(t => t.StaffAssignmentId)
            .HasColumnName("staff_assignment_id")
            .IsRequired();

        builder.Property(t => t.RoomNumber)
            .HasColumnName("room_number")
            .HasMaxLength(50);

        builder.Property(t => t.AcademicYearId)
            .HasColumnName("academic_year_id")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(t => t.UpdatedBy)
            .HasColumnName("updated_by");

        // Relationships
        builder.HasOne(t => t.TimeSlot)
            .WithMany(ts => ts.TimetableEntries)
            .HasForeignKey(t => t.TimeSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.StaffAssignment)
            .WithMany()
            .HasForeignKey(t => t.StaffAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AcademicYear)
            .WithMany()
            .HasForeignKey(t => t.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.TimeSlotId);
        builder.HasIndex(t => t.StaffAssignmentId);
        builder.HasIndex(t => t.AcademicYearId);

        // Unique constraint: No duplicate entry for same assignment at same time
        // Since StaffAssignment now includes Section, this handles both Section slot conflict 
        // and Teacher slot conflict.
        // Wait, if a teacher is teaching two different subjects/sections in the same slot, 
        // that's a conflict. StaffAssignment handles (Staff, Section, Subject).
        // Actually, we need to ensure a StaffAssignment (specific Section/Subject/Teacher) 
        // doesn't have two entries in the same slot.
        builder.HasIndex(t => new { t.AcademicYearId, t.TimeSlotId, t.StaffAssignmentId })
            .IsUnique();
    }
}

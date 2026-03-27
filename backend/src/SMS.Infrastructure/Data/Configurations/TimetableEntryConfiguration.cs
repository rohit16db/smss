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

        builder.Property(t => t.SectionId)
            .HasColumnName("section_id")
            .IsRequired();

        builder.Property(t => t.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();

        builder.Property(t => t.StaffId)
            .HasColumnName("staff_id")
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

        builder.HasOne(t => t.Section)
            .WithMany()
            .HasForeignKey(t => t.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Subject)
            .WithMany()
            .HasForeignKey(t => t.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Staff)
            .WithMany()
            .HasForeignKey(t => t.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AcademicYear)
            .WithMany()
            .HasForeignKey(t => t.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.AcademicYearId);
        builder.HasIndex(t => t.TimeSlotId);
        builder.HasIndex(t => t.SectionId);
        builder.HasIndex(t => t.StaffId);
        
        // Ensure a section doesn't have two classes at the same time
        builder.HasIndex(t => new { t.AcademicYearId, t.TimeSlotId, t.SectionId }).IsUnique();
        
        // Ensure a staff member is not assigned to two different classes at the same time
        builder.HasIndex(t => new { t.AcademicYearId, t.TimeSlotId, t.StaffId }).IsUnique();
    }
}

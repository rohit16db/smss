using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.ToTable("time_slots");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.DayOfWeek)
            .HasColumnName("day_of_week")
            .IsRequired();

        builder.Property(t => t.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(t => t.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.IsBreak)
            .HasColumnName("is_break")
            .IsRequired();

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
        builder.HasOne(t => t.AcademicYear)
            .WithMany()
            .HasForeignKey(t => t.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(t => t.AcademicYearId);
        builder.HasIndex(t => new { t.AcademicYearId, t.DayOfWeek, t.StartTime, t.EndTime }).IsUnique();
    }
}

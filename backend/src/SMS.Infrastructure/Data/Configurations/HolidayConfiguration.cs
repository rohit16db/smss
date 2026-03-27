using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("holidays");
        
        builder.HasKey(h => h.Id);
        
        // Properties
        builder.Property(h => h.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(h => h.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();
        
        builder.Property(h => h.HolidayDate)
            .HasColumnName("holiday_date")
            .IsRequired();
        
        builder.Property(h => h.Description)
            .HasColumnName("description")
            .HasMaxLength(500);
        
        builder.Property(h => h.Type)
            .HasColumnName("type")
            .HasMaxLength(50);
        
        builder.Property(h => h.AcademicYearId)
            .HasColumnName("academic_year_id")
            .IsRequired();

        // Relationships
        builder.HasOne(h => h.AcademicYear)
            .WithMany(ay => ay.Holidays)
            .HasForeignKey(h => h.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Audit trail
        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(h => h.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(h => h.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(h => h.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Indexes
        builder.HasIndex(h => h.HolidayDate);
        builder.HasIndex(h => h.AcademicYearId);
        builder.HasIndex(h => h.Type);
        builder.HasIndex(h => new { h.AcademicYearId, h.HolidayDate });
        
        // Unique constraint: No duplicate holiday on same date for same academic year
        builder.HasIndex(h => new { h.HolidayDate, h.AcademicYearId })
            .IsUnique();
    }
}

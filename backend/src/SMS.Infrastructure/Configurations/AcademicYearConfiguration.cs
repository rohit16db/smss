using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Configurations;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("academic_years");
        builder.HasKey(ay => ay.Id);
        
        builder.Property(ay => ay.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
        builder.Property(ay => ay.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(ay => ay.EndDate).HasColumnName("end_date").IsRequired();
        builder.Property(ay => ay.IsCurrent).HasColumnName("is_current").IsRequired();
        builder.Property(ay => ay.IsActive).HasColumnName("is_active").IsRequired();
    }
}

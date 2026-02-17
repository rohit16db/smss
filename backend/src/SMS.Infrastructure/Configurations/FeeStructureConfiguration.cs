using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Configurations;

public class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> builder)
    {
        builder.ToTable("fee_structures");
        
        builder.HasKey(fs => fs.Id);
        
        // Properties
        builder.Property(fs => fs.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(fs => fs.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(fs => fs.AcademicYear)
            .HasColumnName("academic_year")
            .IsRequired();
        
        builder.Property(fs => fs.Frequency)
            .HasColumnName("frequency")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(fs => fs.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(12, 2)
            .IsRequired();
        
        builder.Property(fs => fs.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        
        // Audit trail
        builder.Property(fs => fs.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(fs => fs.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(fs => fs.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(fs => fs.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Relationships
        builder.HasMany(fs => fs.Categories)
            .WithOne(fsc => fsc.FeeStructure)
            .HasForeignKey(fsc => fsc.FeeStructureId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(fs => fs.StudentFees)
            .WithOne(sf => sf.FeeStructure)
            .HasForeignKey(sf => sf.FeeStructureId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(fs => fs.AcademicYear);
        builder.HasIndex(fs => fs.IsActive);
    }
}

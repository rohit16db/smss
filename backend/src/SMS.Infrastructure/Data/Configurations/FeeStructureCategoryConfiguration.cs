using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class FeeStructureCategoryConfiguration : IEntityTypeConfiguration<FeeStructureCategory>
{
    public void Configure(EntityTypeBuilder<FeeStructureCategory> builder)
    {
        builder.ToTable("fee_structure_categories");
        
        builder.HasKey(fsc => fsc.Id);
        
        // Properties
        builder.Property(fsc => fsc.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(fsc => fsc.FeeStructureId)
            .HasColumnName("fee_structure_id")
            .IsRequired();
        
        builder.Property(fsc => fsc.Category)
            .HasColumnName("category")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(fsc => fsc.Amount)
            .HasColumnName("amount")
            .HasPrecision(12, 2)
            .IsRequired();
        
        // Audit trail
        builder.Property(fsc => fsc.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(fsc => fsc.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(fsc => fsc.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(fsc => fsc.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Foreign key
        builder.HasOne(fsc => fsc.FeeStructure)
            .WithMany(fs => fs.Categories)
            .HasForeignKey(fsc => fsc.FeeStructureId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(fsc => fsc.FeeStructureId);
        
        // Unique constraint: No duplicate categories per structure
        builder.HasIndex(fsc => new { fsc.FeeStructureId, fsc.Category })
            .IsUnique();
    }
}

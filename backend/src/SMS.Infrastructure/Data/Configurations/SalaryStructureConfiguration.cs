using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.BaseSalary)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.HRA)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.DA)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.MedicalAllowance)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.ConveyanceAllowance)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.OtherAllowances)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.StandardDeduction)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.MinExperienceYears)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.ApplicableQualifications)
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.EffectiveFromDate)
            .IsRequired();

        builder.Property(s => s.EffectiveToDate);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now() at time zone 'UTC'");

        builder.Property(s => s.UpdatedAt);

        // Index for quick lookups
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.EffectiveFromDate);

        // Relationships
        builder.HasMany(s => s.StaffMembers)
            .WithOne(t => t.SalaryStructure)
            .HasForeignKey(t => t.SalaryStructureId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

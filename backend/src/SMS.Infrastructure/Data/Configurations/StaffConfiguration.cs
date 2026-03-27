using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("staff");
        
        builder.HasKey(s => s.Id);
        
        // Properties
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(s => s.UserProfileId)
            .HasColumnName("user_profile_id")
            .IsRequired();
        
        builder.Property(s => s.DepartmentId)
            .HasColumnName("department_id");
        
        builder.Property(s => s.Designation)
            .HasColumnName("designation")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(s => s.RoleType)
            .HasColumnName("role_type")
            .IsRequired();
        
        builder.Property(s => s.ExperienceYears)
            .HasColumnName("experience_years")
            .HasDefaultValue(0);
        
        builder.Property(s => s.JoiningDate)
            .HasColumnName("joining_date")
            .IsRequired();
        
        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
            
        builder.Property(s => s.BasicSalary)
            .HasColumnName("basic_salary")
            .HasPrecision(18, 2);

        builder.Property(s => s.SalaryStructureId)
            .HasColumnName("salary_structure_id");

        builder.Property(s => s.SalaryStructureEffectiveDate)
            .HasColumnName("salary_structure_effective_date");
        
        // Audit trail
        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(s => s.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Relationships
        builder.HasOne(s => s.UserProfile)
            .WithMany()
            .HasForeignKey(s => s.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Department relationship is configured in DepartmentConfiguration

        builder.HasOne(s => s.SalaryStructure)
            .WithMany()
            .HasForeignKey(s => s.SalaryStructureId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // Indexes
        builder.HasIndex(s => s.UserProfileId);
        builder.HasIndex(s => s.DepartmentId);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.JoiningDate);
    }
}

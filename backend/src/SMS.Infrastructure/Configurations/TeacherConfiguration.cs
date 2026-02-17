using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");
        
        builder.HasKey(t => t.Id);
        
        // Properties
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        
        builder.Property(t => t.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(t => t.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(t => t.Email)
            .HasColumnName("email")
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(t => t.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20);
        
        builder.Property(t => t.Qualification)
            .HasColumnName("qualification")
            .HasMaxLength(500);
        
        builder.Property(t => t.ExperienceYears)
            .HasColumnName("experience_years")
            .HasDefaultValue(0);
        
        builder.Property(t => t.JoiningDate)
            .HasColumnName("joining_date")
            .IsRequired();
        
        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        
        // Audit trail
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(t => t.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Indexes
        builder.HasIndex(t => t.UserId)
            .IsUnique();
        
        builder.HasIndex(t => t.Email)
            .IsUnique();
        
        builder.HasIndex(t => t.IsActive);
        
        builder.HasIndex(t => t.JoiningDate);
    }
}

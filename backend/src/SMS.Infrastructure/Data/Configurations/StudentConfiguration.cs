using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(s => s.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired();

        builder.Property(s => s.Address)
            .HasColumnName("address")
            .HasMaxLength(500);

        builder.Property(s => s.City)
            .HasColumnName("city")
            .HasMaxLength(50);

        builder.Property(s => s.State)
            .HasColumnName("state")
            .HasMaxLength(50);

        builder.Property(s => s.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(10);

        builder.Property(s => s.EnrollmentNumber)
            .HasColumnName("enrollment_number")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.EnrollmentDate)
            .HasColumnName("enrollment_date")
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.GuardianName)
            .HasColumnName("guardian_name")
            .HasMaxLength(100);

        builder.Property(s => s.GuardianPhone)
            .HasColumnName("guardian_phone")
            .HasMaxLength(20);

        builder.Property(s => s.GuardianEmail)
            .HasColumnName("guardian_email")
            .HasMaxLength(100);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(s => s.UpdatedBy)
            .HasColumnName("updated_by");

        // Indexes
        builder.HasIndex(s => s.Email)
            .IsUnique();

        builder.HasIndex(s => s.EnrollmentNumber)
            .IsUnique();

        builder.HasIndex(s => s.IsActive);
        
        builder.HasIndex(s => s.City);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class StudentFeeConfiguration : IEntityTypeConfiguration<StudentFee>
{
    public void Configure(EntityTypeBuilder<StudentFee> builder)
    {
        builder.ToTable("student_fees");
        
        builder.HasKey(sf => sf.Id);
        
        // Properties
        builder.Property(sf => sf.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(sf => sf.EnrollmentId)
            .HasColumnName("enrollment_id")
            .IsRequired();
        
        builder.Property(sf => sf.FeeStructureId)
            .HasColumnName("fee_structure_id")
            .IsRequired();
        
        builder.Property(sf => sf.StartDate)
            .HasColumnName("start_date")
            .IsRequired();
        
        builder.Property(sf => sf.EndDate)
            .HasColumnName("end_date")
            .IsRequired(false);
        
        builder.Property(sf => sf.TotalAmount)
            .HasColumnName("total_amount")
            .HasPrecision(12, 2)
            .IsRequired();
        
        builder.Property(sf => sf.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        
        // Audit trail
        builder.Property(sf => sf.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(sf => sf.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(sf => sf.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(sf => sf.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Foreign keys
        builder.HasOne(sf => sf.Enrollment)
            .WithMany()
            .HasForeignKey(sf => sf.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sf => sf.FeeStructure)
            .WithMany(fs => fs.StudentFees)
            .HasForeignKey(sf => sf.FeeStructureId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(sf => sf.Payments)
            .WithOne(fp => fp.StudentFee)
            .HasForeignKey(fp => fp.StudentFeeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(sf => sf.EnrollmentId);
        builder.HasIndex(sf => sf.FeeStructureId);
        builder.HasIndex(sf => new { sf.StartDate, sf.EndDate });
        
        // Check constraint: StartDate <= EndDate (if EndDate is not null)
        builder.ToTable(tb => tb.HasCheckConstraint("ck_student_fees_date_range", "end_date IS NULL OR start_date <= end_date"));
    }
}

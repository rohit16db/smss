using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Configurations;

public class SalaryPaymentConfiguration : IEntityTypeConfiguration<SalaryPayment>
{
    public void Configure(EntityTypeBuilder<SalaryPayment> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(s => s.TeacherId)
            .IsRequired();

        builder.Property(s => s.PeriodStartDate)
            .IsRequired();

        builder.Property(s => s.PeriodEndDate)
            .IsRequired();

        builder.Property(s => s.BaseSalary)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Deductions)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.Bonus)
            .HasPrecision(18, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.NetSalary)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasDefaultValue(SalaryPaymentStatus.Pending);

        builder.Property(s => s.PaidDate);

        builder.Property(s => s.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(s => s.PaymentMethod)
            .HasConversion<string>();

        builder.Property(s => s.Remarks)
            .HasMaxLength(500);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.UpdatedAt);

        // Relationships
        builder.HasOne(s => s.Teacher)
            .WithMany()
            .HasForeignKey(s => s.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(s => s.TeacherId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => new { s.PeriodStartDate, s.PeriodEndDate });
        builder.HasIndex(s => s.CreatedAt);

        builder.ToTable("SalaryPayments");
    }
}

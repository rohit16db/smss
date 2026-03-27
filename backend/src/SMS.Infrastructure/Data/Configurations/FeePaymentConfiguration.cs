using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data.Configurations;

public class FeePaymentConfiguration : IEntityTypeConfiguration<FeePayment>
{
    public void Configure(EntityTypeBuilder<FeePayment> builder)
    {
        builder.ToTable("fee_payments");
        
        builder.HasKey(fp => fp.Id);
        
        // Properties
        builder.Property(fp => fp.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        
        builder.Property(fp => fp.StudentFeeId)
            .HasColumnName("student_fee_id")
            .IsRequired();
        
        builder.Property(fp => fp.AmountPaid)
            .HasColumnName("amount_paid")
            .HasPrecision(12, 2)
            .IsRequired();
        
        builder.Property(fp => fp.PaymentDate)
            .HasColumnName("payment_date")
            .IsRequired();
        
        builder.Property(fp => fp.ReceiptNumber)
            .HasColumnName("receipt_number")
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(fp => fp.PaymentMethod)
            .HasColumnName("payment_method")
            .HasMaxLength(20)
            .IsRequired();
        
        builder.Property(fp => fp.Notes)
            .HasColumnName("notes")
            .HasMaxLength(500);
        
        // Audit trail (immutable after creation)
        builder.Property(fp => fp.CreatedAt)
            .HasColumnName("created_at");
        
        builder.Property(fp => fp.UpdatedAt)
            .HasColumnName("updated_at");
        
        builder.Property(fp => fp.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100);
        
        builder.Property(fp => fp.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);
        
        // Foreign key
        builder.HasOne(fp => fp.StudentFee)
            .WithMany(sf => sf.Payments)
            .HasForeignKey(fp => fp.StudentFeeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes
        builder.HasIndex(fp => fp.StudentFeeId);
        builder.HasIndex(fp => fp.PaymentDate);
        builder.HasIndex(fp => fp.ReceiptNumber)
            .IsUnique();
        
        // Check constraint: AmountPaid > 0
        builder.ToTable(tb => tb.HasCheckConstraint("ck_fee_payments_amount_positive", "amount_paid > 0"));
    }
}

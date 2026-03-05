using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(s => s.PrimaryColor)
            .HasDefaultValue("#1976D2")
            .HasMaxLength(7);
        
        builder.Property(s => s.SecondaryColor)
            .HasDefaultValue("#DC004E")
            .HasMaxLength(7);
        
        builder.Property(s => s.AccentColor)
            .HasDefaultValue("#FF6F00")
            .HasMaxLength(7);
        
        builder.Property(s => s.DateFormat)
            .HasDefaultValue("dd/MM/yyyy")
            .HasMaxLength(20);
        
        builder.Property(s => s.CurrencyCode)
            .HasDefaultValue("INR")
            .HasMaxLength(3);
        
        builder.Property(s => s.CurrencySymbol)
            .HasDefaultValue("₹")
            .HasMaxLength(5);
        
        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);
        
        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasIndex(s => s.EmailAddress).IsUnique();
    }
}

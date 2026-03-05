namespace SMS.Domain.Entities;

public class School : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmailAddress { get; set; }
    public string? Website { get; set; }
    public byte[]? LogoImage { get; set; }
    public string? LogoFileName { get; set; }
    public DateTime EstablishedDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Branding
    public string PrimaryColor { get; set; } = "#1976D2";
    public string SecondaryColor { get; set; } = "#DC004E";
    public string AccentColor { get; set; } = "#FF6F00";
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    
    // System Preferences
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string CurrencyCode { get; set; } = "INR";
    public string CurrencySymbol { get; set; } = "₹";
}

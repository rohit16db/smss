namespace SMS.Domain.Enums;

/// <summary>
/// Fee payment frequency options.
/// </summary>
public static class FeeFrequency
{
    public const string Monthly = "monthly";
    public const string Quarterly = "quarterly";
    public const string Yearly = "yearly";
    
    public static readonly string[] ValidFrequencies = { Monthly, Quarterly, Yearly };
    
    public static bool IsValid(string? frequency) => 
        !string.IsNullOrEmpty(frequency) && ValidFrequencies.Contains(frequency.ToLower());
}

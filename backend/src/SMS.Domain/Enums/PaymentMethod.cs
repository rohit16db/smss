namespace SMS.Domain.Enums;

/// <summary>
/// Payment method for fee collection.
/// </summary>
public static class PaymentMethod
{
    public const string Cash = "cash";
    public const string Check = "check";
    public const string BankTransfer = "bank_transfer";
    
    public static readonly string[] ValidMethods = { Cash, Check, BankTransfer };
    
    public static bool IsValid(string? method) => 
        !string.IsNullOrEmpty(method) && ValidMethods.Contains(method.ToLower());
}

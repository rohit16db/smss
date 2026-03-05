using MediatR;
using SMS.Application.Features.Settings.DTOs;

namespace SMS.Application.Features.Settings.Commands;

public class UpdateSchoolSettingsCommand : IRequest<SchoolDto>
{
    public string UpdatedByUserId { get; set; } = string.Empty;
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
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string AccentColor { get; set; } = string.Empty;
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string DateFormat { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
}

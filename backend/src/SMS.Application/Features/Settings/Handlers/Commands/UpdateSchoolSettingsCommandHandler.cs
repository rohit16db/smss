using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Settings.Commands;
using SMS.Application.Features.Settings.DTOs;
using SMS.Domain.Exceptions;

namespace SMS.Application.Features.Settings.Handlers.Commands;

public class UpdateSchoolSettingsCommandHandler : IRequestHandler<UpdateSchoolSettingsCommand, SchoolDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSchoolSettingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolDto> Handle(UpdateSchoolSettingsCommand request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken)
            ?? throw new NotFoundException("School not found");

        // Update basic info
        school.Name = request.Name;
        school.Code = request.Code;
        school.Address = request.Address;
        school.City = request.City;
        school.State = request.State;
        school.PostalCode = request.PostalCode;
        school.PhoneNumber = request.PhoneNumber;
        school.EmailAddress = request.EmailAddress;
        school.Website = request.Website;
        school.EstablishedDate = request.EstablishedDate;

        // Update branding
        school.PrimaryColor = request.PrimaryColor;
        school.SecondaryColor = request.SecondaryColor;
        school.AccentColor = request.AccentColor;
        school.HeaderText = request.HeaderText;
        school.FooterText = request.FooterText;

        // Update system preferences
        school.DateFormat = request.DateFormat;
        school.CurrencyCode = request.CurrencyCode;
        school.CurrencySymbol = request.CurrencySymbol;

        // Update logo if provided
        if (request.LogoImage != null && request.LogoImage.Length > 0)
        {
            school.LogoImage = request.LogoImage;
            school.LogoFileName = request.LogoFileName;
        }

        school.UpdatedBy = request.UpdatedByUserId;
        school.UpdatedAt = DateTime.UtcNow;

        _context.Schools.Update(school);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(school);
    }

    private static SchoolDto MapToDto(SMS.Domain.Entities.School school)
    {
        var dto = new SchoolDto
        {
            Id = school.Id.ToString(),
            Name = school.Name,
            Code = school.Code,
            Address = school.Address,
            City = school.City,
            State = school.State,
            PostalCode = school.PostalCode,
            PhoneNumber = school.PhoneNumber,
            EmailAddress = school.EmailAddress,
            Website = school.Website,
            EstablishedDate = school.EstablishedDate,
            IsActive = school.IsActive,
            PrimaryColor = school.PrimaryColor,
            SecondaryColor = school.SecondaryColor,
            AccentColor = school.AccentColor,
            HeaderText = school.HeaderText,
            FooterText = school.FooterText,
            DateFormat = school.DateFormat,
            CurrencyCode = school.CurrencyCode,
            CurrencySymbol = school.CurrencySymbol
        };

        if (school.LogoImage != null && school.LogoImage.Length > 0)
        {
            dto.LogoBase64 = Convert.ToBase64String(school.LogoImage);
        }

        return dto;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Settings.DTOs;
using SMS.Application.Features.Settings.Queries;

namespace SMS.Application.Features.Settings.Handlers.Queries;

public class GetSchoolSettingsQueryHandler : IRequestHandler<GetSchoolSettingsQuery, SchoolDto>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolDto> Handle(GetSchoolSettingsQuery request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IsActive, cancellationToken);

        // If no school exists, create default
        if (school == null)
        {
            school = new SMS.Domain.Entities.School
            {
                Name = "My School",
                Code = "SCH001",
                EstablishedDate = DateTime.UtcNow,
                IsActive = true
            };
            _context.Schools.Add(school);
            await _context.SaveChangesAsync(cancellationToken);
        }

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

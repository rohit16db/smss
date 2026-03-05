using MediatR;
using SMS.Application.Features.Settings.DTOs;

namespace SMS.Application.Features.Settings.Queries;

public class GetSchoolSettingsQuery : IRequest<SchoolDto>
{
}

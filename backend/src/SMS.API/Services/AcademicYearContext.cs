using SMS.Application.Common.Interfaces;

namespace SMS.API.Services;

public class AcademicYearContext : IAcademicYearContext
{
    public Guid? AcademicYearId { get; set; }
}

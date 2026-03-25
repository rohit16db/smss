namespace SMS.Application.Common.Interfaces;

public interface IAcademicYearContext
{
    Guid? AcademicYearId { get; set; }
    Guid RequiredAcademicYearId => AcademicYearId ?? throw new InvalidOperationException("Academic Year context is missing.");
}

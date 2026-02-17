using MediatR;
using SMS.Application.Features.Payroll.DTOs;

namespace SMS.Application.Features.Payroll.Queries;

public class GetTeacherPayrollReportQuery : IRequest<PayrollPeriodReportDto>
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class GetBonusEligibilityQuery : IRequest<List<BonusEligibilityDto>>
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal BonusThresholdPercentage { get; set; } = 90m; // Default 90% attendance
}

public class GetTeacherAttendanceSummaryQuery : IRequest<List<TeacherAttendanceSummaryDto>>
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

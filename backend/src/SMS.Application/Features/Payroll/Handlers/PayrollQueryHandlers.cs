using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Payroll.DTOs;
using SMS.Application.Features.Payroll.Queries;
using SMS.Domain.Enums;

namespace SMS.Application.Features.Payroll.Handlers;

public class GetTeacherPayrollReportQueryHandler : IRequestHandler<GetTeacherPayrollReportQuery, PayrollPeriodReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherPayrollReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollPeriodReportDto> Handle(GetTeacherPayrollReportQuery request, CancellationToken cancellationToken)
    {
        var teachers = await _context.Teachers
            .Include(t => t.SalaryStructure)
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);
        var attendanceRecords = await _context.TeacherAttendances
            .Where(a => a.AttendanceDate >= request.StartDate && a.AttendanceDate <= request.EndDate)
            .ToListAsync(cancellationToken);

        // Calculate working days (excluding weekends)
        var totalWorkingDays = CalculateWorkingDays(request.StartDate, request.EndDate);

        var teacherPayrolls = new List<TeacherPayrollReportDto>();
        decimal totalPayrollAmount = 0;
        decimal totalBonusAmount = 0;
        int eligibleTeachersCount = 0;

        foreach (var teacher in teachers)
        {
            var teacherAttendance = attendanceRecords.Where(a => a.TeacherId == teacher.Id).ToList();
            var presentDays = teacherAttendance.Count(a => a.Status == AttendanceStatus.Present);
            var absentDays = teacherAttendance.Count(a => a.Status == AttendanceStatus.Absent);
            var leaveDays = teacherAttendance.Count(a => a.Status == AttendanceStatus.Leave);

            var attendancePercentage = totalWorkingDays > 0 ? ((decimal)presentDays / totalWorkingDays) * 100 : 0;

            // Bonus calculation (90% threshold)
            var bonusEligible = attendancePercentage >= 90;
            var bonusPercentage = bonusEligible ? 10m : 0m; // 10% bonus if eligible
            var baseSalary = teacher.SalaryStructure?.BaseSalary ?? 50000m; // Get from assigned salary structure, fallback to 50k if not assigned
            var bonusAmount = baseSalary * (bonusPercentage / 100);

            // Deductions
            var deductionsForAbsence = baseSalary * ((decimal)absentDays / 30); // Pro-rata deduction
            var grossSalary = baseSalary;
            var netSalary = grossSalary - deductionsForAbsence + bonusAmount;

            var payrollReport = new TeacherPayrollReportDto
            {
                TeacherId = teacher.Id,
                TeacherName = $"{teacher.FirstName} {teacher.LastName}",
                TeacherImagePath = teacher.ImagePath,
                BaseSalary = baseSalary,
                PeriodStartDate = request.StartDate,
                PeriodEndDate = request.EndDate,
                TotalWorkingDays = totalWorkingDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LeaveDays = leaveDays,
                AttendancePercentage = Math.Round(attendancePercentage, 2),
                GrossSalary = grossSalary,
                DeductionsForAbsence = Math.Round(deductionsForAbsence, 2),
                BonusAmount = bonusAmount,
                NetSalary = Math.Round(netSalary, 2),
                IsBonusEligible = bonusEligible,
                BonusEligibilityReason = bonusEligible ? "Attendance >= 90%" : $"Attendance {attendancePercentage:F2}% < 90%"
            };

            teacherPayrolls.Add(payrollReport);
            totalPayrollAmount += netSalary;
            totalBonusAmount += bonusAmount;

            if (bonusEligible)
                eligibleTeachersCount++;
        }

        return new PayrollPeriodReportDto
        {
            GeneratedAt = DateTime.UtcNow,
            PeriodStartDate = request.StartDate,
            PeriodEndDate = request.EndDate,
            TeacherPayrolls = teacherPayrolls.OrderBy(p => p.TeacherName).ToList(),
            TotalPayrollAmount = Math.Round(totalPayrollAmount, 2),
            TotalBonusAmount = Math.Round(totalBonusAmount, 2),
            EligibleTeachers = eligibleTeachersCount
        };
    }

    private int CalculateWorkingDays(DateOnly startDate, DateOnly endDate)
    {
        int workingDays = 0;
        var current = startDate;

        while (current <= endDate)
        {
            var dayOfWeek = current.DayOfWeek;
            if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
            {
                workingDays++;
            }
            current = current.AddDays(1);
        }

        return workingDays;
    }
}

public class GetBonusEligibilityQueryHandler : IRequestHandler<GetBonusEligibilityQuery, List<BonusEligibilityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBonusEligibilityQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BonusEligibilityDto>> Handle(GetBonusEligibilityQuery request, CancellationToken cancellationToken)
    {
        var teachers = await _context.Teachers
            .Include(t => t.SalaryStructure)
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);
        var attendanceRecords = await _context.TeacherAttendances
            .Where(a => a.AttendanceDate >= request.StartDate && a.AttendanceDate <= request.EndDate)
            .ToListAsync(cancellationToken);

        var totalWorkingDays = CalculateWorkingDays(request.StartDate, request.EndDate);
        var bonusEligibilities = new List<BonusEligibilityDto>();

        foreach (var teacher in teachers)
        {
            var teacherAttendance = attendanceRecords.Where(a => a.TeacherId == teacher.Id).ToList();
            var presentDays = teacherAttendance.Count(a => a.Status == AttendanceStatus.Present);

            var attendancePercentage = totalWorkingDays > 0 ? ((decimal)presentDays / totalWorkingDays) * 100 : 0;
            var isEligible = attendancePercentage >= request.BonusThresholdPercentage;
            var baseSalary = teacher.SalaryStructure?.BaseSalary ?? 50000m; // Get from assigned salary structure, fallback to 50k
            var bonusPercentage = isEligible ? 10m : 0m;
            var bonusAmount = baseSalary * (bonusPercentage / 100);

            bonusEligibilities.Add(new BonusEligibilityDto
            {
                TeacherId = teacher.Id,
                TeacherName = $"{teacher.FirstName} {teacher.LastName}",
                AttendancePercentage = Math.Round(attendancePercentage, 2),
                BonusPercentage = bonusPercentage,
                BonusAmount = bonusAmount,
                IsEligible = isEligible,
                Reason = isEligible 
                    ? $"Attendance {attendancePercentage:F2}% meets {request.BonusThresholdPercentage}% threshold"
                    : $"Attendance {attendancePercentage:F2}% below {request.BonusThresholdPercentage}% threshold"
            });
        }

        return bonusEligibilities.OrderByDescending(b => b.AttendancePercentage).ToList();
    }

    private int CalculateWorkingDays(DateOnly startDate, DateOnly endDate)
    {
        int workingDays = 0;
        var current = startDate;

        while (current <= endDate)
        {
            var dayOfWeek = current.DayOfWeek;
            if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
            {
                workingDays++;
            }
            current = current.AddDays(1);
        }

        return workingDays;
    }
}

public class GetTeacherAttendanceSummaryQueryHandler : IRequestHandler<GetTeacherAttendanceSummaryQuery, List<TeacherAttendanceSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherAttendanceSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeacherAttendanceSummaryDto>> Handle(GetTeacherAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var teachers = await _context.Teachers.Where(t => t.IsActive).ToListAsync(cancellationToken);
        var attendanceRecords = await _context.TeacherAttendances
            .Where(a => a.AttendanceDate >= request.StartDate && a.AttendanceDate <= request.EndDate)
            .ToListAsync(cancellationToken);

        var totalDays = (request.EndDate.DayNumber - request.StartDate.DayNumber) + 1;
        var summaries = new List<TeacherAttendanceSummaryDto>();

        foreach (var teacher in teachers)
        {
            var teacherAttendance = attendanceRecords.Where(a => a.TeacherId == teacher.Id).ToList();
            var presentDays = teacherAttendance.Count(a => a.Status == AttendanceStatus.Present);
            var absentDays = teacherAttendance.Count(a => a.Status == AttendanceStatus.Absent);
            var leaveDays = teacherAttendance.Count(a => a.Status == AttendanceStatus.Leave);

            var attendancePercentage = totalDays > 0 ? ((decimal)presentDays / totalDays) * 100 : 0;

            summaries.Add(new TeacherAttendanceSummaryDto
            {
                TeacherId = teacher.Id,
                TeacherName = $"{teacher.FirstName} {teacher.LastName}",
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LeaveDays = leaveDays,
                AttendancePercentage = Math.Round(attendancePercentage, 2)
            });
        }

        return summaries.OrderBy(s => s.TeacherName).ToList();
    }
}

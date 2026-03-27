using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Payroll.DTOs;
using SMS.Application.Features.Payroll.Queries;
using SMS.Domain.Enums;

namespace SMS.Application.Features.Payroll.Handlers;

public class GetStaffPayrollReportQueryHandler : IRequestHandler<GetStaffPayrollReportQuery, PayrollPeriodReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetStaffPayrollReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollPeriodReportDto> Handle(GetStaffPayrollReportQuery request, CancellationToken cancellationToken)
    {
        var staffMembers = await _context.Staff
            .Include(t => t.UserProfile)
            .Include(t => t.SalaryStructure)
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);
        var attendanceRecords = await _context.StaffAttendances
            .Where(a => a.AttendanceDate >= request.StartDate && a.AttendanceDate <= request.EndDate)
            .ToListAsync(cancellationToken);

        // Calculate working days (excluding weekends)
        var totalWorkingDays = CalculateWorkingDays(request.StartDate, request.EndDate);

        var staffPayrolls = new List<StaffPayrollReportDto>();
        decimal totalPayrollAmount = 0;
        decimal totalBonusAmount = 0;
        int eligibleStaffCount = 0;

        foreach (var staff in staffMembers)
        {
            var staffAttendance = attendanceRecords.Where(a => a.StaffId == staff.Id).ToList();
            var presentDays = staffAttendance.Count(a => a.Status == AttendanceStatus.Present);
            var absentDays = staffAttendance.Count(a => a.Status == AttendanceStatus.Absent);
            var leaveDays = staffAttendance.Count(a => a.Status == AttendanceStatus.Leave);

            var attendancePercentage = totalWorkingDays > 0 ? ((decimal)presentDays / totalWorkingDays) * 100 : 0;

            // Bonus calculation (90% threshold)
            var bonusEligible = attendancePercentage >= 90;
            var bonusPercentage = bonusEligible ? 10m : 0m; // 10% bonus if eligible
            var baseSalary = staff.SalaryStructure?.BaseSalary ?? 0m; // Get from assigned salary structure, fallback to 0 if not assigned
            var bonusAmount = baseSalary * (bonusPercentage / 100);

            // Deductions
            var deductionsForAbsence = baseSalary * ((decimal)absentDays / 30); // Pro-rata deduction
            var grossSalary = baseSalary;
            var netSalary = grossSalary - deductionsForAbsence + bonusAmount;

            var payrollReport = new StaffPayrollReportDto
            {
                StaffId = staff.Id,
                StaffName = staff.FullName,
                StaffImagePath = staff.UserProfile?.ImagePath,
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

            staffPayrolls.Add(payrollReport);
            totalPayrollAmount += netSalary;
            totalBonusAmount += bonusAmount;

            if (bonusEligible)
                eligibleStaffCount++;
        }

        return new PayrollPeriodReportDto
        {
            GeneratedAt = DateTime.UtcNow,
            PeriodStartDate = request.StartDate,
            PeriodEndDate = request.EndDate,
            StaffPayrolls = staffPayrolls.OrderBy(p => p.StaffName).ToList(),
            TotalPayrollAmount = Math.Round(totalPayrollAmount, 2),
            TotalBonusAmount = Math.Round(totalBonusAmount, 2),
            EligibleStaffs = eligibleStaffCount
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
        var staffMembers = await _context.Staff
            .Include(t => t.UserProfile)
            .Include(t => t.SalaryStructure)
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);
        var attendanceRecords = await _context.StaffAttendances
            .Where(a => a.AttendanceDate >= request.StartDate && a.AttendanceDate <= request.EndDate)
            .ToListAsync(cancellationToken);

        var totalWorkingDays = CalculateWorkingDays(request.StartDate, request.EndDate);
        var bonusEligibilities = new List<BonusEligibilityDto>();

        foreach (var staff in staffMembers)
        {
            var staffAttendance = attendanceRecords.Where(a => a.StaffId == staff.Id).ToList();
            var presentDays = staffAttendance.Count(a => a.Status == AttendanceStatus.Present);

            var attendancePercentage = totalWorkingDays > 0 ? ((decimal)presentDays / totalWorkingDays) * 100 : 0;
            var isEligible = attendancePercentage >= request.BonusThresholdPercentage;
            var baseSalary = staff.SalaryStructure?.BaseSalary ?? 0m; // Get from assigned salary structure, fallback to 0
            var bonusPercentage = isEligible ? 10m : 0m;
            var bonusAmount = baseSalary * (bonusPercentage / 100);

            bonusEligibilities.Add(new BonusEligibilityDto
            {
                StaffId = staff.Id,
                StaffName = staff.FullName,
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

public class GetStaffAttendanceSummaryQueryHandler : IRequestHandler<GetStaffAttendanceSummaryQuery, List<StaffAttendanceSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffAttendanceSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StaffAttendanceSummaryDto>> Handle(GetStaffAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var staffMembers = await _context.Staff
            .Include(s => s.UserProfile)
            .Where(t => t.IsActive).ToListAsync(cancellationToken);
        var attendanceRecords = await _context.StaffAttendances
            .Where(a => a.AttendanceDate >= request.StartDate && a.AttendanceDate <= request.EndDate)
            .ToListAsync(cancellationToken);

        var totalDays = (request.EndDate.DayNumber - request.StartDate.DayNumber) + 1;
        var summaries = new List<StaffAttendanceSummaryDto>();

        foreach (var staff in staffMembers)
        {
            var staffAttendance = attendanceRecords.Where(a => a.StaffId == staff.Id).ToList();
            var presentDays = staffAttendance.Count(a => a.Status == AttendanceStatus.Present);
            var absentDays = staffAttendance.Count(a => a.Status == AttendanceStatus.Absent);
            var leaveDays = staffAttendance.Count(a => a.Status == AttendanceStatus.Leave);

            var attendancePercentage = totalDays > 0 ? ((decimal)presentDays / totalDays) * 100 : 0;

            summaries.Add(new StaffAttendanceSummaryDto
            {
                StaffId = staff.Id,
                StaffName = staff.FullName,
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                LeaveDays = leaveDays,
                AttendancePercentage = Math.Round(attendancePercentage, 2)
            });
        }

        return summaries.OrderBy(s => s.StaffName).ToList();
    }
}

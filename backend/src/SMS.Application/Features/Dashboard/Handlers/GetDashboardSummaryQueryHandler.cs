using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Dashboard.DTOs;
using SMS.Application.Features.Dashboard.Queries;
using SMS.Domain.Enums;

namespace SMS.Application.Features.Dashboard.Handlers;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponseDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryResponseDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;
        var startDateOnly = DateOnly.FromDateTime(startDate);
        var endDateOnly = DateOnly.FromDateTime(endDate);

        // Get academic summary
        var academicSummary = await GetAcademicSummary(cancellationToken);
        
        // Get financial summary
        var financialSummary = await GetFinancialSummary(startDateOnly, endDateOnly, cancellationToken);
        
        // Get attendance summary
        var attendanceSummary = await GetAttendanceSummary(startDate, endDate, cancellationToken);

        // Create summary cards
        var summaryCards = CreateSummaryCards(academicSummary, financialSummary, attendanceSummary);

        return new DashboardSummaryResponseDto
        {
            GeneratedAt = DateTime.UtcNow,
            AcademicSummary = academicSummary,
            FinancialSummary = financialSummary,
            AttendanceSummary = attendanceSummary,
            SummaryCards = summaryCards
        };
    }

    private async Task<AcademicSummaryDto> GetAcademicSummary(CancellationToken cancellationToken)
    {
        var totalStudents = await _context.Students.CountAsync(cancellationToken);
        var totalTeachers = await _context.Staff.Where(s => s.RoleType == UserRole.Teacher).CountAsync(cancellationToken);
        var activeStudents = await _context.Students.Where(s => s.IsActive).CountAsync(cancellationToken);
        var activeTeachers = await _context.Staff.Where(t => t.IsActive && t.RoleType == UserRole.Teacher).CountAsync(cancellationToken);

        return new AcademicSummaryDto
        {
            TotalStudents = totalStudents,
            TotalStaff = totalTeachers,
            TotalClasses = totalStudents > 0 ? (totalStudents / 40) + 1 : 0, // Estimate classes
            ActiveStudents = activeStudents,
            ActiveStaff = activeTeachers
        };
    }

    private async Task<FinancialSummaryDto> GetFinancialSummary(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var totalStudents = await _context.Students.Where(s => s.IsActive).CountAsync(cancellationToken);

        // Total fees collected in period
        var totalCollected = await _context.FeePayments
            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
            .SumAsync(p => (decimal?)p.AmountPaid, cancellationToken) ?? 0;

        // Total outstanding fees (TotalAmount - payments made)
        var totalOutstanding = 0m;
        var studentFees = await _context.StudentFees.Where(sf => sf.IsActive).ToListAsync(cancellationToken);
        
        foreach (var sf in studentFees)
        {
            var paid = await _context.FeePayments
                .Where(p => p.StudentFeeId == sf.Id)
                .SumAsync(p => (decimal?)p.AmountPaid, cancellationToken) ?? 0;
            
            var outstanding = sf.TotalAmount - paid;
            if (outstanding > 0)
                totalOutstanding += outstanding;
        }

        // Total expected fees (all active student fees)
        var totalExpected = await _context.StudentFees
            .Where(sf => sf.IsActive)
            .SumAsync(sf => (decimal?)sf.TotalAmount, cancellationToken) ?? 0;

        var collectionPercentage = totalExpected > 0 ? (totalCollected / totalExpected) * 100 : 0;

        return new FinancialSummaryDto
        {
            TotalFeesCollected = totalCollected,
            TotalOutstandingFees = totalOutstanding,
            TotalExpectedFees = totalExpected,
            CollectionPercentage = Math.Round(collectionPercentage, 2),
            TotalStudents = totalStudents,
            AveragePaymentPerStudent = totalStudents > 0 ? totalCollected / totalStudents : 0
        };
    }

    private async Task<AttendanceSummaryDto> GetAttendanceSummary(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var totalStudents = await _context.Students.Where(s => s.IsActive).CountAsync(cancellationToken);
        var totalTeachers = await _context.Staff.Where(t => t.IsActive && t.RoleType == UserRole.Teacher).CountAsync(cancellationToken);

        var startDateOnly = DateOnly.FromDateTime(startDate);
        var endDateOnly = DateOnly.FromDateTime(endDate);
        var todayDateOnly = DateOnly.FromDateTime(DateTime.UtcNow);

        // Average student attendance in period
        var studentAttendanceRecords = await _context.StudentAttendances
            .Where(a => a.AttendanceDate >= startDateOnly && a.AttendanceDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        var avgStudentAttendance = 0m;
        if (studentAttendanceRecords.Count > 0)
        {
            var presentDays = studentAttendanceRecords.Count(a => a.Status == AttendanceStatus.Present);
            avgStudentAttendance = ((decimal)presentDays / studentAttendanceRecords.Count) * 100;
        }

        // Average teacher attendance in period
        var teacherAttendanceRecords = await _context.StaffAttendances
            .Where(a => a.AttendanceDate >= startDateOnly && a.AttendanceDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        var avgStaffAttendance = 0m;
        if (teacherAttendanceRecords.Count > 0)
        {
            var presentDays = teacherAttendanceRecords.Count(a => a.Status == AttendanceStatus.Present);
            avgStaffAttendance = ((decimal)presentDays / teacherAttendanceRecords.Count) * 100;
        }

        // Today's attendance
        var todayStudentAttendance = await _context.StudentAttendances
            .Where(a => a.AttendanceDate == todayDateOnly)
            .ToListAsync(cancellationToken);

        var presentToday = todayStudentAttendance.Count(a => a.Status == AttendanceStatus.Present);
        var absentToday = todayStudentAttendance.Count(a => a.Status == AttendanceStatus.Absent);

        return new AttendanceSummaryDto
        {
            AverageStudentAttendance = Math.Round(avgStudentAttendance, 2),
            AverageStaffAttendance = Math.Round(avgStaffAttendance, 2),
            TotalStaff = totalTeachers,
            TotalStudents = totalStudents,
            PresentStudentsTodayCount = presentToday,
            AbsentStudentsTodayCount = absentToday
        };
    }

    private List<DashboardSummaryCardDto> CreateSummaryCards(
        AcademicSummaryDto academic,
        FinancialSummaryDto financial,
        AttendanceSummaryDto attendance)
    {
        return new List<DashboardSummaryCardDto>
        {
            new DashboardSummaryCardDto
            {
                Title = "Total Students",
                Value = academic.TotalStudents,
                Unit = "Students",
                IconName = "students",
                TrendDirection = "stable"
            },
            new DashboardSummaryCardDto
            {
                Title = "Active Staff",
                Value = academic.ActiveStaff,
                Unit = "Staff",
                IconName = "staff",
                TrendDirection = "stable"
            },
            new DashboardSummaryCardDto
            {
                Title = "Fees Collected",
                Value = financial.TotalFeesCollected,
                Unit = "₹",
                IconName = "money",
                TrendDirection = "up"
            },
            new DashboardSummaryCardDto
            {
                Title = "Outstanding Fees",
                Value = financial.TotalOutstandingFees,
                Unit = "₹",
                IconName = "alert",
                TrendDirection = "down"
            },
            new DashboardSummaryCardDto
            {
                Title = "Collection Rate",
                Value = financial.CollectionPercentage,
                Unit = "%",
                IconName = "chart",
                TrendDirection = financial.CollectionPercentage >= 80 ? "up" : "down"
            },
            new DashboardSummaryCardDto
            {
                Title = "Avg Student Attendance",
                Value = attendance.AverageStudentAttendance,
                Unit = "%",
                IconName = "attendance",
                TrendDirection = attendance.AverageStudentAttendance >= 75 ? "up" : "down"
            }
        };
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Reports.DTOs;
using SMS.Application.Features.Reports.Queries;

namespace SMS.Application.Features.Reports.Handlers;

/// <summary>
/// Handler for salary expense summary query
/// </summary>
public class GetSalaryExpenseSummaryQueryHandler : IRequestHandler<GetSalaryExpenseSummaryQuery, SalaryExpenseSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalaryExpenseSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryExpenseSummaryDto> Handle(GetSalaryExpenseSummaryQuery request, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = DateOnly.FromDateTime(request.EndDate);

        var salaryPayments = await _context.SalaryPayments
            .Where(s => s.PeriodStartDate >= startDateOnly && s.PeriodEndDate <= endDateOnly)
            .Include(s => s.Staff)
                .ThenInclude(t => t.SalaryStructure)
            .ToListAsync(cancellationToken);

        var totalNetSalary = salaryPayments.Sum(s => s.NetSalary);
        var totalBaseSalary = salaryPayments.Sum(s => s.BaseSalary);
        var totalBonus = salaryPayments.Sum(s => s.Bonus);
        var totalDeductions = salaryPayments.Sum(s => s.Deductions);
        var staffCount = salaryPayments.DistinctBy(s => s.StaffId).Count();
        var bonusRecipients = salaryPayments.Where(s => s.Bonus > 0).DistinctBy(s => s.StaffId).Count();

        var averageSalary = staffCount > 0 ? totalNetSalary / staffCount : 0;
        var bonusPercentage = totalBaseSalary > 0 ? (totalBonus / totalBaseSalary) * 100 : 0;
        var deductionPercentage = totalBaseSalary > 0 ? (totalDeductions / totalBaseSalary) * 100 : 0;

        // Calculate previous period comparison if dates provided
        decimal? previousPeriodTotal = null;
        decimal? expenseTrend = null;

        if (request.PreviousPeriodStartDate.HasValue && request.PreviousPeriodEndDate.HasValue)
        {
            var prevStartDateOnly = DateOnly.FromDateTime(request.PreviousPeriodStartDate.Value);
            var prevEndDateOnly = DateOnly.FromDateTime(request.PreviousPeriodEndDate.Value);

            var prevSalaryPayments = await _context.SalaryPayments
                .Where(s => s.PeriodStartDate >= prevStartDateOnly && s.PeriodEndDate <= prevEndDateOnly)
                .ToListAsync(cancellationToken);

            previousPeriodTotal = prevSalaryPayments.Sum(s => s.NetSalary);
            expenseTrend = previousPeriodTotal.HasValue && previousPeriodTotal.Value > 0
                ? ((totalNetSalary - previousPeriodTotal.Value) / previousPeriodTotal.Value) * 100
                : 0;
        }

        return new SalaryExpenseSummaryDto
        {
            TotalNetSalary = totalNetSalary,
            AverageSalary = averageSalary,
            TotalBaseSalary = totalBaseSalary,
            TotalBonus = totalBonus,
            TotalDeductions = totalDeductions,
            StaffCount = staffCount,
            BonusRecipients = bonusRecipients,
            BonusPercentage = (decimal)bonusPercentage,
            DeductionPercentage = (decimal)deductionPercentage,
            PreviousPeriodTotal = previousPeriodTotal,
            ExpenseTrend = expenseTrend
        };
    }
}


/// <summary>
/// Handler for monthly salary trend query
/// </summary>
public class GetMonthlySalaryTrendQueryHandler : IRequestHandler<GetMonthlySalaryTrendQuery, IEnumerable<MonthlySalaryTrendDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMonthlySalaryTrendQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MonthlySalaryTrendDto>> Handle(GetMonthlySalaryTrendQuery request, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = DateOnly.FromDateTime(request.EndDate);

        var salaryPayments = await _context.SalaryPayments
            .Where(s => s.PeriodStartDate >= startDateOnly && s.PeriodEndDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        // Group by month
        var months = new SortedDictionary<string, MonthlySalaryTrendDto>();

        // Initialize all months in range
        var currentDate = new DateTime(request.StartDate.Year, request.StartDate.Month, 1);
        var endDate = new DateTime(request.EndDate.Year, request.EndDate.Month, 1);
        while (currentDate <= endDate)
        {
            var monthKey = currentDate.ToString("yyyy-MM");
            months[monthKey] = new MonthlySalaryTrendDto
            {
                Month = monthKey,
                TotalNetSalary = 0,
                TotalBaseSalary = 0,
                TotalBonus = 0,
                TotalDeductions = 0,
                StaffCount = 0,
                BonusRecipients = 0,
                AverageSalary = 0
            };
            currentDate = currentDate.AddMonths(1);
        }

        // Aggregate salary data by month
        var paymentsByMonth = salaryPayments.GroupBy(p => p.PeriodStartDate.ToString("yyyy-MM"));
        foreach (var monthGroup in paymentsByMonth)
        {
            var monthKey = monthGroup.Key;
            if (months.ContainsKey(monthKey))
            {
                var monthPayments = monthGroup.ToList();
                months[monthKey].TotalNetSalary = monthPayments.Sum(s => s.NetSalary);
                months[monthKey].TotalBaseSalary = monthPayments.Sum(s => s.BaseSalary);
                months[monthKey].TotalBonus = monthPayments.Sum(s => s.Bonus);
                months[monthKey].TotalDeductions = monthPayments.Sum(s => s.Deductions);
                months[monthKey].StaffCount = monthPayments.DistinctBy(s => s.StaffId).Count();
                months[monthKey].BonusRecipients = monthPayments.Where(s => s.Bonus > 0).DistinctBy(s => s.StaffId).Count();
            }
        }

        // Calculate averages
        foreach (var month in months.Values)
        {
            month.AverageSalary = month.StaffCount > 0 ? month.TotalNetSalary / month.StaffCount : 0;
        }

        return months.Values;
    }
}



/// <summary>
/// Handler for salary component breakdown query
/// </summary>
public class GetSalaryComponentBreakdownQueryHandler : IRequestHandler<GetSalaryComponentBreakdownQuery, SalaryComponentBreakdownDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalaryComponentBreakdownQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryComponentBreakdownDto> Handle(GetSalaryComponentBreakdownQuery request, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = DateOnly.FromDateTime(request.EndDate);

        var salaryPayments = await _context.SalaryPayments
            .Where(s => s.PeriodStartDate >= startDateOnly && s.PeriodEndDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        var totalBaseSalary = salaryPayments.Sum(s => s.BaseSalary);
        var totalBonus = salaryPayments.Sum(s => s.Bonus);
        var totalDeductions = salaryPayments.Sum(s => s.Deductions);
        var netSalary = totalBaseSalary + totalBonus - totalDeductions;

        var basePercentage = netSalary > 0 ? (totalBaseSalary / netSalary) * 100 : 0;
        var bonusPercentage = netSalary > 0 ? (totalBonus / netSalary) * 100 : 0;
        var deductionPercentage = netSalary > 0 ? (totalDeductions / netSalary) * 100 : 0;

        return new SalaryComponentBreakdownDto
        {
            BaseSalary = totalBaseSalary,
            Bonus = totalBonus,
            Deductions = totalDeductions,
            NetSalary = netSalary,
            BasePercentage = (decimal)basePercentage,
            BonusPercentage = (decimal)bonusPercentage,
            DeductionsPercentage = (decimal)deductionPercentage,
            RecordCount = salaryPayments.Count
        };
    }
}



/// <summary>
/// Handler for staff-wise salary comparison query
/// </summary>
public class GetStaffSalaryComparisonQueryHandler : IRequestHandler<GetStaffSalaryComparisonQuery, IEnumerable<StaffSalaryComparisonDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffSalaryComparisonQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StaffSalaryComparisonDto>> Handle(GetStaffSalaryComparisonQuery request, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = DateOnly.FromDateTime(request.EndDate);

        var salaryPayments = await _context.SalaryPayments
            .Where(s => s.PeriodStartDate >= startDateOnly && s.PeriodEndDate <= endDateOnly)
            .Include(s => s.Staff)
                .ThenInclude(t => t.UserProfile)
            .Include(s => s.Staff)
                .ThenInclude(t => t.SalaryStructure)
            .ToListAsync(cancellationToken);

        var results = salaryPayments
            .GroupBy(s => s.StaffId)
            .Select(g =>
            {
                var firstPayment = g.First();
                var totalNetSalary = g.Sum(s => s.NetSalary);
                var totalBaseSalary = g.Sum(s => s.BaseSalary);
                var totalBonus = g.Sum(s => s.Bonus);
                var totalDeductions = g.Sum(s => s.Deductions);

                return new StaffSalaryComparisonDto
                {
                    StaffId = firstPayment.StaffId.ToString(),
                    StaffName = firstPayment.Staff?.FullName ?? "Unknown",
                    BaseSalary = totalBaseSalary,
                    Bonus = totalBonus,
                    Deductions = totalDeductions,
                    NetSalary = totalNetSalary,
                    AttendancePercentage = 85m, // Default placeholder
                    BonusEligible = totalBonus > 0,
                    Status = firstPayment.Status.ToString()
                };
            })
            .AsEnumerable();

        // Sort
        results = request.SortBy?.ToLower() switch
        {
            "netsalary" => request.Descending ? results.OrderByDescending(r => r.NetSalary) : results.OrderBy(r => r.NetSalary),
            "bonus" => request.Descending ? results.OrderByDescending(r => r.Bonus) : results.OrderBy(r => r.Bonus),
            "deduction" => request.Descending ? results.OrderByDescending(r => r.Deductions) : results.OrderBy(r => r.Deductions),
            _ => request.Descending ? results.OrderByDescending(r => r.StaffName) : results.OrderBy(r => r.StaffName)
        };

        return results;
    }
}


/// <summary>
/// Handler for attendance to salary correlation query
/// </summary>
public class GetAttendanceToSalaryCorrelationQueryHandler : IRequestHandler<GetAttendanceToSalaryCorrelationQuery, IEnumerable<AttendanceToSalaryCorrelationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAttendanceToSalaryCorrelationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AttendanceToSalaryCorrelationDto>> Handle(GetAttendanceToSalaryCorrelationQuery request, CancellationToken cancellationToken)
    {
        var startOfMonth = new DateTime(request.Month.Year, request.Month.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        var startOfMonthDateOnly = DateOnly.FromDateTime(startOfMonth);
        var endOfMonthDateOnly = DateOnly.FromDateTime(endOfMonth);

        // Get all staff with their attendance and salary data
        var staffMembers = await _context.Staff
            .Include(t => t.UserProfile)
            .Include(t => t.SalaryStructure)
            .ToListAsync(cancellationToken);

        var teacherAttendance = await _context.StaffAttendances
            .Where(a => a.AttendanceDate >= startOfMonthDateOnly && a.AttendanceDate <= endOfMonthDateOnly)
            .ToListAsync(cancellationToken);

        var salaryPayments = await _context.SalaryPayments
            .Where(s => s.PeriodStartDate >= startOfMonthDateOnly && s.PeriodEndDate <= endOfMonthDateOnly)
            .ToListAsync(cancellationToken);

        var results = staffMembers
            .Select(staff =>
            {
                var attendance = teacherAttendance.Where(a => a.StaffId == staff.Id).ToList();
                var totalDays = attendance.Count == 0 ? 1 : attendance.Count;
                var presentDays = attendance.Count(a => a.Status.ToLower() == "present");
                var absentDays = attendance.Count(a => a.Status.ToLower() == "absent");
                var attendancePercentage = totalDays > 0 ? ((decimal)presentDays / totalDays) * 100 : 0;

                // Get the base salary
                var baseSalary = staff.SalaryStructure?.BaseSalary ?? 50000m;

                // Calculate deduction based on policy: 0.5% salary per day absent
                var calculatedDeduction = (baseSalary / 30) * absentDays * 0.5m;

                // Get actual deduction from salary payment
                var actualSalaryPayment = salaryPayments.FirstOrDefault(s => s.StaffId == staff.Id);
                var actualDeduction = actualSalaryPayment?.Deductions ?? 0;

                // Check for bonus eligibility (>= 90% attendance)
                var bonusEligible = attendancePercentage >= 90;
                var bonusAmount = actualSalaryPayment?.Bonus ?? 0;

                // Check for discrepancy
                var hasDiscrepancy = Math.Abs(calculatedDeduction - actualDeduction) > 100;

                return new AttendanceToSalaryCorrelationDto
                {
                    StaffId = staff.Id.ToString(),
                    StaffName = staff.FullName,
                    AttendancePercentage = attendancePercentage,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    TotalDays = totalDays,
                    CalculatedDeduction = calculatedDeduction,
                    ActualDeduction = actualDeduction,
                    DeductionDifference = actualDeduction - calculatedDeduction,
                    BonusEligible = bonusEligible,
                    BonusAmount = bonusAmount,
                    BaseSalary = baseSalary,
                    HasDiscrepancy = hasDiscrepancy,
                    DiscrepancyReason = hasDiscrepancy ? $"Calculated: {calculatedDeduction:C}, Actual: {actualDeduction:C}" : null
                };
            })
            .AsEnumerable();

        // Filter by discrepancies if requested
        if (request.OnlyDiscrepancies)
        {
            results = results.Where(r => r.HasDiscrepancy);
        }

        return results.OrderBy(r => r.StaffName);
    }
}

/// <summary>
/// Handler for budget vs actual comparison query
/// </summary>
public class GetBudgetVsActualQueryHandler : IRequestHandler<GetBudgetVsActualQuery, IEnumerable<BudgetVsActualDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBudgetVsActualQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BudgetVsActualDto>> Handle(GetBudgetVsActualQuery request, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = DateOnly.FromDateTime(request.EndDate);
        var results = new List<BudgetVsActualDto>();

        if (request.ReportType == "FeeCollection")
        {
            var fees = await _context.StudentFees
                .Where(f => f.StartDate >= startDateOnly && 
                           f.StartDate <= endDateOnly)
                .ToListAsync(cancellationToken);

            var payments = await _context.FeePayments
                .Where(p => p.PaymentDate >= startDateOnly && p.PaymentDate <= endDateOnly)
                .ToListAsync(cancellationToken);

            if (request.GroupBy == "month")
            {
                var startDate = new DateTime(request.StartDate.Year, request.StartDate.Month, 1);
                var endDate = new DateTime(request.EndDate.Year, request.EndDate.Month, 1).AddMonths(1).AddDays(-1);

                for (var date = startDate; date <= endDate; date = date.AddMonths(1))
                {
                    var monthKey = date.ToString("yyyy-MM");
                    var monthFees = fees.Where(f => f.StartDate.Year == date.Year && f.StartDate.Month == date.Month).Sum(f => f.TotalAmount);
                    var monthPayments = payments.Where(p => p.PaymentDate.Year == date.Year && p.PaymentDate.Month == date.Month).Sum(p => p.AmountPaid);

                    var variance = monthPayments - monthFees;
                    var variancePercentage = monthFees > 0 ? (variance / monthFees) * 100 : 0;

                    results.Add(new BudgetVsActualDto
                    {
                        Month = monthKey,
                        BudgetedAmount = monthFees,
                        ActualAmount = monthPayments,
                        Variance = variance,
                        VariancePercentage = (decimal)variancePercentage,
                        Category = "Fee Collection"
                    });
                }
            }
        }
        else if (request.ReportType == "SalaryExpense")
        {
            var salaryPayments = await _context.SalaryPayments
                .Where(s => s.PeriodStartDate >= startDateOnly && s.PeriodEndDate <= endDateOnly)
                .ToListAsync(cancellationToken);

            if (salaryPayments.Any())
            {
                var monthlyData = salaryPayments
                    .GroupBy(s => new { s.PeriodStartDate.Year, s.PeriodStartDate.Month })
                    .Select(g =>
                    {
                        var monthKey = $"{g.Key.Year:D4}-{g.Key.Month:D2}";
                        var actual = g.Sum(s => s.NetSalary);
                        var budgeted = actual;

                        return new BudgetVsActualDto
                        {
                            Month = monthKey,
                            BudgetedAmount = budgeted,
                            ActualAmount = actual,
                            Variance = 0,
                            VariancePercentage = 0,
                            Category = "Salary Expense"
                        };
                    });

                results.AddRange(monthlyData);
            }
        }

        return results;
    }
}

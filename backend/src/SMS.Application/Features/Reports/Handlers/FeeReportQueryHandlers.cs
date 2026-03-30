using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Reports.DTOs;
using SMS.Application.Features.Reports.Queries;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Reports.Handlers;

/// <summary>
/// Handler for fee collection summary query
/// </summary>
public class GetFeeCollectionSummaryQueryHandler : IRequestHandler<GetFeeCollectionSummaryQuery, FeeCollectionSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetFeeCollectionSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FeeCollectionSummaryDto> Handle(GetFeeCollectionSummaryQuery request, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = DateOnly.FromDateTime(request.EndDate);

        // Get all fees and their payments for the period
        var fees = await _context.StudentFees
            .Where(f => f.StartDate >= startDateOnly && 
                       f.StartDate <= endDateOnly)
            .Include(f => f.Payments)
            .ToListAsync(cancellationToken);

        var payments = await _context.FeePayments
            .Where(p => p.PaymentDate >= startDateOnly && p.PaymentDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        // Calculate summary metrics
        var totalExpected = fees.Sum(f => f.TotalAmount);
        var totalCollected = payments.Sum(p => p.AmountPaid);
        var totalPending = totalExpected - totalCollected;
        var totalOverdue = 0m; // Calculate based on due date logic if needed

        var collectionRate = totalExpected > 0 ? (totalCollected / totalExpected) * 100 : 0;

        // Get student statistics
        var feesByStudent = fees.GroupBy(f => f.EnrollmentId).ToList();
        var paidStudents = feesByStudent.Count(g => g.Sum(f => f.TotalAmount) <= payments.Where(p => g.Any(f => f.Id == p.StudentFeeId)).Sum(p => p.AmountPaid));
        var allStudents = feesByStudent.Count;
        var dueStudents = feesByStudent.Count(g => g.Sum(f => f.TotalAmount) > payments.Where(p => g.Any(f => f.Id == p.StudentFeeId)).Sum(p => p.AmountPaid));
        var partialStudents = allStudents - paidStudents;

        // Calculate previous period comparison if dates provided
        decimal? previousCollectionRate = null;
        decimal? collectionRateTrend = null;

        if (request.PreviousPeriodStartDate.HasValue && request.PreviousPeriodEndDate.HasValue)
        {
                var prevStartDateOnly = DateOnly.FromDateTime(request.PreviousPeriodStartDate.Value);
                var prevEndDateOnly = DateOnly.FromDateTime(request.PreviousPeriodEndDate.Value);

                var prevFees = await _context.StudentFees
                    .Where(f => f.StartDate >= prevStartDateOnly && 
                               f.StartDate <= prevEndDateOnly)
                    .ToListAsync(cancellationToken);

                var prevPayments = await _context.FeePayments
                    .Where(p => p.PaymentDate >= prevStartDateOnly && p.PaymentDate <= prevEndDateOnly)
                    .ToListAsync(cancellationToken);

                var prevTotalExpected = prevFees.Sum(f => f.TotalAmount);
            var prevTotalCollected = prevPayments.Sum(p => p.AmountPaid);

            previousCollectionRate = prevTotalExpected > 0 ? (prevTotalCollected / prevTotalExpected) * 100 : 0;
            collectionRateTrend = previousCollectionRate.HasValue ? collectionRate - previousCollectionRate.Value : 0;
        }

        return new FeeCollectionSummaryDto
        {
            TotalCollected = totalCollected,
            TotalPending = totalPending,
            TotalOverdue = totalOverdue,
            TotalExpected = totalExpected,
            CollectionRate = (decimal)collectionRate,
            PaidStudents = paidStudents,
            PartialStudents = partialStudents,
            DueStudents = dueStudents,
            OverdueStudents = dueStudents, // Using dueStudents as proxy
            PreviousPeriodCollectionRate = previousCollectionRate,
            CollectionRateTrend = collectionRateTrend
        };
    }
}


/// <summary>
/// Handler for monthly fee collection trend query
/// </summary>
public class GetMonthlyFeeCollectionTrendQueryHandler : IRequestHandler<GetMonthlyFeeCollectionTrendQuery, IEnumerable<MonthlyCollectionTrendDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMonthlyFeeCollectionTrendQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MonthlyCollectionTrendDto>> Handle(GetMonthlyFeeCollectionTrendQuery request, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = DateOnly.FromDateTime(request.EndDate);

        var fees = await _context.StudentFees
            .Where(f => f.StartDate >= startDateOnly && 
                       f.StartDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        var payments = await _context.FeePayments
            .Where(p => p.PaymentDate >= startDateOnly && p.PaymentDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        // Group by month
        var months = new SortedDictionary<string, MonthlyCollectionTrendDto>();

        // Initialize all months in range
        var currentDate = new DateTime(request.StartDate.Year, request.StartDate.Month, 1);
        var endDate = new DateTime(request.EndDate.Year, request.EndDate.Month, 1);
        while (currentDate <= endDate)
        {
            var monthKey = currentDate.ToString("yyyy-MM");
            months[monthKey] = new MonthlyCollectionTrendDto
            {
                Month = monthKey,
                Collected = 0,
                Pending = 0,
                Overdue = 0,
                Expected = 0,
                CollectionRate = 0
            };
            currentDate = currentDate.AddMonths(1);
        }

        // Aggregate fee data by month
        foreach (var fee in fees)
        {
            var monthKey = fee.StartDate.ToString("yyyy-MM");
            if (months.ContainsKey(monthKey))
            {
                months[monthKey].Expected += fee.TotalAmount;
                var feePaid = payments.Where(p => p.StudentFeeId == fee.Id).Sum(p => p.AmountPaid);
                var remaining = fee.TotalAmount - feePaid;
                
                if (remaining <= 0)
                {
                    months[monthKey].Collected += fee.TotalAmount;
                }
                else
                {
                    months[monthKey].Collected += feePaid;
                    months[monthKey].Pending += remaining;
                }
            }
        }

        // Calculate collection rates
        foreach (var month in months.Values)
        {
            month.CollectionRate = month.Expected > 0 ? (month.Collected / month.Expected) * 100 : 0;
        }

        return months.Values;
    }
}



/// <summary>
/// Handler for fee collection by category query
/// </summary>
public class GetFeeCollectionByCategoryQueryHandler : IRequestHandler<GetFeeCollectionByCategoryQuery, IEnumerable<FeeCollectionByCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeeCollectionByCategoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FeeCollectionByCategoryDto>> Handle(GetFeeCollectionByCategoryQuery request, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = DateOnly.FromDateTime(request.EndDate);

        var fees = await _context.StudentFees
            .Where(f => f.StartDate >= startDateOnly && 
                       f.StartDate <= endDateOnly)
            .Include(f => f.FeeStructure)
            .ToListAsync(cancellationToken);

        var payments = await _context.FeePayments
            .Where(p => p.PaymentDate >= startDateOnly && p.PaymentDate <= endDateOnly)
            .ToListAsync(cancellationToken);

        // Group by FeeStructure name as category
        var categories = fees
            .GroupBy(f => f.FeeStructure?.Name ?? "Uncategorized")
            .Select(g => 
            {
                var totalExpected = g.Sum(f => f.TotalAmount);
                var totalCollected = payments.Where(p => g.Any(f => f.Id == p.StudentFeeId)).Sum(p => p.AmountPaid);
                var totalPending = totalExpected - totalCollected;
                
                return new FeeCollectionByCategoryDto
                {
                    Category = g.Key,
                    Expected = totalExpected,
                    Collected = totalCollected,
                    Pending = Math.Max(0, totalPending),
                    Overdue = 0,
                    Count = g.Count()
                };
            })
            .ToList();

        // Calculate percentages
        var totalCollected = categories.Sum(c => c.Collected);
        var totalExpected = categories.Sum(c => c.Expected);
        
        foreach (var category in categories)
        {
            category.CollectionPercentage = category.Expected > 0 ? (category.Collected / category.Expected) * 100 : 0;
            category.PercentageOfTotal = totalExpected > 0 ? (category.Expected / totalExpected) * 100 : 0;
        }

        return categories.OrderByDescending(c => c.Collected);
    }
}



/// <summary>
/// Handler for outstanding fees query
/// </summary>
public class GetOutstandingFeesQueryHandler : IRequestHandler<GetOutstandingFeesQuery, IEnumerable<OutstandingFeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOutstandingFeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OutstandingFeeDto>> Handle(GetOutstandingFeesQuery request, CancellationToken cancellationToken)
    {
        var unpaidFees = await _context.StudentFees
            .Include(f => f.Enrollment)
            .ThenInclude(e => e!.Student)
            .Include(f => f.Enrollment)
            .ThenInclude(e => e!.Section)
            .ThenInclude(s => s!.Class)
            .Include(f => f.Payments)
            .ToListAsync(cancellationToken);

        var results = unpaidFees
            .Select(f =>
            {
                var totalPaid = f.Payments?.Sum(p => p.AmountPaid) ?? 0;
                var outstanding = f.TotalAmount - totalPaid;
                
                if (outstanding <= 0)
                    return null; // Skip fully paid fees

                var lastPaymentDate = f.Payments?.OrderByDescending(p => p.PaymentDate).FirstOrDefault()?.PaymentDate;
                var agingBucket = "0-30"; // Default to current
                
                if (lastPaymentDate.HasValue)
                {
                    var daysSinceLast = (DateTime.UtcNow.Date - lastPaymentDate.Value.ToDateTime(TimeOnly.MinValue).Date).Days;
                    agingBucket = daysSinceLast <= 30 ? "0-30"
                        : daysSinceLast <= 60 ? "31-60"
                        : daysSinceLast <= 90 ? "61-90"
                        : "90+";
                }

                // Get current section for the student
                var currentSection = f.Enrollment;
                var className = currentSection?.Section?.Class?.Name ?? "N/A";
                var sectionName = currentSection?.Section?.SectionName ?? "N/A";
                var classSection = $"{className} - {sectionName}";

                return new OutstandingFeeDto
                {
                    StudentId = f.Enrollment?.StudentId.ToString() ?? "",
                    StudentInfo = $"{f.Enrollment?.Student?.FirstName} {f.Enrollment?.Student?.LastName}",
                    ClassSection = classSection,
                    DueAmount = outstanding,
                    DaysOverdue = lastPaymentDate.HasValue ? (DateTime.UtcNow.Date - lastPaymentDate.Value.ToDateTime(TimeOnly.MinValue).Date).Days : 0,
                    DueDate = f.EndDate.HasValue ? f.EndDate.Value.ToDateTime(TimeOnly.MinValue) : DateTime.UtcNow.AddDays(30),
                    LastPaymentDate = lastPaymentDate?.ToDateTime(TimeOnly.MinValue),
                    AgingBucket = agingBucket,
                    IsActive = f.IsActive
                };
            })
            .OfType<OutstandingFeeDto>();

        // Filter by aging bucket if provided
        if (!string.IsNullOrEmpty(request.AgingBucket))
        {
            results = results.Where(r => r.AgingBucket == request.AgingBucket);
        }

        // Filter by minimum due amount if provided
        if (request.MinimumDueAmount.HasValue)
        {
            results = results.Where(r => r.DueAmount >= request.MinimumDueAmount.Value);
        }

        // Sort
        results = request.SortBy?.ToLower() switch
        {
            "dueamount" => request.Descending ? results.OrderByDescending(r => r.DueAmount) : results.OrderBy(r => r.DueAmount),
            "name" => request.Descending ? results.OrderByDescending(r => r.StudentInfo) : results.OrderBy(r => r.StudentInfo),
            "class" => request.Descending ? results.OrderByDescending(r => r.ClassSection) : results.OrderBy(r => r.ClassSection),
            _ => request.Descending ? results.OrderByDescending(r => r.DaysOverdue) : results.OrderBy(r => r.DaysOverdue)
        };

        return results;
    }
}



/// <summary>
/// Handler for student payment history query
/// </summary>
public class GetStudentPaymentHistoryQueryHandler : IRequestHandler<GetStudentPaymentHistoryQuery, IEnumerable<StudentPaymentHistoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentPaymentHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StudentPaymentHistoryDto>> Handle(GetStudentPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentGuid))
            return new List<StudentPaymentHistoryDto>();

        var studentFees = await _context.StudentFees
            .Where(f => f.Enrollment != null && f.Enrollment.StudentId == studentGuid)
            .Include(f => f.Payments)
            .OrderBy(f => f.StartDate)
            .ToListAsync(cancellationToken);

        var history = studentFees
            .Select(f =>
            {
                var payments = f.Payments?.OrderByDescending(p => p.PaymentDate).ToList() ?? new List<FeePayment>();
                var totalPaid = payments.Sum(p => p.AmountPaid);
                var status = totalPaid >= f.TotalAmount ? "Paid"
                    : totalPaid > 0 ? "Partial"
                    : DateTime.UtcNow > f.EndDate?.ToDateTime(TimeOnly.MaxValue) ? "Overdue"
                    : "Due";

                return new StudentPaymentHistoryDto
                {
                    Month = f.StartDate.ToString("yyyy-MM"),
                    DueAmount = f.TotalAmount,
                    PaidAmount = totalPaid,
                    Status = status,
                    DueDate = f.EndDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow.AddDays(30),
                    Balance = f.TotalAmount - totalPaid,
                    PaymentMethod = payments.FirstOrDefault()?.PaymentMethod,
                    PaymentDate = payments.FirstOrDefault()?.PaymentDate.ToDateTime(TimeOnly.MinValue),
                    ReferenceNumber = payments.FirstOrDefault()?.ReceiptNumber
                };
            })
            .ToList();

        return history;
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Salary.DTOs;
using SMS.Application.Features.Salary.Queries;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Salary.Handlers.Queries;

public class GetSalaryPaymentQueryHandler : IRequestHandler<GetSalaryPaymentQuery, SalaryPaymentDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalaryPaymentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentDto> Handle(GetSalaryPaymentQuery request, CancellationToken cancellationToken)
    {
        var salary = await _context.SalaryPayments
            .Include(s => s.Staff)
                .ThenInclude(s => s.UserProfile)
            .FirstOrDefaultAsync(s => s.Id == request.SalaryPaymentId, cancellationToken);

        if (salary == null)
            throw new InvalidOperationException($"Salary payment with ID {request.SalaryPaymentId} not found");

        return MapToDto(salary);
    }

    private SalaryPaymentDto MapToDto(SalaryPayment salary)
    {
        return new SalaryPaymentDto
        {
            Id = salary.Id,
            StaffId = salary.StaffId,
            StaffName = salary.Staff?.FullName ?? "Unknown",
            PeriodStartDate = salary.PeriodStartDate,
            PeriodEndDate = salary.PeriodEndDate,
            BaseSalary = salary.BaseSalary,
            Deductions = salary.Deductions,
            Bonus = salary.Bonus,
            NetSalary = salary.NetSalary,
            Status = salary.Status.ToString(),
            PaidDate = salary.PaidDate,
            ReferenceNumber = salary.ReferenceNumber,
            PaymentMethod = salary.PaymentMethod?.ToString(),
            Remarks = salary.Remarks,
            CreatedAt = salary.CreatedAt
        };
    }
}

public class GetSalaryPaymentsByPeriodQueryHandler : IRequestHandler<GetSalaryPaymentsByPeriodQuery, SalaryPaymentReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalaryPaymentsByPeriodQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentReportDto> Handle(GetSalaryPaymentsByPeriodQuery request, CancellationToken cancellationToken)
    {
        var salaries = await _context.SalaryPayments
            .Include(s => s.Staff)
                .ThenInclude(s => s.UserProfile)
            .Where(s => s.PeriodStartDate >= request.StartDate && s.PeriodEndDate <= request.EndDate)
            .OrderByDescending(s => s.PeriodStartDate)
            .ToListAsync(cancellationToken);

        var totalStaff = salaries.Select(s => s.StaffId).Distinct().Count();
        var paidStaff = salaries.Count(s => s.Status == SalaryPaymentStatus.Paid);
        var pendingStaff = salaries.Count(s => s.Status == SalaryPaymentStatus.Pending || s.Status == SalaryPaymentStatus.Approved);

        return new SalaryPaymentReportDto
        {
            MonthStart = request.StartDate,
            MonthEnd = request.EndDate,
            TotalStaff = totalStaff,
            PaidStaff = paidStaff,
            PendingStaff = pendingStaff,
            TotalBaseSalary = Math.Round(salaries.Sum(s => s.BaseSalary), 2),
            TotalDeductions = Math.Round(salaries.Sum(s => s.Deductions), 2),
            TotalBonus = Math.Round(salaries.Sum(s => s.Bonus), 2),
            TotalNetSalary = Math.Round(salaries.Sum(s => s.NetSalary), 2),
            PaymentDetails = salaries.Select(MapToDto).ToList()
        };
    }

    private SalaryPaymentDto MapToDto(SalaryPayment salary)
    {
        return new SalaryPaymentDto
        {
            Id = salary.Id,
            StaffId = salary.StaffId,
            StaffName = salary.Staff?.FullName ?? "Unknown",
            PeriodStartDate = salary.PeriodStartDate,
            PeriodEndDate = salary.PeriodEndDate,
            BaseSalary = salary.BaseSalary,
            Deductions = salary.Deductions,
            Bonus = salary.Bonus,
            NetSalary = salary.NetSalary,
            Status = salary.Status.ToString(),
            PaidDate = salary.PaidDate,
            ReferenceNumber = salary.ReferenceNumber,
            PaymentMethod = salary.PaymentMethod?.ToString(),
            Remarks = salary.Remarks,
            CreatedAt = salary.CreatedAt
        };
    }
}

public class GetStaffSalaryPaymentsQueryHandler : IRequestHandler<GetStaffSalaryPaymentsQuery, SalaryHistoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetStaffSalaryPaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryHistoryDto> Handle(GetStaffSalaryPaymentsQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff
            .Include(s => s.UserProfile)
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);
        if (staff == null)
            throw new InvalidOperationException($"Staff with ID {request.StaffId} not found");

        var query = _context.SalaryPayments
            .Where(s => s.StaffId == request.StaffId);

        if (request.StartDate.HasValue)
            query = query.Where(s => s.PeriodStartDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(s => s.PeriodEndDate <= request.EndDate.Value);

        var salaries = await query
            .OrderByDescending(s => s.PeriodStartDate)
            .ToListAsync(cancellationToken);

        var paymentDetails = salaries.Select(MapToDto).ToList();

        return new SalaryHistoryDto
        {
            StaffId = request.StaffId,
            StaffName = staff.FullName,
            PaymentHistory = paymentDetails,
            TotalSalaryPaid = Math.Round(salaries.Where(s => s.Status == SalaryPaymentStatus.Paid).Sum(s => s.NetSalary), 2),
            AverageMonthlySalary = salaries.Count > 0 ? Math.Round(salaries.Average(s => s.NetSalary), 2) : 0,
            TotalPayments = salaries.Count,
            PendingPayments = salaries.Count(s => s.Status == SalaryPaymentStatus.Pending || s.Status == SalaryPaymentStatus.Approved)
        };
    }

    private SalaryPaymentDto MapToDto(SalaryPayment salary)
    {
        return new SalaryPaymentDto
        {
            Id = salary.Id,
            StaffId = salary.StaffId,
            StaffName = salary.Staff?.FullName ?? "Unknown",
            PeriodStartDate = salary.PeriodStartDate,
            PeriodEndDate = salary.PeriodEndDate,
            BaseSalary = salary.BaseSalary,
            Deductions = salary.Deductions,
            Bonus = salary.Bonus,
            NetSalary = salary.NetSalary,
            Status = salary.Status.ToString(),
            PaidDate = salary.PaidDate,
            ReferenceNumber = salary.ReferenceNumber,
            PaymentMethod = salary.PaymentMethod?.ToString(),
            Remarks = salary.Remarks,
            CreatedAt = salary.CreatedAt
        };
    }
}

public class GetPendingSalaryPaymentsQueryHandler : IRequestHandler<GetPendingSalaryPaymentsQuery, List<SalaryPaymentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingSalaryPaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalaryPaymentDto>> Handle(GetPendingSalaryPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SalaryPayments
            .Include(s => s.Staff)
                .ThenInclude(s => s.UserProfile)
            .Where(s => s.Status == SalaryPaymentStatus.Pending || s.Status == SalaryPaymentStatus.Approved);

        if (request.AsOfDate.HasValue)
            query = query.Where(s => s.PeriodEndDate <= request.AsOfDate.Value);

        var salaries = await query
            .OrderBy(s => s.PeriodEndDate)
            .ToListAsync(cancellationToken);

        return salaries.Select(MapToDto).ToList();
    }

    private SalaryPaymentDto MapToDto(SalaryPayment salary)
    {
        return new SalaryPaymentDto
        {
            Id = salary.Id,
            StaffId = salary.StaffId,
            StaffName = salary.Staff?.FullName ?? "Unknown",
            PeriodStartDate = salary.PeriodStartDate,
            PeriodEndDate = salary.PeriodEndDate,
            BaseSalary = salary.BaseSalary,
            Deductions = salary.Deductions,
            Bonus = salary.Bonus,
            NetSalary = salary.NetSalary,
            Status = salary.Status.ToString(),
            PaidDate = salary.PaidDate,
            ReferenceNumber = salary.ReferenceNumber,
            PaymentMethod = salary.PaymentMethod?.ToString(),
            Remarks = salary.Remarks,
            CreatedAt = salary.CreatedAt
        };
    }
}

public class GetStaffSalarySummaryQueryHandler : IRequestHandler<GetStaffSalarySummaryQuery, StaffSalarySummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetStaffSalarySummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffSalarySummaryDto> Handle(GetStaffSalarySummaryQuery request, CancellationToken cancellationToken)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;
        var month = request.Month ?? DateTime.UtcNow.Month;

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var salaries = await _context.SalaryPayments
            .Where(s => s.PeriodStartDate >= startDate && s.PeriodEndDate <= endDate)
            .ToListAsync(cancellationToken);

        var totalStaff = salaries.Select(s => s.StaffId).Distinct().Count();
        var paidStaff = salaries.Count(s => s.Status == SalaryPaymentStatus.Paid);
        var pendingStaff = salaries.Count(s => s.Status == SalaryPaymentStatus.Pending || s.Status == SalaryPaymentStatus.Approved);
        var totalSalaryExpense = salaries.Sum(s => s.NetSalary);

        return new StaffSalarySummaryDto
        {
            TotalSalaryExpense = Math.Round(totalSalaryExpense, 2),
            TotalPaid = Math.Round(salaries.Where(s => s.Status == SalaryPaymentStatus.Paid).Sum(s => s.NetSalary), 2),
            TotalPending = Math.Round(salaries.Where(s => s.Status != SalaryPaymentStatus.Paid && s.Status != SalaryPaymentStatus.Cancelled).Sum(s => s.NetSalary), 2),
            StaffCount = totalStaff,
            PaidCount = paidStaff,
            PendingCount = pendingStaff,
            AverageSalaryPerStaff = totalStaff > 0 ? Math.Round(totalSalaryExpense / totalStaff, 2) : 0
        };
    }
}

public class GetAllSalaryPaymentsQueryHandler : IRequestHandler<GetAllSalaryPaymentsQuery, List<SalaryPaymentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSalaryPaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalaryPaymentDto>> Handle(GetAllSalaryPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SalaryPayments
            .Include(sp => sp.Staff)
                .ThenInclude(s => s.UserProfile)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<SalaryPaymentStatus>(request.Status, true, out var statusEnum))
            query = query.Where(sp => sp.Status == statusEnum);

        if (request.StaffId.HasValue)
            query = query.Where(sp => sp.StaffId == request.StaffId.Value);

        if (request.PeriodStartDate.HasValue)
            query = query.Where(sp => sp.PeriodStartDate >= DateOnly.FromDateTime(request.PeriodStartDate.Value));

        if (request.PeriodEndDate.HasValue)
            query = query.Where(sp => sp.PeriodEndDate <= DateOnly.FromDateTime(request.PeriodEndDate.Value));

        var payments = await query
            .OrderByDescending(sp => sp.PeriodStartDate)
            .ToListAsync(cancellationToken);

        return payments.Select(MapToDto).ToList();
    }

    private static SalaryPaymentDto MapToDto(SalaryPayment payment)
    {
        return new SalaryPaymentDto
        {
            Id = payment.Id,
            StaffId = payment.StaffId,
            StaffName = payment.Staff?.FullName ?? "Unknown",
            PeriodStartDate = payment.PeriodStartDate,
            PeriodEndDate = payment.PeriodEndDate,
            BaseSalary = payment.BaseSalary,
            Deductions = payment.Deductions,
            Bonus = payment.Bonus,
            NetSalary = payment.NetSalary,
            Status = payment.Status.ToString(),
            PaidDate = payment.PaidDate,
            ReferenceNumber = payment.ReferenceNumber,
            PaymentMethod = payment.PaymentMethod?.ToString(),
            Remarks = payment.Remarks,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt ?? payment.CreatedAt
        };
    }
}

public class GetSalaryPaymentsSummaryQueryHandler : IRequestHandler<GetSalaryPaymentsSummaryQuery, SalaryPaymentSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalaryPaymentsSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentSummaryDto> Handle(GetSalaryPaymentsSummaryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SalaryPayments.AsQueryable();

        if (request.PeriodStartDate.HasValue)
            query = query.Where(sp => sp.PeriodStartDate >= DateOnly.FromDateTime(request.PeriodStartDate.Value));

        if (request.PeriodEndDate.HasValue)
            query = query.Where(sp => sp.PeriodEndDate <= DateOnly.FromDateTime(request.PeriodEndDate.Value));

        var payments = await query.ToListAsync(cancellationToken);

        return new SalaryPaymentSummaryDto
        {
            TotalPayments = payments.Count,
            PendingCount = payments.Count(p => p.Status == SalaryPaymentStatus.Pending),
            ApprovedCount = payments.Count(p => p.Status == SalaryPaymentStatus.Approved),
            PaidCount = payments.Count(p => p.Status == SalaryPaymentStatus.Paid),
            OnHoldCount = payments.Count(p => p.Status == SalaryPaymentStatus.OnHold),
            CancelledCount = payments.Count(p => p.Status == SalaryPaymentStatus.Cancelled),
            TotalBaseSalary = payments.Sum(p => p.BaseSalary),
            TotalDeductions = payments.Sum(p => p.Deductions),
            TotalBonus = payments.Sum(p => p.Bonus),
            TotalNetSalary = payments.Sum(p => p.NetSalary),
            TotalPaidAmount = payments.Where(p => p.Status == SalaryPaymentStatus.Paid).Sum(p => p.NetSalary)
        };
    }
}

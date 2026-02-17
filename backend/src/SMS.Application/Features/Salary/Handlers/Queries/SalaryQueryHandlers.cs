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
            .Include(s => s.Teacher)
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
            TeacherId = salary.TeacherId,
            TeacherName = $"{salary.Teacher.FirstName} {salary.Teacher.LastName}",
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
            .Include(s => s.Teacher)
            .Where(s => s.PeriodStartDate >= request.StartDate && s.PeriodEndDate <= request.EndDate)
            .OrderByDescending(s => s.PeriodStartDate)
            .ToListAsync(cancellationToken);

        var totalTeachers = salaries.Select(s => s.TeacherId).Distinct().Count();
        var paidTeachers = salaries.Count(s => s.Status == SalaryPaymentStatus.Paid);
        var pendingTeachers = salaries.Count(s => s.Status == SalaryPaymentStatus.Pending || s.Status == SalaryPaymentStatus.Approved);

        return new SalaryPaymentReportDto
        {
            MonthStart = request.StartDate,
            MonthEnd = request.EndDate,
            TotalTeachers = totalTeachers,
            PaidTeachers = paidTeachers,
            PendingTeachers = pendingTeachers,
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
            TeacherId = salary.TeacherId,
            TeacherName = $"{salary.Teacher.FirstName} {salary.Teacher.LastName}",
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

public class GetTeacherSalaryPaymentsQueryHandler : IRequestHandler<GetTeacherSalaryPaymentsQuery, SalaryHistoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherSalaryPaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryHistoryDto> Handle(GetTeacherSalaryPaymentsQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _context.Teachers.FindAsync(new object[] { request.TeacherId }, cancellationToken);
        if (teacher == null)
            throw new InvalidOperationException($"Teacher with ID {request.TeacherId} not found");

        var query = _context.SalaryPayments
            .Where(s => s.TeacherId == request.TeacherId);

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
            TeacherId = request.TeacherId,
            TeacherName = $"{teacher.FirstName} {teacher.LastName}",
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
            TeacherId = salary.TeacherId,
            TeacherName = $"{salary.Teacher?.FirstName} {salary.Teacher?.LastName}",
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
            .Include(s => s.Teacher)
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
            TeacherId = salary.TeacherId,
            TeacherName = $"{salary.Teacher.FirstName} {salary.Teacher.LastName}",
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

public class GetSalarySummaryQueryHandler : IRequestHandler<GetSalarySummaryQuery, SalarySummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalarySummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalarySummaryDto> Handle(GetSalarySummaryQuery request, CancellationToken cancellationToken)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;
        var month = request.Month ?? DateTime.UtcNow.Month;

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var salaries = await _context.SalaryPayments
            .Where(s => s.PeriodStartDate >= startDate && s.PeriodEndDate <= endDate)
            .ToListAsync(cancellationToken);

        var totalTeachers = salaries.Select(s => s.TeacherId).Distinct().Count();
        var paidTeachers = salaries.Count(s => s.Status == SalaryPaymentStatus.Paid);
        var pendingTeachers = salaries.Count(s => s.Status == SalaryPaymentStatus.Pending || s.Status == SalaryPaymentStatus.Approved);
        var totalSalaryExpense = salaries.Sum(s => s.NetSalary);

        return new SalarySummaryDto
        {
            TotalSalaryExpense = Math.Round(totalSalaryExpense, 2),
            TotalPaid = Math.Round(salaries.Where(s => s.Status == SalaryPaymentStatus.Paid).Sum(s => s.NetSalary), 2),
            TotalPending = Math.Round(salaries.Where(s => s.Status != SalaryPaymentStatus.Paid && s.Status != SalaryPaymentStatus.Cancelled).Sum(s => s.NetSalary), 2),
            TeacherCount = totalTeachers,
            PaidCount = paidTeachers,
            PendingCount = pendingTeachers,
            AverageSalaryPerTeacher = totalTeachers > 0 ? Math.Round(totalSalaryExpense / totalTeachers, 2) : 0
        };
    }
}

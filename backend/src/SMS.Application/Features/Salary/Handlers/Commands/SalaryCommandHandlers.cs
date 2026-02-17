using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Salary.Commands;
using SMS.Application.Features.Salary.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Salary.Handlers.Commands;

public class CreateSalaryPaymentCommandHandler : IRequestHandler<CreateSalaryPaymentCommand, SalaryPaymentDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSalaryPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentDto> Handle(CreateSalaryPaymentCommand request, CancellationToken cancellationToken)
    {
        var teacher = await _context.Teachers.FindAsync(new object[] { request.TeacherId }, cancellationToken);
        if (teacher == null)
            throw new InvalidOperationException($"Teacher with ID {request.TeacherId} not found");

        // Parse payment method enum
        PaymentMethod? paymentMethod = null;
        if (!string.IsNullOrEmpty(request.PaymentMethod) && Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var pm))
            paymentMethod = pm;

        var netSalary = request.BaseSalary - request.Deductions + request.Bonus;

        var salary = new SalaryPayment
        {
            Id = Guid.NewGuid(),
            TeacherId = request.TeacherId,
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            BaseSalary = request.BaseSalary,
            Deductions = request.Deductions,
            Bonus = request.Bonus,
            NetSalary = netSalary,
            Status = SalaryPaymentStatus.Pending,
            ReferenceNumber = request.ReferenceNumber,
            PaymentMethod = paymentMethod,
            Remarks = request.Remarks,
            CreatedAt = DateTime.UtcNow
        };

        _context.SalaryPayments.Add(salary);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(salary, teacher);
    }

    private SalaryPaymentDto MapToDto(SalaryPayment salary, Teacher teacher)
    {
        return new SalaryPaymentDto
        {
            Id = salary.Id,
            TeacherId = salary.TeacherId,
            TeacherName = $"{teacher.FirstName} {teacher.LastName}",
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

public class UpdateSalaryPaymentStatusCommandHandler : IRequestHandler<UpdateSalaryPaymentStatusCommand, SalaryPaymentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSalaryPaymentStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentDto> Handle(UpdateSalaryPaymentStatusCommand request, CancellationToken cancellationToken)
    {
        var salary = await _context.SalaryPayments
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == request.SalaryPaymentId, cancellationToken);

        if (salary == null)
            throw new InvalidOperationException($"Salary payment with ID {request.SalaryPaymentId} not found");

        if (Enum.TryParse<SalaryPaymentStatus>(request.Status, out var status))
            salary.Status = status;

        if (request.PaidDate.HasValue)
            salary.PaidDate = request.PaidDate.Value;

        if (!string.IsNullOrEmpty(request.ReferenceNumber))
            salary.ReferenceNumber = request.ReferenceNumber;

        if (!string.IsNullOrEmpty(request.Remarks))
            salary.Remarks = request.Remarks;

        salary.UpdatedAt = DateTime.UtcNow;

        _context.SalaryPayments.Update(salary);
        await _context.SaveChangesAsync(cancellationToken);

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

public class MarkSalaryAsPaidCommandHandler : IRequestHandler<MarkSalaryAsPaidCommand, SalaryPaymentDto>
{
    private readonly IApplicationDbContext _context;

    public MarkSalaryAsPaidCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentDto> Handle(MarkSalaryAsPaidCommand request, CancellationToken cancellationToken)
    {
        var salary = await _context.SalaryPayments
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == request.SalaryPaymentId, cancellationToken);

        if (salary == null)
            throw new InvalidOperationException($"Salary payment with ID {request.SalaryPaymentId} not found");

        salary.Status = SalaryPaymentStatus.Paid;
        salary.PaidDate = request.PaidDate;

        if (!string.IsNullOrEmpty(request.PaymentMethod) && Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var pm))
            salary.PaymentMethod = pm;

        if (!string.IsNullOrEmpty(request.ReferenceNumber))
            salary.ReferenceNumber = request.ReferenceNumber;

        salary.UpdatedAt = DateTime.UtcNow;

        _context.SalaryPayments.Update(salary);
        await _context.SaveChangesAsync(cancellationToken);

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

public class CreateBulkSalaryPaymentsCommandHandler : IRequestHandler<CreateBulkSalaryPaymentsCommand, SalaryPaymentReportDto>
{
    private readonly IApplicationDbContext _context;

    public CreateBulkSalaryPaymentsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentReportDto> Handle(CreateBulkSalaryPaymentsCommand request, CancellationToken cancellationToken)
    {
        var teachers = await _context.Teachers.Where(t => t.IsActive).ToListAsync(cancellationToken);

        var salaryPayments = new List<SalaryPayment>();
        foreach (var teacher in teachers)
        {
            var baseSalary = request.BaseSalariesByTeacherId.TryGetValue(teacher.Id, out var bs) ? bs : 50000m;
            var deductions = request.DeductionsByTeacherId.TryGetValue(teacher.Id, out var d) ? d : 0m;
            var bonus = request.BonusesByTeacherId.TryGetValue(teacher.Id, out var b) ? b : 0m;
            var netSalary = baseSalary - deductions + bonus;

            var salary = new SalaryPayment
            {
                Id = Guid.NewGuid(),
                TeacherId = teacher.Id,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate,
                BaseSalary = baseSalary,
                Deductions = deductions,
                Bonus = bonus,
                NetSalary = netSalary,
                Status = SalaryPaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            salaryPayments.Add(salary);
        }

        _context.SalaryPayments.AddRange(salaryPayments);
        await _context.SaveChangesAsync(cancellationToken);

        var report = new SalaryPaymentReportDto
        {
            MonthStart = request.PeriodStartDate,
            MonthEnd = request.PeriodEndDate,
            TotalTeachers = teachers.Count,
            PaidTeachers = 0,
            PendingTeachers = teachers.Count,
            TotalBaseSalary = Math.Round(salaryPayments.Sum(s => s.BaseSalary), 2),
            TotalDeductions = Math.Round(salaryPayments.Sum(s => s.Deductions), 2),
            TotalBonus = Math.Round(salaryPayments.Sum(s => s.Bonus), 2),
            TotalNetSalary = Math.Round(salaryPayments.Sum(s => s.NetSalary), 2),
            PaymentDetails = salaryPayments.Select((s, i) => new SalaryPaymentDto
            {
                Id = s.Id,
                TeacherId = s.TeacherId,
                TeacherName = $"{teachers[i].FirstName} {teachers[i].LastName}",
                PeriodStartDate = s.PeriodStartDate,
                PeriodEndDate = s.PeriodEndDate,
                BaseSalary = s.BaseSalary,
                Deductions = s.Deductions,
                Bonus = s.Bonus,
                NetSalary = s.NetSalary,
                Status = s.Status.ToString(),
                CreatedAt = s.CreatedAt
            }).ToList()
        };

        return report;
    }
}

public class DeleteSalaryPaymentCommandHandler : IRequestHandler<DeleteSalaryPaymentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteSalaryPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteSalaryPaymentCommand request, CancellationToken cancellationToken)
    {
        var salary = await _context.SalaryPayments.FindAsync(new object[] { request.SalaryPaymentId }, cancellationToken);
        if (salary == null)
            throw new InvalidOperationException($"Salary payment with ID {request.SalaryPaymentId} not found");

        // Only allow deletion if not paid
        if (salary.Status == SalaryPaymentStatus.Paid)
            throw new InvalidOperationException("Cannot delete a salary payment that has been paid");

        _context.SalaryPayments.Remove(salary);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class UpdateSalaryPaymentCommandHandler : IRequestHandler<UpdateSalaryPaymentCommand, SalaryPaymentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSalaryPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentDto> Handle(UpdateSalaryPaymentCommand request, CancellationToken cancellationToken)
    {
        var salary = await _context.SalaryPayments
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == request.SalaryPaymentId, cancellationToken);

        if (salary == null)
            throw new InvalidOperationException($"Salary payment with ID {request.SalaryPaymentId} not found");

        // Cannot update if already paid
        if (salary.Status == SalaryPaymentStatus.Paid)
            throw new InvalidOperationException("Cannot update a salary payment that has already been paid");

        // Cannot update if cancelled
        if (salary.Status == SalaryPaymentStatus.Cancelled)
            throw new InvalidOperationException("Cannot update a cancelled salary payment");

        if (request.BaseSalary.HasValue)
            salary.BaseSalary = request.BaseSalary.Value;

        if (request.Deductions.HasValue)
            salary.Deductions = request.Deductions.Value;

        if (request.Bonus.HasValue)
            salary.Bonus = request.Bonus.Value;

        // Recalculate net salary
        salary.NetSalary = salary.BaseSalary + salary.Bonus - salary.Deductions;

        if (!string.IsNullOrWhiteSpace(request.Remarks))
            salary.Remarks = request.Remarks;

        salary.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(salary);
    }

    private SalaryPaymentDto MapToDto(SalaryPayment salary)
    {
        return new SalaryPaymentDto
        {
            Id = salary.Id,
            TeacherId = salary.TeacherId,
            TeacherName = salary.Teacher?.FullName ?? $"{salary.Teacher?.FirstName} {salary.Teacher?.LastName}",
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
            CreatedAt = salary.CreatedAt,
            UpdatedAt = salary.UpdatedAt ?? salary.CreatedAt
        };
    }
}

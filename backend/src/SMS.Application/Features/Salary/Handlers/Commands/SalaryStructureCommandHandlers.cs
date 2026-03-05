using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Salary.Commands;
using SMS.Application.Features.Salary.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Salary.Handlers.Commands;

public class CreateSalaryStructureCommandHandler : IRequestHandler<CreateSalaryStructureCommand, SalaryStructureDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSalaryStructureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryStructureDto> Handle(CreateSalaryStructureCommand request, CancellationToken cancellationToken)
    {
        var structure = new SalaryStructure
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            BaseSalary = request.BaseSalary,
            HRA = request.HRA,
            DA = request.DA,
            MedicalAllowance = request.MedicalAllowance,
            ConveyanceAllowance = request.ConveyanceAllowance,
            OtherAllowances = request.OtherAllowances,
            StandardDeduction = request.StandardDeduction,
            MinExperienceYears = request.MinExperienceYears,
            ApplicableQualifications = request.ApplicableQualifications,
            EffectiveFromDate = request.EffectiveFromDate,
            EffectiveToDate = request.EffectiveToDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.SalaryStructures.Add(structure);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(structure);
    }

    private SalaryStructureDto MapToDto(SalaryStructure structure)
    {
        return new SalaryStructureDto
        {
            Id = structure.Id,
            Name = structure.Name,
            Description = structure.Description,
            BaseSalary = structure.BaseSalary,
            HRA = structure.HRA,
            DA = structure.DA,
            MedicalAllowance = structure.MedicalAllowance,
            ConveyanceAllowance = structure.ConveyanceAllowance,
            OtherAllowances = structure.OtherAllowances,
            StandardDeduction = structure.StandardDeduction,
            GrossSalary = structure.GrossSalary,
            TotalAllowances = structure.TotalAllowances,
            MinExperienceYears = structure.MinExperienceYears,
            ApplicableQualifications = structure.ApplicableQualifications,
            IsActive = structure.IsActive,
            EffectiveFromDate = structure.EffectiveFromDate,
            EffectiveToDate = structure.EffectiveToDate,
            CreatedAt = structure.CreatedAt,
            UpdatedAt = structure.UpdatedAt
        };
    }
}

public class UpdateSalaryStructureCommandHandler : IRequestHandler<UpdateSalaryStructureCommand, SalaryStructureDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSalaryStructureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryStructureDto> Handle(UpdateSalaryStructureCommand request, CancellationToken cancellationToken)
    {
        var structure = await _context.SalaryStructures.FindAsync(new object[] { request.Id }, cancellationToken);
        if (structure == null)
            throw new InvalidOperationException($"Salary structure with ID {request.Id} not found");

        structure.Name = request.Name;
        structure.Description = request.Description;
        structure.BaseSalary = request.BaseSalary;
        structure.HRA = request.HRA;
        structure.DA = request.DA;
        structure.MedicalAllowance = request.MedicalAllowance;
        structure.ConveyanceAllowance = request.ConveyanceAllowance;
        structure.OtherAllowances = request.OtherAllowances;
        structure.StandardDeduction = request.StandardDeduction;
        structure.MinExperienceYears = request.MinExperienceYears;
        structure.ApplicableQualifications = request.ApplicableQualifications;
        structure.EffectiveFromDate = request.EffectiveFromDate;
        structure.EffectiveToDate = request.EffectiveToDate;
        structure.UpdatedAt = DateTime.UtcNow;

        _context.SalaryStructures.Update(structure);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(structure);
    }

    private SalaryStructureDto MapToDto(SalaryStructure structure)
    {
        return new SalaryStructureDto
        {
            Id = structure.Id,
            Name = structure.Name,
            Description = structure.Description,
            BaseSalary = structure.BaseSalary,
            HRA = structure.HRA,
            DA = structure.DA,
            MedicalAllowance = structure.MedicalAllowance,
            ConveyanceAllowance = structure.ConveyanceAllowance,
            OtherAllowances = structure.OtherAllowances,
            StandardDeduction = structure.StandardDeduction,
            GrossSalary = structure.GrossSalary,
            TotalAllowances = structure.TotalAllowances,
            MinExperienceYears = structure.MinExperienceYears,
            ApplicableQualifications = structure.ApplicableQualifications,
            IsActive = structure.IsActive,
            EffectiveFromDate = structure.EffectiveFromDate,
            EffectiveToDate = structure.EffectiveToDate,
            CreatedAt = structure.CreatedAt,
            UpdatedAt = structure.UpdatedAt
        };
    }
}

public class DeleteSalaryStructureCommandHandler : IRequestHandler<DeleteSalaryStructureCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteSalaryStructureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteSalaryStructureCommand request, CancellationToken cancellationToken)
    {
        var structure = await _context.SalaryStructures.FindAsync(new object[] { request.Id }, cancellationToken);
        if (structure == null)
            throw new InvalidOperationException($"Salary structure with ID {request.Id} not found");

        _context.SalaryStructures.Remove(structure);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class AssignSalaryStructureToTeacherCommandHandler : IRequestHandler<AssignSalaryStructureToTeacherCommand, TeacherSalaryAssignmentDto>
{
    private readonly IApplicationDbContext _context;

    public AssignSalaryStructureToTeacherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherSalaryAssignmentDto> Handle(AssignSalaryStructureToTeacherCommand request, CancellationToken cancellationToken)
    {
        var teacher = await _context.Teachers
            .Include(t => t.SalaryStructure)
            .FirstOrDefaultAsync(t => t.Id == request.TeacherId, cancellationToken);

        if (teacher == null)
            throw new InvalidOperationException($"Teacher with ID {request.TeacherId} not found");

        var structure = await _context.SalaryStructures.FindAsync(new object[] { request.SalaryStructureId }, cancellationToken);
        if (structure == null)
            throw new InvalidOperationException($"Salary structure with ID {request.SalaryStructureId} not found");

        teacher.SalaryStructureId = request.SalaryStructureId;
        teacher.SalaryStructureEffectiveDate = request.EffectiveDate;

        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync(cancellationToken);

        return new TeacherSalaryAssignmentDto
        {
            TeacherId = teacher.Id,
            TeacherName = teacher.FullName,
            TeacherEmail = teacher.Email,
            TeacherImagePath = teacher.ImagePath,
            SalaryStructureId = structure.Id,
            SalaryStructureName = structure.Name,
            GrossSalary = structure.GrossSalary,
            EffectiveDate = request.EffectiveDate,
            AssignedAt = DateTime.UtcNow
        };
    }
}

public class RemoveSalaryStructureAssignmentCommandHandler : IRequestHandler<RemoveSalaryStructureAssignmentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveSalaryStructureAssignmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveSalaryStructureAssignmentCommand request, CancellationToken cancellationToken)
    {
        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Id == request.TeacherId, cancellationToken);

        if (teacher == null)
            throw new InvalidOperationException($"Teacher with ID {request.TeacherId} not found");

        if (teacher.SalaryStructureId == null)
            throw new InvalidOperationException($"Teacher {teacher.FullName} does not have any salary structure assigned");

        teacher.SalaryStructureId = null;
        teacher.SalaryStructureEffectiveDate = null;

        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class BulkCreateSalaryFromStructuresCommandHandler : IRequestHandler<BulkCreateSalaryFromStructuresCommand, SalaryPaymentReportDto>
{
    private readonly IApplicationDbContext _context;

    public BulkCreateSalaryFromStructuresCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryPaymentReportDto> Handle(BulkCreateSalaryFromStructuresCommand request, CancellationToken cancellationToken)
    {
        var teachers = await _context.Teachers
            .Include(t => t.SalaryStructure)
            .Where(t => t.IsActive && t.SalaryStructureId != null)
            .ToListAsync(cancellationToken);

        var salaryPayments = new List<SalaryPayment>();

        foreach (var teacher in teachers)
        {
            if (teacher.SalaryStructure == null)
                continue;

            var netSalary = teacher.SalaryStructure.GrossSalary - request.FixedDeductions;

            var salary = new SalaryPayment
            {
                Id = Guid.NewGuid(),
                TeacherId = teacher.Id,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate,
                BaseSalary = teacher.SalaryStructure.BaseSalary,
                Deductions = request.FixedDeductions,
                Bonus = 0,
                NetSalary = Math.Max(0, netSalary), // Ensure non-negative
                Status = SalaryPaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            salaryPayments.Add(salary);
        }

        _context.SalaryPayments.AddRange(salaryPayments);
        await _context.SaveChangesAsync(cancellationToken);

        var paymentDetails = salaryPayments.Select(s => MapToDto(s, teachers.First(t => t.Id == s.TeacherId))).ToList();

        return new SalaryPaymentReportDto
        {
            MonthStart = request.PeriodStartDate,
            MonthEnd = request.PeriodEndDate,
            TotalTeachers = salaryPayments.Count,
            PaidTeachers = 0,
            PendingTeachers = salaryPayments.Count,
            TotalBaseSalary = salaryPayments.Sum(s => s.BaseSalary),
            TotalDeductions = salaryPayments.Sum(s => s.Deductions),
            TotalBonus = 0,
            TotalNetSalary = salaryPayments.Sum(s => s.NetSalary),
            PaymentDetails = paymentDetails
        };
    }

    private SalaryPaymentDto MapToDto(SalaryPayment salary, Teacher teacher)
    {
        return new SalaryPaymentDto
        {
            Id = salary.Id,
            TeacherId = salary.TeacherId,
            TeacherName = teacher.FullName,
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

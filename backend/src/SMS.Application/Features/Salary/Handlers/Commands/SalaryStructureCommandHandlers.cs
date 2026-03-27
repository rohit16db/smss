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

public class AssignSalaryStructureToStaffCommandHandler : IRequestHandler<AssignSalaryStructureToStaffCommand, StaffSalaryAssignmentDto>
{
    private readonly IApplicationDbContext _context;

    public AssignSalaryStructureToStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffSalaryAssignmentDto> Handle(AssignSalaryStructureToStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff
            .Include(t => t.UserProfile)
            .Include(t => t.SalaryStructure)
            .FirstOrDefaultAsync(t => t.Id == request.StaffId, cancellationToken);

        if (staff == null)
            throw new InvalidOperationException($"Staff with ID {request.StaffId} not found");

        var structure = await _context.SalaryStructures.FindAsync(new object[] { request.SalaryStructureId }, cancellationToken);
        if (structure == null)
            throw new InvalidOperationException($"Salary structure with ID {request.SalaryStructureId} not found");

        staff.SalaryStructureId = request.SalaryStructureId;
        staff.SalaryStructureEffectiveDate = request.EffectiveDate;

        _context.Staff.Update(staff);
        await _context.SaveChangesAsync(cancellationToken);

        return new StaffSalaryAssignmentDto
        {
            StaffId = staff.Id,
            StaffName = staff.FullName,
            StaffEmail = staff.UserProfile.Email,
            StaffImagePath = staff.UserProfile.ImagePath,
            SalaryStructureId = structure.Id,
            SalaryStructureName = structure.Name,
            GrossSalary = structure.GrossSalary,
            EffectiveDate = request.EffectiveDate,
            AssignedAt = DateTime.UtcNow
        };
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
        var staffList = await _context.Staff
            .Include(t => t.SalaryStructure)
            .Include(t => t.UserProfile)
            .Where(t => t.IsActive && t.SalaryStructureId != null)
            .ToListAsync(cancellationToken);

        var salaryPayments = new List<SalaryPayment>();

        foreach (var staff in staffList)
        {
            if (staff.SalaryStructure == null)
                continue;

            var netSalary = staff.SalaryStructure.GrossSalary - request.FixedDeductions;

            var salary = new SalaryPayment
            {
                Id = Guid.NewGuid(),
                StaffId = staff.Id,
                PeriodStartDate = request.PeriodStartDate,
                PeriodEndDate = request.PeriodEndDate,
                BaseSalary = staff.SalaryStructure.BaseSalary,
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

        var paymentDetails = salaryPayments.Select(s => MapToDto(s, staffList.First(t => t.Id == s.StaffId))).ToList();

        return new SalaryPaymentReportDto
        {
            MonthStart = request.PeriodStartDate,
            MonthEnd = request.PeriodEndDate,
            TotalStaff = salaryPayments.Count,
            PaidStaff = 0,
            PendingStaff = salaryPayments.Count,
            TotalBaseSalary = salaryPayments.Sum(s => s.BaseSalary),
            TotalDeductions = salaryPayments.Sum(s => s.Deductions),
            TotalBonus = 0,
            TotalNetSalary = salaryPayments.Sum(s => s.NetSalary),
            PaymentDetails = paymentDetails
        };
    }

    private SalaryPaymentDto MapToDto(SalaryPayment salary, Staff staff)
    {
        return new SalaryPaymentDto
        {
            Id = salary.Id,
            StaffId = salary.StaffId,
            StaffName = staff.FullName,
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

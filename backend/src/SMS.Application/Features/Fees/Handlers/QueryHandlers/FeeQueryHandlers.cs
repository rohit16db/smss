using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Fees.DTOs;
using SMS.Application.Features.Fees.Queries;

namespace SMS.Application.Features.Fees.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetFeeStructureByIdQuery
/// </summary>
public class GetFeeStructureByIdQueryHandler : IRequestHandler<GetFeeStructureByIdQuery, FeeStructureDto?>
{
    private readonly IApplicationDbContext _context;

    public GetFeeStructureByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FeeStructureDto?> Handle(GetFeeStructureByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var feeStructureId))
            return null;

        var feeStructure = await _context.FeeStructures
            .Include(f => f.Categories)
            .FirstOrDefaultAsync(f => f.Id == feeStructureId, cancellationToken);

        if (feeStructure == null)
            return null;

        return new FeeStructureDto
        {
            Id = feeStructure.Id.ToString(),
            Name = feeStructure.Name,
            AcademicYear = feeStructure.AcademicYear,
            Frequency = feeStructure.Frequency,
            TotalAmount = feeStructure.TotalAmount,
            IsActive = feeStructure.IsActive,
            Categories = feeStructure.Categories.Select(c => new FeeStructureCategoryDto
            {
                Category = c.Category,
                Amount = c.Amount
            }).ToList(),
            CreatedAt = feeStructure.CreatedAt,
            UpdatedAt = feeStructure.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for GetAllFeeStructuresQuery
/// </summary>
public class GetAllFeeStructuresQueryHandler : IRequestHandler<GetAllFeeStructuresQuery, PaginatedFeeStructureListDto>
{
    private readonly IApplicationDbContext _context;

    public GetAllFeeStructuresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedFeeStructureListDto> Handle(GetAllFeeStructuresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FeeStructures.AsQueryable();

        // Apply filters
        if (request.IsActive.HasValue)
            query = query.Where(f => f.IsActive == request.IsActive.Value);

        if (request.AcademicYear.HasValue)
            query = query.Where(f => f.AcademicYear == request.AcademicYear.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(f => f.Name.ToLower().Contains(searchLower));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var feeStructures = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FeeStructureListDto
            {
                Id = f.Id.ToString(),
                Name = f.Name,
                AcademicYear = f.AcademicYear,
                Frequency = f.Frequency,
                TotalAmount = f.TotalAmount,
                IsActive = f.IsActive
            })
            .ToListAsync(cancellationToken);

        return new PaginatedFeeStructureListDto
        {
            Items = feeStructures,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for GetActiveFeeStructuresQuery
/// </summary>
public class GetActiveFeeStructuresQueryHandler : IRequestHandler<GetActiveFeeStructuresQuery, List<FeeStructureListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveFeeStructuresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FeeStructureListDto>> Handle(GetActiveFeeStructuresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FeeStructures
            .Where(f => f.IsActive);

        if (request.AcademicYear.HasValue)
            query = query.Where(f => f.AcademicYear == request.AcademicYear.Value);

        return await query
            .OrderBy(f => f.Name)
            .Select(f => new FeeStructureListDto
            {
                Id = f.Id.ToString(),
                Name = f.Name,
                AcademicYear = f.AcademicYear,
                Frequency = f.Frequency,
                TotalAmount = f.TotalAmount,
                IsActive = f.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for GetStudentFeesByStudentIdQuery
/// </summary>
public class GetStudentFeesByStudentIdQueryHandler : IRequestHandler<GetStudentFeesByStudentIdQuery, List<StudentFeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentFeesByStudentIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentFeeDto>> Handle(GetStudentFeesByStudentIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId))
            return new List<StudentFeeDto>();

        var query = _context.StudentFees
            .Where(sf => sf.StudentId == studentId);

        if (request.IsActive.HasValue)
            query = query.Where(sf => sf.IsActive == request.IsActive.Value);

        var studentFees = await query
            .Include(sf => sf.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .OrderByDescending(sf => sf.CreatedAt)
            .ToListAsync(cancellationToken);

        return studentFees.Select(sf => 
        {
            var paidAmount = sf.Payments?.Sum(p => p.AmountPaid) ?? 0;
            return new StudentFeeDto
            {
                Id = sf.Id.ToString(),
                StudentId = sf.StudentId.ToString(),
                StudentName = sf.Student != null ? $"{sf.Student.FirstName} {sf.Student.LastName}" : "N/A",
                EnrollmentNumber = sf.Student?.EnrollmentNumber ?? "N/A",
                FeeStructureId = sf.FeeStructureId.ToString(),
                FeeStructureName = sf.FeeStructure?.Name ?? string.Empty,
                StartDate = sf.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                EndDate = sf.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                TotalAmount = sf.TotalAmount,
                PaidAmount = paidAmount,
                BalanceAmount = sf.TotalAmount - paidAmount,
                IsActive = sf.IsActive,
                CreatedAt = sf.CreatedAt
            };
        }).ToList();
    }
}

/// <summary>
/// Handler for GetAllStudentFeesQuery
/// </summary>
public class GetAllStudentFeesQueryHandler : IRequestHandler<GetAllStudentFeesQuery, PaginatedStudentFeeListDto>
{
    private readonly IApplicationDbContext _context;

    public GetAllStudentFeesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedStudentFeeListDto> Handle(GetAllStudentFeesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StudentFees.AsQueryable();

        // Apply filters
        if (request.IsActive.HasValue)
            query = query.Where(sf => sf.IsActive == request.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(request.StudentId))
        {
            if (Guid.TryParse(request.StudentId, out var studentId))
            {
                query = query.Where(sf => sf.StudentId == studentId);
            }
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var studentFees = await query
            .Include(sf => sf.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .OrderByDescending(sf => sf.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedStudentFeeListDto
        {
            Items = studentFees.Select(sf =>
            {
                var paidAmount = sf.Payments?.Sum(p => p.AmountPaid) ?? 0;
                return new StudentFeeDto
                {
                    Id = sf.Id.ToString(),
                    StudentId = sf.StudentId.ToString(),
                    StudentName = sf.Student != null ? $"{sf.Student.FirstName} {sf.Student.LastName}" : "N/A",
                    EnrollmentNumber = sf.Student?.EnrollmentNumber ?? "N/A",
                    FeeStructureId = sf.FeeStructureId.ToString(),
                    FeeStructureName = sf.FeeStructure?.Name ?? string.Empty,
                    StartDate = sf.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    EndDate = sf.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    TotalAmount = sf.TotalAmount,
                    PaidAmount = paidAmount,
                    BalanceAmount = sf.TotalAmount - paidAmount,
                    IsActive = sf.IsActive,
                    CreatedAt = sf.CreatedAt
                };
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for GetStudentFeeByIdQuery
/// </summary>
public class GetStudentFeeByIdQueryHandler : IRequestHandler<GetStudentFeeByIdQuery, StudentFeeDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStudentFeeByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentFeeDto?> Handle(GetStudentFeeByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var studentFeeId))
            return null;

        var studentFee = await _context.StudentFees
            .Include(sf => sf.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .FirstOrDefaultAsync(sf => sf.Id == studentFeeId, cancellationToken);

        if (studentFee == null)
            return null;

        var paidAmount = studentFee.Payments?.Sum(p => p.AmountPaid) ?? 0;
        return new StudentFeeDto
        {
            Id = studentFee.Id.ToString(),
            StudentId = studentFee.StudentId.ToString(),
            StudentName = studentFee.Student != null ? $"{studentFee.Student.FirstName} {studentFee.Student.LastName}" : "N/A",
            EnrollmentNumber = studentFee.Student?.EnrollmentNumber ?? "N/A",
            FeeStructureId = studentFee.FeeStructureId.ToString(),
            FeeStructureName = studentFee.FeeStructure?.Name ?? string.Empty,
            StartDate = studentFee.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = studentFee.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            TotalAmount = studentFee.TotalAmount,
            PaidAmount = paidAmount,
            BalanceAmount = studentFee.TotalAmount - paidAmount,
            IsActive = studentFee.IsActive,
            CreatedAt = studentFee.CreatedAt
        };
    }
}

/// <summary>
/// Handler for GetFeePaymentsByStudentFeeIdQuery
/// </summary>
public class GetFeePaymentsByStudentFeeIdQueryHandler : IRequestHandler<GetFeePaymentsByStudentFeeIdQuery, List<FeePaymentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeePaymentsByStudentFeeIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FeePaymentDto>> Handle(GetFeePaymentsByStudentFeeIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentFeeId, out var studentFeeId))
            return new List<FeePaymentDto>();

        var payments = await _context.FeePayments
            .Where(p => p.StudentFeeId == studentFeeId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        return payments.Select(p => new FeePaymentDto
        {
            Id = p.Id.ToString(),
            StudentFeeId = p.StudentFeeId.ToString(),
            AmountPaid = p.AmountPaid,
            PaymentDate = p.PaymentDate.ToDateTime(TimeOnly.MinValue),
            ReceiptNumber = p.ReceiptNumber,
            PaymentMethod = p.PaymentMethod,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt
        }).ToList();
    }
}
/// <summary>
/// Handler for GetAllFeePaymentsQuery
/// </summary>
public class GetAllFeePaymentsQueryHandler : IRequestHandler<GetAllFeePaymentsQuery, PaginatedFeePaymentListDto>
{
    private readonly IApplicationDbContext _context;

    public GetAllFeePaymentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedFeePaymentListDto> Handle(GetAllFeePaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FeePayments.AsQueryable();

        // Filter by StudentFeeId if provided
        if (!string.IsNullOrWhiteSpace(request.StudentFeeId))
        {
            if (Guid.TryParse(request.StudentFeeId, out var studentFeeId))
            {
                query = query.Where(fp => fp.StudentFeeId == studentFeeId);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var payments = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedFeePaymentListDto
        {
            Items = payments.Select(p => new FeePaymentDto
            {
                Id = p.Id.ToString(),
                StudentFeeId = p.StudentFeeId.ToString(),
                AmountPaid = p.AmountPaid,
                PaymentDate = p.PaymentDate.ToDateTime(TimeOnly.MinValue),
                ReceiptNumber = p.ReceiptNumber,
                PaymentMethod = p.PaymentMethod,
                Notes = p.Notes,
                CreatedAt = p.CreatedAt
            }).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
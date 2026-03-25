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
            AcademicYearId = feeStructure.AcademicYearId.ToString(),
            AcademicYearName = feeStructure.AcademicYear?.Name ?? string.Empty,
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
    private readonly IAcademicYearContext _academicYearContext;

    public GetAllFeeStructuresQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<PaginatedFeeStructureListDto> Handle(GetAllFeeStructuresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FeeStructures.AsQueryable();

        // Apply filters
        query = query.Where(f => f.AcademicYearId == _academicYearContext.RequiredAcademicYearId);

        if (!string.IsNullOrEmpty(request.AcademicYearId) && Guid.TryParse(request.AcademicYearId, out var ayId))
            query = query.Where(f => f.AcademicYearId == ayId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLower();
            query = query.Where(f => f.Name.ToLower().Contains(searchLower));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var feeStructures = await query
            .Include(f => f.AcademicYear)
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FeeStructureListDto
            {
                Id = f.Id.ToString(),
                Name = f.Name,
                AcademicYearId = f.AcademicYearId.ToString(),
                AcademicYearName = f.AcademicYear != null ? f.AcademicYear.Name : string.Empty,
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
    private readonly IAcademicYearContext _academicYearContext;

    public GetActiveFeeStructuresQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<List<FeeStructureListDto>> Handle(GetActiveFeeStructuresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FeeStructures
            .Where(f => f.IsActive && f.AcademicYearId == _academicYearContext.RequiredAcademicYearId);

        if (!string.IsNullOrEmpty(request.AcademicYearId) && Guid.TryParse(request.AcademicYearId, out var ayId))
            query = query.Where(f => f.AcademicYearId == ayId);

        return await query
            .Include(f => f.AcademicYear)
            .OrderBy(f => f.Name)
            .Select(f => new FeeStructureListDto
            {
                Id = f.Id.ToString(),
                Name = f.Name,
                AcademicYearId = f.AcademicYearId.ToString(),
                AcademicYearName = f.AcademicYear != null ? f.AcademicYear.Name : string.Empty,
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
            .Where(sf => sf.Enrollment.StudentId == studentId);

        if (request.IsActive.HasValue)
            query = query.Where(sf => sf.IsActive == request.IsActive.Value);

        var studentFees = await query
            .Include(sf => sf.Enrollment)
            .ThenInclude(e => e.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .OrderByDescending(sf => sf.CreatedAt)
            .ToListAsync(cancellationToken);

        // Get student's current section
        var currentSection = await _context.Enrollments
            .Where(ss => ss.StudentId == studentId && ss.Status == "Enrolled")
            .Select(ss => new { ss.SectionId, ss.Section!.SectionName })
            .FirstOrDefaultAsync(cancellationToken);

        return studentFees.Select(sf => 
        {
            var paidAmount = sf.Payments?.Sum(p => p.AmountPaid) ?? 0;
            return new StudentFeeDto
            {
                Id = sf.Id.ToString(),
                StudentId = sf.Enrollment?.StudentId.ToString() ?? "",
                StudentName = sf.Enrollment?.Student != null ? $"{sf.Enrollment.Student.FirstName} {sf.Enrollment.Student.LastName}" : "N/A",
                EnrollmentNumber = sf.Enrollment?.Student?.EnrollmentNumber ?? "N/A",
                FeeStructureId = sf.FeeStructureId.ToString(),
                FeeStructureName = sf.FeeStructure?.Name ?? string.Empty,
                StartDate = sf.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                EndDate = sf.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                TotalAmount = sf.TotalAmount,
                PaidAmount = paidAmount,
                BalanceAmount = sf.TotalAmount - paidAmount,
                IsActive = sf.IsActive,
                SectionId = currentSection?.SectionId.ToString(),
                SectionName = currentSection?.SectionName,
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
                query = query.Where(sf => sf.Enrollment.StudentId == studentId);
            }
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var studentFees = await query
            .Include(sf => sf.Enrollment)
            .ThenInclude(e => e.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .OrderByDescending(sf => sf.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Get current sections for all students in this result set
        var studentIds = studentFees.Where(sf => sf.Enrollment != null).Select(sf => sf.Enrollment!.StudentId).Distinct().ToList();
        var studentSections = await _context.Enrollments
            .Where(ss => studentIds.Contains(ss.StudentId) && ss.Status == "Enrolled")
            .Select(ss => new { ss.StudentId, ss.SectionId, ss.Section!.SectionName })
            .ToListAsync(cancellationToken);

        var sectionMap = studentSections.ToDictionary(x => x.StudentId, x => new { x.SectionId, x.SectionName });

        return new PaginatedStudentFeeListDto
        {
            Items = studentFees.Select(sf =>
            {
                var paidAmount = sf.Payments?.Sum(p => p.AmountPaid) ?? 0;
                var sectionInfo = sf.Enrollment != null && sectionMap.ContainsKey(sf.Enrollment.StudentId) ? sectionMap[sf.Enrollment.StudentId] : null;
                return new StudentFeeDto
                {
                    Id = sf.Id.ToString(),
                    StudentId = sf.Enrollment?.StudentId.ToString() ?? "",
                    StudentName = sf.Enrollment?.Student != null ? $"{sf.Enrollment.Student.FirstName} {sf.Enrollment.Student.LastName}" : "N/A",
                    EnrollmentNumber = sf.Enrollment?.Student?.EnrollmentNumber ?? "N/A",
                    FeeStructureId = sf.FeeStructureId.ToString(),
                    FeeStructureName = sf.FeeStructure?.Name ?? string.Empty,
                    StartDate = sf.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    EndDate = sf.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                    TotalAmount = sf.TotalAmount,
                    PaidAmount = paidAmount,
                    BalanceAmount = sf.TotalAmount - paidAmount,
                    IsActive = sf.IsActive,
                    SectionId = sectionInfo?.SectionId.ToString(),
                    SectionName = sectionInfo?.SectionName,
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
            .Include(sf => sf.Enrollment)
            .ThenInclude(e => e.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .FirstOrDefaultAsync(sf => sf.Id == studentFeeId, cancellationToken);

        if (studentFee == null)
            return null;

        // Get student's current section
        var currentSection = await _context.Enrollments
            .Where(ss => studentFee.Enrollment != null && ss.StudentId == studentFee.Enrollment.StudentId && ss.Status == "Enrolled")
            .Select(ss => new { ss.SectionId, ss.Section!.SectionName })
            .FirstOrDefaultAsync(cancellationToken);

        var paidAmount = studentFee.Payments?.Sum(p => p.AmountPaid) ?? 0;
        return new StudentFeeDto
        {
            Id = studentFee.Id.ToString(),
            StudentId = studentFee.Enrollment?.StudentId.ToString() ?? "",
            StudentName = studentFee.Enrollment?.Student != null ? $"{studentFee.Enrollment.Student.FirstName} {studentFee.Enrollment.Student.LastName}" : "N/A",
            EnrollmentNumber = studentFee.Enrollment?.Student?.EnrollmentNumber ?? "N/A",
            FeeStructureId = studentFee.FeeStructureId.ToString(),
            FeeStructureName = studentFee.FeeStructure?.Name ?? string.Empty,
            StartDate = studentFee.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = studentFee.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            TotalAmount = studentFee.TotalAmount,
            PaidAmount = paidAmount,
            BalanceAmount = studentFee.TotalAmount - paidAmount,
            IsActive = studentFee.IsActive,
            SectionId = currentSection?.SectionId.ToString(),
            SectionName = currentSection?.SectionName,
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

/// <summary>
/// Handler for GetFeesBySectionQuery
/// Retrieves all student fees for students in a specific section
/// </summary>
public class GetFeesBySectionQueryHandler : IRequestHandler<GetFeesBySectionQuery, List<StudentFeeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeesBySectionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentFeeDto>> Handle(GetFeesBySectionQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.SectionId, out var sectionId))
            return new List<StudentFeeDto>();

        // Get all students in this section
        var enrollmentsInSection = await _context.Enrollments
            .Where(ss => ss.SectionId == sectionId && ss.Status == "Enrolled")
            .Select(ss => ss.Id)
            .ToListAsync(cancellationToken);

        if (!enrollmentsInSection.Any())
            return new List<StudentFeeDto>();

        var query = _context.StudentFees
            .Where(sf => enrollmentsInSection.Contains(sf.EnrollmentId));

        if (request.IsActive.HasValue)
            query = query.Where(sf => sf.IsActive == request.IsActive.Value);

        var studentFees = await query
            .Include(sf => sf.Enrollment)
            .ThenInclude(e => e.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .OrderBy(sf => sf.Enrollment!.Student!.EnrollmentNumber)
            .ToListAsync(cancellationToken);

        // Get section name for context
        var section = await _context.Sections
            .FirstOrDefaultAsync(s => s.Id == sectionId, cancellationToken);

        return studentFees.Select(sf =>
        {
            var paidAmount = sf.Payments?.Sum(p => p.AmountPaid) ?? 0;
            return new StudentFeeDto
            {
                Id = sf.Id.ToString(),
                StudentId = sf.Enrollment?.StudentId.ToString() ?? "",
                StudentName = sf.Enrollment?.Student != null ? $"{sf.Enrollment.Student.FirstName} {sf.Enrollment.Student.LastName}" : "N/A",
                EnrollmentNumber = sf.Enrollment?.Student?.EnrollmentNumber ?? "N/A",
                FeeStructureId = sf.FeeStructureId.ToString(),
                FeeStructureName = sf.FeeStructure?.Name ?? string.Empty,
                StartDate = sf.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                EndDate = sf.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                TotalAmount = sf.TotalAmount,
                PaidAmount = paidAmount,
                BalanceAmount = sf.TotalAmount - paidAmount,
                IsActive = sf.IsActive,
                SectionId = sectionId.ToString(),
                SectionName = section?.SectionName,
                CreatedAt = sf.CreatedAt
            };
        }).ToList();
    }
}

/// <summary>
/// Handler for GetFeeReportQuery
/// Generates a fee report with payment status and summary statistics
/// </summary>
public class GetFeeReportQueryHandler : IRequestHandler<GetFeeReportQuery, PaginatedFeeReportListDto>
{
    private readonly IApplicationDbContext _context;

    public GetFeeReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedFeeReportListDto> Handle(GetFeeReportQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StudentFees
            .Where(sf => sf.IsActive == true);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.StudentId))
        {
            if (Guid.TryParse(request.StudentId, out var studentId))
            {
                query = query.Where(sf => sf.Enrollment.StudentId == studentId);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SectionId) && Guid.TryParse(request.SectionId, out var sectionIdGuid))
        {
            var enrollmentsInSection = await _context.Enrollments
                .Where(ss => ss.SectionId == sectionIdGuid && ss.Status == "Enrolled")
                .Select(ss => ss.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(sf => enrollmentsInSection.Contains(sf.EnrollmentId));
        }

        // Filter by date range
        if (request.StartDate.HasValue)
        {
            var startDateOnly = DateOnly.FromDateTime(request.StartDate.Value);
            query = query.Where(sf => sf.StartDate >= startDateOnly);
        }

        if (request.EndDate.HasValue)
        {
            var endDateOnly = DateOnly.FromDateTime(request.EndDate.Value);
            query = query.Where(sf => sf.StartDate <= endDateOnly);
        }

        // Get all student fees with includes
        var allStudentFees = await query
            .Include(sf => sf.Enrollment)
            .ThenInclude(e => e.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .OrderBy(sf => sf.Enrollment!.Student!.EnrollmentNumber)
            .ToListAsync(cancellationToken);

        // Get current sections for all students
        var studentIds = allStudentFees.Where(sf => sf.Enrollment != null).Select(sf => sf.Enrollment!.StudentId).Distinct().ToList();
        var studentSections = await _context.Enrollments
            .Where(ss => studentIds.Contains(ss.StudentId) && ss.Status == "Enrolled")
            .Select(ss => new { ss.StudentId, ss.SectionId, ss.Section!.SectionName })
            .ToListAsync(cancellationToken);

        var sectionMap = studentSections.ToDictionary(x => x.StudentId, x => new { x.SectionId, x.SectionName });

        // Calculate status and create report items
        var reportItems = allStudentFees.Select(sf =>
        {
            var paidAmount = sf.Payments?.Sum(p => p.AmountPaid) ?? 0;
            var balanceAmount = sf.TotalAmount - paidAmount;
            var lastPaymentDate = sf.Payments?.Any() == true 
                ? sf.Payments.Max(p => p.PaymentDate).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                : (DateTime?)null;

            var status = CalculateStatus(sf.TotalAmount, paidAmount, balanceAmount, sf.StartDate);

            var sectionInfo = sf.Enrollment != null && sectionMap.ContainsKey(sf.Enrollment.StudentId) ? sectionMap[sf.Enrollment.StudentId] : null;

            return new FeeReportDto
            {
                Id = sf.Id.ToString(),
                StudentId = sf.Enrollment?.StudentId.ToString() ?? "",
                StudentName = sf.Enrollment?.Student != null ? $"{sf.Enrollment.Student.FirstName} {sf.Enrollment.Student.LastName}" : "N/A",
                EnrollmentNumber = sf.Enrollment?.Student?.EnrollmentNumber ?? "N/A",
                SectionId = sectionInfo?.SectionId.ToString(),
                SectionName = sectionInfo?.SectionName,
                FeeStructureId = sf.FeeStructureId.ToString(),
                FeeStructureName = sf.FeeStructure?.Name ?? string.Empty,
                TotalAmount = sf.TotalAmount,
                PaidAmount = paidAmount,
                BalanceAmount = balanceAmount,
                Status = status,
                LastPaymentDate = lastPaymentDate,
                StartDate = sf.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                DueDate = CalculateDueDate(sf.StartDate, sf.FeeStructure?.Frequency)
            };
        }).ToList();

        // Filter by status if provided
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            reportItems = reportItems.Where(r => r.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Calculate summary statistics
        var totalDueAmount = reportItems.Sum(r => r.TotalAmount);
        var totalPaidAmount = reportItems.Sum(r => r.PaidAmount);
        var totalBalanceAmount = reportItems.Sum(r => r.BalanceAmount);
        var paidCount = reportItems.Count(r => r.Status == "Paid");
        var partialCount = reportItems.Count(r => r.Status == "Partial");
        var dueCount = reportItems.Count(r => r.Status == "Due");
        var overdueCount = reportItems.Count(r => r.Status == "Overdue");

        var totalCount = reportItems.Count;

        // Apply pagination
        var paginatedItems = reportItems
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedFeeReportListDto
        {
            Items = paginatedItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalDueAmount = totalDueAmount,
            TotalPaidAmount = totalPaidAmount,
            TotalBalanceAmount = totalBalanceAmount,
            PaidCount = paidCount,
            PartialCount = partialCount,
            DueCount = dueCount,
            OverdueCount = overdueCount
        };
    }

    private static string CalculateStatus(decimal totalAmount, decimal paidAmount, decimal balanceAmount, DateOnly startDate)
    {
        if (balanceAmount <= 0)
            return "Paid";

        if (paidAmount > 0 && balanceAmount < totalAmount)
            return "Partial";

        // Check if overdue (e.g., more than 30 days from start date)
        var dueDate = startDate.AddMonths(1);
        if (DateOnly.FromDateTime(DateTime.UtcNow) > dueDate)
            return "Overdue";

        return "Due";
    }

    private static DateTime? CalculateDueDate(DateOnly startDate, string? frequency)
    {
        // Calculate due date based on frequency
        return frequency?.ToLower() switch
        {
            "monthly" => startDate.AddMonths(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            "quarterly" => startDate.AddMonths(3).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            "half-yearly" => startDate.AddMonths(6).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            "yearly" => startDate.AddYears(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            _ => startDate.AddMonths(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) // Default to monthly
        };
    }
}

/// <summary>
/// Handler for GetFeeReceiptDataQuery
/// Fetches data required for fee receipt PDF generation
/// </summary>
public class GetFeeReceiptDataQueryHandler : IRequestHandler<GetFeeReceiptDataQuery, FeeReceiptDto?>
{
    private readonly IApplicationDbContext _context;

    public GetFeeReceiptDataQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FeeReceiptDto?> Handle(GetFeeReceiptDataQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.PaymentId, out var paymentId))
            return null;

        // Get the payment with related student fee, student, structure, and section
        var payment = await _context.FeePayments
            .Include(p => p.StudentFee)
            .ThenInclude(sf => sf!.Enrollment)
            .ThenInclude(e => e!.Student)
            .Include(p => p.StudentFee)
            .ThenInclude(sf => sf!.Enrollment)
            .ThenInclude(e => e!.Section)
            .ThenInclude(s => s!.Class)
            .Include(p => p.StudentFee)
            .ThenInclude(sf => sf!.FeeStructure)
            .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

        if (payment?.StudentFee == null)
            return null;

        var studentFee = payment.StudentFee;
        var student = studentFee.Enrollment?.Student;
        var feeStructure = studentFee.FeeStructure;

        if (student == null || feeStructure == null)
            return null;

        // Get student's current section info
        var studentSection = studentFee.Enrollment;

        // Get total paid for this student fee (all payments)
        var totalPaid = await _context.FeePayments
            .Where(fp => fp.StudentFeeId == studentFee.Id)
            .SumAsync(fp => fp.AmountPaid, cancellationToken);

        var previousBalance = totalPaid - payment.AmountPaid;
        var currentBalance = (decimal)studentFee.TotalAmount - totalPaid;

        // Get active school settings for the receipt
        var school = await _context.Schools
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        return new FeeReceiptDto
        {
            ReceiptNumber = payment.ReceiptNumber,
            StudentName = $"{student.FirstName} {student.LastName}",
            EnrollmentNumber = student.EnrollmentNumber,
            ClassName = studentSection?.Section?.Class?.Name ?? "N/A",
            SectionName = studentSection?.Section?.SectionName ?? "N/A",
            FeeStructureName = feeStructure.Name,
            AmountPaid = payment.AmountPaid,
            PaymentDate = payment.PaymentDate.ToDateTime(TimeOnly.MinValue),
            PaymentMethod = payment.PaymentMethod,
            Notes = payment.Notes,
            PreviousBalance = Math.Max(0, previousBalance),
            CurrentBalance = Math.Max(0, currentBalance),
            TotalDueAmount = studentFee.TotalAmount,
            SchoolName = school?.Name ?? "School Management System",
            SchoolAddress = !string.IsNullOrEmpty(school?.Address) ? $"{school.Address}, {school.City}" : "123 Education Street, City",
            SchoolPhone = school?.PhoneNumber ?? "+91-XXXX-XXXX",
            SchoolEmail = school?.EmailAddress,
            SchoolWebsite = school?.Website,
            SchoolCode = school?.Code
        };
    }
}
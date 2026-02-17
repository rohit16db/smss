using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Subjects.DTOs;
using SMS.Application.Features.Subjects.Queries;

namespace SMS.Application.Features.Subjects.Handlers.QueryHandlers;

/// <summary>
/// Handler for getting all subjects with pagination
/// </summary>
public class GetAllSubjectsQueryHandler : IRequestHandler<GetAllSubjectsQuery, PaginatedSubjectListDto>
{
    private readonly IApplicationDbContext _context;

    public GetAllSubjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedSubjectListDto> Handle(GetAllSubjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Subjects.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(s => s.Name.Contains(request.SearchTerm) || 
                                     s.Code.Contains(request.SearchTerm));
        }

        // Apply active filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var subjects = await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SubjectListDto
            {
                Id = s.Id.ToString(),
                Name = s.Name,
                Code = s.Code,
                Credits = s.Credits,
                IsActive = s.IsActive,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new PaginatedSubjectListDto
        {
            Items = subjects,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for getting a subject by ID
/// </summary>
public class GetSubjectByIdQueryHandler : IRequestHandler<GetSubjectByIdQuery, SubjectDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSubjectByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubjectDto?> Handle(GetSubjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var subjectId))
            return null;

        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == subjectId, cancellationToken);

        if (subject == null)
            return null;

        return new SubjectDto
        {
            Id = subject.Id.ToString(),
            Name = subject.Name,
            Code = subject.Code,
            Description = subject.Description,
            Credits = subject.Credits,
            IsActive = subject.IsActive,
            DisplayOrder = subject.DisplayOrder,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for getting all active subjects (for dropdowns)
/// </summary>
public class GetActiveSubjectsQueryHandler : IRequestHandler<GetActiveSubjectsQuery, List<SubjectListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveSubjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubjectListDto>> Handle(GetActiveSubjectsQuery request, CancellationToken cancellationToken)
    {
        var subjects = await _context.Subjects
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Select(s => new SubjectListDto
            {
                Id = s.Id.ToString(),
                Name = s.Name,
                Code = s.Code,
                Credits = s.Credits,
                IsActive = s.IsActive,
                DisplayOrder = s.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return subjects;
    }
}

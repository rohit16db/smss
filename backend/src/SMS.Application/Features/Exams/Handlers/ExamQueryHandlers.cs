using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

namespace SMS.Application.Features.Exams.Handlers;

public class ExamQueryHandlers
{
    public class GetExamsQueryHandler : IRequestHandler<GetExamsQuery, PaginatedResult<ExamDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAcademicYearContext _academicYearContext;

        public GetExamsQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
        {
            _context = context;
            _academicYearContext = academicYearContext;
        }
        
        public async Task<PaginatedResult<ExamDto>> Handle(GetExamsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Exams
                .Where(e => e.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
                .AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(request.Status))
                query = query.Where(e => e.Status.ToString() == request.Status);

            // Count total before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var exams = await query
                .OrderByDescending(e => e.StartDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new ExamDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    TotalMarks = e.TotalMarks,
                    PassMarks = e.PassMarks,
                    Status = e.Status.ToString(),
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new PaginatedResult<ExamDto>
            {
                Data = exams,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }

    public class GetExamByIdQueryHandler : IRequestHandler<GetExamByIdQuery, ExamDetailDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAcademicYearContext _academicYearContext;

        public GetExamByIdQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
        {
            _context = context;
            _academicYearContext = academicYearContext;
        }
        
        public async Task<ExamDetailDto> Handle(GetExamByIdQuery request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamSubjects)
                    .ThenInclude(es => es.Subject)
                .Include(e => e.ExamClasses)
                    .ThenInclude(ec => ec.Class)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            // Get student counts for each class
            // Step 1: Get all sections for all classes
            var classIds = exam.ExamClasses.Select(ec => ec.ClassId).ToList();
            var sectionsByClass = await _context.Sections
                .Where(s => classIds.Contains(s.ClassId))
                .Select(s => new { s.ClassId, s.Id })
                .ToListAsync(cancellationToken);

            // Step 2: Get count of current students for each section in the active academic year
            var studentCountsBySection = await _context.Enrollments
                .Where(ss => ss.Status == "Enrolled" && ss.AcademicYearId == _academicYearContext.RequiredAcademicYearId) 
                .GroupBy(ss => ss.SectionId)
                .Select(g => new { SectionId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            // Step 3: Map section counts to class counts
            var classStudentCounts = sectionsByClass
                .GroupBy(sb => sb.ClassId)
                .ToDictionary(g => g.Key, g => 
                    g.Sum(sb => studentCountsBySection.FirstOrDefault(sc => sc.SectionId == sb.Id)?.Count ?? 0)
                );

            return new ExamDetailDto
            {
                Id = exam.Id,
                Name = exam.Name,
                Description = exam.Description,
                StartDate = exam.StartDate,
                EndDate = exam.EndDate,
                TotalMarks = exam.TotalMarks,
                PassMarks = exam.PassMarks,
                Status = exam.Status.ToString(),
                Subjects = exam.ExamSubjects.Select(es => new ExamSubjectDto
                {
                    SubjectId = es.SubjectId,
                    SubjectName = es.Subject!.Name,
                    MaxMarks = es.MaxMarks,
                    PassMarks = es.PassMarks
                }).ToList(),
                Classes = exam.ExamClasses.Select(ec => new ExamClassDto
                {
                    ClassId = ec.ClassId,
                    ClassName = ec.Class!.Name,
                    StudentCount = classStudentCounts.ContainsKey(ec.ClassId) ? classStudentCounts[ec.ClassId] : 0,
                    MarksEntryStatus = ec.MarksEntryStatus.ToString(),
                    SubmittedAt = ec.SubmittedAt
                }).ToList(),
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt
            };
        }
    }
}

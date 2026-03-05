using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

#nullable disable
namespace SMS.Application.Features.Exams.Handlers;

/// <summary>
/// CQRS Query Handlers for Analytics
/// Single Responsibility: Handle analytics queries with database queries
/// </summary>
public class AnalyticsQueryHandlers
{
    public class GetExamAnalyticsQueryHandler : IRequestHandler<GetExamAnalyticsQuery, ExamAnalyticsDto>
    {
        private readonly IApplicationDbContext _context;
        public GetExamAnalyticsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ExamAnalyticsDto> Handle(GetExamAnalyticsQuery request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams
                .Include(e => e.StudentReportCards)
                .ThenInclude(rc => rc.Student)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            // Load all student sections at once to avoid N+1 queries
            var studentIds = exam.StudentReportCards.Select(rc => rc.StudentId).Distinct().ToList();
            var studentSectionsQuery = _context.StudentSections
                .Where(ss => studentIds.Contains(ss.StudentId) && ss.IsCurrent);

            // Filter by class if provided
            if (request.ClassId.HasValue)
            {
                studentSectionsQuery = studentSectionsQuery.Where(ss => ss.Section != null && ss.Section.ClassId == request.ClassId.Value);
            }

            var studentSections = await studentSectionsQuery.Include(ss => ss.Section).ToListAsync(cancellationToken);

            // Filter report cards to only include students in the selected class (if classId provided)
            var filteredStudentIds = studentSections.Select(ss => ss.StudentId).ToList();
            var reportCards = request.ClassId.HasValue
                ? exam.StudentReportCards.Where(rc => filteredStudentIds.Contains(rc.StudentId)).ToList()
                : exam.StudentReportCards.ToList();

            var totalStudents = reportCards.Count;
            var passedStudents = reportCards.Count(rc => rc.Pass);
            var failedStudents = totalStudents - passedStudents;
            var passRate = totalStudents > 0 ? (passedStudents * 100.0m) / totalStudents : 0;
            var classAverage = totalStudents > 0 ? reportCards.Average(rc => rc.Percentage) : 0;

            // Grade distribution
            var gradeDistribution = reportCards
                .GroupBy(rc => rc.OverallGrade)
                .Select(g => new GradeDistributionDto
                {
                    Grade = g.Key,
                    Count = g.Count(),
                    Percentage = (g.Count() * 100.0m) / totalStudents
                })
                .OrderByDescending(gd => gd.Grade)
                .ToList();

            // Top 5 performers
            var topPerformers = reportCards
                .OrderByDescending(rc => rc.Percentage)
                .Take(5)
                .Select(rc => new StudentPerformanceDto
                {
                    StudentId = rc.StudentId,
                    StudentName = rc.Student != null ? $"{rc.Student.FirstName} {rc.Student.LastName}" : "Unknown Student",
                    RollNumber = studentSections.FirstOrDefault(ss => ss.StudentId == rc.StudentId)?.RollNumber?.ToString() ?? "",
                    MarksObtained = rc.TotalMarksObtained,
                    Percentage = rc.Percentage,
                    Grade = rc.OverallGrade,
                    ClassPosition = 0
                })
                .ToList();

            // Bottom 5 performers
            var bottomPerformers = reportCards
                .OrderBy(rc => rc.Percentage)
                .Take(5)
                .Select(rc => new StudentPerformanceDto
                {
                    StudentId = rc.StudentId,
                    StudentName = rc.Student != null ? $"{rc.Student.FirstName} {rc.Student.LastName}" : "Unknown Student",
                    RollNumber = studentSections.FirstOrDefault(ss => ss.StudentId == rc.StudentId)?.RollNumber?.ToString() ?? "",
                    MarksObtained = rc.TotalMarksObtained,
                    Percentage = rc.Percentage,
                    Grade = rc.OverallGrade,
                    ClassPosition = 0
                })
                .ToList();

            // Subject-wise analysis - Load student marks and group by subject
            var studentMarks = await _context.StudentMarks
                .Where(m => m.ExamId == request.ExamId && studentIds.Contains(m.StudentId))
                .Include(m => m.ExamSubject)
                .ThenInclude(es => es.Subject)
                .ToListAsync(cancellationToken);

            // Filter student marks by class if provided
            if (request.ClassId.HasValue)
            {
                studentMarks = studentMarks
                    .Where(m => filteredStudentIds.Contains(m.StudentId))
                    .ToList();
            }

            var subjectAnalysis = studentMarks
                .GroupBy(m => m.SubjectId)
                .Select(g => new SubjectAnalysisDto
                {
                    SubjectId = g.Key,
                    SubjectName = g.First().ExamSubject?.Subject?.Name ?? "Unknown Subject",
                    MaxMarks = (int)(g.First().ExamSubject?.MaxMarks ?? 0),
                    AverageMarks = g.Where(m => m.MarksObtained.HasValue).Any() 
                        ? g.Where(m => m.MarksObtained.HasValue).Average(m => m.MarksObtained!.Value)
                        : 0,
                    AveragePercentage = g.Where(m => m.MarksObtained.HasValue).Any()
                        ? g.Where(m => m.MarksObtained.HasValue).Average(m => (m.MarksObtained!.Value / (decimal)(g.First().ExamSubject?.MaxMarks ?? 1)) * 100)
                        : 0,
                    HighestMarks = g.Where(m => m.MarksObtained.HasValue).Any()
                        ? g.Where(m => m.MarksObtained.HasValue).Max(m => m.MarksObtained!.Value)
                        : 0,
                    LowestMarks = g.Where(m => m.MarksObtained.HasValue && !m.IsAbsent).Any()
                        ? g.Where(m => m.MarksObtained.HasValue && !m.IsAbsent).Min(m => m.MarksObtained!.Value)
                        : 0,
                    PassCount = g.Count(m => m.MarksObtained.HasValue && (m.MarksObtained!.Value / (decimal)(g.First().ExamSubject?.MaxMarks ?? 1)) * 100 >= (g.First().ExamSubject?.PassMarks ?? 40)),
                    FailCount = g.Count(m => !m.MarksObtained.HasValue || (m.MarksObtained!.Value / (decimal)(g.First().ExamSubject?.MaxMarks ?? 1)) * 100 < (g.First().ExamSubject?.PassMarks ?? 40)),
                    PassPercentage = g.Any() 
                        ? (g.Count(m => m.MarksObtained.HasValue && (m.MarksObtained!.Value / (decimal)(g.First().ExamSubject?.MaxMarks ?? 1)) * 100 >= (g.First().ExamSubject?.PassMarks ?? 40)) * 100.0m) / g.Count()
                        : 0
                })
                .ToList();

            return new ExamAnalyticsDto
            {
                ExamId = exam.Id,
                ExamName = exam.Name,
                StartDate = exam.StartDate,
                EndDate = exam.EndDate,
                TotalStudents = totalStudents,
                PassedStudents = passedStudents,
                FailedStudents = failedStudents,
                PassRate = passRate,
                ClassAverage = classAverage,
                ClassAverageMarks = reportCards.Any() ? reportCards.Average(rc => rc.TotalMarksObtained) : 0,
                GradeDistribution = gradeDistribution,
                TopPerformers = topPerformers,
                BottomPerformers = bottomPerformers,
                SubjectAnalysis = subjectAnalysis
            };
        }
    }

    public class GetClassPerformanceQueryHandler : IRequestHandler<GetClassPerformanceQuery, ClassPerformanceDto>
    {
        private readonly IApplicationDbContext _context;
        public GetClassPerformanceQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ClassPerformanceDto> Handle(GetClassPerformanceQuery request, CancellationToken cancellationToken)
        {
            var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
            if (classEntity == null)
                throw new InvalidOperationException($"Class with ID {request.ClassId} not found");

            var sectionIds = await _context.Sections
                .Where(s => s.ClassId == request.ClassId)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            var studentIds = await _context.StudentSections
                .Where(ss => sectionIds.Contains(ss.SectionId) && ss.IsCurrent)
                .Select(ss => ss.StudentId)
                .ToListAsync(cancellationToken);

            var reportCards = await _context.StudentReportCards
                .Where(rc => rc.ExamId == request.ExamId && studentIds.Contains(rc.StudentId))
                .ToListAsync(cancellationToken);

            var totalStudents = studentIds.Count;
            var passedStudents = reportCards.Count(rc => rc.Pass);
            var passPercentage = totalStudents > 0 ? (passedStudents * 100.0m) / totalStudents : 0;
            var classAverage = reportCards.Any() ? reportCards.Average(rc => rc.Percentage) : 0;

            // Subject-wise analysis
            var studentMarksForClass = await _context.StudentMarks
                .Where(m => m.ExamId == request.ExamId && studentIds.Contains(m.StudentId))
                .Include(m => m.ExamSubject)
                .ThenInclude(es => es.Subject)
                .ToListAsync(cancellationToken);

            var subjectAnalysis = studentMarksForClass
                .GroupBy(m => m.SubjectId)
                .Select(g => new SubjectAnalysisDto
                {
                    SubjectId = g.Key,
                    SubjectName = g.First().ExamSubject?.Subject?.Name ?? "Unknown Subject",
                    MaxMarks = (int)(g.First().ExamSubject?.MaxMarks ?? 0),
                    AverageMarks = g.Where(m => m.MarksObtained.HasValue).Any()
                        ? g.Where(m => m.MarksObtained.HasValue).Average(m => m.MarksObtained!.Value)
                        : 0,
                    AveragePercentage = g.Where(m => m.MarksObtained.HasValue).Any()
                        ? g.Where(m => m.MarksObtained.HasValue).Average(m => (m.MarksObtained!.Value / (decimal)(g.First().ExamSubject?.MaxMarks ?? 1)) * 100)
                        : 0,
                    HighestMarks = g.Where(m => m.MarksObtained.HasValue).Any()
                        ? g.Where(m => m.MarksObtained.HasValue).Max(m => m.MarksObtained!.Value)
                        : 0,
                    LowestMarks = g.Where(m => m.MarksObtained.HasValue && !m.IsAbsent).Any()
                        ? g.Where(m => m.MarksObtained.HasValue && !m.IsAbsent).Min(m => m.MarksObtained!.Value)
                        : 0,
                    PassCount = g.Count(m => m.MarksObtained.HasValue && (m.MarksObtained!.Value / (decimal)(g.First().ExamSubject?.MaxMarks ?? 1)) * 100 >= (g.First().ExamSubject?.PassMarks ?? 40)),
                    FailCount = g.Count(m => !m.MarksObtained.HasValue || (m.MarksObtained!.Value / (decimal)(g.First().ExamSubject?.MaxMarks ?? 1)) * 100 < (g.First().ExamSubject?.PassMarks ?? 40)),
                    PassPercentage = g.Any()
                        ? (g.Count(m => m.MarksObtained.HasValue && (m.MarksObtained!.Value / (decimal)(g.First().ExamSubject?.MaxMarks ?? 1)) * 100 >= (g.First().ExamSubject?.PassMarks ?? 40)) * 100.0m) / g.Count()
                        : 0
                })
                .ToList();

            return new ClassPerformanceDto
            {
                ClassId = request.ClassId,
                ClassName = classEntity.Name,
                ExamId = request.ExamId,
                ExamName = "",
                TotalEnrolled = totalStudents,
                AppearedCount = reportCards.Count,
                AbsentCount = totalStudents - reportCards.Count,
                PassCount = passedStudents,
                FailCount = totalStudents - passedStudents,
                PassPercentage = passPercentage,
                ClassAverage = classAverage,
                ClassAveragePercentage = classAverage,
                HighestMarks = reportCards.Any() ? reportCards.Max(rc => rc.TotalMarksObtained) : 0,
                LowestMarks = reportCards.Any() ? reportCards.Min(rc => rc.TotalMarksObtained) : 0,
                SubjectWiseAnalysis = subjectAnalysis,
                StudentsPassed = passedStudents,
                StudentsFailed = totalStudents - passedStudents
            };
        }
    }

    public class GetStudentPerformanceTrendQueryHandler : IRequestHandler<GetStudentPerformanceTrendQuery, StudentPerformanceTrendDto>
    {
        private readonly IApplicationDbContext _context;
        public GetStudentPerformanceTrendQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<StudentPerformanceTrendDto> Handle(GetStudentPerformanceTrendQuery request, CancellationToken cancellationToken)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);
            if (student == null)
                throw new InvalidOperationException($"Student with ID {request.StudentId} not found");

            // Get current student section to get roll number and class info
            var studentSection = await _context.StudentSections
                .Where(ss => ss.StudentId == request.StudentId && ss.IsCurrent)
                .Include(ss => ss.Section)
                .FirstOrDefaultAsync(cancellationToken);

            var reportCards = await _context.StudentReportCards
                .Where(rc => rc.StudentId == request.StudentId)
                .Include(rc => rc.Exam)
                .OrderBy(rc => rc.Exam!.StartDate)
                .ToListAsync(cancellationToken);

            var performanceHistory = reportCards
                .Select(rc => new ExamPerformancePointDto
                {
                    ExamId = rc.ExamId,
                    ExamName = rc.Exam!.Name,
                    StartDate = rc.Exam.StartDate,
                    EndDate = rc.Exam.EndDate,
                    MarksObtained = rc.TotalMarksObtained,
                    Percentage = rc.Percentage,
                    Grade = rc.OverallGrade,
                    ClassPosition = 0,
                    Passed = rc.Pass
                })
                .ToList();

            return new StudentPerformanceTrendDto
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}",
                RollNumber = studentSection != null ? (studentSection.RollNumber ?? 0).ToString() : "",
                ClassId = studentSection?.SectionId ?? Guid.Empty,
                ClassName = studentSection?.Section?.SectionName ?? "Unknown",
                PerformanceHistory = performanceHistory,
                AveragePercentage = performanceHistory.Any() ? performanceHistory.Average(p => p.Percentage) : 0,
                HighestPercentage = performanceHistory.Any() ? performanceHistory.Max(p => p.Percentage) : 0,
                LowestPercentage = performanceHistory.Any() ? performanceHistory.Min(p => p.Percentage) : 0
            };
        }
    }

    public class GetClassComparativeAnalysisQueryHandler : IRequestHandler<GetClassComparativeAnalysisQuery, ClassComparativeAnalysisDto>
    {
        private readonly IApplicationDbContext _context;
        public GetClassComparativeAnalysisQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ClassComparativeAnalysisDto> Handle(GetClassComparativeAnalysisQuery request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamClasses)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            var classComparisons = new List<ClassComparisonItemDto>();

            foreach (var ec in exam.ExamClasses)
            {
                var sectionIds = await _context.Sections
                    .Where(s => s.ClassId == ec.ClassId)
                    .Select(s => s.Id)
                    .ToListAsync(cancellationToken);

                var studentIds = await _context.StudentSections
                    .Where(ss => sectionIds.Contains(ss.SectionId) && ss.IsCurrent)
                    .Select(ss => ss.StudentId)
                    .ToListAsync(cancellationToken);

                var reportCards = await _context.StudentReportCards
                    .Where(rc => rc.ExamId == request.ExamId && studentIds.Contains(rc.StudentId))
                    .ToListAsync(cancellationToken);

                var totalStudents = studentIds.Count;
                var passedStudents = reportCards.Count(rc => rc.Pass);
                var passPercentage = totalStudents > 0 ? (passedStudents * 100.0m) / totalStudents : 0;

                classComparisons.Add(new ClassComparisonItemDto
                {
                    ClassId = ec.ClassId,
                    ClassName = _context.Classes.First(c => c.Id == ec.ClassId).Name,
                    EnrolledCount = totalStudents,
                    PassCount = passedStudents,
                    PassPercentage = passPercentage,
                    ClassAverage = reportCards.Any() ? reportCards.Average(rc => rc.Percentage) : 0
                });
            }

            return new ClassComparativeAnalysisDto
            {
                ExamId = exam.Id,
                ExamName = exam.Name,
                ClassComparisons = classComparisons.OrderByDescending(c => c.ClassAverage).ToList()
            };
        }
    }

    public class GetMarksDistributionQueryHandler : IRequestHandler<GetMarksDistributionQuery, MarksDistributionDto>
    {
        private readonly IApplicationDbContext _context;
        public GetMarksDistributionQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<MarksDistributionDto> Handle(GetMarksDistributionQuery request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            // Build query for report cards
            var reportCardsQuery = _context.StudentReportCards
                .Include(rc => rc.Exam)
                .Include(rc => rc.Student)
                .ThenInclude(s => s.StudentSections)
                .ThenInclude(ss => ss.Section)
                .Where(rc => rc.ExamId == request.ExamId);

            // Filter by class if provided
            if (request.ClassId.HasValue)
            {
                reportCardsQuery = reportCardsQuery.Where(rc =>
                    rc.Student != null && rc.Student.StudentSections != null && rc.Student.StudentSections.Any(ss =>
                        ss.Section != null && ss.Section.ClassId == request.ClassId &&
                        rc.Exam != null && (ss.LeftDate == null || ss.LeftDate > rc.Exam.StartDate)));
            }

            var reportCards = await reportCardsQuery.ToListAsync(cancellationToken);

            var bucketSize = request.BucketSize > 0 ? request.BucketSize : 10;
            var buckets = new List<MarkRangeBucketDto>();
            var maxMarks = (decimal)exam.TotalMarks;

            // Create buckets based on exam total marks
            for (decimal i = 0; i <= maxMarks; i += bucketSize)
            {
                var start = i;
                var end = Math.Min(i + bucketSize - 1, maxMarks);
                var count = reportCards.Count(rc => rc.TotalMarksObtained >= start && rc.TotalMarksObtained <= end);
                var percentage = reportCards.Any() ? (count * 100.0m) / reportCards.Count : 0;

                buckets.Add(new MarkRangeBucketDto
                {
                    RangeLabel = $"{(int)start}-{(int)end}",
                    StartMark = (int)start,
                    EndMark = (int)end,
                    StudentCount = count,
                    Percentage = percentage
                });
            }

            return new MarksDistributionDto
            {
                ExamId = exam.Id,
                Buckets = buckets,
                Total = reportCards.Count
            };
        }
    }

    public class GetExamComparisonQueryHandler : IRequestHandler<GetExamComparisonQuery, ExamComparisonAnalysisDto>
    {
        private readonly IApplicationDbContext _context;
        public GetExamComparisonQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ExamComparisonAnalysisDto> Handle(GetExamComparisonQuery request, CancellationToken cancellationToken)
        {
            var studentIds = await _context.StudentSections
                .Where(ss => ss.SectionId == request.ClassId)
                .Select(ss => ss.StudentId)
                .ToListAsync(cancellationToken);

            var exams = await _context.Exams
                .OrderByDescending(e => e.StartDate)
                .Take(request.LimitToLastNExams ?? 10)
                .ToListAsync(cancellationToken);

            var comparisons = new List<ExamComparisonItemDto>();

            foreach (var exam in exams)
            {
                var reportCards = await _context.StudentReportCards
                    .Where(rc => rc.ExamId == exam.Id && studentIds.Contains(rc.StudentId))
                    .ToListAsync(cancellationToken);

                var totalStudents = studentIds.Count;
                var passedStudents = reportCards.Count(rc => rc.Pass);

                comparisons.Add(new ExamComparisonItemDto
                {
                    ExamId = exam.Id,
                    ExamName = exam.Name,
                    StartDate = exam.StartDate,
                    EndDate = exam.EndDate,
                    PassCount = passedStudents,
                    TotalStudents = totalStudents,
                    PassPercentage = totalStudents > 0 ? (passedStudents * 100.0m) / totalStudents : 0,
                    ClassAverage = reportCards.Any() ? reportCards.Average(rc => rc.Percentage) : 0
                });
            }

            return new ExamComparisonAnalysisDto
            {
                ClassId = request.ClassId,
                ClassName = "",
                ExamComparisons = comparisons
            };
        }
    }

    public class GetSubjectComparisonQueryHandler : IRequestHandler<GetSubjectComparisonQuery, SubjectComparisonAnalysisDto>
    {
        private readonly IApplicationDbContext _context;
        public GetSubjectComparisonQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<SubjectComparisonAnalysisDto> Handle(GetSubjectComparisonQuery request, CancellationToken cancellationToken)
        {
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);
            if (subject == null)
                throw new InvalidOperationException($"Subject with ID {request.SubjectId} not found");

            var exams = await _context.Exams
                .OrderByDescending(e => e.StartDate)
                .Take(request.LimitToLastNExams ?? 10)
                .ToListAsync(cancellationToken);

            var subjectPerformance = new List<SubjectExamComparisonDto>();

            foreach (var exam in exams)
            {
                var marks = await _context.StudentMarks
                    .Where(m => m.SubjectId == request.SubjectId && m.ExamId == exam.Id)
                    .ToListAsync(cancellationToken);

                var validMarks = marks.Where(m => m.MarksObtained.HasValue).ToList();
            subjectPerformance.Add(new SubjectExamComparisonDto
                {
                    ExamId = exam.Id,
                    ExamName = exam.Name,
                    StartDate = exam.StartDate,
                    EndDate = exam.EndDate,
                    AverageMarks = validMarks.Any() ? validMarks.Average(m => m.MarksObtained!.Value) : 0,
                    AveragePercentage = validMarks.Any() ? validMarks.Average(m => (m.MarksObtained!.Value / exam.TotalMarks) * 100) : 0,
                    PassCount = validMarks.Count(m => (m.MarksObtained!.Value / exam.TotalMarks) * 100 >= exam.PassMarks),
                    FailCount = marks.Count - validMarks.Count(m => (m.MarksObtained!.Value / exam.TotalMarks) * 100 >= exam.PassMarks)
                });
            }

            return new SubjectComparisonAnalysisDto
            {
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                ExamPerformance = subjectPerformance
            };
        }
    }

    public class GetDetailedAnalyticsReportQueryHandler : IRequestHandler<GetDetailedAnalyticsReportQuery, DetailedAnalyticsReportDto>
    {
        private readonly IApplicationDbContext _context;
        public GetDetailedAnalyticsReportQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<DetailedAnalyticsReportDto> Handle(GetDetailedAnalyticsReportQuery request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamClasses)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            var reportCards = await _context.StudentReportCards
                .Where(rc => rc.ExamId == request.ExamId)
                .Include(rc => rc.Student)
                .ToListAsync(cancellationToken);

            var studentDetails = reportCards
                .Select(rc => new StudentPerformanceDto
                {
                    StudentId = rc.StudentId,
                    StudentName = $"{rc.Student!.FirstName} {rc.Student.LastName}",
                    RollNumber = _context.StudentSections.Where(ss => ss.StudentId == rc.StudentId && ss.IsCurrent).Select(ss => (ss.RollNumber ?? 0).ToString()).FirstOrDefault() ?? "",
                    MarksObtained = rc.TotalMarksObtained,
                    Percentage = rc.Percentage,
                    Grade = rc.OverallGrade,
                    ClassPosition = 0
                })
                .OrderByDescending(s => s.Percentage)
                .ToList();

            // Add rank
            for (int i = 0; i < studentDetails.Count; i++)
                studentDetails[i].ClassPosition = i + 1;

            return new DetailedAnalyticsReportDto
            {
                ExamId = exam.Id,
                ExamName = exam.Name,
                GeneratedDate = DateTime.UtcNow,
                TotalStudents = reportCards.Count,
                StudentsAppeared = reportCards.Count,
                StudentsAbsent = 0,
                OverallPassPercentage = reportCards.Any() ? (reportCards.Count(rc => rc.Pass) * 100.0m) / reportCards.Count : 0,
                OverallClassAverage = reportCards.Any() ? reportCards.Average(rc => rc.Percentage) : 0,
                AllStudentPerformance = studentDetails
            };
        }
    }
}

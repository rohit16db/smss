using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

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
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            var reportCards = exam.StudentReportCards.ToList();
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
                    StudentName = $"{rc.Student!.FirstName} {rc.Student.LastName}",
                    RollNumber = _context.StudentSections.Where(ss => ss.StudentId == rc.StudentId && ss.IsCurrent).Select(ss => (ss.RollNumber ?? 0).ToString()).FirstOrDefault() ?? "",
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
                    StudentName = $"{rc.Student!.FirstName} {rc.Student.LastName}",
                    RollNumber = _context.StudentSections.Where(ss => ss.StudentId == rc.StudentId && ss.IsCurrent).Select(ss => (ss.RollNumber ?? 0).ToString()).FirstOrDefault() ?? "",
                    MarksObtained = rc.TotalMarksObtained,
                    Percentage = rc.Percentage,
                    Grade = rc.OverallGrade,
                    ClassPosition = 0
                })
                .ToList();

            return new ExamAnalyticsDto
            {
                ExamId = exam.Id,
                ExamName = exam.Name,
                ExamDate = exam.ExamDate,
                TotalStudents = totalStudents,
                PassedStudents = passedStudents,
                FailedStudents = failedStudents,
                PassRate = passRate,
                ClassAverage = classAverage,
                ClassAverageMarks = reportCards.Any() ? reportCards.Average(rc => rc.TotalMarksObtained) : 0,
                GradeDistribution = gradeDistribution,
                TopPerformers = topPerformers,
                BottomPerformers = bottomPerformers
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
            var subjectAnalysis = await _context.StudentMarks
                .Where(m => m.ExamId == request.ExamId && studentIds.Contains(m.StudentId))
                .Include(m => m.ExamSubject)
                .GroupBy(m => m.SubjectId)
                .Select(g => new SubjectAnalysisDto
                {
                    SubjectId = g.Key,
                    SubjectName = g.First().ExamSubject!.Subject!.Name,
                    MaxMarks = _context.Exams.First(e => e.Id == request.ExamId).TotalMarks,
                    AverageMarks = g.Where(m => m.MarksObtained.HasValue).Average(m => m.MarksObtained!.Value),
                    AveragePercentage = g.Where(m => m.MarksObtained.HasValue).Average(m => (m.MarksObtained!.Value / _context.Exams.First(e => e.Id == request.ExamId).TotalMarks) * 100),
                    HighestMarks = g.Where(m => m.MarksObtained.HasValue).Max(m => m.MarksObtained!.Value),
                    LowestMarks = g.Where(m => m.MarksObtained.HasValue && !m.IsAbsent).Min(m => m.MarksObtained!.Value),
                    PassCount = g.Count(m => m.MarksObtained.HasValue && (m.MarksObtained!.Value / _context.Exams.First(e => e.Id == request.ExamId).TotalMarks) * 100 >= _context.Exams.First(e => e.Id == request.ExamId).PassMarks),
                    FailCount = g.Count(m => !m.MarksObtained.HasValue || (m.MarksObtained!.Value / _context.Exams.First(e => e.Id == request.ExamId).TotalMarks) * 100 < _context.Exams.First(e => e.Id == request.ExamId).PassMarks),
                    PassPercentage = (g.Count(m => m.MarksObtained.HasValue && (m.MarksObtained!.Value / _context.Exams.First(e => e.Id == request.ExamId).TotalMarks) * 100 >= _context.Exams.First(e => e.Id == request.ExamId).PassMarks) * 100.0m) / g.Count()
                })
                .ToListAsync(cancellationToken);

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
                .OrderBy(rc => rc.Exam!.ExamDate)
                .ToListAsync(cancellationToken);

            var performanceHistory = reportCards
                .Select(rc => new ExamPerformancePointDto
                {
                    ExamId = rc.ExamId,
                    ExamName = rc.Exam!.Name,
                    ExamDate = rc.Exam.ExamDate,
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

            var reportCards = await _context.StudentReportCards
                .Where(rc => rc.ExamId == request.ExamId)
                .ToListAsync(cancellationToken);

            var bucketSize = request.BucketSize > 0 ? request.BucketSize : 10;
            var buckets = new List<MarkRangeBucketDto>();

            for (decimal i = 0; i < 100; i += bucketSize)
            {
                var start = i;
                var end = i + bucketSize - 1;
                var count = reportCards.Count(rc => rc.TotalMarksObtained >= start && rc.TotalMarksObtained <= end);
                var percentage = reportCards.Any() ? (count * 100.0m) / reportCards.Count : 0;

                buckets.Add(new MarkRangeBucketDto
                {
                    RangeLabel = $"{start}-{end}",
                    StartMark = (int)start,
                    EndMark = (int)end,
                    StudentCount = count,
                    Percentage = percentage
                });
            }

            return new MarksDistributionDto
            {
                ExamId = exam.Id,
                Buckets = buckets.Where(b => b.StudentCount > 0).ToList(),
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
                .OrderByDescending(e => e.ExamDate)
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
                    ExamDate = exam.ExamDate,
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
                .OrderByDescending(e => e.ExamDate)
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
                    ExamDate = exam.ExamDate,
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

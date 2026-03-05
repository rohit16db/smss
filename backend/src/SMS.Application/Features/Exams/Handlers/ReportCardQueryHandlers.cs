using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace SMS.Application.Features.Exams.Handlers;

public class ReportCardQueryHandlers
{
    public class GetReportCardQueryHandler : IRequestHandler<GetReportCardQuery, ReportCardDto>
    {
        private readonly IApplicationDbContext _context;
        public GetReportCardQueryHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<ReportCardDto> Handle(GetReportCardQuery request, CancellationToken cancellationToken)
        {
            var reportCard = await _context.StudentReportCards
                .Include(rc => rc.Exam)
                .Include(rc => rc.Student)
                .FirstOrDefaultAsync(rc => rc.ExamId == request.ExamId && rc.StudentId == request.StudentId, cancellationToken);

            if (reportCard == null)
                throw new InvalidOperationException($"Report card not found for exam {request.ExamId} and student {request.StudentId}");

            // Get student's section for class name
            var studentSection = await _context.StudentSections
                .Where(ss => ss.StudentId == request.StudentId && ss.IsCurrent)
                .Select(ss => new { ss.SectionId, ss.RollNumber })
                .FirstOrDefaultAsync(cancellationToken);

            var sectionName = studentSection != null 
                ? (await _context.Sections.FirstOrDefaultAsync(s => s.Id == studentSection.SectionId, cancellationToken))?.SectionName ?? "Unknown"
                : "Unknown";

            // Get subject-wise marks
            var studentMarks = await _context.StudentMarks
                .Where(m => m.ExamId == request.ExamId && m.StudentId == request.StudentId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Load all ExamSubjects for this exam with their Subject names
            var examSubjects = await _context.ExamSubjects
                .Where(es => es.ExamId == request.ExamId)
                .Include(es => es.Subject)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Create a lookup dictionary: (ExamId, SubjectId) -> (MaxMarks, SubjectName)
            var subjectLookup = examSubjects.ToDictionary(
                es => (es.ExamId, es.SubjectId),
                es => (es.MaxMarks, es.Subject?.Name ?? "Unknown"));

            // Map to DTO with MaxMarks from lookup
            var subjectWiseMarks = studentMarks.Select(mark =>
            {
                var key = (mark.ExamId, mark.SubjectId);
                var (maxMarks, subjectName) = subjectLookup.ContainsKey(key)
                    ? subjectLookup[key]
                    : (0m, "Unknown");

                var percentage = mark.MarksObtained.HasValue && maxMarks > 0
                    ? (mark.MarksObtained.Value / maxMarks) * 100
                    : 0;

                var grade = DetermineGrade((double)percentage);

                return new SubjectReportCardDto
                {
                    SubjectId = mark.SubjectId,
                    SubjectName = subjectName,
                    MaxMarks = maxMarks,
                    Obtained = mark.MarksObtained ?? 0,
                    Percentage = percentage,
                    Grade = grade
                };
            }).ToList();

            // Get active school settings
            var school = await _context.Schools
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            return new ReportCardDto
            {
                Id = reportCard.Id,
                StudentId = reportCard.StudentId,
                StudentName = $"{reportCard.Student!.FirstName} {reportCard.Student.LastName}",
                RollNumber = studentSection != null ? (studentSection.RollNumber ?? 0).ToString() : "",
                ClassName = sectionName,
                ExamId = reportCard.ExamId,
                ExamName = reportCard.Exam!.Name,
                StartDate = reportCard.Exam!.StartDate,
                EndDate = reportCard.Exam!.EndDate,
                SubjectMarks = subjectWiseMarks,
                Summary = new ReportCardSummaryDto
                {
                    TotalMarks = reportCard.TotalMarks,
                    TotalObtained = reportCard.TotalMarksObtained,
                    Percentage = reportCard.Percentage,
                    OverallGrade = reportCard.OverallGrade,
                    ClassPosition = reportCard.ClassPosition
                },
                AttendancePercentage = 100,
                Remarks = "",
                GeneratedAt = reportCard.GeneratedAt ?? DateTime.UtcNow,
                SchoolName = school?.Name ?? "School Management System",
                SchoolAddress = !string.IsNullOrEmpty(school?.Address) ? $"{school.Address}, {school.City}" : "123 Education Street, City",
                SchoolPhone = school?.PhoneNumber ?? "+91-XXXX-XXXX",
                SchoolEmail = school?.EmailAddress,
                SchoolWebsite = school?.Website,
                SchoolCode = school?.Code
            };
        }
    }

    public class GetExamReportCardsQueryHandler : IRequestHandler<GetExamReportCardsQuery, List<ReportCardListDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetExamReportCardsQueryHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<List<ReportCardListDto>> Handle(GetExamReportCardsQuery request, CancellationToken cancellationToken)
        {
            var reportCards = await _context.StudentReportCards
                .Where(rc => rc.ExamId == request.ExamId)
                .Include(rc => rc.Student)
                .Include(rc => rc.Exam)
                .OrderByDescending(rc => rc.Percentage)
                .Select(rc => new ReportCardListDto
                {
                    Id = rc.Id,
                    ExamId = rc.ExamId,
                    ExamName = rc.Exam!.Name,
                    StudentId = rc.StudentId,
                    StudentName = $"{rc.Student!.FirstName} {rc.Student.LastName}",
                    ClassName = "",
                    TotalObtained = rc.TotalMarksObtained,
                    TotalMarks = rc.TotalMarks,
                    Percentage = rc.Percentage,
                    OverallGrade = rc.OverallGrade,
                    ClassPosition = rc.ClassPosition,
                    Status = rc.Pass ? "Pass" : "Fail",
                    GeneratedAt = rc.GeneratedAt ?? DateTime.UtcNow
                })
                .ToListAsync(cancellationToken);

            // Get active school settings
            var school = await _context.Schools
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            // Populate school settings for all report cards
            foreach (var card in reportCards)
            {
                card.SchoolName = school?.Name ?? "School Management System";
                card.SchoolAddress = !string.IsNullOrEmpty(school?.Address) ? $"{school.Address}, {school.City}" : "123 Education Street, City";
                card.SchoolPhone = school?.PhoneNumber ?? "+91-XXXX-XXXX";
            }

            return reportCards;
        }
    }

    public class GetStudentReportCardsQueryHandler : IRequestHandler<GetStudentReportCardsQuery, List<ReportCardListDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetStudentReportCardsQueryHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<List<ReportCardListDto>> Handle(GetStudentReportCardsQuery request, CancellationToken cancellationToken)
        {
            var reportCards = await _context.StudentReportCards
                .Where(rc => rc.StudentId == request.StudentId)
                .Include(rc => rc.Exam)
                .Include(rc => rc.Student)
                .OrderByDescending(rc => rc.CreatedAt)
                .Select(rc => new ReportCardListDto
                {
                    Id = rc.Id,
                    ExamId = rc.ExamId,
                    ExamName = rc.Exam!.Name,
                    StudentId = rc.StudentId,
                    StudentName = $"{rc.Student!.FirstName} {rc.Student.LastName}",
                    ClassName = "",
                    TotalObtained = rc.TotalMarksObtained,
                    TotalMarks = rc.TotalMarks,
                    Percentage = rc.Percentage,
                    OverallGrade = rc.OverallGrade,
                    ClassPosition = rc.ClassPosition,
                    Status = rc.Pass ? "Pass" : "Fail",
                    GeneratedAt = rc.GeneratedAt ?? DateTime.UtcNow
                })
                .ToListAsync(cancellationToken);

            // Get active school settings
            var school = await _context.Schools
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            // Populate school settings for all report cards
            foreach (var card in reportCards)
            {
                card.SchoolName = school?.Name ?? "School Management System";
                card.SchoolAddress = !string.IsNullOrEmpty(school?.Address) ? $"{school.Address}, {school.City}" : "123 Education Street, City";
                card.SchoolPhone = school?.PhoneNumber ?? "+91-XXXX-XXXX";
            }

            return reportCards;
        }
    }

    public class GetReportCardByIdQueryHandler : IRequestHandler<GetReportCardByIdQuery, ReportCardDto>
    {
        private readonly IApplicationDbContext _context;
        public GetReportCardByIdQueryHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<ReportCardDto> Handle(GetReportCardByIdQuery request, CancellationToken cancellationToken)
        {
            // Get report card by ID
            var reportCard = await _context.StudentReportCards
                .Include(rc => rc.Exam)
                .Include(rc => rc.Student)
                .FirstOrDefaultAsync(rc => rc.Id == request.ReportCardId, cancellationToken);

            if (reportCard == null)
                throw new InvalidOperationException($"Report card not found with ID {request.ReportCardId}");

            // Get student's section for class name
            var studentSection = await _context.StudentSections
                .Where(ss => ss.StudentId == reportCard.StudentId && ss.IsCurrent)
                .Select(ss => new { ss.SectionId, ss.RollNumber })
                .FirstOrDefaultAsync(cancellationToken);

            var sectionName = studentSection != null 
                ? (await _context.Sections.FirstOrDefaultAsync(s => s.Id == studentSection.SectionId, cancellationToken))?.SectionName ?? "Unknown"
                : "Unknown";

            // Get subject-wise marks
            var studentMarks = await _context.StudentMarks
                .Where(m => m.ExamId == reportCard.ExamId && m.StudentId == reportCard.StudentId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Load all ExamSubjects for this exam with their Subject names
            var examSubjects = await _context.ExamSubjects
                .Where(es => es.ExamId == reportCard.ExamId)
                .Include(es => es.Subject)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Create a lookup dictionary: (ExamId, SubjectId) -> (MaxMarks, SubjectName)
            var subjectLookup = examSubjects.ToDictionary(
                es => (es.ExamId, es.SubjectId),
                es => (es.MaxMarks, es.Subject?.Name ?? "Unknown"));

            // Map to DTO with MaxMarks from lookup
            var subjectWiseMarks = studentMarks.Select(mark =>
            {
                var key = (mark.ExamId, mark.SubjectId);
                var (maxMarks, subjectName) = subjectLookup.ContainsKey(key)
                    ? subjectLookup[key]
                    : (0m, "Unknown");

                var percentage = mark.MarksObtained.HasValue && maxMarks > 0
                    ? (mark.MarksObtained.Value / maxMarks) * 100
                    : 0;

                var grade = DetermineGrade((double)percentage);

                return new SubjectReportCardDto
                {
                    SubjectId = mark.SubjectId,
                    SubjectName = subjectName,
                    MaxMarks = maxMarks,
                    Obtained = mark.MarksObtained ?? 0,
                    Percentage = percentage,
                    Grade = grade
                };
            }).ToList();

            // Get active school settings
            var school = await _context.Schools
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            return new ReportCardDto
            {
                Id = reportCard.Id,
                StudentId = reportCard.StudentId,
                StudentName = $"{reportCard.Student!.FirstName} {reportCard.Student.LastName}",
                RollNumber = studentSection != null ? (studentSection.RollNumber ?? 0).ToString() : "",
                ClassName = sectionName,
                ExamId = reportCard.ExamId,
                ExamName = reportCard.Exam!.Name,
                StartDate = reportCard.Exam!.StartDate,
                EndDate = reportCard.Exam!.EndDate,
                SubjectMarks = subjectWiseMarks,
                Summary = new ReportCardSummaryDto
                {
                    TotalMarks = reportCard.TotalMarks,
                    TotalObtained = reportCard.TotalMarksObtained,
                    Percentage = reportCard.Percentage,
                    OverallGrade = reportCard.OverallGrade,
                    ClassPosition = reportCard.ClassPosition
                },
                AttendancePercentage = 100,
                Remarks = "",
                GeneratedAt = reportCard.GeneratedAt ?? DateTime.UtcNow,
                SchoolName = school?.Name ?? "School Management System",
                SchoolAddress = !string.IsNullOrEmpty(school?.Address) ? $"{school.Address}, {school.City}" : "123 Education Street, City",
                SchoolPhone = school?.PhoneNumber ?? "+91-XXXX-XXXX",
                SchoolEmail = school?.EmailAddress,
                SchoolWebsite = school?.Website,
                SchoolCode = school?.Code
            };
        }
    }

    public class ExportReportCardPdfQueryHandler : IRequestHandler<ExportReportCardPdfQuery, byte[]>
    {
        private readonly IApplicationDbContext _context;
        public ExportReportCardPdfQueryHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<byte[]> Handle(ExportReportCardPdfQuery request, CancellationToken cancellationToken)
        {
            // Fetch report card data
            var reportCard = await _context.StudentReportCards
                .Include(rc => rc.Exam)
                .Include(rc => rc.Student)
                .FirstOrDefaultAsync(rc => rc.Id == request.CardId, cancellationToken);

            if (reportCard == null)
                throw new InvalidOperationException($"Report card not found with ID: {request.CardId}");

            // Get student's section
            var studentSection = await _context.StudentSections
                .Where(ss => ss.StudentId == reportCard.StudentId && ss.IsCurrent)
                .FirstOrDefaultAsync(cancellationToken);

            // Get subject-wise marks
            var studentMarks = await _context.StudentMarks
                .Where(m => m.ExamId == reportCard.ExamId && m.StudentId == reportCard.StudentId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Load all ExamSubjects for this exam with their Subject names
            var examSubjects = await _context.ExamSubjects
                .Where(es => es.ExamId == reportCard.ExamId)
                .Include(es => es.Subject)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Create a lookup dictionary: (ExamId, SubjectId) -> ExamSubject
            var subjectLookup = examSubjects.ToDictionary(
                es => (es.ExamId, es.SubjectId),
                es => es);

            // Map to StudentMarks with ExamSubject navigation populated
            var subjectMarksList = studentMarks.Select(mark =>
            {
                var key = (mark.ExamId, mark.SubjectId);
                if (subjectLookup.ContainsKey(key))
                {
                    mark.ExamSubject = subjectLookup[key];
                }
                return mark;
            }).ToList();

            // Get active school settings
            var school = await _context.Schools
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            var schoolName = school?.Name ?? "School Management System";
            var schoolAddress = !string.IsNullOrEmpty(school?.Address) ? $"{school.Address}, {school.City}" : "";
            var schoolPhone = school?.PhoneNumber ?? "";

            // Generate PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    
                    // Header with background
                    page.Header().Column(col =>
                    {
                        col.Item().Background("#1F2937").Padding(20).Column(inner =>
                        {
                            inner.Item().Text("STUDENT REPORT CARD").FontSize(28).Bold().FontColor("#FFFFFF");
                            inner.Item().PaddingTop(5).Text(schoolName).FontSize(12).FontColor("#D1D5DB");
                            if (!string.IsNullOrEmpty(schoolAddress))
                                inner.Item().PaddingTop(2).Text(schoolAddress).FontSize(9).FontColor("#9CA3AF");
                            if (!string.IsNullOrEmpty(schoolPhone))
                                inner.Item().PaddingTop(2).Text($"Phone: {schoolPhone}").FontSize(9).FontColor("#9CA3AF");
                        });
                        col.Item().PaddingVertical(5);
                    });

                    page.Content().Column(col =>
                    {
                        // Student Information Section
                        col.Item().Background("#F3F4F6").Padding(15).Column(studentInfo =>
                        {
                            studentInfo.Item().Text("Student Information").FontSize(14).Bold().FontColor("#1F2937");
                            studentInfo.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Column(c1 =>
                                {
                                    c1.Item().Text($"Name: {reportCard.Student!.FirstName} {reportCard.Student.LastName}").FontSize(11f).FontColor("#374151");
                                    c1.Item().PaddingTop(4).Text($"Roll Number: {(studentSection?.RollNumber ?? 0).ToString()}").FontSize(11f).FontColor("#374151");
                                });
                                row.RelativeItem().Column(c2 =>
                                {
                                    c2.Item().Text($"Exam: {reportCard.Exam!.Name}").FontSize(11f).FontColor("#374151");
                                    c2.Item().PaddingTop(4).Text($"Exam Date: {reportCard.Exam.StartDate:dd MMM yyyy}").FontSize(11f).FontColor("#374151");
                                });
                            });
                        });

                        col.Item().PaddingVertical(15);

                        // Performance Summary Cards
                        col.Item().Text("Performance Summary").FontSize(13).Bold().FontColor("#1F2937");
                        col.Item().PaddingVertical(10).Row(row =>
                        {
                            // Total Marks Card
                            row.RelativeItem().Border(1).BorderColor("#E5E7EB").Background("#EFF6FF").Padding(12).Column(c =>
                            {
                                c.Item().Text("Total Marks").FontSize(9).FontColor("#6B7280");
                                c.Item().PaddingTop(6).Text($"{reportCard.TotalMarksObtained:F0} / {reportCard.TotalMarks}").FontSize(13f).Bold().FontColor("#1E40AF");
                            });

                            row.RelativeItem().PaddingHorizontal(5);

                            // Percentage Card
                            row.RelativeItem().Border(1).BorderColor("#E5E7EB").Background("#F0FDF4").Padding(12).Column(c =>
                            {
                                c.Item().Text("Percentage").FontSize(9).FontColor("#6B7280");
                                c.Item().PaddingTop(6).Text($"{reportCard.Percentage:F2}%").FontSize(13f).Bold().FontColor("#15803D");
                            });

                            row.RelativeItem().PaddingHorizontal(5);

                            // Grade Card
                            row.RelativeItem().Border(1).BorderColor("#E5E7EB").Background("#FEF3C7").Padding(12).Column(c =>
                            {
                                c.Item().Text("Grade").FontSize(9).FontColor("#6B7280");
                                c.Item().PaddingTop(6).Text(reportCard.OverallGrade).FontSize(13f).Bold().FontColor("#D97706");
                            });

                            row.RelativeItem().PaddingHorizontal(5);

                            // Rank Card
                            row.RelativeItem().Border(1).BorderColor("#E5E7EB").Background("#F3E8FF").Padding(12).Column(c =>
                            {
                                c.Item().Text("Class Rank").FontSize(9).FontColor("#6B7280");
                                c.Item().PaddingTop(6).Text($"#{reportCard.ClassPosition}").FontSize(13f).Bold().FontColor("#7C3AED");
                            });

                            row.RelativeItem().PaddingHorizontal(5);

                            // Status Card
                            var statusBg = reportCard.Pass ? "#DCFCE7" : "#FEE2E2";
                            var statusTextColor = reportCard.Pass ? "#15803D" : "#DC2626";
                            var statusText = reportCard.Pass ? "PASS" : "FAIL";

                            row.RelativeItem().Border(1).BorderColor("#E5E7EB").Background(statusBg).Padding(12).Column(c =>
                            {
                                c.Item().Text("Status").FontSize(9).FontColor("#6B7280");
                                c.Item().PaddingTop(6).Text(statusText).FontSize(13f).Bold().FontColor(statusTextColor);
                            });
                        });

                        col.Item().PaddingVertical(15);

                        // Subject Details Table
                        col.Item().Text("Subject-wise Marks").FontSize(13).Bold().FontColor("#1F2937");
                        col.Item().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(0.8f);
                                columns.RelativeColumn(1);
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Background("#1F2937").Padding(8).Text("Subject").FontColor("#FFFFFF").Bold().FontSize(10);
                                header.Cell().Background("#1F2937").Padding(8).Text("Max Marks").FontColor("#FFFFFF").Bold().FontSize(10).AlignRight();
                                header.Cell().Background("#1F2937").Padding(8).Text("Obtained").FontColor("#FFFFFF").Bold().FontSize(10).AlignRight();
                                header.Cell().Background("#1F2937").Padding(8).Text("Percentage").FontColor("#FFFFFF").Bold().FontSize(10).AlignRight();
                                header.Cell().Background("#1F2937").Padding(8).Text("Grade").FontColor("#FFFFFF").Bold().FontSize(10).AlignCenter();
                                header.Cell().Background("#1F2937").Padding(8).Text("Result").FontColor("#FFFFFF").Bold().FontSize(10).AlignCenter();
                            });

                            // Rows
                            int rowIndex = 0;
                            foreach (var mark in subjectMarksList)
                            {
                                var percentage = mark.MarksObtained.HasValue && mark.ExamSubject!.MaxMarks > 0
                                    ? ((double)mark.MarksObtained.Value) / ((double)mark.ExamSubject.MaxMarks) * 100
                                    : 0;
                                var grade = DetermineGrade(percentage);
                                var isPassed = percentage >= 40;
                                var rowBg = rowIndex % 2 == 0 ? "#F9FAFB" : "#FFFFFF";
                                var gradeColor = percentage >= 90 ? "#059669" : percentage >= 70 ? "#0891B2" : percentage >= 50 ? "#D97706" : "#DC2626";
                                var resultText = isPassed ? "PASS" : "FAIL";
                                var resultColor = isPassed ? "#15803D" : "#DC2626";

                                table.Cell().Background(rowBg).Padding(8).Text(mark.ExamSubject!.Subject!.Name).FontSize(10);
                                table.Cell().Background(rowBg).Padding(8).Text(mark.ExamSubject.MaxMarks.ToString()).FontSize(10).AlignRight();
                                table.Cell().Background(rowBg).Padding(8).Text((mark.MarksObtained ?? 0).ToString()).FontSize(10).AlignRight().Bold();
                                table.Cell().Background(rowBg).Padding(8).Text($"{percentage:F1}%").FontSize(10).AlignRight();
                                table.Cell().Background(rowBg).Padding(8).Text(grade).FontSize(10).AlignCenter().Bold().FontColor(gradeColor);
                                table.Cell().Background(rowBg).Padding(8).Text(resultText).FontSize(10).AlignCenter().Bold().FontColor(resultColor);

                                rowIndex++;
                            }
                        });

                        col.Item().PaddingVertical(15);

                        // Overall Result Banner
                        var finalBg = reportCard.Pass ? "#DCFCE7" : "#FEE2E2";
                        var finalColor = reportCard.Pass ? "#15803D" : "#DC2626";
                        var finalText = reportCard.Pass ? "✓ STUDENT PASSED" : "✗ STUDENT FAILED";
                        var finalBorder = reportCard.Pass ? "#16A34A" : "#EF4444";

                        col.Item().Border(2).BorderColor(finalBorder).Background(finalBg).Padding(20).Column(result =>
                        {
                            result.Item().Text(finalText).FontSize(18).Bold().FontColor(finalColor).AlignCenter();
                            result.Item().PaddingTop(8).Text($"Overall Grade: {reportCard.OverallGrade}").FontSize(12).FontColor(finalColor).AlignCenter();
                        });
                    });

                    // Footer
                    page.Footer().PaddingTop(10).BorderTop(1).BorderColor("#E5E7EB").Column(footer =>
                    {
                        footer.Item().AlignCenter().Text($"Generated on: {DateTime.UtcNow:dd MMM yyyy HH:mm:ss}").FontSize(9).FontColor("#9CA3AF");
                        footer.Item().PaddingTop(5).AlignCenter().Text("This is an official document from School Management System").FontSize(8).FontColor("#D1D5DB");
                    });
                });
            });

            return document.GeneratePdf();
        }
    }

    private static string DetermineGrade(double percentage)
    {
        if (percentage >= 90) return "A";
        if (percentage >= 80) return "B";
        if (percentage >= 70) return "C";
        if (percentage >= 60) return "D";
        return "F";
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace SMS.Application.Features.Exams.Handlers;

public class ReportCardQueryHandlers
{
    public class GetReportCardQueryHandler : IRequestHandler<GetReportCardQuery, ReportCardDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAcademicYearContext _academicYearContext;

        public GetReportCardQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
        {
            _context = context;
            _academicYearContext = academicYearContext;
        }
        
        public async Task<ReportCardDto> Handle(GetReportCardQuery request, CancellationToken cancellationToken)
        {
            var reportCard = await _context.StudentReportCards
                .Include(rc => rc.Exam)
                .Include(rc => rc.Enrollment).ThenInclude(e => e!.Student)
                .FirstOrDefaultAsync(rc => rc.ExamId == request.ExamId && rc.Enrollment!.StudentId == request.StudentId, cancellationToken);

            if (reportCard == null)
                throw new InvalidOperationException($"Report card not found for exam {request.ExamId} and student {request.StudentId}");

            // Get student's section for class name in active academic year
            var studentSection = await _context.Enrollments
                .Where(ss => ss.StudentId == request.StudentId && ss.Status == "Enrolled" && ss.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
                .Select(ss => new { ss.SectionId, ss.RollNumber })
                .FirstOrDefaultAsync(cancellationToken);

            var sectionName = studentSection != null 
                ? (await _context.Sections.FirstOrDefaultAsync(s => s.Id == studentSection.SectionId, cancellationToken))?.SectionName ?? "Unknown"
                : "Unknown";

            // Get subject-wise marks
            var studentMarks = await _context.StudentMarks
                .Where(m => m.ExamId == request.ExamId && m.Enrollment!.StudentId == request.StudentId)
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
                StudentId = reportCard.Enrollment!.StudentId,
                StudentName = $"{reportCard.Enrollment.Student!.FirstName} {reportCard.Enrollment.Student.LastName}",
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
                .Include(rc => rc.Enrollment).ThenInclude(e => e!.Student)
                .Include(rc => rc.Exam)
                .OrderByDescending(rc => rc.Percentage)
                .Select(rc => new ReportCardListDto
                {
                    Id = rc.Id,
                    ExamId = rc.ExamId,
                    ExamName = rc.Exam!.Name,
                    StudentId = rc.Enrollment!.StudentId,
                    StudentName = $"{rc.Enrollment.Student!.FirstName} {rc.Enrollment.Student.LastName}",
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
                .Where(rc => rc.Enrollment!.StudentId == request.StudentId)
                .Include(rc => rc.Exam)
                .Include(rc => rc.Enrollment).ThenInclude(e => e!.Student)
                .OrderByDescending(rc => rc.CreatedAt)
                .Select(rc => new ReportCardListDto
                {
                    Id = rc.Id,
                    ExamId = rc.ExamId,
                    ExamName = rc.Exam!.Name,
                    StudentId = rc.Enrollment!.StudentId,
                    StudentName = $"{rc.Enrollment.Student!.FirstName} {rc.Enrollment.Student.LastName}",
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
        private readonly IAcademicYearContext _academicYearContext;

        public GetReportCardByIdQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
        {
            _context = context;
            _academicYearContext = academicYearContext;
        }
        
        public async Task<ReportCardDto> Handle(GetReportCardByIdQuery request, CancellationToken cancellationToken)
        {
            // Get report card by ID
            var reportCard = await _context.StudentReportCards
                .Include(rc => rc.Exam)
                .Include(rc => rc.Enrollment).ThenInclude(e => e!.Student)
                .FirstOrDefaultAsync(rc => rc.Id == request.ReportCardId, cancellationToken);

            if (reportCard == null)
                throw new InvalidOperationException($"Report card not found with ID {request.ReportCardId}");

            // Get student's section for class name in active academic year
            var studentSection = await _context.Enrollments
                .Where(ss => ss.StudentId == reportCard.Enrollment!.StudentId && ss.Status == "Enrolled" && ss.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
                .Select(ss => new { ss.SectionId, ss.RollNumber })
                .FirstOrDefaultAsync(cancellationToken);

            var sectionName = studentSection != null 
                ? (await _context.Sections.FirstOrDefaultAsync(s => s.Id == studentSection.SectionId, cancellationToken))?.SectionName ?? "Unknown"
                : "Unknown";

            // Get subject-wise marks
            var studentMarks = await _context.StudentMarks
                .Where(m => m.ExamId == reportCard.ExamId && m.Enrollment!.StudentId == reportCard.Enrollment!.StudentId)
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
                StudentId = reportCard.Enrollment!.StudentId,
                StudentName = $"{reportCard.Enrollment.Student!.FirstName} {reportCard.Enrollment.Student.LastName}",
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
        private readonly IAcademicYearContext _academicYearContext;

        public ExportReportCardPdfQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
        {
            _context = context;
            _academicYearContext = academicYearContext;
        }
        
        public async Task<byte[]> Handle(ExportReportCardPdfQuery request, CancellationToken cancellationToken)
        {
            // Fetch report card data
            var reportCard = await _context.StudentReportCards
                .Include(rc => rc.Exam)
                .Include(rc => rc.Enrollment).ThenInclude(e => e!.Student)
                .FirstOrDefaultAsync(rc => rc.Id == request.CardId, cancellationToken);

            if (reportCard == null)
                throw new InvalidOperationException($"Report card not found with ID: {request.CardId}");

            // Get student's section in active academic year
            var studentSection = await _context.Enrollments
                .Where(ss => ss.StudentId == reportCard.Enrollment!.StudentId && ss.Status == "Enrolled" && ss.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
                .FirstOrDefaultAsync(cancellationToken);

            // Get subject-wise marks
            var studentMarks = await _context.StudentMarks
                .Where(m => m.ExamId == reportCard.ExamId && m.Enrollment!.StudentId == reportCard.Enrollment!.StudentId)
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
            var schoolAddress = !string.IsNullOrEmpty(school?.Address) ? $"{school.Address}, {school.City}" : "123 Education Street, City Center";
            var schoolPhone = school?.PhoneNumber ?? "+91-1234-567-890";
            var schoolEmail = school?.EmailAddress ?? "info@school.edu";

            // Colors
            var primaryColor = "#312E81"; // Deep Indigo
            var secondaryColor = "#475569"; // Slate
            var accentColor = "#6366F1"; // Indigo Light
            var successColor = "#059669"; // Emerald
            var dangerColor = "#DC2626"; // Crimson
            var lightGray = "#F8FAFC";
            var borderColor = "#E2E8F0";
            var whiteColor = "#FFFFFF";
            var blackColor = "#000000";

            // Generate PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(whiteColor);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial").FontColor(secondaryColor));

                    // ==========================================
                    // HEADER
                    // ==========================================
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(headerCol =>
                            {
                                headerCol.Item().Text(schoolName.ToUpper()).FontSize(24).Bold().FontColor(primaryColor);
                                if (!string.IsNullOrEmpty(schoolAddress))
                                    headerCol.Item().PaddingTop(2).Text(schoolAddress).FontSize(9).FontColor(secondaryColor);
                                headerCol.Item().PaddingTop(4).Row(contactRow =>
                                {
                                    contactRow.AutoItem().Text($"📞 {schoolPhone}").FontSize(8);
                                    contactRow.AutoItem().PaddingHorizontal(8).Text("|").FontSize(8);
                                    contactRow.AutoItem().Text($"📧 {schoolEmail}").FontSize(8);
                                });
                            });

                            // Right-aligned Title
                            row.AutoItem().Background(primaryColor).PaddingHorizontal(15).PaddingVertical(10).AlignCenter().Column(titleCol =>
                            {
                                titleCol.Item().Text("ACADEMIC").FontSize(10).Bold().FontColor(whiteColor).AlignCenter();
                                titleCol.Item().Text("REPORT CARD").FontSize(16).Bold().FontColor(whiteColor).AlignCenter();
                                titleCol.Item().Text($"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Year + 1}").FontSize(9).FontColor(whiteColor).AlignCenter();
                            });
                        });
                        
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(borderColor);
                    });

                    // ==========================================
                    // CONTENT
                    // ==========================================
                    page.Content().Column(col =>
                    {
                        // 1. Student Information Grid
                        col.Item().PaddingVertical(10).Row(row =>
                        {
                            row.RelativeItem().Column(c1 =>
                            {
                                c1.Item().Row(r => { r.ConstantItem(80).Text("STUDENT NAME").Bold().FontSize(8); r.RelativeItem().Text($": {reportCard.Enrollment!.Student!.FirstName} {reportCard.Enrollment.Student.LastName}").Bold().FontColor(blackColor); });
                                c1.Item().PaddingTop(5).Row(r => { r.ConstantItem(80).Text("ROLL NUMBER").Bold().FontSize(8); r.RelativeItem().Text($": {(studentSection?.RollNumber ?? 0).ToString()}").FontColor(blackColor); });
                                c1.Item().PaddingTop(5).Row(r => { r.ConstantItem(80).Text("CLASS/SECTION").Bold().FontSize(8); r.RelativeItem().Text($": {studentSection?.Section?.SectionName ?? "N/A"}").FontColor(blackColor); });
                            });

                            row.RelativeItem().Column(c2 =>
                            {
                                c2.Item().Row(r => { r.ConstantItem(80).Text("EXAMINATION").Bold().FontSize(8); r.RelativeItem().Text($": {reportCard.Exam!.Name}").Bold().FontColor(primaryColor); });
                                c2.Item().PaddingTop(5).Row(r => { r.ConstantItem(80).Text("ISSUE DATE").Bold().FontSize(8); r.RelativeItem().Text($": {DateTime.UtcNow:dd MMM yyyy}").FontColor(blackColor); });
                                c2.Item().PaddingTop(5).Row(r => { r.ConstantItem(80).Text("STATUS").Bold().FontSize(8); r.RelativeItem().Text(reportCard.Pass ? ": QUALIFIED" : ": NOT QUALIFIED").Bold().FontColor(reportCard.Pass ? successColor : dangerColor); });
                            });
                        });

                        col.Item().PaddingTop(20).Text("PERFORMANCE OVERVIEW").FontSize(11).Bold().FontColor(primaryColor);
                        col.Item().PaddingTop(5).LineHorizontal(0.5f).LineColor(borderColor);

                        // 2. Summary Cards Container
                        col.Item().PaddingVertical(15).Row(row =>
                        {
                            void SummaryCard(string label, string value, string color, string bgColor)
                            {
                                row.RelativeItem().PaddingRight(10).Border(1).BorderColor(borderColor).Background(bgColor).Padding(10).Column(c =>
                                {
                                    c.Item().Text(label).FontSize(8).Bold().FontColor(secondaryColor).AlignCenter();
                                    c.Item().PaddingTop(4).Text(value).FontSize(14).Bold().FontColor(color).AlignCenter();
                                });
                            }

                            SummaryCard("TOTAL MARKS", $"{reportCard.TotalMarksObtained:F0}/{reportCard.TotalMarks:F0}", primaryColor, lightGray);
                            SummaryCard("PERCENTAGE", $"{reportCard.Percentage:F1}%", successColor, "#F0FDF4");
                            SummaryCard("GRADE", reportCard.OverallGrade, "#B45309", "#FFFBEB");
                            SummaryCard("CLASS RANK", $"#{reportCard.ClassPosition}", "#7C3AED", "#F5F3FF");
                        });

                        // 3. Subject Wise Details Table
                        col.Item().PaddingTop(15).Text("SUBJECT-WISE SCHOLASTIC PERFORMANCE").FontSize(11).Bold().FontColor(primaryColor);
                        
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30); // SR.
                                columns.RelativeColumn(3);  // SUBJECT
                                columns.RelativeColumn(1);  // MAX MARKS
                                columns.RelativeColumn(1);  // PASS MARKS
                                columns.RelativeColumn(1);  // OBTAINED
                                columns.RelativeColumn(1);  // GRADE
                                columns.RelativeColumn(1.2f); // STATUS
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                void HeaderCell(string text, bool right = false, bool center = false)
                                {
                                    var cell = header.Cell().Background(primaryColor).Padding(8);
                                    var txt = cell.Text(text).FontColor(whiteColor).Bold().FontSize(9);
                                    if (right) txt.AlignRight();
                                    if (center) txt.AlignCenter();
                                }

                                HeaderCell("#");
                                HeaderCell("SUBJECT NAME");
                                HeaderCell("MAX MARKS", center: true);
                                HeaderCell("PASS MARKS", center: true);
                                HeaderCell("OBTAINED", center: true);
                                HeaderCell("GRADE", center: true);
                                HeaderCell("RESULT", center: true);
                            });

                            // Table Rows
                            int index = 1;
                            foreach (var mark in subjectMarksList)
                            {
                                var percentage = mark.MarksObtained.HasValue && mark.ExamSubject!.MaxMarks > 0
                                    ? ((double)mark.MarksObtained.Value) / ((double)mark.ExamSubject.MaxMarks) * 100
                                    : 0;
                                var grade = DetermineGrade(percentage);
                                var isPassed = percentage >= 40;
                                var rowBg = index % 2 == 0 ? lightGray : whiteColor;

                                table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(borderColor).Padding(8).Text(index.ToString()).FontSize(9).AlignCenter();
                                table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(borderColor).Padding(8).Text(mark.ExamSubject?.Subject?.Name ?? "N/A").FontSize(9).Bold();
                                table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(borderColor).Padding(8).Text(mark.ExamSubject?.MaxMarks.ToString("F0") ?? "0").FontSize(9).AlignCenter();
                                table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(borderColor).Padding(8).Text(mark.ExamSubject?.PassMarks.ToString("F0") ?? "0").FontSize(9).AlignCenter();
                                table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(borderColor).Padding(8).Text((mark.MarksObtained ?? 0).ToString("F0")).FontSize(9).Bold().AlignCenter().FontColor(primaryColor);
                                table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(borderColor).Padding(8).Text(grade).FontSize(9).Bold().AlignCenter().FontColor(percentage < 50 ? dangerColor : successColor);
                                table.Cell().Background(rowBg).BorderBottom(0.5f).BorderColor(borderColor).Padding(8).Text(isPassed ? "PASS" : "FAIL").FontSize(8).Bold().AlignCenter().FontColor(isPassed ? successColor : dangerColor);

                                index++;
                            }
                        });

                        // 4. Grading Legend & Remarks
                        col.Item().PaddingTop(25).Row(row =>
                        {
                            // Legend
                            row.RelativeItem(1.5f).Column(legendCol =>
                            {
                                legendCol.Item().Text("GRADING SCALE").FontSize(8).Bold();
                                legendCol.Item().PaddingTop(4).Table(t =>
                                {
                                    t.ColumnsDefinition(c => { c.ConstantColumn(40); c.RelativeColumn(); });
                                    void LRow(string g, string d) { t.Cell().Border(0.5f).BorderColor(borderColor).Padding(2).AlignCenter().Text(g).FontSize(7).Bold(); t.Cell().Border(0.5f).BorderColor(borderColor).Padding(2).PaddingLeft(5).Text(d).FontSize(7); }
                                    LRow("A", "EXCELLENT (90-100%)");
                                    LRow("B", "VERY GOOD (80-89%)");
                                    LRow("C", "GOOD (70-79%)");
                                    LRow("D", "SATISFACTORY (60-69%)");
                                    LRow("F", "FAIL (<60%)");
                                });
                            });

                            row.ConstantItem(30);

                            // Signatures
                            row.RelativeItem(2).Column(sigCol =>
                            {
                                sigCol.Item().PaddingTop(20).Row(sigRow =>
                                {
                                    sigRow.RelativeItem().Column(s1 =>
                                    {
                                        s1.Item().PaddingTop(30).LineHorizontal(0.5f).LineColor(secondaryColor);
                                        s1.Item().PaddingTop(4).Text("CLASS TEACHER").FontSize(8).Bold().AlignCenter();
                                    });
                                    sigRow.ConstantItem(40);
                                    sigRow.RelativeItem().Column(s2 =>
                                    {
                                        s2.Item().PaddingTop(30).LineHorizontal(0.5f).LineColor(secondaryColor);
                                        s2.Item().PaddingTop(4).Text("PRINCIPAL SIGNATURE").FontSize(8).Bold().AlignCenter();
                                    });
                                });
                                
                                sigCol.Item().PaddingTop(15).Text("Disclaimer: This is a computer-generated report and does not require a physical seal to be valid for internal school purposes.").FontSize(7).Italic().FontColor("#94A3B8");
                            });
                        });
                    });

                    // ==========================================
                    // FOOTER
                    // ==========================================
                    page.Footer().Column(footerCol =>
                    {
                        footerCol.Item().LineHorizontal(0.5f).LineColor(borderColor);
                        footerCol.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"System Generated on: {DateTime.UtcNow:dd/MM/yyyy HH:mm}").FontSize(7).FontColor("#94A3B8");
                            row.RelativeItem().AlignRight().Text(x => { x.Span("Page "); x.CurrentPageNumber(); x.Span(" of "); x.TotalPages(); });
                        });
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

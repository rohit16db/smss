using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.Commands;
using SMS.Application.Features.Exams.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Exams.Handlers;

public class MarksCommandHandlers
{
    public class SaveStudentMarksCommandHandler : IRequestHandler<SaveStudentMarksCommand, SaveMarksResponseDto>
    {
        private readonly IApplicationDbContext _context;
        public SaveStudentMarksCommandHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<SaveMarksResponseDto> Handle(SaveStudentMarksCommand request, CancellationToken cancellationToken)
        {
            // Validate exam exists
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            if (exam == null)
                return new SaveMarksResponseDto { Success = false, Message = "Exam not found" };

            // Validate section exists
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == request.SectionId && s.ClassId == request.ClassId, cancellationToken);
            if (section == null)
                return new SaveMarksResponseDto { Success = false, Message = "Section not found" };

            // Save marks for all students
            var (savedCount, markedStudents, unmarkedStudents) = await SaveStudentMarksAsync(request, cancellationToken);

            return new SaveMarksResponseDto
            {
                Success = true,
                Message = $"Marks saved successfully. Saved: {savedCount}",
                MarksCount = savedCount,
                ValidationResults = new()
                {
                    StudentCount = request.MarksData.Count,
                    MarkedCount = markedStudents,
                    UnmarkedCount = unmarkedStudents
                }
            };
        }

        private async Task<(int savedCount, int markedStudents, int unmarkedStudents)> SaveStudentMarksAsync(
            SaveStudentMarksCommand request, CancellationToken cancellationToken)
        {
            var savedCount = 0;
            var markedStudents = 0;
            var unmarkedStudents = 0;

            foreach (var marksEntry in request.MarksData)
            {
                var studentMarksCount = await SaveSubjectMarksForStudentAsync(_context, request, marksEntry, cancellationToken);
                savedCount += studentMarksCount;

                if (studentMarksCount > 0)
                    markedStudents++;
                else
                    unmarkedStudents++;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return (savedCount, markedStudents, unmarkedStudents);
        }

        private static async Task<int> SaveSubjectMarksForStudentAsync(
            IApplicationDbContext context,
            SaveStudentMarksCommand request, 
            StudentMarksEntryDto marksEntry, 
            CancellationToken cancellationToken)
        {
            var savedCount = 0;

            foreach (var subjectMarkEntry in marksEntry.SubjectMarks)
            {
                try
                {
                    await SaveSubjectMarkAsync(context, request, marksEntry.StudentId, subjectMarkEntry, cancellationToken);
                    savedCount++;
                }
                catch
                {
                    // Log error but continue processing
                }
            }

            return savedCount;
        }

        private static async Task SaveSubjectMarkAsync(
            IApplicationDbContext context,
            SaveStudentMarksCommand request,
            Guid studentId,
            KeyValuePair<Guid, SubjectMarkEntryDto> subjectMarkEntry,
            CancellationToken cancellationToken)
        {
            var existingMarks = await context.StudentMarks
                .FirstOrDefaultAsync(m => m.ExamId == request.ExamId && 
                                          m.StudentId == studentId &&
                                          m.SubjectId == subjectMarkEntry.Key, cancellationToken);

            if (existingMarks != null)
            {
                existingMarks.MarksObtained = subjectMarkEntry.Value.Obtained;
                existingMarks.IsAbsent = subjectMarkEntry.Value.IsAbsent;
                existingMarks.UpdatedAt = DateTime.UtcNow;
                context.StudentMarks.Update(existingMarks);
            }
            else
            {
                var marks = new StudentMarks
                {
                    Id = Guid.NewGuid(),
                    ExamId = request.ExamId,
                    StudentId = studentId,
                    SubjectId = subjectMarkEntry.Key,
                    MarksObtained = subjectMarkEntry.Value.Obtained,
                    IsAbsent = subjectMarkEntry.Value.IsAbsent,
                    CreatedAt = DateTime.UtcNow
                };
                context.StudentMarks.Add(marks);
            }
        }
    }

    public class SubmitMarksCommandHandler : IRequestHandler<SubmitMarksCommand, SaveMarksResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMediator _mediator;
        
        public SubmitMarksCommandHandler(IApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<SaveMarksResponseDto> Handle(SubmitMarksCommand request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            if (exam == null)
                return new SaveMarksResponseDto { Success = false, Message = "Exam not found" };

            // Validate section exists
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == request.SectionId && s.ClassId == request.ClassId, cancellationToken);
            if (section == null)
                return new SaveMarksResponseDto { Success = false, Message = "Section not found" };

            // Get all students for this specific section
            var sectionStudents = await _context.StudentSections
                .Where(ss => ss.SectionId == request.SectionId && ss.IsCurrent)
                .Select(ss => ss.StudentId)
                .ToListAsync(cancellationToken);

            if (!sectionStudents.Any())
                return new SaveMarksResponseDto { Success = false, Message = "No students found in this section" };

            // Trigger report card generation for each student
            var successCount = 0;
            foreach (var studentId in sectionStudents)
            {
                try
                {
                    await _mediator.Send(
                        new GenerateReportCardCommand
                        {
                            ExamId = request.ExamId,
                            StudentId = studentId,
                            GeneratedBy = request.ConfirmedById
                        },
                        cancellationToken);
                    successCount++;
                }
                catch
                {
                    // Log but continue
                }
            }

            return new SaveMarksResponseDto
            {
                Success = successCount == sectionStudents.Count,
                Message = $"Marks submitted successfully. Report cards generated for {successCount}/{sectionStudents.Count} students",
                MarksCount = successCount
            };
        }
    }

    public class GenerateReportCardCommandHandler : IRequestHandler<GenerateReportCardCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        public GenerateReportCardCommandHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<Unit> Handle(GenerateReportCardCommand request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            if (exam == null)
                return Unit.Value;

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);
            if (student == null)
                return Unit.Value;

            // Get all subject marks for this student in this exam
            var studentMarks = await _context.StudentMarks
                .Where(m => m.ExamId == request.ExamId && m.StudentId == request.StudentId)
                .ToListAsync(cancellationToken);

            if (!studentMarks.Any())
                return Unit.Value;

            // Calculate total marks
            var totalMarksObtained = studentMarks
                .Where(m => !m.IsAbsent && m.MarksObtained.HasValue)
                .Sum(m => m.MarksObtained ?? 0);

            var totalMarks = exam.TotalMarks;
            var percentage = totalMarks > 0 ? (totalMarksObtained / totalMarks) * 100 : 0;

            // Determine grade based on percentage
            var grade = DetermineGrade(percentage);

            // Check if student passed - consistent with grade system (D grade or above = pass, F = fail)
            // D grade is 40%, so student passes if overall percentage >= 40%
            var passed = percentage >= 40;

            // Check if report card already exists
            var existingReportCard = await _context.StudentReportCards
                .FirstOrDefaultAsync(rc => rc.ExamId == request.ExamId && rc.StudentId == request.StudentId, cancellationToken);

            if (existingReportCard != null)
            {
                existingReportCard.TotalMarksObtained = totalMarksObtained;
                existingReportCard.TotalMarks = totalMarks;
                existingReportCard.Percentage = percentage;
                existingReportCard.OverallGrade = grade;
                existingReportCard.Pass = passed;
                existingReportCard.GeneratedAt = DateTime.UtcNow;
                existingReportCard.UpdatedAt = DateTime.UtcNow;
                _context.StudentReportCards.Update(existingReportCard);
            }
            else
            {
                var reportCard = new StudentReportCard
                {
                    Id = Guid.NewGuid(),
                    ExamId = request.ExamId,
                    StudentId = request.StudentId,
                    TotalMarksObtained = totalMarksObtained,
                    TotalMarks = totalMarks,
                    Percentage = percentage,
                    OverallGrade = grade,
                    Pass = passed,
                    GeneratedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _context.StudentReportCards.Add(reportCard);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        private static string DetermineGrade(decimal percentage)
        {
            return percentage switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B+",
                >= 60 => "B",
                >= 50 => "C",
                >= 40 => "D",
                _ => "F"
            };
        }
    }
}

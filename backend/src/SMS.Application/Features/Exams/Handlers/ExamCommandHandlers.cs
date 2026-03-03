using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.Commands;
using SMS.Application.Features.Exams.DTOs;
using SMS.Domain.Entities;
using SMS.Domain.Enums;

namespace SMS.Application.Features.Exams.Handlers;

public class ExamCommandHandlers
{
    public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, ExamDto>
    {
        private readonly IApplicationDbContext _context;
        public CreateExamCommandHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<ExamDto> Handle(CreateExamCommand request, CancellationToken cancellationToken)
        {
            // Convert StartDate to UTC if it's Unspecified
            var startDate = request.StartDate;
            if (startDate.Kind == DateTimeKind.Unspecified)
            {
                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            }
            else if (startDate.Kind == DateTimeKind.Local)
            {
                startDate = startDate.ToUniversalTime();
            }

            // Convert EndDate to UTC if it's Unspecified
            var endDate = request.EndDate;
            if (endDate.Kind == DateTimeKind.Unspecified)
            {
                endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            }
            else if (endDate.Kind == DateTimeKind.Local)
            {
                endDate = endDate.ToUniversalTime();
            }

            var exam = new Exam
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                StartDate = startDate,
                EndDate = endDate,
                TotalMarks = request.TotalMarks,
                PassMarks = request.PassMarks,
                Status = ExamStatus.Draft,
                CreatedById = request.CreatedById,
                CreatedAt = DateTime.UtcNow
            };

            // Add exam-subject associations with max marks
            foreach (var subjectInput in request.Subjects)
            {
                exam.ExamSubjects.Add(new ExamSubject
                {
                    Id = Guid.NewGuid(),
                    ExamId = exam.Id,
                    SubjectId = subjectInput.SubjectId,
                    MaxMarks = subjectInput.MaxMarks,
                    PassMarks = subjectInput.PassMarks
                });
            }

            // Add exam-class associations
            foreach (var classId in request.ClassIds)
            {
                exam.ExamClasses.Add(new ExamClass
                {
                    Id = Guid.NewGuid(),
                    ExamId = exam.Id,
                    ClassId = classId
                });
            }

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync(cancellationToken);

            return new ExamDto
            {
                Id = exam.Id,
                Name = exam.Name,
                Description = exam.Description,
                StartDate = exam.StartDate,
                EndDate = exam.EndDate,
                TotalMarks = exam.TotalMarks,
                PassMarks = exam.PassMarks,
                Status = exam.Status.ToString()
            };
        }
    }

    public class UpdateExamCommandHandler : IRequestHandler<UpdateExamCommand, ExamDto>
    {
        private readonly IApplicationDbContext _context;
        public UpdateExamCommandHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<ExamDto> Handle(UpdateExamCommand request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamSubjects)
                .Include(e => e.ExamClasses)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            // Only allow updates if exam is in Draft status
            if (exam.Status != ExamStatus.Draft)
                throw new InvalidOperationException("Only draft exams can be updated");

            // Convert StartDate to UTC if it's Unspecified
            var startDate = request.StartDate;
            if (startDate.Kind == DateTimeKind.Unspecified)
            {
                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            }
            else if (startDate.Kind == DateTimeKind.Local)
            {
                startDate = startDate.ToUniversalTime();
            }

            // Convert EndDate to UTC if it's Unspecified
            var endDate = request.EndDate;
            if (endDate.Kind == DateTimeKind.Unspecified)
            {
                endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            }
            else if (endDate.Kind == DateTimeKind.Local)
            {
                endDate = endDate.ToUniversalTime();
            }

            exam.Name = request.Name;
            exam.Description = request.Description;
            exam.StartDate = startDate;
            exam.EndDate = endDate;
            exam.TotalMarks = request.TotalMarks;
            exam.PassMarks = request.PassMarks;
            exam.UpdatedAt = DateTime.UtcNow;

            // Update exam-subject associations
            // Remove old subjects
            _context.ExamSubjects.RemoveRange(exam.ExamSubjects);

            // Add new subjects
            foreach (var subjectInput in request.Subjects)
            {
                exam.ExamSubjects.Add(new ExamSubject
                {
                    Id = Guid.NewGuid(),
                    ExamId = exam.Id,
                    SubjectId = subjectInput.SubjectId,
                    MaxMarks = subjectInput.MaxMarks,
                    PassMarks = subjectInput.PassMarks
                });
            }

            // Update exam-class associations
            // Remove old classes
            _context.ExamClasses.RemoveRange(exam.ExamClasses);

            // Add new classes
            foreach (var classId in request.ClassIds)
            {
                exam.ExamClasses.Add(new ExamClass
                {
                    Id = Guid.NewGuid(),
                    ExamId = exam.Id,
                    ClassId = classId
                });
            }

            _context.Exams.Update(exam);
            await _context.SaveChangesAsync(cancellationToken);

            return new ExamDto
            {
                Id = exam.Id,
                Name = exam.Name,
                Description = exam.Description,
                StartDate = exam.StartDate,
                EndDate = exam.EndDate,
                TotalMarks = exam.TotalMarks,
                PassMarks = exam.PassMarks,
                Status = exam.Status.ToString()
            };
        }
    }

    public class PublishExamCommandHandler : IRequestHandler<PublishExamCommand, ExamDto>
    {
        private readonly IApplicationDbContext _context;
        public PublishExamCommandHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<ExamDto> Handle(PublishExamCommand request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            if (exam.Status != ExamStatus.Draft)
                throw new InvalidOperationException("Only draft exams can be published");

            exam.Status = ExamStatus.Published;
            exam.UpdatedAt = DateTime.UtcNow;

            _context.Exams.Update(exam);
            await _context.SaveChangesAsync(cancellationToken);

            return new ExamDto
            {
                Id = exam.Id,
                Name = exam.Name,
                Description = exam.Description,
                StartDate = exam.StartDate,
                EndDate = exam.EndDate,
                TotalMarks = exam.TotalMarks,
                PassMarks = exam.PassMarks,
                Status = exam.Status.ToString()
            };
        }
    }

    public class DeleteExamCommandHandler : IRequestHandler<DeleteExamCommand, bool>
    {
        private readonly IApplicationDbContext _context;
        public DeleteExamCommandHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<bool> Handle(DeleteExamCommand request, CancellationToken cancellationToken)
        {
            var exam = await _context.Exams
                .Include(e => e.StudentMarks)
                .Include(e => e.ExamSubjects)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            
            if (exam == null)
                throw new InvalidOperationException($"Exam not found");

            if (exam.Status != ExamStatus.Draft)
                throw new InvalidOperationException("Only draft exams can be deleted");

            // Delete StudentReportCards for this exam
            var reportCards = await _context.StudentReportCards
                .Where(rc => rc.ExamId == request.ExamId)
                .ToListAsync(cancellationToken);
            if (reportCards.Any())
                _context.StudentReportCards.RemoveRange(reportCards);

            // Delete StudentMarks for this exam
            if (exam.StudentMarks.Any())
                _context.StudentMarks.RemoveRange(exam.StudentMarks);

            // Delete ExamSubjects for this exam
            if (exam.ExamSubjects.Any())
                _context.ExamSubjects.RemoveRange(exam.ExamSubjects);

            // Delete the exam itself
            _context.Exams.Remove(exam);
            
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

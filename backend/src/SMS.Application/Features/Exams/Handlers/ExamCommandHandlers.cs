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
            // Convert ExamDate to UTC if it's Unspecified
            var examDate = request.ExamDate;
            if (examDate.Kind == DateTimeKind.Unspecified)
            {
                examDate = DateTime.SpecifyKind(examDate, DateTimeKind.Utc);
            }
            else if (examDate.Kind == DateTimeKind.Local)
            {
                examDate = examDate.ToUniversalTime();
            }

            var exam = new Exam
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                ExamDate = examDate,
                TotalMarks = request.TotalMarks,
                PassMarks = request.PassMarks,
                Status = ExamStatus.Draft,
                CreatedById = request.CreatedById,
                CreatedAt = DateTime.UtcNow
            };

            // Add exam-subject associations
            foreach (var subjectId in request.SubjectIds)
            {
                exam.ExamSubjects.Add(new ExamSubject
                {
                    Id = Guid.NewGuid(),
                    ExamId = exam.Id,
                    SubjectId = subjectId
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
                ExamDate = exam.ExamDate,
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
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            // Only allow updates if exam is in Draft status
            if (exam.Status != ExamStatus.Draft)
                throw new InvalidOperationException("Only draft exams can be updated");

            // Convert ExamDate to UTC if it's Unspecified
            var examDate = request.ExamDate;
            if (examDate.Kind == DateTimeKind.Unspecified)
            {
                examDate = DateTime.SpecifyKind(examDate, DateTimeKind.Utc);
            }
            else if (examDate.Kind == DateTimeKind.Local)
            {
                examDate = examDate.ToUniversalTime();
            }

            exam.Name = request.Name;
            exam.Description = request.Description;
            exam.ExamDate = examDate;
            exam.TotalMarks = request.TotalMarks;
            exam.PassMarks = request.PassMarks;
            exam.UpdatedAt = DateTime.UtcNow;

            _context.Exams.Update(exam);
            await _context.SaveChangesAsync(cancellationToken);

            return new ExamDto
            {
                Id = exam.Id,
                Name = exam.Name,
                Description = exam.Description,
                ExamDate = exam.ExamDate,
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
                ExamDate = exam.ExamDate,
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
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);
            
            if (exam == null)
                throw new InvalidOperationException($"Exam with ID {request.ExamId} not found");

            if (exam.Status != ExamStatus.Draft)
                throw new InvalidOperationException("Only draft exams can be deleted");

            // Check if there are any marks for this exam
            if (exam.StudentMarks.Any())
                throw new InvalidOperationException("Cannot delete exam that has marks recorded");

            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

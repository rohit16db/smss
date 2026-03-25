using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Repositories;

/// <summary>
/// Repository for StudentMarks entity operations
/// Single Responsibility: Handle all student marks entry, retrieval, and updates
/// </summary>
public interface IStudentMarksRepository
{
    Task<StudentMarks?> GetSingleAsync(Guid examId, Guid enrollmentId, Guid subjectId, CancellationToken cancellationToken = default);
    Task<List<StudentMarks>> GetByExamAndClassAsync(Guid examId, Guid classId, CancellationToken cancellationToken = default);
    Task<List<StudentMarks>> GetByStudentAndExamAsync(Guid enrollmentId, Guid examId, CancellationToken cancellationToken = default);
    Task SaveAsync(List<StudentMarks> marks, CancellationToken cancellationToken = default);
    Task<StudentMarks> UpdateAsync(StudentMarks marks, CancellationToken cancellationToken = default);
}

public class StudentMarksRepository : IStudentMarksRepository
{
    private readonly ApplicationDbContext _context;

    public StudentMarksRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentMarks?> GetSingleAsync(Guid examId, Guid enrollmentId, Guid subjectId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentMarks
            .FirstOrDefaultAsync(sm => sm.ExamId == examId && sm.EnrollmentId == enrollmentId && sm.SubjectId == subjectId, cancellationToken);
    }

    public async Task<List<StudentMarks>> GetByExamAndClassAsync(Guid examId, Guid classId, CancellationToken cancellationToken = default)
    {
        var studentIds = await _context.Enrollments
            .Where(ss => ss.Section != null && ss.Section.ClassId == classId && ss.Status == "Enrolled")
            .Select(ss => ss.StudentId)
            .ToListAsync(cancellationToken);

        return await _context.StudentMarks
            .Where(sm => sm.ExamId == examId && studentIds.Contains(sm.Enrollment!.StudentId))
            .Include(sm => sm.Enrollment)
            .Include(sm => sm.ExamSubject)
            .ThenInclude(es => es!.Subject)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StudentMarks>> GetByStudentAndExamAsync(Guid enrollmentId, Guid examId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentMarks
            .Where(sm => sm.EnrollmentId == enrollmentId && sm.ExamId == examId)
            .Include(sm => sm.ExamSubject)
            .ThenInclude(es => es!.Subject)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(List<StudentMarks> marks, CancellationToken cancellationToken = default)
    {
        foreach (var mark in marks)
        {
            var existing = await _context.StudentMarks
                .FirstOrDefaultAsync(sm => sm.ExamId == mark.ExamId && sm.EnrollmentId == mark.EnrollmentId && sm.SubjectId == mark.SubjectId, cancellationToken);

            if (existing != null)
            {
                existing.MarksObtained = mark.MarksObtained;
                existing.IsAbsent = mark.IsAbsent;
                existing.Remarks = mark.Remarks;
                _context.StudentMarks.Update(existing);
            }
            else
            {
                _context.StudentMarks.Add(mark);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<StudentMarks> UpdateAsync(StudentMarks marks, CancellationToken cancellationToken = default)
    {
        _context.StudentMarks.Update(marks);
        await _context.SaveChangesAsync(cancellationToken);
        return marks;
    }
}

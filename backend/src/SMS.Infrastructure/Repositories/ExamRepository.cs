using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Repositories;

/// <summary>
/// Repository for Exam entity operations
/// Single Responsibility: Handle all Exam CRUD operations and queries
/// </summary>
public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(Guid examId, CancellationToken cancellationToken = default);
    Task<List<Exam>> GetAllAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<List<Exam>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<Exam> CreateAsync(Exam exam, CancellationToken cancellationToken = default);
    Task<Exam> UpdateAsync(Exam exam, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid examId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

public class ExamRepository : IExamRepository
{
    private readonly ApplicationDbContext _context;

    public ExamRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Exam?> GetByIdAsync(Guid examId, CancellationToken cancellationToken = default)
    {
        return await _context.Exams
            .Include(e => e.ExamSubjects)
            .ThenInclude(es => es.Subject)
            .Include(e => e.ExamClasses)
            .ThenInclude(ec => ec.Class)
            .FirstOrDefaultAsync(e => e.Id == examId, cancellationToken);
    }

    public async Task<List<Exam>> GetAllAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Exams
            .OrderByDescending(e => e.StartDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Exam>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Exams
            .Where(e => e.Status.ToString() == status)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Exam> CreateAsync(Exam exam, CancellationToken cancellationToken = default)
    {
        _context.Exams.Add(exam);
        await _context.SaveChangesAsync(cancellationToken);
        return exam;
    }

    public async Task<Exam> UpdateAsync(Exam exam, CancellationToken cancellationToken = default)
    {
        _context.Exams.Update(exam);
        await _context.SaveChangesAsync(cancellationToken);
        return exam;
    }

    public async Task<bool> DeleteAsync(Guid examId, CancellationToken cancellationToken = default)
    {
        var exam = await _context.Exams.FindAsync(new object[] { examId }, cancellationToken: cancellationToken);
        if (exam == null)
            return false;

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Exams.CountAsync(cancellationToken);
    }
}

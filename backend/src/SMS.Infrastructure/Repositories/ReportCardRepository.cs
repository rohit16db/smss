using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Repositories;

/// <summary>
/// Repository for StudentReportCard entity operations
/// Single Responsibility: Handle report card generation, retrieval, and queries
/// </summary>
public interface IReportCardRepository
{
    Task<StudentReportCard?> GetByIdAsync(Guid examId, Guid studentId, CancellationToken cancellationToken = default);
    Task<List<StudentReportCard>> GetByExamAsync(Guid examId, CancellationToken cancellationToken = default);
    Task<List<StudentReportCard>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<StudentReportCard> CreateAsync(StudentReportCard reportCard, CancellationToken cancellationToken = default);
    Task<StudentReportCard> UpdateAsync(StudentReportCard reportCard, CancellationToken cancellationToken = default);
}

public class ReportCardRepository : IReportCardRepository
{
    private readonly ApplicationDbContext _context;

    public ReportCardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentReportCard?> GetByIdAsync(Guid examId, Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentReportCards
            .Include(src => src.Exam)
            .Include(src => src.Student)
            .FirstOrDefaultAsync(src => src.ExamId == examId && src.StudentId == studentId, cancellationToken);
    }

    public async Task<List<StudentReportCard>> GetByExamAsync(Guid examId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentReportCards
            .Where(src => src.ExamId == examId)
            .Include(src => src.Student)
            .OrderBy(src => src.ClassPosition)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StudentReportCard>> GetByStudentAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentReportCards
            .Where(src => src.StudentId == studentId)
            .Include(src => src.Exam)
            .OrderByDescending(src => src.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentReportCard> CreateAsync(StudentReportCard reportCard, CancellationToken cancellationToken = default)
    {
        _context.StudentReportCards.Add(reportCard);
        await _context.SaveChangesAsync(cancellationToken);
        return reportCard;
    }

    public async Task<StudentReportCard> UpdateAsync(StudentReportCard reportCard, CancellationToken cancellationToken = default)
    {
        _context.StudentReportCards.Update(reportCard);
        await _context.SaveChangesAsync(cancellationToken);
        return reportCard;
    }
}

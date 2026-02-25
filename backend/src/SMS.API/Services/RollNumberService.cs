using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;

namespace SMS.API.Services;

/// <summary>
/// Implementation of roll number service for managing student roll numbers in sections
/// </summary>
public class RollNumberService : IRollNumberService
{
    private readonly IApplicationDbContext _context;

    public RollNumberService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AssignSequentialRollNumbersAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        // Get all current student sections for the section, ordered by joined date
        var studentSections = await _context.StudentSections
            .Where(ss => ss.SectionId == sectionId && ss.IsCurrent)
            .OrderBy(ss => ss.JoinedDate)
            .ToListAsync(cancellationToken);

        // Assign sequential roll numbers starting from 1
        int rollNumber = 1;
        foreach (var studentSection in studentSections)
        {
            studentSection.RollNumber = rollNumber;
            rollNumber++;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRollNumberAsync(Guid studentSectionId, int rollNumber, CancellationToken cancellationToken = default)
    {
        var studentSection = await _context.StudentSections
            .FirstOrDefaultAsync(ss => ss.Id == studentSectionId, cancellationToken)
            ?? throw new InvalidOperationException($"StudentSection with Id {studentSectionId} not found");

        // Check if roll number already exists in the same section
        var existingWithRollNumber = await _context.StudentSections
            .FirstOrDefaultAsync(
                ss => ss.SectionId == studentSection.SectionId
                    && ss.RollNumber == rollNumber
                    && ss.Id != studentSectionId
                    && ss.IsCurrent,
                cancellationToken);

        if (existingWithRollNumber != null)
        {
            throw new InvalidOperationException(
                $"Roll number {rollNumber} is already assigned to another student in this section");
        }

        studentSection.RollNumber = rollNumber;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetNextAvailableRollNumberAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var maxRollNumber = await _context.StudentSections
            .Where(ss => ss.SectionId == sectionId && ss.IsCurrent)
            .MaxAsync(ss => (int?)ss.RollNumber, cancellationToken) ?? 0;

        return maxRollNumber + 1;
    }

    public async Task<List<StudentSectionForRollManagement>> GetStudentsWithRollNumbersAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var studentSections = await _context.StudentSections
            .Where(ss => ss.SectionId == sectionId && ss.IsCurrent)
            .Include(ss => ss.Student)
            .OrderBy(ss => ss.RollNumber ?? int.MaxValue)
            .ThenBy(ss => ss.JoinedDate)
            .Select(ss => new StudentSectionForRollManagement
            {
                StudentSectionId = ss.Id.ToString(),
                StudentId = ss.Student!.Id.ToString(),
                StudentName = $"{ss.Student.FirstName} {ss.Student.LastName}",
                CurrentRollNumber = ss.RollNumber,
                JoinedDate = ss.JoinedDate
            })
            .ToListAsync(cancellationToken);

        return studentSections;
    }

    public async Task BulkUpdateRollNumbersAsync(Guid sectionId, Dictionary<Guid, int> rollNumberUpdates, CancellationToken cancellationToken = default)
    {
        // Validate all roll numbers are unique within the updates
        if (rollNumberUpdates.Values.Distinct().Count() != rollNumberUpdates.Count)
        {
            throw new InvalidOperationException("Duplicate roll numbers found in the update");
        }

        // Get all student sections to update
        var studentSections = await _context.StudentSections
            .Where(ss => sectionId == ss.SectionId && ss.IsCurrent && rollNumberUpdates.Keys.Contains(ss.Id))
            .ToListAsync(cancellationToken);

        // Update roll numbers
        foreach (var studentSection in studentSections)
        {
            if (rollNumberUpdates.TryGetValue(studentSection.Id, out var newRollNumber))
            {
                studentSection.RollNumber = newRollNumber;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

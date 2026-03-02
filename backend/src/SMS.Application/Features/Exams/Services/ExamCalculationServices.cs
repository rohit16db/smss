using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Exams.Services;

public interface IGradeCalculationService
{
    Task<string> GetGradeAsync(decimal percentage, CancellationToken cancellationToken = default);
    Task<bool> IsPassedAsync(decimal obtainedMarks, decimal passMarks, CancellationToken cancellationToken = default);
    Task<decimal> GetGradePointAsync(string grade, CancellationToken cancellationToken = default);
}

public class GradeCalculationService : IGradeCalculationService
{
    private readonly IApplicationDbContext _context;

    public GradeCalculationService(IApplicationDbContext context) => _context = context;

    public Task<string> GetGradeAsync(decimal percentage, CancellationToken cancellationToken = default)
    {
        var grades = _context.GradeConfigurations.ToList();
        var matching = grades.FirstOrDefault(g => percentage >= g.MinPercentage && percentage <= g.MaxPercentage);
        return Task.FromResult(matching?.GradeName ?? "F");
    }

    public Task<bool> IsPassedAsync(decimal obtainedMarks, decimal passMarks, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(obtainedMarks >= passMarks);
    }

    public Task<decimal> GetGradePointAsync(string grade, CancellationToken cancellationToken = default)
    {
        var points = grade switch
        {
            "A" => 4.0m,
            "B" => 3.0m,
            "C" => 2.0m,
            "D" => 1.0m,
            _ => 0.0m
        };

        return Task.FromResult(points);
    }
}

public interface IMarksCalculationService
{
    decimal CalculateTotal(Dictionary<Guid, decimal> marks);
    decimal CalculatePercentage(decimal obtained, decimal total);
    void ValidateMarks(decimal obtained, decimal maxMarks);
}

public class MarksCalculationService : IMarksCalculationService
{
    public decimal CalculateTotal(Dictionary<Guid, decimal> marks)
        => marks.Values.Sum();

    public decimal CalculatePercentage(decimal obtained, decimal total)
        => total > 0 ? Math.Round((obtained / total) * 100, 2) : 0;

    public void ValidateMarks(decimal obtained, decimal maxMarks)
    {
        if (obtained < 0 || obtained > maxMarks)
            throw new ArgumentException($"Marks must be between 0 and {maxMarks}");
    }
}

public interface IClassPositionService
{
    List<(Guid StudentId, int Position)> CalculatePositions(List<(Guid StudentId, decimal Percentage)> results);
}

public class ClassPositionService : IClassPositionService
{
    public List<(Guid StudentId, int Position)> CalculatePositions(List<(Guid StudentId, decimal Percentage)> results)
    {
        var ordered = results.OrderByDescending(r => r.Percentage).ToList();
        var positions = new List<(Guid, int)>();
        int currentPosition = 1;
        decimal lastPercentage = -1;

        foreach (var (studentId, percentage) in ordered)
        {
            if (percentage != lastPercentage)
                currentPosition = positions.Count + 1;

            positions.Add((studentId, currentPosition));
            lastPercentage = percentage;
        }

        return positions;
    }
}

using SMS.Domain.Entities;

namespace SMS.Domain.Services;

/// <summary>
/// Service for marks validation logic
/// Single Responsibility: Validate marks against rules and constraints
/// </summary>
public interface IMarksValidationService
{
    ValidationResult ValidateMarks(decimal marks, decimal maxMarks, bool isAbsent);
    ValidationResult ValidateAllStudentMarks(List<StudentMarks> marks, Dictionary<Guid, ExamSubject> examSubjectMap);
}

public class MarksValidationService : IMarksValidationService
{
    public ValidationResult ValidateMarks(decimal marks, decimal maxMarks, bool isAbsent)
    {
        if (isAbsent)
            return ValidationResult.Success();

        if (marks < 0)
            return ValidationResult.Failure("Marks cannot be negative");

        if (marks > maxMarks)
            return ValidationResult.Failure($"Marks cannot exceed {maxMarks}");

        return ValidationResult.Success();
    }

    public ValidationResult ValidateAllStudentMarks(List<StudentMarks> marks, Dictionary<Guid, ExamSubject> examSubjectMap)
    {
        var errors = new List<string>();

        foreach (var mark in marks)
        {
            if (!examSubjectMap.TryGetValue(mark.SubjectId, out var examSubject))
            {
                errors.Add($"Subject {mark.SubjectId} not found in exam");
                continue;
            }

            var result = ValidateMarks(mark.MarksObtained ?? 0, examSubject.MaxMarks, mark.IsAbsent);
            if (!result.IsValid)
                errors.Add(result.Message);
        }

        return errors.Any()
            ? ValidationResult.Failure(string.Join("; ", errors))
            : ValidationResult.Success();
    }
}

public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string Message { get; private set; } = string.Empty;

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(string message) => new() { IsValid = false, Message = message };
}

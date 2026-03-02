namespace SMS.Domain.Entities;

/// <summary>
/// Represents a student's report card for an exam (denormalized for performance)
/// Single Responsibility: Stores pre-calculated report card data for quick retrieval
/// </summary>
public class StudentReportCard : BaseEntity
{
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public decimal TotalMarksObtained { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal Percentage { get; set; }
    public string OverallGrade { get; set; } = string.Empty;
    public int ClassPosition { get; set; }
    public bool Pass { get; set; }
    public string? Remarks { get; set; }
    public DateTime? GeneratedAt { get; set; }

    // Navigation properties
    public virtual Exam? Exam { get; set; }
    public virtual Student? Student { get; set; }
}

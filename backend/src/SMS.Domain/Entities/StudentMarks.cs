namespace SMS.Domain.Entities;

/// <summary>
/// Represents marks obtained by a student in a specific subject for an exam
/// Single Responsibility: Stores individual student marks for a subject in an exam
/// </summary>
public class StudentMarks : BaseEntity
{
    public Guid ExamId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid SubjectId { get; set; }
    public decimal? MarksObtained { get; set; }
    public bool IsAbsent { get; set; } = false;
    public string? Remarks { get; set; }

    // Navigation properties
    public virtual Exam? Exam { get; set; }
    public virtual Enrollment? Enrollment { get; set; }
    public virtual ExamSubject? ExamSubject { get; set; }
}

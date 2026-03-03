namespace SMS.Domain.Entities;

/// <summary>
/// Junction table: Associates subjects with an exam and defines max marks per subject
/// Single Responsibility: Represents exam-subject relationship
/// </summary>
public class ExamSubject : BaseEntity
{
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal PassMarks { get; set; } = 40;

    // Navigation properties
    public virtual Exam? Exam { get; set; }
    public virtual Subject? Subject { get; set; }
    public virtual ICollection<StudentMarks> StudentMarks { get; set; } = new List<StudentMarks>();
}

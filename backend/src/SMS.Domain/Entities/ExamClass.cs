using SMS.Domain.Enums;

namespace SMS.Domain.Entities;

/// <summary>
/// Junction table: Associates classes with an exam and tracks marks entry status
/// Single Responsibility: Represents exam-class relationship and marks submission status
/// </summary>
public class ExamClass : BaseEntity
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public MarksEntryStatus MarksEntryStatus { get; set; } = MarksEntryStatus.Pending;
    public DateTime? SubmittedAt { get; set; }
    public Guid? SubmittedById { get; set; }

    // Navigation properties
    public virtual Exam? Exam { get; set; }
    public virtual Class? Class { get; set; }
    public virtual User? SubmittedBy { get; set; }
}

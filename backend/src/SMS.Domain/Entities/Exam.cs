using SMS.Domain.Enums;

namespace SMS.Domain.Entities;

/// <summary>
/// Represents an examination/test in the school
/// Single Responsibility: Represents the exam entity with all required properties
/// </summary>
public class Exam : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalMarks { get; set; } = 100;
    public decimal PassMarks { get; set; } = 40;
    public ExamStatus Status { get; set; } = ExamStatus.Draft;
    public Guid AcademicYearId { get; set; }
    public Guid CreatedById { get; set; }

    // Navigation properties
    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual User? Creator { get; set; }
    public virtual ICollection<ExamSubject> ExamSubjects { get; set; } = new List<ExamSubject>();
    public virtual ICollection<ExamClass> ExamClasses { get; set; } = new List<ExamClass>();
    public virtual ICollection<StudentMarks> StudentMarks { get; set; } = new List<StudentMarks>();
    public virtual ICollection<StudentReportCard> StudentReportCards { get; set; } = new List<StudentReportCard>();
}

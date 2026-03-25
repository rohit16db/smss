namespace SMS.Domain.Entities;

public class AcademicYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; } = false;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
    public virtual ICollection<FeeStructure> FeeStructures { get; set; } = new List<FeeStructure>();
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
}

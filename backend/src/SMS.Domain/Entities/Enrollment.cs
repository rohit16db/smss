namespace SMS.Domain.Entities;

public class Enrollment : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid AcademicYearId { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SectionId { get; set; }
    public int? RollNumber { get; set; }
    public string Status { get; set; } = "Enrolled";
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Student? Student { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual Class? Class { get; set; }
    public virtual Section? Section { get; set; }
}

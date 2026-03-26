using System;

namespace SMS.Domain.Entities;

public class TimetableEntry : BaseEntity
{
    public Guid TimeSlotId { get; set; }
    public TimeSlot? TimeSlot { get; set; }

    public Guid SectionId { get; set; }
    public Section? Section { get; set; }

    public Guid SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public string? RoomNumber { get; set; }
    
    public Guid AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
}

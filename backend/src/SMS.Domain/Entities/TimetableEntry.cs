using System;

namespace SMS.Domain.Entities;

public class TimetableEntry : BaseEntity
{
    public Guid TimeSlotId { get; set; }
    public TimeSlot? TimeSlot { get; set; }

    public Guid StaffAssignmentId { get; set; }
    public StaffAssignment? StaffAssignment { get; set; }

    public string? RoomNumber { get; set; }
    
    public Guid AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }
}

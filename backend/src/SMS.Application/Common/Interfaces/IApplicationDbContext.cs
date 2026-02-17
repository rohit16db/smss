using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;

namespace SMS.Application.Common.Interfaces;

/// <summary>
/// Interface for the application database context
/// Allows Application layer to reference DbContext without depending on Infrastructure
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Student> Students { get; }
    
    // Phase 2: Class & Section Management
    DbSet<Class> Classes { get; }
    DbSet<Section> Sections { get; }
    DbSet<StudentSection> StudentSections { get; }
    
    // Phase 3: Teacher Management
    DbSet<Teacher> Teachers { get; }
    DbSet<TeacherAssignment> TeacherAssignments { get; }
    DbSet<Subject> Subjects { get; }
    
    // Phase 3: Fee Management
    DbSet<FeeStructure> FeeStructures { get; }
    DbSet<FeeStructureCategory> FeeStructureCategories { get; }
    DbSet<StudentFee> StudentFees { get; }
    DbSet<FeePayment> FeePayments { get; }
    
    // Phase 3: Attendance Management
    DbSet<StudentAttendance> StudentAttendances { get; }
    DbSet<TeacherAttendance> TeacherAttendances { get; }

    // Phase 8: Salary Management
    DbSet<SalaryPayment> SalaryPayments { get; }

    /// <summary>
    /// Save all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

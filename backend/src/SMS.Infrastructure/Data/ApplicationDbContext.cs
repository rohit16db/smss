using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Data;

/// <summary>
/// Main database context for the application
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    
    // Phase 2: Class & Section Management
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();    
    // Phase 3: Teacher Management
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<TeacherAssignment> TeacherAssignments => Set<TeacherAssignment>();
    public DbSet<Subject> Subjects => Set<Subject>();
    
    // Phase 3: Fee Management
    public DbSet<FeeStructure> FeeStructures => Set<FeeStructure>();
    public DbSet<FeeStructureCategory> FeeStructureCategories => Set<FeeStructureCategory>();
    public DbSet<StudentFee> StudentFees => Set<StudentFee>();
    public DbSet<FeePayment> FeePayments => Set<FeePayment>();
    
    // Phase 3: Attendance Management
    public DbSet<StudentAttendance> StudentAttendances => Set<StudentAttendance>();
    public DbSet<TeacherAttendance> TeacherAttendances => Set<TeacherAttendance>();
    
    // Holiday Management
    public DbSet<Holiday> Holidays => Set<Holiday>();

    // Phase 8: Salary Management
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<SalaryPayment> SalaryPayments => Set<SalaryPayment>();

    // Phase 4: Exam & Marks Management
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamSubject> ExamSubjects => Set<ExamSubject>();
    public DbSet<ExamClass> ExamClasses => Set<ExamClass>();
    public DbSet<StudentMarks> StudentMarks => Set<StudentMarks>();
    public DbSet<GradeConfiguration> GradeConfigurations => Set<GradeConfiguration>();
    public DbSet<StudentReportCard> StudentReportCards => Set<StudentReportCard>();

    // Phase 10: Timetable Management
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<TimetableEntry> TimetableEntries => Set<TimetableEntry>();
    
    // School Configuration
    public DbSet<School> Schools => Set<School>();

    /// <summary>
    /// Save changes with automatic audit field updates and DateTime UTC conversion
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Convert all DateTime values with Unspecified kind to UTC
        ConvertAllDateTimesToUtc();
        
        // Update timestamps automatically
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Convert all DateTime properties with Unspecified kind to UTC
    /// Prevents: PostgreSQL "Cannot write DateTime with Kind=Unspecified" errors
    /// </summary>
    private void ConvertAllDateTimesToUtc()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dateTime)
                {
                    if (dateTime.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    }
                    else if (dateTime.Kind == DateTimeKind.Local)
                    {
                        property.CurrentValue = dateTime.ToUniversalTime();
                    }
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

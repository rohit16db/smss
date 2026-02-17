using Microsoft.EntityFrameworkCore;
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
    public DbSet<StudentSection> StudentSections => Set<StudentSection>();
    
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

    // Phase 8: Salary Management
    public DbSet<SalaryPayment> SalaryPayments => Set<SalaryPayment>();

    /// <summary>
    /// Save changes with automatic audit field updates
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

# Data Model: Initial Project Setup

**Feature**: Initial Project Setup  
**Date**: 2026-01-12  
**Status**: Phase 1 - Infrastructure Foundation

## Overview

This feature establishes the database infrastructure foundation without implementing business entities. The data model for this phase focuses solely on the Entity Framework Core configuration structure that will support future entities (students, teachers, fees, etc.).

**Architecture Pattern**: This project uses CQRS (Command Query Responsibility Segregation) via MediatR, separating read operations (queries) from write operations (commands). The DbContext is used by command handlers for writes and query handlers for reads with optimized queries.

## Database Context

### ApplicationDbContext

**Purpose**: Central EF Core DbContext for the School Management System

**Configuration**:
- Inherits from `DbContext`
- Configured with PostgreSQL provider (Npgsql)
- Connection string from configuration/environment variables
- Migrations enabled
- Automatic timestamp handling (CreatedAt, UpdatedAt)
- Snake_case naming convention for PostgreSQL compatibility

**Base Structure**:
```csharp
// Located in: SMS.Infrastructure/Data/ApplicationDbContext.cs

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Future DbSets will be added here
    // public DbSet<Student> Students { get; set; }
    // public DbSet<Teacher> Teachers { get; set; }
    // etc.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all IEntityTypeConfiguration implementations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Global configuration
        ConfigureConventions(modelBuilder);
    }

    private void ConfigureConventions(ModelBuilder modelBuilder)
    {
        // Use snake_case for table and column names
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table names to snake_case
            entity.SetTableName(entity.GetTableName().ToSnakeCase());

            // Convert column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }

            // Convert foreign key names to snake_case
            foreach (var key in entity.GetKeys())
            {
                key.SetName(key.GetName().ToSnakeCase());
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(foreignKey.GetConstraintName().ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatic timestamp handling for future entities
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
```

## Base Entity Pattern

### BaseEntity (Abstract Class)

**Purpose**: Common properties for all domain entities (to be used by future entities)

**Location**: SMS.Domain/Entities/BaseEntity.cs

**Properties**:
- `Id` (Guid): Primary key, generated on creation
- `CreatedAt` (DateTime): UTC timestamp when entity was created
- `UpdatedAt` (DateTime): UTC timestamp when entity was last modified
- `IsDeleted` (bool): Soft delete flag (default: false)

**Rationale**:
- Guid IDs prevent enumeration attacks and enable distributed systems
- UTC timestamps avoid timezone confusion
- Soft deletes preserve audit trail and enable recovery
- Base class ensures consistency across all entities

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

## Configuration Structure

### Entity Configuration Pattern

**Purpose**: Separate entity configuration from DbContext for maintainability

**Pattern**: IEntityTypeConfiguration<T>

**Location**: SMS.Infrastructure/Data/Configurations/

**Example** (for future entities):
```csharp
// SMS.Infrastructure/Data/Configurations/StudentConfiguration.cs
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.FirstName)
            .IsRequired()
            .HasMaxLength(100);
        
        // Additional configuration...
    }
}
```

## Migrations

### Initial Migration

**Name**: `InitialCreate`

**Purpose**: Create database schema infrastructure

**Contains**:
- Database creation script
- Schema versioning table (__EFMigrationsHistory)
- Initial configuration (no business tables yet)

**Commands**:
```bash
# Create migration
dotnet ef migrations add InitialCreate --project src/SMS.Infrastructure --startup-project src/SMS.API

# Apply migration
dotnet ef database update --project src/SMS.Infrastructure --startup-project src/SMS.API

# View SQL script (optional)
dotnet ef migrations script --project src/SMS.Infrastructure --startup-project src/SMS.API
```

## Connection String Format

### Development
```
Host=localhost;Port=5432;Database=sms_db;Username=sms_user;Password=<secure-password>;Include Error Detail=true
```

### Production
```
Host=<db-host>;Port=5432;Database=sms_db;Username=sms_user;Password=<secure-password>;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;SSL Mode=Require
```

**Configuration Location**:
- Development: appsettings.Development.json or User Secrets
- Docker: Environment variable `DATABASE_CONNECTION_STRING`
- Production: Environment variable (injected by hosting platform)

## Database Schema (Current State)

### Tables
*No business tables in initial setup - just infrastructure*

**System Tables**:
- `__efmigrationshistory`: EF Core migration tracking

### Future Entities (Documented for Reference)
Based on PRD requirements, these entities will be added in subsequent features:
- Students
- Teachers
- Classes
- Subjects
- Fees
- FeePayments
- Salaries
- SalaryPayments
- Users (for authentication)
- Attendance (Phase 2)

## Indexing Strategy (For Future Implementation)

### Guidelines
- Primary keys: Clustered index (automatic)
- Foreign keys: Non-clustered index (explicit)
- Frequently filtered columns: Non-clustered index
- Composite indexes for common query patterns
- Avoid over-indexing (write performance penalty)

### Example (Future):
```csharp
builder.HasIndex(s => s.Email).IsUnique();
builder.HasIndex(s => s.ClassId);
builder.HasIndex(s => new { s.ClassId, s.IsDeleted }); // Composite
```

## Data Validation

### Validation Layers
1. **Database Level**: NOT NULL constraints, CHECK constraints, foreign keys
2. **Domain Level**: Business rule validation in entity methods
3. **Application Level**: DTO validation with Data Annotations or FluentValidation
4. **API Level**: Model state validation in controllers

### Current Implementation
For initial setup, validation infrastructure is prepared but no business rules implemented.

## Transaction Management

### Strategy
- DbContext tracks changes and commits as single transaction by default
- Explicit transactions via `context.Database.BeginTransactionAsync()` for multi-step operations
- Automatic rollback on exceptions
- Idempotent operations where possible

### Future Considerations
- Distributed transactions (if microservices adopted)
- Eventual consistency patterns (if event sourcing adopted)
- Saga pattern for complex workflows (if needed)

## Performance Considerations

### Connection Pooling
- Minimum: 5 connections
- Maximum: 100 connections
- Pooling enabled by default in Npgsql

### Query Optimization
- Use `.AsNoTracking()` for read-only queries
- Use `.Include()` for eager loading (prevent N+1)
- Use `.Select()` for projection (reduce data transfer)
- Implement pagination for all list queries

### Caching Strategy (Future)
- Deferred until performance testing proves necessity (Constitution Principle V: YAGNI)
- When needed: Redis for distributed caching
- Cache invalidation strategy to be defined

## Backup and Recovery

### Development
- Docker volume persistence: `postgres-data`
- Manual backup: `pg_dump` via Docker exec
- Restore: `pg_restore` via Docker exec

### Production (Future)
- Automated daily backups (30-day retention minimum per Constitution)
- Point-in-time recovery (PITR) configuration
- Backup testing quarterly
- Disaster recovery plan documentation

## Audit Trail (Future Feature)

### Design Consideration
Track who made changes and when:
- CreatedBy, UpdatedBy columns (Guid - User ID)
- CreatedAt, UpdatedAt timestamps (already in BaseEntity)
- Audit log table for sensitive operations (fees, salaries)
- Temporal tables for full history (if required)

**Status**: Deferred to future feature when user authentication is implemented

## Notes

### Why No Business Entities Yet?
This feature focuses on infrastructure setup. Business entities (Student, Teacher, etc.) will be added in separate features with proper domain modeling, validation rules, and business logic. This separation:
- Follows Constitution Principle V (Simplicity - YAGNI)
- Enables independent testing of infrastructure
- Prevents scope creep in setup feature
- Allows proper analysis before committing to schema

### Migration Strategy
- One migration per logical schema change
- Descriptive migration names (e.g., "AddStudentsTable", "AddFeeManagement")
- Never modify existing migrations after deployment
- Rollback via new migration (not down migration)
- Test migrations in development before production

### Database Naming Conventions
- Tables: plural, snake_case (students, fee_payments)
- Columns: singular, snake_case (first_name, created_at)
- Foreign keys: {table}_id (class_id, teacher_id)
- Indexes: idx_{table}_{columns} (idx_students_email)
- Constraints: ck_{table}_{column} (ck_students_age)

### PostgreSQL-Specific Features (Future Use)
- JSONB columns for flexible data (if needed)
- Full-text search (if search feature required)
- Partitioning for large tables (if scale requires)
- Materialized views for complex reporting (if performance requires)

---

**Phase 1 Status**: ✅ Data model structure defined, ready for implementation
**Next Step**: Generate contracts/health-api.yaml

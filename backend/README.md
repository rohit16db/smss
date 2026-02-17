# School Management System - Backend API

ASP.NET Core Web API with Clean Architecture and CQRS pattern.

## Architecture

```
backend/
├── src/
│   ├── SMS.Domain/          # Domain entities, interfaces, exceptions (no dependencies)
│   ├── SMS.Application/     # CQRS commands/queries, MediatR pipeline behaviors
│   ├── SMS.Infrastructure/  # EF Core DbContext, repositories, data access
│   └── SMS.API/            # Controllers, Program.cs, API configuration
└── SMS.sln
```

### Clean Architecture Layers

1. **Domain Layer** (`SMS.Domain`)
   - Core business entities
   - Domain interfaces
   - Domain exceptions
   - **Zero external dependencies**

2. **Application Layer** (`SMS.Application`)
   - CQRS Commands and Queries
   - MediatR handlers
   - Validation behaviors (FluentValidation)
   - Logging behaviors
   - DTOs and interfaces
   - **Depends only on Domain**

3. **Infrastructure Layer** (`SMS.Infrastructure`)
   - Entity Framework Core DbContext
   - Database migrations
   - Repository implementations
   - PostgreSQL configuration
   - **Depends on Application**

4. **API Layer** (`SMS.API`)
   - REST API controllers
   - Dependency injection setup
   - Swagger/OpenAPI configuration
   - Health checks
   - **Depends on Application and Infrastructure**

## Technology Stack

- **.NET 10.0** - Runtime platform
- **ASP.NET Core 10** - Web API framework
- **Entity Framework Core 10** - ORM
- **PostgreSQL 15+** - Database
- **MediatR 14** - CQRS implementation
- **FluentValidation 12** - Request validation
- **Swashbuckle 10** - Swagger/OpenAPI documentation

## Prerequisites

- .NET 10 SDK
- PostgreSQL 15+ (via Docker Compose or local installation)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

## Quick Start

### 1. Database Setup

Start PostgreSQL using Docker Compose from repository root:

```bash
docker-compose up -d postgres
```

Or configure your local PostgreSQL connection in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=sms_db;Username=sms_user;Password=your_password"
  }
}
```

### 2. Build the Solution

```bash
dotnet build
```

### 3. Run Database Migrations

```bash
# From backend/ directory
dotnet ef database update --project src/SMS.Infrastructure --startup-project src/SMS.API
```

### 4. Run the API

```bash
# From backend/ directory
dotnet run --project src/SMS.API
```

The API will start on:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

### 5. Access Swagger UI

Open your browser: `http://localhost:5000/swagger`

## Development Commands

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Run API with Hot Reload

```bash
dotnet watch --project src/SMS.API
```

### Create Migration

```bash
dotnet ef migrations add MigrationName --project src/SMS.Infrastructure --startup-project src/SMS.API
```

### Apply Migrations

```bash
dotnet ef database update --project src/SMS.Infrastructure --startup-project src/SMS.API
```

### Rollback Migration

```bash
dotnet ef database update PreviousMigrationName --project src/SMS.Infrastructure --startup-project src/SMS.API
```

### Remove Last Migration

```bash
dotnet ef migrations remove --project src/SMS.Infrastructure --startup-project src/SMS.API
```

## API Endpoints

### Health Checks

- **GET** `/health` - Basic health check
- **GET** `/health/ready` - Readiness check (includes DB)
- **GET** `/health/live` - Liveness check

### Example Response

```json
{
  "status": "Healthy",
  "timestamp": "2026-01-12T10:30:00Z",
  "service": "School Management System API",
  "version": "1.0.0"
}
```

## CQRS Pattern

### Commands (Write Operations)

Commands modify state and are placed in `SMS.Application/Commands/`:

```csharp
public record CreateStudentCommand : IRequest<Guid>
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
}

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    // Implementation
}
```

### Queries (Read Operations)

Queries fetch data without side effects, placed in `SMS.Application/Queries/`:

```csharp
public record GetStudentByIdQuery(Guid Id) : IRequest<StudentDto>;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    // Implementation
}
```

### Pipeline Behaviors

Automatically applied to all requests:

1. **LoggingBehavior** - Logs request execution time
2. **ValidationBehavior** - Validates requests using FluentValidation

## Configuration

### Connection Strings

- **Production**: Use User Secrets or environment variables
- **Development**: Set in `appsettings.Development.json`

### User Secrets (Recommended for Development)

```bash
# Initialize user secrets
dotnet user-secrets init --project src/SMS.API

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=sms_db;Username=sms_user;Password=secure_password" --project src/SMS.API
```

### CORS Configuration

Development CORS allows:
- `http://localhost:5173` (Vite default)
- `http://localhost:3000` (React default)

Update `Program.cs` to add more origins.

## Database Conventions

### Snake Case Naming

All database tables and columns use `snake_case` naming:

- `Student` entity → `student` table
- `FirstName` property → `first_name` column

Configured automatically in `ApplicationDbContext.OnModelCreating()`.

### Audit Fields

All entities inherit from `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

Timestamps are automatically updated in `SaveChangesAsync()`.

## Project Structure Details

### Domain Layer

```
SMS.Domain/
├── Entities/
│   └── BaseEntity.cs       # Base class with audit fields
├── Interfaces/
│   └── IRepository.cs      # Generic repository interface
└── Exceptions/
    └── DomainException.cs  # Domain-specific exceptions
```

### Application Layer

```
SMS.Application/
├── Commands/               # Write operations
├── Queries/               # Read operations
├── DTOs/                  # Data transfer objects
└── Common/
    ├── Behaviors/
    │   ├── ValidationBehavior.cs
    │   └── LoggingBehavior.cs
    └── Interfaces/
        └── IApplicationDbContext.cs
```

### Infrastructure Layer

```
SMS.Infrastructure/
├── Data/
│   └── ApplicationDbContext.cs  # EF Core DbContext
└── Repositories/                # Repository implementations
```

### API Layer

```
SMS.API/
├── Controllers/
│   └── HealthController.cs
├── Program.cs                   # DI, middleware configuration
├── appsettings.json
└── appsettings.Development.json
```

## Contributing

1. Follow Clean Architecture principles
2. Keep Domain layer dependency-free
3. Use CQRS for all business operations
4. Add validation for all commands
5. Write unit tests for handlers
6. Update Swagger documentation

## Troubleshooting

### Build Errors

**Issue**: Missing package references
**Solution**: Run `dotnet restore` from `backend/` directory

### Database Connection Errors

**Issue**: Cannot connect to PostgreSQL
**Solution**: 
1. Verify PostgreSQL is running: `docker-compose ps`
2. Check connection string in `appsettings.Development.json`
3. Ensure database exists: `docker-compose exec postgres psql -U sms_user -l`

### Migration Errors

**Issue**: Migrations not applying
**Solution**:
1. Check EF Core tools installed: `dotnet ef --version`
2. Verify connection string is correct
3. Run migrations manually: `dotnet ef database update --project src/SMS.Infrastructure --startup-project src/SMS.API --verbose`

## License

See root LICENSE file.

## Support

For issues and questions, see the main project [README](../README.md).

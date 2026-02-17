# Research: Initial Project Setup

**Feature**: Initial Project Setup  
**Date**: 2026-01-12  
**Purpose**: Resolve technical unknowns and document technology choices for foundational infrastructure setup

## Research Tasks

### 1. ASP.NET Core Clean Architecture Best Practices

**Question**: What is the recommended structure for Clean Architecture in ASP.NET Core 8 for a school management system?

**Findings**:

**Decision**: 4-project solution structure
- **SMS.Domain**: Core entities, value objects, domain events, interfaces (zero dependencies)
- **SMS.Application**: Use cases, DTOs, application services, CQRS patterns (depends on Domain only)
- **SMS.Infrastructure**: EF Core, repositories, external services (depends on Application)
- **SMS.API**: Controllers, middleware, Swagger (depends on Application)

**Rationale**:
- Clear separation of concerns aligns with Constitution Principle I
- Domain isolation ensures business rules are testable without infrastructure
- Dependency inversion via interfaces in Domain/Application
- Infrastructure can be swapped without touching business logic
- Proven pattern for .NET applications at scale

**Alternatives Considered**:
- 3-project structure (combining Application + Domain): Rejected - loses domain isolation
- 5-project structure (separate Shared/Common): Rejected - adds complexity without clear benefit (violates YAGNI)
- Vertical slice architecture: Rejected - better for established teams, Clean Architecture better for greenfield with growth plans

---

### 2. CQRS Pattern Implementation with MediatR

**Question**: How should we implement CQRS pattern in ASP.NET Core to separate read and write operations?

**Findings**:

**Decision**: Use MediatR library with CQRS pattern
- Commands for write operations (Create, Update, Delete)
- Queries for read operations (Get, List)
- MediatR as mediator for handling commands and queries
- Separate handlers for each command and query
- Validation using FluentValidation in pipeline behaviors
- Located in Application layer

**CQRS Structure**:
```
SMS.Application/
├── Commands/
│   ├── CreateStudent/
│   │   ├── CreateStudentCommand.cs
│   │   ├── CreateStudentCommandHandler.cs
│   │   └── CreateStudentCommandValidator.cs
│   └── UpdateStudent/
│       ├── UpdateStudentCommand.cs
│       └── UpdateStudentCommandHandler.cs
├── Queries/
│   ├── GetStudent/
│   │   ├── GetStudentQuery.cs
│   │   └── GetStudentQueryHandler.cs
│   └── GetStudents/
│       ├── GetStudentsQuery.cs
│       └── GetStudentsQueryHandler.cs
└── Common/
    ├── Behaviors/
    │   ├── ValidationBehavior.cs
    │   └── LoggingBehavior.cs
    └── Interfaces/
```

**Rationale**:
- Clear separation of concerns between reads and writes
- Single Responsibility Principle - each handler does one thing
- Easy to test handlers independently
- Scalable - can optimize queries separately from commands
- MediatR reduces coupling between controllers and business logic
- Aligns with Constitution Principle I (Clean Architecture)
- Enables future CQRS scaling (separate read/write databases if needed)

**Alternatives Considered**:
- No CQRS (traditional repository pattern): Rejected - less scalable, mixed concerns
- Custom mediator implementation: Rejected - MediatR is battle-tested and well-documented
- Event Sourcing + CQRS: Rejected - too complex for Phase 1 (violates YAGNI), can add later if needed

**MediatR Pipeline Behaviors**:
- Validation: FluentValidation to validate commands/queries before execution
- Logging: Log all commands and queries for audit trail
- Transaction: Automatic transaction handling for commands
- Exception handling: Centralized error handling

**Controller Pattern**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateStudentCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result }, result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id)
    {
        var query = new GetStudentQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

**Benefits for School Management System**:
- Audit trail: All commands logged for fee/salary transactions
- Performance: Queries optimized separately (AsNoTracking)
- Security: Validation in pipeline before reaching handlers
- Testability: Mock IMediator for controller tests
- Maintainability: New features = new handlers, no existing code changes

---

### 3. Entity Framework Core with PostgreSQL Configuration

**Question**: How should we configure EF Core with PostgreSQL for optimal performance and development experience?

**Findings**:

**Decision**: Use Npgsql.EntityFrameworkCore.PostgreSQL with migrations
- Connection pooling enabled (min: 5, max: 100 connections)
- Migrations stored in Infrastructure/Data/Migrations
- DbContext scoped lifetime in DI container
- Separate configuration classes for each entity (IEntityTypeConfiguration)
- UTC timestamps for all date/time fields
- Snake_case naming convention for PostgreSQL (via EF Core naming convention extension)

**Rationale**:
- Npgsql is the official and most mature PostgreSQL provider for EF Core
- Connection pooling reduces overhead and improves performance
- Migrations provide version control for schema changes
- Entity configurations keep DbContext clean and modular
- UTC timestamps prevent timezone issues
- Snake_case aligns with PostgreSQL conventions

**Alternatives Considered**:
- Dapper (micro-ORM): Rejected - more manual work, less type safety, violates Constitution's EF Core mandate
- Fluent Migrations: Rejected - EF Core migrations are built-in and sufficient
- Code-first without migrations: Rejected - loses schema versioning capability

**Best Practices**:
- Always use async methods (.ToListAsync(), .FirstOrDefaultAsync())
- Use `.AsNoTracking()` for read-only queries
- Implement soft deletes (IsDeleted flag) rather than hard deletes
- Use proper indexes on foreign keys and frequently queried columns
- Configure cascade delete behavior explicitly

---

### 3. React + Vite + UI Library Selection

**Question**: Should we use Material UI or Ant Design, and how should we structure the React application with Vite?

**Findings**:

**Decision**: Use **Material UI (MUI)** with Vite + React Query
- Material UI v5 (latest stable)
- React Query v5 (TanStack Query) for server state
- React Router v6 for routing
- Axios for HTTP client
- Vite v5 for build tooling

**Rationale**:
- Material UI has larger community, better documentation, more examples
- MUI follows Material Design principles (familiar to users)
- Better TypeScript support than Ant Design
- More frequent updates and active maintenance
- Lighter bundle size with tree-shaking
- Customizable theming system

**Alternatives Considered**:
- Ant Design: Rejected - designed for Chinese market, less Western design patterns, heavier bundle
- Chakra UI: Rejected - less mature, smaller ecosystem
- Tailwind CSS + Headless UI: Rejected - requires more custom styling work (violates YAGNI for MVP)

**React Project Structure**:
```
src/
├── components/      # Reusable components
│   ├── common/     # Generic UI components
│   └── layout/     # Layout components (Header, Sidebar)
├── pages/          # Route components
├── services/       # API client and queries
│   ├── api/       # Axios configuration
│   └── queries/   # React Query hooks
├── utils/          # Helper functions
├── types/          # TypeScript type definitions
└── theme/          # MUI theme customization
```

---

### 4. Docker Compose Configuration for Development

**Question**: How should Docker Compose be configured for local development with hot reload and debugging?

**Findings**:

**Decision**: Multi-stage Dockerfiles with docker-compose.yml + docker-compose.override.yml
- `docker-compose.yml`: Production-like configuration
- `docker-compose.override.yml`: Development overrides (volume mounts, debug ports)
- PostgreSQL container with named volume for persistence
- Backend container with hot reload via dotnet watch
- Frontend container with Vite HMR (Hot Module Replacement)
- Network isolation with custom bridge network

**Services**:
1. **postgres**: PostgreSQL 15-alpine, port 5432, volume for data persistence
2. **backend**: ASP.NET Core API, port 5000 (HTTP) and 5001 (HTTPS), depends on postgres
3. **frontend**: React + Vite dev server, port 5173, depends on backend

**Rationale**:
- Docker Compose simplifies multi-container orchestration
- Override file separates dev-specific config from production baseline
- Named volumes persist data across container restarts
- Volume mounts enable hot reload without rebuilding images
- Alpine Linux images reduce image size
- Custom network allows service discovery by name

**Development Workflow**:
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Rebuild after dependency changes
docker-compose up -d --build

# Stop all services
docker-compose down

# Stop and remove volumes (fresh start)
docker-compose down -v
```

**Alternatives Considered**:
- Separate docker run commands: Rejected - too complex, no orchestration
- Kubernetes: Rejected - massive overkill for local dev (violates Simplicity principle)
- Podman Compose: Rejected - less mature, compatibility issues

---

### 5. Environment Configuration and Secrets Management

**Question**: How should we manage environment variables and secrets for different environments?

**Findings**:

**Decision**: .env files with .env.example templates
- `.env` file for local development (gitignored)
- `.env.example` committed to repository (template with placeholders)
- ASP.NET Core User Secrets for sensitive local config
- Docker Compose env_file directive for container environment variables
- Separate .env files per environment (.env.development, .env.staging, .env.production)

**Backend Configuration**:
- appsettings.json: Non-sensitive defaults
- appsettings.Development.json: Development overrides
- Environment variables: Secrets and environment-specific values
- User Secrets (dotnet user-secrets): Local developer secrets

**Frontend Configuration**:
- Vite environment variables prefixed with VITE_
- .env.development for local API URL
- .env.production for production API URL
- Build-time variable substitution

**Required Environment Variables**:
```bash
# Database
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=sms_db
DATABASE_USER=sms_user
DATABASE_PASSWORD=<secure-password>

# Backend API
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000;https://+:5001
JWT_SECRET=<min-32-char-secret>
JWT_EXPIRY_HOURS=8

# Frontend
VITE_API_BASE_URL=http://localhost:5000
VITE_APP_TITLE=School Management System
```

**Rationale**:
- .env files are standard practice in modern development
- .env.example provides documentation without exposing secrets
- Different files per environment prevent accidental production secret leaks
- ASP.NET Core User Secrets keeps local secrets out of source control
- Vite's VITE_ prefix prevents accidental exposure of server-side secrets

**Alternatives Considered**:
- Hardcoded values: Rejected - security risk, violates Constitution Principle III
- Azure Key Vault / AWS Secrets Manager: Rejected - costs money, not needed for local dev
- .NET Core Protected Configuration: Rejected - too complex for Phase 1

---

### 6. API Health Check Implementation

**Question**: What should the health check endpoint return and how should it be implemented?

**Findings**:

**Decision**: Use ASP.NET Core Health Checks middleware
- Endpoint: `GET /health`
- Response: JSON with status, timestamp, and checks
- Database connectivity check included
- Swagger documentation for health endpoint

**Response Format**:
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-12T10:30:00Z",
  "checks": {
    "database": "Healthy",
    "api": "Healthy"
  },
  "duration": "00:00:00.0234567"
}
```

**Status Codes**:
- 200 OK: All checks healthy
- 503 Service Unavailable: Any check unhealthy

**Rationale**:
- Built-in ASP.NET Core feature (no external dependencies)
- Standardized health check format
- Enables monitoring and container orchestration health probes
- Database check verifies EF Core connection
- Lightweight and fast (<100ms requirement)

**Implementation**:
```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "database");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

**Alternatives Considered**:
- Custom health endpoint without middleware: Rejected - reinventing the wheel
- /ping endpoint with simple string: Rejected - less informative, no database check
- Multiple health endpoints: Rejected - overcomplicated for Phase 1

---

### 7. CORS Configuration for Local Development

**Question**: How should CORS be configured to allow frontend-backend communication locally?

**Findings**:

**Decision**: Environment-specific CORS policies
- Development: Allow all origins (localhost:5173, localhost:3000)
- Production: Whitelist specific frontend domain only
- Allow credentials (for future JWT cookie auth)
- Allow common HTTP methods (GET, POST, PUT, DELETE, PATCH)
- Allow common headers (Content-Type, Authorization)

**Development Configuration**:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Apply middleware
app.UseCors("DevelopmentPolicy");
```

**Rationale**:
- CORS is required for browser security (same-origin policy)
- Different policies per environment balance security and dev experience
- AllowCredentials enables future cookie-based auth
- Specific origins prevent CORS attacks in production

**Alternatives Considered**:
- AllowAnyOrigin: Rejected - security risk in production
- No CORS policy: Rejected - frontend cannot call backend
- Proxy server in frontend: Rejected - adds complexity, not idiomatic

---

## Technology Stack Summary

### Backend
- **Runtime**: .NET 8 SDK (LTS)
- **Framework**: ASP.NET Core Web API 8.0
- **Architecture**: Clean Architecture (4 projects) + CQRS
- **CQRS Library**: MediatR with FluentValidation
- **ORM**: Entity Framework Core 8 with Npgsql
- **Authentication**: JWT Bearer (implementation in future feature)
- **API Documentation**: Swagger/Swashbuckle
- **Testing**: xUnit, FluentAssertions, Moq
- **Container**: Docker (multi-stage build)

### Frontend
- **Runtime**: Node.js 20 LTS
- **Framework**: React 18
- **Build Tool**: Vite 5
- **UI Library**: Material UI v5
- **State Management**: React Query v5, Context API
- **Routing**: React Router v6
- **HTTP Client**: Axios
- **Testing**: Vitest, React Testing Library
- **Container**: Docker (nginx for production)

### Database
- **RDBMS**: PostgreSQL 15 (Alpine Linux container)
- **Migrations**: EF Core Migrations
- **Connection Pooling**: Npgsql (5-100 connections)
- **Naming Convention**: snake_case

### DevOps
- **Containerization**: Docker & Docker Compose
- **Development**: Hot reload (dotnet watch, Vite HMR)
- **Environment Config**: .env files, User Secrets
- **Version Control**: Git with .gitignore

---

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| .NET 10 not available | User mentioned Core 10, but it doesn't exist | Use .NET 8 LTS (current stable), document in README |
| Developer lacks Docker experience | Setup takes longer | Provide detailed quickstart.md with screenshots |
| PostgreSQL port conflict (5432) | Cannot start database | Document how to check/change port in docker-compose |
| Node.js version mismatch | Frontend build fails | Specify Node 20 LTS in README, use .nvmrc file |
| UI library learning curve | Slower frontend development | Include Material UI examples in research.md |
| Docker build times | Slow feedback loop | Use multi-stage builds, .dockerignore, layer caching |

---

## Next Steps (Phase 1)

With research complete, proceed to:
1. Generate data-model.md (minimal - just DbContext structure for now)
2. Generate contracts/health-api.yaml (OpenAPI spec for health endpoint)
3. Generate quickstart.md (step-by-step setup instructions)
4. Update agent context with new technologies

**No blocking unknowns remain** - all technical decisions documented with rationale.

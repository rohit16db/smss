# Implementation Plan: Initial Project Setup

**Branch**: `001-project-setup` | **Date**: 2026-01-12 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-project-setup/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Set up foundational infrastructure for School Management Software including ASP.NET Core backend with Clean Architecture, React frontend with Vite, PostgreSQL database with Entity Framework Core, and Docker Compose for containerized deployment. This establishes the development environment and project structure for all subsequent features while adhering to constitution principles of clean architecture, cost-effectiveness, and security-first design.

## Technical Context

**Language/Version**: 
- Backend: C# with ASP.NET Core 8 (Note: User mentioned Core 10, but .NET 10 is not yet released. Using .NET 8 LTS as per constitution)
- Frontend: TypeScript/JavaScript with React 18+
- Runtime: .NET 8 SDK, Node.js 20 LTS

**Primary Dependencies**: 
- Backend: ASP.NET Core Web API, Entity Framework Core 8, Npgsql.EntityFrameworkCore.PostgreSQL, MediatR (for CQRS), JWT Bearer Authentication, Swagger/Swashbuckle
- Frontend: React 18, Vite 5, React Query (TanStack Query), React Router 6, Material UI (selected in research phase)

**Storage**: PostgreSQL 15+ with EF Core migrations, connection pooling via Npgsql

**Testing**: 
- Backend: xUnit with FluentAssertions, Moq for mocking, Microsoft.AspNetCore.Mvc.Testing for integration tests
- Frontend: Vitest with React Testing Library

**Target Platform**: 
- Development: Windows/macOS/Linux via Docker Compose
- Production: Linux containers (Docker)
- Web browsers: Modern evergreen browsers (Chrome, Firefox, Safari, Edge)

**Project Type**: Web application (backend + frontend)

**Performance Goals**: 
- API health check response < 100ms
- Frontend initial load < 2s on broadband
- Database connection established < 500ms
- Docker compose up to ready state < 30s

**Constraints**: 
- Zero-cost development environment (free tools only)
- Must work offline after initial setup (local Docker containers)
- Cross-platform compatibility (Windows, macOS, Linux)
- No cloud services required for local development

**Scale/Scope**: 
- Phase 1: Support 200 students, single admin user
- Architecture ready to scale to 1,000+ students
- 5 user stories, 15 functional requirements
- Estimated 2-3 days for complete setup implementation

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle I: Clean Architecture (NON-NEGOTIABLE)
**Status**: ✅ COMPLIANT

**Verification**:
- Backend structure enforces dependency flow: Domain ← Application ← Infrastructure ← API
- Each layer in separate project with explicit references
- Domain entities have zero external dependencies
- EF Core isolated in Infrastructure layer

**Action**: Create 4-project solution structure with proper project references

---

### Principle II: Cost-Effectiveness
**Status**: ✅ COMPLIANT

**Verification**:
- All development tools are free (Docker, .NET SDK, Node.js, PostgreSQL)
- No cloud services required for local development
- Docker Compose enables free local hosting
- All libraries use free/open-source licenses

**Action**: Document zero-cost setup in quickstart.md

---

### Principle III: Security-First
**Status**: ✅ COMPLIANT

**Verification**:
- Docker Compose will use environment variables for secrets (not committed)
- HTTPS configuration prepared (even for local dev with self-signed certs)
- .gitignore excludes .env files, connection strings, certificates
- PostgreSQL requires password authentication (no trust mode)

**Action**: Create .env.example templates without actual secrets

---

### Principle IV: Scalability by Design
**Status**: ✅ COMPLIANT

**Verification**:
- Stateless API design (no in-memory session state)
- PostgreSQL supports vertical and horizontal scaling
- Docker containers enable scaling via orchestration later
- EF Core connection pooling configured from start

**Action**: Configure proper connection pooling and document scaling paths

---

### Principle V: Simplicity & Pragmatism (YAGNI)
**Status**: ✅ COMPLIANT

**Verification**:
- Single monolithic backend (no microservices)
- Standard project structure without over-abstraction
- Docker Compose for simplicity (not Kubernetes)
- No caching, message queues, or distributed systems in initial setup

**Action**: Document decisions to defer complexity in research.md

---

**GATE RESULT**: ✅ PASSED - All principles compliant, proceed to Phase 0 research

## Project Structure

### Documentation (this feature)

```text
specs/001-project-setup/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── health-api.yaml  # OpenAPI spec for health check endpoint
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
# Web application structure (backend + frontend + docker)

backend/
├── src/
│   ├── SMS.Domain/              # Core business entities (no external dependencies)
│   │   ├── Entities/            # Domain models (future: Student, Teacher, etc.)
│   │   ├── Interfaces/          # Repository and service contracts
│   │   └── SMS.Domain.csproj
│   │
│   ├── SMS.Application/         # Use cases and business logic (CQRS)
│   │   ├── Commands/            # Write operations (Create, Update, Delete)
│   │   ├── Queries/             # Read operations (Get, List)
│   │   ├── Common/              # Shared application logic
│   │   │   ├── Behaviors/       # MediatR pipeline behaviors
│   │   │   └── Interfaces/      # Shared interfaces
│   │   ├── DTOs/                # Data transfer objects
│   │   └── SMS.Application.csproj
│   │
│   ├── SMS.Infrastructure/      # External concerns (database, file system)
│   │   ├── Data/                # EF Core DbContext and configurations
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Migrations/      # EF Core migrations
│   │   ├── Repositories/        # Repository implementations
│   │   └── SMS.Infrastructure.csproj
│   │
│   └── SMS.API/                 # Web API controllers and middleware
│       ├── Controllers/         # API endpoints
│       │   └── HealthController.cs
│       ├── Middleware/          # Custom middleware
│       ├── Program.cs           # Application entry point
│       ├── appsettings.json     # Configuration (no secrets)
│       ├── appsettings.Development.json
│       └── SMS.API.csproj
│
├── tests/
│   ├── SMS.Domain.Tests/        # Domain unit tests
│   ├── SMS.Application.Tests/   # Application unit tests
│   └── SMS.API.Tests/           # Integration tests
│
├── SMS.sln                      # Visual Studio solution file
└── README.md                    # Backend setup instructions

frontend/
├── src/
│   ├── components/              # Reusable UI components
│   │   ├── common/              # Shared components (Button, Input, etc.)
│   │   └── layout/              # Layout components (Header, Sidebar, etc.)
│   │
│   ├── pages/                   # Page-level components
│   │   └── Home.tsx             # Landing page
│   │
│   ├── services/                # API client and React Query hooks
│   │   ├── api/                 # Axios client configuration
│   │   │   └── client.ts        # Base API client
│   │   └── queries/             # React Query hooks
│   │       └── useHealth.ts     # Health check query
│   │
│   ├── utils/                   # Helper functions
│   │   └── constants.ts         # App constants
│   │
│   ├── App.tsx                  # Root component with routing
│   ├── main.tsx                 # Application entry point
│   └── vite-env.d.ts           # Vite type declarations
│
├── public/                      # Static assets
│   └── vite.svg
│
├── tests/                       # Frontend tests
│   └── App.test.tsx
│
├── index.html                   # HTML entry point
├── package.json                 # Dependencies and scripts
├── vite.config.ts              # Vite configuration
├── tsconfig.json               # TypeScript configuration
└── README.md                    # Frontend setup instructions

# Docker and root-level files
docker-compose.yml               # Multi-container orchestration
docker-compose.override.yml      # Local development overrides
.dockerignore                    # Docker build exclusions
Dockerfile.backend               # Backend container definition
Dockerfile.frontend              # Frontend container definition

.env.example                     # Environment variable template (no secrets)
.gitignore                       # Git exclusions
README.md                        # Root project documentation
```

**Structure Decision**: Web application structure selected based on backend (ASP.NET Core) + frontend (React) + database (PostgreSQL) requirements. Clean Architecture enforced via 4-project backend structure with dependency flow: API → Application → Domain ← Infrastructure. Docker Compose provides containerization for development and deployment consistency.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**Status**: No violations - all constitution principles compliant.

This feature adheres to Principle V (Simplicity & Pragmatism) by:
- Using monolithic architecture (no microservices)
- Standard Docker Compose (no Kubernetes)
- No premature abstractions or unnecessary patterns
- Deferring caching, message queues, and distributed systems to future phases

---

## Post-Design Constitution Re-Check

*GATE: Final verification after Phase 1 design completion*

### Re-evaluation Summary

**Status**: ✅ ALL PRINCIPLES REMAIN COMPLIANT

#### Principle I: Clean Architecture
- **Compliant**: 4-project structure implemented in data-model.md
- **Evidence**: Clear dependency flow documented, BaseEntity pattern established
- **Action**: None - design maintains compliance

#### Principle II: Cost-Effectiveness
- **Compliant**: All tools remain free/open-source
- **Evidence**: quickstart.md documents zero-cost setup
- **Action**: None - no cost increases from design

#### Principle III: Security-First
- **Compliant**: Environment variable management, no hardcoded secrets
- **Evidence**: .env.example pattern, User Secrets for development
- **Action**: None - security measures in place

#### Principle IV: Scalability
- **Compliant**: Connection pooling, stateless API, proper indexing strategy documented
- **Evidence**: data-model.md includes scalability considerations
- **Action**: None - scalability built into foundation

#### Principle V: Simplicity
- **Compliant**: No over-engineering, deferred complexity
- **Evidence**: No business entities yet, minimal infrastructure only
- **Action**: None - YAGNI principle followed

**FINAL GATE RESULT**: ✅ PASSED - All principles maintained through design phase

---

## Artifacts Generated

### Phase 0: Research
- ✅ [research.md](research.md) - Technology choices and best practices documented

### Phase 1: Design
- ✅ [data-model.md](data-model.md) - Database context and entity structure
- ✅ [contracts/health-api.yaml](contracts/health-api.yaml) - OpenAPI specification for health endpoint
- ✅ [quickstart.md](quickstart.md) - Step-by-step setup guide (15-minute setup time)

### Agent Context
- ✅ Updated copilot-instructions.md with PostgreSQL and project structure

---

## Ready for Implementation

**Status**: ✅ Planning Complete - Ready for `/speckit.tasks`

**Next Command**: `/speckit.tasks` to generate task breakdown

**Summary**:
- All research questions resolved
- All technical unknowns documented with rationale
- Design compliant with all constitution principles
- Quickstart guide provides 15-minute setup path
- Health API contract documented in OpenAPI format
- Agent context updated with new technologies

**Estimated Implementation Time**: 2-3 days for complete setup

**Critical Path**:
1. Backend Clean Architecture structure (P1)
2. PostgreSQL + EF Core configuration (P1)
3. React + Vite frontend setup (P1)
4. Docker Compose orchestration
5. Integration testing and documentation

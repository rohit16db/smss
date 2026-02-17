---
description: "Task list for Initial Project Setup implementation"
---

# Tasks: Initial Project Setup

**Input**: Design documents from `/specs/001-project-setup/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: Not requested in specification - focus on infrastructure verification via health checks and manual testing

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

**Web app structure**: `backend/src/`, `frontend/src/`, root-level Docker files

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic directory structure

- [x] T001 Create root-level project structure (backend/, frontend/, specs/, .github/ directories)
- [x] T002 [P] Create .gitignore file at repository root with .NET, Node, Docker exclusions
- [x] T003 [P] Create .env.example file at repository root with template environment variables
- [x] T004 [P] Create root README.md with project overview and links to backend/frontend READMs

**Checkpoint**: Directory structure exists, version control configured

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 Install .NET 8 SDK and verify with `dotnet --version`
- [x] T006 Install Node.js 20 LTS and verify with `node --version`
- [x] T007 Install Docker Desktop and verify with `docker --version`
- [x] T008 Install EF Core tools globally with `dotnet tool install --global dotnet-ef`

**Checkpoint**: All required SDKs and tools installed - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Backend API Foundation (Priority: P1) 🎯 MVP

**Goal**: Set up ASP.NET Core Web API with Clean Architecture structure (4 projects) and CQRS pattern

**Independent Test**: Run `dotnet build` successfully, access `/health` endpoint and get HTTP 200, verify project references enforce dependency flow

### Backend Solution Structure

- [x] T009 [US1] Create backend/SMS.sln solution file
- [x] T010 [P] [US1] Create SMS.Domain class library in backend/src/SMS.Domain/
- [x] T011 [P] [US1] Create SMS.Application class library in backend/src/SMS.Application/
- [x] T012 [P] [US1] Create SMS.Infrastructure class library in backend/src/SMS.Infrastructure/
- [x] T013 [P] [US1] Create SMS.API web API project in backend/src/SMS.API/
- [x] T014 [US1] Add project references: API → Application, Application → Domain, Infrastructure → Application
- [x] T015 [US1] Verify dependency flow by attempting invalid reference (Domain → Infrastructure should fail)

### Domain Layer Setup

- [x] T016 [P] [US1] Create BaseEntity abstract class in backend/src/SMS.Domain/Entities/BaseEntity.cs
- [x] T017 [P] [US1] Create IRepository interface in backend/src/SMS.Domain/Interfaces/IRepository.cs
- [x] T018 [P] [US1] Add common domain exceptions in backend/src/SMS.Domain/Exceptions/

### Application Layer Setup (CQRS)

- [x] T019 [P] [US1] Install MediatR NuGet package in SMS.Application project
- [x] T020 [P] [US1] Install FluentValidation NuGet package in SMS.Application project
- [x] T021 [P] [US1] Create Commands/ directory in backend/src/SMS.Application/
- [x] T022 [P] [US1] Create Queries/ directory in backend/src/SMS.Application/
- [x] T023 [P] [US1] Create Common/Behaviors/ directory in backend/src/SMS.Application/
- [x] T024 [P] [US1] Create Common/Interfaces/ directory in backend/src/SMS.Application/
- [x] T025 [P] [US1] Create DTOs/ directory in backend/src/SMS.Application/
- [x] T026 [US1] Implement ValidationBehavior pipeline in backend/src/SMS.Application/Common/Behaviors/ValidationBehavior.cs
- [x] T027 [US1] Implement LoggingBehavior pipeline in backend/src/SMS.Application/Common/Behaviors/LoggingBehavior.cs

### Infrastructure Layer Setup

- [x] T028 [P] [US1] Install Npgsql.EntityFrameworkCore.PostgreSQL in SMS.Infrastructure
- [x] T029 [P] [US1] Create Data/ directory in backend/src/SMS.Infrastructure/
- [x] T030 [P] [US1] Create Repositories/ directory in backend/src/SMS.Infrastructure/
- [x] T031 [US1] Create ApplicationDbContext class in backend/src/SMS.Infrastructure/Data/ApplicationDbContext.cs with snake_case conventions
- [x] T032 [US1] Implement SaveChangesAsync with automatic timestamp handling in ApplicationDbContext

### API Layer Setup

- [x] T033 [P] [US1] Install Swashbuckle.AspNetCore (Swagger) in SMS.API
- [x] T034 [P] [US1] Install Microsoft.AspNetCore.HealthChecks.NpgSql in SMS.API
- [x] T035 [US1] Configure MediatR dependency injection in backend/src/SMS.API/Program.cs
- [x] T036 [US1] Configure Swagger with XML documentation in backend/src/SMS.API/Program.cs
- [x] T037 [US1] Add health checks middleware in backend/src/SMS.API/Program.cs
- [x] T038 [US1] Create HealthController in backend/src/SMS.API/Controllers/HealthController.cs
- [x] T039 [US1] Configure CORS policy for development in backend/src/SMS.API/Program.cs
- [x] T040 [US1] Create appsettings.json with logging configuration (no secrets)
- [x] T041 [US1] Create appsettings.Development.json with development overrides
- [x] T042 [US1] Create .editorconfig in backend/ directory with C# formatting rules

### Backend Testing & Verification

- [x] T043 [US1] Run `dotnet build` from backend/ directory - verify success
- [x] T044 [US1] Run `dotnet run --project src/SMS.API` - verify starts on port 5000
- [x] T045 [US1] Access http://localhost:5000/swagger - verify Swagger UI loads
- [x] T046 [US1] Create backend/README.md with setup instructions and architecture diagram

**Checkpoint**: Backend API foundation complete - can start independently, Swagger accessible, Clean Architecture verified

---

## Phase 4: User Story 2 - Database Configuration (Priority: P1)

**Goal**: Configure PostgreSQL database with Entity Framework Core migrations and verify connectivity

**Independent Test**: Run `dotnet ef database update`, verify database created, application connects without errors

### Database Container Setup

- [x] T047 [US2] Create docker-compose.yml at repository root with PostgreSQL 15-alpine service
- [x] T048 [US2] Configure PostgreSQL environment variables in docker-compose.yml (user, password, database)
- [x] T049 [US2] Add postgres-data named volume for data persistence in docker-compose.yml
- [x] T050 [US2] Create docker-compose.override.yml for development-specific settings

### EF Core Configuration

- [x] T051 [US2] Configure DbContext with connection string from environment in backend/src/SMS.API/Program.cs
- [x] T052 [US2] Configure connection pooling (min: 5, max: 100) in DbContext options
- [x] T053 [US2] Add User Secrets initialization to backend/src/SMS.API/ project
- [x] T054 [US2] Set connection string in User Secrets: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<value>"`
- [x] T055 [US2] Create extension method for snake_case naming in backend/src/SMS.Infrastructure/Data/Extensions/StringExtensions.cs

### Migrations

- [x] T056 [US2] Start PostgreSQL container: `docker-compose up -d postgres`
- [x] T057 [US2] Create initial migration: `dotnet ef migrations add InitialCreate --project src/SMS.Infrastructure --startup-project src/SMS.API`
- [x] T058 [US2] Apply migration: `dotnet ef database update --project src/SMS.Infrastructure --startup-project src/SMS.API`
- [x] T059 [US2] Verify database created: `docker-compose exec postgres psql -U sms_user -d sms_db -c "\dt"`

### Database Health Check

- [x] T060 [US2] Update health check endpoint to include PostgreSQL check in backend/src/SMS.API/Program.cs
- [x] T061 [US2] Test health endpoint returns database status: http://localhost:5000/health
- [x] T062 [US2] Test graceful failure by stopping PostgreSQL and verifying 503 error
- [x] T063 [US2] Document connection string format in backend/README.md

**Checkpoint**: Database configured, migrations working, health check verifies connectivity

---

## Phase 5: User Story 3 - Frontend React Application (Priority: P1)

**Goal**: Set up React application with Vite, Material UI, and essential routing

**Independent Test**: Run `npm run dev`, access http://localhost:5173, see "Welcome to SMS Admin", Material UI components render

### Frontend Project Initialization

- [ ] T064 [US3] Create React + TypeScript project with Vite in frontend/ directory: `npm create vite@latest frontend -- --template react-ts`
- [ ] T065 [US3] Install Material UI: `npm install @mui/material @emotion/react @emotion/styled`
- [ ] T066 [P] [US3] Install React Router: `npm install react-router-dom`
- [ ] T067 [P] [US3] Install React Query: `npm install @tanstack/react-query`
- [ ] T068 [P] [US3] Install Axios: `npm install axios`
- [ ] T069 [P] [US3] Install dev dependencies: `npm install -D @types/node`

### Frontend Structure

- [ ] T070 [P] [US3] Create src/components/common/ directory
- [ ] T071 [P] [US3] Create src/components/layout/ directory
- [ ] T072 [P] [US3] Create src/pages/ directory
- [ ] T073 [P] [US3] Create src/services/api/ directory
- [ ] T074 [P] [US3] Create src/services/queries/ directory
- [ ] T075 [P] [US3] Create src/utils/ directory
- [ ] T076 [P] [US3] Create src/types/ directory
- [ ] T077 [P] [US3] Create src/theme/ directory

### Frontend Configuration

- [ ] T078 [US3] Configure TypeScript path aliases in frontend/tsconfig.json
- [ ] T079 [US3] Configure Vite proxy for API in frontend/vite.config.ts
- [ ] T080 [US3] Create .env.development with VITE_API_BASE_URL=http://localhost:5000
- [ ] T081 [US3] Update .gitignore to exclude .env files
- [ ] T082 [US3] Create .prettierrc for code formatting in frontend/ directory
- [ ] T083 [US3] Add format script to frontend/package.json: `"format": "prettier --write \"src/**/*.{ts,tsx}\""`

### UI Components

- [ ] T084 [P] [US3] Create Material UI theme in frontend/src/theme/theme.ts with school color scheme
- [ ] T085 [P] [US3] Create Header component in frontend/src/components/layout/Header.tsx
- [ ] T086 [P] [US3] Create Sidebar component in frontend/src/components/layout/Sidebar.tsx
- [ ] T087 [P] [US3] Create MainLayout component in frontend/src/components/layout/MainLayout.tsx
- [ ] T088 [US3] Create Home page in frontend/src/pages/Home.tsx with "Welcome to SMS Admin"

### Routing Setup

- [ ] T089 [US3] Configure React Router in frontend/src/App.tsx with routes
- [ ] T090 [US3] Wrap app with ThemeProvider and CssBaseline in frontend/src/App.tsx
- [ ] T091 [US3] Update frontend/src/main.tsx to include QueryClientProvider for React Query

### Frontend Testing & Verification

- [ ] T092 [US3] Run `npm run dev` - verify dev server starts on port 5173
- [ ] T093 [US3] Open http://localhost:5173 - verify "Welcome to SMS Admin" displays
- [ ] T094 [US3] Verify Material UI Button component renders with proper styling
- [ ] T095 [US3] Check browser console - verify no errors
- [ ] T096 [US3] Create frontend/README.md with setup instructions and component structure

**Checkpoint**: Frontend application running, Material UI components work, routing configured

---

## Phase 6: User Story 4 - API-Frontend Integration (Priority: P2)

**Goal**: Configure communication between frontend and backend with React Query

**Independent Test**: Frontend makes API call to `/health`, response displayed, CORS working with zero errors

### API Client Setup

- [X] T097 [US4] Create Axios client configuration in frontend/src/services/api/client.ts with base URL
- [X] T098 [US4] Add request interceptor for error handling in client.ts
- [X] T099 [US4] Add response interceptor for logging in client.ts
- [X] T100 [US4] Configure timeout (10 seconds) in Axios client

### React Query Configuration

- [X] T101 [US4] Configure QueryClient with default options in frontend/src/services/queries/queryClient.ts
- [X] T102 [US4] Create useHealth query hook in frontend/src/services/queries/useHealth.ts
- [X] T103 [US4] Add TypeScript types for health check response in frontend/src/types/api.ts

### Integration Component

- [X] T104 [US4] Create HealthCheck component in frontend/src/components/common/HealthCheck.tsx
- [X] T105 [US4] Display health status with loading, error, and success states in HealthCheck component
- [X] T106 [US4] Add HealthCheck component to Home page in frontend/src/pages/Home.tsx

### CORS Verification

- [X] T107 [US4] Start both backend and frontend servers
- [X] T108 [US4] Open browser DevTools Network tab and verify API call to /health succeeds
- [X] T109 [US4] Verify no CORS errors in browser console
- [X] T110 [US4] Test error handling by stopping backend and verifying error message displays

### Integration Testing

- [X] T111 [US4] Create integration test document in specs/001-project-setup/integration-tests.md
- [X] T112 [US4] Manually verify all acceptance scenarios from spec.md User Story 4
- [X] T113 [US4] Document API integration patterns in frontend/README.md

**Checkpoint**: Frontend successfully communicates with backend, React Query working, CORS configured

---

## Phase 7: User Story 5 - Development Environment Configuration (Priority: P2)

**Goal**: Complete Docker Compose setup for full-stack development with hot reload

**Independent Test**: Run `docker-compose up`, all services start, frontend and backend both have hot reload working

### Docker Configuration Files

- [X] T114 [US5] Create Dockerfile.backend in backend/ with multi-stage build (development + production)
- [X] T115 [US5] Create Dockerfile.frontend in frontend/ with multi-stage build (development + production)
- [X] T116 [US5] Create .dockerignore in backend/ to exclude bin/, obj/, node_modules/
- [X] T117 [US5] Create .dockerignore in frontend/ to exclude node_modules/, dist/

### Docker Compose Services

- [X] T118 [US5] Add backend service to docker-compose.yml with volume mounts for hot reload
- [X] T119 [US5] Add frontend service to docker-compose.yml with volume mounts for hot reload
- [X] T120 [US5] Configure service dependencies: frontend → backend → postgres
- [X] T121 [US5] Add custom network (sms_network) to docker-compose.yml
- [X] T122 [US5] Configure health checks for all services in docker-compose.yml
- [X] T123 [US5] Add development-specific volume mounts in docker-compose.override.yml

### Environment Variables

- [X] T124 [US5] Update .env.example with all required variables (database, JWT secret placeholder, API URLs)
- [X] T125 [US5] Create .env file from .env.example with actual development values (gitignored)
- [X] T126 [US5] Configure backend to read DATABASE_CONNECTION_STRING from environment
- [X] T127 [US5] Configure frontend to read VITE_API_BASE_URL from environment
- [X] T128 [US5] Document environment variable setup in root README.md

### NPM Scripts for Development

- [X] T129 [US5] Add `dev:backend` script to root package.json (if created): `cd backend && dotnet watch run`
- [X] T130 [US5] Add `dev:frontend` script to root package.json: `cd frontend && npm run dev`
- [X] T131 [US5] Add `dev:all` script using concurrently or similar tool (optional)

### Docker Testing & Verification

- [X] T132 [US5] Run `docker-compose build` - verify all images build successfully
- [X] T133 [US5] Run `docker-compose up -d` - verify all 3 containers start
- [X] T134 [US5] Check container status: `docker-compose ps` - all should show "Up"
- [X] T135 [US5] Test hot reload: change backend file, verify automatic reload in logs
- [X] T136 [US5] Test hot reload: change frontend file, verify browser updates
- [X] T137 [US5] Test environment variable changes require container restart
- [X] T138 [US5] Run `docker-compose down -v` and verify clean shutdown

**Checkpoint**: Complete Docker Compose environment working with hot reload for development

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, final verification, and project polish

### Documentation

- [X] T139 [P] Update root README.md with quickstart guide, architecture overview, and links
- [X] T140 [P] Add CONTRIBUTING.md with development workflow and code standards
- [ ] T141 [P] Add LICENSE file (if applicable)
- [ ] T142 [P] Create architecture diagram showing Clean Architecture + CQRS structure
- [ ] T143 [P] Document Docker commands in quickstart.md

### Code Quality

- [X] T144 [P] Run `dotnet format` on backend code
- [X] T145 [P] Run `npm run format` on frontend code
- [X] T146 [P] Review all TODO comments and resolve or document
- [X] T147 [P] Verify all secrets excluded from git: check .gitignore coverage

### Final Verification

- [X] T148 Fresh clone test: Clone repository to new directory and follow README setup
- [X] T149 Verify setup time: Should complete in under 15 minutes per SC-001
- [X] T150 Verify all Success Criteria from spec.md (SC-001 through SC-008)
- [X] T151 Create verification checklist document in specs/001-project-setup/verification.md

### Deployment Preparation (Future)

- [ ] T152 Document production deployment steps in deployment.md (deferred to Phase 2)
- [ ] T153 Create CI/CD pipeline configuration (deferred to Phase 2)
- [ ] T154 Setup monitoring and logging infrastructure (deferred to Phase 2)

**Final Checkpoint**: All user stories complete, documentation finished, project ready for next feature

---

## Implementation Strategy

### MVP First (Phase 3-5)
Complete User Stories 1, 2, and 3 first. These form the MVP:
- Backend API with Clean Architecture ✅
- Database with migrations ✅
- Frontend with routing ✅

At this point you have a working full-stack application that can be demonstrated.

### Integration & Polish (Phase 6-8)
- User Story 4: Connect frontend to backend
- User Story 5: Docker Compose for easy deployment
- Phase 8: Documentation and polish

### Parallel Execution Examples

**After Phase 2 (Foundation Complete):**
```
Parallel Group 1 (All US1 [P] tasks):
├── T010-T013: Create all 4 projects simultaneously
├── T016-T018: Domain layer files
├── T019-T025: Application layer structure
└── T028-T030: Infrastructure layer structure

Parallel Group 2 (Backend API + Frontend Init):
├── T033-T042: API layer configuration (US1)
└── T064-T077: Frontend project setup (US3)
```

**User Story Independence:**
- US1 (Backend): Can be fully built and tested independently
- US2 (Database): Depends on US1 (needs DbContext)
- US3 (Frontend): Can be built in parallel with US1
- US4 (Integration): Requires US1 and US3 complete
- US5 (Docker): Requires all previous stories

---

## Task Execution Order (Critical Path)

1. **Phase 1-2**: Setup and tools (T001-T008) - Sequential
2. **Phase 3**: Backend foundation (T009-T046) - Some parallel opportunities
3. **Phase 4**: Database configuration (T047-T063) - Sequential, depends on Phase 3
4. **Phase 5**: Frontend (T064-T096) - Can run parallel with Phase 3-4
5. **Phase 6**: Integration (T097-T113) - Depends on Phase 3, 4, 5
6. **Phase 7**: Docker (T114-T138) - Depends on all previous phases
7. **Phase 8**: Polish (T139-T154) - Mostly parallel

**Estimated Total Time**: 2-3 days for one developer following this task list

---

## Notes

- **Tests**: Not included as spec did not request TDD approach. Verification via manual testing of acceptance scenarios and health checks.
- **Dependencies**: Tasks with [P] marker can run in parallel with other [P] tasks in the same phase
- **Story Labels**: [US1]-[US5] map to User Stories 1-5 from spec.md
- **Checkpoints**: Each phase ends with a checkpoint to verify progress before moving on
- **File Paths**: All tasks include specific file paths as required by task template format

**Next Steps After Tasks Complete**: 
- Proceed to authentication feature (next user story from PRD)
- Setup CI/CD pipeline
- Add automated testing infrastructure

# Feature Specification: Initial Project Setup

**Feature Branch**: `001-project-setup`  
**Created**: 2026-01-12  
**Status**: Draft  
**Input**: User description: "Initial project setup for frontend and backend"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Backend API Foundation (Priority: P1)

As a developer, I need to set up the ASP.NET Core Web API project with Clean Architecture structure so that I can build features following the constitution's architectural principles.

**Why this priority**: This is the foundation for all backend functionality. Without this, no API endpoints can be developed. Following Clean Architecture from the start prevents costly refactoring later.

**Independent Test**: Can be fully tested by running the backend project, accessing the health check endpoint, and verifying the project structure contains Core, Application, Infrastructure, and API layers with proper dependencies.

**Acceptance Scenarios**:

1. **Given** .NET 8 SDK is installed, **When** developer runs `dotnet build` in backend directory, **Then** project compiles successfully without errors
2. **Given** backend project is running, **When** developer navigates to `/health` endpoint, **Then** API returns HTTP 200 with status "Healthy"
3. **Given** solution structure is created, **When** developer examines project references, **Then** dependencies flow inward (Infrastructure → Application → Domain, API → Application)
4. **Given** developer attempts to reference Infrastructure from Domain, **When** building the solution, **Then** build fails with dependency violation error

---

### User Story 2 - Database Configuration (Priority: P1)

As a developer, I need to configure PostgreSQL database connection with Entity Framework Core so that the application can persist and retrieve data.

**Why this priority**: Core dependency for all data-driven features (students, fees, salaries). Must be set up before any business logic implementation.

**Independent Test**: Can be fully tested by running EF Core migrations, verifying database creation, and executing a test query against the database.

**Acceptance Scenarios**:

1. **Given** PostgreSQL connection string is configured, **When** developer runs `dotnet ef database update`, **Then** database is created successfully
2. **Given** database exists, **When** application starts, **Then** application connects to database without errors
3. **Given** connection string is invalid, **When** application starts, **Then** application logs clear error message and fails gracefully
4. **Given** database schema changes, **When** developer creates a new migration, **Then** migration file is generated with proper up/down methods

---

### User Story 3 - Frontend React Application (Priority: P1)

As a developer, I need to set up the React application with Vite and essential dependencies so that I can build the admin dashboard UI.

**Why this priority**: Foundation for all user interface development. Admin cannot interact with the system without the frontend.

**Independent Test**: Can be fully tested by running `npm run dev`, accessing localhost in browser, and verifying the app renders with proper routing and UI library components.

**Acceptance Scenarios**:

1. **Given** Node.js and npm are installed, **When** developer runs `npm install` in frontend directory, **Then** all dependencies install successfully
2. **Given** dependencies are installed, **When** developer runs `npm run dev`, **Then** development server starts on configured port
3. **Given** dev server is running, **When** developer opens browser to localhost, **Then** React app renders with "Welcome to SMS Admin" message
4. **Given** UI library (Material UI or Ant Design) is installed, **When** developer imports a Button component, **Then** component renders with proper styling

---

### User Story 4 - API-Frontend Integration (Priority: P2)

As a developer, I need to configure React Query and API client setup so that the frontend can communicate with the backend API.

**Why this priority**: Enables frontend-backend communication. Without this, frontend cannot fetch or submit data. Lower priority because it can be added after both projects exist independently.

**Independent Test**: Can be fully tested by creating a test API call from frontend to backend health check endpoint and verifying the response is received and displayed.

**Acceptance Scenarios**:

1. **Given** frontend and backend are running, **When** frontend makes API call to `/health`, **Then** request succeeds and response is logged
2. **Given** React Query is configured, **When** component uses `useQuery` hook, **Then** data fetching works with proper loading/error states
3. **Given** API returns error, **When** frontend handles the error, **Then** user-friendly error message is displayed
4. **Given** CORS is configured, **When** frontend on localhost:5173 calls backend on localhost:5000, **Then** request is not blocked by CORS policy

---

### User Story 5 - Development Environment Configuration (Priority: P2)

As a developer, I need environment configuration files and development scripts so that the application can run in different environments (development, staging, production).

**Why this priority**: Important for proper environment management but not blocking initial development. Can work with hardcoded values initially.

**Independent Test**: Can be fully tested by switching environment variables, running the application, and verifying it uses the correct configuration for each environment.

**Acceptance Scenarios**:

1. **Given** `.env.development` file exists, **When** application runs in development mode, **Then** it uses development database and API URLs
2. **Given** `.env.production` template exists, **When** deploying to production, **Then** sensitive values are not committed to repository
3. **Given** environment variable is missing, **When** application starts, **Then** it fails fast with clear error message indicating missing config
4. **Given** npm scripts are configured, **When** developer runs `npm run dev:all`, **Then** both frontend and backend start concurrently

---

### Edge Cases

- What happens when developer tries to run backend without .NET 8 SDK installed? → Clear error message with installation instructions
- What happens when PostgreSQL is not running? → Application startup fails with connection error and retry guidance
- What happens when frontend can't reach backend API? → React Query shows loading state, then error state with retry option
- What happens when environment variables are missing? → Application fails to start with validation error listing missing variables
- What happens when database migrations fail? → Transaction rollback occurs, database state remains unchanged, error logged

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Backend project MUST use ASP.NET Core Web API (.NET 8) following Clean Architecture with separate projects for Domain, Application, Infrastructure, and API layers
- **FR-001a**: Backend MUST implement CQRS pattern using MediatR library with separate Commands and Queries in the Application layer
- **FR-002**: Backend MUST include Entity Framework Core with PostgreSQL provider for data access
- **FR-003**: Backend MUST expose a `/health` endpoint returning HTTP 200 when application is healthy
- **FR-004**: Database connection MUST be configurable via environment variables (connection string, host, port, database name)
- **FR-005**: Backend MUST fail gracefully with clear error messages when database connection fails
- **FR-006**: Frontend project MUST use React 18+ with Vite as the build tool
- **FR-007**: Frontend MUST include either Material UI or Ant Design for UI components
- **FR-008**: Frontend MUST include React Query for API state management
- **FR-009**: Frontend MUST include React Router for navigation
- **FR-010**: Frontend MUST be configured to make API calls to backend with proper CORS handling
- **FR-011**: Both projects MUST include `.gitignore` files to exclude node_modules, build outputs, and environment files
- **FR-012**: Project MUST include README files with setup instructions for both frontend and backend
- **FR-013**: Backend MUST include Swagger/OpenAPI documentation for API endpoints
- **FR-014**: Both projects MUST use consistent code formatting (Prettier for frontend, .editorconfig for backend)
- **FR-015**: Environment configuration MUST separate development, staging, and production settings

### Key Entities *(include if feature involves data)*

**Note**: This feature focuses on infrastructure setup. Entity definitions will come in subsequent features. However, we establish the database context structure:

- **DbContext**: Central database context inheriting from EF Core's DbContext, configured with PostgreSQL provider
- **Migration**: EF Core migration infrastructure for schema versioning and updates

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developer can clone repository and run both frontend and backend locally within 15 minutes following README instructions
- **SC-002**: Backend project compiles without errors and passes architectural dependency validation (Core → Application → Infrastructure flow)
- **SC-003**: Frontend development server starts in under 5 seconds after dependencies are installed
- **SC-004**: API health check endpoint responds within 100ms in local development environment
- **SC-005**: Frontend successfully fetches data from backend health check endpoint with zero CORS errors
- **SC-006**: Database migrations execute successfully and database schema is created without manual SQL intervention
- **SC-007**: Both projects use configuration from environment variables without hardcoded connection strings or API URLs in source code
- **SC-008**: Total setup costs zero dollars (all tools and services use free tiers or open-source options)

## Assumptions

- Developer has .NET 8 SDK installed or can install it (free from Microsoft)
- Developer has Node.js 18+ and npm installed (free)
- Developer has PostgreSQL installed locally or access to free PostgreSQL instance (Supabase free tier)
- Developer uses VS Code, Visual Studio, or Rider IDE
- Developer has basic familiarity with C# and React
- Internet connection available for downloading packages
- Operating system is Windows, macOS, or Linux (cross-platform support)
- UI library choice (Material UI vs Ant Design) will be documented once selected during implementation

## Out of Scope

- User authentication implementation (separate feature)
- Business logic or domain entities (separate features)
- Automated testing setup (will be added after basic structure is working)
- CI/CD pipeline configuration (Phase 2)
- Production deployment and hosting setup (Phase 2)
- Database seeding or sample data
- Admin dashboard UI implementation (separate feature)
- Docker containerization (Phase 2)
- Logging and monitoring infrastructure (Phase 2)
- Background job processing setup (Phase 2)

## Dependencies

- .NET 8 SDK (external dependency)
- Node.js 18+ (external dependency)
- PostgreSQL 15+ (external dependency)
- Git for version control (external dependency)
- No internal feature dependencies (this is the first feature)

## Technical Notes

### Backend Project Structure
```
src/
├── SMS.Domain/          # Core business entities (no dependencies)
├── SMS.Application/     # Use cases, interfaces (depends on Domain)
├── SMS.Infrastructure/  # EF Core, external services (depends on Application)
└── SMS.API/            # Controllers, middleware (depends on Application)
```

### Frontend Project Structure
```
frontend/
├── src/
│   ├── components/     # Reusable UI components
│   ├── pages/         # Page-level components
│   ├── services/      # API client and React Query hooks
│   ├── utils/         # Helper functions
│   └── App.tsx        # Root component
├── public/            # Static assets
└── package.json
```

### Technology Choices Based on Constitution
- **Clean Architecture**: Mandated by Constitution Principle I
- **ASP.NET Core + React + PostgreSQL**: Fixed by Constitution Technology Stack section
- **Cost target < ₹1,000/month**: Driven by Constitution Principle II (Cost-Effectiveness)
- **Security-first approach**: Required by Constitution Principle III (HTTPS, no hardcoded secrets)
- **Scalability considerations**: Principle IV requires proper database indexing and stateless API design from the start

## Notes

This specification intentionally focuses only on the foundational infrastructure setup. Business features (students, fees, authentication) will be implemented as separate features following this foundation. The setup must be simple enough for a small team but robust enough to support 1,000+ students as per the PRD scalability goals.

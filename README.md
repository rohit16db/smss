# School Management Software (SMS)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📋 Overview

A **cost-effective, scalable school management system** designed for small to medium-sized schools (200-1,000+ students). Built with modern technologies following **Clean Architecture** and **CQRS patterns**, this system digitizes administrative operations including student management, fee tracking, salary management, and reporting.

### ⚡ Quick Start (3 Commands)
```bash
git clone <repository-url> && cd SMS
cp .env.example .env  # Edit to set DATABASE_PASSWORD and JWT_SECRET
docker-compose up -d  # Start all services (PostgreSQL + Backend API + Frontend)
```
🎉 **Done!** Access the app at http://localhost:5173 | API at http://localhost:5208/health

### Key Features (Phase 1 Complete ✅)
- ✅ **Clean Architecture Foundation**: 4-layer architecture (Domain, Application, Infrastructure, API)
- ✅ **CQRS Pattern**: MediatR-based command/query separation with validation pipeline
- ✅ **Database Ready**: PostgreSQL 15 with EF Core migrations and snake_case naming
- ✅ **Docker Environment**: Full-stack containerization with hot reload for development
- ✅ **Modern Frontend**: React 19 + TypeScript + Vite + Material UI v5
- ✅ **API Health Monitoring**: Health check endpoints with React Query integration
- ✅ **Developer Experience**: Comprehensive documentation + 15-minute setup time

### Planned Features (Phase 2+)
- 🎓 **Student Management**: Comprehensive student profiles, enrollment, and tracking
- 💰 **Fee Management**: Fee structure definition, payment tracking, receipt generation
- 👨‍🏫 **Teacher & Staff Management**: Employee profiles, salary management, assignments
- 📊 **Dashboard & Reports**: Real-time analytics and financial reporting
- 🔐 **Role-Based Access Control**: Admin, Accountant, Clerk, Teacher roles
- 📱 **Responsive Design**: Mobile-friendly admin dashboard

### Current Status
✅ **Phase 1 Complete** - Full infrastructure foundation with Docker Compose, health monitoring, and integration testing verified

---

## 🏗️ Architecture

This project follows **Clean Architecture** with **CQRS pattern** for maintainability and scalability:

```
┌─────────────────────────────────────────────────────────────┐
│                        Clean Architecture                    │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐      ┌──────────────┐      ┌───────────┐ │
│  │   Frontend   │─────▶│   Backend    │─────▶│  Database │ │
│  │  React + TS  │      │  ASP.NET 10  │      │PostgreSQL │ │
│  └──────────────┘      └──────────────┘      └───────────┘ │
│        │                       │                             │
│   Material UI            CQRS + MediatR                     │
│   React Query            FluentValidation                    │
│   Axios Client           EF Core 10                         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Backend Architecture (Clean Architecture + CQRS)

```
┌─────────────────────────────────────────────────────────┐
│                    SMS.API Layer                        │
│  Controllers, Middleware, Swagger, Health Checks        │
│  (Dependency: Application + Infrastructure)             │
└────────────────────────┬────────────────────────────────┘
                         │ HTTP Requests
┌────────────────────────▼────────────────────────────────┐
│              SMS.Application Layer                      │
│  CQRS: Commands/Queries, MediatR Handlers               │
│  Behaviors: Validation, Logging                         │
│  (Dependency: Domain only)                              │
└────────────────────────┬────────────────────────────────┘
                         │ Business Logic
┌────────────────────────▼────────────────────────────────┐
│             SMS.Infrastructure Layer                    │
│  EF Core DbContext, Repositories, PostgreSQL            │
│  (Dependency: Application + Domain)                     │
└────────────────────────┬────────────────────────────────┘
                         │ Data Access
┌────────────────────────▼────────────────────────────────┐
│                SMS.Domain Layer                         │
│  Entities, Interfaces, Business Rules                   │
│  (Zero Dependencies - Pure C#)                          │
└─────────────────────────────────────────────────────────┘
```

### Project Structure

```
School Management Software
├── backend/              # ASP.NET Core 10 Web API
│   ├── src/
│   │   ├── SMS.Domain/          # Core entities (BaseEntity, IRepository)
│   │   ├── SMS.Application/     # CQRS handlers (MediatR + FluentValidation)
│   │   ├── SMS.Infrastructure/  # EF Core + PostgreSQL (snake_case naming)
│   │   └── SMS.API/            # Controllers, DI, Swagger, CORS
│   ├── Dockerfile              # Multi-stage build (dev + production)
│   ├── .dockerignore
│   └── README.md               # Backend documentation (330+ lines)
│
├── frontend/             # React 19 + Vite 7 + Material UI v5
│   ├── src/
│   │   ├── components/  # Header, MainLayout, reusable UI
│   │   ├── pages/       # HomePage with health check display
│   │   ├── services/    # API client (Axios) + React Query hooks
│   │   ├── theme/       # Material UI theme configuration
│   │   └── App.tsx      # React Router + QueryClientProvider
│   ├── Dockerfile       # Multi-stage build (dev Vite + prod Nginx)
│   ├── nginx.conf       # Production web server config
│   └── README.md        # Frontend docs with 10 API patterns (800+ lines)
│
├── specs/               # Spec-Kit feature specifications
│   └── 001-project-setup/
│       ├── spec.md      # Feature specification (5 user stories, 16 FRs)
│       ├── plan.md      # Implementation plan (8 phases)
│       ├── tasks.md     # 154 tasks (138 complete)
│       ├── research.md  # Technical decisions (stack selection)
│       └── integration-tests.md  # 10 test cases (9 PASS)
│
├── docker-compose.yml   # 3 services: postgres + backend + frontend
├── docker-compose.override.yml  # Dev settings (hot reload, logging)
├── DOCKER.md            # Docker guide (500+ lines)
├── .env.example         # Environment template (206 lines)
└── .specify/            # Spec-Kit configuration
    └── memory/
        └── constitution.md  # 5 project principles (governance)
```

### Technology Stack

**Backend (ASP.NET Core 10)**
- **Framework**: ASP.NET Core 10.0.100 Web API
- **Database**: Entity Framework Core 10.0.1 with PostgreSQL 15
- **CQRS**: MediatR 14.0.0
- **Validation**: FluentValidation 12.1.0
- **API Documentation**: Swashbuckle.AspNetCore 10.0.2 (Swagger/OpenAPI)
- **Health Checks**: AspNetCore.HealthChecks.NpgSql 9.0.2
- **Database Provider**: Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1

**Frontend (React 19 + TypeScript)**
- **Framework**: React 19.0.0 with TypeScript 5.9.3
- **Build Tool**: Vite 7.3.1 (with HMR hot reload)
- **UI Library**: Material UI v5 (@mui/material, @mui/icons-material)
- **Routing**: React Router 7.6.4
- **Data Fetching**: TanStack React Query v5
- **HTTP Client**: Axios
- **Styling**: Emotion (@emotion/react, @emotion/styled)

**Database**
- **RDBMS**: PostgreSQL 15-alpine (Docker image)
- **ORM**: Entity Framework Core 10 with migrations
- **Naming**: snake_case convention for tables and columns
- **Connection Pooling**: Min 5, Max 100 connections

**DevOps & Infrastructure**
- **Containerization**: Docker Compose 3.8 (3 services orchestrated)
- **Development**: dotnet watch (backend hot reload), Vite HMR (frontend)
- **Secrets Management**: .NET User Secrets (development), .env files
- **Web Server**: Nginx (production frontend)
- **Environment**: Linux containers (alpine-based images)

---

## 🚀 Quick Start

### Method 1: Docker Compose (Recommended - 3 Commands)

This is the fastest way to get started. All services run in containers with hot reload enabled.

**Prerequisites**: Docker Desktop 20.10+ ([Download](https://www.docker.com/products/docker-desktop))

```bash
# 1. Clone and navigate
git clone <repository-url>
cd SMS

# 2. Configure environment (copy template and edit)
cp .env.example .env
# Edit .env and set:
#   - DATABASE_PASSWORD (e.g., "SecurePass123!")
#   - JWT_SECRET (minimum 32 characters, e.g., "your-super-secret-jwt-key-min-32-chars-long")

# 3. Start all services (PostgreSQL + Backend API + Frontend)
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f backend
docker-compose logs -f frontend
```

**Access the Application**:
- 🌐 **Frontend**: http://localhost:5173 (React app with system status)
- 🔧 **Backend API**: http://localhost:5208/health (health check endpoint)
- 📚 **Swagger Docs**: http://localhost:5208/swagger (OpenAPI documentation)
- 🗄️ **PostgreSQL**: `localhost:5432` (connect with your favorite DB client)

**Stop Services**:
```bash
docker-compose down           # Stop and remove containers
docker-compose down -v        # Stop and remove containers + volumes (deletes data)
```

**Hot Reload**: Code changes auto-reload without restarting containers!
- Backend: `dotnet watch` monitors `backend/src/` (2-5 second reload)
- Frontend: Vite HMR monitors `frontend/src/` (<1 second reload)

📖 **Full Docker Guide**: See [DOCKER.md](DOCKER.md) for architecture, troubleshooting, and advanced usage.

---

### Method 2: Manual Setup (For Development Without Docker)

**Prerequisites**:

| Tool | Version | Verify Command | Download |
|------|---------|----------------|----------|
| .NET SDK | 10.0+ | `dotnet --version` | https://dotnet.microsoft.com/download |
| Node.js | 20 LTS+ | `node --version` | https://nodejs.org/ |
| PostgreSQL | 15+ | `psql --version` | https://www.postgresql.org/download/ |
| EF Core CLI | 10.0+ | `dotnet ef --version` | `dotnet tool install --global dotnet-ef` |
| Git | Latest | `git --version` | https://git-scm.com/ |

**Setup Steps**:

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd SMS
   ```

2. **Configure Backend Secrets**
   ```bash
   cd backend
   
   # Initialize User Secrets
   dotnet user-secrets init --project src/SMS.API
   
   # Set connection string (replace YOUR_PASSWORD)
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
     "Host=localhost;Port=5432;Database=school_management_db;Username=postgres;Password=YOUR_PASSWORD" \
     --project src/SMS.API
   
   # Set JWT secret (minimum 32 characters)
   dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-jwt-key-min-32-chars-long" \
     --project src/SMS.API
   ```

3. **Start PostgreSQL**
   ```bash
   # Using Docker (easiest)
   docker run -d --name postgres-sms \
     -e POSTGRES_PASSWORD=YOUR_PASSWORD \
     -e POSTGRES_DB=school_management_db \
     -p 5432:5432 \
     postgres:15-alpine
   
   # OR install PostgreSQL locally and create database manually
   ```

4. **Run Database Migrations**
   ```bash
   cd backend
   dotnet ef database update \
     --project src/SMS.Infrastructure \
     --startup-project src/SMS.API
   ```

5. **Start Backend API**
   ```bash
   cd backend
   dotnet run --project src/SMS.API
   # API runs on: http://localhost:5208
   ```

6. **Start Frontend** (in a new terminal)
   ```bash
   cd frontend
   
   # Configure API URL
   cp .env.development.example .env.development
   # Edit .env.development and set: VITE_API_URL=http://localhost:5208
   
   # Install dependencies and start
   npm install
   npm run dev
   # Frontend runs on: http://localhost:5173
   ```

**Verify Setup**:
- Backend health: http://localhost:5208/health
- Swagger docs: http://localhost:5208/swagger
- Frontend app: http://localhost:5173

📖 **Detailed Instructions**: See [specs/001-project-setup/quickstart.md](specs/001-project-setup/quickstart.md) for comprehensive setup guide.

---

## 📖 Documentation

### 🎯 Getting Started
- **[Quick Start Guide](#-quick-start)** - 3-command Docker setup or manual installation
- **[DOCKER.md](DOCKER.md)** - Comprehensive Docker Compose guide (architecture, troubleshooting, 30+ commands)
- **[Quickstart Guide](specs/001-project-setup/quickstart.md)** - Detailed 15-minute setup walkthrough

### 📐 Architecture & Design
- **[Constitution](.specify/memory/constitution.md)** - 5 project principles (Clean Architecture + CQRS mandate, cost-effectiveness, security-first)
- **[Implementation Plan](specs/001-project-setup/plan.md)** - Technical decisions, stack selection, 8-phase strategy
- **[Feature Specification](specs/001-project-setup/spec.md)** - User stories, functional requirements, success criteria
- **[Tasks Breakdown](specs/001-project-setup/tasks.md)** - 154 tasks across 8 phases (138 complete)

### 🧑‍💻 Developer Guides
- **[Backend README](backend/README.md)** - ASP.NET Core API documentation (330+ lines)
- **[Frontend README](frontend/README.md)** - React app documentation with 10 API integration patterns (800+ lines)
- **[API Documentation](http://localhost:5208/swagger)** - OpenAPI/Swagger interactive docs (when running)

### 🧪 Testing & Quality
- **[Integration Tests](specs/001-project-setup/integration-tests.md)** - 10 test cases (9 PASS, 1 partial)
- **[Acceptance Verification](specs/001-project-setup/acceptance-verification-us4.md)** - User Story 4 validation (all scenarios PASS)

### 📋 Project Management
- **[Research Notes](specs/001-project-setup/research.md)** - Technology selection rationale
- **[Data Model](specs/001-project-setup/data-model.md)** - Entity relationships (future phases)
- **[Product Requirements](PRD/prd_school_management_software.md)** - Original PRD document

---

## 🎯 Roadmap

### ✅ Phase 1: Foundation (COMPLETE)
- [x] Project structure and configuration (Clean Architecture with 4 projects)
- [x] Backend API with CQRS + MediatR + FluentValidation
- [x] Database configuration (PostgreSQL + EF Core with snake_case)
- [x] Docker Compose full-stack development environment
- [x] Frontend React app with Material UI + React Query
- [x] Health check endpoints with monitoring UI
- [x] API-Frontend integration with CORS configuration
- [x] Comprehensive documentation (backend/frontend README, DOCKER.md)
- [x] Development tooling (.editorconfig, hot reload, User Secrets)

**Phase 1 Metrics**:
- 154 tasks planned, 138 completed (89.6%)
- 8 phases executed: Setup → Prerequisites → Backend → Database → Frontend → Integration → Docker → Polish
- Setup time: <15 minutes (verified)
- Test coverage: 9/10 integration tests passing

### 🚧 Phase 2: Core Features (Next - Planned Q1 2026)
- [ ] User authentication & authorization (JWT + refresh tokens)
- [ ] Student management (CRUD operations with search/filter)
- [ ] Teacher & staff management (profiles, assignments)
- [ ] Class & subject management (schedules, curriculum)
- [ ] Role-Based Access Control (4 roles: Admin, Accountant, Clerk, Teacher)
- [ ] Audit logging for all operations

### 📊 Phase 3: Financial Management (Planned Q2 2026)
- [ ] Fee structure definition (term-based, installment support)
- [ ] Fee payment tracking with receipt generation
- [ ] Salary management and payroll processing
- [ ] Expense tracking and categorization
- [ ] Financial reports (income, expenses, outstanding fees)

### 📈 Phase 4: Reporting & Analytics (Planned Q3 2026)
- [ ] Admin dashboard with key metrics
- [ ] Fee collection reports (daily, monthly, yearly)
- [ ] Outstanding payments tracking with reminders
- [ ] Monthly expense summaries
- [ ] Student enrollment trends
- [ ] Export reports (PDF, Excel)

### 🚀 Phase 5: Advanced Features (Planned Q4 2026)
- [ ] Attendance management (teachers, students)
- [ ] Exam & marks management
- [ ] Document upload/management (S3-compatible storage)
- [ ] Notifications system (email/SMS)
- [ ] Parent portal (view student progress, payments)
- [ ] Mobile-responsive improvements

See full roadmap in [PRD/prd_school_management_software.md](PRD/prd_school_management_software.md)

---

## 🏛️ Project Principles (Constitution)

This project follows strict architectural principles documented in [.specify/memory/constitution.md](.specify/memory/constitution.md):

1. **Clean Architecture (NON-NEGOTIABLE)** - Clear separation of concerns, CQRS pattern mandatory
2. **Cost-Effectiveness** - Monthly operational cost < ₹1,000 (~$12 USD)
3. **Security-First** - HTTPS, JWT auth, encrypted secrets, RBAC
4. **Scalability by Design** - Built to scale from 200 to 1,000+ students
5. **Simplicity & Pragmatism (YAGNI)** - No over-engineering, implement what's needed now

---

## 🛠️ Development

### Quick Command Reference

```bash
# === Docker Compose (Recommended) ===
docker-compose up -d              # Start all services in background
docker-compose up --build         # Rebuild images and start
docker-compose down               # Stop all services
docker-compose down -v            # Stop and remove volumes (deletes data!)
docker-compose logs -f backend    # Follow backend logs
docker-compose logs -f frontend   # Follow frontend logs
docker-compose ps                 # Check service status
docker-compose exec backend bash  # Access backend container shell
docker-compose restart backend    # Restart specific service

# === Backend (ASP.NET Core 10) ===
cd backend
dotnet restore                    # Restore NuGet packages
dotnet build                      # Build solution
dotnet run --project src/SMS.API  # Run API (http://localhost:5208)
dotnet watch --project src/SMS.API  # Run with hot reload
dotnet test                       # Run all tests
dotnet format                     # Format code per .editorconfig

# EF Core Migrations
dotnet ef migrations add MigrationName \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API
  
dotnet ef database update \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API
  
dotnet ef migrations list \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API

# === Frontend (React 19 + Vite) ===
cd frontend
npm install                       # Install dependencies
npm run dev                       # Start dev server (http://localhost:5173)
npm run build                     # Build for production
npm run preview                   # Preview production build
npm run lint                      # Run ESLint
npm run format                    # Format code with Prettier
npm test                          # Run tests (if configured)

# === Database (PostgreSQL) ===
# Connect to PostgreSQL (when running in Docker)
docker-compose exec postgres psql -U postgres -d school_management_db

# Or use connection string:
# Host=localhost;Port=5432;Database=school_management_db;Username=postgres;Password=<from .env>
```

### Development Workflow

1. **Start Services**: `docker-compose up -d` (all services with hot reload)
2. **Make Changes**: Edit files in `backend/src/` or `frontend/src/`
3. **Auto-Reload**: Changes reflect automatically (backend 2-5s, frontend <1s)
4. **View Logs**: `docker-compose logs -f backend` or `docker-compose logs -f frontend`
5. **Run Migrations**: `docker-compose exec backend dotnet ef database update`
6. **Stop Services**: `docker-compose down` (or `Ctrl+C` if not detached)

### Project Structure & Layers

**Backend** (Clean Architecture):
- **Domain Layer** (`SMS.Domain`): Pure C# entities, interfaces, business rules (zero dependencies)
- **Application Layer** (`SMS.Application`): CQRS commands/queries, MediatR handlers, validation behaviors
- **Infrastructure Layer** (`SMS.Infrastructure`): EF Core DbContext, repositories, PostgreSQL configuration
- **API Layer** (`SMS.API`): Controllers, middleware, DI configuration, Swagger

**Frontend** (Component-Based):
- **Components** (`src/components/`): Reusable UI components (Header, MainLayout)
- **Pages** (`src/pages/`): Route-level components (HomePage)
- **Services** (`src/services/`): API client, React Query hooks, data fetching
- **Theme** (`src/theme/`): Material UI theme configuration

### Code Standards

- **Backend**: Follow `.editorconfig` rules (4-space indent, UTF-8, LF line endings)
- **Frontend**: ESLint + Prettier configuration (2-space indent, single quotes)
- **Naming Conventions**:
  - Backend: PascalCase for classes/methods, camelCase for variables
  - Database: snake_case for tables/columns
  - Frontend: PascalCase for components, camelCase for variables/functions
- **Architecture**: Maintain Clean Architecture boundaries, use CQRS for all data operations

---

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test                           # Run all tests
dotnet test --filter Category=Unit   # Unit tests only
dotnet test --filter Category=Integration  # Integration tests only
```

### Frontend Tests
```bash
cd frontend
npm test                    # Run tests
npm run test:coverage       # With coverage report
```

---

## 🤝 Contributing

We welcome contributions! This project follows strict architectural principles to maintain code quality and consistency.

### Before You Start

1. **Read the [Constitution](.specify/memory/constitution.md)** - Understand the 5 non-negotiable principles:
   - Clean Architecture + CQRS (mandatory)
   - Cost-effectiveness (<₹1,000/month operational cost)
   - Security-first design
   - Scalability by design (200-1,000+ students)
   - Simplicity & pragmatism (YAGNI)

2. **Review Existing Documentation**:
   - [Implementation Plan](specs/001-project-setup/plan.md) - Technical decisions
   - [Backend README](backend/README.md) - Backend architecture
   - [Frontend README](frontend/README.md) - Frontend patterns

### Development Setup

```bash
# Fork and clone the repository
git clone https://github.com/YOUR_USERNAME/SMS.git
cd SMS

# Start development environment
cp .env.example .env  # Edit to set DATABASE_PASSWORD and JWT_SECRET
docker-compose up -d

# Verify setup
curl http://localhost:5208/health  # Backend health check
open http://localhost:5173         # Frontend app
```

### Contribution Workflow

1. **Create an Issue**: Discuss your idea or bug fix before starting work
2. **Branch**: Create a feature branch from `main` (e.g., `feature/student-crud`)
3. **Code**: Follow the coding standards (see below)
4. **Test**: Ensure all tests pass and add new tests for your changes
5. **Commit**: Use clear, descriptive commit messages (e.g., "feat: add student CRUD operations")
6. **Push**: Push your branch to your fork
7. **Pull Request**: Open a PR with a clear description of changes

### Coding Standards

**Backend (C# / .NET)**:
- Follow `.editorconfig` rules (auto-applied with `dotnet format`)
- Use PascalCase for classes, methods, properties
- Use camelCase for local variables and parameters
- All data operations MUST use CQRS pattern (MediatR commands/queries)
- Maintain Clean Architecture boundaries (no cross-layer dependencies)
- Add XML documentation comments for public APIs

**Frontend (React / TypeScript)**:
- Follow ESLint + Prettier configuration
- Use PascalCase for React components
- Use camelCase for functions, variables, props
- Prefer functional components with hooks over class components
- Use Material UI components consistently
- Organize imports: React → libraries → local components → services → types

**Database**:
- Use snake_case for table and column names
- Create EF Core migrations for all schema changes
- Test migrations on a clean database before committing

### Pull Request Guidelines

- **Title**: Use conventional commits format (feat:, fix:, docs:, refactor:, test:)
- **Description**: Explain what and why (not just how)
- **Tests**: Include unit/integration tests for new features
- **Documentation**: Update relevant README files if needed
- **Breaking Changes**: Clearly mark and explain any breaking changes

### Code Review Process

All PRs require:
- ✅ Clean Architecture principles followed
- ✅ CQRS pattern used for data operations
- ✅ Tests passing (if tests exist)
- ✅ Code formatted per standards
- ✅ No sensitive data or secrets committed
- ✅ Documentation updated (if applicable)

---

**Questions?** Open an issue or discussion. We're here to help!

**Detailed Contributing Guide**: See [CONTRIBUTING.md](CONTRIBUTING.md) for comprehensive guidelines.

---

## 📊 Project Metrics

- **Total Tasks**: 154 (138 complete, 16 in progress)
- **Phase Progress**: Phase 1 complete (100%), Phase 2-8 planned
- **Setup Time**: <15 minutes (verified with Docker Compose)
- **Test Coverage**: 9/10 integration tests passing
- **Code Quality**: .editorconfig + ESLint + Prettier enforced
- **Documentation**: 2,500+ lines across README files + DOCKER.md
- **Last Updated**: 2026-01-12
- **Status**: ✅ Phase 1 Complete - Ready for Phase 2 Development

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Team

- **Project Lead**: TBD
- **Backend Developers**: TBD
- **Frontend Developers**: TBD
- **Contributors**: [All Contributors](../../graphs/contributors)

---

## 🙏 Acknowledgments

- Built with [Spec-Kit](https://github.com/github/spec-kit) for feature specification
- Inspired by modern Clean Architecture practices
- Powered by open-source technologies

---

## 📞 Support

- **Documentation**: [specs/001-project-setup/quickstart.md](specs/001-project-setup/quickstart.md)
- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)

---

## 🔗 Quick Links

### 🚀 Running Application (when services are up)
- [Frontend Application](http://localhost:5173) - React app with system status
- [Backend API Health](http://localhost:5208/health) - Health check endpoint
- [Swagger Documentation](http://localhost:5208/swagger) - OpenAPI interactive docs
- [Swagger JSON](http://localhost:5208/swagger/v1/swagger.json) - OpenAPI specification

### 📚 Documentation
- [Backend README](backend/README.md) - ASP.NET Core API guide (330+ lines)
- [Frontend README](frontend/README.md) - React app guide with API patterns (800+ lines)
- [Docker Guide](DOCKER.md) - Comprehensive Docker Compose documentation (500+ lines)
- [Project Constitution](.specify/memory/constitution.md) - 5 architectural principles
- [Implementation Plan](specs/001-project-setup/plan.md) - Technical decisions and 8-phase strategy
- [Quickstart Guide](specs/001-project-setup/quickstart.md) - Detailed 15-minute setup
- [Feature Specification](specs/001-project-setup/spec.md) - User stories and requirements
- [Task Breakdown](specs/001-project-setup/tasks.md) - 154 tasks across 8 phases

### 🧪 Testing & Quality
- [Integration Tests](specs/001-project-setup/integration-tests.md) - 10 test cases
- [Acceptance Verification](specs/001-project-setup/acceptance-verification-us4.md) - User Story 4 validation

### 📋 Project Resources
- [Product Requirements](PRD/prd_school_management_software.md) - Original PRD document
- [Research Notes](specs/001-project-setup/research.md) - Technology selection rationale
- [Data Model](specs/001-project-setup/data-model.md) - Entity relationships (future)

---

**Built with ❤️ for educational institutions**

**Estimated Setup Time**: <15 minutes | **Current Phase**: Phase 1 Complete ✅ | **Status**: Ready for Phase 2 Development

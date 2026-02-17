# Contributing to School Management Software (SMS)

Thank you for your interest in contributing to the School Management Software project! This document provides guidelines and instructions for contributing effectively.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Before You Start](#before-you-start)
3. [Development Setup](#development-setup)
4. [How to Contribute](#how-to-contribute)
5. [Coding Standards](#coding-standards)
6. [Architecture Guidelines](#architecture-guidelines)
7. [Git Workflow](#git-workflow)
8. [Pull Request Process](#pull-request-process)
9. [Testing Requirements](#testing-requirements)
10. [Documentation Guidelines](#documentation-guidelines)

---

## Code of Conduct

This project follows a professional code of conduct. We expect all contributors to:
- Be respectful and inclusive
- Focus on constructive feedback
- Accept criticism gracefully
- Prioritize the project's goals and quality
- Follow the project's architectural principles

---

## Before You Start

### 1. Understand the Project Principles

Read the **[Constitution](.specify/memory/constitution.md)** first. This project follows 5 **non-negotiable principles**:

1. **Clean Architecture + CQRS (MANDATORY)**
   - 4-layer architecture: Domain → Application → Infrastructure → API
   - All data operations MUST use CQRS pattern with MediatR
   - No cross-layer dependencies (Domain has zero dependencies)
   - Validation using FluentValidation in Application layer

2. **Cost-Effectiveness**
   - Monthly operational cost must remain < ₹1,000 (~$12 USD)
   - Use cost-effective cloud resources (e.g., free tier PostgreSQL)
   - Optimize database queries and API calls

3. **Security-First**
   - HTTPS everywhere in production
   - JWT authentication + refresh tokens
   - Encrypted secrets (never commit .env files)
   - Role-Based Access Control (RBAC)
   - Input validation on all endpoints

4. **Scalability by Design**
   - Built to scale from 200 to 1,000+ students
   - Efficient database indexing
   - Connection pooling (PostgreSQL 5-100 connections)
   - Stateless API design

5. **Simplicity & Pragmatism (YAGNI)**
   - Don't over-engineer
   - Implement what's needed now, not what might be needed later
   - Prefer simple solutions over complex abstractions

### 2. Review Existing Documentation

- **[README.md](README.md)** - Project overview and quick start
- **[Implementation Plan](specs/001-project-setup/plan.md)** - Technical decisions and architecture
- **[Backend README](backend/README.md)** - Backend architecture and patterns
- **[Frontend README](frontend/README.md)** - Frontend architecture with 10 API patterns
- **[DOCKER.md](DOCKER.md)** - Docker Compose development guide

### 3. Check Existing Issues

Before starting work:
- Browse [open issues](../../issues) to see if your idea already exists
- Check [pull requests](../../pulls) to avoid duplicate work
- For new features, create an issue first to discuss with maintainers

---

## Development Setup

### Prerequisites

| Tool | Version | Install Command | Verify |
|------|---------|-----------------|--------|
| .NET SDK | 10.0+ | [Download](https://dotnet.microsoft.com/download) | `dotnet --version` |
| Node.js | 20 LTS+ | [Download](https://nodejs.org/) | `node --version` |
| Docker Desktop | Latest | [Download](https://www.docker.com/products/docker-desktop) | `docker --version` |
| EF Core CLI | 10.0+ | `dotnet tool install --global dotnet-ef` | `dotnet ef --version` |
| Git | Latest | [Download](https://git-scm.com/) | `git --version` |

### Quick Setup (Docker Compose)

```bash
# 1. Fork the repository on GitHub
# 2. Clone your fork
git clone https://github.com/YOUR_USERNAME/SMS.git
cd SMS

# 3. Configure environment
cp .env.example .env
# Edit .env and set:
#   - DATABASE_PASSWORD (e.g., "SecurePass123!")
#   - JWT_SECRET (minimum 32 characters)

# 4. Start all services
docker-compose up -d

# 5. Verify setup
docker-compose ps                    # All services should be "Up"
curl http://localhost:5208/health    # Should return {"status":"Healthy"}
open http://localhost:5173           # Frontend should display system status

# 6. View logs
docker-compose logs -f backend
docker-compose logs -f frontend
```

### Manual Setup (Without Docker)

See detailed instructions in [README.md - Method 2: Manual Setup](README.md#method-2-manual-setup-for-development-without-docker).

---

## How to Contribute

### Types of Contributions

We welcome the following contributions:

1. **Bug Fixes**: Fix issues reported in GitHub Issues
2. **New Features**: Implement features from the roadmap (discuss first!)
3. **Documentation**: Improve README files, code comments, or guides
4. **Tests**: Add unit tests, integration tests, or improve coverage
5. **Performance**: Optimize queries, reduce bundle size, improve load times
6. **Refactoring**: Improve code quality while maintaining behavior

### Contribution Workflow

1. **Create/Find an Issue**
   - For bugs: Describe the issue with steps to reproduce
   - For features: Propose the feature and wait for approval from maintainers
   - For documentation: Describe what you want to improve

2. **Fork and Branch**
   ```bash
   # Fork the repo on GitHub, then:
   git clone https://github.com/YOUR_USERNAME/SMS.git
   cd SMS
   git checkout -b feature/your-feature-name
   # Or: git checkout -b fix/bug-description
   ```

3. **Make Changes**
   - Write code following the [Coding Standards](#coding-standards)
   - Follow [Architecture Guidelines](#architecture-guidelines)
   - Add tests for new functionality
   - Update documentation if needed

4. **Test Your Changes**
   ```bash
   # Backend
   cd backend
   dotnet build
   dotnet test
   dotnet format --verify-no-changes  # Verify formatting
   
   # Frontend
   cd frontend
   npm run lint
   npm run format
   npm run build  # Ensure production build works
   ```

5. **Commit**
   ```bash
   git add .
   git commit -m "feat: add student CRUD operations"
   # Use conventional commits format (see below)
   ```

6. **Push and Create PR**
   ```bash
   git push origin feature/your-feature-name
   # Then create a Pull Request on GitHub
   ```

---

## Coding Standards

### Backend (C# / ASP.NET Core)

#### Formatting
- Use `.editorconfig` rules (auto-applied with `dotnet format`)
- **Indentation**: 4 spaces (not tabs)
- **Line Endings**: LF (Unix-style)
- **Encoding**: UTF-8
- **Line Width**: 120 characters max (guideline, not strict)

#### Naming Conventions
```csharp
// Classes, Methods, Properties: PascalCase
public class StudentService { }
public void CreateStudent() { }
public string FirstName { get; set; }

// Interfaces: IPascalCase (with I prefix)
public interface IStudentRepository { }

// Local variables, parameters: camelCase
var studentId = Guid.NewGuid();
public void UpdateStudent(Guid studentId, string firstName) { }

// Private fields: _camelCase (with underscore prefix)
private readonly ILogger _logger;

// Constants: PascalCase
public const int MaxStudents = 1000;
```

#### Architecture Rules
```csharp
// ✅ CORRECT: Use CQRS pattern with MediatR
public class CreateStudentCommand : IRequest<StudentDto>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
{
    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
    }
}

// ❌ INCORRECT: Don't put business logic in controllers
public class StudentController : ControllerBase
{
    public async Task<IActionResult> CreateStudent(CreateStudentRequest request)
    {
        // ❌ NO business logic here - use MediatR!
        var student = new Student { FirstName = request.FirstName };
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync();
        return Ok(student);
    }
}
```

#### Code Documentation
```csharp
/// <summary>
/// Creates a new student in the system.
/// </summary>
/// <param name="command">The student creation command containing required details.</param>
/// <returns>The created student DTO.</returns>
/// <exception cref="ValidationException">Thrown when validation fails.</exception>
public async Task<StudentDto> Handle(CreateStudentCommand command, CancellationToken cancellationToken)
{
    // Implementation
}
```

### Frontend (React / TypeScript)

#### Formatting
- Use ESLint + Prettier configuration (auto-applied with `npm run format`)
- **Indentation**: 2 spaces
- **Quotes**: Single quotes for strings
- **Semicolons**: Required
- **Line Width**: 100 characters

#### Naming Conventions
```typescript
// React Components: PascalCase
const StudentList = () => { ... };
export default StudentList;

// Functions, Variables: camelCase
const fetchStudents = async () => { ... };
const studentId = '123';

// Constants: UPPER_SNAKE_CASE
const API_BASE_URL = 'http://localhost:5208';

// Interfaces/Types: PascalCase (with I prefix optional)
interface StudentProps {
  id: string;
  name: string;
}

// Files:
// - Components: PascalCase (StudentList.tsx)
// - Utilities/Hooks: camelCase (useStudents.ts, apiClient.ts)
```

#### Component Structure
```typescript
// Imports: React → libraries → local components → services → types
import React, { useState, useEffect } from 'react';
import { Box, Typography } from '@mui/material';
import { StudentCard } from '../components/StudentCard';
import { useStudents } from '../services/queries/useStudents';
import type { Student } from '../types/student';

// Props interface
interface StudentListProps {
  classId: string;
}

// Component
export const StudentList: React.FC<StudentListProps> = ({ classId }) => {
  const { data: students, isLoading, error } = useStudents(classId);

  if (isLoading) return <Typography>Loading...</Typography>;
  if (error) return <Typography color="error">Error loading students</Typography>;

  return (
    <Box>
      {students?.map(student => (
        <StudentCard key={student.id} student={student} />
      ))}
    </Box>
  );
};
```

### Database

#### Naming Conventions
- **Tables**: `snake_case`, plural (e.g., `students`, `fee_payments`)
- **Columns**: `snake_case` (e.g., `first_name`, `created_at`)
- **Primary Keys**: `id` (Guid/UUID)
- **Foreign Keys**: `{table}_id` (e.g., `student_id`, `class_id`)

#### Migrations
```csharp
// Always use EF Core migrations for schema changes
dotnet ef migrations add AddStudentTable \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API

// Review the generated migration before applying
dotnet ef database update \
  --project src/SMS.Infrastructure \
  --startup-project src/SMS.API
```

---

## Architecture Guidelines

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────┐
│                      SMS.API                            │
│  - Controllers (thin, delegate to MediatR)              │
│  - Middleware (exception handling, auth)                │
│  - DI configuration (Program.cs)                        │
│  Dependencies: Application + Infrastructure             │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│                  SMS.Application                        │
│  - CQRS Commands & Queries (IRequest)                   │
│  - Command/Query Handlers (IRequestHandler)             │
│  - Behaviors (Validation, Logging)                      │
│  - DTOs (Data Transfer Objects)                         │
│  Dependencies: Domain ONLY                              │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│                SMS.Infrastructure                       │
│  - ApplicationDbContext (EF Core)                       │
│  - Repositories (implement Domain interfaces)           │
│  - External service integrations                        │
│  Dependencies: Application + Domain                     │
└──────────────────────┬──────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────┐
│                   SMS.Domain                            │
│  - Entities (BaseEntity, Student, etc.)                 │
│  - Interfaces (IRepository, etc.)                       │
│  - Business Rules & Exceptions                          │
│  Dependencies: NONE (pure C#)                           │
└─────────────────────────────────────────────────────────┘
```

### CQRS Pattern (MANDATORY)

**Commands** (Create, Update, Delete):
```csharp
// Command
public class CreateStudentCommand : IRequest<StudentDto>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

// Validator
public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow);
    }
}

// Handler
public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IApplicationDbContext _context;

    public CreateStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync(cancellationToken);

        return new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth
        };
    }
}
```

**Queries** (Read operations):
```csharp
// Query
public class GetStudentByIdQuery : IRequest<StudentDto>
{
    public Guid StudentId { get; set; }
}

// Handler
public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    private readonly IApplicationDbContext _context;

    public GetStudentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

        if (student == null)
            throw new EntityNotFoundException(nameof(Student), request.StudentId);

        return new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth
        };
    }
}
```

### Frontend Architecture

**API Client Pattern**:
```typescript
// services/api/studentApi.ts
export const studentApi = {
  getAll: () => apiClient.get<Student[]>('/students'),
  getById: (id: string) => apiClient.get<Student>(`/students/${id}`),
  create: (data: CreateStudentRequest) => apiClient.post<Student>('/students', data),
  update: (id: string, data: UpdateStudentRequest) => apiClient.put<Student>(`/students/${id}`, data),
  delete: (id: string) => apiClient.delete(`/students/${id}`),
};
```

**React Query Hook Pattern**:
```typescript
// services/queries/useStudents.ts
export const useStudents = () => {
  return useQuery({
    queryKey: ['students'],
    queryFn: async () => {
      const response = await studentApi.getAll();
      return response.data;
    },
    staleTime: 30000, // 30 seconds
  });
};

export const useCreateStudent = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateStudentRequest) => studentApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
    },
  });
};
```

---

## Git Workflow

### Branch Naming

```bash
# Feature branches
feature/student-crud
feature/fee-payment-tracking

# Bug fix branches
fix/login-validation-error
fix/database-connection-timeout

# Documentation branches
docs/update-readme
docs/add-api-examples

# Refactoring branches
refactor/optimize-student-query
refactor/extract-validation-service
```

### Commit Message Format

Use **Conventional Commits** format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code refactoring (no functional changes)
- `test`: Add or update tests
- `chore`: Build process, dependencies, tooling
- `perf`: Performance improvements
- `style`: Code style changes (formatting, whitespace)

**Examples**:
```bash
# Simple commit
git commit -m "feat: add student CRUD operations"

# With scope
git commit -m "fix(auth): resolve JWT token expiration issue"

# With body
git commit -m "refactor: extract validation logic to separate service

Move all validation logic from controllers to dedicated validation
service to improve testability and reusability."

# Breaking change
git commit -m "feat: migrate to .NET 10

BREAKING CHANGE: Requires .NET 10 SDK. Update your development
environment before pulling this change."
```

### Pull Request Preparation

Before creating a PR:

```bash
# 1. Update your branch with latest main
git checkout main
git pull origin main
git checkout your-feature-branch
git rebase main  # Or: git merge main

# 2. Run all checks
cd backend
dotnet build
dotnet test
dotnet format --verify-no-changes

cd ../frontend
npm run lint
npm run format
npm run build

# 3. Push your branch
git push origin your-feature-branch
```

---

## Pull Request Process

### PR Checklist

Before submitting, ensure:

- [ ] Code follows [Coding Standards](#coding-standards)
- [ ] Clean Architecture + CQRS principles maintained
- [ ] All tests pass (`dotnet test`, `npm test` if applicable)
- [ ] Code formatted (`dotnet format`, `npm run format`)
- [ ] No new linting errors (`npm run lint`)
- [ ] Documentation updated (if applicable)
- [ ] No secrets or sensitive data committed (check .env, appsettings.*.json)
- [ ] PR title follows conventional commits format
- [ ] PR description explains what and why (not just how)

### PR Template

```markdown
## Description
Brief description of what this PR does.

## Type of Change
- [ ] Bug fix (non-breaking change which fixes an issue)
- [ ] New feature (non-breaking change which adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Related Issue
Closes #123 (replace with actual issue number)

## How Has This Been Tested?
Describe the tests you ran to verify your changes.

## Checklist
- [ ] Code follows project coding standards
- [ ] Clean Architecture + CQRS principles maintained
- [ ] All tests pass
- [ ] Code formatted and linted
- [ ] Documentation updated
- [ ] No secrets committed

## Screenshots (if applicable)
Add screenshots for UI changes.
```

### Review Process

All PRs require:

1. **Automated Checks** (CI/CD - when configured):
   - Backend build passes
   - Frontend build passes
   - Tests pass
   - Linting passes

2. **Code Review** (by maintainers):
   - Clean Architecture principles followed
   - CQRS pattern used correctly
   - Code quality and readability
   - Test coverage adequate
   - Documentation sufficient

3. **Approval**: At least one maintainer must approve

4. **Merge**: Maintainers will merge using "Squash and Merge" strategy

---

## Testing Requirements

### Backend Testing

#### Unit Tests
Test individual components in isolation:

```csharp
// Example: Testing a command handler
public class CreateStudentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesStudent()
    {
        // Arrange
        var context = new Mock<IApplicationDbContext>();
        var handler = new CreateStudentCommandHandler(context.Object);
        var command = new CreateStudentCommand
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(2005, 1, 1)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

#### Integration Tests
Test multiple components together:

```csharp
public class StudentIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public StudentIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateStudent_ReturnsCreatedStudent()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            DateOfBirth = new DateTime(2006, 5, 15)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/students", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        Assert.Equal("Jane", student.FirstName);
    }
}
```

### Frontend Testing

#### Component Tests (with React Testing Library)

```typescript
import { render, screen } from '@testing-library/react';
import { StudentList } from './StudentList';

test('renders student list', () => {
  const students = [
    { id: '1', firstName: 'John', lastName: 'Doe' },
    { id: '2', firstName: 'Jane', lastName: 'Smith' },
  ];

  render(<StudentList students={students} />);

  expect(screen.getByText('John Doe')).toBeInTheDocument();
  expect(screen.getByText('Jane Smith')).toBeInTheDocument();
});
```

### Test Coverage Goals

- **Backend**: Aim for 80%+ coverage on Application and Domain layers
- **Frontend**: Aim for 70%+ coverage on components and hooks
- **Critical Paths**: 100% coverage (authentication, payment processing)

---

## Documentation Guidelines

### Code Comments

**When to comment**:
- Complex algorithms or business logic
- Non-obvious workarounds or hacks
- Public APIs and interfaces
- "Why" not "what" (code should be self-explanatory)

**When NOT to comment**:
- Obvious code (e.g., `// Set first name`)
- Redundant documentation (let TypeScript/C# types speak)

### README Updates

Update relevant README files when:
- Adding new features
- Changing setup process
- Introducing new dependencies
- Modifying architecture

### API Documentation

- Keep Swagger/OpenAPI docs up to date
- Add XML comments to controllers and DTOs
- Document request/response examples

---

## Questions or Issues?

- **Questions**: Open a [GitHub Discussion](../../discussions)
- **Bugs**: Open a [GitHub Issue](../../issues)
- **Security**: Email maintainers directly (DO NOT open public issue)

---

Thank you for contributing to School Management Software! 🎓

Your efforts help schools digitize their operations and improve efficiency. Every contribution matters!

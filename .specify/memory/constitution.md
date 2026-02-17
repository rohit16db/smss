<!--
Sync Impact Report:
Version: 0.0.0 → 1.0.0
- Initial constitution created based on PRD for School Management Software
- Principles established: Clean Architecture, Cost-Effectiveness, Security-First, Scalability, Simplicity
- Technology Stack section defined with mandatory technologies
- Security & Compliance requirements established
- Development Workflow and Governance sections completed

Modified Principles:
- N/A (initial creation)

Added Sections:
- Core Principles (5 principles)
- Technology Stack (FIXED)
- Security & Compliance Requirements
- Development Workflow
- Governance

Removed Sections:
- N/A (initial creation)

Templates Status:
- ✅ plan-template.md: Verified - "Constitution Check" section aligns with new principles
- ✅ spec-template.md: Verified - User story prioritization aligns with Simplicity principle
- ✅ tasks-template.md: Verified - Task organization aligns with workflow requirements
- ✅ checklist-template.md: No updates required
- ✅ agent-file-template.md: No updates required

Follow-up TODOs:
- None - all placeholders filled with concrete values derived from PRD
- UI Library choice (Material UI vs Ant Design) to be documented when selected
- CI/CD pipeline setup to be configured in accordance with Development Workflow section
-->

# School Management Software (SMS) Constitution

## Core Principles

### I. Clean Architecture (NON-NEGOTIABLE)
**Principle**: All backend code MUST follow Clean Architecture / Modular Monolith patterns with clear separation of concerns.

**Rules**:
- Domain entities remain independent of infrastructure
- Business logic resides in the Core layer, never in Controllers or UI
- Dependencies flow inward: Infrastructure → Application → Domain
- Each module is independently testable
- Entity Framework Core used exclusively for data access with proper repository patterns
- CQRS (Command Query Responsibility Segregation) pattern MUST be used for separating read and write operations

**Rationale**: Ensures maintainability, testability, and supports the project's goal to scale from 200 to 1,000+ students without architectural rewrites.

### II. Cost-Effectiveness
**Principle**: Every architectural decision MUST prioritize minimizing monthly operational costs while maintaining quality and scalability.

**Rules**:
- Target monthly hosting cost < ₹1,000 (~$12 USD) for Phase 1
- Use free tiers and cost-effective managed services (Render, Supabase, Netlify/Vercel)
- Implement caching and optimization only when proven necessary
- Prefer vertical scaling initially over complex distributed systems
- Monitor resource usage to prevent cost overruns

**Rationale**: Small school budget constraints require cost-conscious engineering without compromising on professional quality or future growth potential.

### III. Security-First
**Principle**: Security is mandatory at every layer, not optional or deferred.

**Rules**:
- HTTPS enforced for all environments (dev, staging, production)
- JWT-based authentication with secure token management
- All passwords MUST be hashed using industry-standard algorithms (bcrypt/PBKDF2)
- SQL injection prevention via parameterized queries (EF Core enforced)
- Role-based access control architecture prepared from Phase 1 (even if single role initially)
- Sensitive data (PII, financial records) encrypted at rest
- Input validation on both client and server sides
- Regular dependency updates for security patches

**Rationale**: Handles sensitive student data, financial transactions, and personal information; security breaches are unacceptable and could destroy trust.

### IV. Scalability by Design
**Principle**: The system MUST be designed to scale from 200 to 1,000+ students without requiring a complete rewrite.

**Rules**:
- Database schema designed for growth (proper indexing, normalization)
- Stateless API design to support horizontal scaling later
- Pagination mandatory for all list endpoints from day one
- Performance benchmarks: API responses < 500ms for 95th percentile
- Background job infrastructure prepared (even if not used immediately)
- Modular architecture allows adding features without refactoring core

**Rationale**: Rewrites are expensive and risky; building scalability into the foundation prevents technical debt and enables business growth.

### V. Simplicity & Pragmatism (YAGNI)
**Principle**: Implement only what is needed now; avoid over-engineering.

**Rules**:
- No microservices in Phase 1 (monolith is appropriate for scale)
- No complex caching until performance testing proves necessity
- No premature abstraction—wait for patterns to emerge
- Feature flags and configuration over code changes
- Direct implementation over elaborate frameworks when complexity is unjustified
- Documentation must be clear and practical, not exhaustive

**Rationale**: Small team, limited resources; complexity is the enemy. Focus on delivering working features that solve real problems rather than theoretical perfection.

## Technology Stack (FIXED)

**These technologies are non-negotiable for consistency and maintainability:**

### Backend
- **Framework**: ASP.NET Core Web API (.NET 8)
- **Architecture**: Clean Architecture / Modular Monolith
- **ORM**: Entity Framework Core with Migrations
- **Authentication**: JWT-based
- **Language**: C# 12+

### Frontend
- **Framework**: React 18+
- **Build Tool**: Vite
- **UI Library**: Material UI or Ant Design (choose one, document choice)
- **Data Fetching**: React Query
- **State Management**: Context API / Zustand (for simple state)

### Database
- **Primary Database**: PostgreSQL 15+
- **Migration Tool**: EF Core Migrations
- **Connection**: Managed via connection pooling

### Hosting (Initial Phase)
- **Backend**: Render / Railway / DigitalOcean App Platform
- **Database**: Supabase Postgres / Neon / Managed PostgreSQL
- **Frontend**: Netlify / Vercel (static hosting)
- **CI/CD**: GitHub Actions (preferred for automation)

**Rationale**: These technologies are mature, cost-effective, well-documented, and provide clear migration paths as the system scales.

## Security & Compliance Requirements

### Data Protection
- Student and teacher personally identifiable information (PII) treated as sensitive
- Financial records (fees, salaries) require audit trails (who, when, what changed)
- Automated daily database backups with 30-day retention minimum
- Backup restoration tested quarterly

### Access Control
- Admin-only access in Phase 1
- Role-based access control (RBAC) architecture prepared for future roles: Accountant, Clerk, Teacher, Parent
- Session timeouts: 8 hours for admin, configurable per role
- Failed login attempt tracking (lockout after 5 failures)

### Audit & Monitoring
- All financial transactions logged (fee payments, salary disbursements)
- Error logging with severity levels (Critical, Error, Warning, Info)
- Monitoring dashboard for system health (uptime, error rates)
- Monthly security review of dependencies and access logs

## Development Workflow

### Code Quality Gates
- All code changes via pull requests (no direct commits to main)
- Unit tests required for business logic (Core layer)
- Integration tests required for API endpoints with database interactions
- Test coverage target: >70% for Core and Application layers
- Code reviews by at least one team member before merge

### Migration & Deployment
- Database migrations tested in staging before production
- Blue-green deployment strategy for zero-downtime updates (Phase 2+)
- Rollback plan documented for each deployment
- Feature toggles for risky changes

### Documentation Requirements
- API endpoints documented (OpenAPI/Swagger)
- README maintained with setup instructions
- Architecture Decision Records (ADRs) for major technical choices
- User guides for admin features

## Governance

### Amendment Process
This constitution supersedes all other technical practices and coding preferences. Amendments require:
1. Documented rationale for the proposed change
2. Impact analysis on existing codebase and architecture
3. Approval from project owner/lead architect
4. Migration plan if changes affect existing code
5. Version bump according to semantic versioning rules

### Versioning Policy
Constitution versions follow MAJOR.MINOR.PATCH:
- **MAJOR**: Backward-incompatible principle removal or redefinition
- **MINOR**: New principle added or material expansion
- **PATCH**: Clarifications, wording fixes, non-semantic changes

### Compliance Verification
- All pull requests MUST include a compliance checklist confirming adherence to applicable principles
- Architecture reviews conducted at phase boundaries
- Technical debt tracked and prioritized quarterly

### Runtime Guidance
For day-to-day development decisions and practical implementation guidance, developers should consult additional documentation in the `.specify/` directory and project wiki.

**Version**: 1.0.0 | **Ratified**: 2026-01-12 | **Last Amended**: 2026-01-12

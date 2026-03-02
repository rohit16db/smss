# Implementation Plan: Exam & Marks Management

**Feature**: 004-exam-management  
**Phase**: Phase 2 (Post MVP)  
**Timeline**: 2-3 weeks  
**Team Size**: 1-2 developers

---

## 📋 Executive Summary

The Exam & Marks Management module enables schools to:
- Create and manage examinations across subjects and classes
- Record student marks efficiently with validation
- Auto-generate report cards with grades and class rankings
- Analyze academic performance and trends
- Export report cards and analytics as PDF/Excel

**Scope**: 65+ implementation tasks organized in 3 development phases  
**Deliverables**: 15+ new pages/components, 25+ API endpoints, 8+ database tables  

---

## 🗓️ Development Timeline

### Week 1-2: Core Features (Phase 1) ✅ PRIORITY
**Tasks**: T001-T060 (60 tasks)  
**Features**: Exam CRUD, Marks Entry, Report Cards, Basic Testing

**Breakdown**:
- **Days 1-2**: Database schema and migrations (T001-T005)
- **Days 2-3**: Domain models and repositories (T006-T015)
- **Days 3-4**: CQRS commands and queries (T016-T030)
- **Days 4-5**: DTOs and API endpoints (T031-T040)
- **Days 5-7**: Frontend pages and components (T041-T057)
- **Days 7-8**: Documentation and testing (T058-T061)

### Week 2-3: Analytics Features (Phase 2)
**Tasks**: T062-T070 (9 tasks)  
**Features**: Performance analytics, PDF export, Charts

### Week 3+: Nice-to-Have Enhancements (Phase 3)
**Tasks**: Beyond Phase 2  
**Features**: Student portal, Advanced filtering, Weighted marks

---

## 👥 Team Assignments (Suggested)

**Single Developer (Recommended for MVP)**:
- Do Phase 1 completely (all tasks T001-T060)
- Move to Phase 2 after Phase 1 testing
- Phase 3 as time permits

**Two Developers**:
- **Dev 1**: Backend (Database, CQRS, API) + Testing
- **Dev 2**: Frontend (Pages, Components) + Styling
- Both work on Documentation together

---

## 📦 Deliverables per Phase

### Phase 1 Deliverables
```
Backend:
✅ 6 new database tables (exams, exam_subjects, exam_classes, student_marks, 
   grade_configuration, student_report_cards)
✅ 25 CQRS handlers (Commands + Queries)
✅ 10+ API endpoints
✅ 8 repositories
✅ 12+ DTOs

Frontend:
✅ 5 new pages (Exams, MarksEntry, ReportCard, etc.)
✅ 10+ new React components
✅ React Query hooks and mutations
✅ Type definitions for all entities
✅ Mock data for testing

Testing:
✅ 30+ unit tests
✅ 15+ integration tests
✅ Mock API setup
✅ Component tests

Documentation:
✅ Implementation summary
✅ Detailed guide with code examples
✅ API endpoints reference
✅ Database schema documentation
```

### Phase 2 Deliverables
```
Backend:
✅ 3 advanced analytics queries
✅ Analytics endpoints

Frontend:
✅ Analytics dashboard page
✅ 3+ chart components
✅ PDF export functionality
```

---

## 🛠️ Tech Stack Assumptions

**Backend**:
- C# (.NET 8)
- Entity Framework Core
- MediatR (CQRS)
- AutoMapper (for DTOs)
- PostgreSQL

**Frontend**:
- React 19 + TypeScript
- Vite
- React Query
- Material UI
- Chart library (recharts or chart.js)
- jsPDF or html2pdf (for PDF export)

---

## 📊 Detailed Task Breakdown

### Phase 1 Task Groups

#### Group A: Database & Domain (Days 1-2) - 11 tasks
```
T001: Exam tables migration
T002: Grade configuration table
T003: Report card table
T004: Foreign key relationships
T005: Migration verification
T006: Exam entity
T007: ExamSubject entity
T008: ExamClass entity
T009: StudentMarks entity
T010: GradeConfiguration entity
T011: StudentReportCard entity
```

**Lead Task**: T001 (Blocker for T006-T011)  
**Dependencies**: None  
**Effort**: 8-10 hours  

#### Group B: Repositories & Data Access (Days 2-3) - 4 tasks
```
T012: Add DbSets to context
T013: ExamRepository
T014: StudentMarksRepository
T015: ReportCardRepository
```

**Lead Task**: T012 (Blocker for T013-T015)  
**Dependencies**: Group A (T001-T005)  
**Effort**: 6-8 hours  

#### Group C: CQRS - Commands (Days 3-4) - 8 tasks
```
T016: CreateExamCommand
T017: UpdateExamCommand
T018: PublishExamCommand
T019: DeleteExamCommand
T020: SaveStudentMarksCommand
T021: SubmitMarksCommand
T022: GenerateReportCardCommand
T023: ConfigureGradesCommand
```

**Lead Task**: T016 (Blocker for T017-T019)  
**Dependencies**: Group B (T013, T014, T015)  
**Effort**: 10-12 hours  
**Can run parallel**: T016-T019 can be done together, then T020-T023

#### Group D: CQRS - Queries (Days 3-4) - 7 tasks
```
T024: GetExamsQuery
T025: GetExamByIdQuery
T026: GetStudentMarksQuery
T027: GetSingleStudentMarksQuery
T028: GetReportCardQuery
T029: GetExamReportCardsQuery
T030: GetGradeConfigurationQuery
```

**Lead Task**: T024 (Blocker for others)  
**Dependencies**: Group B (T013, T014, T015)  
**Effort**: 8-10 hours  
**Can run parallel**: All can be done in parallel

#### Group E: DTOs & API (Days 4-5) - 9 tasks
```
T031: Exam DTOs
T032: Marks DTOs
T033: Report Card DTOs
T034: Grade DTOs
T035: ExamsController
T036: MarksController
T037: ReportCardsController
T038: GradesController
T039: Error handling
T040: Swagger docs
```

**Lead Task**: T031 (Blocker for T035-T040)  
**Dependencies**: Group C & D (T016-T030)  
**Effort**: 10-12 hours  

#### Group F: Frontend Pages (Days 5-7) - 10 tasks
```
T041: ExamsPage
T042: ExamForm
T043: ExamDetailsPage
T044: MarksEntryPage
T045: MarksEntryTable
T046: CSVImporter
T047: ReportCardPage
T048: ReportCardTemplate
T049: ReportCardView
T050: Types (exam.ts)
```

**Lead Task**: T050 (Blocker for T041-T049)  
**Dependencies**: T031-T034 (DTOs defined)  
**Effort**: 14-16 hours  
**Can run parallel**: T041-T049 can be done in parallel

#### Group G: Frontend Services & Hooks (Days 5-6) - 3 tasks
```
T051: examApi.ts service
T052: useExamHooks.ts
T053: useExamMutations.ts
```

**Lead Task**: T051 (Blocker for T052, T053)  
**Dependencies**: T031-T034, T035-T040  
**Effort**: 6-8 hours  

#### Group H: Testing & Mock Data (Days 6-7) - 4 tasks
```
T054: Mock data
T055: ExamWorkflow tests
T056: MarksCalculation tests
T057: ExamAPI integration tests
```

**Lead Task**: T054 (Blocker for T055-T057)  
**Dependencies**: All previous groups  
**Effort**: 10-12 hours  

#### Group I: Documentation (Days 7-8) - 4 tasks
```
T058: Implementation summary
T059: Implementation guide
T060: API reference
T061: Database schema docs
```

**Lead Task**: None (can do in parallel)  
**Dependencies**: All code complete  
**Effort**: 8-10 hours  

---

## 🔄 Parallel Execution Strategy

**Single Developer**: Sequential (Group A → B → C/D → E → F → G → H → I)

**Two Developers**:
```
Days 1-2:
  Dev 1: Group A (Database)
  Dev 2: Starts Group F prep (understand types)

Days 2-3:
  Dev 1: Group B (Repositories)
  Dev 2: Starts Group F prep

Days 3-4:
  Dev 1: Group C (Commands) + Group D (Queries) in parallel
  Dev 2: Group F prep completes, starts Group F tasks

Days 4-5:
  Dev 1: Group E (DTOs & API)
  Dev 2: Group F (Pages & Components)

Days 5-6:
  Dev 1: Group G (Hooks & Integration)
  Dev 2: Group F completion + Group H tests

Days 6-7:
  Dev 1: Group H tests
  Dev 2: Group I documentation

Days 7-8:
  Both: Final testing, bug fixes, review
```

---

## 🎯 Key Milestones

### Milestone 1: MVP - Core Exam Management (End of Week 1)
**Status**: All of Group A-E complete
- Can create exams
- Can publish exams
- Can view exam list
- API endpoints working

**Pass Criteria**:
- [ ] All CQRS handlers implemented
- [ ] All API endpoints working
- [ ] Swagger docs complete
- [ ] Backend tests passing

### Milestone 2: Full Marks Entry & Report Cards (Mid Week 2)
**Status**: Group A-H complete
- Can enter marks for students
- Can submit marks
- Report cards auto-generate
- Can view report cards

**Pass Criteria**:
- [ ] Marks entry page working
- [ ] Report cards generating correctly
- [ ] Calculations verified
- [ ] Frontend tests passing

### Milestone 3: Testing & Polish (End of Week 2)
**Status**: All unit tests and integration tests passing
- All edge cases covered
- Error handling working
- UI polished

**Pass Criteria**:
- [ ] 95%+ test coverage
- [ ] All critical bugs fixed
- [ ] Documentation complete

### Milestone 4: Analytics & Export (Week 3)
**Status**: Phase 2 complete
- Analytics dashboard working
- PDF export functional
- Performance optimized

---

## 📈 Effort Estimation

```
Backend Development:     50-60 hours
Frontend Development:    40-50 hours
Testing:                15-20 hours
Documentation:           8-12 hours
Code Review & Fixes:    10-15 hours
─────────────────────────────────
TOTAL:                 123-157 hours

For 1 Developer (40 hrs/week):  3-4 weeks
For 2 Developers (80 hrs/week): 1.5-2 weeks
```

---

## 🚀 Development Sequence (Single Dev)

```
Start → T001 → T006 → T012 → T016 → T024 → T031 → T035 → T041 → T050 → T051 → T054 → T058 → Done
         (DB)   (EM)   (EM)   (Cmd)  (Qry)  (DTO)  (API)  (Frnt)  (Type) (Svc)  (Test) (Doc)
```

---

## 🔗 Critical Path Analysis

**Critical Path**: Database Schema → Domain Models → CQRS Handlers → API → Frontend

**Longest Chain** (determines minimum timeline):
1. Database schema (T001) - 1 day
2. Domain models (T006-T011) - 1 day
3. Repositories (T013-T015) - 0.5 day
4. CQRS Commands (T016-T023) - 1.5 days
5. CQRS Queries (T024-T030) - 1.5 days
6. DTOs & API (T031-T040) - 1.5 days
7. Frontend Pages (T041-T049) - 2 days
8. Services & Hooks (T051-T053) - 1 day
9. Testing (T054-T057) - 1.5 days

**Total Critical Path**: 11-12 days (~2 weeks for one developer with daily testing)

---

## ⚠️ Risk Mitigation

### High Risk: Database Migration Issues
**Mitigation**: 
- Test migrations in development first
- Keep rollback scripts
- Backup database before migration

### High Risk: Report Card Calculation Complexity
**Mitigation**:
- Create unit tests for all calculations first
- Document calculation logic
- Use fixtures for edge cases

### Medium Risk: Performance with Large Classes
**Mitigation**:
- Add database indices (T005)
- Batch queries where possible
- Pagination on frontend

### Medium Risk: CSV Import Edge Cases
**Mitigation**:
- Comprehensive validation (T046)
- Clear error messages
- Rollback on any error

---

## 📋 Pre-Development Checklist

Before starting implementation:

- [ ] Team members have access to repository
- [ ] Development environment is set up (Docker, .NET, React)
- [ ] Database backup strategy is in place
- [ ] Code review process is defined
- [ ] Testing tools are configured (xUnit, Jest, Vitest)
- [ ] CI/CD pipeline includes test execution
- [ ] Spec document is reviewed and approved
- [ ] Any open questions (see SPEC.md) are resolved

---

## 🔍 Code Quality Standards

All code must adhere to:
- **Backend**: Clean Architecture, CQRS pattern, single responsibility
- **Frontend**: React best practices, hooks only, prop composition
- **Testing**: Unit test each handler, integration test each endpoint
- **Documentation**: Inline comments, method documentation
- **Performance**: Database queries indexed, API responses < 2 seconds

---

## 📞 Support & Escalation

### If Blocked On...
**Database**: Check migration scripts, verify PostgreSQL version  
**API Endpoints**: Test with Postman/Swagger UI  
**Frontend**: Check React devtools, verify API response structure  
**Calculations**: Write unit test with expected output  

### Daily Progress Tracking
- [ ] Morning: Review previous day's work
- [ ] Mid-day: Document blockers
- [ ] EOD: Commit completed tasks, plan next day

---

## ✅ Final Acceptance

Feature is ready when:
1. ✅ All 65+ tasks completed
2. ✅ All tests passing (95%+ coverage)
3. ✅ Performance benchmarks met (< 2s API response)
4. ✅ Documentation complete and reviewed
5. ✅ Code reviewed and approved
6. ✅ Zero critical/high severity bugs
7. ✅ Ready for staging deployment


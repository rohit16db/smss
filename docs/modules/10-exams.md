# Module: Exams, Marks & Report Cards

## Overview
Full exam lifecycle: create exams, assign classes/subjects, enter marks, generate report cards, export PDFs, and view performance analytics. Supports grade configuration (A/B/C/D/F system).

---

## Domain Entities

### Exam (`SMS.Domain.Entities.Exam` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Mid-Term 2026" |
| Description | string? | |
| StartDate, EndDate | DateTime | |
| TotalMarks | decimal | Default 100 |
| PassMarks | decimal | Default 40 |
| Status | ExamStatus (enum) | Draft/Published/Completed |
| AcademicYearId | Guid | FK |
| CreatedById | Guid | FK to User |
| *Nav* | AcademicYear, Creator, ExamSubjects, ExamClasses, StudentMarks, StudentReportCards |

### ExamClass (`SMS.Domain.Entities.ExamClass`)
Links Exam → Class (which classes take this exam).

### ExamSubject (`SMS.Domain.Entities.ExamSubject`)
| Property | Type | Notes |
|----------|------|-------|
| ExamId | Guid | FK |
| SubjectId | Guid | FK |
| MaxMarks | decimal | Max marks for this subject |

### StudentMarks (`SMS.Domain.Entities.StudentMarks`)
| Property | Type | Notes |
|----------|------|-------|
| ExamId | Guid | FK |
| StudentId | Guid | FK |
| SubjectId | Guid | FK |
| MarksObtained | decimal | |
| IsAbsent | bool | |

### StudentReportCard (`SMS.Domain.Entities.StudentReportCard`)
Generated summary per student per exam.

### GradeConfiguration (`SMS.Domain.Entities.GradeConfiguration` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| GradeName | string | e.g., "A", "B+" |
| MinPercentage, MaxPercentage | decimal | |
| Description | string? | |
| SchoolId | Guid | FK |

### ExamStatus (Enum)
```csharp
Draft = 0, Published = 1, Completed = 2
```

---

## API Endpoints

### Exams Controller — Route: `api/v1/exams`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/exams` | List exams (filtered by academic year) |
| GET | `/api/v1/exams/{examId}` | Get exam details |
| POST | `/api/v1/exams` | Create exam + assign classes/subjects |
| PUT | `/api/v1/exams/{examId}` | Update exam |
| DELETE | `/api/v1/exams/{examId}` | Delete exam |
| POST | `/api/v1/exams/{examId}/publish` | Change status to Published |

### Marks Controller — Route: `api/v1/exams/{examId}/marks`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/exams/{examId}/marks/form/{classId}` | Get marks entry form |
| POST | `/api/v1/exams/{examId}/marks/save/{classId}` | Save marks |
| GET | `/api/v1/exams/{examId}/marks/student/{studentId}` | Student marks |
| GET | `/api/v1/exams/{examId}/marks/class/{classId}` | Class marks |
| POST | `/api/v1/exams/{examId}/marks/submit/{classId}` | Submit/finalize marks |

### Report Cards Controller — Route: `api/v1/reportcards`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/reportcards/exam/{examId}` | All report cards for exam |
| GET | `/api/v1/reportcards/student/{studentId}` | Report cards for student |
| GET | `/api/v1/reportcards/{cardId}` | Single report card |
| GET | `/api/v1/reportcards/{examId}/{studentId}` | Report card by exam+student |
| POST | `/api/v1/reportcards/{cardId}/export-pdf` | Export as PDF |

### Grades Controller — Route: `api/v1/grades`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/grades` | List grade configurations |
| POST | `/api/v1/grades` | Create/update grades |

### Analytics Controller — Route: `api/analytics`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | Various | Performance analytics data |

---

## CQRS (in `Features/Exams`)
- **Commands**: CreateExam, UpdateExam, DeleteExam, PublishExam, SaveMarks, SubmitMarks
- **Queries**: GetExams, GetExamById, GetMarksForm, GetStudentMarks, GetClassMarks, GetReportCards
- **Services**: `Features/Exams/Services/` — Report card generation logic

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/Exam.cs` |
| Entity | `backend/src/SMS.Domain/Entities/ExamClass.cs` |
| Entity | `backend/src/SMS.Domain/Entities/ExamSubject.cs` |
| Entity | `backend/src/SMS.Domain/Entities/StudentMarks.cs` |
| Entity | `backend/src/SMS.Domain/Entities/StudentReportCard.cs` |
| Entity | `backend/src/SMS.Domain/Entities/GradeConfiguration.cs` |
| Enum | `backend/src/SMS.Domain/Enums/ExamStatus.cs` |
| Enum | `backend/src/SMS.Domain/Enums/MarksEntryStatus.cs` |
| Commands | `backend/src/SMS.Application/Features/Exams/Commands/` |
| DTOs | `backend/src/SMS.Application/Features/Exams/DTOs/` |
| Handlers | `backend/src/SMS.Application/Features/Exams/Handlers/` |
| Queries | `backend/src/SMS.Application/Features/Exams/Queries/` |
| Services | `backend/src/SMS.Application/Features/Exams/Services/` |
| Validators | `backend/src/SMS.Application/Features/Exams/Validators/` |
| Controllers | `backend/src/SMS.API/Controllers/ExamsController.cs` |
| Controllers | `backend/src/SMS.API/Controllers/MarksController.cs` |
| Controllers | `backend/src/SMS.API/Controllers/ReportCardsController.cs` |
| Controllers | `backend/src/SMS.API/Controllers/GradesController.cs` |
| Controllers | `backend/src/SMS.API/Controllers/AnalyticsController.cs` |
| Frontend | `frontend/src/pages/ExamsPage.tsx` |
| Frontend | `frontend/src/pages/MarksPage.tsx` |
| Frontend | `frontend/src/pages/ReportCardsPage.tsx` |
| Frontend | `frontend/src/pages/ReportCardDetailPage.tsx` |
| Frontend | `frontend/src/pages/PerformanceAnalyticsPage.tsx` |
| Hooks | `frontend/src/hooks/useExamHooks.ts` |
| Hooks | `frontend/src/hooks/useMarksHooks.ts` |
| Hooks | `frontend/src/hooks/useReportCardHooks.ts` |
| Hooks | `frontend/src/hooks/useGradeHooks.ts` |
| Hooks | `frontend/src/hooks/useAnalyticsHooks.ts` |
| Frontend API | `frontend/src/services/examApi.ts` |

---

## Business Rules
- Exam status workflow: Draft → Published → Completed
- Marks can only be entered for Published exams
- MarksObtained must be ≤ ExamSubject.MaxMarks
- Report card uses GradeConfiguration to assign letter grades
- PDF report cards include school branding from Settings

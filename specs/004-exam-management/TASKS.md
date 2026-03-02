# Implementation Tasks: Exam & Marks Management

**Feature**: 004-exam-management  
**Created**: February 2026  
**Total Tasks**: 65+ items across 3 phases

---

## Phase 1: Core Exam & Marks Entry (Week 1-2) 🎯 PRIORITY

### Backend: Database Schema & Migrations

- [ ] **T001** Create migrations for core exam tables:
  - `examinations` table
  - `exam_subjects` junction table
  - `exam_classes` junction table
  - `student_marks` table
  - Add indices on exam_id, student_id, subject_id

- [ ] **T002** Create `grade_configuration` table with defaults:
  - Grade A (90-100)
  - Grade B (80-89)
  - Grade C (70-79)
  - Grade D (60-69)
  - Grade F (0-59)

- [ ] **T003** Create `student_report_cards` table:
  - Denormalized view for report card data
  - Index on exam_id and student_id
  - Pre-calculated fields

- [ ] **T004** Add foreign key relationships:
  - exams → users (created_by)
  - exam_subjects → subjects
  - exam_classes → classes
  - student_marks → exams, students, subjects

- [ ] **T005** Run migrations in development and test:
  - Verify all table structures
  - Test relationships
  - Confirm indices created

### Backend: Domain Models

- [ ] **T006** Create `Exam` entity in SMS.Domain:
  - Properties: Id, Name, Description, ExamDate, TotalMarks, PassMarks, Status
  - Relations: ExamSubjects, ExamClasses
  - Status enum: Draft, Published, Completed, Archived

- [ ] **T007** Create `ExamSubject` entity:
  - Properties: Id, ExamId, SubjectId, MaxMarks, PassMarks
  - Validation: MaxMarks > 0, PassMarks <= MaxMarks

- [ ] **T008** Create `ExamClass` entity:
  - Properties: Id, ExamId, ClassId, MarksEntryStatus, SubmittedAt
  - Status enum: Pending, InProgress, Submitted

- [ ] **T009** Create `StudentMarks` entity:
  - Properties: Id, ExamId, StudentId, SubjectId, MarksObtained, IsAbsent
  - Validation: 0 <= MarksObtained <= MaxMarks (when not absent)

- [ ] **T010** Create `GradeConfiguration` entity:
  - Properties: Id, GradeName, MinPercentage, MaxPercentage, SchoolId
  - Default grades: A, B, C, D, F

- [ ] **T011** Create `StudentReportCard` entity (read-only/denormalized):
  - Properties: Id, ExamId, StudentId, TotalMarksObtained, Percentage, OverallGrade, ClassPosition, Pass

### Backend: Repository & DbContext

- [ ] **T012** Add DbSets to `IApplicationDbContext`:
  - `DbSet<Exam> Exams`
  - `DbSet<ExamSubject> ExamSubjects`
  - `DbSet<ExamClass> ExamClasses`
  - `DbSet<StudentMarks> StudentMarks`
  - `DbSet<GradeConfiguration> GradeConfigurations`
  - `DbSet<StudentReportCard> StudentReportCards`

- [ ] **T013** Create `ExamRepository` with methods:
  - `GetAllExams(filter)` - with status, class, subject filters
  - `GetExamById(id)` - includes subjects and classes
  - `CreateExam(exam)` - saves new exam
  - `UpdateExam(exam)` - updates existing exam
  - `DeleteExam(id)` - soft delete or archive
  - `PublishExam(id)` - change status to Published

- [ ] **T014** Create `StudentMarksRepository` with methods:
  - `GetMarksForClass(examId, classId)` - all students in class
  - `GetMarksForStudent(examId, studentId)` - single student marks
  - `SaveStudentMarks(marks)` - bulk save
  - `UpdateStudentMark(studentId, subjectId, marks)` - single update
  - `SubmitClassMarks(examId, classId)` - finalize marks

- [ ] **T015** Create `ReportCardRepository` with methods:
  - `GenerateReportCard(examId, studentId)` - create report card
  - `GetReportCard(examId, studentId)` - retrieve report card
  - `GetExamReportCards(examId)` - all report cards for exam
  - `GetStudentReportCards(studentId)` - all cards for student

### Backend: CQRS Handlers - Commands

- [ ] **T016** Create `CreateExamCommand` & Handler:
  - Validate: Name not empty, Date not in past, TotalMarks > 0
  - Create Exam with status: Draft
  - Return created exam ID

- [ ] **T017** Create `UpdateExamCommand` & Handler:
  - Only allow if status is Draft
  - Update: Name, Description, ExamDate, TotalMarks, PassMarks
  - Validate same as Create
  - Return updated exam

- [ ] **T018** Create `PublishExamCommand` & Handler:
  - Check status is Draft
  - Validate: At least one subject assigned
  - Validate: At least one class assigned
  - Change status to Published
  - Emit ExamPublishedEvent

- [ ] **T019** Create `DeleteExamCommand` & Handler:
  - Only allow if status is Draft
  - Soft delete or archive exam
  - Prevent deletion if marks already entered

- [ ] **T020** Create `SaveStudentMarksCommand` & Handler:
  - Accept: ExamId, ClassId, List<StudentMarksData>
  - Validate: All marks ≤ MaxMarks per subject
  - Validate: StudentId exists in class
  - Validate: Is absent OR has marks (not both empty)
  - Batch insert/update StudentMarks
  - Status remains InProgress

- [ ] **T021** Create `SubmitMarksCommand` & Handler:
  - Accept: ExamId, ClassId
  - Validate: All students have marks or marked absent
  - Change ExamClass status to Submitted
  - Trigger GenerateReportCardCommand for each student
  - Emit MarksSubmittedEvent

- [ ] **T022** Create `GenerateReportCardCommand` & Handler:
  - Accept: ExamId, StudentId
  - Calculate: Total marks, Percentage, Overall grade
  - Calculate: Class position/rank
  - Create StudentReportCard record
  - Return report card data

- [ ] **T023** Create `ConfigureGradesCommand` & Handler:
  - Accept: List<GradeConfigurationData>
  - Validate: No overlapping ranges
  - Validate: Ranges cover 0-100%
  - Update or create grade configurations
  - Emit GradeConfigurationChangedEvent

### Backend: CQRS Handlers - Queries

- [ ] **T024** Create `GetExamsQuery` & Handler:
  - Support filters: Status, ClassId, SubjectId, DateRange
  - Support sorting: ByDate, ByName, ByStatus
  - Support pagination
  - Return: ExamListDto with subject/class counts

- [ ] **T025** Create `GetExamByIdQuery` & Handler:
  - Load exam with all subjects and classes
  - Load marks entry status per class
  - Return: ExamDetailDto

- [ ] **T026** Create `GetStudentMarksQuery` & Handler:
  - Accept: ExamId, ClassId
  - Return: All students in class with marks for each subject
  - Include: Auto-calculated totals and percentages
  - Include: Grade assignment
  - Return: List<StudentMarksDto>

- [ ] **T027** Create `GetSingleStudentMarksQuery` & Handler:
  - Accept: ExamId, StudentId
  - Return: Marks for all subjects in exam
  - Include: Total, Percentage, Grade

- [ ] **T028** Create `GetReportCardQuery` & Handler:
  - Accept: ExamId, StudentId
  - Return: Complete report card: StudentReportCardDto
  - Include: All subject marks, total, grade, class position

- [ ] **T029** Create `GetExamReportCardsQuery` & Handler:
  - Accept: ExamId
  - Support filters: ClassName, PassStatus
  - Support sorting: ByClassPosition, ByName
  - Return: List<StudentReportCardDto>

- [ ] **T030** Create `GetGradeConfigurationQuery` & Handler:
  - Return: Current grade configuration
  - Format: Sorted by percentage range

### Backend: DTOs

- [ ] **T031** Create Exam DTOs:
  - `CreateExamDto` - Input for exam creation
  - `UpdateExamDto` - Input for exam update
  - `ExamDto` - Basic exam info
  - `ExamDetailDto` - Complete exam with subjects, classes, marks status

- [ ] **T032** Create Marks DTOs:
  - `StudentMarksDto` - Single student marks
  - `MarksEntryDto` - Marks form data (all students, all subjects)
  - `SubjectMarksDto` - Marks for one subject
  - `StudentMarkRowDto` - Table row: student + all subject marks

- [ ] **T033** Create Report Card DTOs:
  - `ReportCardDto` - Complete report card
  - `SubjectMarkDetailDto` - Subject marks in report card
  - `StudentReportCardListDto` - Report card summary for list
  - `ReportCardExportDto` - For PDF/print export

- [ ] **T034** Create Grade & Configuration DTOs:
  - `GradeConfigurationDto` - Grade scale definition
  - `GradeAssignmentDto` - Grade with range
  - `GradeConfigUpdateDto` - Update grade configuration

### Backend: API Endpoints

- [ ] **T035** Create `ExamsController` with endpoints:
  - `POST /api/v1/exams` - Create exam (T016)
  - `GET /api/v1/exams` - List exams with filters (T024)
  - `GET /api/v1/exams/{examId}` - Get exam details (T025)
  - `PUT /api/v1/exams/{examId}` - Update exam (T017)
  - `DELETE /api/v1/exams/{examId}` - Delete/Archive exam (T019)
  - `POST /api/v1/exams/{examId}/publish` - Publish exam (T018)

- [ ] **T036** Create `MarksController` with endpoints:
  - `GET /api/v1/exams/{examId}/classes/{classId}/marks` - Get marks form (T026)
  - `POST /api/v1/exams/{examId}/classes/{classId}/marks` - Save marks (T020)
  - `GET /api/v1/exams/{examId}/marks/{studentId}` - Get student marks (T027)
  - `PUT /api/v1/exams/{examId}/marks/{studentId}` - Update student marks (T020)
  - `POST /api/v1/exams/{examId}/classes/{classId}/submit` - Submit marks (T021)

- [ ] **T037** Create `ReportCardsController` with endpoints:
  - `GET /api/v1/report-cards` - List all report cards (T029)
  - `GET /api/v1/report-cards/{examId}/{studentId}` - Get single card (T028)
  - `GET /api/v1/exams/{examId}/report-cards` - Get exam report cards (T029)

- [ ] **T038** Create `GradesController` with endpoints:
  - `GET /api/v1/grades` - Get grade configuration (T030)
  - `PUT /api/v1/grades` - Update grade configuration (T023)

- [ ] **T039** Add error handling to all endpoints:
  - 404: Exam/Student/Class not found
  - 400: Invalid marks (exceeds max), invalid state
  - 409: Conflict (status not Draft, already submitted)
  - 500: Server errors with logging

- [ ] **T040** Add Swagger/OpenAPI documentation:
  - Request/response examples
  - Parameter descriptions
  - Error code documentation
  - Status code ranges

### Frontend: Pages & Components

- [ ] **T041** Create `ExamsPage.tsx`:
  - List all exams in table
  - Columns: Name, Date, Classes, Subjects, Status
  - Filters: By Status, Class, Subject, Date Range
  - Create exam button → Navigate to create form
  - View exam button → Navigate to details
  - Edit exam button (if draft) → Navigate to edit form
  - Search bar by exam name

- [ ] **T042** Create `ExamForm.tsx` component:
  - Input fields: Name, Description, ExamDate, TotalMarks, PassMarks
  - Subject selector (multi-select dropdown)
  - Class selector (multi-select dropdown)
  - Save/Cancel buttons
  - Auto-save draft
  - Validation: Show errors for invalid inputs
  - Loading state during submit

- [ ] **T043** Create `ExamDetailsPage.tsx`:
  - Display exam header: Name, Date, Status, TotalMarks
  - Show subjects assigned (table: Subject, MaxMarks)
  - Show classes assigned (table: Class, MarksEntryStatus, SubmittedAt)
  - Buttons: Edit (if draft), Publish (if draft), Marks Entry (if published)
  - View analytics button
  - Report cards button

- [ ] **T044** Create `MarksEntryPage.tsx`:
  - Class selector dropdown (filtered to exam's classes)
  - Load all students in selected class (table rows)
  - Subject columns (from exam)
  - Input cells for marks with validation:
    - Only numbers
    - Max value = subjectMaxMarks
    - Red border if invalid
    - Show error on blur
  - Auto-calculated columns: Total, Percentage, Grade
  - Checkbox "Mark Absent" per student (disables marks input)
  - "Save Draft" button (saves current state)
  - "Submit Marks" button (finalizes marks, shows confirmation)
  - Status indicator: Draft/Submitted
  - "CSV Import" button → Opens CSV importer modal

- [ ] **T045** Create `MarksEntryTable.tsx` component:
  - Table with students as rows, subjects as columns
  - Sortable columns
  - Responsive design
  - Total/Percentage/Grade columns read-only
  - Cell editor for marks input

- [ ] **T046** Create `CSVImporter.tsx` component:
  - File uploader
  - CSV validation:
    - Headers match subjects
    - Data matches students
    - Marks are valid numbers
  - Preview imported data
  - Confirm import button
  - Error messages for invalid data

- [ ] **T047** Create `ReportCardPage.tsx`:
  - Exam selector dropdown
  - Class selector dropdown
  - List of report cards as cards/table rows:
    - Student name, Roll No, Total Marks, Grade, Class Position, Status
  - Click card to view full report card
  - Download PDF button per card
  - Print button

- [ ] **T048** Create `ReportCardTemplate.tsx` component:
  - Beautiful report card layout:
    - School logo/name header
    - Student info: Name, Roll No, Class, Date
    - Marksheet: Subject, Marks Obtained, MaxMarks, Grade
    - Summary: Total, Percentage, Overall Grade, Position
    - Remarks field
    - Principal signature line
    - Teacher signature line
  - Print-friendly styling
  - A4 page size optimized

- [ ] **T049** Create `ReportCardView.tsx` component:
  - Display report card data in template
  - Download PDF button
  - Print button
  - Back button
  - Share/Email button (future)

### Frontend: Types & Hooks

- [ ] **T050** Create `src/types/exam.ts`:
  - Export type definitions from SPEC.md
  - Exam, ExamSubject, ExamClass
  - StudentMarks, ReportCard, PerformanceAnalytics
  - All DTOs from backend

- [ ] **T051** Create `src/services/examApi.ts`:
  - HTTP client wrapper for exam endpoints
  - Methods matching backend endpoints:
    - createExam, updateExam, publishExam, deleteExam
    - getExams, getExamById
    - getMarksForm, saveMarks, submitMarks
    - getReportCard, getReportCards
    - updateGradeConfiguration

- [ ] **T052** Create `src/services/queries/useExamHooks.ts`:
  - React Query hooks:
    - useExams(filters, enabled)
    - useExamById(examId, enabled)
    - useMarksForm(examId, classId, enabled)
    - useReportCard(examId, studentId, enabled)
    - useReportCards(examId, filters, enabled)
    - useGradeConfiguration()
  - Cache configuration (5 min stale time)
  - Query key constants: examKeys, marksKeys, cardKeys

- [ ] **T053** Create `src/services/mutations/useExamMutations.ts`:
  - React Query mutations:
    - useCreateExam()
    - useUpdateExam()
    - usePublishExam()
    - useSaveMarks()
    - useSubmitMarks()
    - useUpdateGrades()
  - Error handling with toasts
  - Cache invalidation after mutations

### Frontend: Testing & Mock Data

- [ ] **T054** Update `src/test/mockData.ts`:
  - Add `mockExams` - 5-10 sample exams
  - Add `mockStudentMarks` - Sample marks data
  - Add `mockReportCards` - Sample report cards
  - Add `mockGradeConfiguration` - Default grades
  - Include various statuses (Draft, Published, Completed)

- [ ] **T055** Create `src/__tests__/ExamWorkflow.test.tsx`:
  - Test exam creation form (fields, validation)
  - Test exam list loading and filtering
  - Test marks entry table (input, validation, calc)
  - Test marks submission flow
  - Test report card display
  - Mock API calls using msw or jest.mock

- [ ] **T056** Create `src/__tests__/MarksCalculation.test.ts`:
  - Test grade assignment from percentage
  - Test total and percentage calculation
  - Test class position/rank calculation
  - Test invalid marks validation

- [ ] **T057** Create `src/__tests__/ExamAPI.integration.test.tsx`:
  - Test exam creation API
  - Test marks submission API
  - Test report card generation API
  - Test grade calculation API
  - Edge cases: No students, All absent, All passed

### Documentation

- [ ] **T058** Create `IMPLEMENTATION_SUMMARY.md`:
  - Overview of what was built
  - File changes summary (Created, Modified)
  - Feature checklist
  - Database schema (tables created)
  - API endpoints overview

- [ ] **T059** Create `IMPLEMENTATION-GUIDE.md`:
  - Detailed guide with code examples
  - Data flow diagrams (Mermaid)
  - File structure explanation
  - How to use each feature
  - Code snippets for developers
  - Troubleshooting guide

- [ ] **T060** Create `API-ENDPOINTS.md`:
  - Full API endpoint reference
  - Request/response examples
  - Error codes and messages
  - Query parameters and filters
  - Curl examples for testing

- [ ] **T061** Create `DATABASE-SCHEMA.md`:
  - Table definitions
  - Column descriptions
  - Foreign key relationships
  - Indices and performance notes
  - Migration history

---

## Phase 2: Analytics & Export (Week 2-3)

### Backend: Analytics Queries

- [ ] **T062** Create `GetExamAnalyticsQuery` & Handler:
  - Calculate: Class average, pass rate, grade distribution
  - Return: ExamAnalyticsDto
  - Include: Subject-wise analysis

- [ ] **T063** Create `GetClassPerformanceQuery` & Handler:
  - Class-level performance summary
  - Top/Bottom 5 students
  - Subject-wise stats

- [ ] **T064** Create `GetStudentPerformanceTrendQuery` & Handler:
  - Student marks across multiple exams
  - Trend graph data
  - Grade progression

### Frontend: Analytics Components

- [ ] **T065** Create `PerformanceAnalyticsPage.tsx`
- [ ] **T066** Create chart components (PerformanceChart, GradeDistribution, TrendChart)
- [ ] **T067** Create TOP 5 / BOTTOM 5 performer tables

### PDF Export

- [ ] **T068** Integrate PDF library (react-pdf or html2pdf)
- [ ] **T069** Implement report card PDF export
- [ ] **T070** Add school logo/letterhead to PDF

---

## Phase 3: Enhancements (Week 3+)

### Student Portal

- [ ] Add student dashboard to view own report cards
- [ ] Student performance trend view
- [ ] Download own report cards

### Advanced Features

- [ ] Weighted marks (theory 70%, practical 30%)
- [ ] Weighted subjects (core vs elective)
- [ ] Exam time duration and schedule
- [ ] Marks approval workflow

---

## Testing Checkpoints

After Phase 1:
- [ ] All exams can be created (Draft)
- [ ] Exams can be published
- [ ] Marks can be entered for all students
- [ ] Marks are validated correctly
- [ ] Report cards are generated on submit
- [ ] Report cards show correct calculations
- [ ] PDF export works
- [ ] API tests pass
- [ ] UI tests pass

---

## Success Criteria

✅ 65+ tasks completed  
✅ Full exam module working end-to-end  
✅ Marks entry with validation  
✅ Auto-report card generation  
✅ PDF export  
✅ Performance analytics (Phase 2)  
✅ 90%+ test coverage  
✅ Zero critical bugs  


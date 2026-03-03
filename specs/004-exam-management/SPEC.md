# Feature Specification: Exam & Marks Management

**Feature**: 004-exam-management  
**Created**: February 2026  
**Status**: Draft  
**Priority**: P2 (Phase 2 Enhancement)  
**Timeline**: 2-3 weeks  
**Related PRD**: Section 5.2 (Exam & Marks Management)

---

## Overview

This specification covers the implementation of comprehensive exam and marks management functionality for the School Management System. It enables educators to conduct examinations, record student marks, generate report cards, and analyze academic performance.

### Current State
- ❌ No exam management capability
- ❌ No grades/marks recording
- ❌ No report cards

### Target State
- 📝 Full exam lifecycle management (create → conduct → marks entry → report generation)
- 📊 Student performance analytics and grade distribution
- 📋 Automated report card generation
- 🎓 Grade calculations based on configurable criteria
- 📈 Class-wise performance analytics

---

## User Stories & Acceptance Criteria

### **US1: Exam Creation & Management**
**Priority**: P1 | **Effort**: Medium | **Timeline**: 3-4 days

**Business Value**: Teachers need to plan and schedule school examinations across multiple subjects and classes.

**Scenario**:
Teacher/Admin wants to:
- Create exam (name, date, total marks)
- Assign exam to multiple classes
- Set maximum marks per subject
- Define exam schedule (date, duration)
- Publish/Update exam details

**Acceptance Criteria**:
1. Create new exam with: Name, Date, Total Marks, Description
2. Select subjects for exam
3. Assign exam to one or multiple classes/sections
4. Set pass marks (default: 40% or configurable)
5. Mark exam as Draft/Published/Completed
6. Edit exam details before publishing
7. View all exams with filter by: Class, Subject, Date, Status
8. Exam list shows: Name, Date, Class(es), Subject count, Status
9. Sort by: Date, Subject count, Status
10. Archive completed exams

---

### **US2: Marks Entry for Students**
**Priority**: P1 | **Effort**: Medium | **Timeline**: 3-4 days

**Business Value**: Teachers need an efficient way to record and manage student marks for each examination.

**Scenario**:
Teacher wants to:
- View all students in their class
- Enter marks for each student per subject
- Auto-calculate total and percentage
- Validate marks don't exceed maximum
- Save/Draft/Submit marks
- Edit marks before final submission
- Bulk import marks from Excel/CSV

**Acceptance Criteria**:
1. Marks entry table showing:
   - Student name, roll number
   - Text input fields for subject marks
   - Auto-calculated total column
   - Auto-calculated percentage column
   - Grade column (auto-calculated based on marks)
2. Real-time validation:
   - Marks ≤ max marks configured
   - Marks ≥ 0
   - Only numeric input accepted
3. Status tracking: Draft/Submitted/Completed
4. Submit button finalizes marks
5. Edit button allows changes before final submission
6. Batch mark import from CSV with validation
7. Mark individual student entry as absent (A) - shows as 0
8. Duplicate marks from previous exam (optional)
9. Mark distribution: Show which students passed/failed

---

### **US3: Grade Calculation & Report Card Generation**
**Priority**: P1 | **Effort**: Medium | **Timeline**: 3-4 days

**Business Value**: Automated report card generation with grades and remarks based on school policies.

**Scenario**:
Admin wants to:
- View student report cards
- See marks, calculated grade, position in class
- Print/export report cards
- Set grading scale (A, B, C, D, F)
- Configure pass/fail criteria

**Acceptance Criteria**:
1. Report card shows:
   - Student name, roll number, class, date
   - Subject-wise marks with max marks
   - Subject-wise grades
   - Total marks and percentage
   - Overall grade/GPA
   - Class position/rank
   - Attendance percentage (if available)
   - Principal remarks field
   - Teacher's signature area
2. Configurable grading scale:
   - Grade A: 90-100%
   - Grade B: 80-89%
   - Grade C: 70-79%
   - Grade D: 60-69%
   - Grade F: <60% (Fail)
3. Pass criteria: Student must pass all subjects (≥40%) OR average ≥50%
4. GPA calculation: Sum of grade points / number of subjects
5. Class rank: Calculated based on overall marks/percentage
6. Report card soft copy export: PDF with formatted layout
7. Multi-subject report in single card
8. Remarks field for principal comments

---

### **US4: Performance Analysis & Analytics**
**Priority**: P2 | **Effort**: Medium | **Timeline**: 3-4 days

**Business Value**: Administrators and teachers need insights into student and class performance.

**Scenario**:
Principal/Admin wants to:
- See class average performance across exams
- Identify top/bottom performing students
- Analyze subject-wise performance
- Track improvement over exams
- Compare class performance

**Acceptance Criteria**:
1. Exam Performance Dashboard shows:
   - Class average marks and percentage
   - Pass rate % (students who passed)
   - Subject-wise pass rate
   - Grade distribution (count of A, B, C, D, F)
   - Mark distribution chart (histogram)
2. Top 5 & Bottom 5 performing students in class
3. Subject-wise performance:
   - Average marks per subject
   - Highest/Lowest marks per subject
   - Subject pass rate
4. Trend analysis:
   - Student performance trend across exams
   - Class average trend across exams
   - Subject performance trend
5. Comparative analysis:
   - Performance comparison across classes
   - Subject comparison across classes
6. Filters: By Exam, Class, Subject, Date range
7. Export performance report as PDF/Excel

---

### **US5: Student Report Card View (Portal)**
**Priority**: P2 | **Effort**: Small | **Timeline**: 2 days

**Business Value**: Students can view their report cards and academic progress.

**Scenario**:
Student wants to:
- View their report cards for all exams
- Track academic progress over time
- See their grades and class position
- Download report card as PDF

**Acceptance Criteria**:
1. Student dashboard shows: List of all completed exams
2. Click exam to view their report card
3. Report card shows all details (marks, grade, class rank)
4. Download report card as PDF
5. View trend graph: My marks across exams
6. See class average for comparison (anonymous)
7. Parents can also view student's report cards

---

---

## Technical Requirements

### **Backend Specifications**

#### Database Schema

**Exams Table**
```sql
CREATE TABLE exams (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    exam_date DATE NOT NULL,
    total_marks DECIMAL(5, 2) NOT NULL DEFAULT 100,
    pass_marks DECIMAL(5, 2) NOT NULL DEFAULT 40,
    status VARCHAR(50) NOT NULL DEFAULT 'draft', -- draft, published, completed, archived
    created_by UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(name, exam_date)
);
```

**ExamSubjects Table** (Many-to-Many)
```sql
CREATE TABLE exam_subjects (
    id UUID PRIMARY KEY,
    exam_id UUID NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
    subject_id UUID NOT NULL REFERENCES subjects(id),
    max_marks DECIMAL(5, 2) NOT NULL,
    pass_marks DECIMAL(5, 2) NOT NULL DEFAULT 40,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(exam_id, subject_id)
);
```

**ExamClasses Table** (Many-to-Many)
```sql
CREATE TABLE exam_classes (
    id UUID PRIMARY KEY,
    exam_id UUID NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
    class_id UUID NOT NULL REFERENCES classes(id),
    marks_entry_status VARCHAR(50) NOT NULL DEFAULT 'pending', -- pending, in_progress, submitted
    submitted_at TIMESTAMP,
    submitted_by UUID REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(exam_id, class_id)
);
```

**StudentMarks Table**
```sql
CREATE TABLE student_marks (
    id UUID PRIMARY KEY,
    exam_id UUID NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
    student_id UUID NOT NULL REFERENCES students(id),
    subject_id UUID NOT NULL REFERENCES subjects(id),
    marks_obtained DECIMAL(5, 2),
    is_absent BOOLEAN DEFAULT FALSE,
    remarks VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(exam_id, student_id, subject_id)
);
```

**GradeConfiguration Table**
```sql
CREATE TABLE grade_configuration (
    id UUID PRIMARY KEY,
    grade_name VARCHAR(10) NOT NULL,
    min_percentage DECIMAL(5, 2) NOT NULL,
    max_percentage DECIMAL(5, 2) NOT NULL,
    description VARCHAR(255),
    school_id UUID NOT NULL REFERENCES schools(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(school_id, grade_name)
);

-- Default data:
-- A: 90-100, B: 80-89, C: 70-79, D: 60-69, F: 0-59
```

**StudentReportCard Table** (Denormalized for Report Generation)
```sql
CREATE TABLE student_report_cards (
    id UUID PRIMARY KEY,
    exam_id UUID NOT NULL REFERENCES exams(id),
    student_id UUID NOT NULL REFERENCES students(id),
    total_marks_obtained DECIMAL(5, 2),
    total_marks DECIMAL(5, 2),
    percentage DECIMAL(5, 2),
    overall_grade VARCHAR(10),
    class_position INT,
    pass BOOLEAN,
    generated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(exam_id, student_id)
);
```

#### API Endpoints

**Exam Management**
```
POST   /api/v1/exams                          -- Create exam
GET    /api/v1/exams                          -- List exams (with filters)
GET    /api/v1/exams/{examId}                 -- Get exam details
PUT    /api/v1/exams/{examId}                 -- Update exam (if draft)
DELETE /api/v1/exams/{examId}                 -- Delete/Archive exam
POST   /api/v1/exams/{examId}/publish         -- Publish exam
POST   /api/v1/exams/{examId}/classes/{classId}/submit -- Submit marks for class
```

**Marks Entry**
```
GET    /api/v1/exams/{examId}/classes/{classId}/marks    -- Get marks entry form (all students)
POST   /api/v1/exams/{examId}/classes/{classId}/marks    -- Save/Submit marks
PUT    /api/v1/exams/{examId}/marks/{studentId}          -- Update single student marks
GET    /api/v1/exams/{examId}/marks/{studentId}          -- Get student marks for exam
```

**Report Cards**
```
GET    /api/v1/report-cards                   -- List all report cards (with filters)
GET    /api/v1/report-cards/{examId}/{studentId} -- Get single report card
GET    /api/v1/exams/{examId}/report-cards   -- Get all report cards for exam
POST   /api/v1/report-cards/{cardId}/export-pdf -- Export report card as PDF
```

**Analytics**
```
GET    /api/v1/exams/{examId}/analytics      -- Exam performance analytics
GET    /api/v1/exams/{examId}/class/{classId}/performance -- Class performance
GET    /api/v1/students/{studentId}/performance -- Student performance trend
GET    /api/v1/exams/{examId}/subject/{subjectId}/analysis -- Subject analysis
```

**Configuration**
```
GET    /api/v1/grade-configuration           -- Get grading scale
PUT    /api/v1/grade-configuration           -- Update grading scale
```

#### CQRS Handlers to Create

**Commands**
- `CreateExamCommand` → `CreateExamCommandHandler`
- `UpdateExamCommand` → `UpdateExamCommandHandler`
- `PublishExamCommand` → `PublishExamCommandHandler`
- `DeleteExamCommand` → `DeleteExamCommandHandler`
- `SaveStudentMarksCommand` → `SaveStudentMarksCommandHandler`
- `SubmitMarksCommand` → `SubmitMarksCommandHandler`
- `GenerateReportCardCommand` → `GenerateReportCardCommandHandler`
- `ConfigureGradesCommand` → `ConfigureGradesCommandHandler`

**Queries**
- `GetExamsQuery` → `GetExamsQueryHandler`
- `GetExamByIdQuery` → `GetExamByIdQueryHandler`
- `GetStudentMarksQuery` → `GetStudentMarksQueryHandler`
- `GetClassMarksQuery` → `GetClassMarksQueryHandler`
- `GetReportCardQuery` → `GetReportCardQueryHandler`
- `GetExamAnalyticsQuery` → `GetExamAnalyticsQueryHandler`
- `GetClassPerformanceQuery` → `GetClassPerformanceQueryHandler`
- `GetStudentPerformanceTrendQuery` → `GetStudentPerformanceTrendQueryHandler`

#### DTOs to Create
- `CreateExamDto`
- `ExamDto`
- `ExamDetailDto`
- `StudentMarksDto`
- `MarksEntryDto` (includes subject marks)
- `ReportCardDto`
- `PerformanceAnalyticsDto`
- `ClassPerformanceDto`
- `GradeConfigurationDto`

---

### **Frontend Specifications**

#### New Pages

1. **ExamsPage** (`src/pages/ExamsPage.tsx`)
   - List all exams
   - Filter by class, subject, status, date
   - Create new exam button
   - View exam details
   - Edit exam (if draft)
   - Publish exam button

2. **ExamDetailsPage** (`src/pages/ExamDetailsPage.tsx`)
   - Show exam information
   - Subject list with max marks
   - Class assignment list
   - Marks entry button (if published)
   - Analytics view button

3. **MarksEntryPage** (`src/pages/MarksEntryPage.tsx`)
   - Marks entry table for class
   - Subject columns with max marks
   - Auto-calculated total/percentage
   - Auto-calculated grade
   - Save/Submit buttons
   - Bulk import from CSV
   - Mark absent for students

4. **ReportCardPage** (`src/pages/ReportCardPage.tsx`)
   - Exam selection
   - Class selection
   - List of students with report cards
   - View individual report card
   - Print/Export to PDF

5. **PerformanceAnalyticsPage** (`src/pages/PerformanceAnalyticsPage.tsx`)
   - Exam selection
   - Class selection
   - Class average, pass rate, grade distribution charts
   - Subject-wise performance
   - Top/Bottom performers
   - Trend analysis

#### New Components

**Exam Creation/Update**
- `ExamForm.tsx` - Form to create/edit exam
- `ExamSubjectSelector.tsx` - Select subjects for exam
- `ExamClassSelector.tsx` - Assign classes to exam

**Marks Entry**
- `MarksEntryTable.tsx` - Table with student marks input
- `MarksValidation.tsx` - Validate marks entry
- `CSVImporter.tsx` - Import marks from CSV
- `MarksCellEditor.tsx` - Inline cell editor for marks

**Report Cards**
- `ReportCardTemplate.tsx` - Report card layout/design
- `ReportCardPreview.tsx` - Preview before PDF generation
- `ReportCardList.tsx` - List of report cards for exam
- `ReportCardPdfExport.tsx` - Export functionality

**Analytics**
- `PerformanceChart.tsx` - Mark distribution histogram
- `GradeDistributionChart.tsx` - Grade breakdown pie chart
- `ClassAverageCard.tsx` - Class average summary
- `TopPerformersTable.tsx` - Top 5 students
- `BottomPerformersTable.tsx` - Bottom 5 students
- `TrendChart.tsx` - Performance trend line chart
- `SubjectPerformanceTable.tsx` - Subject-wise analysis

#### Type Definitions (`src/types/exam.ts`)
```typescript
export type Exam = {
  id: string;
  name: string;
  description?: string;
  examDate: Date;
  totalMarks: number;
  passMarks: number;
  status: 'draft' | 'published' | 'completed' | 'archived';
  subjects: ExamSubject[];
  classes: ExamClass[];
  createdAt: Date;
  updatedAt: Date;
};

export type StudentMarks = {
  id: string;
  examId: string;
  studentId: string;
  subjectId: string;
  marksObtained: number | null;
  isAbsent: boolean;
  remarks?: string;
};

export type ReportCard = {
  id: string;
  examId: string;
  studentId: string;
  studentName: string;
  rollNumber: string;
  className: string;
  totalMarksObtained: number;
  totalMarks: number;
  percentage: number;
  overallGrade: string;
  classPosition: number;
  pass: boolean;
  subjectMarks: SubjectMarkDetail[];
};

export type PerformanceAnalytics = {
  examId: string;
  className: string;
  classAverage: number;
  passRate: number;
  totalStudents: number;
  passedStudents: number;
  failedStudents: number;
  gradeDistribution: GradeCount[];
  topPerformers: StudentPerformance[];
  bottomPerformers: StudentPerformance[];
  subjectAnalysis: SubjectAnalysis[];
};
```

#### React Query Hooks (`src/services/queries/useExamHooks.ts`)
```typescript
- useExams(filters) - List exams with filters
- useExamById(examId) - Get exam details
- useExamMarks(examId, classId) - Get marks for class
- useReportCard(examId, studentId) - Get report card
- useReportCards(examId, filters) - List report cards
- useExamAnalytics(examId) - Get analytics data
- useClassPerformance(examId, classId) - Class performance
- useStudentPerformanceTrend(studentId) - Performance trend
- useGradeConfiguration() - Get grading scale
```

#### API Service Methods (`src/services/examApi.ts`)
```typescript
- createExam(data) - Create exam
- updateExam(examId, data) - Update exam
- publishExam(examId) - Publish exam
- getExams(filters) - List exams
- getExamById(examId) - Get exam details
- getMarksEntryForm(examId, classId) - Get marks form
- saveMarks(examId, classId, marksData) - Save marks
- submitMarks(examId, classId) - Submit marks
- getReportCard(examId, studentId) - Get report card
- exportReportCardPdf(cardId) - Export PDF
- getAnalytics(examId) - Get analytics
- updateGradeConfiguration(data) - Config grades
```

---

## Data Flow

### Exam Creation Flow
```
Admin/Teacher creates exam
  → Fills exam details (name, date, max marks)
  → Selects subjects (one or multiple)
  → Assigns to classes (one or multiple)
  → Saves as DRAFT
  → ✋ Reviews exam details
  → Publishes exam (status: PUBLISHED)
  → Exam available for marks entry
```

### Marks Entry Flow
```
Exam Published and Assigned to Class
  → Teacher navigates to Marks Entry page
  → Loads all students in class
  → Enters marks for each subject per student
  → System validates marks ≤ max marks
  → System auto-calculates total & percentage
  → Teacher saves marks (DRAFT mode)
  → Teacher reviews and submits marks (FINAL)
  → Marks locked, report cards auto-generated
```

### Report Card Generation Flow
```
Marks Submitted for Exam/Class
  → System calculates per-student totals
  → System applies grading scale
  → System calculates class position/rank
  → StudentReportCard records created
  → Report card available for viewing/export
  → Admin/Student can view/print/export PDF
```

---

## Key Features

### ✅ Exam Lifecycle Management
- Draft → Published → Completed → Archived
- Edit only in draft status
- Publish to enable marks entry

### ✅ Bulk Marks Entry
- CSV import with validation
- Duplicate from previous exam
- Mark students absent (auto-zero)

### ✅ Automated Grading
- Configurable grade scale (A, B, C, D, F)
- Auto-grade assignment based on percentage
- GPA calculation

### ✅ Report Card Generation
- Auto-generated on marks submission
- Beautiful PDF export
- Class rank/position calculation
- Principal remarks field

### ✅ Performance Analytics
- Class average and pass rate
- Grade distribution charts
- Subject-wise analysis
- Top/Bottom performers
- Trend analysis across exams

### ✅ Multi-Subject Support
- Per-subject max marks
- Per-subject pass criteria
- Subject-wise report in card

---

## Implementation Phases

### Phase 1: Core Exam & Marks (Week 1-2)
- [ ] Database schema and migrations
- [ ] Exam CRUD operations
- [ ] Marks entry form and validation
- [ ] Basic report card generation

### Phase 2: Analytics & Export (Week 2-3)
- [ ] Performance analytics dashboard
- [ ] PDF export functionality
- [ ] Grading configuration
- [ ] Trend analysis

### Phase 3: Enhancements (Week 3+)
- [ ] Student performance portal
- [ ] Bulk CSV import
- [ ] Advanced filtering and search
- [ ] Performance comparison reports

---

## Success Criteria

✅ All exams can be created, published, and managed  
✅ Marks can be entered for all students across subjects  
✅ Report cards auto-generate with correct grades  
✅ Class performance analytics available  
✅ PDF export of report cards works  
✅ Grading scale is configurable  
✅ All data is validated and persisted correctly  
✅ Performance queries execute in < 2 seconds  

---

## Assumptions & Constraints

- **Subjects** already exist in the system (linked from Phase 2)
- **Classes** and **Students** already exist (linked from Phase 2)
- **Users** with Teacher/Admin roles already exist
- Grade scale is school-wide (not student-specific)
- Report cards are denormalized for performance
- One exam per day (no multiple exams same date)
- Marks entry happens after exam publishing
- No real-time collaboration for marks entry

---

## Open Questions

1. Should exam scheduling have time duration? (exam start/end times)
2. Should there be different pass marks per subject in exam?
3. Should we support different grading scales per class?
4. Should practical exams be supported (separate marks)?
5. Should weighted average be supported (theory 70%, practical 30%)?
6. Should exam have different weightage for different subjects?
7. Should there be an approval workflow for marks before finalization?


# Phase 1 Frontend Implementation Guide

## Overview

This guide documents the Phase 1 Frontend for the Exam Management Module. All components, hooks, and services have been created and are ready for integration into your React application.

## Architecture

The frontend follows a **clean, modular architecture** with separation of concerns:

```
src/
├── services/
│   └── examApi.ts              # HTTP client for all API endpoints
├── hooks/
│   ├── useExamHooks.ts         # React Query hooks for exams
│   ├── useMarksHooks.ts        # React Query hooks for marks
│   ├── useReportCardHooks.ts   # React Query hooks for report cards
│   └── useGradeHooks.ts        # React Query hooks for grades
├── pages/
│   ├── ExamsPage.tsx           # Exams management page
│   ├── MarksPage.tsx           # Marks entry page
│   └── ReportCardsPage.tsx     # Report cards view page
└── styles/
    └── pages.css               # Styling for all pages
```

## Components

### 1. ExamsPage.tsx
**Purpose**: Display and manage exams

**Features**:
- ✅ List all exams with pagination
- ✅ Create new exam form
- ✅ Edit exam details
- ✅ Publish exam
- ✅ Delete exam
- ✅ Navigate to marks entry
- ✅ Navigate to report cards

**Entity Usage**: Exam CRUD operations

**Key Props**: None (uses React Router params)

**Dependencies**:
- `useExams` - Fetch exams list
- `useCreateExam` - Create exam
- `useUpdateExam` - Update exam
- `usePublishExam` - Publish exam
- `useDeleteExam` - Delete exam

### 2. MarksPage.tsx
**Purpose**: Enter and manage student marks

**Features**:
- ✅ Select exam and class
- ✅ Display all students and subjects
- ✅ Enter marks for each student-subject combination
- ✅ Mark students as absent
- ✅ Save marks as draft
- ✅ Submit marks (triggers report card generation)
- ✅ Real-time statistics (marked/unmarked count)

**Entity Usage**: StudentMarks CRUD, status tracking

**Key Props**: `examId` from URL params

**Dependencies**:
- `useMarksEntryForm` - Get student/subject data
- `useSaveMarks` - Save marks draft
- `useSubmitMarks` - Submit marks

### 3. ReportCardsPage.tsx
**Purpose**: View and manage report cards

**Features**:
- ✅ List all report cards for exam
- ✅ Filter by status (pass/fail)
- ✅ Sort by class position, name, or percentage
- ✅ Pagination
- ✅ View detailed report card in modal
- ✅ Download report card as PDF
- ✅ View subject-wise marks breakdown

**Entity Usage**: StudentReportCard queries

**Key Props**: `examId` from URL params

**Dependencies**:
- `useExamReportCards` - Get report cards list
- `useReportCard` - Get detailed report card
- `useExportReportCardPdf` - Export PDF

## Hooks Documentation

### useExamHooks.ts

```typescript
// Get all exams with pagination
const { data, isLoading, error } = useExams(page, pageSize);

// Get single exam details
const { data, isLoading, error } = useExam(examId);

// Create exam
const { mutateAsync, isPending } = useCreateExam();
await mutateAsync(createExamData);

// Update exam
const { mutateAsync, isPending } = useUpdateExam();
await mutateAsync({ examId, data });

// Publish exam
const { mutateAsync, isPending } = usePublishExam();
await mutateAsync(examId);

// Delete exam
const { mutateAsync, isPending } = useDeleteExam();
await mutateAsync(examId);
```

### useMarksHooks.ts

```typescript
// Get marks entry form
const { data, isLoading, error } = useMarksEntryForm(examId, classId);

// Get student marks
const { data, isLoading, error } = useStudentMarks(studentId, examId);

// Get class marks with pagination
const { data, isLoading, error } = useClassMarks(classId, examId, page, pageSize);

// Save marks (draft)
const { mutateAsync, isPending } = useSaveMarks();
await mutateAsync(saveMarksData);

// Submit marks
const { mutateAsync, isPending } = useSubmitMarks();
await mutateAsync({ examId, classId });
```

### useReportCardHooks.ts

```typescript
// Get specific report card
const { data, isLoading, error } = useReportCard(examId, studentId);

// Get exam report cards with filters
const { data, isLoading, error } = useExamReportCards(
  examId,
  classId,
  status,
  sortBy,
  sortOrder,
  page,
  pageSize
);

// Get student report cards
const { data, isLoading, error } = useStudentReportCards(studentId, page, pageSize);

// Export report card as PDF
const { mutateAsync, isPending } = useExportReportCardPdf();
await mutateAsync({ examId, studentId });
```

### useGradeHooks.ts

```typescript
// Get all grade configurations
const { data, isLoading, error } = useGradeConfigurations();

// Update grade configurations
const { mutateAsync, isPending } = useUpdateGradeConfigurations();
await mutateAsync(gradeconfigData);
```

## API Service Layer (examApi.ts)

The `examApi.ts` file contains all HTTP client methods organized by domain:

```typescript
// Access APIs
examApi.exam.createExam(data)
examApi.exam.getExams(page, pageSize)
examApi.exam.getExamById(examId)
examApi.exam.updateExam(examId, data)
examApi.exam.publishExam(examId)
examApi.exam.deleteExam(examId)

examApi.marks.getMarksEntryForm(examId, classId)
examApi.marks.saveMarks(data)
examApi.marks.getStudentMarks(studentId, examId)
examApi.marks.getClassMarks(classId, examId, page, pageSize)
examApi.marks.submitMarks(examId, classId)

examApi.reportCard.getReportCard(examId, studentId)
examApi.reportCard.getExamReportCards(examId, classId, status, sortBy, sortOrder, page, pageSize)
examApi.reportCard.getStudentReportCards(studentId, page, pageSize)
examApi.reportCard.exportReportCardPdf(examId, studentId)

examApi.grades.getGradeConfigurations()
examApi.grades.updateGradeConfiguration(data)
```

## Integration Steps

### Step 1: Update routing in App.tsx

```typescript
import { ExamsPage } from './pages/ExamsPage';
import { MarksPage } from './pages/MarksPage';
import { ReportCardsPage } from './pages/ReportCardsPage';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        {/* ... other routes ... */}
        <Route path="/exams" element={<ExamsPage />} />
        <Route path="/marks/:examId" element={<MarksPage />} />
        <Route path="/report-cards/:examId" element={<ReportCardsPage />} />
      </Routes>
    </BrowserRouter>
  );
}
```

### Step 2: Ensure axios is installed

```bash
npm install axios @tanstack/react-query
```

### Step 3: Set API URL environment variable

Create `.env.local` file:
```
VITE_API_URL=http://localhost:5000/api
```

### Step 4: Setup React Query Provider

In `main.tsx`:
```typescript
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </React.StrictMode>,
);
```

### Step 5: Add CSS import in App.tsx

```typescript
import './styles/pages.css';
```

## Data Flow

### Exam Creation Flow
```
ExamsPage Form
    ↓
useCreateExam() mutation
    ↓
examApi.exam.createExam()
    ↓
POST /api/exams
    ↓
Backend Handler: CreateExamCommandHandler
    ↓
Save to database
    ↓
Cache updated, list refetched
```

### Marks Entry & Submission Flow
```
MarksPage (Student marks table)
    ↓
useSaveMarks() / useSubmitMarks()
    ↓
examApi.marks.saveMarks() / submitMarks()
    ↓
POST /api/marks/save or /api/marks/submit
    ↓
Backend Handler: SaveStudentMarksCommandHandler / SubmitMarksCommandHandler
    ↓
Save/Submit marks, trigger report generation
    ↓
Cache invalidated, report cards updated
```

### Report Card Display Flow
```
ReportCardsPage (List view)
    ↓
useExamReportCards()
    ↓
examApi.reportCard.getExamReportCards()
    ↓
GET /api/report-cards/exam
    ↓
Backend Handler: GetExamReportCardsQueryHandler
    ↓
Fetch from database with filtering/sorting
    ↓
Display in table with pagination
    ↓
User clicks "View Details"
    ↓
useReportCard() loads detailed card
    ↓
Display in modal
```

## Caching Strategy

React Query caching is configured as follows:

| Entity | Stale Time | GC Time | Notes |
|--------|-----------|---------|-------|
| Exams | 5 min | 10 min | Listed exams |
| Exam Details | 5 min | 10 min | Single exam |
| Marks Entry Form | 5 min | 10 min | Students/subjects |
| Student Marks | 3 min | 10 min | Individual marks |
| Class Marks | 3 min | 10 min | Class marks list |
| Report Cards | 10 min | 20 min | List/detail views |
| Grade Configs | 1 hour | 24 hour | Rarely changed |

When mutations occur (save, submit, create), related query keys are invalidated to trigger refetches.

## Error Handling

All components include error handling:

```typescript
// Display error message
if (error) {
  return <div className="error">Error loading data: {error.message}</div>;
}

// From API utility
const errorMessage = getErrorMessage(error); // Returns human-readable message
```

## Loading States

All mutations and queries show loading states:

```typescript
<button disabled={mutation.isPending}>
  {mutation.isPending ? "Saving..." : "Save"}
</button>
```

## Styling

All components use the `pages.css` stylesheet which provides:
- ✅ Responsive grid layouts
- ✅ Color scheme and themes
- ✅ Table styling
- ✅ Form styling
- ✅ Modal styling
- ✅ Badge and status indicators
- ✅ Mobile-friendly breakpoints

## Testing

Example unit test for ExamsPage:

```typescript
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ExamsPage } from './ExamsPage';

describe('ExamsPage', () => {
  it('should render exams list', async () => {
    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <ExamsPage />
        </BrowserRouter>
      </QueryClientProvider>
    );

    const heading = await screen.findByText('Exams Management');
    expect(heading).toBeInTheDocument();
  });
});
```

## Production Checklist

Before deploying to production:

- [ ] Update API URL: `VITE_API_URL=https://api.yourdomain.com/api`
- [ ] Add authentication token handling in examApi.ts
- [ ] Configure CORS if API on different domain
- [ ] Test all create/update/delete operations
- [ ] Test PDF export functionality
- [ ] Verify pagination with large datasets
- [ ] Add error analytics/logging
- [ ] Performance test with 1000+ records
- [ ] Test on mobile devices
- [ ] Add form validation feedback
- [ ] Setup error boundary component

## Known Limitations

1. **Stub Implementations**: Backend handlers are still stubs and need real EF Core query logic
2. **No PDF Generation**: PDF export is a placeholder - needs actual PDF library integration on backend
3. **No Concurrent Edits**: No conflict detection if multiple users edit same exam
4. **Form Validation**: Frontend validation only - backend validation also needed

## Next Steps

1. **Backend Handler Implementation**: Add actual EF Core logic to handlers
2. **Integration Testing**: Create integration tests for all endpoints
3. **PDF Generation**: Implement PDF export on backend
4. **Grade Management Page**: Create UI for grade configuration  
5. **Analytics Dashboard**: Add exam analytics and reporting
6. **Bulk Import**: CSV/Excel import for marks
7. **Email Notifications**: Notify students of report card generation

## Support

For issues or questions:
1. Check the error messages in browser console
2. Verify backend API is running (http://localhost:5000)
3. Check `examApi.ts` for endpoint definitions
4. Review React Query docs for hook usage patterns

---

**Created**: Phase 1 Frontend Implementation
**Backend Status**: Phase 1 Backend Compilation Successful ✅
**Frontend Status**: All components created and ready for integration

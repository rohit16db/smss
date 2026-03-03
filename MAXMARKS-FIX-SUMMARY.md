# Max Marks and Percentage Display Fix - Summary

## Problem Identified
Max marks and percentage were displaying as **0** in both:
- View Report Card (API response)
- Download Report Card PDF

## Root Cause
Three report card handlers were using `.Include(m => m.ExamSubject)` followed by `.Select()` to project to DTOs. This pattern causes EF Core to ignore the Include statement, resulting in null navigation properties and missing data.

```csharp
// ❌ BROKEN PATTERN
var subjectWiseMarks = await _context.StudentMarks
    .Where(m => m.ExamId == request.ExamId && m.StudentId == request.StudentId)
    .Include(m => m.ExamSubject)  // ← Ignored due to Select below
    .Select(m => new SubjectReportCardDto
    {
        MaxMarks = m.ExamSubject.MaxMarks,  // ← ExamSubject is null
        ...
    })
    .ToListAsync(cancellationToken);
```

## Solution Applied
Replaced all three handlers with explicit LINQ JOIN queries to ensure proper data loading:

```csharp
// ✅ FIXED PATTERN
var subjectWiseMarks = await (from mark in _context.StudentMarks
    where mark.ExamId == request.ExamId && mark.StudentId == request.StudentId
    join examSubject in _context.ExamSubjects 
        on new { mark.ExamId, mark.SubjectId } equals new { examSubject.ExamId, examSubject.SubjectId }
    join subject in _context.Subjects on examSubject.SubjectId equals subject.Id
    select new SubjectReportCardDto
    {
        SubjectId = mark.SubjectId,
        SubjectName = subject.Name,
        MaxMarks = examSubject.MaxMarks,  // ✅ Now properly loaded
        Obtained = mark.MarksObtained ?? 0,
        Percentage = mark.MarksObtained.HasValue && examSubject.MaxMarks > 0 
            ? (mark.MarksObtained.Value / examSubject.MaxMarks) * 100 
            : 0,
        Grade = ""
    })
    .ToListAsync(cancellationToken);
```

## Handlers Fixed

### 1. **GetReportCardQueryHandler** (lines 40-57)
- Fetches report card for API view endpoint
- Used by: `GET /exams/{examId}/students/{studentId}/report-card`

### 2. **GetReportCardByIdQueryHandler** (lines 178-198)
- Fetches report card by report card ID
- Used by: `GET /report-cards/{cardId}`

### 3. **ExportReportCardPdfQueryHandler** (lines 244-257)
- Generates PDF report card
- Used by: `POST /report-cards/{cardId}/export-pdf`

## Test Verification

### Build Status
✅ **Build Succeeded** with 0 errors

### Expected Results After Fix
1. **View Report Card API Response**: MaxMarks and Percentage will display correctly
2. **PDF Download**: MaxMarks column will show actual values instead of 0
3. **Percentage Calculation**: Will correctly compute percentage based on MaxMarks

### To Verify
1. Create an exam with subjects and max marks
2. Enter student marks
3. Generate/view report card
4. Confirm:
   - MaxMarks shows correct values (e.g., 50, 100)
   - Percentage calculates correctly
   - PDF export displays both properly

## Files Modified
- `d:\practice\SMS\backend\src\SMS.Application\Features\Exams\Handlers\ReportCardQueryHandlers.cs`
  - GetReportCardQueryHandler
  - GetReportCardByIdQueryHandler
  - ExportReportCardPdfQueryHandler (previously fixed)

## Technical Notes
- Used explicit SQL JOIN pattern instead of Include+Select
- JOINs StudentMarks → ExamSubjects → Subjects
- Projects directly to DTO to avoid navigation property issues
- Maintains backward compatibility with existing API contracts

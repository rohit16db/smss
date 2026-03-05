# Report Card / Student Transcript - School Settings Integration

## Overview
Integrated **dynamic school settings** into student report cards (both view and PDF export) to replace hardcoded placeholder values with actual school information from the database.

## Changes Made

### 1. **DTO Enhancements**
📁 **File**: `backend/src/SMS.Application/Features/Exams/DTOs/ReportCardDtos.cs`

#### ReportCardDto
Added school settings fields for detailed report card view:
```csharp
// School Settings
public string SchoolName { get; set; } = "School Management System";
public string SchoolAddress { get; set; } = "123 Education Street, City";
public string SchoolPhone { get; set; } = "+91-XXXX-XXXX";
public string? SchoolEmail { get; set; }
public string? SchoolWebsite { get; set; }
public string? SchoolCode { get; set; }
```

#### ReportCardListDto
Added school settings fields for list/grid views:
```csharp
// School Settings
public string SchoolName { get; set; } = "School Management System";
public string SchoolAddress { get; set; } = "123 Education Street, City";
public string SchoolPhone { get; set; } = "+91-XXXX-XXXX";
```

### 2. **Query Handler Updates**
📁 **File**: `backend/src/SMS.Application/Features/Exams/Handlers/ReportCardQueryHandlers.cs`

#### GetReportCardQueryHandler
- **Fetches**: Active school settings from database
- **Returns**: ReportCardDto with populated school fields
- **Used by**: Detailed report card view endpoint

```csharp
// Get active school settings
var school = await _context.Schools
    .Where(s => s.IsActive)
    .FirstOrDefaultAsync(cancellationToken);

// Populate in DTO
SchoolName = school?.Name ?? "School Management System",
SchoolAddress = !string.IsNullOrEmpty(school?.Address) 
    ? $"{school.Address}, {school.City}" 
    : "123 Education Street, City",
SchoolPhone = school?.PhoneNumber ?? "+91-XXXX-XXXX",
SchoolEmail = school?.EmailAddress,
SchoolWebsite = school?.Website,
SchoolCode = school?.Code
```

#### GetExamReportCardsQueryHandler
- **Fetches**: School settings once for all cards in exam
- **Populates**: Each report card in the list
- **Used by**: Exam report cards list/grid view
  
```csharp
// Get active school settings
var school = await _context.Schools
    .Where(s => s.IsActive)
    .FirstOrDefaultAsync(cancellationToken);

// Add to each report card
foreach (var card in reportCards)
{
    card.SchoolName = school?.Name ?? "School Management System";
    card.SchoolAddress = !string.IsNullOrEmpty(school?.Address) 
        ? $"{school.Address}, {school.City}" 
        : "123 Education Street, City";
    card.SchoolPhone = school?.PhoneNumber ?? "+91-XXXX-XXXX";
}
```

#### GetStudentReportCardsQueryHandler
- **Fetches**: School settings for all student's report cards
- **Populates**: Each report card with school info
- **Used by**: Student transcript/history view

Same pattern as GetExamReportCardsQueryHandler.

#### GetReportCardByIdQueryHandler
- **Fetches**: School settings along with report card details
- **Returns**: ReportCardDto with all school fields
- **Used by**: Report card detail view by ID

#### ExportReportCardPdfQueryHandler
**Major changes to PDF generation:**

**Before** ❌:
```csharp
inner.Item().Text("STUDENT REPORT CARD").FontSize(28).Bold().FontColor("#FFFFFF");
inner.Item().PaddingTop(5).Text("School Management System").FontSize(12).FontColor("#D1D5DB");
// Hardcoded school name + no address/phone
```

**After** ✅:
```csharp
// Get active school settings
var school = await _context.Schools
    .Where(s => s.IsActive)
    .FirstOrDefaultAsync(cancellationToken);

var schoolName = school?.Name ?? "School Management System";
var schoolAddress = !string.IsNullOrEmpty(school?.Address) 
    ? $"{school.Address}, {school.City}" 
    : "";
var schoolPhone = school?.PhoneNumber ?? "";

// Dynamic PDF Header
inner.Item().Text("STUDENT REPORT CARD").FontSize(28).Bold().FontColor("#FFFFFF");
inner.Item().PaddingTop(5).Text(schoolName).FontSize(12).FontColor("#D1D5DB");
if (!string.IsNullOrEmpty(schoolAddress))
    inner.Item().PaddingTop(2).Text(schoolAddress).FontSize(9).FontColor("#9CA3AF");
if (!string.IsNullOrEmpty(schoolPhone))
    inner.Item().PaddingTop(2).Text($"Phone: {schoolPhone}").FontSize(9).FontColor("#9CA3AF");
```

## API Endpoints Affected

| Endpoint | Query Handler | Receives School Info |
|----------|---------------|---------------------|
| `GET /api/v1/reportcards/{examId}/{studentId}` | GetReportCardQueryHandler | ✅ Yes |
| `GET /api/v1/reportcards/exam/{examId}` | GetExamReportCardsQueryHandler | ✅ Yes (each card) |
| `GET /api/v1/reportcards/student/{studentId}` | GetStudentReportCardsQueryHandler | ✅ Yes (each card) |
| `GET /api/v1/reportcards/{cardId}` | GetReportCardByIdQueryHandler | ✅ Yes |
| `POST /api/v1/reportcards/{cardId}/export-pdf` | ExportReportCardPdfQueryHandler | ✅ Yes (in PDF) |

## Data Flow

### Report Card View
```
GET /reportcards/{examId}/{studentId}
    ↓
GetReportCardQueryHandler
    ├─ Fetch StudentReportCard with Exam & Student data
    ├─ Fetch StudentMarks and ExamSubjects
    ├─ ✅ NEW: Fetch active School settings
    └─ Return ReportCardDto with school data
        ↓
        Frontend displays:
        - School Name
        - School Address
        - School Phone
        - School Email (optional)
        - School Website (optional)
        - School Code (optional)
```

### Report Card PDF Export
```
POST /reportcards/{cardId}/export-pdf
    ↓
ExportReportCardPdfQueryHandler
    ├─ Fetch StudentReportCard
    ├─ Fetch StudentMarks and ExamSubjects
    ├─ ✅ NEW: Fetch active School settings
    └─ Generate PDF with dynamic school header
        ├─ School Name
        ├─ School Address (if set)
        ├─ School Phone (if set)
        └─ Student Report Content...
        ↓
        PDF returned with dynamic school branding
```

## PDF Header Examples

### Before ❌
```
STUDENT REPORT CARD
School Management System
```

### After ✅
```
STUDENT REPORT CARD
St. Mary's Academy
45 Educational Lane, Mumbai
Phone: +91-9876543210
```

## Database Integration

The implementation uses the existing **Schools** table:

**Fields Used:**
- `name` - School name
- `address` - Street address
- `city` - City/District
- `phone_number` - Contact phone
- `email_address` - Email (optional, new)
- `website` - Website URL (optional, new)
- `code` - School registration code (optional, new)
- `is_active` - Filter active school

**Query Pattern:**
```csharp
var school = await _context.Schools
    .Where(s => s.IsActive)
    .FirstOrDefaultAsync(cancellationToken);
```

## Frontend Integration

The frontend can now use the new school fields from the API response:

### Report Card List (Grid/Table View)
```json
{
  "id": "...",
  "studentName": "John Doe",
  "examName": "Mid Term",
  "percentage": 85.5,
  "schoolName": "St. Mary's Academy",      // ✅ NEW
  "schoolAddress": "45 Lane, Mumbai",      // ✅ NEW
  "schoolPhone": "+91-9876543210"          // ✅ NEW
}
```

### Report Card Detail View
```json
{
  "studentName": "John Doe",
  "className": "Class X-A",
  "subjectMarks": [...],
  "summary": {...},
  "schoolName": "St. Mary's Academy",      // ✅ NEW
  "schoolAddress": "45 Lane, Mumbai",      // ✅ NEW
  "schoolPhone": "+91-9876543210",         // ✅ NEW
  "schoolEmail": "info@marysacademy.edu",  // ✅ NEW
  "schoolWebsite": "www.marysacademy.edu", // ✅ NEW
  "schoolCode": "CBSE-12345"               // ✅ NEW
}
```

## Testing

### 1. Verify School Settings Configured
- Go to Settings page in frontend
- Ensure school information is complete:
  - Name ✓
  - Address + City ✓
  - Phone ✓
  - Email (optional)
  - Website (optional)
  - School Code (optional)

### 2. Test List Views
- Navigate to exam report cards → School info displayed ✓
- Navigate to student report cards → School info displayed ✓
- Check grid/table shows school name and address ✓

### 3. Test Detail Views
- Open individual report card → All school fields visible ✓
- Email and website shown if configured ✓

### 4. Test PDF Export
- Generate report card PDF → PDF header shows:
  - ✓ Dynamic school name
  - ✓ Dynamic address (if set)
  - ✓ Dynamic phone (if set)
  - ✓ Professional formatting

### 5. Test Fallbacks
- Remove or inactivate school settings → Defaults used ✓
- No errors in PDF generation ✓
- API still returns standard responses ✓

## Fallback Behavior

If school settings are not configured:

| Field | Fallback |
|-------|----------|
| SchoolName | "School Management System" |
| SchoolAddress | "123 Education Street, City" |
| SchoolPhone | "+91-XXXX-XXXX" |
| SchoolEmail | Empty string (optional field) |
| SchoolWebsite | Empty string (optional field) |
| SchoolCode | Empty string (optional field) |

## Related Configuration

### School Settings Management
- **API Endpoint**: `PUT /api/v1/settings/school`
- **Frontend Page**: SettingsPage.tsx
- **Controller**: SettingsController.cs
- **Handler**: UpdateSchoolSettingsCommandHandler

Any changes to school information automatically reflect in:
- ✅ New report cards generated
- ✅ New PDFs exported
- ✅ New API responses

## Future Enhancements

The school settings data is now available for:
1. **School Logo in PDF** - Use `schools.logo_image`
2. **Email Footer** - Include school email/website
3. **School Branding** - Use `primary_color`, `secondary_color`, `accent_color`
4. **Header/Footer Text** - Use `header_text`, `footer_text` fields
5. **Multi-School Support** - Each student linked to their school
6. **School Comparison Reports** - Cross-school performance analysis

## Summary

✅ **All Report Card Views** - Now display dynamic school settings
✅ **PDF Export** - Enhanced header with actual school information
✅ **List Endpoints** - School info available in all list responses
✅ **Fallback Handling** - Graceful defaults if settings not configured
✅ **No Breaking Changes** - New fields are optional, backward compatible with existing clients

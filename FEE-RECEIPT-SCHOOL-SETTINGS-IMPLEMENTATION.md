# Fee Receipt PDF - School Settings Integration

## Overview
Updated the fee receipt PDF generation to use **dynamic school settings** from the database instead of hardcoded placeholder values.

## Changes Made

### 1. **Query Handler Enhancement** 
📁 **File**: `backend/src/SMS.Application/Features/Fees/Handlers/QueryHandlers/FeeQueryHandlers.cs`

**Added Database Query for School Settings**:
```csharp
// Get active school settings for the receipt
var school = await _context.Schools
    .Where(s => s.IsActive)
    .FirstOrDefaultAsync(cancellationToken);
```

**Updated FeeReceiptDto Population**:
- `SchoolName`: Now fetches from `school?.Name` (fallback: "School Management System")
- `SchoolAddress`: Combines `school?.Address` and `school?.City` (fallback: "123 Education Street, City")
- `SchoolPhone`: Fetches from `school?.PhoneNumber` (fallback: "+91-XXXX-XXXX")
- `SchoolEmail`: Fetches from `school?.EmailAddress` (new field)
- `SchoolWebsite`: Fetches from `school?.Website` (new field)
- `SchoolCode`: Fetches from `school?.Code` (new field)

### 2. **DTO Expansion**
📁 **File**: `backend/src/SMS.Application/Features/Fees/DTOs/FeeDto.cs`

**Added New Fields to FeeReceiptDto**:
```csharp
// School Settings
public string SchoolName { get; set; } = "School Management System";
public string SchoolAddress { get; set; } = "123 Education Street, City";
public string SchoolPhone { get; set; } = "+91-XXXX-XXXX";
public string? SchoolEmail { get; set; }           // NEW
public string? SchoolWebsite { get; set; }         // NEW
public string? SchoolCode { get; set; }            // NEW
```

## How It Works

### Data Flow
```
GET /payments/{paymentId}/receipt (API Endpoint)
    ↓
GenerateFeeReceiptPdfCommand
    ↓
GetFeeReceiptDataQueryHandler
    ├─ Fetches FeePayment with Student, FeeStructure, Section
    ├─ ✅ NEW: Fetches School settings from database
    └─ Returns populated FeeReceiptDto with actual school info
        ↓
        QuestPDF Document
            └─ Renders PDF with dynamic school information
```

## PDF Display

The PDF header now shows:
- **School Name**: From `schools.name` (e.g., "St. Mary's Academy")
- **School Address**: From `schools.address` + `schools.city` (e.g., "123 Main Street, New York")
- **School Phone**: From `schools.phone_number` (e.g., "+91-9876543210")

### Example Before ❌
```
School Management System
123 Education Street, City
Phone: +91-XXXX-XXXX
```

### Example After ✅
```
St. Mary's Academy
45 Educational Lane, Mumbai
Phone: +91-9876543210
```

## Database Integration

The implementation uses the existing **Schools** table:
- **Table**: `schools`
- **Retrieved from**: Active school record (`schools.is_active = true`)
- **Fields Used**:
  - `name` - School name
  - `address` - School address
  - `city` - School city
  - `phone_number` - Contact number
  - `email_address` - Email (optional, new)
  - `website` - Website URL (optional, new)
  - `code` - School code (optional, new)

## Future Enhancements

The new DTO fields can be used to enhance the PDF further:
- Display school email and website in email footer
- Show school code/registration number
- Add school logo (already available in `schools.logo_image`)
- Include additional contact information

## Testing

To test the implementation:

1. **Ensure School Settings are configured**:
   - Go to SettingsPage in frontend
   - Verify school name, address, phone are set
   - The values should now appear in generated receipts

2. **Generate a Fee Receipt**:
   - Navigate to a student's fee payment
   - Click "Generate Receipt" or download PDF
   - Verify the header shows actual school information

3. **Verify Fallbacks**:
   - If school settings are not configured, default values are used
   - No errors should occur in PDF generation

## API Endpoint Affected

- `GET /api/v1/payments/{paymentId}/receipt` - Returns PDF with updated school settings

## Configuration Management

School settings can be updated via:
- **Settings Controller**: `PUT /api/v1/settings/school`
- **Frontend**: Settings page (SettingsPage.tsx)
- **Database**: Direct modification of `schools` table

Any changes to school information in settings will automatically reflect in newly generated fee receipts.

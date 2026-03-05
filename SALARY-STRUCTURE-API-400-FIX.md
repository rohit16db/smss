# Salary Structure API - 400 Bad Request Fix

## Problem
API endpoint `POST http://localhost:5208/api/v1/salarystructure` returns **400 Bad Request** when creating a teacher salary structure.

## Root Causes for 400 Bad Request

### 1. **DateOnly Format Issue** ⚠️ (Most Common)
`EffectiveFromDate` must be sent as **`yyyy-MM-dd`** (ISO 8601 date format, NOT datetime).

**❌ Wrong:**
```json
{
  "name": "Senior Teacher",
  "baseSalary": 50000,
  "effectiveFromDate": "2024-01-15T00:00:00Z",  // ❌ ISO datetime format
  "effectiveToDate": "2025-12-31T23:59:59Z"
}
```

**✅ Correct:**
```json
{
  "name": "Senior Teacher",
  "baseSalary": 50000,
  "effectiveFromDate": "2024-01-15",  // ✅ ISO date format only
  "effectiveToDate": "2025-12-31"
}
```

### 2. **Missing Required Fields**
Must provide: `Name`, `BaseSalary`, `EffectiveFromDate`

**❌ Wrong:**
```json
{
  "baseSalary": 50000,
  "effectiveFromDate": "2024-01-15"
  // Missing: name
}
```

**✅ Correct:**
```json
{
  "name": "Senior Teacher",
  "baseSalary": 50000,
  "effectiveFromDate": "2024-01-15"
}
```

### 3. **BaseSalary Validation**
Must be **greater than 0** (minimum 0.01).

**❌ Wrong:**
```json
{
  "name": "Teacher",
  "baseSalary": 0,  // ❌ Must be > 0
  "effectiveFromDate": "2024-01-15"
}
```

**✅ Correct:**
```json
{
  "name": "Teacher",
  "baseSalary": 50000.00,  // ✅ > 0
  "effectiveFromDate": "2024-01-15"
}
```

### 4. **Negative Allowances or Deductions**
All allowances and deductions must be **>= 0** (non-negative).

**❌ Wrong:**
```json
{
  "name": "Teacher",
  "baseSalary": 50000,
  "hra": -5000,  // ❌ Cannot be negative
  "effectiveFromDate": "2024-01-15"
}
```

**✅ Correct:**
```json
{
  "name": "Teacher",
  "baseSalary": 50000,
  "hra": 5000,  // ✅ >= 0
  "effectiveFromDate": "2024-01-15"
}
```

### 5. **String Length Limits**
- `Name`: Maximum 100 characters
- `Description`: Maximum 500 characters
- `ApplicableQualifications`: Maximum 500 characters

**❌ Wrong:**
```json
{
  "name": "This is a very very very very very very very very very very very very very very long name exceeding 100 chars",
  "baseSalary": 50000,
  "effectiveFromDate": "2024-01-15"
}
```

## Complete Valid Request Examples

### Example 1: Minimum Required Fields
```json
{
  "name": "Junior Teacher",
  "baseSalary": 40000.00,
  "effectiveFromDate": "2024-01-15"
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Junior Teacher",
  "description": null,
  "baseSalary": 40000.00,
  "hra": 0.00,
  "da": 0.00,
  "medicalAllowance": 0.00,
  "conveyanceAllowance": 0.00,
  "otherAllowances": 0.00,
  "standardDeduction": 0.00,
  "grossSalary": 40000.00,
  "totalAllowances": 0.00,
  "minExperienceYears": 0,
  "applicableQualifications": null,
  "effectiveFromDate": "2024-01-15",
  "effectiveToDate": null,
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### Example 2: Complete Detailed Structure
```json
{
  "name": "Senior Teacher - Science",
  "description": "Salary structure for experienced science teachers with 5+ years",
  "baseSalary": 75000.00,
  "hra": 15000.00,
  "da": 10000.00,
  "medicalAllowance": 2000.00,
  "conveyanceAllowance": 2000.00,
  "otherAllowances": 3000.00,
  "standardDeduction": 5000.00,
  "minExperienceYears": 5,
  "applicableQualifications": "B.Sc, B.Ed, M.A",
  "effectiveFromDate": "2024-04-01",
  "effectiveToDate": "2025-03-31"
}
```

**Calculation:**
- Total Allowances = 15000 + 10000 + 2000 + 2000 + 3000 = 32000
- Gross Salary = 75000 + 32000 - 5000 = 102000

### Example 3: Using Postman/curl

**PowerShell/curl:**
```powershell
$body = @{
    name = "Teacher Salary"
    baseSalary = 50000
    hra = 5000
    da = 3000
    medicalAllowance = 1000
    effectiveFromDate = "2024-01-15"
} | ConvertTo-Json

curl -X POST "http://localhost:5208/api/v1/salarystructure" `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer YOUR_TOKEN" `
  -d $body
```

**Using Invoke-WebRequest:**
```powershell
$param = @{
    Uri = "http://localhost:5208/api/v1/salarystructure"
    Method = "POST"
    Headers = @{
        "Content-Type" = "application/json"
        "Authorization" = "Bearer YOUR_TOKEN"
    }
    Body = @{
        name = "Teacher Salary"
        baseSalary = 50000
        hra = 5000
        effectiveFromDate = "2024-01-15"
    } | ConvertTo-Json
}

Invoke-WebRequest @param
```

## Validation Rules Summary

| Field | Required | Type | Validation |
|-------|----------|------|-----------|
| `name` | ✅ Yes | string | Min: 1, Max: 100 chars |
| `description` | ❌ No | string\| null | Max: 500 chars |
| `baseSalary` | ✅ Yes | decimal | Must be > 0 (minimum 0.01) |
| `hra` | ❌ No | decimal | Must be >= 0 |
| `da` | ❌ No | decimal | Must be >= 0 |
| `medicalAllowance` | ❌ No | decimal | Must be >= 0 |
| `conveyanceAllowance` | ❌ No | decimal | Must be >= 0 |
| `otherAllowances` | ❌ No | decimal | Must be >= 0 |
| `standardDeduction` | ❌ No | decimal | Must be >= 0 |
| `minExperienceYears` | ❌ No | integer | Must be >= 0 |
| `applicableQualifications` | ❌ No | string\| null | Max: 500 chars |
| `effectiveFromDate` | ✅ Yes | date | Format: **yyyy-MM-dd** |
| `effectiveToDate` | ❌ No | date\| null | Format: **yyyy-MM-dd** |

## Debugging 400 Error

### Step 1: Check Response Body
The API returns detailed error messages:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "...",
  "errors": {
    "Name": [
      "Name is required"
    ],
    "BaseSalary": [
      "Base salary must be greater than 0"
    ],
    "EffectiveFromDate": [
      "The EffectiveFromDate field is required."
    ]
  }
}
```

### Step 2: Verify Date Format
If you see issues with `effectiveFromDate`, ensure:
- Format is `yyyy-MM-dd` (example: `2024-01-15`)
- NOT `yyyy-MM-ddTHH:mm:ssZ` (that's datetime)
- NO time component

### Step 3: Check Content-Type
Add header:
```
Content-Type: application/json
```

### Step 4: Check Authorization
The endpoint requires `SalaryManageAccess` policy. Ensure your token has this permission.

## Field Defaults
These fields default to 0 if not provided:
- `HRA` → 0
- `DA` → 0
- `MedicalAllowance` → 0
- `ConveyanceAllowance` → 0
- `OtherAllowances` → 0
- `StandardDeduction` → 0
- `MinExperienceYears` → 0

## Quick Checklist for 400 Error

- ✅ Is `name` provided and not empty?
- ✅ Is `baseSalary` > 0 (not zero)?
- ✅ Is `effectiveFromDate` in format `yyyy-MM-dd`?
- ✅ Are all allowances >= 0 (non-negative)?
- ✅ Is `name` <= 100 characters?
- ✅ Is `description` <= 500 characters (if provided)?
- ✅ Is `Content-Type: application/json` header set?
- ✅ Are you authorized with `SalaryManageAccess` policy?

## Example: Step-by-Step Creation

### Request:
```json
POST /api/v1/salarystructure HTTP/1.1
Host: localhost:5208
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

{
  "name": "Senior Teacher",
  "description": "For teachers with 5+ years experience",
  "baseSalary": 50000.00,
  "hra": 10000.00,
  "da": 8000.00,
  "medicalAllowance": 2000.00,
  "conveyanceAllowance": 1500.00,
  "otherAllowances": 2500.00,
  "standardDeduction": 5000.00,
  "minExperienceYears": 5,
  "applicableQualifications": "B.Sc, B.Ed, M.A",
  "effectiveFromDate": "2024-04-01",
  "effectiveToDate": "2025-03-31"
}
```

### Response (201 Created):
```json
HTTP/1.1 201 Created
Location: /api/v1/salarystructure/550e8400-e29b-41d4-a716-446655440000
Content-Type: application/json

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Senior Teacher",
  "description": "For teachers with 5+ years experience",
  "baseSalary": 50000.00,
  "hra": 10000.00,
  "da": 8000.00,
  "medicalAllowance": 2000.00,
  "conveyanceAllowance": 1500.00,
  "otherAllowances": 2500.00,
  "standardDeduction": 5000.00,
  "grossSalary": 69000.00,
  "totalAllowances": 24000.00,
  "minExperienceYears": 5,
  "applicableQualifications": "B.Sc, B.Ed, M.A",
  "effectiveFromDate": "2024-04-01",
  "effectiveToDate": "2025-03-31",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:42.123Z"
}
```

## Summary

Most 400 errors are due to:
1. **❌ Date format** - Using datetime instead of date (`yyyy-MM-dd`)
2. **❌ Missing fields** - Not providing `name`, `baseSalary`, or `effectiveFromDate`
3. **❌ Invalid values** - `baseSalary` is 0 or negative allowances
4. **❌ String too long** - `name` > 100 chars or `description` > 500 chars

**Solution:** Use the example payloads above with correct date format `yyyy-MM-dd`.

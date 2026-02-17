# API Endpoint Specifications
**Feature**: 002-teacher-fee-attendance  
**Created**: January 12, 2026  
**Phase**: Planning / Design  
**Based on**: [spec.md](../spec.md)  
**API Version**: v1  
**Base URL**: `/api/v1`

---

## Overview

This document defines all REST API endpoints for Teacher, Fee, and Attendance features. All endpoints:
- Require JWT authentication (Bearer token in Authorization header)
- Return JSON responses with consistent error format
- Use HTTP status codes appropriately (200, 201, 204, 400, 401, 403, 404, 409, 500)
- Support pagination for list endpoints (pageNumber, pageSize defaults)
- Are subject to [Authorize] attribute (admin-only in Phase 2)

**Response Format**:
```json
{
  "success": true,
  "data": { /* response body */ },
  "message": "Operation completed successfully",
  "timestamp": "2026-01-12T10:30:00Z"
}
```

**Error Format**:
```json
{
  "success": false,
  "error": {
    "code": "TEACHER_NOT_FOUND",
    "message": "Teacher with ID 'abc-123' was not found",
    "details": "Please verify the teacher ID and try again"
  },
  "timestamp": "2026-01-12T10:30:00Z"
}
```

---

## Teacher Management Endpoints

### 1. Create Teacher

```
POST /api/v1/teachers
```

**Request**:
```json
{
  "firstName": "Priya",
  "lastName": "Kumar",
  "email": "priya.kumar@school.edu",
  "phone": "+91-9876543210",
  "qualification": "M.Sc. Mathematics, B.Ed.",
  "experienceYears": 5,
  "joiningDate": "2026-01-15"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "firstName": "Priya",
    "lastName": "Kumar",
    "email": "priya.kumar@school.edu",
    "phone": "+91-9876543210",
    "qualification": "M.Sc. Mathematics, B.Ed.",
    "experienceYears": 5,
    "joiningDate": "2026-01-15",
    "status": "Active",
    "isActive": true,
    "assignedClasses": [],
    "createdAt": "2026-01-12T10:00:00Z"
  },
  "message": "Teacher created successfully"
}
```

**Validation**:
- firstName, lastName: Required, max 50 chars, no special characters
- email: Required, valid email format, must be unique in system
- phone: Optional, max 20 chars
- qualification: Optional, max 500 chars
- experienceYears: Non-negative integer
- joiningDate: Valid date, not in future

**Status Codes**:
- 201 Created
- 400 Bad Request (validation failure)
- 409 Conflict (email already exists)

---

### 2. Get Teacher by ID

```
GET /api/v1/teachers/{id}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "firstName": "Priya",
    "lastName": "Kumar",
    "email": "priya.kumar@school.edu",
    "phone": "+91-9876543210",
    "qualification": "M.Sc. Mathematics, B.Ed.",
    "experienceYears": 5,
    "joiningDate": "2026-01-15",
    "status": "Active",
    "isActive": true,
    "assignedClasses": [
      {
        "classId": "class-id-1",
        "className": "10A",
        "subjectName": "Mathematics",
        "assignmentDate": "2026-01-15",
        "removalDate": null
      },
      {
        "classId": "class-id-2",
        "className": "10B",
        "subjectName": "Mathematics",
        "assignmentDate": "2026-01-15",
        "removalDate": null
      }
    ],
    "recentAttendance": {
      "presentDays": 19,
      "absentDays": 1,
      "leaveDays": 0,
      "attendancePercentage": 95.0
    },
    "createdAt": "2026-01-12T10:00:00Z",
    "updatedAt": "2026-01-12T10:00:00Z"
  }
}
```

**Status Codes**:
- 200 OK
- 404 Not Found

---

### 3. Get All Teachers (Paginated)

```
GET /api/v1/teachers?pageNumber=1&pageSize=20&isActive=true&searchTerm=priya
```

**Query Parameters**:
- pageNumber: Integer, default 1
- pageSize: Integer, default 20, max 100
- isActive: Boolean, optional (filters by active/inactive status)
- searchTerm: String, optional (searches firstName, lastName, email)
- sortBy: String, optional (joiningDate|name|email, default: name)
- sortOrder: String, optional (asc|desc, default: asc)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440000",
        "firstName": "Priya",
        "lastName": "Kumar",
        "email": "priya.kumar@school.edu",
        "qualification": "M.Sc. Mathematics",
        "experienceYears": 5,
        "joiningDate": "2026-01-15",
        "status": "Active",
        "assignedClassCount": 2
      }
    ],
    "pagination": {
      "pageNumber": 1,
      "pageSize": 20,
      "totalCount": 15,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

**Status Codes**:
- 200 OK

---

### 4. Update Teacher

```
PUT /api/v1/teachers/{id}
```

**Request** (all fields optional):
```json
{
  "firstName": "Priya",
  "lastName": "Kumar",
  "phone": "+91-9876543211",
  "qualification": "M.Sc. Mathematics, B.Ed., M.Ed.",
  "experienceYears": 6,
  "isActive": true
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": { /* updated teacher object */ },
  "message": "Teacher updated successfully"
}
```

**Validation**:
- Same as Create, but all fields optional
- Cannot change email (use separate endpoint if needed)

**Status Codes**:
- 200 OK
- 400 Bad Request
- 404 Not Found

---

### 5. Assign Teacher to Class

```
POST /api/v1/teachers/{id}/assignments
```

**Request**:
```json
{
  "classId": "class-id-1",
  "subjectId": "subject-id-1",
  "assignmentDate": "2026-01-15"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "assignmentId": "assign-id-1",
    "teacherId": "550e8400-e29b-41d4-a716-446655440000",
    "classId": "class-id-1",
    "className": "10A",
    "subjectId": "subject-id-1",
    "subjectName": "Mathematics",
    "assignmentDate": "2026-01-15",
    "removalDate": null
  },
  "message": "Teacher assigned to class successfully"
}
```

**Validation**:
- classId: Must exist and be active
- subjectId: Must exist
- assignmentDate: Valid date, teacher must be active on this date
- No duplicate active assignments for same (teacher, class, subject)
- Teacher cannot be assigned to overlapping time slots (check against class schedule)

**Status Codes**:
- 201 Created
- 400 Bad Request (validation)
- 404 Not Found (teacher, class, or subject)
- 409 Conflict (duplicate assignment)

---

### 6. Remove Teacher from Class

```
DELETE /api/v1/teachers/{id}/assignments/{assignmentId}
```

**Request** (optional):
```json
{
  "removalDate": "2026-06-30"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "assignmentId": "assign-id-1",
    "teacherId": "550e8400-e29b-41d4-a716-446655440000",
    "classId": "class-id-1",
    "className": "10A",
    "removalDate": "2026-06-30"
  },
  "message": "Teacher removed from class successfully"
}
```

**Status Codes**:
- 200 OK
- 404 Not Found

---

## Fee Management Endpoints

### 7. Create Fee Structure

```
POST /api/v1/fee-structures
```

**Request**:
```json
{
  "name": "Regular Monthly 2026",
  "academicYear": 2026,
  "frequency": "monthly",
  "categories": [
    {
      "category": "tuition",
      "amount": 5000
    },
    {
      "category": "transport",
      "amount": 500
    }
  ]
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "id": "fee-struct-id-1",
    "name": "Regular Monthly 2026",
    "academicYear": 2026,
    "frequency": "monthly",
    "totalAmount": 5500,
    "categories": [
      { "category": "tuition", "amount": 5000 },
      { "category": "transport", "amount": 500 }
    ],
    "isActive": true,
    "createdAt": "2026-01-12T10:00:00Z"
  }
}
```

**Validation**:
- name: Required, max 100 chars, should include year
- academicYear: Valid year (>= 2020)
- frequency: One of [monthly, quarterly, yearly]
- categories: At least one category, sum must match totalAmount
- Each category amount > 0

**Status Codes**:
- 201 Created
- 400 Bad Request

---

### 8. Get Fee Structure by ID

```
GET /api/v1/fee-structures/{id}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": "fee-struct-id-1",
    "name": "Regular Monthly 2026",
    "academicYear": 2026,
    "frequency": "monthly",
    "totalAmount": 5500,
    "categories": [
      { "category": "tuition", "amount": 5000 },
      { "category": "transport", "amount": 500 }
    ],
    "isActive": true,
    "studentsAssigned": 145,
    "totalMonthlyCollection": 797500
  }
}
```

**Status Codes**:
- 200 OK
- 404 Not Found

---

### 9. Get All Fee Structures

```
GET /api/v1/fee-structures?pageNumber=1&pageSize=20&academicYear=2026&isActive=true
```

**Query Parameters**:
- pageNumber: Integer, default 1
- pageSize: Integer, default 20
- academicYear: Integer, optional
- isActive: Boolean, optional
- sortBy: String (name|academicYear, default: academicYear DESC)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "fee-struct-id-1",
        "name": "Regular Monthly 2026",
        "academicYear": 2026,
        "frequency": "monthly",
        "totalAmount": 5500,
        "studentsAssigned": 145,
        "isActive": true
      }
    ],
    "pagination": { /* ... */ }
  }
}
```

**Status Codes**:
- 200 OK

---

### 10. Assign Fee Structure to Student

```
POST /api/v1/students/{studentId}/fees
```

**Request**:
```json
{
  "feeStructureId": "fee-struct-id-1",
  "startDate": "2026-01-01",
  "endDate": "2026-12-31",
  "customAmount": null
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "studentFeeId": "stud-fee-id-1",
    "studentId": "student-id-1",
    "studentName": "Rahul Kumar",
    "feeStructureId": "fee-struct-id-1",
    "feeStructureName": "Regular Monthly 2026",
    "frequency": "monthly",
    "totalAmount": 5500,
    "startDate": "2026-01-01",
    "endDate": "2026-12-31",
    "paymentObligation": {
      "periods": 12,
      "periodType": "month",
      "expectedDates": [
        { "period": "January", "dueDate": "2026-01-31", "amount": 5500 },
        { "period": "February", "dueDate": "2026-02-28", "amount": 5500 }
      ]
    },
    "createdAt": "2026-01-12T10:00:00Z"
  }
}
```

**Validation**:
- studentId: Must exist and be active
- feeStructureId: Must exist and be active
- startDate <= endDate
- customAmount: If provided, must be > 0 (for pro-rata amounts)

**Status Codes**:
- 201 Created
- 400 Bad Request
- 404 Not Found (student or fee structure)

---

### 11. Record Fee Payment

```
POST /api/v1/students/{studentId}/fee-payments
```

**Request**:
```json
{
  "studentFeeId": "stud-fee-id-1",
  "amountPaid": 2750,
  "paymentDate": "2026-01-15",
  "receiptNumber": "RCP-2026-0001",
  "paymentMethod": "bank_transfer",
  "notes": "Partial payment for January"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "paymentId": "payment-id-1",
    "studentId": "student-id-1",
    "studentName": "Rahul Kumar",
    "studentFeeId": "stud-fee-id-1",
    "amountPaid": 2750,
    "paymentDate": "2026-01-15",
    "receiptNumber": "RCP-2026-0001",
    "paymentMethod": "bank_transfer",
    "feeStatus": {
      "totalDue": 5500,
      "totalPaid": 2750,
      "balanceRemaining": 2750,
      "status": "Partial",
      "daysOverdue": 0
    },
    "createdAt": "2026-01-12T10:00:00Z"
  }
}
```

**Validation**:
- studentFeeId: Must exist
- amountPaid: Must be > 0
- paymentDate: Valid date, not in future (but may be in past)
- receiptNumber: Unique in system, max 50 chars, required
- paymentMethod: One of [cash, check, bank_transfer]

**Status Codes**:
- 201 Created
- 400 Bad Request
- 404 Not Found
- 409 Conflict (duplicate receipt number)

---

### 12. Get Student Fee Status

```
GET /api/v1/students/{studentId}/fee-status
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "studentId": "student-id-1",
    "studentName": "Rahul Kumar",
    "currentFees": [
      {
        "studentFeeId": "stud-fee-id-1",
        "feeStructureName": "Regular Monthly 2026",
        "frequency": "monthly",
        "totalDue": 5500,
        "totalPaid": 2750,
        "balanceRemaining": 2750,
        "status": "Partial",
        "daysOverdue": 0,
        "periods": [
          {
            "period": "January 2026",
            "dueDate": "2026-01-31",
            "amountDue": 5500,
            "amountPaid": 2750,
            "balanceRemaining": 2750,
            "status": "Partial",
            "daysOverdue": 0
          },
          {
            "period": "February 2026",
            "dueDate": "2026-02-28",
            "amountDue": 5500,
            "amountPaid": 0,
            "balanceRemaining": 5500,
            "status": "Pending",
            "daysOverdue": -47
          }
        ]
      }
    ],
    "summary": {
      "totalFeesDue": 11000,
      "totalPaid": 2750,
      "totalOutstanding": 8250,
      "totalOverdue": 0,
      "overallStatus": "Partial"
    }
  }
}
```

**Status Codes**:
- 200 OK
- 404 Not Found

---

### 13. Get Outstanding Fees Report

```
GET /api/v1/fees/outstanding?sortBy=amount&sortOrder=desc&daysOverdue=30&pageNumber=1&pageSize=50
```

**Query Parameters**:
- sortBy: String (amount|studentName|daysOverdue, default: amount)
- sortOrder: String (asc|desc, default: desc)
- daysOverdue: Integer, optional (filter only fees overdue > N days)
- classFilter: UUID, optional (filter by class)
- pageNumber, pageSize: Pagination

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "studentId": "student-id-5",
        "studentName": "Arjun Singh",
        "className": "10A",
        "totalOutstanding": 16500,
        "daysOverdue": 45,
        "lastPaymentDate": "2025-12-15",
        "daysOutstandingSince": 28
      }
    ],
    "summary": {
      "totalOutstandingAmount": 2345000,
      "totalOutstandingCount": 67,
      "averageDaysOverdue": 18.5
    },
    "pagination": { /* ... */ }
  }
}
```

**Status Codes**:
- 200 OK

---

### 14. Reverse / Adjust Fee Payment

```
POST /api/v1/fee-payments/{paymentId}/reverse
```

**Request**:
```json
{
  "reason": "Duplicate payment - already paid via check",
  "notes": "Original receipt: RCP-2026-0001"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "reversalId": "reversal-id-1",
    "originalPaymentId": "payment-id-1",
    "amountReversed": 2750,
    "reversalDate": "2026-01-13",
    "reason": "Duplicate payment - already paid via check",
    "previousBalance": 2750,
    "newBalance": 5500,
    "auditLog": {
      "reversedBy": "admin@school.edu",
      "reversalTimestamp": "2026-01-13T10:30:00Z"
    }
  }
}
```

**Validation**:
- paymentId: Must exist
- reason: Required, max 500 chars

**Status Codes**:
- 200 OK
- 404 Not Found
- 400 Bad Request

---

## Attendance Management Endpoints

### 15. Mark Student Attendance

```
POST /api/v1/attendance/student
```

**Request** (batch operation allowed):
```json
{
  "classId": "class-id-1",
  "attendanceDate": "2026-01-12",
  "attendance": [
    {
      "studentId": "student-id-1",
      "status": "present"
    },
    {
      "studentId": "student-id-2",
      "status": "absent",
      "reason": "Medical leave with approval"
    },
    {
      "studentId": "student-id-3",
      "status": "leave",
      "reason": "School trip - permission letter on file"
    },
    {
      "studentId": "student-id-4",
      "status": "unexcused"
    }
  ]
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "classId": "class-id-1",
    "className": "10A",
    "attendanceDate": "2026-01-12",
    "totalStudents": 45,
    "presentCount": 42,
    "absentCount": 1,
    "leaveCount": 1,
    "unexcusedCount": 1,
    "attendancePercentage": 93.33,
    "recordedBy": "user@school.edu",
    "recordedAt": "2026-01-12T10:30:00Z",
    "details": [
      {
        "studentId": "student-id-1",
        "studentName": "Rahul Kumar",
        "status": "present",
        "reason": null
      }
    ]
  }
}
```

**Validation**:
- classId: Must exist and be active
- attendanceDate: Valid date, not in future (but allow grace period of 7 days past)
- status: One of [present, absent, leave, unexcused]
- No duplicate attendance for same (student, class, date)

**Status Codes**:
- 201 Created
- 400 Bad Request
- 404 Not Found (class or student)
- 409 Conflict (duplicate attendance)

---

### 16. Update Student Attendance

```
PUT /api/v1/attendance/student/{attendanceId}
```

**Request**:
```json
{
  "status": "present",
  "reason": "Initially marked absent, but student was present",
  "editReason": "Teacher made data entry error - verified with student roll"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "attendanceId": "attend-id-1",
    "studentId": "student-id-1",
    "studentName": "Rahul Kumar",
    "classId": "class-id-1",
    "className": "10A",
    "attendanceDate": "2026-01-12",
    "status": "present",
    "reason": "Initially marked absent, but student was present",
    "originalStatus": "absent",
    "originalReason": "Medical leave",
    "editHistory": [
      {
        "previousStatus": "absent",
        "newStatus": "present",
        "editedBy": "teacher@school.edu",
        "editedAt": "2026-01-12T14:30:00Z",
        "editReason": "Teacher made data entry error - verified with student roll"
      }
    ]
  }
}
```

**Status Codes**:
- 200 OK
- 404 Not Found
- 400 Bad Request

---

### 17. Get Student Attendance Record

```
GET /api/v1/students/{studentId}/attendance?month=2026-01&year=2026
```

**Query Parameters**:
- month: String (YYYY-MM), optional (default: current month)
- year: Integer, optional
- classId: UUID, optional (filter by class)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "studentId": "student-id-1",
    "studentName": "Rahul Kumar",
    "month": "January 2026",
    "summary": {
      "workingDays": 22,
      "presentDays": 20,
      "absentDays": 1,
      "leaveDays": 1,
      "unexcusedDays": 0,
      "attendancePercentage": 90.91,
      "status": "Normal"
    },
    "dailyRecords": [
      {
        "date": "2026-01-01",
        "dayOfWeek": "Thursday",
        "classId": "class-id-1",
        "className": "10A",
        "status": "holiday"
      },
      {
        "date": "2026-01-02",
        "dayOfWeek": "Friday",
        "classId": "class-id-1",
        "className": "10A",
        "status": "present",
        "reason": null,
        "markedBy": "teacher@school.edu"
      },
      {
        "date": "2026-01-03",
        "dayOfWeek": "Saturday",
        "status": "weekend"
      }
    ]
  }
}
```

**Status Codes**:
- 200 OK
- 404 Not Found

---

### 18. Get Class Attendance Summary

```
GET /api/v1/classes/{classId}/attendance-summary?month=2026-01&sortBy=percentage&sortOrder=asc
```

**Query Parameters**:
- month: String (YYYY-MM), optional
- sortBy: String (name|percentage|presentDays, default: name)
- sortOrder: String (asc|desc)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "classId": "class-id-1",
    "className": "10A",
    "month": "January 2026",
    "classSummary": {
      "totalStudents": 45,
      "averageAttendancePercentage": 92.15,
      "highestAttendance": {
        "studentName": "Priya Singh",
        "percentage": 100
      },
      "lowestAttendance": {
        "studentName": "Arjun Kumar",
        "percentage": 72.73,
        "status": "LowAttendance"
      }
    },
    "studentAttendance": [
      {
        "studentId": "student-id-1",
        "studentName": "Rahul Kumar",
        "presentDays": 20,
        "absentDays": 1,
        "leaveDays": 1,
        "unexcusedDays": 0,
        "attendancePercentage": 90.91,
        "status": "Normal"
      }
    ]
  }
}
```

**Status Codes**:
- 200 OK
- 404 Not Found

---

### 19. Mark Teacher Attendance

```
POST /api/v1/attendance/teacher
```

**Request**:
```json
{
  "teacherId": "teacher-id-1",
  "attendanceDate": "2026-01-12",
  "status": "present",
  "reason": null
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "data": {
    "attendanceId": "attend-id-1",
    "teacherId": "teacher-id-1",
    "teacherName": "Priya Kumar",
    "attendanceDate": "2026-01-12",
    "status": "present",
    "reason": null,
    "recordedBy": "admin@school.edu",
    "recordedAt": "2026-01-12T10:30:00Z"
  }
}
```

**Validation**:
- teacherId: Must exist
- attendanceDate: Valid date
- status: One of [present, absent, leave]

**Status Codes**:
- 201 Created
- 400 Bad Request
- 404 Not Found

---

### 20. Get Teacher Attendance Report

```
GET /api/v1/teachers/{teacherId}/attendance-report?year=2026&month=01
```

**Query Parameters**:
- year: Integer, optional
- month: Integer (1-12), optional
- period: String (month|quarter|year, default: month)

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "teacherId": "teacher-id-1",
    "teacherName": "Priya Kumar",
    "period": "January 2026",
    "workingDays": 22,
    "presentDays": 20,
    "absentDays": 1,
    "leaveDays": 1,
    "attendancePercentage": 90.91,
    "bonusEligibility": {
      "minimumRequired": 90,
      "achieved": 90.91,
      "eligible": true,
      "notes": "Meets minimum attendance for full bonus"
    },
    "dailyRecords": [
      {
        "date": "2026-01-01",
        "status": "holiday"
      },
      {
        "date": "2026-01-02",
        "status": "present",
        "reason": null
      }
    ]
  }
}
```

**Status Codes**:
- 200 OK
- 404 Not Found

---

## Dashboard Endpoints

### 21. Get Dashboard Summary

```
GET /api/v1/dashboard/summary
```

**Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "teachers": {
      "totalTeachers": 15,
      "activeTeachers": 14,
      "newJoins": 2
    },
    "students": {
      "totalStudents": 200,
      "activeStudents": 195,
      "newAdmissions": 5
    },
    "fees": {
      "expectedThisMonth": 1100000,
      "collectedThisMonth": 850000,
      "collectionPercentage": 77.27,
      "outstandingAmount": 2345000,
      "outstandingCount": 67,
      "overdue30Days": 23
    },
    "attendance": {
      "studentAverageAttendance": 92.15,
      "teacherAverageAttendance": 94.50,
      "studentsWithLowAttendance": 12,
      "threshold": 75
    }
  }
}
```

**Status Codes**:
- 200 OK

---

## Error Codes Reference

| Code | HTTP Status | Description |
|------|------------|-------------|
| TEACHER_NOT_FOUND | 404 | Teacher with given ID does not exist |
| DUPLICATE_EMAIL | 409 | Teacher with this email already exists |
| DUPLICATE_ASSIGNMENT | 409 | Teacher already assigned to this class/subject |
| CLASS_NOT_FOUND | 404 | Class does not exist |
| SUBJECT_NOT_FOUND | 404 | Subject does not exist |
| FEE_STRUCTURE_NOT_FOUND | 404 | Fee structure does not exist |
| STUDENT_NOT_FOUND | 404 | Student does not exist |
| ATTENDANCE_NOT_FOUND | 404 | Attendance record does not exist |
| DUPLICATE_ATTENDANCE | 409 | Attendance already marked for this student/class/date |
| DUPLICATE_RECEIPT | 409 | Receipt number already exists in system |
| INVALID_DATE_RANGE | 400 | Start date must be <= end date |
| INVALID_AMOUNT | 400 | Amount must be greater than zero |
| INVALID_STATUS | 400 | Provided status is not valid |
| VALIDATION_FAILED | 400 | One or more fields failed validation |
| UNAUTHORIZED | 401 | User not authenticated |
| FORBIDDEN | 403 | User lacks permission for this operation |
| INTERNAL_ERROR | 500 | Unexpected server error |

---

## Pagination Standards

All list endpoints follow this pagination format:

**Request**:
```
GET /api/v1/resource?pageNumber=1&pageSize=20
```

**Response**:
```json
{
  "data": {
    "items": [ /* resource array */ ],
    "pagination": {
      "pageNumber": 1,
      "pageSize": 20,
      "totalCount": 150,
      "totalPages": 8,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  }
}
```

---

## Rate Limiting (Future)

Proposed for Phase 2+:
- 100 requests/minute per user
- 1000 requests/hour per API key
- X-RateLimit-Remaining header in responses

---

## Authentication

All endpoints require:
```
Authorization: Bearer {JWT_TOKEN}
```

Token must contain claims:
- `sub`: User ID
- `username`: Username
- `email`: Email
- `role`: User role (Admin for Phase 2)


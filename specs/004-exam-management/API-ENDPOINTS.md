# API Endpoints: Exam & Marks Management

**Feature**: 004-exam-management  
**Version**: 1.0  
**Base URL**: `/api/v1`  
**Authentication**: JWT Bearer token required for all endpoints

---

## 📋 Endpoint Summary

| Method | Endpoint | Purpose | Status |
|--------|----------|---------|--------|
| **EXAMS** |
| POST | `/exams` | Create new exam | 201 |
| GET | `/exams` | List all exams | 200 |
| GET | `/exams/{examId}` | Get exam details | 200 |
| PUT | `/exams/{examId}` | Update exam | 200 |
| DELETE | `/exams/{examId}` | Delete/Archive exam | 204 |
| POST | `/exams/{examId}/publish` | Publish exam | 200 |
| **MARKS** |
| GET | `/exams/{examId}/classes/{classId}/marks` | Get marks form | 200 |
| POST | `/exams/{examId}/classes/{classId}/marks` | Save marks | 200 |
| GET | `/exams/{examId}/marks/{studentId}` | Get student marks | 200 |
| PUT | `/exams/{examId}/marks/{studentId}` | Update student marks | 200 |
| POST | `/exams/{examId}/classes/{classId}/submit` | Submit class marks | 200 |
| **REPORT CARDS** |
| GET | `/report-cards` | List report cards | 200 |
| GET | `/report-cards/{examId}/{studentId}` | Get single report card | 200 |
| GET | `/exams/{examId}/report-cards` | Get all report cards for exam | 200 |
| POST | `/report-cards/{cardId}/export-pdf` | Export report card as PDF | 200 |
| **GRADES** |
| GET | `/grades` | Get grade configuration | 200 |
| PUT | `/grades` | Update grade configuration | 200 |

---

## 🔴 EXAM ENDPOINTS

### POST /exams
**Create a new exam**

**Headers**:
```
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

**Request Body**:
```json
{
  "name": "Mid Term Exam 2026",
  "description": "First term examination",
  "examDate": "2026-03-15",
  "totalMarks": 100,
  "passMarks": 40,
  "subjects": [
    { "subjectId": "uuid-1", "maxMarks": 50, "passMarks": 20 },
    { "subjectId": "uuid-2", "maxMarks": 50, "passMarks": 20 }
  ],
  "classes": ["class-uuid-1", "class-uuid-2"]
}
```

**Response (201 Created)**:
```json
{
  "id": "exam-uuid",
  "name": "Mid Term Exam 2026",
  "description": "First term examination",
  "examDate": "2026-03-15",
  "totalMarks": 100,
  "passMarks": 40,
  "status": "draft",
  "subjects": [
    { "id": "uuid-1", "name": "Mathematics", "maxMarks": 50, "passMarks": 20 },
    { "id": "uuid-2", "name": "English", "maxMarks": 50, "passMarks": 20 }
  ],
  "classes": [
    { "id": "class-uuid-1", "name": "Class 10-A", "marksEntryStatus": "pending" },
    { "id": "class-uuid-2", "name": "Class 10-B", "marksEntryStatus": "pending" }
  ],
  "createdAt": "2026-02-25T10:30:00Z",
  "createdBy": "user-uuid"
}
```

**Error Responses**:
```json
400 Bad Request:
{
  "errors": [
    "Exam date cannot be in the past",
    "Total marks must be greater than 0"
  ]
}

401 Unauthorized:
{ "message": "Invalid or missing JWT token" }

403 Forbidden:
{ "message": "You don't have permission to create exams" }

500 Internal Server Error:
{ "message": "Failed to create exam", "details": "..." }
```

---

### GET /exams
**List all exams with optional filtering**

**Query Parameters**:
```
?status=published      # Filter by status: draft, published, completed, archived
&classId=uuid          # Filter by class ID
&subjectId=uuid        # Filter by subject ID
&fromDate=2026-01-01   # Filter by date range (from)
&toDate=2026-12-31     # Filter by date range (to)
&sortBy=date           # Sort by: name, date, createdAt
&sortOrder=desc        # asc or desc
&page=1                # Pagination (1-based)
&pageSize=20           # Items per page
```

**Example Request**:
```
GET /api/v1/exams?status=published&sortBy=date&sortOrder=desc&page=1&pageSize=10
```

**Response (200 OK)**:
```json
{
  "data": [
    {
      "id": "exam-uuid",
      "name": "Mid Term Exam 2026",
      "examDate": "2026-03-15",
      "totalMarks": 100,
      "status": "published",
      "subjectCount": 2,
      "classCount": 2,
      "createdAt": "2026-02-25T10:30:00Z"
    },
    {
      "id": "exam-uuid-2",
      "name": "Final Exam 2026",
      "examDate": "2026-05-20",
      "totalMarks": 100,
      "status": "draft",
      "subjectCount": 3,
      "classCount": 1,
      "createdAt": "2026-02-24T14:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 15,
    "totalPages": 2
  }
}
```

---

### GET /exams/{examId}
**Get detailed exam information**

**Path Parameters**:
```
examId: string (UUID)
```

**Response (200 OK)**:
```json
{
  "id": "exam-uuid",
  "name": "Mid Term Exam 2026",
  "description": "First term examination",
  "examDate": "2026-03-15",
  "totalMarks": 100,
  "passMarks": 40,
  "status": "published",
  "subjects": [
    {
      "id": "subject-uuid-1",
      "name": "Mathematics",
      "maxMarks": 50,
      "passMarks": 20,
      "marksSubmittedCount": 35,
      "marksSubmittedPercent": 100
    },
    {
      "id": "subject-uuid-2",
      "name": "English",
      "maxMarks": 50,
      "passMarks": 20,
      "marksSubmittedCount": 35,
      "marksSubmittedPercent": 100
    }
  ],
  "classes": [
    {
      "id": "class-uuid-1",
      "name": "Class 10-A",
      "studentCount": 35,
      "marksEntryStatus": "submitted",
      "submittedAt": "2026-03-16T15:45:00Z",
      "submittedBy": "user-uuid"
    }
  ],
  "createdAt": "2026-02-25T10:30:00Z",
  "updatedAt": "2026-03-10T12:00:00Z"
}
```

**Error Responses**:
```json
404 Not Found:
{ "message": "Exam not found" }

401/403: Same as POST /exams
```

---

### PUT /exams/{examId}
**Update exam (only if status is Draft)**

**Request Body**: Same structure as POST (only editable fields)
```json
{
  "name": "Updated Exam Name",
  "description": "Updated description",
  "examDate": "2026-03-20",
  "totalMarks": 100,
  "passMarks": 45
}
```

**Response (200 OK)**: Same as GET /exams/{examId}

**Error Responses**:
```json
400 Bad Request:
{
  "message": "Cannot update exam",
  "reason": "Exam status must be 'draft' to update"
}

404 Not Found:
{ "message": "Exam not found" }
```

---

### DELETE /exams/{examId}
**Delete/Archive exam (only if status is Draft)**

**Response (204 No Content)**: No response body

**Error Responses**:
```json
400 Bad Request:
{
  "message": "Cannot delete exam",
  "reason": "Exam must be in draft status to delete"
}

404 Not Found:
{ "message": "Exam not found" }
```

---

### POST /exams/{examId}/publish
**Publish exam to enable marks entry**

**Request Body**: Empty or minimal
```json
{}
```

**Response (200 OK)**:
```json
{
  "id": "exam-uuid",
  "status": "published",
  "publishedAt": "2026-02-26T10:00:00Z",
  "message": "Exam published successfully. Marks entry is now enabled."
}
```

**Error Responses**:
```json
400 Bad Request:
{
  "errors": [
    "Cannot publish exam",
    "At least one subject must be assigned",
    "At least one class must be assigned"
  ]
}
```

---

## 🟢 MARKS ENDPOINTS

### GET /exams/{examId}/classes/{classId}/marks
**Get marks entry form for a class**

Returns all students in class with empty marks fields for data entry.

**Query Parameters**:
```
?sortBy=rollNumber    # Sort by: rollNumber, name, totalMarks
&sortOrder=asc        # asc or desc
```

**Response (200 OK)**:
```json
{
  "examId": "exam-uuid",
  "examName": "Mid Term Exam 2026",
  "classId": "class-uuid-1",
  "className": "Class 10-A",
  "totalStudents": 35,
  "marksEntryStatus": "draft",
  "subjects": [
    { "id": "subject-uuid-1", "name": "Mathematics", "maxMarks": 50 },
    { "id": "subject-uuid-2", "name": "English", "maxMarks": 50 }
  ],
  "students": [
    {
      "studentId": "student-uuid-1",
      "name": "Rahul Singh",
      "rollNumber": "1001",
      "marks": {
        "subject-uuid-1": { "obtained": null, "isAbsent": false },
        "subject-uuid-2": { "obtained": null, "isAbsent": false }
      },
      "total": null,
      "percentage": null,
      "grade": null
    },
    {
      "studentId": "student-uuid-2",
      "name": "Priya Sharma",
      "rollNumber": "1002",
      "marks": {
        "subject-uuid-1": { "obtained": 45, "isAbsent": false },
        "subject-uuid-2": { "obtained": 48, "isAbsent": false }
      },
      "total": 93,
      "percentage": 93,
      "grade": "A"
    }
  ],
  "lastUpdated": "2026-03-15T14:30:00Z"
}
```

---

### POST /exams/{examId}/classes/{classId}/marks
**Save marks for a class (Draft mode)**

**Request Body**:
```json
{
  "examId": "exam-uuid",
  "classId": "class-uuid-1",
  "marksData": [
    {
      "studentId": "student-uuid-1",
      "subjectMarks": {
        "subject-uuid-1": { "obtained": 45, "isAbsent": false },
        "subject-uuid-2": { "obtained": 42, "isAbsent": false }
      }
    },
    {
      "studentId": "student-uuid-2",
      "subjectMarks": {
        "subject-uuid-1": { "obtained": null, "isAbsent": true },
        "subject-uuid-2": { "obtained": 38, "isAbsent": false }
      }
    }
  ]
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Marks saved successfully (Draft)",
  "marksCount": 2,
  "validationResults": {
    "studentCount": 35,
    "markedCount": 2,
    "unmarkedCount": 33,
    "totalMarksObtained": 173,
    "averagePercentage": 49.4
  }
}
```

**Error Responses**:
```json
400 Bad Request:
{
  "errors": [
    "Marks cannot exceed max marks (50) for subject: Mathematics",
    "Student uuid-3 not found in class"
  ],
  "successCount": 0,
  "failureCount": 2
}

409 Conflict:
{
  "message": "Cannot save marks",
  "reason": "Marks already submitted for this class"
}
```

---

### GET /exams/{examId}/marks/{studentId}
**Get marks for a single student in an exam**

**Response (200 OK)**:
```json
{
  "examId": "exam-uuid",
  "studentId": "student-uuid-1",
  "studentName": "Rahul Singh",
  "rollNumber": "1001",
  "className": "Class 10-A",
  "subjectMarks": [
    {
      "subjectId": "subject-uuid-1",
      "subjectName": "Mathematics",
      "maxMarks": 50,
      "obtained": 45,
      "percentage": 90,
      "grade": "A",
      "isAbsent": false
    },
    {
      "subjectId": "subject-uuid-2",
      "subjectName": "English",
      "maxMarks": 50,
      "obtained": 42,
      "percentage": 84,
      "grade": "B",
      "isAbsent": false
    }
  ],
  "totalMarks": 100,
  "totalObtained": 87,
  "totalPercentage": 87,
  "overallGrade": "A",
  "status": "pass"
}
```

---

### PUT /exams/{examId}/marks/{studentId}
**Update marks for a single student**

**Request Body**:
```json
{
  "subjectMarks": {
    "subject-uuid-1": { "obtained": 48, "isAbsent": false },
    "subject-uuid-2": { "obtained": 45, "isAbsent": false }
  }
}
```

**Response (200 OK)**: Same as GET /exams/{examId}/marks/{studentId}

---

### POST /exams/{examId}/classes/{classId}/submit
**Submit marks for a class (Finalize marks)**

Once submitted, marks are locked and report cards are generated.

**Request Body**:
```json
{
  "confirmedBy": "user-uuid"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Marks submitted successfully",
  "examId": "exam-uuid",
  "classId": "class-uuid-1",
  "marksEntryStatus": "submitted",
  "submittedAt": "2026-03-16T15:45:00Z",
  "studentCount": 35,
  "reportCardsGenerated": 35,
  "nextStep": "View report cards or analytics"
}
```

**Error Responses**:
```json
400 Bad Request:
{
  "errors": [
    "Cannot submit marks",
    "5 students have no marks entered or marked absent"
  ]
}

409 Conflict:
{
  "message": "Marks already submitted for this class",
  "submittedAt": "2026-03-16T10:00:00Z"
}
```

---

## 🔵 REPORT CARD ENDPOINTS

### GET /report-cards
**List all report cards with filtering**

**Query Parameters**:
```
?examId=uuid           # Required or filter combination
&classId=uuid          # Optional
&studentId=uuid        # Optional
&status=pass/fail      # Filter by pass status
&sortBy=classPosition  # Sort by: classPosition, name, percentage
&page=1                # Pagination
```

**Response (200 OK)**:
```json
{
  "data": [
    {
      "id": "card-uuid-1",
      "examId": "exam-uuid",
      "examName": "Mid Term Exam 2026",
      "studentId": "student-uuid-1",
      "studentName": "Rahul Singh",
      "className": "Class 10-A",
      "totalObtained": 87,
      "totalMarks": 100,
      "percentage": 87,
      "overallGrade": "A",
      "classPosition": 2,
      "status": "pass",
      "generatedAt": "2026-03-16T16:00:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 100
  }
}
```

---

### GET /report-cards/{examId}/{studentId}
**Get complete report card for student**

**Response (200 OK)**:
```json
{
  "id": "card-uuid",
  "examId": "exam-uuid",
  "examName": "Mid Term Exam 2026",
  "examDate": "2026-03-15",
  "studentId": "student-uuid-1",
  "studentName": "Rahul Singh",
  "rollNumber": "1001",
  "className": "Class 10-A",
  "fatherName": "Ram Singh",
  "subjectMarks": [
    {
      "subjectId": "subject-uuid-1",
      "subjectName": "Mathematics",
      "maxMarks": 50,
      "obtained": 45,
      "percentage": 90,
      "grade": "A"
    },
    {
      "subjectId": "subject-uuid-2",
      "subjectName": "English",
      "maxMarks": 50,
      "obtained": 42,
      "percentage": 84,
      "grade": "B"
    }
  ],
  "summary": {
    "totalMarks": 100,
    "totalObtained": 87,
    "percentage": 87,
    "overallGrade": "A",
    "classPosition": 2,
    "totalStudents": 35,
    "status": "pass",
    "remarks": "Excellent performance. Keep it up!"
  },
  "attendancePercentage": 95,
  "generatedAt": "2026-03-16T16:00:00Z"
}
```

---

### GET /exams/{examId}/report-cards
**Get all report cards for an exam**

**Query Parameters**: Same as GET /report-cards

**Response (200 OK)**: Similar to GET /report-cards (array of cards)

---

### POST /report-cards/{cardId}/export-pdf
**Export report card as PDF**

**Request Body**:
```json
{
  "includeRemarks": true,
  "signature": "Principal Signature"
}
```

**Response (200 OK)**:
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="ReportCard_RahulSingh_MidTerm2026.pdf"

[Binary PDF content]
```

**Error Responses**:
```json
404 Not Found:
{ "message": "Report card not found" }

500 Internal Server Error:
{ "message": "Failed to generate PDF", "details": "..." }
```

---

## 🟡 GRADES ENDPOINTS

### GET /grades
**Get grade configuration (grading scale)**

**Response (200 OK)**:
```json
{
  "schoolId": "school-uuid",
  "grades": [
    {
      "id": "grade-uuid-1",
      "name": "A",
      "minPercentage": 90,
      "maxPercentage": 100,
      "description": "Excellent"
    },
    {
      "id": "grade-uuid-2",
      "name": "B",
      "minPercentage": 80,
      "maxPercentage": 89,
      "description": "Good"
    },
    {
      "id": "grade-uuid-3",
      "name": "C",
      "minPercentage": 70,
      "maxPercentage": 79,
      "description": "Average"
    },
    {
      "id": "grade-uuid-4",
      "name": "D",
      "minPercentage": 60,
      "maxPercentage": 69,
      "description": "Below Average"
    },
    {
      "id": "grade-uuid-5",
      "name": "F",
      "minPercentage": 0,
      "maxPercentage": 59,
      "description": "Fail"
    }
  ]
}
```

---

### PUT /grades
**Update grade configuration**

**Request Body**:
```json
{
  "grades": [
    {
      "name": "A",
      "minPercentage": 85,
      "maxPercentage": 100,
      "description": "Excellent"
    },
    {
      "name": "B",
      "minPercentage": 75,
      "maxPercentage": 84,
      "description": "Good"
    },
    {
      "name": "C",
      "minPercentage": 65,
      "maxPercentage": 74,
      "description": "Average"
    },
    {
      "name": "D",
      "minPercentage": 55,
      "maxPercentage": 64,
      "description": "Below Average"
    },
    {
      "name": "F",
      "minPercentage": 0,
      "maxPercentage": 54,
      "description": "Fail"
    }
  ]
}
```

**Response (200 OK)**: Same as GET /grades

**Error Responses**:
```json
400 Bad Request:
{
  "errors": [
    "Grade ranges must be continuous",
    "Ranges must cover 0-100%",
    "No overlapping ranges allowed"
  ]
}
```

---

## 🔐 Authentication & Authorization

All endpoints require:
- **Header**: `Authorization: Bearer <JWT_TOKEN>`
- **Permissions**: 
  - Admin/Teacher: Can create/publish exams, enter marks
  - Admin: Can view all analytics, generate reports
  - Student: Can view own report cards (future)

---

## 📊 Common Response Patterns

### Success Response
```json
{
  "data": { ... },
  "success": true,
  "message": "Operation successful"
}
```

### Paginated Response
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error message",
  "errors": ["Error 1", "Error 2"],
  "statusCode": 400
}
```

---

## ⏱️ Rate Limiting

- **Global Limit**: 100 requests per minute per user
- **Bulk Operations**: 10 requests per second for marks submission
- **Header**: `X-RateLimit-Remaining: 99`

---

## 📝 Postman Collection

[Download Exam Module Postman Collection](./exam-module.postman_collection.json)

---

## 🧪 Testing with cURL

### Create Exam
```bash
curl -X POST http://localhost:5208/api/v1/exams \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Mid Term 2026",
    "examDate": "2026-03-15",
    "totalMarks": 100,
    "passMarks": 40,
    "subjects": [{"subjectId": "uuid", "maxMarks": 50}],
    "classes": ["class-uuid"]
  }'
```

### List Exams
```bash
curl -X GET "http://localhost:5208/api/v1/exams?status=published&page=1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Get Report Card
```bash
curl -X GET http://localhost:5208/api/v1/report-cards/exam-uuid/student-uuid \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## 🔄 Version History

**v1.0** (Feb 2026): Initial release
- Basic exam CRUD
- Marks entry
- Report cards
- Grade configuration

**v1.1** (TBD): Enhancements
- Bulk CSV import
- Advanced analytics
- Performance reports


# Module: Fee Management

## Overview
Complete fee lifecycle: define fee structures with categories, assign fees to students (individually or bulk), record payments, generate receipts (PDF), and track outstanding balances. Transport fees are automatically included when a student has a transport assignment.

---

## Domain Entities

### FeeStructure (`SMS.Domain.Entities.FeeStructure`)
| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | |
| Name | string | e.g., "Regular Monthly 2026" |
| AcademicYearId | Guid | FK |
| Frequency | string | "monthly", "quarterly", "yearly" |
| TotalAmount | decimal | Sum of categories |
| IsActive | bool | |
| CreatedAt, UpdatedAt | DateTime | Audit |
| CreatedBy, UpdatedBy | string? | |
| *Nav* | AcademicYear, Categories, StudentFees |

### FeeStructureCategory (`SMS.Domain.Entities.FeeStructureCategory`)
| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | |
| FeeStructureId | Guid | FK |
| Name | string | e.g., "Tuition", "Lab Fee" |
| Amount | decimal | |

### StudentFee (`SMS.Domain.Entities.StudentFee` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| EnrollmentId | Guid | FK to Enrollment |
| FeeStructureId | Guid | FK |
| StartDate | DateOnly | |
| EndDate | DateOnly? | |
| IsActive | bool | |
| StructureAmount | decimal | From fee structure |
| TransportFeeAmount | decimal | From transport route |
| *Computed* | `TotalAmount` = StructureAmount + TransportFeeAmount |
| PaidAmount | decimal | Sum of payments |
| *Computed* | `BalanceAmount` = TotalAmount - PaidAmount |
| *Nav* | Enrollment, FeeStructure, Payments |

### FeePayment (`SMS.Domain.Entities.FeePayment` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| StudentFeeId | Guid | FK |
| AmountPaid | decimal | |
| PaymentDate | DateOnly | |
| ReceiptNumber | string | Auto-generated, unique |
| PaymentMethod | string | "cash", "check", "bank_transfer" |
| Notes | string? | |
| *Nav* | StudentFee |

---

## API Endpoints

**Controller**: `FeesController` — Route: `api/fees`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/fees/structures` | List structures (paginated) |
| GET | `/api/fees/structures/active` | Active structures |
| GET | `/api/fees/structures/{id}` | Structure by ID |
| POST | `/api/fees/structures` | Create structure + categories |
| PUT | `/api/fees/structures/{id}` | Update structure |
| DELETE | `/api/fees/structures/{id}` | Delete structure |
| GET | `/api/fees/student-fees` | List student fees |
| GET | `/api/fees/student-fees/student/{studentId}` | Fees for a student |
| GET | `/api/fees/student-fees/section/{sectionId}` | Fees for a section |
| GET | `/api/fees/student-fees/{id}` | Single student fee |
| POST | `/api/fees/student-fees` | Assign fee to student |
| POST | `/api/fees/student-fees/bulk-assign` | Bulk assign to section |
| PATCH | `/api/fees/student-fees/{id}/terminate` | Terminate fee |
| GET | `/api/fees/student-fees/{id}/pdf` | Download fee statement PDF |
| GET | `/api/fees/payments` | List payments |
| GET | `/api/fees/payments/student-fee/{studentFeeId}` | Payments for a student fee |
| POST | `/api/fees/payments` | Record payment |
| GET | `/api/fees/payments/{paymentId}/receipt` | Download receipt PDF |
| GET | `/api/fees/report` | Fee collection report |

---

## CQRS (in `Features/Fees`)

### Commands
- `CreateFeeStructureCommand` — Name, AcademicYearId, Frequency, Categories (name+amount list)
- `UpdateFeeStructureCommand`, `DeleteFeeStructureCommand`
- `AssignFeeToStudentCommand` — EnrollmentId, FeeStructureId, StartDate, EndDate
- `BulkAssignFeeCommand` — SectionId, FeeStructureId, AcademicYearId, StartDate
- `TerminateStudentFeeCommand`
- `RecordFeePaymentCommand` — StudentFeeId, AmountPaid, PaymentDate, PaymentMethod, Notes

### Queries
- `GetFeeStructuresQuery` — Paginated
- `GetActiveFeeStructuresQuery`
- `GetFeeStructureByIdQuery`
- `GetStudentFeesQuery`, `GetStudentFeesByStudentQuery`, `GetStudentFeesBySectionQuery`
- `GetPaymentsQuery`, `GetPaymentsByStudentFeeQuery`
- `GetFeeReportQuery` — DateRange, Class filter
- `GetFeeReceiptPdfQuery`, `GetFeeStatementPdfQuery`

### Validators
- FluentValidation rules in `Features/Fees/Validators/`

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/FeeStructure.cs` |
| Entity | `backend/src/SMS.Domain/Entities/FeeStructureCategory.cs` |
| Entity | `backend/src/SMS.Domain/Entities/StudentFee.cs` |
| Entity | `backend/src/SMS.Domain/Entities/FeePayment.cs` |
| Enum | `backend/src/SMS.Domain/Enums/FeeFrequency.cs` |
| Enum | `backend/src/SMS.Domain/Enums/PaymentMethod.cs` |
| Commands | `backend/src/SMS.Application/Features/Fees/Commands/` |
| DTOs | `backend/src/SMS.Application/Features/Fees/DTOs/` |
| Handlers | `backend/src/SMS.Application/Features/Fees/Handlers/` |
| Queries | `backend/src/SMS.Application/Features/Fees/Queries/` |
| Validators | `backend/src/SMS.Application/Features/Fees/Validators/` |
| Controller | `backend/src/SMS.API/Controllers/FeesController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/FeeStructureConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/FeeStructureCategoryConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/StudentFeeConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/FeePaymentConfiguration.cs` |
| Frontend Page | `frontend/src/pages/FeesPage.tsx` |
| Fee Report Page | `frontend/src/pages/FeeReportPage.tsx` |
| Fee Reports Page | `frontend/src/pages/FeeReportsPage.tsx` |
| Outstanding Fees | `frontend/src/pages/OutstandingFeesPage.tsx` |
| Fee Hooks | `frontend/src/hooks/useFeeReports.ts` |
| Query Hooks | `frontend/src/services/queries/useFeeHooks.ts` |
| Frontend API | `frontend/src/services/api.ts` (feeApi section) |

---

## Business Rules
- FeeStructure.TotalAmount = sum of all FeeStructureCategory amounts
- StudentFee.TransportFeeAmount is populated from the student's transport route MonthlyFee
- FeePayment is immutable (cannot be edited, only reversed via new entry)
- ReceiptNumber is auto-generated (unique per payment)
- PDF receipts use school branding (logo, colors, header/footer) from Settings
- Fee statement PDF includes category breakdown + transport fee as separate line
- Bulk assign creates StudentFee for all students in a section

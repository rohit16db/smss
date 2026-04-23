# Module: Salary & Payroll

## Overview
Comprehensive salary management: define salary structures (base + allowances − deductions), assign structures to staff, process salary payments (individual & bulk), and payroll reporting. Three separate controllers handle different aspects.

---

## Domain Entities

### SalaryStructure (`SMS.Domain.Entities.SalaryStructure` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Grade A", "Senior Teacher" |
| Description | string? | |
| BaseSalary | decimal | Core pay |
| HRA | decimal | House Rent Allowance |
| DA | decimal | Dearness Allowance |
| MedicalAllowance | decimal | |
| ConveyanceAllowance | decimal | Transport allowance |
| OtherAllowances | decimal | |
| StandardDeduction | decimal | PF, tax, etc. |
| *Computed* | `TotalAllowances` = HRA + DA + Medical + Conveyance + Other |
| *Computed* | `GrossSalary` = BaseSalary + TotalAllowances − StandardDeduction |
| MinExperienceYears | int | |
| ApplicableQualifications | string? | Comma-separated |
| IsActive | bool | |
| EffectiveFromDate | DateOnly | |
| EffectiveToDate | DateOnly? | null = still active |
| *Nav* | StaffMembers (collection) |

### SalaryPayment (`SMS.Domain.Entities.SalaryPayment`)
| Property | Type | Notes |
|----------|------|-------|
| Id | Guid | |
| StaffId | Guid | FK |
| PeriodStartDate, PeriodEndDate | DateOnly | Salary period |
| BaseSalary | decimal | |
| Deductions | decimal | |
| Bonus | decimal | |
| NetSalary | decimal | Base − Deductions + Bonus |
| Status | SalaryPaymentStatus | Pending/Approved/Paid/Cancelled/OnHold |
| PaidDate | DateOnly? | |
| ReferenceNumber | string? | |
| PaymentMethod | SalaryPaymentMethod? | Cash/BankTransfer/Cheque/MobilePayment/Other |
| Remarks | string? | |
| CreatedAt, UpdatedAt | DateTime | |

### Enums
```csharp
SalaryPaymentStatus: Pending=0, Approved=1, Paid=2, Cancelled=3, OnHold=4
SalaryPaymentMethod: Cash=0, BankTransfer=1, Cheque=2, MobilePayment=3, Other=4
```

---

## API Endpoints

### SalaryStructureController — Route: `api/v1/salarystructure`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/salarystructure` | List all structures |
| GET | `/api/v1/salarystructure/{id}` | Get by ID |
| GET | `/api/v1/salarystructure/applicable/{staffId}` | Structures applicable to staff |
| GET | `/api/v1/salarystructure/Staff/{staffId}/current` | Current structure for staff |
| GET | `/api/v1/salarystructure/Staffs/assignments` | All staff-structure assignments |
| POST | `/api/v1/salarystructure` | Create structure |
| PUT | `/api/v1/salarystructure/{id}` | Update structure |
| DELETE | `/api/v1/salarystructure/{id}` | Delete structure |
| POST | `/api/v1/salarystructure/assign-to-Staff` | Assign structure to staff |
| POST | `/api/v1/salarystructure/bulk-create-salaries` | Bulk create salary records |

### SalaryController — Route: `api/v1/salary`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/salary` | List salary payments |
| GET | `/api/v1/salary/{id}` | Get by ID |
| GET | `/api/v1/salary/staff/{staffId}` | Payments for staff |
| GET | `/api/v1/salary/pending` | Pending payments |
| GET | `/api/v1/salary/summary` | Salary summary |
| GET | `/api/v1/salary/period/report` | Period report |
| POST | `/api/v1/salary/bulk` | Bulk create salary records |
| PUT | `/api/v1/salary/{id}/status` | Update status |
| PUT | `/api/v1/salary/{id}/mark-paid` | Mark as paid |
| DELETE | `/api/v1/salary/{id}` | Delete |

### SalaryPaymentController — Route: `api/v1/salary-management`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/salary-management` | List payments |
| GET | `/api/v1/salary-management/{id}` | Get by ID |
| GET | `/api/v1/salary-management/staff/{staffId}` | Staff payments |
| GET | `/api/v1/salary-management/summary` | Summary |
| PUT | `/api/v1/salary-management/{id}/status` | Update status |
| PUT | `/api/v1/salary-management/{id}/pay` | Process payment |
| PUT | `/api/v1/salary-management/{id}` | Update payment |
| DELETE | `/api/v1/salary-management/{id}` | Delete |

### PayrollController — Route: `api/v1/payroll`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/payroll/report` | Payroll report |
| GET | `/api/v1/payroll/bonus-eligibility` | Bonus eligibility |
| GET | `/api/v1/payroll/attendance-summary` | Attendance-based summary |

---

## CQRS

### Features/Salary
- **Commands**: CreateSalaryStructure, UpdateSalaryStructure, DeleteSalaryStructure, AssignStructureToStaff, BulkCreateSalaries, CreateSalaryPayment, UpdateSalaryPaymentStatus, MarkSalaryPaid, DeleteSalaryPayment
- **Queries**: GetSalaryStructures, GetSalaryStructureById, GetApplicableStructures, GetCurrentStructure, GetStaffAssignments, GetSalaryPayments, GetPendingSalaries, GetSalarySummary, GetPeriodReport

### Features/Payroll
- **Queries**: GetPayrollReport, GetBonusEligibility, GetAttendanceSummary

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/SalaryStructure.cs` |
| Entity | `backend/src/SMS.Domain/Entities/SalaryPayment.cs` |
| Commands | `backend/src/SMS.Application/Features/Salary/Commands/SalaryCommands.cs` |
| Commands | `backend/src/SMS.Application/Features/Salary/Commands/SalaryStructureCommands.cs` |
| DTOs | `backend/src/SMS.Application/Features/Salary/DTOs/SalaryDTOs.cs` |
| DTOs | `backend/src/SMS.Application/Features/Salary/DTOs/SalaryStructureDTOs.cs` |
| Cmd Handlers | `backend/src/SMS.Application/Features/Salary/Handlers/Commands/SalaryCommandHandlers.cs` |
| Cmd Handlers | `backend/src/SMS.Application/Features/Salary/Handlers/Commands/SalaryStructureCommandHandlers.cs` |
| Query Handlers | `backend/src/SMS.Application/Features/Salary/Handlers/Queries/SalaryQueryHandlers.cs` |
| Query Handlers | `backend/src/SMS.Application/Features/Salary/Handlers/Queries/SalaryStructureQueryHandlers.cs` |
| Queries | `backend/src/SMS.Application/Features/Salary/Queries/SalaryQueries.cs` |
| Queries | `backend/src/SMS.Application/Features/Salary/Queries/SalaryStructureQueries.cs` |
| Payroll DTOs | `backend/src/SMS.Application/Features/Payroll/DTOs/PayrollDTOs.cs` |
| Payroll Handlers | `backend/src/SMS.Application/Features/Payroll/Handlers/PayrollQueryHandlers.cs` |
| Payroll Queries | `backend/src/SMS.Application/Features/Payroll/Queries/PayrollQueries.cs` |
| Controllers | `backend/src/SMS.API/Controllers/SalaryStructureController.cs` |
| Controllers | `backend/src/SMS.API/Controllers/SalaryController.cs` |
| Controllers | `backend/src/SMS.API/Controllers/SalaryPaymentController.cs` |
| Controllers | `backend/src/SMS.API/Controllers/PayrollController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/SalaryStructureConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/SalaryPaymentConfiguration.cs` |
| Frontend | `frontend/src/pages/SalaryPage.tsx` |
| Frontend | `frontend/src/pages/SalaryStructurePage.tsx` |
| Frontend | `frontend/src/pages/StaffSalaryAssignmentPage.tsx` |
| Frontend | `frontend/src/pages/BulkSalaryProcessingPage.tsx` |
| Frontend | `frontend/src/pages/SalaryPaymentPage.tsx` |
| Frontend | `frontend/src/pages/PayrollPage.tsx` |
| Services | `frontend/src/services/salaryService.ts` |
| Services | `frontend/src/services/salaryStructureService.ts` |
| Services | `frontend/src/services/salaryPaymentService.ts` |
| Services | `frontend/src/services/payrollService.ts` |
| Types | `frontend/src/types/salary.ts` |
| Types | `frontend/src/types/salaryStructure.ts` |
| Types | `frontend/src/types/salaryPayment.ts` |
| Types | `frontend/src/types/payroll.ts` |

---

## Business Rules
- SalaryStructure.GrossSalary = BaseSalary + all allowances − StandardDeduction
- Staff.SalaryStructureId links to assigned structure with effective date
- Salary payment status workflow: Pending → Approved → Paid (or Cancelled/OnHold)
- Bulk salary processing creates SalaryPayment records for multiple staff at once
- Payroll report can correlate with staff attendance for deduction calculations

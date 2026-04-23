# Module: Reports & Dashboard

## Overview
Aggregated reporting across modules: fee collection summaries, outstanding fees, salary reports, budget vs actual, attendance correlation, and a dashboard API that provides summary cards and chart data.

---

## API Endpoints

### ReportsController — Route: `api/reports` (Fee Reports)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/reports/collection-summary` | Fee collection summary by period |
| GET | `/api/reports/monthly-trend` | Monthly fee collection trend |
| GET | `/api/reports/by-category` | Collection by fee category |
| GET | `/api/reports/outstanding` | Outstanding fees list |
| GET | `/api/reports/student/{studentId}/payment-history` | Student payment history |

### ReportsController — Route: `api/salary-reports` (Salary Reports)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/salary-reports/expense-summary` | Salary expense summary |
| GET | `/api/salary-reports/monthly-trend` | Monthly salary trend |
| GET | `/api/salary-reports/component-breakdown` | Salary component breakdown |
| GET | `/api/salary-reports/staff-comparison` | Staff salary comparison |
| GET | `/api/salary-reports/attendance-correlation` | Attendance vs salary |
| GET | `/api/salary-reports/budget-vs-actual` | Budget vs actual spending |

### DashboardController — Route: `api/dashboard`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/dashboard/summary` | Dashboard summary (cards, charts, trends) |

---

## CQRS

### Features/Reports
- **DTOs**: `ReportDTOs.cs` — CollectionSummaryDto, MonthlyTrendDto, CategoryBreakdownDto, OutstandingFeeDto, ExpenseSummaryDto, ComponentBreakdownDto, StaffComparisonDto, BudgetVsActualDto
- **Queries**: `FeeAndSalaryQueries.cs`
- **Handlers**: `FeeReportQueryHandlers.cs`, `SalaryReportQueryHandlers.cs`

### Features/Dashboard
- **DTOs**: `DashboardDTOs.cs`
- **Queries**: `DashboardQueries.cs`
- **Handlers**: `DashboardHandlers.cs`

---

## File Map

| Layer | File |
|-------|------|
| Report DTOs | `backend/src/SMS.Application/Features/Reports/DTOs/ReportDTOs.cs` |
| Report Queries | `backend/src/SMS.Application/Features/Reports/Queries/FeeAndSalaryQueries.cs` |
| Fee Report Handlers | `backend/src/SMS.Application/Features/Reports/Handlers/FeeReportQueryHandlers.cs` |
| Salary Report Handlers | `backend/src/SMS.Application/Features/Reports/Handlers/SalaryReportQueryHandlers.cs` |
| Dashboard DTOs | `backend/src/SMS.Application/Features/Dashboard/DTOs/` |
| Dashboard Queries | `backend/src/SMS.Application/Features/Dashboard/Queries/` |
| Dashboard Handlers | `backend/src/SMS.Application/Features/Dashboard/Handlers/` |
| Controller | `backend/src/SMS.API/Controllers/ReportsController.cs` |
| Controller | `backend/src/SMS.API/Controllers/DashboardController.cs` |
| Frontend | `frontend/src/pages/FeeReportPage.tsx` |
| Frontend | `frontend/src/pages/FeeReportsPage.tsx` |
| Frontend | `frontend/src/pages/OutstandingFeesPage.tsx` |
| Frontend | `frontend/src/pages/SalaryReportsPage.tsx` |
| Frontend | `frontend/src/pages/StaffSalaryComparisonPage.tsx` |
| Frontend | `frontend/src/pages/BudgetVsActualPage.tsx` |
| Frontend | `frontend/src/pages/AttendanceReportPage.tsx` |
| Frontend | `frontend/src/pages/HomePage.tsx` (dashboard) |
| Dashboard Components | `frontend/src/components/dashboard/DashboardSummaryCards.tsx` |
| Dashboard Components | `frontend/src/components/dashboard/FeesCollectionChart.tsx` |
| Dashboard Components | `frontend/src/components/dashboard/AttendanceTrendChart.tsx` |
| Report Components | `frontend/src/components/reports/` |
| Hooks | `frontend/src/hooks/useFeeReports.ts` |
| Hooks | `frontend/src/hooks/useSalaryReports.ts` |
| Services | `frontend/src/services/dashboardService.ts` |
| Types | `frontend/src/types/dashboard.ts` |
| Types | `frontend/src/types/reports.ts` |
| Styles | `frontend/src/pages/ReportPages.css` |

---

## Dashboard Summary Response Structure
The `/api/dashboard/summary` endpoint returns:
- `summaryCards` — Array of card data (title, value, icon, trend)
- `financialSummary` — Total collected, outstanding, collection percentage
- `academicSummary` — Total students, active staff
- `attendanceSummary` — Attendance trend data for charts

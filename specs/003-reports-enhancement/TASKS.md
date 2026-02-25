# Implementation Tasks: Fee & Salary Reports Enhancement

**Feature**: 003-reports-enhancement  
**Created**: February 2026  
**Total Tasks**: 45+ items across 4 phases

---

## Phase 1: Fee Collections Analytics (Week 1-2) 🎯 PRIORITY

### Backend: Database Queries & DTOs

- [ ] T001 Create `ReportDTOs.cs` with:
  - `FeeCollectionSummaryDto` (totalCollected, pending, overdue, collectionRate)
  - `MonthlyCollectionTrendDto` (month, collected, pending, overdue)
  - `CategoryBreakdownDto` (categoryName, collected, pending, percentage)
  - `OutstandingFeeDto` (studentName, dueAmount, daysOverdue, lastPaymentDate, class)
  - `StudentPaymentHistoryDto` (month, dueAmount, paidAmount, method, status)

- [ ] T002 Create `FeeReportSummaryQueryHandler` 
  - Query: Sum all StudentFee payments for date range
  - Calculate: Paid, Pending, Overdue amounts
  - Calculate: Collection rate %
  - Return aggregated summary

- [ ] T003 Create `FeeTrendsQueryHandler`
  - Query: Group StudentFee payments by month
  - Calculate: Monthly collected, pending, overdue
  - Return: Last 6 months trend data
  - Index: Optimize by created_at on StudentFees

- [ ] T004 Create `FeeCategoryBreakdownQueryHandler`
  - Query: Group by FeeStructure category
  - Calculate: Collected and pending per category
  - Calculate: Percentage breakdown
  - Return: Pie chart ready data

- [ ] T005 Create `OutstandingFeesQueryHandler`
  - Query: StudentFees where balance > 0
  - Calculate: Days overdue from due date
  - Filter: By daysOverdue range, amount range, section
  - Sort: By daysOverdue DESC (oldest first)
  - Pagination: Support top 50 / all
  - Include: Student contact info for collection

- [ ] T006 Create `StudentPaymentHistoryQueryHandler`
  - Query: Single student's fees and payments
  - Return: Month-by-month payment history
  - Include: Payment methods, dates

### Backend: New Endpoints

- [ ] T007 Add to `FeesController.cs`:
  - `GET /api/v1/fees/report/summary` - Fee collection summary
  - `GET /api/v1/fees/report/trends` - Trend data
  - `GET /api/v1/fees/report/category-breakdown` - Category data
  - `GET /api/v1/fees/report/outstanding` - Outstanding fees with filters
  - `GET /api/v1/fees/report/student/{studentId}/history` - Payment history

- [ ] T008 Add Swagger documentation to all endpoints

- [ ] T009 Add error handling (invalid date ranges, missing data, etc.)

### Frontend: Services

- [ ] T010 Enhance `feeApi.ts`:
  - `getSummary(startDate, endDate)` 
  - `getTrends(months)` 
  - `getCategoryBreakdown(month)` 
  - `getOutstanding(filters)` 
  - `getPaymentHistory(studentId)` 

### Frontend: React Query Hooks

- [ ] T011 Create `useFeeReports.ts` with hooks:
  - `useFeeCollectionSummary(startDate, endDate)`
  - `useFeeTrends(months)`
  - `FeeCategoryBreakdown(month)`
  - `useOutstandingFees(filters)`
  - `usePaymentHistory(studentId)`

### Frontend: Components

- [ ] T012 Create `FeeAnalyticsDashboard.tsx` 
  - Main container for fee analytics
  - Layout: Summary cards + Charts section
  - Responsive grid (1 col mobile, 2 col tablet, 3 col desktop)

- [ ] T013 Create `FeeSummaryCards.tsx`
  - Card 1: Total Collected (₹, % from budget)
  - Card 2: Total Pending (count, % of total)
  - Card 3: Total Overdue (count, days avg)
  - Card 4: Collection Rate % (green if >80%, yellow if 50-80%, red if <50%)
  - Period selector (month, custom range)

- [ ] T014 Create `FeeTrendChart.tsx`
  - Line chart: Last 6 months collection trend
  - Y-axis: Amount (₹)
  - X-axis: Month name
  - Show: Collected vs Pending vs Overdue
  - Interactive: Hover shows exact values
  - Use: Recharts library

- [ ] T015 Create `FeeCategoryBreakdown.tsx`
  - Pie chart: Fee categories
  - Each slice: Category name + percentage + amount
  - Legend: Show all categories
  - Interactive: Click slice for detail

- [ ] T016 Create `OutstandingStudentsList.tsx`
  - Table: Student name, class, due amount, days overdue, last payment
  - Filters: Amount range, daysOverdue, class/section
  - Sorting: By amount (DESC), days overdue (DESC)
  - Highlight: Red if > 90 days, orange if 60-90, yellow if 30-60
  - Actions: Send reminder, collection notes
  - Pagination: Top 20, or all

### Frontend: Pages

- [ ] T017 Create `FeeAnalyticsPage.tsx`
  - Wrapper with date filters
  - Tab 1: Analytics Dashboard (charts)
  - Tab 2: Outstanding Report (detailed table)
  - Tab 3: Export/Download
  - Loading states

- [ ] T018 Update routing in `App.tsx`
  - Add route: `/reports/fees/analytics` → FeeAnalyticsPage

### Export: CSV/PDF

- [ ] T019 Create `reportExportService.ts`
  - `exportOutstandingToCSV(data)` - CSV format for mail merge
  - `exportSummaryToCSV(summary)` - Summary sheet
  - Use: papaparse library for CSV

- [ ] T020 Implement fee report export button
  - Format options: CSV, PDF
  - Type options: Outstanding, Summary, Trend
  - Download triggers correctly

### Testing

- [ ] T021 Unit tests for all backend handlers
- [ ] T022 Integration tests for report endpoints
- [ ] T023 Component tests for charts
- [ ] T024 E2E test: All filters work correctly

---

## Phase 2: Fee Outstanding Detail Report (Week 3)

- [ ] T025 Enhance `OutstandingStudentsList.tsx` with:
  - Aging column (30, 60, 90+ days)
  - Color coding by age
  - Bulk email/reminder actions
  - Export selected students

- [ ] T026 Create `CustomerOutstandingDetail.tsx`
  - Student detail modal
  - Payment history timeline
  - Contact info + notes
  - Send reminder button

- [ ] T027 Create PDF export for outstanding report
  - Include: Report header, date range, summary
  - Table: All outstanding fees with aging
  - Footer: Total overdue, next steps
  - Use: jsPDF + autoTable

---

## Phase 3: Salary Analytics (Week 4)

- [ ] T028 Create salary report DTOs similar to fees
  - `SalaryCollectionSummaryDto`
  - `SalaryComponentDto` (base, bonus, deductions breakdown)
  - `SalaryTrendDto` (month data)
  - `TeacherSalaryComparisonDto`

- [ ] T029 Create salary report query handlers
  - Summary (total net, avg salary, total bonus)
  - Trends (last 12 months)
  - Component breakdown (base, bonus, deductions)
  - Teacher comparison
  - Bonus analysis

- [ ] T030 Add salary report endpoints (5 new)

- [ ] T031 Create `SalaryAnalyticsDashboard.tsx`
  - Similar layout to fee analytics
  - Summary cards + charts
  - Period selector

- [ ] T032 Create salary charts:
  - `SalaryComponentChart.tsx` (stacked bar)
  - `SalaryTrendChart.tsx` (12-month line)
  - `TeacherSalaryComparisonChart.tsx` (bar chart by teacher)

- [ ] T033 Create `TeacherSalaryComparison.tsx`
  - Table: Teacher name, base, bonus, deductions, net
  - Sort: By amount
  - Filter: By section/class

- [ ] T034 Export salary reports (Excel format)

---

## Phase 4: Correlation & Advanced (Week 5)

- [ ] T035 Create `AttendanceToSalaryCorrelation.tsx`
  - Table: Teacher, attendance %, deduction calc, actual deduction
  - Highlight mismatches (formula vs actual)
  - Bonus eligibility check

- [ ] T036 Implement attendance-to-salary query handler

- [ ] T037 Budget vs Actual (if time permits)
  - Budget entry page
  - Actual vs budget comparison
  - Forecast next 3 months

---

## Testing & Deployment

- [ ] T038 Performance testing (queries < 500ms)
- [ ] T039 Load testing (concurrent users)
- [ ] T040 Accessibility testing (WCAG AA)
- [ ] T041 Browser compatibility testing
- [ ] T042 Documentation: User guide for reports
- [ ] T043 Documentation: API endpoint guide
- [ ] T044 Create video tutorial: How to use reports
- [ ] T045 Deployment checklist & rollout plan

---

## Estimated Effort

| Phase | Duration | Effort | Priority |
|-------|----------|--------|----------|
| Phase 1 | Week 1-2 | 40 hours | P1 - High Value |
| Phase 2 | Week 3 | 16 hours | P1 - Detail |
| Phase 3 | Week 4 | 32 hours | P1 - Core Insights |
| Phase 4 | Week 5 | 24 hours | P2 - Enhancement |
| **Total** | **5 weeks** | **112 hours** | **MVP Ready** |

---

## Dependencies & Prerequisites

✅ Phase 1 (MVP)
- All fee and salary data already captured
- Existing API structure ready
- React Query already in use
- Chart library selection needed (recommend Recharts)

⚠️ Phase 2-4
- Phase 1 completion
- Chart library installed and tested
- Export libraries integrated


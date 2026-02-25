# Feature Specification: Fee & Salary Reports Enhancement

**Feature**: 003-reports-enhancement  
**Created**: February 2026  
**Status**: Draft  
**Priority**: P1 (High Value, Post-MVP)  
**Related PRD**: Sections 4.7 (Dashboard & Reports)

---

## Overview

This specification covers enhancements to existing Fee and Salary reporting modules to provide administrators with comprehensive analytics, trend analysis, and actionable insights.

### Current State
- ✅ Fee Report Page - Basic table view with filtering
- ✅ Salary Payment Page - Status tracking and payments list
- ✅ Payroll Dashboard - Summary cards and payroll details

### Target State
- 📊 Advanced analytics with charts and trends
- 📈 Period comparison and forecasting
- 📋 Export-ready reports (CSV, PDF)
- 💡 Actionable insights (overdue analysis, spend trends)
- 🎯 Performance metrics and KPIs

---

## User Stories & Acceptance Criteria

### **US1: Fee Collection Analytics Dashboard**
**Priority**: P1 | **Effort**: Medium | **Timeline**: 2 weeks

**Business Value**: School administrators need to understand fee collection patterns, identify problem areas, and forecast revenue.

**Scenario**: 
Principal logs in and wants to see: 
- How much fee was collected this month (vs last month)?
- Which students are overdue? (aging report)
- What percentage is collected by category? (tuition vs transport)
- Trend over last 6 months

**Acceptance Criteria**:
1. Dashboard shows total collected amount with % change from previous month
2. Collection status breakdown (Paid, Partial, Due, Overdue)
3. Category-wise collection chart (pie chart)
4. Monthly collection trend (line chart for last 6 months)
5. Outstanding dues report with aging (30, 60, 90+ days)
6. Top 5 overdraft students highlighted
7. All charts are interactive (hover shows exact values)
8. Export full report as PDF with charts and summary

---

### **US2: Fee Outstanding Analysis Report**
**Priority**: P1 | **Effort**: Medium | **Timeline**: 1.5 weeks

**Business Value**: Detailed insight into who owes fees, how much, and for how long enables targeted collection efforts.

**Scenario**: 
Accountant needs to send reminders. They filter to see:
- Students with dues > ₹5,000
- Overdue by > 60 days  
- Group by class/section
- Export to send collection notices

**Acceptance Criteria**:
1. Filter by: Due amount range, Overdue days, Student status, Class/Section
2. Table shows: Student name, Class, Due amount, Days overdue, Last payment date, Contact info
3. Sorting by: Amount due, Days overdue, Student name
4. Bulk actions: Generate reminder letters, export selected list
5. Summary: Total outstanding, student count, average due amount
6. Export as CSV with all details for mail merge

---

### **US3: Salary Expense Analytics**
**Priority**: P1 | **Effort**: Medium | **Timeline**: 2 weeks

**Business Value**: CFO/Principal needs visibility into salary expenses, trends, and forecasting for budgeting.

**Scenario**: 
Principal wants to:
- See total monthly salary spend (vs budget)
- Breakdown of salary components (base, bonus, deductions)
- Teacher-wise salary comparison
- Bonus payout analysis (who got bonus, total bonus cost)
- Net salary trend over 12 months

**Acceptance Criteria**:
1. Period selector (month, custom date range)
2. Summary card: Total net salary, Avg salary per teacher, Total bonus paid
3. Salary component breakdown chart (stacked bar: base, bonus, deductions)
4. Teacher-wise salary table (sortable by amount, name, status)
5. Bonus analysis: Eligible teachers, bonus % of payroll, bonus trend
6. 12-month salary expense trend (line chart)
7. Year-over-year comparison (current year vs previous)
8. Export as Excel with multiple sheets (summary, detail, trends)

---

### **US4: Attendance-to-Salary Correlation**
**Priority**: P2 | **Effort**: Medium | **Timeline**: 1.5 weeks

**Business Value**: Validates that salary deductions are correctly applied based on attendance.

**Scenario**: 
HR manager audits:
- Teacher marked absent 3 days → X deduction applied?
- Attendance % 95% → Bonus eligible?
- Verify deductions match policy (₹Xper day absent)

**Acceptance Criteria**:
1. Show teacher attendance % and salary in same view
2. Highlight deductions with formula used (days absent × daily rate)
3. Flag if deduction doesn't match policy
4. Bonus eligibility highlighted (green if ≥90%, red if <90%)
5. Period: Selectable month/range
6. Export for audit trail

---

### **US5: Budget vs Actual Reports**
**Priority**: P2 | **Effort**: Medium | **Timeline**: 2 weeks

**Business Value**: Track if collection and expenses are within budget.

**Scenario**: 
School planned:
- Collect ₹X in fees per month
- Spend ₹Y in salaries per month

Actual shows:
- Collect ₹A (variance: +₹B or -₹B)
- Spend ₹C (variance: +₹D or -₹D)
- Forecast next 3 months based on trend

**Acceptance Criteria**:
1. Budget entry page (set monthly fee collection target, salary budget)
2. Actual vs Budget comparison charts
3. Variance analysis (over/under, %)
4. Rolling 3-month forecast based on trend
5. Alert if projected surplus/deficit exceeds threshold
6. Export comparison report

---

## Technical Requirements

### Backend APIs to Create/Enhance

#### Fee Reports
```
1. GET /api/v1/fees/report/summary?startDate=&endDate=
   Returns: {totalCollected, totalPending, totalOverdue, collectionRate%}

2. GET /api/v1/fees/report/trends?months=6
   Returns: [{month, collected, pending, overdue}, ...]

3. GET /api/v1/fees/report/category-breakdown?month=
   Returns: [{category, collected, pending}, ...]

4. GET /api/v1/fees/report/outstanding?days=&amountMin=&amountMax=
   Returns: [{studentName, dueAmount, daysOverdue, lastPaymentDate}, ...]

5. GET /api/v1/fees/report/by-student/{studentId}/history
   Returns: [{month, dueAmount, paidAmount, method}, ...]

6. POST /api/v1/fees/report/export?format=pdf&type=collection_summary
   Returns: PDF file download
```

#### Salary Reports
```
1. GET /api/v1/salaries/report/summary?startDate=&endDate=
   Returns: {totalNetSalary, avgSalary, totalBonus, bonusPercentage%}

2. GET /api/v1/salaries/report/trends?months=12
   Returns: [{month, totalNetSalary, totalBonus, avgDeductions}, ...]

3. GET /api/v1/salaries/report/attendance-correlation?period=month
   Returns: [{teacherId, teacherName, attendance%, deductions, bonusEligible}, ...]

4. GET /api/v1/salaries/report/component-breakdown?month=
   Returns: {totalBase, totalBonus, totalDeductions, netTotal}

5. GET /api/v1/salaries/report/teacher-comparison?month=
   Returns: [{teacherId, name, baseSalary, bonus, deductions, netSalary}, ...]

6. POST /api/v1/salaries/report/export?format=excel&type=expense_summary
   Returns: Excel file download
```

### Frontend Components to Create

#### Fee Reports
- `FeeAnalyticsDashboard.tsx` - Main analytics view with charts
- `FeeOutstandingReport.tsx` - Aging analysis detail view
- `FeeCategoryBreakdown.tsx` - Category pie chart
- `FeeTrendChart.tsx` - Monthly trend line chart
- `OutstandingStudentsList.tsx` - Sortable table with filters

#### Salary Reports
- `SalaryAnalyticsDashboard.tsx` - Main analytics view
- `SalaryComponentChart.tsx` - Stacked bar chart
- `SalaryTrendChart.tsx` - 12-month trend
- `TeacherSalaryComparison.tsx` - Teacher-wise table
- `AttendanceToSalaryView.tsx` - Correlation analysis

### Libraries
- **Charts**: Chart.js or Recharts (preferred: Recharts - already similar to PayrollPage)
- **Export**: pdfkit/jsPDF for PDF, xlsx for Excel
- **Date**: date-fns for date manipulation

---

## Data Model Changes (No DB schema changes needed)

All data already exists in DB. We're adding:
- DTOs for analytics responses
- Query handlers for aggregated data
- Report generation services (PDF/Excel export)

---

## Implementation Phases

### Phase 1 (Week 1-2): Fee Collections Analytics
- Backend: Summary APIs, trends, category breakdown
- Frontend: FeeAnalyticsDashboard with charts
- Export: CSV export for outstanding report

### Phase 2 (Week 3): Fee Outstanding Analysis
- Backend: Outstanding query with filtering
- Frontend: Outstanding detail page with aging
- Export: PDF with aging report

### Phase 3 (Week 4): Salary Analytics
- Backend: Similar to fee analytics but for salary
- Frontend: SalaryAnalyticsDashboard
- Export: Excel with multiple sheets

### Phase 4 (Week 5): Correlation & Advanced Features
- Backend: Attendance-to-salary correlation
- Frontend: Correlation view
- Budget vs Actual (if time permits)

---

## Success Metrics

- ✅ 10+ analytics queries responding < 500ms
- ✅ Charts render smoothly even with 500+ data points
- ✅ PDF/Excel exports < 30 seconds
- ✅ Admin can identify top 10 overdues in < 2 clicks
- ✅ Salary trends visible at a glance
- ✅ 95%+ data accuracy vs manual calculation

---

## Future Enhancements (Phase 3+)

- Real-time notifications for threshold breaches (overdue > ₹50k)
- Predictive analytics (forecasted next month's collection)
- Department/class-wise breakdown
- Graphical insights with custom date ranges
- Mobile app reports
- Email scheduled reports


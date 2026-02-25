# Unused Report Endpoints Implementation Guide

## Overview
There are **11 sophisticated reporting endpoints** across two controllers that have been implemented in the backend but are not connected to the frontend UI.

---

## 📊 FeeReportsController - 5 Unused Endpoints

### 1. `GET /feereports/collection-summary`
**Purpose:** Get fee collection summary for a date range with optional comparison period

**Query Parameters:**
```
- startDate (required): YYYY-MM-DD
- endDate (required): YYYY-MM-DD
- category (optional): Fee category filter
- prevStartDate (optional): Previous period start for comparison
- prevEndDate (optional): Previous period end for comparison
```

**Response:**
```typescript
{
  currentPeriod: {
    totalCollected: number,
    numberOfTransactions: number,
    averagePerTransaction: number,
    completionPercentage: number
  },
  previousPeriod: {
    totalCollected: number,
    numberOfTransactions: number,
    averagePerTransaction: number,
    completionPercentage: number
  },
  growth: {
    percentageChange: number,
    absoluteChange: number
  }
}
```

**Use Case:** Dashboard widget showing fee collection metrics and YoY/period comparison

**Frontend Implementation Needed:**
- New page: `FeeReportsPage.tsx`
- Component: `FeeCollectionSummary.tsx`
- Date range picker for comparison analysis

---

### 2. `GET /feereports/monthly-trend`
**Purpose:** Monthly fee collection trend analysis

**Query Parameters:**
```
- startDate (required): YYYY-MM-DD
- endDate (required): YYYY-MM-DD
- category (optional): Fee category filter
```

**Response:**
```typescript
{
  month: string,        // "2026-02"
  collected: number,
  pending: number,
  overdue: number,
  trend: number         // Percentage change from previous month
}[]
```

**Use Case:** Line/bar chart showing fee collection trends over time

**Frontend Implementation Needed:**
- Chart component (line chart with trend indicators)
- Add to Reports page
- Integration with analytics library

---

### 3. `GET /feereports/by-category`
**Purpose:** Fee collection breakdown by category

**Response:**
```typescript
{
  category: string,
  amount: number,
  collected: number,
  pending: number,
  percentageOfTotal: number
}[]
```

**Use Case:** Pie/donut chart showing fee distribution by type

**Frontend Implementation Needed:**
- Pie chart component
- Add to Reports page
- Category-wise filter and details view

---

### 4. `GET /feereports/outstanding`
**Purpose:** Outstanding and overdue fees with aging analysis (critical report)

**Query Parameters:**
```
- asOfDate (optional): Analysis date (defaults to today)
- agingBucket (optional): 0-30, 31-60, 61-90, 90+
- minAmount (optional): Minimum due amount filter
- sortBy (optional): daysoverdue | dueamount | name | class
- descending (optional): Sort order
```

**Response:**
```typescript
{
  studentId: string,
  studentName: string,
  enrollmentNumber: string,
  className: string,
  dueAmount: decimal,
  daysOverdue: number,
  agingBucket: string,  // "0-30", "31-60", etc.
  lastPaymentDate?: string
}[]
```

**Use Case:** Focus collection efforts on overdue accounts (collections dashboard)

**Frontend Implementation Needed:**
- New page: `OutstandingFeesPage.tsx`
- Advanced filtering by aging bucket
- Sortable table
- Action items (send reminder, collection action)
- Export to Excel for collections team

---

### 5. `GET /feereports/student/{studentId}/payment-history`
**Purpose:** Student's payment history for a date range

**Query Parameters:**
```
- startDate (required): YYYY-MM-DD
- endDate (required): YYYY-MM-DD
```

**Response:**
```typescript
{
  paymentDate: string,
  amountPaid: number,
  receiptNumber: string,
  paymentMethod: string,
  notes?: string,
  balanceAfterPayment: number
}[]
```

**Use Case:** Detailed payment history per student (accessible from student detail page)

**Frontend Implementation Needed:**
- Add to Student Detail Page
- Timeline/table view of payments
- Filter by date range
- Print/export receipt

---

## 💰 SalaryReportsController - 6 Unused Endpoints

### 1. `GET /salaryreports/expense-summary`
**Purpose:** Salary expense summary with period comparison

**Query Parameters:**
```
- startDate (required): YYYY-MM-DD
- endDate (required): YYYY-MM-DD
- prevStartDate (optional): Previous period start
- prevEndDate (optional): Previous period end
```

**Response:**
```typescript
{
  currentPeriod: {
    totalExpense: number,
    baseSalary: number,
    bonuses: number,
    deductions: number,
    numberOfEmployees: number
  },
  previousPeriod: { /* same structure */ },
  growth: {
    percentageChange: number,
    absoluteChange: number
  }
}
```

**Use Case:** Financial dashboard - salary expense tracking and budgeting

**Frontend Implementation Needed:**
- New page: `SalaryReportsPage.tsx`
- Summary cards with metrics
- Period comparison visualization

---

### 2. `GET /salaryreports/monthly-trend`
**Purpose:** Monthly salary expense trend

**Query Parameters:**
```
- startDate (required): YYYY-MM-DD
- endDate (required): YYYY-MM-DD
```

**Response:**
```typescript
{
  month: string,        // "2026-02"
  baseSalary: number,
  bonuses: number,
  deductions: number,
  totalExpense: number,
  trend: number         // % change from previous month
}[]
```

**Use Case:** Line chart showing salary expenses over time

**Frontend Implementation Needed:**
- Stacked area/bar chart
- Monthly breakdown
- Component reusable across reports

---

### 3. `GET /salaryreports/component-breakdown`
**Purpose:** Salary expense breakdown by component

**Response:**
```typescript
{
  baseSalary: {
    amount: number,
    percentage: number,
    headCount: number
  },
  bonuses: {
    amount: number,
    percentage: number,
    headCount: number
  },
  deductions: {
    amount: number,
    percentage: number,
    description: string[]
  }
}
```

**Use Case:** Understanding salary structure costs (pie chart showing distribution)

**Frontend Implementation Needed:**
- Pie chart with drill-down capability
- Component-wise details table

---

### 4. `GET /salaryreports/teacher-comparison`
**Purpose:** Teacher-wise salary comparison (important for HR)

**Query Parameters:**
```
- startDate (required): YYYY-MM-DD
- endDate (required): YYYY-MM-DD
- status (optional): Pending | Approved | Paid
- sortBy (optional): name | netsalary | bonus | deduction
- descending (optional): Sort order
```

**Response:**
```typescript
{
  teacherId: string,
  teacherName: string,
  baseSalary: number,
  bonus: number,
  deduction: number,
  netSalary: number,
  status: string,
  paymentDate?: string
}[]
```

**Use Case:** HR analytical reports - compare salaries, identify outliers

**Frontend Implementation Needed:**
- New page: `TeacherSalaryComparisonPage.tsx`
- Sortable/filterable table
- Charts for visualization

---

### 5. `GET /salaryreports/attendance-correlation`
**Purpose:** Analyze relationship between attendance and salary deductions

**Query Parameters:**
```
- month (required): YYYY-MM-DD (month to analyze)
- onlyDiscrepancies (optional): Show only mismatches
```

**Response:**
```typescript
{
  teacherId: string,
  teacherName: string,
  expectedDeduction: number,     // Based on attendance
  actualDeduction: number,        // What was applied
  discrepancy: number,           // Difference
  attendancePercentage: number,
  workingDays: number,
  presentDays: number,
  absentDays: number
}[]
```

**Use Case:** Audit trail - verify salary calculations match attendance records

**Frontend Implementation Needed:**
- New page: `AttendanceToSalaryCorrelationPage.tsx`
- Highlight discrepancies
- Adjustment interface
- Export for audit

---

### 6. `GET /salaryreports/budget-vs-actual`
**Purpose:** Compare budgeted vs actual salary expenses

**Query Parameters:**
```
- reportType (required): FeeCollection | SalaryExpense
- startDate (required): YYYY-MM-DD
- endDate (required): YYYY-MM-DD
- groupBy (optional): month | category | class
```

**Response:**
```typescript
{
  period: string,       // Month or category
  budgeted: number,
  actual: number,
  variance: number,     // Positive = over budget
  variancePercentage: number
}[]
```

**Use Case:** Financial planning and variance analysis

**Frontend Implementation Needed:**
- New page: `BudgetVsActualPage.tsx`
- Comparison chart (side-by-side bar chart)
- Variance highlighting (red for over, green for under)

---

## 🗺️ Implementation Roadmap

### Phase 1: Foundation (Week 1-2)
- [ ] Create Reports index page
- [ ] Create shared chart components (line, bar, pie)
- [ ] Create date range filter component
- [ ] Create table component for detailed reports

### Phase 2: Fee Reports (Week 2-3)
- [ ] `FeeCollectionSummary` - Dashboard widget + full page
- [ ] `MonthlyTrend` - Line chart
- [ ] `ByCategory` - Pie chart
- [ ] `Outstanding` - Collections dashboard (HIGH PRIORITY)
- [ ] `StudentPaymentHistory` - Detail view in student page

### Phase 3: Salary Reports (Week 3-4)
- [ ] `ExpenseSummary` - Dashboard metrics
- [ ] `MonthlyTrend` - Stacked area chart
- [ ] `ComponentBreakdown` - Pie chart
- [ ] `TeacherComparison` - Advanced table + analytics
- [ ] `AttendanceCorrelation` - Audit tool
- [ ] `BudgetVsActual` - Variance analysis

### Phase 4: Integration & Polish (Week 4-5)
- [ ] Add menu items for new reports
- [ ] Export functionality (CSV, PDF)
- [ ] Dashboard widgets
- [ ] Role-based access (Admin/Accountant/HR)
- [ ] Testing and optimization

---

## 📁 Suggested Frontend Structure

```
frontend/src/
├── pages/
│   ├── ReportsPage/
│   │   ├── FeeReportsPage.tsx
│   │   ├── SalaryReportsPage.tsx
│   │   ├── OutstandingFeesPage.tsx
│   │   └── TeacherSalaryComparisonPage.tsx
│
├── components/
│   ├── Reports/
│   │   ├── FeeCollectionSummary.tsx
│   │   ├── SalaryExpenseSummary.tsx
│   │   ├── OutstandingFeesTable.tsx
│   │   ├── MonthlyTrendChart.tsx
│   │   ├── ComponentBreakdownChart.tsx
│   │   ├── DateRangeFilter.tsx
│   │   └── ReportExporter.tsx
│   │
│   └── Charts/
│       ├── LineChart.tsx
│       ├── BarChart.tsx
│       ├── PieChart.tsx
│       └── AreaChart.tsx
│
├── services/
│   ├── reportService.ts      (NEW - centralized report API calls)
│   └── api.ts                (existing)
│
└── hooks/
    └── useReports.ts         (NEW - report-related queries)
```

---

## 🔗 Service Integration Example

```typescript
// frontend/src/services/reportService.ts
export const reportService = {
  // Fee Reports
  getFeeCollectionSummary: async (params: FeeReportParams) => {
    const response = await api.get('/feereports/collection-summary', { params });
    return response.data;
  },
  
  getMonthlyFeeTrend: async (params: DateRangeParams) => {
    const response = await api.get('/feereports/monthly-trend', { params });
    return response.data;
  },
  
  getOutstandingFees: async (params: OutstandingFeesParams) => {
    const response = await api.get('/feereports/outstanding', { params });
    return response.data;
  },

  // Salary Reports
  getSalaryExpenseSummary: async (params: DateRangeParams) => {
    const response = await api.get('/salaryreports/expense-summary', { params });
    return response.data;
  },
  
  // ... etc
};
```

---

## 💡 High-Priority Implementations

### Tier 1 (Most Valuable)
1. **Outstanding Fees Report** - Collections tool, direct business value
2. **Teacher Salary Comparison** - HR analytics, decision support
3. **Budget vs Actual** - Financial planning

### Tier 2 (Good to Have)
4. **Fee Collection Summary** - Dashboard metric
5. **Salary Expense Summary** - Dashboard metric
6. **Monthly Trends** - Both fee and salary

### Tier 3 (Nice to Have)
7. Component breakdowns
8. Attendance correlation (audit)
9. Student payment history (detail view)

---

## 🤔 Questions for Clarification

1. **Priority:** Which reports are most important for your school operations?
2. **Timelines:** When would these reports need to be available?
3. **Access Control:** Should reports be available to all users or role-restricted?
4. **Export Formats:** Do you need PDF, Excel, or just on-screen viewing?
5. **Dashboards:** Should any of these be dashboard widgets for quick overview?

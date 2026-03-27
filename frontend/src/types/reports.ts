/**
 * Fee Collection Summary Statistics
 */
export interface FeeCollectionSummaryDto {
  totalCollected: number;
  totalPending: number;
  totalOverdue: number;
  totalExpected: number;
  collectionRate: number;
  paidStudents: number;
  partialStudents: number;
  dueStudents: number;
  overdueStudents: number;
  previousPeriodCollectionRate?: number;
  collectionRateTrend?: number;
}

/**
 * Monthly Collection Trend Data
 */
export interface MonthlyCollectionTrendDto {
  month: string; // YYYY-MM format
  collected: number;
  pending: number;
  overdue: number;
  collectionRate: number;
  expected: number;
}

/**
 * Fee Collection by Category Breakdown
 */
export interface FeeCollectionByCategoryDto {
  category: string;
  collected: number;
  pending: number;
  overdue: number;
  collectionPercentage: number;
  percentageOfTotal: number;
  count: number;
}

/**
 * Outstanding Fee Analysis (Aging Report)
 */
export interface OutstandingFeeDto {
  studentId: string;
  studentInfo: string;
  classSection: string;
  dueAmount: number;
  daysOverdue: number;
  dueDate: string; // ISO date string
  lastPaymentDate?: string;
  agingBucket: string; // "0-30", "31-60", "61-90", "90+"
  remarks?: string;
  contactInfo?: string;
  isActive: boolean;
}

/**
 * Student Payment History Over Time
 */
export interface StudentPaymentHistoryDto {
  month: string; // YYYY-MM format
  dueAmount: number;
  paidAmount: number;
  status: string; // "Paid", "Partial", "Due", "Overdue"
  paymentMethod?: string;
  dueDate: string;
  paymentDate?: string;
  referenceNumber?: string;
  balance: number;
}

/**
 * Salary Expense Summary
 */
export interface SalaryExpenseSummaryDto {
  totalNetSalary: number;
  averageSalary: number;
  totalBaseSalary: number;
  totalBonus: number;
  totalDeductions: number;
  StaffCount: number;
  bonusRecipients: number;
  bonusPercentage: number;
  deductionPercentage: number;
  previousPeriodTotal?: number;
  expenseTrend?: number;
}

/**
 * Monthly Salary Expense Trend
 */
export interface MonthlySalaryTrendDto {
  month: string; // YYYY-MM format
  totalNetSalary: number;
  totalBaseSalary: number;
  totalBonus: number;
  totalDeductions: number;
  StaffCount: number;
  bonusRecipients: number;
  averageSalary: number;
}

/**
 * Salary Component Breakdown
 */
export interface SalaryComponentBreakdownDto {
  baseSalary: number;
  bonus: number;
  deductions: number;
  netSalary: number;
  basePercentage: number;
  bonusPercentage: number;
  deductionsPercentage: number;
  recordCount: number;
}

/**
 * Staff-wise Salary Comparison
 */
export interface StaffSalaryComparisonDto {
  StaffId: string;
  StaffName: string;
  baseSalary: number;
  bonus: number;
  deductions: number;
  netSalary: number;
  attendancePercentage?: number;
  bonusEligible: boolean;
  status: string; // "Pending", "Approved", "Paid"
}

/**
 * Attendance to Salary Correlation View
 */
export interface AttendanceToSalaryCorrelationDto {
  StaffId: string;
  StaffName: string;
  attendancePercentage: number;
  presentDays: number;
  absentDays: number;
  totalDays: number;
  calculatedDeduction: number;
  actualDeduction: number;
  deductionDifference: number;
  bonusEligible: boolean;
  bonusAmount: number;
  baseSalary: number;
  hasDiscrepancy: boolean;
  discrepancyReason?: string;
}

/**
 * Budget vs Actual Comparison
 */
export interface BudgetVsActualDto {
  budgetedAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  month: string;
  category: string; // "Fee Collection" or "Salary Expense"
}

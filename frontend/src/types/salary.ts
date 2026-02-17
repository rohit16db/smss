export interface SalaryPaymentDto {
  id: string;
  teacherId: string;
  teacherName: string;
  periodStartDate: string;
  periodEndDate: string;
  baseSalary: number;
  deductions: number;
  bonus: number;
  netSalary: number;
  status: 'Pending' | 'Approved' | 'Paid' | 'Cancelled' | 'OnHold';
  paidDate?: string;
  referenceNumber?: string;
  paymentMethod?: 'Cash' | 'BankTransfer' | 'Cheque' | 'MobilePayment' | 'Other';
  remarks?: string;
  createdAt: string;
}

export interface SalaryPaymentReportDto {
  monthStart: string;
  monthEnd: string;
  totalTeachers: number;
  paidTeachers: number;
  pendingTeachers: number;
  totalBaseSalary: number;
  totalDeductions: number;
  totalBonus: number;
  totalNetSalary: number;
  paymentDetails: SalaryPaymentDto[];
}

export interface SalaryHistoryDto {
  teacherId: string;
  teacherName: string;
  paymentHistory: SalaryPaymentDto[];
  totalSalaryPaid: number;
  averageMonthlySalary: number;
  totalPayments: number;
  pendingPayments: number;
}

export interface SalarySummaryDto {
  totalSalaryExpense: number;
  totalPaid: number;
  totalPending: number;
  teacherCount: number;
  paidCount: number;
  pendingCount: number;
  averageSalaryPerTeacher: number;
}

export interface CreateSalaryPaymentDto {
  teacherId: string;
  periodStartDate: string;
  periodEndDate: string;
  baseSalary: number;
  deductions: number;
  bonus: number;
  referenceNumber?: string;
  paymentMethod?: string;
  remarks?: string;
}

export interface UpdateSalaryPaymentStatusDto {
  status: string;
  paidDate?: string;
  referenceNumber?: string;
  remarks?: string;
}

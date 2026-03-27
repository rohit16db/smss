export interface SalaryPaymentDto {
  id: string;
  StaffId: string;
  StaffName: string;
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
  totalStaffs: number;
  paidStaffs: number;
  pendingStaffs: number;
  totalBaseSalary: number;
  totalDeductions: number;
  totalBonus: number;
  totalNetSalary: number;
  paymentDetails: SalaryPaymentDto[];
}

export interface SalaryHistoryDto {
  StaffId: string;
  StaffName: string;
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
  StaffCount: number;
  paidCount: number;
  pendingCount: number;
  averageSalaryPerStaff: number;
}

export interface CreateSalaryPaymentDto {
  StaffId: string;
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

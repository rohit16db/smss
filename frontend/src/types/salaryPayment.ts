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
  status: string;
  paidDate?: string;
  referenceNumber?: string;
  paymentMethod?: string;
  remarks?: string;
  createdAt: string;
  updatedAt: string;
}

export interface SalaryPaymentSummaryDto {
  totalPayments: number;
  pendingCount: number;
  approvedCount: number;
  paidCount: number;
  onHoldCount: number;
  cancelledCount: number;
  totalBaseSalary: number;
  totalDeductions: number;
  totalBonus: number;
  totalNetSalary: number;
  totalPaidAmount: number;
}

export interface UpdateSalaryPaymentStatusDto {
  status: string;
  paidDate?: string;
  referenceNumber?: string;
  remarks?: string;
}

export interface MarkSalaryAsPaidDto {
  paidDate: string;
  referenceNumber: string;
  paymentMethod?: string;
}

export interface UpdateSalaryPaymentDto {
  baseSalary?: number;
  deductions?: number;
  bonus?: number;
  remarks?: string;
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

export type SalaryPaymentStatus = 'Pending' | 'Approved' | 'Paid' | 'OnHold' | 'Cancelled';

export type PaymentMethod = 'Cash' | 'BankTransfer' | 'Cheque' | 'MobilePayment' | 'Other';

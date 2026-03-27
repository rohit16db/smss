export interface StaffAttendancePayrollDto {
  id: string;
  staffId: string;
  staffName: string;
  attendanceDate: string;
  status: 'Present' | 'Absent' | 'Leave';
  createdAt: string;
}

export interface StaffPayrollReportDto {
  staffId: string;
  staffName: string;
  staffImagePath?: string;
  baseSalary: number;
  periodStartDate: string;
  periodEndDate: string;
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  leaveDays: number;
  attendancePercentage: number;
  grossSalary: number;
  deductionsForAbsence: number;
  bonusAmount: number;
  netSalary: number;
  isBonusEligible: boolean;
  bonusEligibilityReason: string;
}

export interface BonusEligibilityDto {
  staffId: string;
  staffName: string;
  attendancePercentage: number;
  bonusPercentage: number;
  bonusAmount: number;
  isEligible: boolean;
  reason: string;
}

export interface StaffAttendanceSummaryDto {
  staffId: string;
  staffName: string;
  totalDays: number;
  presentDays: number;
  absentDays: number;
  leaveDays: number;
  attendancePercentage: number;
}

export interface PayrollPeriodReportDto {
  generatedAt: string;
  periodStartDate: string;
  periodEndDate: string;
  staffPayrolls: StaffPayrollReportDto[];
  totalPayrollAmount: number;
  totalBonusAmount: number;
  eligibleStaffs: number;
}

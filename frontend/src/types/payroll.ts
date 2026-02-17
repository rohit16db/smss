export interface TeacherAttendancePayrollDto {
  id: string;
  teacherId: string;
  teacherName: string;
  attendanceDate: string;
  status: 'Present' | 'Absent' | 'Leave';
  createdAt: string;
}

export interface TeacherPayrollReportDto {
  teacherId: string;
  teacherName: string;
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
  teacherId: string;
  teacherName: string;
  attendancePercentage: number;
  bonusPercentage: number;
  bonusAmount: number;
  isEligible: boolean;
  reason: string;
}

export interface TeacherAttendanceSummaryDto {
  teacherId: string;
  teacherName: string;
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
  teacherPayrolls: TeacherPayrollReportDto[];
  totalPayrollAmount: number;
  totalBonusAmount: number;
  eligibleTeachers: number;
}

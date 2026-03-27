export interface DashboardSummaryCard {
  title: string;
  value: number;
  unit?: string;
  percentageChange?: number;
  iconName?: string;
  trendDirection?: 'up' | 'down' | 'stable';
}

export interface FinancialSummary {
  totalFeesCollected: number;
  totalOutstandingFees: number;
  totalExpectedFees: number;
  collectionPercentage: number;
  totalStudents: number;
  averagePaymentPerStudent: number;
}

export interface AttendanceSummary {
  averageStudentAttendance: number;
  averageStaffAttendance: number;
  totalStaffs: number;
  totalStudents: number;
  presentStudentsTodayCount: number;
  absentStudentsTodayCount: number;
}

export interface AcademicSummary {
  totalStudents: number;
  totalStaffs: number;
  totalClasses: number;
  activeStudents: number;
  activeStaffs: number;
}

export interface DashboardSummaryResponse {
  generatedAt: string;
  academicSummary: AcademicSummary;
  financialSummary: FinancialSummary;
  attendanceSummary: AttendanceSummary;
  summaryCards: DashboardSummaryCard[];
}

export interface FeeCollectionTrend {
  date: string;
  collectedAmount: number;
  outstandingAmount: number;
  targetAmount: number;
}

export interface AttendanceTrend {
  date: string;
  studentAttendancePercentage: number;
  StaffAttendancePercentage: number;
}

export interface OutstandingFeeDetail {
  studentId: string;
  studentName: string;
  outstandingAmount: number;
  dueDate: string;
  daysOverdue: number;
  status: 'Due Soon' | 'Overdue' | 'Severely Overdue';
}

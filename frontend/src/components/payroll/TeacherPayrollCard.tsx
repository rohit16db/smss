import React from 'react';
import { TrendingUp, TrendingDown, AlertCircle } from 'lucide-react';
import type { TeacherPayrollReportDto } from '../../types/payroll';

interface TeacherPayrollCardProps {
  payroll: TeacherPayrollReportDto;
}

export const TeacherPayrollCard: React.FC<TeacherPayrollCardProps> = ({
  payroll,
}) => {
  const getAttendanceColor = (percentage: number) => {
    if (percentage >= 90) return 'text-green-600';
    if (percentage >= 75) return 'text-blue-600';
    if (percentage >= 60) return 'text-orange-600';
    return 'text-red-600';
  };

  const getAttendanceLabel = (percentage: number) => {
    if (percentage >= 90) return 'Excellent';
    if (percentage >= 75) return 'Good';
    if (percentage >= 60) return 'Fair';
    return 'Poor';
  };

  const getTrendIcon = (percentage: number) => {
    return percentage >= 75 ? (
      <TrendingUp className="w-4 h-4" />
    ) : (
      <TrendingDown className="w-4 h-4" />
    );
  };

  return (
    <div className="border border-gray-200 rounded-lg p-6 hover:shadow-lg transition-shadow">
      {/* Header */}
      <div className="flex justify-between items-start mb-4">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">
            {payroll.teacherName}
          </h3>
          <p className="text-sm text-gray-500 mt-1">
            {new Date(payroll.periodStartDate).toLocaleDateString()} -{' '}
            {new Date(payroll.periodEndDate).toLocaleDateString()}
          </p>
        </div>
        {payroll.isBonusEligible && (
          <div className="bg-green-100 text-green-800 px-3 py-1 rounded-full text-xs font-semibold">
            Bonus ✓
          </div>
        )}
      </div>

      {/* Attendance Section */}
      <div className="mb-4 p-3 bg-gray-50 rounded">
        <div className="flex justify-between items-center mb-2">
          <span className="text-sm font-medium text-gray-600">Attendance</span>
          <div className={`flex items-center gap-1 ${getAttendanceColor(payroll.attendancePercentage)}`}>
            {payroll.attendancePercentage}%
            {getTrendIcon(payroll.attendancePercentage)}
          </div>
        </div>
        <p className={`text-xs font-medium ${getAttendanceColor(payroll.attendancePercentage)}`}>
          {getAttendanceLabel(payroll.attendancePercentage)}
        </p>
        <div className="grid grid-cols-3 gap-2 mt-2 text-xs">
          <div>
            <p className="text-gray-500">Present</p>
            <p className="font-semibold text-gray-900">{payroll.presentDays}</p>
          </div>
          <div>
            <p className="text-gray-500">Absent</p>
            <p className="font-semibold text-gray-900">{payroll.absentDays}</p>
          </div>
          <div>
            <p className="text-gray-500">Leave</p>
            <p className="font-semibold text-gray-900">{payroll.leaveDays}</p>
          </div>
        </div>
      </div>

      {/* Salary Section */}
      <div className="space-y-2">
        <div className="flex justify-between">
          <span className="text-sm text-gray-600">Base Salary:</span>
          <span className="font-medium">₹{payroll.baseSalary.toLocaleString()}</span>
        </div>
        <div className="flex justify-between">
          <span className="text-sm text-gray-600">Deductions:</span>
          <span className="font-medium text-red-600">
            -₹{payroll.deductionsForAbsence.toLocaleString()}
          </span>
        </div>
        {payroll.isBonusEligible && (
          <div className="flex justify-between">
            <span className="text-sm text-gray-600">Bonus (10%):</span>
            <span className="font-medium text-green-600">
              +₹{payroll.bonusAmount.toLocaleString()}
            </span>
          </div>
        )}
        <div className="border-t border-gray-200 pt-2 mt-2 flex justify-between">
          <span className="text-sm font-semibold text-gray-900">Net Salary:</span>
          <span className="text-lg font-bold text-gray-900">
            ₹{payroll.netSalary.toLocaleString()}
          </span>
        </div>
      </div>

      {/* Bonus Eligibility Reason */}
      {!payroll.isBonusEligible && (
        <div className="mt-4 p-2 bg-yellow-50 rounded flex items-start gap-2">
          <AlertCircle className="w-4 h-4 text-yellow-600 mt-0.5 flex-shrink-0" />
          <p className="text-xs text-yellow-800">{payroll.bonusEligibilityReason}</p>
        </div>
      )}
    </div>
  );
};

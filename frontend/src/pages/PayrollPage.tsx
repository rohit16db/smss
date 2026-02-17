import React, { useState } from 'react';
import {
  usePayrollReport,
  useBonusEligibility,
  useAttendanceSummary,
} from '../services/payrollService';
import { TeacherPayrollCard } from '../components/payroll/TeacherPayrollCard';
import { BonusEligibilityList } from '../components/payroll/BonusEligibilityList';
import { Calendar, DollarSign, Award } from 'lucide-react';

export const PayrollPage: React.FC = () => {
  const today = new Date().toISOString().split('T')[0];
  const firstDayOfMonth = new Date(
    new Date().getFullYear(),
    new Date().getMonth(),
    1
  )
    .toISOString()
    .split('T')[0];

  const [startDate, setStartDate] = useState(firstDayOfMonth);
  const [endDate, setEndDate] = useState(today);
  const [activeTab, setActiveTab] = useState<'payroll' | 'bonus' | 'attendance'>(
    'payroll'
  );

  const payrollQuery = usePayrollReport(startDate, endDate);
  const bonusQuery = useBonusEligibility(startDate, endDate);
  const attendanceQuery = useAttendanceSummary(startDate, endDate);

  const handleRefresh = () => {
    payrollQuery.refetch();
    bonusQuery.refetch();
    attendanceQuery.refetch();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Teacher Payroll</h1>
        <p className="text-gray-600 mt-2">
          Manage teacher salaries, bonuses, and attendance tracking
        </p>
      </div>

      {/* Date Range Selector */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        <div className="flex flex-col sm:flex-row gap-4 items-end">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Start Date
            </label>
            <input
              type="date"
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              End Date
            </label>
            <input
              type="date"
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>
          <button
            onClick={handleRefresh}
            className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
          >
            Refresh
          </button>
        </div>
      </div>

      {/* Summary Cards */}
      {payrollQuery.data && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Total Payroll */}
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">
                  Total Payroll
                </p>
                <p className="text-2xl font-bold text-gray-900 mt-2">
                  ₹
                  {payrollQuery.data.totalPayrollAmount.toLocaleString(
                    'en-IN',
                    {
                      maximumFractionDigits: 0,
                    }
                  )}
                </p>
              </div>
              <DollarSign className="w-10 h-10 text-blue-600" />
            </div>
          </div>

          {/* Total Bonus */}
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">
                  Total Bonus
                </p>
                <p className="text-2xl font-bold text-green-600 mt-2">
                  ₹
                  {payrollQuery.data.totalBonusAmount.toLocaleString('en-IN', {
                    maximumFractionDigits: 0,
                  })}
                </p>
              </div>
              <Award className="w-10 h-10 text-green-600" />
            </div>
          </div>

          {/* Eligible Teachers */}
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">
                  Eligible for Bonus
                </p>
                <p className="text-2xl font-bold text-gray-900 mt-2">
                  {payrollQuery.data.eligibleTeachers}/
                  {payrollQuery.data.teacherPayrolls.length}
                </p>
              </div>
              <Calendar className="w-10 h-10 text-purple-600" />
            </div>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <div className="flex gap-8">
          <button
            onClick={() => setActiveTab('payroll')}
            className={`py-4 px-2 font-medium transition-colors ${
              activeTab === 'payroll'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Payroll Details
          </button>
          <button
            onClick={() => setActiveTab('bonus')}
            className={`py-4 px-2 font-medium transition-colors ${
              activeTab === 'bonus'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Bonus Eligibility
          </button>
          <button
            onClick={() => setActiveTab('attendance')}
            className={`py-4 px-2 font-medium transition-colors ${
              activeTab === 'attendance'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Attendance Summary
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        {activeTab === 'payroll' && (
          <div>
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Payroll Details
            </h2>
            {payrollQuery.isLoading ? (
              <div className="space-y-4">
                {[...Array(3)].map((_, i) => (
                  <div
                    key={i}
                    className="h-32 bg-gray-100 rounded animate-pulse"
                  />
                ))}
              </div>
            ) : payrollQuery.isError ? (
              <div className="text-red-600 text-center py-8">
                Error loading payroll data
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {payrollQuery.data?.teacherPayrolls.map((payroll) => (
                  <TeacherPayrollCard key={payroll.teacherId} payroll={payroll} />
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'bonus' && (
          <div>
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Bonus Eligibility
            </h2>
            <BonusEligibilityList
              bonuses={bonusQuery.data || []}
              isLoading={bonusQuery.isLoading}
            />
            {bonusQuery.isError && (
              <div className="text-red-600 text-center py-8">
                Error loading bonus eligibility data
              </div>
            )}
          </div>
        )}

        {activeTab === 'attendance' && (
          <div>
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Attendance Summary
            </h2>
            {attendanceQuery.isLoading ? (
              <div className="space-y-3">
                {[...Array(5)].map((_, i) => (
                  <div
                    key={i}
                    className="h-16 bg-gray-100 rounded animate-pulse"
                  />
                ))}
              </div>
            ) : attendanceQuery.isError ? (
              <div className="text-red-600 text-center py-8">
                Error loading attendance data
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b-2 border-gray-200">
                      <th className="text-left py-3 px-4 font-semibold text-gray-900">
                        Teacher Name
                      </th>
                      <th className="text-right py-3 px-4 font-semibold text-gray-900">
                        Total Days
                      </th>
                      <th className="text-right py-3 px-4 font-semibold text-gray-900">
                        Present
                      </th>
                      <th className="text-right py-3 px-4 font-semibold text-gray-900">
                        Absent
                      </th>
                      <th className="text-right py-3 px-4 font-semibold text-gray-900">
                        Leave
                      </th>
                      <th className="text-right py-3 px-4 font-semibold text-gray-900">
                        Attendance %
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {attendanceQuery.data?.map((summary) => (
                      <tr key={summary.teacherId} className="hover:bg-gray-50">
                        <td className="py-4 px-4 font-medium text-gray-900">
                          {summary.teacherName}
                        </td>
                        <td className="py-4 px-4 text-right text-gray-900">
                          {summary.totalDays}
                        </td>
                        <td className="py-4 px-4 text-right text-green-600 font-medium">
                          {summary.presentDays}
                        </td>
                        <td className="py-4 px-4 text-right text-red-600 font-medium">
                          {summary.absentDays}
                        </td>
                        <td className="py-4 px-4 text-right text-blue-600 font-medium">
                          {summary.leaveDays}
                        </td>
                        <td className="py-4 px-4 text-right">
                          <span
                            className={`font-semibold ${
                              summary.attendancePercentage >= 90
                                ? 'text-green-600'
                                : summary.attendancePercentage >= 75
                                  ? 'text-blue-600'
                                  : 'text-red-600'
                            }`}
                          >
                            {summary.attendancePercentage}%
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

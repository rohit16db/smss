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
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 pb-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
          <div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
              Teacher Payroll
            </h1>
            <p className="text-gray-600 mt-2">
              Manage teacher salaries, bonuses, and attendance tracking
            </p>
          </div>
        </div>
        {/* Date Range Selector */}
        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 hover:shadow-xl transition-shadow duration-300">
          <div className="flex flex-col sm:flex-row gap-4 items-end">
            <div className="flex-1">
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                Start Date
              </label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
              />
            </div>
            <div className="flex-1">
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                End Date
              </label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
              />
            </div>
            <button
              onClick={handleRefresh}
              className="px-8 py-2.5 bg-gradient-to-r from-purple-600 to-blue-600 text-white rounded-xl hover:from-purple-700 hover:to-blue-700 transition-all font-semibold shadow-md hover:shadow-lg transform hover:-translate-y-0.5"
            >
              Refresh
            </button>
          </div>
        </div>

        {/* Summary Cards */}
        {payrollQuery.data && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {/* Total Payroll */}
            <div className="group bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 p-6 border border-gray-100 transform hover:-translate-y-1">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">
                    Total Payroll
                  </p>
                  <p className="text-3xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent mt-2">
                    ₹
                    {payrollQuery.data.totalPayrollAmount.toLocaleString(
                      'en-IN',
                      {
                        maximumFractionDigits: 0,
                      }
                    )}
                  </p>
                </div>
                <div className="p-4 bg-gradient-to-br from-purple-500 to-blue-500 rounded-xl shadow-lg group-hover:scale-110 transition-transform">
                  <DollarSign className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>

            {/* Total Bonus */}
            <div className="group bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 p-6 border border-gray-100 transform hover:-translate-y-1">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">
                    Total Bonus
                  </p>
                  <p className="text-3xl font-bold text-green-600 mt-2">
                    ₹
                    {payrollQuery.data.totalBonusAmount.toLocaleString('en-IN', {
                      maximumFractionDigits: 0,
                    })}
                  </p>
                </div>
                <div className="p-4 bg-gradient-to-br from-green-500 to-emerald-500 rounded-xl shadow-lg group-hover:scale-110 transition-transform">
                  <Award className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>

            {/* Eligible Teachers */}
            <div className="group bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 p-6 border border-gray-100 transform hover:-translate-y-1">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">
                    Eligible for Bonus
                  </p>
                  <p className="text-3xl font-bold text-gray-900 mt-2">
                    {payrollQuery.data.eligibleTeachers}/
                    {payrollQuery.data.teacherPayrolls.length}
                  </p>
                </div>
                <div className="p-4 bg-gradient-to-br from-indigo-500 to-purple-500 rounded-xl shadow-lg group-hover:scale-110 transition-transform">
                  <Calendar className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>
          </div>
        )}

      {/* Tabs */}
      <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden">
        <div className="flex gap-1 p-2 bg-gray-50">
          <button
            onClick={() => setActiveTab('payroll')}
            className={`flex-1 py-3 px-4 font-semibold rounded-xl transition-all ${
              activeTab === 'payroll'
                ? 'bg-gradient-to-r from-purple-600 to-blue-600 text-white shadow-md'
                : 'text-gray-600 hover:bg-white hover:text-gray-900'
            }`}
          >
            Payroll Details
          </button>
          <button
            onClick={() => setActiveTab('bonus')}
            className={`flex-1 py-3 px-4 font-semibold rounded-xl transition-all ${
              activeTab === 'bonus'
                ? 'bg-gradient-to-r from-purple-600 to-blue-600 text-white shadow-md'
                : 'text-gray-600 hover:bg-white hover:text-gray-900'
            }`}
          >
            Bonus Eligibility
          </button>
          <button
            onClick={() => setActiveTab('attendance')}
            className={`flex-1 py-3 px-4 font-semibold rounded-xl transition-all ${
              activeTab === 'attendance'
                ? 'bg-gradient-to-r from-purple-600 to-blue-600 text-white shadow-md'
                : 'text-gray-600 hover:bg-white hover:text-gray-900'
            }`}
          >
            Attendance Summary
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
        {activeTab === 'payroll' && (
          <div>
            <h2 className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent mb-6">
              Payroll Details
            </h2>
            {payrollQuery.isLoading ? (
              <div className="space-y-4">
                {[...Array(3)].map((_, i) => (
                  <div
                    key={i}
                    className="h-32 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse"
                  />
                ))}
              </div>
            ) : payrollQuery.isError ? (
              <div className="text-red-600 text-center py-12 font-semibold">
                Error loading payroll data
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {payrollQuery.data?.teacherPayrolls.map((payroll) => (
                  <TeacherPayrollCard key={payroll.teacherId} payroll={payroll} />
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'bonus' && (
          <div>
            <h2 className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent mb-6">
              Bonus Eligibility
            </h2>
            <BonusEligibilityList
              bonuses={bonusQuery.data || []}
              isLoading={bonusQuery.isLoading}
            />
            {bonusQuery.isError && (
              <div className="text-red-600 text-center py-12 font-semibold">
                Error loading bonus eligibility data
              </div>
            )}
          </div>
        )}

        {activeTab === 'attendance' && (
          <div>
            <h2 className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent mb-6">
              Attendance Summary
            </h2>
            {attendanceQuery.isLoading ? (
              <div className="space-y-3">
                {[...Array(5)].map((_, i) => (
                  <div
                    key={i}
                    className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse"
                  />
                ))}
              </div>
            ) : attendanceQuery.isError ? (
              <div className="text-red-600 text-center py-12 font-semibold">
                Error loading attendance data
              </div>
            ) : (
              <div className="overflow-x-auto rounded-xl border border-gray-200">
                <table className="w-full">
                  <thead>
                    <tr className="bg-gradient-to-r from-gray-50 to-gray-100 border-b-2 border-gray-200">
                      <th className="text-left py-4 px-5 font-bold text-gray-900">
                        Teacher Name
                      </th>
                      <th className="text-right py-4 px-5 font-bold text-gray-900">
                        Total Days
                      </th>
                      <th className="text-right py-4 px-5 font-bold text-gray-900">
                        Present
                      </th>
                      <th className="text-right py-4 px-5 font-bold text-gray-900">
                        Absent
                      </th>
                      <th className="text-right py-4 px-5 font-bold text-gray-900">
                        Leave
                      </th>
                      <th className="text-right py-4 px-5 font-bold text-gray-900">
                        Attendance %
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {attendanceQuery.data?.map((summary) => (
                      <tr key={summary.teacherId} className="hover:bg-gray-50 transition-colors">
                        <td className="py-4 px-5 font-semibold text-gray-900">
                          {summary.teacherName}
                        </td>
                        <td className="py-4 px-5 text-right text-gray-900 font-medium">
                          {summary.totalDays}
                        </td>
                        <td className="py-4 px-5 text-right text-green-600 font-bold">
                          {summary.presentDays}
                        </td>
                        <td className="py-4 px-5 text-right text-red-600 font-bold">
                          {summary.absentDays}
                        </td>
                        <td className="py-4 px-5 text-right text-blue-600 font-bold">
                          {summary.leaveDays}
                        </td>
                        <td className="py-4 px-5 text-right">
                          <span
                            className={`font-bold text-lg ${
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
      </div>
    </div>
  );
};

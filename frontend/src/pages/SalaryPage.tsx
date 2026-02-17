import React, { useState } from 'react';
import {
  useSalaryPaymentsByPeriod,
  usePendingSalaries,
  useSalarySummary,
} from '../services/salaryService';
import { SalaryPaymentCard } from '../components/salary/SalaryPaymentCard';
import { DollarSign, Clock, CheckCircle } from 'lucide-react';

export const SalaryPage: React.FC = () => {
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
  const [activeTab, setActiveTab] = useState<'overview' | 'pending' | 'report'>(
    'overview'
  );

  const salaryQuery = useSalaryPaymentsByPeriod(startDate, endDate);
  const pendingQuery = usePendingSalaries();
  const summaryQuery = useSalarySummary();

  const handleRefresh = () => {
    salaryQuery.refetch();
    pendingQuery.refetch();
    summaryQuery.refetch();
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50 to-purple-50 pb-8">
      {/* Header Section */}
      <div className="bg-gradient-to-r from-blue-600 to-purple-600 shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-white/20 rounded-xl backdrop-blur-sm">
              <DollarSign className="w-8 h-8 text-white" />
            </div>
            <div>
              <h1 className="text-3xl font-bold text-white">Salary Management</h1>
              <p className="text-blue-100 mt-1">
                Process and track teacher salary payments
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
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
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
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
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
              />
            </div>
            <button
              onClick={handleRefresh}
              className="px-8 py-2.5 bg-gradient-to-r from-blue-600 to-purple-600 text-white rounded-xl hover:from-blue-700 hover:to-purple-700 transition-all font-semibold shadow-md hover:shadow-lg transform hover:-translate-y-0.5"
            >
              Refresh
            </button>
          </div>
        </div>

        {/* Summary Cards */}
        {summaryQuery.data && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            {/* Total Salary */}
            <div className="group bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 p-6 border border-gray-100 transform hover:-translate-y-1">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">
                    Total Salary Expense
                  </p>
                  <p className="text-3xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mt-2">
                    ₹
                    {summaryQuery.data.totalSalaryExpense.toLocaleString(
                      'en-IN',
                      {
                        maximumFractionDigits: 0,
                      }
                    )}
                  </p>
                </div>
                <div className="p-4 bg-gradient-to-br from-blue-500 to-purple-500 rounded-xl shadow-lg group-hover:scale-110 transition-transform">
                  <DollarSign className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>

            {/* Paid */}
            <div className="group bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 p-6 border border-gray-100 transform hover:-translate-y-1">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">Paid</p>
                  <p className="text-3xl font-bold text-green-600 mt-2">
                    ₹
                    {summaryQuery.data.totalPaid.toLocaleString('en-IN', {
                      maximumFractionDigits: 0,
                    })}
                  </p>
                  <p className="text-xs text-gray-500 mt-1 font-medium">
                    {summaryQuery.data.paidCount}/{summaryQuery.data.teacherCount}{' '}
                    teachers
                  </p>
                </div>
                <div className="p-4 bg-gradient-to-br from-green-500 to-emerald-500 rounded-xl shadow-lg group-hover:scale-110 transition-transform">
                  <CheckCircle className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>

            {/* Pending */}
            <div className="group bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 p-6 border border-gray-100 transform hover:-translate-y-1">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">Pending</p>
                  <p className="text-3xl font-bold text-orange-600 mt-2">
                    ₹
                    {summaryQuery.data.totalPending.toLocaleString('en-IN', {
                      maximumFractionDigits: 0,
                    })}
                  </p>
                  <p className="text-xs text-gray-500 mt-1 font-medium">
                    {summaryQuery.data.pendingCount}/{summaryQuery.data.teacherCount}{' '}
                    teachers
                  </p>
                </div>
                <div className="p-4 bg-gradient-to-br from-orange-500 to-amber-500 rounded-xl shadow-lg group-hover:scale-110 transition-transform">
                  <Clock className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>

            {/* Average */}
            <div className="group bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 p-6 border border-gray-100 transform hover:-translate-y-1">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">
                    Avg per Teacher
                  </p>
                  <p className="text-3xl font-bold text-gray-900 mt-2">
                    ₹
                    {summaryQuery.data.averageSalaryPerTeacher.toLocaleString(
                      'en-IN',
                      {
                        maximumFractionDigits: 0,
                      }
                    )}
                  </p>
                </div>
                <div className="p-4 bg-gradient-to-br from-gray-500 to-slate-500 rounded-xl shadow-lg group-hover:scale-110 transition-transform">
                  <DollarSign className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>
          </div>
        )}

      {/* Tabs */}
      <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden">
        <div className="flex gap-1 p-2 bg-gray-50">
          <button
            onClick={() => setActiveTab('overview')}
            className={`flex-1 py-3 px-4 font-semibold rounded-xl transition-all ${
              activeTab === 'overview'
                ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white shadow-md'
                : 'text-gray-600 hover:bg-white hover:text-gray-900'
            }`}
          >
            All Payments
          </button>
          <button
            onClick={() => setActiveTab('pending')}
            className={`flex-1 py-3 px-4 font-semibold rounded-xl transition-all ${
              activeTab === 'pending'
                ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white shadow-md'
                : 'text-gray-600 hover:bg-white hover:text-gray-900'
            }`}
          >
            Pending ({pendingQuery.data?.length || 0})
          </button>
          <button
            onClick={() => setActiveTab('report')}
            className={`flex-1 py-3 px-4 font-semibold rounded-xl transition-all ${
              activeTab === 'report'
                ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white shadow-md'
                : 'text-gray-600 hover:bg-white hover:text-gray-900'
            }`}
          >
            Summary
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
        {activeTab === 'overview' && (
          <div>
            <h2 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-6">
              Salary Payments
            </h2>
            {salaryQuery.isLoading ? (
              <div className="space-y-4">
                {[...Array(3)].map((_, i) => (
                  <div
                    key={i}
                    className="h-48 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse"
                  />
                ))}
              </div>
            ) : salaryQuery.isError ? (
              <div className="text-red-600 text-center py-12 font-semibold">
                Error loading salary payments
              </div>
            ) : salaryQuery.data?.paymentDetails.length === 0 ? (
              <div className="text-center py-12 text-gray-500">
                No salary payments for the selected period
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {salaryQuery.data?.paymentDetails.map((salary) => (
                  <SalaryPaymentCard key={salary.id} salary={salary} />
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'pending' && (
          <div>
            <h2 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-6">
              Pending Salaries
            </h2>
            {pendingQuery.isLoading ? (
              <div className="space-y-4">
                {[...Array(3)].map((_, i) => (
                  <div
                    key={i}
                    className="h-48 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse"
                  />
                ))}
              </div>
            ) : pendingQuery.isError ? (
              <div className="text-red-600 text-center py-12 font-semibold">
                Error loading pending salaries
              </div>
            ) : pendingQuery.data?.length === 0 ? (
              <div className="text-center py-12 text-gray-500">
                No pending salary payments
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {pendingQuery.data?.map((salary) => (
                  <SalaryPaymentCard key={salary.id} salary={salary} />
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'report' && (
          <div>
            <h2 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-6">
              Salary Summary
            </h2>
            {salaryQuery.data && (
              <div className="space-y-6">
                {/* Period Info */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 p-6 bg-gradient-to-br from-blue-50 to-purple-50 rounded-xl border border-blue-100">
                  <div>
                    <p className="text-sm text-gray-600 font-semibold">Period</p>
                    <p className="text-lg font-bold text-gray-900 mt-1">
                      {new Date(
                        salaryQuery.data.monthStart
                      ).toLocaleDateString()}{' '}
                      to{' '}
                      {new Date(
                        salaryQuery.data.monthEnd
                      ).toLocaleDateString()}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600 font-semibold">Total Teachers</p>
                    <p className="text-lg font-bold text-gray-900 mt-1">
                      {salaryQuery.data.totalTeachers}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600 font-semibold">Paid</p>
                    <p className="text-lg font-bold text-green-600 mt-1">
                      {salaryQuery.data.paidTeachers}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600 font-semibold">Pending</p>
                    <p className="text-lg font-bold text-orange-600 mt-1">
                      {salaryQuery.data.pendingTeachers}
                    </p>
                  </div>
                </div>

                {/* Amounts */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                  <div className="p-6 bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl border-2 border-blue-200 shadow-md hover:shadow-lg transition-shadow">
                    <p className="text-sm text-blue-700 font-bold">
                      Total Base Salary
                    </p>
                    <p className="text-3xl font-bold text-blue-900 mt-2">
                      ₹
                      {salaryQuery.data.totalBaseSalary.toLocaleString('en-IN')}
                    </p>
                  </div>

                  <div className="p-6 bg-gradient-to-br from-red-50 to-red-100 rounded-xl border-2 border-red-200 shadow-md hover:shadow-lg transition-shadow">
                    <p className="text-sm text-red-700 font-bold">
                      Total Deductions
                    </p>
                    <p className="text-3xl font-bold text-red-900 mt-2">
                      ₹
                      {salaryQuery.data.totalDeductions.toLocaleString('en-IN')}
                    </p>
                  </div>

                  <div className="p-6 bg-gradient-to-br from-green-50 to-green-100 rounded-xl border-2 border-green-200 shadow-md hover:shadow-lg transition-shadow">
                    <p className="text-sm text-green-700 font-bold">
                      Total Bonus
                    </p>
                    <p className="text-3xl font-bold text-green-900 mt-2">
                      ₹
                      {salaryQuery.data.totalBonus.toLocaleString('en-IN')}
                    </p>
                  </div>

                  <div className="p-6 bg-gradient-to-br from-purple-50 to-purple-100 rounded-xl border-2 border-purple-200 shadow-md hover:shadow-lg transition-shadow">
                    <p className="text-sm text-purple-700 font-bold">
                      Total Net Salary
                    </p>
                    <p className="text-3xl font-bold text-purple-900 mt-2">
                      ₹
                      {salaryQuery.data.totalNetSalary.toLocaleString('en-IN')}
                    </p>
                  </div>
                </div>
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

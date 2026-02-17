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
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Salary Management</h1>
        <p className="text-gray-600 mt-2">
          Process and track teacher salary payments
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
      {summaryQuery.data && (
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Total Salary */}
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">
                  Total Salary Expense
                </p>
                <p className="text-2xl font-bold text-gray-900 mt-2">
                  ₹
                  {summaryQuery.data.totalSalaryExpense.toLocaleString(
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

          {/* Paid */}
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">Paid</p>
                <p className="text-2xl font-bold text-green-600 mt-2">
                  ₹
                  {summaryQuery.data.totalPaid.toLocaleString('en-IN', {
                    maximumFractionDigits: 0,
                  })}
                </p>
                <p className="text-xs text-gray-600 mt-1">
                  {summaryQuery.data.paidCount}/{summaryQuery.data.teacherCount}{' '}
                  teachers
                </p>
              </div>
              <CheckCircle className="w-10 h-10 text-green-600" />
            </div>
          </div>

          {/* Pending */}
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">Pending</p>
                <p className="text-2xl font-bold text-orange-600 mt-2">
                  ₹
                  {summaryQuery.data.totalPending.toLocaleString('en-IN', {
                    maximumFractionDigits: 0,
                  })}
                </p>
                <p className="text-xs text-gray-600 mt-1">
                  {summaryQuery.data.pendingCount}/{summaryQuery.data.teacherCount}{' '}
                  teachers
                </p>
              </div>
              <Clock className="w-10 h-10 text-orange-600" />
            </div>
          </div>

          {/* Average */}
          <div className="bg-white rounded-lg border border-gray-200 p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 text-sm font-medium">
                  Avg per Teacher
                </p>
                <p className="text-2xl font-bold text-gray-900 mt-2">
                  ₹
                  {summaryQuery.data.averageSalaryPerTeacher.toLocaleString(
                    'en-IN',
                    {
                      maximumFractionDigits: 0,
                    }
                  )}
                </p>
              </div>
              <DollarSign className="w-10 h-10 text-gray-600" />
            </div>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <div className="flex gap-8">
          <button
            onClick={() => setActiveTab('overview')}
            className={`py-4 px-2 font-medium transition-colors ${
              activeTab === 'overview'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            All Payments
          </button>
          <button
            onClick={() => setActiveTab('pending')}
            className={`py-4 px-2 font-medium transition-colors ${
              activeTab === 'pending'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Pending ({pendingQuery.data?.length || 0})
          </button>
          <button
            onClick={() => setActiveTab('report')}
            className={`py-4 px-2 font-medium transition-colors ${
              activeTab === 'report'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Summary
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="bg-white rounded-lg border border-gray-200 p-6">
        {activeTab === 'overview' && (
          <div>
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Salary Payments
            </h2>
            {salaryQuery.isLoading ? (
              <div className="space-y-4">
                {[...Array(3)].map((_, i) => (
                  <div
                    key={i}
                    className="h-48 bg-gray-100 rounded animate-pulse"
                  />
                ))}
              </div>
            ) : salaryQuery.isError ? (
              <div className="text-red-600 text-center py-8">
                Error loading salary payments
              </div>
            ) : salaryQuery.data?.paymentDetails.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                No salary payments for the selected period
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {salaryQuery.data?.paymentDetails.map((salary) => (
                  <SalaryPaymentCard key={salary.id} salary={salary} />
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'pending' && (
          <div>
            <h2 className="text-xl font-semibold text-gray-900 mb-4">
              Pending Salaries
            </h2>
            {pendingQuery.isLoading ? (
              <div className="space-y-4">
                {[...Array(3)].map((_, i) => (
                  <div
                    key={i}
                    className="h-48 bg-gray-100 rounded animate-pulse"
                  />
                ))}
              </div>
            ) : pendingQuery.isError ? (
              <div className="text-red-600 text-center py-8">
                Error loading pending salaries
              </div>
            ) : pendingQuery.data?.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                No pending salary payments
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {pendingQuery.data?.map((salary) => (
                  <SalaryPaymentCard key={salary.id} salary={salary} />
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'report' && (
          <div>
            <h2 className="text-xl font-semibold text-gray-900 mb-6">
              Salary Summary
            </h2>
            {salaryQuery.data && (
              <div className="space-y-6">
                {/* Period Info */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 p-4 bg-gray-50 rounded-lg">
                  <div>
                    <p className="text-sm text-gray-600">Period</p>
                    <p className="text-lg font-semibold text-gray-900 mt-1">
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
                    <p className="text-sm text-gray-600">Total Teachers</p>
                    <p className="text-lg font-semibold text-gray-900 mt-1">
                      {salaryQuery.data.totalTeachers}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Paid</p>
                    <p className="text-lg font-semibold text-green-600 mt-1">
                      {salaryQuery.data.paidTeachers}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-600">Pending</p>
                    <p className="text-lg font-semibold text-orange-600 mt-1">
                      {salaryQuery.data.pendingTeachers}
                    </p>
                  </div>
                </div>

                {/* Amounts */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                  <div className="p-4 bg-blue-50 rounded-lg border border-blue-200">
                    <p className="text-sm text-blue-600 font-medium">
                      Total Base Salary
                    </p>
                    <p className="text-2xl font-bold text-blue-900 mt-2">
                      ₹
                      {salaryQuery.data.totalBaseSalary.toLocaleString('en-IN')}
                    </p>
                  </div>

                  <div className="p-4 bg-red-50 rounded-lg border border-red-200">
                    <p className="text-sm text-red-600 font-medium">
                      Total Deductions
                    </p>
                    <p className="text-2xl font-bold text-red-900 mt-2">
                      ₹
                      {salaryQuery.data.totalDeductions.toLocaleString('en-IN')}
                    </p>
                  </div>

                  <div className="p-4 bg-green-50 rounded-lg border border-green-200">
                    <p className="text-sm text-green-600 font-medium">
                      Total Bonus
                    </p>
                    <p className="text-2xl font-bold text-green-900 mt-2">
                      ₹
                      {salaryQuery.data.totalBonus.toLocaleString('en-IN')}
                    </p>
                  </div>

                  <div className="p-4 bg-purple-50 rounded-lg border border-purple-200">
                    <p className="text-sm text-purple-600 font-medium">
                      Total Net Salary
                    </p>
                    <p className="text-2xl font-bold text-purple-900 mt-2">
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
  );
};

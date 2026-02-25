import React, { useState, useMemo } from 'react';
import {
  usePayrollReport,
  useBonusEligibility,
  useAttendanceSummary,
} from '../services/payrollService';
import { TeacherPayrollCard } from '../components/payroll/TeacherPayrollCard';
import { BonusEligibilityList } from '../components/payroll/BonusEligibilityList';
import { DollarSign, Award, Search, Download, ArrowUp, ArrowDown, TrendingUp } from 'lucide-react';

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
  const [searchQuery, setSearchQuery] = useState('');
  const [sortBy, setSortBy] = useState<'name' | 'attendance' | 'present' | 'absent'>('name');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');

  // For comparison - previous period
  const prevStartDate = useMemo(() => {
    const start = new Date(startDate);
    const end = new Date(endDate);
    const daysInPeriod = Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));
    const prevStart = new Date(start);
    prevStart.setDate(prevStart.getDate() - daysInPeriod);
    return prevStart.toISOString().split('T')[0];
  }, [startDate, endDate]);

  const prevEndDate = useMemo(() => {
    const start = new Date(startDate);
    const prevEnd = new Date(start);
    prevEnd.setDate(prevEnd.getDate() - 1);
    return prevEnd.toISOString().split('T')[0];
  }, [startDate]);

  const payrollQuery = usePayrollReport(startDate, endDate);
  const bonusQuery = useBonusEligibility(startDate, endDate);
  const attendanceQuery = useAttendanceSummary(startDate, endDate);
  const prevPayrollQuery = usePayrollReport(prevStartDate, prevEndDate);

  // Filter functions
  const filteredPayrolls = useMemo(() => {
    if (!payrollQuery.data) return [];
    return payrollQuery.data.teacherPayrolls.filter(p =>
      p.teacherName.toLowerCase().includes(searchQuery.toLowerCase())
    );
  }, [payrollQuery.data, searchQuery]);

  const filteredAndSortedAttendance = useMemo(() => {
    if (!attendanceQuery.data) return [];
    
    const filtered = attendanceQuery.data.filter(a =>
      a.teacherName.toLowerCase().includes(searchQuery.toLowerCase())
    );

    return [...filtered].sort((a, b) => {
      let aVal: number | string = '';
      let bVal: number | string = '';

      switch (sortBy) {
        case 'attendance':
          aVal = a.attendancePercentage;
          bVal = b.attendancePercentage;
          break;
        case 'present':
          aVal = a.presentDays;
          bVal = b.presentDays;
          break;
        case 'absent':
          aVal = a.absentDays;
          bVal = b.absentDays;
          break;
        default:
          aVal = a.teacherName;
          bVal = b.teacherName;
      }

      if (typeof aVal === 'string') {
        return sortOrder === 'asc' ? aVal.localeCompare(bVal as string) : (bVal as string).localeCompare(aVal);
      }
      return sortOrder === 'asc' ? ((aVal as number) - (bVal as number)) : ((bVal as number) - (aVal as number));
    });
  }, [attendanceQuery.data, searchQuery, sortBy, sortOrder]);

  // Calculate trend
  const calculateTrendPercentage = (current: number, previous: number) => {
    if (previous === 0) return 0;
    return ((current - previous) / previous) * 100;
  };

  const currentPayroll = payrollQuery.data?.totalPayrollAmount || 0;
  const prevPayroll = prevPayrollQuery.data?.totalPayrollAmount || 0;
  const payrollTrend = calculateTrendPercentage(currentPayroll, prevPayroll);

  // Handlers
  const handleRefresh = () => {
    payrollQuery.refetch();
    bonusQuery.refetch();
    attendanceQuery.refetch();
    prevPayrollQuery.refetch();
  };

  const handleExportExcel = () => {
    if (activeTab === 'attendance' && attendanceQuery.data) {
      const headers = ['Teacher Name', 'Total Days', 'Present', 'Absent', 'Leave', 'Attendance %'];
      const rows = filteredAndSortedAttendance.map(a => [
        a.teacherName,
        a.totalDays,
        a.presentDays,
        a.absentDays,
        a.leaveDays,
        `${a.attendancePercentage}%`
      ]);

      const csv = [headers, ...rows]
        .map(row => row.map(cell => `"${cell}"`).join(','))
        .join('\n');

      const blob = new Blob([csv], { type: 'text/csv' });
      const url = globalThis.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `attendance-summary-${startDate}-${endDate}.csv`;
      link.click();
    } else if (activeTab === 'payroll' && payrollQuery.data) {
      const headers = ['Teacher Name', 'Base Salary', 'Deductions', 'Bonus', 'Net Salary', 'Attendance %'];
      const rows = filteredPayrolls.map(p => [
        p.teacherName,
        p.baseSalary,
        p.deductionsForAbsence,
        p.isBonusEligible ? p.bonusAmount : 0,
        p.netSalary,
        `${p.attendancePercentage}%`
      ]);

      const csv = [headers, ...rows]
        .map(row => row.map(cell => `"${cell}"`).join(','))
        .join('\n');

      const blob = new Blob([csv], { type: 'text/csv' });
      const url = globalThis.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `payroll-${startDate}-${endDate}.csv`;
      link.click();
    }
  };

  const getAttendanceColor = (percentage: number): string => {
    if (percentage >= 90) return 'text-green-600';
    if (percentage >= 75) return 'text-blue-600';
    if (percentage >= 60) return 'text-orange-600';
    return 'text-red-600';
  };

  const getAttendanceBgColor = (percentage: number): string => {
    if (percentage >= 90) return 'bg-green-500';
    if (percentage >= 75) return 'bg-blue-500';
    if (percentage >= 60) return 'bg-orange-500';
    return 'bg-red-500';
  };

  // Renderers
  const renderPayrollContent = () => {
    if (payrollQuery.isLoading) {
      return (
        <div className="space-y-4">
          <div className="h-32 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-32 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-32 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
        </div>
      );
    }

    if (payrollQuery.isError) {
      return (
        <div className="text-red-600 text-center py-12 font-semibold">
          Error loading payroll data
        </div>
      );
    }

    if (filteredPayrolls.length === 0) {
      return (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">No payroll data found matching your search</p>
        </div>
      );
    }

    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredPayrolls.map((payroll) => (
          <TeacherPayrollCard key={payroll.teacherId} payroll={payroll} />
        ))}
      </div>
    );
  };

  const renderBonusContent = () => {
    if (bonusQuery.isLoading) {
      return (
        <div className="space-y-3">
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
        </div>
      );
    }

    if (bonusQuery.isError) {
      return (
        <div className="text-red-600 text-center py-12 font-semibold">
          Error loading bonus eligibility data
        </div>
      );
    }

    if (!bonusQuery.data || bonusQuery.data.length === 0) {
      return (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">No bonus data available</p>
        </div>
      );
    }

    const filtered = bonusQuery.data.filter(b =>
      b.teacherName.toLowerCase().includes(searchQuery.toLowerCase())
    );

    return (
      <div>
        <div className="mb-4 p-3 bg-blue-50 text-sm text-blue-700 rounded-lg">
          Showing {filtered.length} of {bonusQuery.data.length} teachers
        </div>
        <BonusEligibilityList bonuses={filtered} isLoading={false} />
      </div>
    );
  };

  const renderAttendanceContent = () => {
    if (attendanceQuery.isLoading) {
      return (
        <div className="space-y-3">
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
          <div className="h-16 bg-gradient-to-r from-gray-100 to-gray-200 rounded-xl animate-pulse" />
        </div>
      );
    }

    if (attendanceQuery.isError) {
      return (
        <div className="text-red-600 text-center py-12 font-semibold">
          Error loading attendance data
        </div>
      );
    }

    if (filteredAndSortedAttendance.length === 0) {
      return (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">No attendance data found matching your search</p>
        </div>
      );
    }

    return (
      <div className="overflow-x-auto rounded-xl border border-gray-200">
        <table className="w-full">
          <thead>
            <tr className="bg-gradient-to-r from-gray-50 to-gray-100 border-b-2 border-gray-200">
              <th className="text-left py-4 px-5 font-bold text-gray-900">
                Teacher Name
              </th>
              <th
                className="text-right py-4 px-5 font-bold text-gray-900 cursor-pointer hover:bg-gray-200 transition-colors"
                onClick={() => setSortBy('name')}
              >
                Total Days
              </th>
              <th
                className="text-right py-4 px-5 font-bold text-gray-900 cursor-pointer hover:bg-gray-200 transition-colors"
                onClick={() => setSortBy('present')}
              >
                Present
              </th>
              <th
                className="text-right py-4 px-5 font-bold text-gray-900 cursor-pointer hover:bg-gray-200 transition-colors"
                onClick={() => setSortBy('absent')}
              >
                Absent
              </th>
              <th className="text-right py-4 px-5 font-bold text-gray-900">
                Leave
              </th>
              <th
                className="text-right py-4 px-5 font-bold text-gray-900 cursor-pointer hover:bg-gray-200 transition-colors"
                onClick={() => setSortBy('attendance')}
              >
                Attendance %
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {filteredAndSortedAttendance.map((summary, idx) => (
              <tr key={summary.teacherId} className="hover:bg-blue-50 transition-colors duration-200">
                <td className="py-4 px-5 font-semibold text-gray-900">
                  <div className="flex items-center gap-2">
                    <span className="text-xs font-bold text-gray-400 min-w-6">{idx + 1}.</span>
                    {summary.teacherName}
                  </div>
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
                  <div className="flex items-center justify-end gap-2">
                    <div className="w-16 bg-gray-200 rounded-full h-2">
                      <div
                        className={`h-2 rounded-full transition-all duration-300 ${getAttendanceBgColor(summary.attendancePercentage)}`}
                        style={{ width: `${Math.min(summary.attendancePercentage, 100)}%` }}
                      />
                    </div>
                    <span
                      className={`font-bold text-lg w-12 text-right ${getAttendanceColor(summary.attendancePercentage)}`}
                    >
                      {summary.attendancePercentage}%
                    </span>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
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
              <label htmlFor="start-date" className="block text-sm font-semibold text-gray-700 mb-2">
                Start Date
              </label>
              <input
                id="start-date"
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
              />
            </div>
            <div className="flex-1">
              <label htmlFor="end-date" className="block text-sm font-semibold text-gray-700 mb-2">
                End Date
              </label>
              <input
                id="end-date"
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
                      { maximumFractionDigits: 0 }
                    )}
                  </p>
                  {payrollTrend !== 0 && (
                    <div className={`flex items-center gap-1 mt-2 text-sm font-semibold ${payrollTrend > 0 ? 'text-green-600' : 'text-red-600'}`}>
                      {payrollTrend > 0 ? <ArrowUp className="w-4 h-4" /> : <ArrowDown className="w-4 h-4" />}
                      {Math.abs(payrollTrend).toFixed(1)}% from last period
                    </div>
                  )}
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
                  <p className="text-xs text-gray-500 mt-2">
                    {payrollQuery.data.eligibleTeachers} teachers eligible
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
                  <p className="text-xs text-gray-500 mt-2">
                    {((payrollQuery.data.eligibleTeachers / payrollQuery.data.teacherPayrolls.length) * 100).toFixed(0)}% of teachers
                  </p>
                </div>
                <div className="p-4 bg-gradient-to-br from-indigo-500 to-purple-500 rounded-xl shadow-lg group-hover:scale-110 transition-transform">
                  <TrendingUp className="w-8 h-8 text-white" />
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Tabs */}
        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden">
          <div className="flex gap-1 p-2 bg-gray-50 border-b border-gray-100">
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

          {/* Search and Filter Bar */}
          <div className="px-6 py-4 border-b border-gray-100 flex flex-col sm:flex-row gap-4 items-start sm:items-center justify-between">
            <div className="flex-1 min-w-0">
              <div className="relative">
                <Search className="absolute left-3 top-3 w-5 h-5 text-gray-400" />
                <input
                  type="text"
                  placeholder="Search by teacher name..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border-2 border-gray-300 rounded-lg focus:border-purple-500 focus:outline-none transition-colors"
                />
              </div>
            </div>

            {activeTab === 'attendance' && (
              <div className="flex gap-2 min-w-max">
                <select
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value as any)}
                  className="px-3 py-2 border-2 border-gray-300 rounded-lg focus:border-purple-500 focus:outline-none text-sm"
                >
                  <option value="name">Sort by Name</option>
                  <option value="attendance">Sort by Attendance %</option>
                  <option value="present">Sort by Present Days</option>
                  <option value="absent">Sort by Absent Days</option>
                </select>
                <button
                  onClick={() => setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc')}
                  className={`px-3 py-2 border-2 rounded-lg transition-all font-medium text-sm ${
                    sortOrder === 'asc'
                      ? 'border-purple-500 bg-purple-50 text-purple-600'
                      : 'border-gray-300 bg-gray-50 text-gray-600'
                  }`}
                >
                  {sortOrder === 'asc' ? '↑' : '↓'}
                </button>
              </div>
            )}

            <button
              onClick={handleExportExcel}
              className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-green-500 to-emerald-500 text-white rounded-lg hover:shadow-md transition-all font-medium text-sm"
            >
              <Download className="w-4 h-4" />
              Export CSV
            </button>
          </div>

          {/* Content */}
          <div className="p-6">
            {activeTab === 'payroll' && (
              <div>
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
                    Payroll Details {filteredPayrolls.length > 0 && <span className="text-gray-600 text-base font-normal ml-2">({filteredPayrolls.length})</span>}
                  </h2>
                  {searchQuery && (
                    <button
                      onClick={() => setSearchQuery('')}
                      className="text-sm text-gray-500 hover:text-gray-700 transition-colors"
                    >
                      Clear Filter
                    </button>
                  )}
                </div>
                {renderPayrollContent()}
              </div>
            )}

            {activeTab === 'bonus' && (
              <div>
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
                    Bonus Eligibility
                  </h2>
                  {searchQuery && (
                    <button
                      onClick={() => setSearchQuery('')}
                      className="text-sm text-gray-500 hover:text-gray-700 transition-colors"
                    >
                      Clear Filter
                    </button>
                  )}
                </div>
                {renderBonusContent()}
              </div>
            )}

            {activeTab === 'attendance' && (
              <div>
                <div className="flex items-center justify-between mb-6">
                  <h2 className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
                    Attendance Summary {filteredAndSortedAttendance.length > 0 && <span className="text-gray-600 text-base font-normal ml-2">({filteredAndSortedAttendance.length})</span>}
                  </h2>
                  {searchQuery && (
                    <button
                      onClick={() => setSearchQuery('')}
                      className="text-sm text-gray-500 hover:text-gray-700 transition-colors"
                    >
                      Clear Filter
                    </button>
                  )}
                </div>
                {renderAttendanceContent()}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

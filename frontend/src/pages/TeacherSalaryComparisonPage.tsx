import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  BarChart3,
  Calendar,
  Download,
  Filter,
  RefreshCw,
  TrendingUp,
  BarChart,
} from 'lucide-react';
import { reportApi } from '../services/api';

interface FilterState {
  startDate: string;
  endDate: string;
  status: string;
  sortBy: string;
  descending: boolean;
}

export const TeacherSalaryComparisonPage: React.FC = () => {
  const today = new Date();
  const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1).toISOString().split('T')[0];
  const endOfMonth = today.toISOString().split('T')[0];

  const [filters, setFilters] = useState<FilterState>({
    startDate: startOfMonth,
    endDate: endOfMonth,
    status: '',
    sortBy: 'name',
    descending: false,
  });

  const { data: salaryComparison = [], isLoading, isError, error, refetch } = useQuery({
    queryKey: ['teacher-salary-comparison', filters],
    queryFn: () =>
      reportApi.getTeacherSalaryComparison({
        startDate: filters.startDate,
        endDate: filters.endDate,
        status: filters.status || undefined,
        sortBy: filters.sortBy,
        descending: filters.descending,
      }),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const statistics = React.useMemo(() => {
    if (salaryComparison.length === 0) {
      return {
        totalSalaries: 0,
        averageNetSalary: 0,
        totalBonus: 0,
        totalDeduction: 0,
        pendingCount: 0,
        approvedCount: 0,
        paidCount: 0,
        highestSalary: 0,
        lowestSalary: 0,
      };
    }

    const stats = {
      totalSalaries: 0,
      averageNetSalary: 0,
      totalBonus: 0,
      totalDeduction: 0,
      pendingCount: 0,
      approvedCount: 0,
      paidCount: 0,
      highestSalary: salaryComparison[0].netSalary,
      lowestSalary: salaryComparison[0].netSalary,
    };

    salaryComparison.forEach((salary) => {
      stats.totalSalaries += salary.netSalary;
      stats.totalBonus += salary.bonus;
      stats.totalDeduction += salary.deductions;

      if (salary.status === 'Pending') stats.pendingCount++;
      if (salary.status === 'Approved') stats.approvedCount++;
      if (salary.status === 'Paid') stats.paidCount++;

      if (salary.netSalary > stats.highestSalary) stats.highestSalary = salary.netSalary;
      if (salary.netSalary < stats.lowestSalary) stats.lowestSalary = salary.netSalary;
    });

    stats.averageNetSalary = stats.totalSalaries / salaryComparison.length;

    return stats;
  }, [salaryComparison]);

  const handleFilterChange = (key: keyof FilterState, value: string | boolean) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handleExport = () => {
    const headers = [
      'Teacher Name',
      'Base Salary',
      'Bonus',
      'Deduction',
      'Net Salary',
      'Status',
    ];
    const rows = salaryComparison.map((salary) => [
      salary.teacherName,
      salary.baseSalary.toFixed(2),
      salary.bonus.toFixed(2),
      salary.deductions.toFixed(2),
      salary.netSalary.toFixed(2),
      salary.status,
    ]);

    const csv = [headers, ...rows].map((row) => row.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `teacher-salary-comparison-${filters.startDate}-to-${filters.endDate}.csv`;
    a.click();
  };

  const getStatusBadgeClass = (status: string) => {
    switch (status) {
      case 'Paid':
        return 'status-paid';
      case 'Approved':
        return 'status-approved';
      case 'Pending':
        return 'status-pending';
      default:
        return 'status-default';
    }
  };

  if (isError) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 p-4 sm:p-6 lg:p-8">
        <div className="max-w-4xl mx-auto bg-red-50 border-2 border-red-200 rounded-2xl p-8">
          <h2 className="text-2xl font-bold text-red-700 mb-2">Error Loading Data</h2>
          <p className="text-red-600 mb-6">{error instanceof Error ? error.message : 'Failed to load teacher salary comparison data'}</p>
          <button
            onClick={() => refetch()}
            className="px-6 py-2.5 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-lg hover:from-red-700 hover:to-red-800 transition-all font-semibold shadow-md hover:shadow-lg"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 pb-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
        
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
          <div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
              Teacher Salary Comparison
            </h1>
            <p className="text-gray-600 mt-2">Analyze and compare teacher salaries across periods</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => refetch()}
              disabled={isLoading}
              className="p-3 bg-white rounded-xl border-2 border-gray-200 hover:border-blue-400 hover:bg-blue-50 transition-all disabled:opacity-50 shadow-md hover:shadow-lg"
              title="Refresh data"
            >
              <RefreshCw size={20} className="text-gray-600" />
            </button>
            <button
              onClick={handleExport}
              disabled={salaryComparison.length === 0}
              className="p-3 bg-white rounded-xl border-2 border-gray-200 hover:border-green-400 hover:bg-green-50 transition-all disabled:opacity-50 shadow-md hover:shadow-lg"
              title="Export to CSV"
            >
              <Download size={20} className="text-gray-600" />
            </button>
          </div>
        </div>

        {/* Summary Statistics */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="bg-white rounded-2xl shadow-lg border-l-4 border-blue-500 p-6 hover:shadow-xl transition-shadow">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-blue-100 rounded-lg">
                <BarChart3 size={24} className="text-blue-600" />
              </div>
              <div className="flex-1">
                <p className="text-gray-600 text-sm font-medium">Average Net Salary</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">₹{statistics.averageNetSalary.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-2xl shadow-lg border-l-4 border-green-500 p-6 hover:shadow-xl transition-shadow">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-green-100 rounded-lg">
                <TrendingUp size={24} className="text-green-600" />
              </div>
              <div className="flex-1">
                <p className="text-gray-600 text-sm font-medium">Paid Status</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">{statistics.paidCount}</p>
                <p className="text-xs text-gray-500 mt-1">Salaries paid</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-2xl shadow-lg border-l-4 border-amber-500 p-6 hover:shadow-xl transition-shadow">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-amber-100 rounded-lg">
                <Calendar size={24} className="text-amber-600" />
              </div>
              <div className="flex-1">
                <p className="text-gray-600 text-sm font-medium">Pending Approval</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">{statistics.pendingCount}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-2xl shadow-lg border-l-4 border-purple-500 p-6 hover:shadow-xl transition-shadow">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-purple-100 rounded-lg">
                <BarChart size={24} className="text-purple-600" />
              </div>
              <div className="flex-1">
                <p className="text-gray-600 text-sm font-medium">Total Bonus Paid</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">₹{statistics.totalBonus.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
              </div>
            </div>
          </div>
        </div>

        {/* Filters Panel */}
        <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-100">
          <div className="flex items-center gap-3 mb-6">
            <Filter size={22} className="text-blue-600" />
            <h3 className="text-xl font-bold text-gray-900">Filters</h3>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Start Date</label>
              <input
                type="date"
                value={filters.startDate}
                onChange={(e) => handleFilterChange('startDate', e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
              />
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">End Date</label>
              <input
                type="date"
                value={filters.endDate}
                onChange={(e) => handleFilterChange('endDate', e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
              />
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Status</label>
              <select
                value={filters.status}
                onChange={(e) => handleFilterChange('status', e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
              >
                <option value="">All Statuses</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Paid">Paid</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Sort By</label>
              <select
                value={filters.sortBy}
                onChange={(e) => handleFilterChange('sortBy', e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
              >
                <option value="name">Name</option>
                <option value="netsalary">Net Salary</option>
                <option value="bonus">Bonus</option>
                <option value="deduction">Deduction</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">Sort Order</label>
              <select
                value={filters.descending ? 'desc' : 'asc'}
                onChange={(e) => handleFilterChange('descending', e.target.value === 'desc')}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
              >
                <option value="asc">Ascending</option>
                <option value="desc">Descending</option>
              </select>
            </div>
          </div>

          <button
            onClick={() =>
              setFilters({
                startDate: startOfMonth,
                endDate: endOfMonth,
                status: '',
                sortBy: 'name',
                descending: false,
              })
            }
            className="mt-6 px-6 py-2.5 bg-gray-100 text-gray-700 rounded-xl hover:bg-gray-200 transition-all font-semibold"
          >
            Reset Filters
          </button>
        </div>

        {/* Data Table */}
        <div className="bg-white rounded-2xl shadow-lg overflow-hidden border border-gray-100">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-16 px-6">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4"></div>
              <p className="text-gray-600 font-medium">Loading salary comparison data...</p>
            </div>
          ) : salaryComparison.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 px-6">
              <div className="p-4 bg-blue-100 rounded-full mb-4">
                <BarChart size={48} className="text-blue-600" />
              </div>
              <h3 className="text-xl font-bold text-gray-900 mb-2">No salary records found</h3>
              <p className="text-gray-600">Try adjusting your filters</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gradient-to-r from-gray-900 to-gray-800">
                  <tr>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-white">Teacher Name</th>
                    <th className="px-6 py-3 text-right text-sm font-semibold text-white">Base Salary</th>
                    <th className="px-6 py-3 text-right text-sm font-semibold text-white">Bonus</th>
                    <th className="px-6 py-3 text-right text-sm font-semibold text-white">Deduction</th>
                    <th className="px-6 py-3 text-right text-sm font-semibold text-white bg-blue-600">Net Salary</th>
                    <th className="px-6 py-3 text-center text-sm font-semibold text-white">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {salaryComparison.map((salary, index) => (
                    <tr key={salary.teacherId} className={index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                      <td className="px-6 py-4 text-sm font-medium text-gray-900">{salary.teacherName}</td>
                      <td className="px-6 py-4 text-sm text-right text-gray-600">₹{salary.baseSalary.toFixed(2)}</td>
                      <td className="px-6 py-4 text-sm text-right text-green-600 font-medium">+₹{salary.bonus.toFixed(2)}</td>
                      <td className="px-6 py-4 text-sm text-right text-red-600 font-medium">-₹{salary.deductions.toFixed(2)}</td>
                      <td className="px-6 py-4 text-sm text-right text-blue-600 font-bold bg-blue-50">₹{salary.netSalary.toFixed(2)}</td>
                      <td className="px-6 py-4 text-center">
                        <span
                          className={`inline-flex px-3 py-1 rounded-full text-xs font-semibold ${
                            salary.status === 'Paid'
                              ? 'bg-green-100 text-green-800'
                              : salary.status === 'Approved'
                              ? 'bg-blue-100 text-blue-800'
                              : 'bg-amber-100 text-amber-800'
                          }`}
                        >
                          {salary.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-100 border-t-2 border-gray-200">
                  <tr>
                    <td className="px-6 py-4 text-sm font-bold text-gray-900">TOTAL / AVERAGE</td>
                    <td className="px-6 py-4 text-right text-sm text-gray-600">-</td>
                    <td className="px-6 py-4 text-right text-sm text-green-600 font-bold">₹{statistics.totalBonus.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</td>
                    <td className="px-6 py-4 text-right text-sm text-red-600 font-bold">₹{statistics.totalDeduction.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</td>
                    <td className="px-6 py-4 text-right text-sm text-blue-600 font-bold">₹{statistics.totalSalaries.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</td>
                    <td>-</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          )}
        </div>

        {/* Summary Footer */}
        {salaryComparison.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-100">
              <p className="text-gray-600 text-sm mb-2">Showing Records</p>
              <p className="text-3xl font-bold text-gray-900">{salaryComparison.length}</p>
            </div>
            <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-100">
              <p className="text-gray-600 text-sm mb-2">Highest Salary</p>
              <p className="text-3xl font-bold text-gray-900">₹{statistics.highestSalary.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
            </div>
            <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-100">
              <p className="text-gray-600 text-sm mb-2">Lowest Salary</p>
              <p className="text-3xl font-bold text-gray-900">₹{statistics.lowestSalary.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

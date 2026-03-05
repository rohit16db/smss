import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Calendar,
  Download,
  Filter,
  RefreshCw,
  TrendingDown,
  TrendingUp,
  AlertTriangle,
} from 'lucide-react';
import { reportApi } from '../services/api';

interface FilterState {
  reportType: string;
  startDate: string;
  endDate: string;
  groupBy: string;
}

export const BudgetVsActualPage: React.FC = () => {
  const today = new Date();
  const startOfYear = new Date(today.getFullYear(), 0, 1).toISOString().split('T')[0];
  const endOfYear = today.toISOString().split('T')[0];

  const [filters, setFilters] = useState<FilterState>({
    reportType: 'SalaryExpense',
    startDate: startOfYear,
    endDate: endOfYear,
    groupBy: 'month',
  });

  const { data: budgetVsActual = [], isLoading, isError, error, refetch } = useQuery({
    queryKey: ['budget-vs-actual', filters],
    queryFn: () =>
      reportApi.getBudgetVsActual({
        reportType: filters.reportType,
        startDate: filters.startDate,
        endDate: filters.endDate,
        groupBy: filters.groupBy,
      }),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const statistics = React.useMemo(() => {
    if (budgetVsActual.length === 0) {
      return {
        totalBudgeted: 0,
        totalActual: 0,
        totalVariance: 0,
        totalVariancePercentage: 0,
        overBudgetCount: 0,
        underBudgetCount: 0,
        avgVariancePercentage: 0,
      };
    }

    const stats = {
      totalBudgeted: 0,
      totalActual: 0,
      totalVariance: 0,
      totalVariancePercentage: 0,
      overBudgetCount: 0,
      underBudgetCount: 0,
      avgVariancePercentage: 0,
    };

    budgetVsActual.forEach((item) => {
      stats.totalBudgeted += item.budgetedAmount;
      stats.totalActual += item.actualAmount;
      stats.totalVariance += item.variance;

      if (item.variance > 0) stats.overBudgetCount++;
      else stats.underBudgetCount++;
    });

    if (stats.totalBudgeted > 0) {
      stats.totalVariancePercentage = (stats.totalVariance / stats.totalBudgeted) * 100;
    }
    stats.avgVariancePercentage =
      budgetVsActual.reduce((sum, item) => sum + item.variancePercentage, 0) / budgetVsActual.length;

    return stats;
  }, [budgetVsActual]);

  const handleFilterChange = (key: keyof FilterState, value: string) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handleExport = () => {
    const headers = ['Period', 'Budgeted', 'Actual', 'Variance', 'Variance %'];
    const rows = budgetVsActual.map((item) => [
      item.month,
      item.budgetedAmount.toFixed(2),
      item.actualAmount.toFixed(2),
      item.variance.toFixed(2),
      item.variancePercentage.toFixed(2),
    ]);

    const csv = [headers, ...rows].map((row) => row.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `budget-vs-actual-${filters.startDate}-to-${filters.endDate}.csv`;
    a.click();
  };

  const getVarianceColor = (variance: number) => {
    if (variance === 0) return 'neutral';
    return variance > 0 ? 'over-budget' : 'under-budget';
  };

  const getVarianceIcon = (variance: number) => {
    if (variance === 0) return <Calendar size={18} />;
    return variance > 0 ? <TrendingUp size={18} /> : <TrendingDown size={18} />;
  };

  const reportTypeLabel = filters.reportType === 'FeeCollection' ? 'Fee Collection' : 'Salary Expense';

  // Error display
  if (isError) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 p-4 sm:p-6 lg:p-8">
        <div className="max-w-4xl mx-auto bg-red-50 border-2 border-red-200 rounded-2xl p-8">
          <h2 className="text-2xl font-bold text-red-700 mb-2">Error Loading Data</h2>
          <p className="text-red-600 mb-6">{error instanceof Error ? error.message : 'Failed to load budget vs actual data'}</p>
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
              Budget vs Actual Report
            </h1>
            <p className="text-gray-600 mt-2">Monitor {reportTypeLabel.toLowerCase()} variance analysis</p>
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
              disabled={budgetVsActual.length === 0}
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
                <Calendar size={24} className="text-blue-600" />
              </div>
              <div className="flex-1">
                <p className="text-gray-600 text-sm font-medium">Total Budgeted</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">₹{statistics.totalBudgeted.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-2xl shadow-lg border-l-4 border-purple-500 p-6 hover:shadow-xl transition-shadow">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-purple-100 rounded-lg">
                <Calendar size={24} className="text-purple-600" />
              </div>
              <div className="flex-1">
                <p className="text-gray-600 text-sm font-medium">Total Actual</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">₹{statistics.totalActual.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
              </div>
            </div>
          </div>

          <div className={`bg-white rounded-2xl shadow-lg border-l-4 p-6 hover:shadow-xl transition-shadow ${statistics.totalVariance > 0 ? 'border-red-500' : 'border-green-500'}`}>
            <div className="flex items-center gap-4">
              <div className={`p-3 rounded-lg ${statistics.totalVariance > 0 ? 'bg-red-100' : 'bg-green-100'}`}>
                <AlertTriangle size={24} className={statistics.totalVariance > 0 ? 'text-red-600' : 'text-green-600'} />
              </div>
              <div className="flex-1">
                <p className="text-gray-600 text-sm font-medium">Total Variance</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">₹{Math.abs(statistics.totalVariance).toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
                <p className={`text-xs mt-1 ${statistics.totalVariance > 0 ? 'text-red-600' : 'text-green-600'}`}>
                  {statistics.totalVariance > 0 ? '↑ Over Budget' : '↓ Under Budget'} ({Math.abs(statistics.totalVariancePercentage).toFixed(2)}%)
                </p>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-2xl shadow-lg border-l-4 border-amber-500 p-6 hover:shadow-xl transition-shadow">
            <div className="flex items-center gap-4">
              <div className="p-3 bg-amber-100 rounded-lg">
                <TrendingUp size={24} className="text-amber-600" />
              </div>
              <div className="flex-1">
                <p className="text-gray-600 text-sm font-medium">Over Budget</p>
                <p className="text-2xl font-bold text-gray-900 mt-1">{statistics.overBudgetCount}</p>
                <p className="text-xs text-gray-500 mt-1">periods</p>
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
              <label className="block text-sm font-semibold text-gray-700 mb-2">Report Type</label>
              <select
                value={filters.reportType}
                onChange={(e) => handleFilterChange('reportType', e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
              >
                <option value="FeeCollection">Fee Collection</option>
                <option value="SalaryExpense">Salary Expense</option>
              </select>
            </div>

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
              <label className="block text-sm font-semibold text-gray-700 mb-2">Group By</label>
              <select
                value={filters.groupBy}
                onChange={(e) => handleFilterChange('groupBy', e.target.value)}
                className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
              >
                <option value="month">Month</option>
                <option value="category">Category</option>
                <option value="class">Class</option>
              </select>
            </div>

            <button
              onClick={() =>
                setFilters({
                  reportType: 'SalaryExpense',
                  startDate: startOfYear,
                  endDate: endOfYear,
                  groupBy: 'month',
                })
              }
              className="mt-7 px-4 py-2.5 bg-gray-100 text-gray-700 rounded-xl hover:bg-gray-200 transition-all font-semibold"
            >
              Reset Filters
            </button>
          </div>
        </div>

        {/* Data Table */}
        <div className="bg-white rounded-2xl shadow-lg overflow-hidden border border-gray-100">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-16 px-6">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4"></div>
              <p className="text-gray-600 font-medium">Loading budget vs actual data...</p>
            </div>
          ) : budgetVsActual.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 px-6">
              <div className="p-4 bg-blue-100 rounded-full mb-4">
                <AlertTriangle size={48} className="text-blue-600" />
              </div>
              <h3 className="text-xl font-bold text-gray-900 mb-2">No data found</h3>
              <p className="text-gray-600">Try adjusting your filters</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gradient-to-r from-gray-900 to-gray-800">
                  <tr>
                    <th className="px-6 py-3 text-left text-sm font-semibold text-white">Period</th>
                    <th className="px-6 py-3 text-right text-sm font-semibold text-white">Budgeted Amount</th>
                    <th className="px-6 py-3 text-right text-sm font-semibold text-white">Actual Amount</th>
                    <th className="px-6 py-3 text-right text-sm font-semibold text-white">Variance</th>
                    <th className="px-6 py-3 text-center text-sm font-semibold text-white">Variance %</th>
                    <th className="px-6 py-3 text-center text-sm font-semibold text-white">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {budgetVsActual.map((item, index) => (
                    <tr key={`${item.month}-${index}`} className={index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}>
                      <td className="px-6 py-4 text-sm font-medium text-gray-900">{item.month}</td>
                      <td className="px-6 py-4 text-sm text-right text-gray-600">₹{item.budgetedAmount.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</td>
                      <td className="px-6 py-4 text-sm text-right text-gray-600">₹{item.actualAmount.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</td>
                      <td className={`px-6 py-4 text-sm text-right font-semibold ${item.variance > 0 ? 'text-red-600' : 'text-green-600'}`}>
                        {item.variance > 0 ? '+' : '-'}₹{Math.abs(item.variance).toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                      </td>
                      <td className="px-6 py-4 text-center">
                        <span className={`inline-flex px-3 py-1 rounded-full text-xs font-semibold ${
                          item.variance > 0
                            ? 'bg-red-100 text-red-800'
                            : item.variance < 0
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
                        }`}>
                          {item.variancePercentage > 0 ? '+' : ''}
                          {item.variancePercentage.toFixed(2)}%
                        </span>
                      </td>
                      <td className="px-6 py-4 text-center">
                        <div className={`flex items-center justify-center gap-2 px-3 py-1 rounded-lg text-xs font-semibold ${
                          item.variance > 0
                            ? 'bg-red-50 text-red-700'
                            : item.variance < 0
                            ? 'bg-green-50 text-green-700'
                            : 'bg-gray-50 text-gray-700'
                        }`}>
                          {item.variance > 0 ? <TrendingUp size={16} /> : item.variance < 0 ? <TrendingDown size={16} /> : <Calendar size={16} />}
                          <span>
                            {item.variance > 0
                              ? 'Over Budget'
                              : item.variance < 0
                              ? 'Under Budget'
                              : 'On Track'}
                          </span>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-100 border-t-2 border-gray-200">
                  <tr>
                    <td className="px-6 py-4 text-sm font-bold text-gray-900">TOTAL</td>
                    <td className="px-6 py-4 text-right text-sm text-gray-600">₹{statistics.totalBudgeted.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</td>
                    <td className="px-6 py-4 text-right text-sm text-gray-600">₹{statistics.totalActual.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</td>
                    <td className={`px-6 py-4 text-right text-sm font-bold ${statistics.totalVariance > 0 ? 'text-red-600' : 'text-green-600'}`}>
                      {statistics.totalVariance > 0 ? '+' : '-'}₹{Math.abs(statistics.totalVariance).toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                    </td>
                    <td className="px-6 py-4 text-center text-sm font-bold text-gray-900">{statistics.totalVariancePercentage.toFixed(2)}%</td>
                    <td>-</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          )}
        </div>

        {/* Legend */}
        <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-100">
          <h3 className="text-lg font-bold text-gray-900 mb-4">Legend</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="flex items-center gap-3 p-4 bg-red-50 rounded-xl border border-red-200">
              <span className="w-4 h-4 bg-red-600 rounded-full"></span>
              <span className="text-sm text-gray-700">Over Budget (Positive variance)</span>
            </div>
            <div className="flex items-center gap-3 p-4 bg-green-50 rounded-xl border border-green-200">
              <span className="w-4 h-4 bg-green-600 rounded-full"></span>
              <span className="text-sm text-gray-700">Under Budget (Negative variance)</span>
            </div>
            <div className="flex items-center gap-3 p-4 bg-gray-50 rounded-xl border border-gray-200">
              <span className="w-4 h-4 bg-gray-600 rounded-full"></span>
              <span className="text-sm text-gray-700">On Track (No variance)</span>
            </div>
          </div>
        </div>

        {/* Summary Footer */}
        {budgetVsActual.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-100">
              <p className="text-gray-600 text-sm mb-2">Showing Periods</p>
              <p className="text-3xl font-bold text-gray-900">{budgetVsActual.length}</p>
            </div>
            <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-100">
              <p className="text-gray-600 text-sm mb-2">Average Variance</p>
              <p className="text-3xl font-bold text-gray-900">{statistics.avgVariancePercentage.toFixed(2)}%</p>
            </div>
            <div className={`bg-white rounded-2xl shadow-lg p-6 border border-gray-100 ${statistics.totalVariance > 0 ? 'border-l-4 border-l-red-500' : 'border-l-4 border-l-green-500'}`}>
              <p className="text-gray-600 text-sm mb-2">Status</p>
              <p className={`text-2xl font-bold ${statistics.totalVariance > 0 ? 'text-red-600' : 'text-green-600'}`}>
                {statistics.totalVariance > 0 ? 'Over Budget' : 'Under Budget'}
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

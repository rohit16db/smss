import React, { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  AlertCircle,
  Filter,
  Download,
  RefreshCw,
} from 'lucide-react';
import { reportApi } from '../services/api';
import { formatDate } from '../utils/dateFormat';

interface FilterState {
  asOfDate: string;
  agingBucket: string;
  minAmount: string;
  sortBy: string;
  descending: boolean;
}

export const OutstandingFeesPage: React.FC = () => {
  const today = new Date().toISOString().split('T')[0];
  const [filters, setFilters] = useState<FilterState>({
    asOfDate: today,
    agingBucket: '',
    minAmount: '',
    sortBy: 'daysoverdue',
    descending: true,
  });

  const { data: outstandingFees = [], isLoading, refetch } = useQuery({
    queryKey: ['outstanding-fees', filters],
    queryFn: () => reportApi.getOutstandingFees({
      asOfDate: filters.asOfDate,
      agingBucket: filters.agingBucket || undefined,
      minAmount: filters.minAmount ? parseFloat(filters.minAmount) : undefined,
      sortBy: filters.sortBy,
      descending: filters.descending,
    }),
    staleTime: 5 * 60 * 1000,
  });

  // Calculate summary statistics
  const statistics = useMemo(() => {
    const stats = {
      totalDue: 0,
      totalRecords: outstandingFees.length,
      overdue0_30: 0,
      overdue31_60: 0,
      overdue61_90: 0,
      overdue90Plus: 0,
      activeStudents: 0,
    };

    outstandingFees.forEach((fee) => {
      stats.totalDue += fee.dueAmount;
      if (fee.isActive) stats.activeStudents++;
      switch (fee.agingBucket) {
        case '0-30':
          stats.overdue0_30++;
          break;
        case '31-60':
          stats.overdue31_60++;
          break;
        case '61-90':
          stats.overdue61_90++;
          break;
        case '90+':
          stats.overdue90Plus++;
          break;
      }
    });

    return stats;
  }, [outstandingFees]);

  const handleFilterChange = (key: keyof FilterState, value: string | boolean) => {
    setFilters((prev) => ({ ...prev, [key]: value }));
  };

  const handleExport = () => {
    const headers = ['Student Info', 'Class/Section', 'Due Amount', 'Days Overdue', 'Aging Bucket', 'Due Date', 'Last Payment', 'Remarks', 'Contact', 'Status'];
    const rows = outstandingFees.map((fee) => [
      fee.studentInfo,
      fee.classSection,
      fee.dueAmount.toFixed(2),
      fee.daysOverdue,
      fee.agingBucket,
      formatDate(fee.dueDate),
      fee.lastPaymentDate ? formatDate(fee.lastPaymentDate) : 'N/A',
      fee.remarks || 'N/A',
      fee.contactInfo || 'N/A',
      fee.isActive ? 'Active' : 'Inactive',
    ]);

    const csv = [headers, ...rows].map((row) => row.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `outstanding-fees-${filters.asOfDate}.csv`;
    a.click();
  };

  const getAgingColor = (bucket: string) => {
    switch (bucket) {
      case '0-30':
        return 'bg-blue-50 border-l-4 border-blue-500';
      case '31-60':
        return 'bg-yellow-50 border-l-4 border-yellow-500';
      case '61-90':
        return 'bg-orange-50 border-l-4 border-orange-500';
      case '90+':
        return 'bg-red-50 border-l-4 border-red-500';
      default:
        return 'bg-gray-50 border-l-4 border-gray-500';
    }
  };

  const getAgingBadgeColor = (bucket: string) => {
    switch (bucket) {
      case '0-30':
        return 'bg-blue-100 text-blue-800';
      case '31-60':
        return 'bg-yellow-100 text-yellow-800';
      case '61-90':
        return 'bg-orange-100 text-orange-800';
      case '90+':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <div className="mb-8">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-rose-600 to-rose-800 bg-clip-text text-transparent">
                ⚠️ Outstanding Fees Report
              </h1>
              <p className="text-gray-600 mt-2">Track overdue student fees and collection priorities</p>
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => refetch()}
                disabled={isLoading}
                className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
                title="Refresh data"
              >
                <RefreshCw size={18} />
                <span className="hidden sm:inline">Refresh</span>
              </button>
              <button
                onClick={handleExport}
                disabled={outstandingFees.length === 0}
                className="inline-flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50"
                title="Export to CSV"
              >
                <Download size={18} />
                <span className="hidden sm:inline">Export</span>
              </button>
            </div>
          </div>
        </div>

        {/* Summary Statistics */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-rose-500">
            <p className="text-gray-600 text-sm font-medium">Total Outstanding</p>
            <p className="text-3xl font-bold text-gray-900 mt-2">
              ₹{statistics.totalDue.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
            </p>
            <p className="text-gray-500 text-xs mt-2">{statistics.totalRecords} students</p>
          </div>

          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-red-500">
            <p className="text-gray-600 text-sm font-medium">90+ Days Overdue</p>
            <p className="text-3xl font-bold text-red-600 mt-2">{statistics.overdue90Plus}</p>
            <p className="text-gray-500 text-xs mt-2">Requires immediate action</p>
          </div>

          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-orange-500">
            <p className="text-gray-600 text-sm font-medium">61-90 Days Overdue</p>
            <p className="text-3xl font-bold text-orange-600 mt-2">{statistics.overdue61_90}</p>
          </div>

          <div className="bg-white rounded-lg shadow p-6 border-l-4 border-blue-500">
            <p className="text-gray-600 text-sm font-medium">0-30 Days Overdue</p>
            <p className="text-3xl font-bold text-blue-600 mt-2">{statistics.overdue0_30}</p>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-lg shadow p-6 mb-8">
          <div className="flex items-center gap-2 mb-4">
            <Filter size={20} className="text-gray-600" />
            <h3 className="text-lg font-semibold text-gray-900">Filters</h3>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-6 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">As Of Date</label>
              <input
                type="date"
                value={filters.asOfDate}
                onChange={(e) => handleFilterChange('asOfDate', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Aging Bucket</label>
              <select
                value={filters.agingBucket}
                onChange={(e) => handleFilterChange('agingBucket', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm"
              >
                <option value="">All Periods</option>
                <option value="0-30">0-30 Days</option>
                <option value="31-60">31-60 Days</option>
                <option value="61-90">61-90 Days</option>
                <option value="90+">90+ Days</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Min Amount (₹)</label>
              <input
                type="number"
                value={filters.minAmount}
                onChange={(e) => handleFilterChange('minAmount', e.target.value)}
                placeholder="0"
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Sort By</label>
              <select
                value={filters.sortBy}
                onChange={(e) => handleFilterChange('sortBy', e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm"
              >
                <option value="daysoverdue">Days Overdue</option>
                <option value="dueamount">Due Amount</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Order</label>
              <select
                value={filters.descending ? 'desc' : 'asc'}
                onChange={(e) => handleFilterChange('descending', e.target.value === 'desc')}
                className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm"
              >
                <option value="desc">Descending</option>
                <option value="asc">Ascending</option>
              </select>
            </div>

            <div className="flex items-end">
              <button
                onClick={() =>
                  setFilters({
                    asOfDate: today,
                    agingBucket: '',
                    minAmount: '',
                    sortBy: 'daysoverdue',
                    descending: true,
                  })
                }
                className="w-full px-3 py-2 bg-gray-100 text-gray-700 rounded-md text-sm font-medium hover:bg-gray-200"
              >
                Reset
              </button>
            </div>
          </div>
        </div>

        {/* Data Grid */}
        {isLoading ? (
          <div className="bg-white rounded-lg shadow p-12 text-center">
            <div className="inline-block">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            </div>
            <p className="text-gray-600 mt-4">Loading outstanding fees...</p>
          </div>
        ) : outstandingFees.length === 0 ? (
          <div className="bg-white rounded-lg shadow p-12 text-center">
            <AlertCircle size={48} className="mx-auto text-gray-400 mb-4" />
            <h3 className="text-lg font-semibold text-gray-900">No outstanding fees found</h3>
            <p className="text-gray-600 mt-2">All fees are up to date!</p>
          </div>
        ) : (
          <div className="grid gap-4">
            {outstandingFees.map((fee) => (
              <div
                key={`${fee.studentId}-${fee.dueAmount}`}
                className={`bg-white rounded-lg shadow hover:shadow-md transition-shadow ${getAgingColor(fee.agingBucket)}`}
              >
                <div className="p-6">
                  <div className="flex flex-col md:flex-row justify-between md:items-start gap-4">
                    {/* Left: Student Info */}
                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between gap-4">
                        <div>
                          <h3 className="text-lg font-semibold text-gray-900 break-words">{fee.studentInfo}</h3>
                          <p className="text-sm text-gray-600 mt-1">{fee.classSection}</p>
                        </div>
                        <span className={`px-3 py-1 rounded-full text-sm font-medium whitespace-nowrap ${getAgingBadgeColor(fee.agingBucket)}`}>
                          {fee.agingBucket} days
                        </span>
                      </div>
                    </div>

                    {/* Right: Amount and Days */}
                    <div className="flex-1 border-t md:border-t-0 md:border-l border-gray-200 pt-4 md:pt-0 md:pl-6">
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <p className="text-xs text-gray-500 uppercase font-medium">Due Amount</p>
                          <p className="text-2xl font-bold text-gray-900 mt-1">₹{fee.dueAmount.toFixed(2)}</p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-500 uppercase font-medium">Days Overdue</p>
                          <p className="text-2xl font-bold text-rose-600 mt-1">{fee.daysOverdue}</p>
                        </div>
                      </div>
                    </div>
                  </div>

                  {/* Details Row */}
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6 pt-6 border-t border-gray-200">
                    <div>
                      <p className="text-xs text-gray-500 uppercase font-medium">Due Date</p>
                      <p className="text-sm text-gray-900 mt-1 font-medium">
                        {formatDate(fee.dueDate)}
                      </p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500 uppercase font-medium">Last Payment</p>
                      <p className="text-sm text-gray-900 mt-1 font-medium">
                        {fee.lastPaymentDate ? formatDate(fee.lastPaymentDate) : 'Never'}
                      </p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500 uppercase font-medium">Contact</p>
                      <p className="text-sm text-gray-900 mt-1 break-words">{fee.contactInfo || '-'}</p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-500 uppercase font-medium">Status</p>
                      <p className="text-sm mt-1">
                        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                          fee.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'
                        }`}>
                          {fee.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </p>
                    </div>
                  </div>

                  {/* Remarks */}
                  {fee.remarks && (
                    <div className="mt-6 pt-6 border-t border-gray-200">
                      <p className="text-xs text-gray-500 uppercase font-medium">Remarks</p>
                      <p className="text-sm text-gray-700 mt-2">{fee.remarks}</p>
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Summary Footer */}
        {outstandingFees.length > 0 && (
          <div className="bg-white rounded-lg shadow p-6 mt-8">
            <div className="flex flex-col sm:flex-row justify-between gap-4">
              <p className="text-gray-600">Showing <strong>{outstandingFees.length}</strong> records</p>
              <p className="text-gray-900 font-semibold">
                Total Outstanding: <strong className="text-rose-600">₹{statistics.totalDue.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</strong>
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

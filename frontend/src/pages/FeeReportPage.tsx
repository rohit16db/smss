import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { feeApi, type FeeReport } from '../services/api';

type StatusFilter = 'All' | 'Paid' | 'Partial' | 'Due' | 'Overdue';

export function FeeReportPage() {
  const [page, setPage] = useState(0);
  const [rowsPerPage] = useState(10);
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('All');
  const [monthFilter, setMonthFilter] = useState('');

  // Prepare query params
  const getQueryParams = () => {
    const params: {
      pageNumber: number;
      pageSize: number;
      status?: string;
      startDate?: string;
      endDate?: string;
    } = {
      pageNumber: page + 1,
      pageSize: rowsPerPage,
    };

    if (statusFilter !== 'All') {
      params.status = statusFilter;
    }

    if (monthFilter) {
      // Convert YYYY-MM to start and end date
      const [year, month] = monthFilter.split('-');
      const startDate = new Date(Number.parseInt(year, 10), Number.parseInt(month, 10) - 1, 1);
      const endDate = new Date(Number.parseInt(year, 10), Number.parseInt(month, 10), 0);
      params.startDate = startDate.toISOString().split('T')[0];
      params.endDate = endDate.toISOString().split('T')[0];
    }

    return params;
  };

  const { data, isLoading } = useQuery({
    queryKey: ['feeReport', page + 1, rowsPerPage, statusFilter, monthFilter],
    queryFn: () => feeApi.getReport(getQueryParams()),
  });

  const handleStatusChange = (status: StatusFilter) => {
    setStatusFilter(status);
    setPage(0);
  };

  const handleMonthChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setMonthFilter(e.target.value);
    setPage(0);
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Paid':
        return 'bg-gradient-to-r from-green-50 to-green-100 text-green-700 border border-green-200';
      case 'Partial':
        return 'bg-gradient-to-r from-yellow-50 to-yellow-100 text-yellow-700 border border-yellow-200';
      case 'Due':
        return 'bg-gradient-to-r from-blue-50 to-blue-100 text-blue-700 border border-blue-200';
      case 'Overdue':
        return 'bg-gradient-to-r from-red-50 to-red-100 text-red-700 border border-red-200';
      default:
        return 'bg-gray-100 text-gray-700 border border-gray-200';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Paid':
        return '✓';
      case 'Partial':
        return '⚡';
      case 'Due':
        return '⏳';
      case 'Overdue':
        return '⚠';
      default:
        return '•';
    }
  };

  const getPaymentPercentage = (paid: number, total: number) => {
    return total > 0 ? Math.round((paid / total) * 100) : 0;
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('en-IN', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50 to-gray-50 p-4 sm:p-6 lg:p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-4 animate-fade-in">
          <div className="flex items-center gap-2.5">
            <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg flex items-center justify-center text-white text-xl shadow-md">
              📊
            </div>
            <div>
              <h1 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent">
                Fee Payment Report
              </h1>
              <p className="text-gray-600 text-xs">Comprehensive fee collection analysis and tracking</p>
            </div>
          </div>
        </div>

        {/* Summary Cards */}
        {data && (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 mb-4 animate-fade-in">
            {/* Total Due Card */}
            <div className="bg-gradient-to-br from-blue-500 to-blue-600 rounded-xl shadow-md p-4 text-white transform hover:scale-105 transition-transform duration-200">
              <div className="flex items-center justify-between mb-2">
                <div className="text-blue-100 text-xs font-medium">Total Amount</div>
                <div className="w-8 h-8 bg-white/20 rounded-lg flex items-center justify-center text-lg">
                  💰
                </div>
              </div>
              <div className="text-2xl font-bold mb-0.5">
                {formatCurrency(data.totalDueAmount)}
              </div>
              <div className="text-blue-100 text-xs flex items-center gap-1">
                <span className="inline-block w-1.5 h-1.5 bg-blue-200 rounded-full"></span>
                {data.totalCount} students enrolled
              </div>
            </div>

            {/* Total Paid Card */}
            <div className="bg-gradient-to-br from-green-500 to-green-600 rounded-xl shadow-md p-4 text-white transform hover:scale-105 transition-transform duration-200">
              <div className="flex items-center justify-between mb-2">
                <div className="text-green-100 text-xs font-medium">Collected</div>
                <div className="w-8 h-8 bg-white/20 rounded-lg flex items-center justify-center text-lg">
                  ✓
                </div>
              </div>
              <div className="text-2xl font-bold mb-0.5">
                {formatCurrency(data.totalPaidAmount)}
              </div>
              <div className="text-green-100 text-xs flex items-center gap-1">
                <span className="inline-block w-1.5 h-1.5 bg-green-200 rounded-full"></span>
                {data.paidCount} fully paid
              </div>
            </div>

            {/* Balance Card */}
            <div className="bg-gradient-to-br from-orange-500 to-orange-600 rounded-xl shadow-md p-4 text-white transform hover:scale-105 transition-transform duration-200">
              <div className="flex items-center justify-between mb-2">
                <div className="text-orange-100 text-xs font-medium">Outstanding</div>
                <div className="w-8 h-8 bg-white/20 rounded-lg flex items-center justify-center text-lg">
                  📈
                </div>
              </div>
              <div className="text-2xl font-bold mb-0.5">
                {formatCurrency(data.totalBalanceAmount)}
              </div>
              <div className="text-orange-100 text-xs flex items-center gap-1">
                <span className="inline-block w-1.5 h-1.5 bg-orange-200 rounded-full"></span>
                {data.partialCount} partial, {data.dueCount} due
              </div>
            </div>

            {/* Overdue Card */}
            <div className="bg-gradient-to-br from-red-500 to-red-600 rounded-xl shadow-md p-4 text-white transform hover:scale-105 transition-transform duration-200">
              <div className="flex items-center justify-between mb-2">
                <div className="text-red-100 text-xs font-medium">Overdue</div>
                <div className="w-8 h-8 bg-white/20 rounded-lg flex items-center justify-center text-lg">
                  ⚠
                </div>
              </div>
              <div className="text-2xl font-bold mb-0.5">{data.overdueCount}</div>
              <div className="text-red-100 text-xs flex items-center gap-1">
                <span className="inline-block w-1.5 h-1.5 bg-red-200 rounded-full animate-pulse"></span>{' '}
                Requires immediate attention
              </div>
            </div>
          </div>
        )}

        {/* Filters */}
        <div className="bg-white rounded-xl shadow-md mb-4 p-4 border border-gray-100 animate-fade-in">
          <h2 className="text-base font-semibold text-gray-900 mb-3 flex items-center gap-2">
            <span className="text-lg">🔍</span>{' '}
            Filters
          </h2>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            {/* Status Filter */}
            <div>
              <div className="block text-xs font-semibold text-gray-700 mb-2">
                Payment Status
              </div>
              <div className="flex flex-wrap gap-2">
                {(['All', 'Paid', 'Partial', 'Due', 'Overdue'] as StatusFilter[]).map((status) => {
                  const isActive = statusFilter === status;
                  const buttonStyles: Record<string, string> = {
                    All: isActive ? 'bg-gray-600 text-white shadow-md' : 'bg-gray-100 text-gray-700 hover:bg-gray-200',
                    Paid: isActive ? 'bg-green-600 text-white shadow-md' : 'bg-green-50 text-green-700 hover:bg-green-100 border border-green-200',
                    Partial: isActive ? 'bg-yellow-600 text-white shadow-md' : 'bg-yellow-50 text-yellow-700 hover:bg-yellow-100 border border-yellow-200',
                    Due: isActive ? 'bg-blue-600 text-white shadow-md' : 'bg-blue-50 text-blue-700 hover:bg-blue-100 border border-blue-200',
                    Overdue: isActive ? 'bg-red-600 text-white shadow-md' : 'bg-red-50 text-red-700 hover:bg-red-100 border border-red-200',
                  };
                  return (
                    <button
                      key={status}
                      onClick={() => handleStatusChange(status)}
                      className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-all duration-200 transform hover:scale-105 ${buttonStyles[status]}`}
                    >
                      <span className="mr-1">{getStatusIcon(status)}</span>
                      {status}
                    </button>
                  );
                })}
              </div>
            </div>

            {/* Month Filter */}
            <div>
              <label htmlFor="monthFilter" className="block text-xs font-semibold text-gray-700 mb-2">
                Filter by Month
              </label>
              <div className="relative">
                <input
                  type="month"
                  id="monthFilter"
                  value={monthFilter}
                  onChange={handleMonthChange}
                  className="w-full px-3 py-1.5 border-2 border-gray-200 rounded-lg text-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all"
                  placeholder="Select month"
                />
                {monthFilter && (
                  <button
                    onClick={() => {
                      setMonthFilter('');
                      setPage(0);
                    }}
                    className="mt-2 px-3 py-1.5 text-xs font-medium text-blue-600 hover:text-blue-800 hover:bg-blue-50 rounded-lg transition-colors flex items-center gap-1"
                  >
                    <span>✕</span>{' '}
                    Clear month filter
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Table */}
        <div className="bg-white rounded-xl shadow-md border border-gray-100 overflow-hidden animate-fade-in">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center py-12">
              <div className="relative">
                <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-200"></div>
                <div className="animate-spin rounded-full h-12 w-12 border-t-4 border-blue-600 absolute top-0 left-0"></div>
              </div>
              <p className="mt-3 text-gray-500 font-medium text-sm">Loading fee report...</p>
            </div>
          ) : (
            <>
              {/* Desktop Table */}
              <div className="hidden md:block overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gradient-to-r from-gray-50 to-gray-100">
                    <tr>
                      <th className="px-4 py-2.5 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">
                        Student Details
                      </th>
                      <th className="px-4 py-2.5 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">
                        Section
                      </th>
                      <th className="px-4 py-2.5 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">
                        Fee Structure
                      </th>
                      <th className="px-4 py-2.5 text-right text-xs font-bold text-gray-600 uppercase tracking-wider">
                        Total Amount
                      </th>
                      <th className="px-4 py-2.5 text-right text-xs font-bold text-gray-600 uppercase tracking-wider">
                        Paid
                      </th>
                      <th className="px-4 py-2.5 text-right text-xs font-bold text-gray-600 uppercase tracking-wider">
                        Balance
                      </th>
                      <th className="px-4 py-2.5 text-center text-xs font-bold text-gray-600 uppercase tracking-wider">
                        Status
                      </th>
                      <th className="px-4 py-2.5 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">
                        Last Payment
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-100">
                    {data?.items.map((item: FeeReport, index: number) => {
                      const percentage = getPaymentPercentage(item.paidAmount, item.totalAmount);
                      return (
                        <tr key={item.id} className={`transition-all duration-150 hover:bg-blue-50/50 ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50/30'}`}>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <div className="flex items-center gap-2.5">
                              <div className="w-8 h-8 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg flex items-center justify-center text-white font-bold text-xs shadow-md">
                                {item.studentName.charAt(0)}
                              </div>
                              <div>
                                <div className="text-sm font-semibold text-gray-900">{item.studentName}</div>
                                <div className="text-xs text-gray-500 flex items-center gap-1">
                                  <span className="inline-block w-1 h-1 bg-gray-400 rounded-full"></span>
                                  {item.enrollmentNumber}
                                </div>
                              </div>
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <div className="text-sm font-medium text-gray-900">
                              {item.sectionName || <span className="text-gray-400">Not assigned</span>}
                            </div>
                          </td>
                          <td className="px-4 py-3">
                            <div className="text-sm font-medium text-gray-900 max-w-xs truncate">
                              {item.feeStructureName}
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap text-right">
                            <div className="text-sm font-bold text-gray-900">
                              {formatCurrency(item.totalAmount)}
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap text-right">
                            <div className="text-sm font-bold text-green-600">
                              {formatCurrency(item.paidAmount)}
                            </div>
                            <div className="w-full bg-gray-200 rounded-full h-1 mt-1">
                              <div
                                className="bg-gradient-to-r from-green-500 to-green-600 h-1 rounded-full transition-all duration-500"
                                style={{ width: `${percentage}%` }}
                              ></div>
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap text-right">
                            <div className="text-sm font-bold text-orange-600">
                              {formatCurrency(item.balanceAmount)}
                            </div>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap text-center">
                            <span
                              className={`px-2.5 py-1 inline-flex items-center gap-1 text-xs font-bold rounded-lg ${getStatusColor(
                                item.status
                              )}`}
                            >
                              <span>{getStatusIcon(item.status)}</span>
                              {item.status}
                            </span>
                          </td>
                          <td className="px-4 py-3 whitespace-nowrap">
                            <div className="text-sm text-gray-700">{formatDate(item.lastPaymentDate)}</div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>

              {/* Mobile Cards */}
              <div className="md:hidden divide-y divide-gray-200">
                {data?.items.map((item: FeeReport) => {
                  const percentage = getPaymentPercentage(item.paidAmount, item.totalAmount);
                  return (
                    <div key={item.id} className="p-4 hover:bg-blue-50/50 transition-all duration-150">
                      <div className="flex items-start gap-2.5 mb-2.5">
                        <div className="w-10 h-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg flex items-center justify-center text-white font-bold shadow-md flex-shrink-0">
                          {item.studentName.charAt(0)}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="font-semibold text-gray-900 text-sm">{item.studentName}</div>
                          <div className="text-xs text-gray-500">{item.enrollmentNumber}</div>
                        </div>
                        <span
                          className={`px-2 py-1 inline-flex items-center gap-1 text-xs font-bold rounded-lg flex-shrink-0 ${getStatusColor(
                            item.status
                          )}`}
                        >
                          <span>{getStatusIcon(item.status)}</span>
                          {item.status}
                        </span>
                      </div>

                      <div className="bg-gray-50 rounded-lg p-3 space-y-2 text-sm">
                        <div className="flex justify-between items-center">
                          <span className="text-gray-600 font-medium">Section:</span>
                          <span className="text-gray-900 font-semibold">{item.sectionName || 'Not assigned'}</span>
                        </div>
                        <div className="flex justify-between items-center">
                          <span className="text-gray-600 font-medium">Fee Structure:</span>
                          <span className="text-gray-900 font-semibold text-right">{item.feeStructureName}</span>
                        </div>
                        <div className="border-t border-gray-200 pt-2 mt-2">
                          <div className="flex justify-between items-center mb-1">
                            <span className="text-gray-600 font-medium">Total:</span>
                            <span className="font-bold text-gray-900 text-base">
                              {formatCurrency(item.totalAmount)}
                            </span>
                          </div>
                          <div className="flex justify-between items-center mb-1">
                            <span className="text-gray-600 font-medium">Paid:</span>
                            <span className="font-bold text-green-600 text-base">{formatCurrency(item.paidAmount)}</span>
                          </div>
                          <div className="w-full bg-gray-200 rounded-full h-1.5 mb-2">
                            <div
                              className="bg-gradient-to-r from-green-500 to-green-600 h-1.5 rounded-full transition-all duration-500"
                              style={{ width: `${percentage}%` }}
                            ></div>
                          </div>
                          <div className="flex justify-between items-center">
                            <span className="text-gray-600 font-medium">Balance:</span>
                            <span className="font-bold text-orange-600 text-base">
                              {formatCurrency(item.balanceAmount)}
                            </span>
                          </div>
                        </div>
                        <div className="flex justify-between items-center pt-2 border-t border-gray-200">
                          <span className="text-gray-600 font-medium">Last Payment:</span>
                          <span className="text-gray-900 font-semibold">{formatDate(item.lastPaymentDate)}</span>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Pagination */}
              {data && data.totalCount > 0 && (
                <div className="px-4 py-3 bg-gradient-to-r from-gray-50 to-gray-100 flex flex-col sm:flex-row items-center justify-between border-t border-gray-200 gap-3">
                  <div className="text-xs text-gray-700 font-medium">
                    Showing <span className="font-bold text-gray-900">{page * rowsPerPage + 1}</span> to{' '}
                    <span className="font-bold text-gray-900">{Math.min((page + 1) * rowsPerPage, data.totalCount)}</span> of{' '}
                    <span className="font-bold text-gray-900">{data.totalCount}</span> results
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => setPage((p) => Math.max(0, p - 1))}
                      disabled={page === 0}
                      className="px-4 py-2 border-2 border-gray-300 rounded-lg text-xs font-semibold text-gray-700 bg-white hover:bg-gray-50 hover:border-gray-400 disabled:opacity-40 disabled:cursor-not-allowed transition-all shadow-sm hover:shadow"
                    >
                      ← Previous
                    </button>
                    <button
                      onClick={() => setPage((p) => p + 1)}
                      disabled={(page + 1) * rowsPerPage >= data.totalCount}
                      className="px-4 py-2 border-2 border-gray-300 rounded-lg text-xs font-semibold text-gray-700 bg-white hover:bg-gray-50 hover:border-gray-400 disabled:opacity-40 disabled:cursor-not-allowed transition-all shadow-sm hover:shadow"
                    >
                      Next →
                    </button>
                  </div>
                </div>
              )}

              {/* No Results */}
              {data && data.totalCount === 0 && (
                <div className="text-center py-12">
                  <div className="inline-block p-4 bg-gradient-to-br from-blue-50 to-blue-100 rounded-xl mb-3">
                    <div className="text-5xl">📊</div>
                  </div>
                  <h3 className="text-base font-semibold text-gray-900 mb-2">No fee records found</h3>
                  <p className="text-gray-500 mb-4 text-sm">No results match your current filter selection</p>
                  <button
                    onClick={() => {
                      setStatusFilter('All');
                      setMonthFilter('');
                      setPage(0);
                    }}
                    className="px-5 py-2 bg-blue-600 text-white rounded-lg text-sm font-semibold hover:bg-blue-700 transition-colors shadow-md hover:shadow-lg"
                  >
                    Clear all filters
                  </button>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}

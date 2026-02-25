import React, { useState } from 'react';
import { Calendar, Download, Filter, TrendingDown, TrendingUp } from 'lucide-react';
import {
  useFeeCollectionSummary,
  useMonthlyFeeCollectionTrend,
  useFeeCollectionByCategory,
  useOutstandingFees,
} from '../hooks/useFeeReports';
import { FeeCollectionSummaryCard } from '../components/reports/FeeCollectionSummaryCard';
import { FeeCollectionTrendChart } from '../components/reports/FeeCollectionTrendChart';
import { FeeCategoryBreakdownChart } from '../components/reports/FeeCategoryBreakdownChart';
import { OutstandingFeesTable } from '../components/reports/OutstandingFeesTable';
import { exportToCSV } from '../utils/export';

/**
 * Fee Reports Dashboard Page
 * Displays fee collection analytics with trends, breakdowns, and outstanding analysis
 */
export const FeeReportsPage: React.FC = () => {
  const [dateRange, setDateRange] = useState({
    startDate: new Date(new Date().getFullYear(), new Date().getMonth() - 2, 1),
    endDate: new Date(),
  });

  const [selectedCategory, setSelectedCategory] = useState<string | undefined>();
  const [agingBucketFilter, setAgingBucketFilter] = useState<string | undefined>();
  const [outstandingSortBy, setOutstandingSortBy] = useState('daysoverdue');

  // Fetch data
  const summaryQuery = useFeeCollectionSummary(dateRange.startDate, dateRange.endDate, {
    category: selectedCategory,
    prevStartDate: new Date(new Date().getFullYear(), new Date().getMonth() - 4, 1),
    prevEndDate: new Date(new Date().getFullYear(), new Date().getMonth() - 2, 0),
  });

  const trendQuery = useMonthlyFeeCollectionTrend(dateRange.startDate, dateRange.endDate, {
    category: selectedCategory,
  });

  const categoryQuery = useFeeCollectionByCategory(dateRange.startDate, dateRange.endDate);

  const outstandingQuery = useOutstandingFees({
    asOfDate: new Date(),
    agingBucket: agingBucketFilter,
    sortBy: outstandingSortBy,
  });

  // Handle date range change
  const handleDateRangeChange = (type: 'start' | 'end', value: string) => {
    const newDate = new Date(value);
    if (type === 'start') {
      setDateRange({ ...dateRange, startDate: newDate });
    } else {
      setDateRange({ ...dateRange, endDate: newDate });
    }
  };

  // Export data
  const handleExportSummary = () => {
    if (!summaryQuery.data) return;

    const data = [
      {
        Metric: 'Total Expected',
        Amount: summaryQuery.data.totalExpected,
      },
      {
        Metric: 'Total Collected',
        Amount: summaryQuery.data.totalCollected,
      },
      {
        Metric: 'Total Pending',
        Amount: summaryQuery.data.totalPending,
      },
      {
        Metric: 'Total Overdue',
        Amount: summaryQuery.data.totalOverdue,
      },
      {
        Metric: 'Collection Rate (%)',
        Amount: summaryQuery.data.collectionRate.toFixed(2),
      },
      {
        Metric: 'Paid Students',
        Amount: summaryQuery.data.paidStudents,
      },
      {
        Metric: 'Due Students',
        Amount: summaryQuery.data.dueStudents,
      },
      {
        Metric: 'Overdue Students',
        Amount: summaryQuery.data.overdueStudents,
      },
    ];

    exportToCSV(
      data,
      `fee-collection-summary-${new Date().toISOString().split('T')[0]}.csv`
    );
  };

  const handleExportOutstanding = () => {
    if (!outstandingQuery.data) return;

    const data = outstandingQuery.data.map((fee) => ({
      'Student Name': fee.studentInfo,
      'Class/Section': fee.classSection,
      'Due Amount': fee.dueAmount,
      'Days Overdue': fee.daysOverdue,
      'Aging Bucket': fee.agingBucket,
      'Due Date': new Date(fee.dueDate).toLocaleDateString(),
    }));

    exportToCSV(data, `outstanding-fees-${new Date().toISOString().split('T')[0]}.csv`);
  };

  const isLoading = summaryQuery.isLoading || trendQuery.isLoading;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">Fee Reports & Analytics</h1>
      </div>

      {/* Date Range Filter */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center gap-4 flex-wrap">
          <div className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-gray-500" />
            <label htmlFor="startDate" className="text-sm font-medium text-gray-700">Start Date:</label>
            <input
              id="startDate"
              type="date"
              value={dateRange.startDate.toISOString().split('T')[0]}
              onChange={(e) => handleDateRangeChange('start', e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md text-sm"
            />
          </div>

          <div className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-gray-500" />
            <label htmlFor="endDate" className="text-sm font-medium text-gray-700">End Date:</label>
            <input
              id="endDate"
              type="date"
              value={dateRange.endDate.toISOString().split('T')[0]}
              onChange={(e) => handleDateRangeChange('end', e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md text-sm"
            />
          </div>

          {categoryQuery.data && (
            <div className="flex items-center gap-2">
              <Filter className="w-5 h-5 text-gray-500" />
              <label htmlFor="category" className="text-sm font-medium text-gray-700">Category:</label>
              <select
                id="category"
                value={selectedCategory || ''}
                onChange={(e) => setSelectedCategory(e.target.value || undefined)}
                className="px-3 py-2 border border-gray-300 rounded-md text-sm"
              >
                <option value="">All Categories</option>
                {categoryQuery.data.map((cat) => (
                  <option key={cat.category} value={cat.category}>
                    {cat.category}
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>
      </div>

      {/* Summary Cards */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {new Array(4).fill(0).map((_, i) => (
            <div key={`skeleton-${i}`} className="bg-white rounded-lg shadow p-6 animate-pulse">
              <div className="h-4 bg-gray-200 rounded w-24 mb-2"></div>
              <div className="h-8 bg-gray-200 rounded w-32"></div>
            </div>
          ))}
        </div>
      ) : summaryQuery.data ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <FeeCollectionSummaryCard
            title="Total Collected"
            amount={summaryQuery.data.totalCollected}
            icon={TrendingUp}
            trend={summaryQuery.data.collectionRateTrend}
            trendLabel="vs last period"
          />
          <FeeCollectionSummaryCard
            title="Total Expected"
            amount={summaryQuery.data.totalExpected}
            trend={
              summaryQuery.data.totalExpected > 0
                ? ((summaryQuery.data.totalCollected / summaryQuery.data.totalExpected) * 100)
                : 0
            }
            trendLabel="Collection %"
          />
          <FeeCollectionSummaryCard
            title="Outstanding"
            amount={summaryQuery.data.totalPending + summaryQuery.data.totalOverdue}
            icon={TrendingDown}
            textColor="text-orange-600"
          />
          <FeeCollectionSummaryCard
            title="Overdue"
            amount={summaryQuery.data.totalOverdue}
            icon={TrendingDown}
            textColor="text-red-600"
            count={summaryQuery.data.overdueStudents}
            countLabel="students"
          />
        </div>
      ) : null}

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Trend Chart */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Monthly Trend</h2>
            <button
              onClick={handleExportSummary}
              className="flex items-center gap-2 px-3 py-1 text-sm text-gray-600 hover:text-gray-900"
            >
              <Download className="w-4 h-4" />
              Export CSV
            </button>
          </div>
          {trendQuery.isLoading ? (
            <div className="h-64 bg-gray-100 rounded animate-pulse"></div>
          ) : trendQuery.data ? (
            <FeeCollectionTrendChart data={trendQuery.data} />
          ) : null}
        </div>

        {/* Category Breakdown */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Category Breakdown</h2>
          </div>
          {categoryQuery.isLoading ? (
            <div className="h-64 bg-gray-100 rounded animate-pulse"></div>
          ) : categoryQuery.data ? (
            <FeeCategoryBreakdownChart data={categoryQuery.data} />
          ) : null}
        </div>
      </div>

      {/* Outstanding Fees Section */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">Outstanding Fees (Aging)</h2>
          <button
            onClick={handleExportOutstanding}
            className="flex items-center gap-2 px-3 py-1 text-sm text-gray-600 hover:text-gray-900"
          >
            <Download className="w-4 h-4" />
            Export CSV
          </button>
        </div>

        {/* Filters */}
        <div className="flex gap-3 mb-4">
          <select
            value={agingBucketFilter || ''}
            onChange={(e) => setAgingBucketFilter(e.target.value || undefined)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm"
          >
            <option value="">All Aging Buckets</option>
            <option value="0-30">0-30 Days</option>
            <option value="31-60">31-60 Days</option>
            <option value="61-90">61-90 Days</option>
            <option value="90+">90+ Days</option>
          </select>

          <select
            value={outstandingSortBy}
            onChange={(e) => setOutstandingSortBy(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm"
          >
            <option value="daysoverdue">Sort: Days Overdue</option>
            <option value="dueamount">Sort: Due Amount</option>
            <option value="name">Sort: Student Name</option>
            <option value="class">Sort: Class</option>
          </select>
        </div>

        {outstandingQuery.isLoading ? (
          <div className="h-64 bg-gray-100 rounded animate-pulse"></div>
        ) : outstandingQuery.data ? (
          <OutstandingFeesTable data={outstandingQuery.data} />
        ) : null}
      </div>
    </div>
  );
};

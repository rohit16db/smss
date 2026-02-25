import React, { useState } from 'react';
import { Calendar, Download, TrendingUp } from 'lucide-react';
import {
  useSalaryExpenseSummary,
  useMonthlySalaryTrend,
  useSalaryComponentBreakdown,
  useTeacherSalaryComparison,
  useAttendanceToSalaryCorrelation,
} from '../hooks/useSalaryReports';
import { SalaryExpenseSummaryCard } from '../components/reports/SalaryExpenseSummaryCard';
import { MonthlySalaryTrendChart } from '../components/reports/MonthlySalaryTrendChart';
import { SalaryComponentPieChart } from '../components/reports/SalaryComponentPieChart';
import { TeacherSalaryComparisonTable } from '../components/reports/TeacherSalaryComparisonTable';
import { AttendanceCorrelationTable } from '../components/reports/AttendanceCorrelationTable';
import { exportToCSV } from '../utils/export';

/**
 * Salary Reports & Analytics Page
 * Displays salary expense analytics, teacher comparisons, and attendance correlation
 */
export const SalaryReportsPage: React.FC = () => {
  const [dateRange, setDateRange] = useState({
    startDate: new Date(new Date().getFullYear(), new Date().getMonth() - 2, 1),
    endDate: new Date(),
  });

  const [selectedMonth, setSelectedMonth] = useState(new Date());
  const [selectedSortBy, setSelectedSortBy] = useState('name');
  const [showDiscrepanciesOnly, setShowDiscrepanciesOnly] = useState(false);

  // Fetch data
  const expenseSummaryQuery = useSalaryExpenseSummary(
    dateRange.startDate,
    dateRange.endDate,
    {
      prevStartDate: new Date(dateRange.startDate.getFullYear(), dateRange.startDate.getMonth() - 2, 1),
      prevEndDate: new Date(dateRange.startDate.getFullYear(), dateRange.startDate.getMonth() - 1, 0),
    }
  );

  const trendQuery = useMonthlySalaryTrend(dateRange.startDate, dateRange.endDate);

  const componentBreakdownQuery = useSalaryComponentBreakdown(
    dateRange.startDate,
    dateRange.endDate
  );

  const teacherComparisonQuery = useTeacherSalaryComparison(
    dateRange.startDate,
    dateRange.endDate,
    {
      sortBy: selectedSortBy,
      descending: selectedSortBy !== 'name',
    }
  );

  const attendanceCorrelationQuery = useAttendanceToSalaryCorrelation(selectedMonth, {
    onlyDiscrepancies: showDiscrepanciesOnly,
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

  // Export functions
  const handleExportSummary = () => {
    if (!expenseSummaryQuery.data) return;

    const data = [
      {
        Metric: 'Total Net Salary',
        Amount: expenseSummaryQuery.data.totalNetSalary,
      },
      {
        Metric: 'Average Salary',
        Amount: expenseSummaryQuery.data.averageSalary,
      },
      {
        Metric: 'Total Base Salary',
        Amount: expenseSummaryQuery.data.totalBaseSalary,
      },
      {
        Metric: 'Total Bonus',
        Amount: expenseSummaryQuery.data.totalBonus,
      },
      {
        Metric: 'Total Deductions',
        Amount: expenseSummaryQuery.data.totalDeductions,
      },
      {
        Metric: 'Bonus Percentage (%)',
        Amount: expenseSummaryQuery.data.bonusPercentage.toFixed(2),
      },
      {
        Metric: 'Deduction Percentage (%)',
        Amount: expenseSummaryQuery.data.deductionPercentage.toFixed(2),
      },
      {
        Metric: 'Teacher Count',
        Amount: expenseSummaryQuery.data.teacherCount,
      },
      {
        Metric: 'Bonus Recipients',
        Amount: expenseSummaryQuery.data.bonusRecipients,
      },
    ];

    exportToCSV(
      data,
      `salary-expense-summary-${new Date().toISOString().split('T')[0]}.csv`
    );
  };

  const handleExportTeacherComparison = () => {
    if (!teacherComparisonQuery.data) return;

    const data = teacherComparisonQuery.data.map((teacher) => ({
      'Teacher Name': teacher.teacherName,
      'Base Salary': teacher.baseSalary,
      'Bonus': teacher.bonus,
      'Deductions': teacher.deductions,
      'Net Salary': teacher.netSalary,
      'Bonus Eligible': teacher.bonusEligible ? 'Yes' : 'No',
      'Status': teacher.status,
    }));

    exportToCSV(
      data,
      `teacher-salary-comparison-${new Date().toISOString().split('T')[0]}.csv`
    );
  };

  const isLoading = expenseSummaryQuery.isLoading || trendQuery.isLoading;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold text-gray-900">Salary Analytics & Reports</h1>
      </div>

      {/* Date Range Filter */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center gap-4 flex-wrap">
          <div className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-gray-500" />
            <label className="text-sm font-medium text-gray-700">Start Date:</label>
            <input
              type="date"
              value={dateRange.startDate.toISOString().split('T')[0]}
              onChange={(e) => handleDateRangeChange('start', e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md text-sm"
            />
          </div>

          <div className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-gray-500" />
            <label className="text-sm font-medium text-gray-700">End Date:</label>
            <input
              type="date"
              value={dateRange.endDate.toISOString().split('T')[0]}
              onChange={(e) => handleDateRangeChange('end', e.target.value)}
              className="px-3 py-2 border border-gray-300 rounded-md text-sm"
            />
          </div>

          <div className="flex items-center gap-2">
            <Calendar className="w-5 h-5 text-gray-500" />
            <label className="text-sm font-medium text-gray-700">Correlation Month:</label>
            <input
              type="month"
              value={selectedMonth.toISOString().slice(0, 7)}
              onChange={(e) => setSelectedMonth(new Date(e.target.value + '-01'))}
              className="px-3 py-2 border border-gray-300 rounded-md text-sm"
            />
          </div>
        </div>
      </div>

      {/* Summary Cards */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {[...Array(4)].map((_, i) => (
            <div key={i} className="bg-white rounded-lg shadow p-6 animate-pulse">
              <div className="h-4 bg-gray-200 rounded w-24 mb-2"></div>
              <div className="h-8 bg-gray-200 rounded w-32"></div>
            </div>
          ))}
        </div>
      ) : expenseSummaryQuery.data ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <SalaryExpenseSummaryCard
            title="Total Net Salary"
            amount={expenseSummaryQuery.data.totalNetSalary}
            icon={TrendingUp}
            trend={expenseSummaryQuery.data.expenseTrend}
            trendLabel="vs last period"
          />
          <SalaryExpenseSummaryCard
            title="Average Salary"
            amount={expenseSummaryQuery.data.averageSalary}
          />
          <SalaryExpenseSummaryCard
            title="Total Bonus"
            amount={expenseSummaryQuery.data.totalBonus}
            count={expenseSummaryQuery.data.bonusRecipients}
            countLabel="recipients"
            textColor="text-green-600"
          />
          <SalaryExpenseSummaryCard
            title="Total Deductions"
            amount={expenseSummaryQuery.data.totalDeductions}
            textColor="text-orange-600"
          />
        </div>
      ) : null}

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Trend Chart */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Monthly Salary Trend</h2>
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
            <MonthlySalaryTrendChart data={trendQuery.data} />
          ) : null}
        </div>

        {/* Component Breakdown */}
        <div className="bg-white rounded-lg shadow p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Component Breakdown</h2>
          </div>
          {componentBreakdownQuery.isLoading ? (
            <div className="h-64 bg-gray-100 rounded animate-pulse"></div>
          ) : componentBreakdownQuery.data ? (
            <SalaryComponentPieChart data={componentBreakdownQuery.data} />
          ) : null}
        </div>
      </div>

      {/* Teacher Salary Comparison */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">Teacher Salary Comparison</h2>
          <button
            onClick={handleExportTeacherComparison}
            className="flex items-center gap-2 px-3 py-1 text-sm text-gray-600 hover:text-gray-900"
          >
            <Download className="w-4 h-4" />
            Export CSV
          </button>
        </div>

        {/* Sort Filter */}
        <div className="flex gap-3 mb-4">
          <select
            value={selectedSortBy}
            onChange={(e) => setSelectedSortBy(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm"
          >
            <option value="name">Sort: Teacher Name</option>
            <option value="netsalary">Sort: Net Salary</option>
            <option value="bonus">Sort: Bonus</option>
            <option value="deduction">Sort: Deductions</option>
          </select>
        </div>

        {teacherComparisonQuery.isLoading ? (
          <div className="h-64 bg-gray-100 rounded animate-pulse"></div>
        ) : teacherComparisonQuery.data ? (
          <TeacherSalaryComparisonTable data={teacherComparisonQuery.data} />
        ) : null}
      </div>

      {/* Attendance to Salary Correlation */}
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">
            Attendance to Salary Correlation
          </h2>
        </div>

        {/* Filter */}
        <div className="flex gap-3 mb-4">
          <label className="flex items-center gap-2 px-3 py-2 border border-gray-300 rounded-md text-sm cursor-pointer">
            <input
              type="checkbox"
              checked={showDiscrepanciesOnly}
              onChange={(e) => setShowDiscrepanciesOnly(e.target.checked)}
              className="rounded"
            />
            <span>Show Discrepancies Only</span>
          </label>
        </div>

        {attendanceCorrelationQuery.isLoading ? (
          <div className="h-64 bg-gray-100 rounded animate-pulse"></div>
        ) : attendanceCorrelationQuery.data ? (
          <AttendanceCorrelationTable data={attendanceCorrelationQuery.data} />
        ) : null}
      </div>
    </div>
  );
};

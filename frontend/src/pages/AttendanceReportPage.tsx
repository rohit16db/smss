import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, type UseQueryResult } from '@tanstack/react-query';
import { attendanceApi, type MonthlyAttendanceReportItem, type LowAttendanceAlertDto, type ClassAttendanceSummaryDto, type PaginatedMonthlyAttendanceReportDto } from '../services/api';
import { formatDate } from '../utils/dateFormat';
import type { AxiosError } from 'axios';

type TabType = 'monthly' | 'low-attendance' | 'class-summary';

// Helper functions
function getAttendanceColor(percentage: number): string {
  if (percentage >= 75) return 'text-green-600 bg-green-50';
  if (percentage >= 50) return 'text-yellow-600 bg-yellow-50';
  return 'text-red-600 bg-red-50';
}

function getAlertColor(level: string): string {
  return level === 'Critical' ? 'border-red-300 bg-red-50' : 'border-yellow-300 bg-yellow-50';
}

function downloadCSV(csv: string, filename: string) {
  const element = document.createElement('a');
  element.setAttribute('href', 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv));
  element.setAttribute('download', filename);
  element.style.display = 'none';
  document.body.appendChild(element);
  element.click();
  element.remove();
}

// Component Props Types
interface MonthlyReportTabProps {
  readonly data: UseQueryResult<PaginatedMonthlyAttendanceReportDto, Error>;
  readonly monthName: string;
  readonly onExport: () => void;
  readonly pageNumber: number;
  readonly setPageNumber: (page: number) => void;
  readonly canLoadMore: boolean;
}

interface LowAttendanceTabProps {
  readonly data: UseQueryResult<LowAttendanceAlertDto[], Error>;
  readonly monthName: string;
  readonly onExport: () => void;
}

interface ClassSummaryTabProps {
  readonly data: UseQueryResult<ClassAttendanceSummaryDto[], Error>;
  readonly monthName: string;
  readonly onExport: () => void;
}

export function AttendanceReportPage() {
  const [activeTab, setActiveTab] = useState<TabType>('monthly');
  const currentDate = new Date();
  const [year, setYear] = useState(currentDate.getFullYear());
  const [month, setMonth] = useState(currentDate.getMonth() + 1);
  const [pageNumber, setPageNumber] = useState(1);
  const [alertThreshold, setAlertThreshold] = useState(75);

  // Monthly Report Query
  const monthlyReportQuery = useQuery({
    queryKey: ['monthlyAttendanceReport', year, month, pageNumber],
    queryFn: async () => {
      try {
        return await attendanceApi.getMonthlyAttendanceReport(year, month, {
          pageNumber,
          pageSize: 15
        });
      } catch (error) {
        const axiosError = error as AxiosError<{ message?: string }>;
        toast.error(axiosError.response?.data?.message || 'Failed to load monthly report');
        throw error;
      }
    },
    enabled: activeTab === 'monthly'
  });

  // Low Attendance Alerts Query
  const lowAttendanceQuery = useQuery({
    queryKey: ['lowAttendanceAlerts', year, month, alertThreshold],
    queryFn: async () => {
      try {
        return await attendanceApi.getLowAttendanceAlerts(year, month, {
          threshold: alertThreshold
        });
      } catch (error) {
        const axiosError = error as AxiosError<{ message?: string }>;
        toast.error(axiosError.response?.data?.message || 'Failed to load low attendance alerts');
        throw error;
      }
    },
    enabled: activeTab === 'low-attendance'
  });

  // Class Summary Query
  const classSummaryQuery = useQuery({
    queryKey: ['classAttendanceSummary', year, month],
    queryFn: async () => {
      try {
        return await attendanceApi.getClassAttendanceSummary(year, month);
      } catch (error) {
        const axiosError = error as AxiosError<{ message?: string }>;
        toast.error(axiosError.response?.data?.message || 'Failed to load class summary');
        throw error;
      }
    },
    enabled: activeTab === 'class-summary'
  });

  const monthName = new Date(year, month - 1).toLocaleString('default', { month: 'long', year: 'numeric' });

  // Export to CSV
  const handleExportMonthly = () => {
    if (!monthlyReportQuery.data?.items) return;

    const csv = [
      ['Student Name', 'Enrollment #', 'Section', 'Present', 'Absent', 'Late', 'Leave', 'Attendance %', 'Status'].join(','),
      ...monthlyReportQuery.data.items.map(item =>
        [
          `"${item.studentName}"`,
          item.enrollmentNumber,
          item.sectionName,
          item.presentDays,
          item.absentDays,
          item.lateDays,
          item.leaveDays,
          item.attendancePercentage.toFixed(2),
          item.attendanceStatus
        ].join(',')
      )
    ].join('\n');

    downloadCSV(csv, `monthly-attendance-${year}-${month}.csv`);
    toast.success('Report exported successfully');
  };

  const handleExportAlerts = () => {
    if (!lowAttendanceQuery.data) return;

    const csv = [
      ['Student Name', 'Enrollment #', 'Section', 'Attendance %', 'Absent Days', 'Alert Level', 'Last Absent'].join(','),
      ...lowAttendanceQuery.data.map(alert =>
        [
          `"${alert.studentName}"`,
          alert.enrollmentNumber,
          alert.sectionName,
          alert.attendancePercentage.toFixed(2),
          alert.absentDays,
          alert.alertLevel,
          formatDate(alert.lastAbsentDate)
        ].join(',')
      )
    ].join('\n');

    downloadCSV(csv, `low-attendance-alerts-${year}-${month}.csv`);
    toast.success('Alerts exported successfully');
  };

  const handleExportClassSummary = () => {
    if (!classSummaryQuery.data) return;

    const csv = [
      ['Class', 'Section', 'Total Students', 'Average Attendance %', 'High (≥75%)', 'Medium (50-75%)', 'Low (<50%)'].join(','),
      ...classSummaryQuery.data.map(summary =>
        [
          summary.className,
          summary.sectionName,
          summary.totalStudents,
          summary.averageAttendancePercentage.toFixed(2),
          summary.highAttendanceCount,
          summary.mediumAttendanceCount,
          summary.lowAttendanceCount
        ].join(',')
      )
    ].join('\n');

    downloadCSV(csv, `class-attendance-summary-${year}-${month}.csv`);
    toast.success('Summary exported successfully');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                📊 Attendance Reports
              </h1>
              <p className="text-gray-600 mt-2">Monitor student attendance trends, alerts, and class-wise summaries</p>
            </div>
          </div>

          {/* Filters */}
          <div className="bg-white rounded-lg shadow-md p-6 border border-gray-200">
            <h2 className="text-lg font-semibold mb-4 text-gray-800">Report Period</h2>
            <div className="flex flex-col sm:flex-row gap-4">
              <div className="flex-1">
                <label htmlFor="year" className="block text-sm font-medium text-gray-700 mb-2">Year</label>
                <input
                  id="year"
                  type="number"
                  min="2020"
                  max="2030"
                  value={year}
                  onChange={(e) => { setYear(Number.parseInt(e.target.value)); setPageNumber(1); }}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div className="flex-1">
                <label htmlFor="month" className="block text-sm font-medium text-gray-700 mb-2">Month</label>
                <select
                  id="month"
                  value={month}
                  onChange={(e) => { setMonth(Number.parseInt(e.target.value)); setPageNumber(1); }}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  {Array.from({ length: 12 }, (_, i) => (
                    <option key={i + 1} value={i + 1}>
                      {new Date(year, i).toLocaleString('default', { month: 'long' })}
                    </option>
                  ))}
                </select>
              </div>
              {activeTab === 'low-attendance' && (
                <div className="flex-1">
                  <label htmlFor="threshold" className="block text-sm font-medium text-gray-700 mb-2">Threshold (%)</label>
                  <input
                    id="threshold"
                    type="number"
                    min="0"
                    max="100"
                    value={alertThreshold}
                    onChange={(e) => setAlertThreshold(Number.parseInt(e.target.value))}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              )}
            </div>
          </div>

          {/* Tabs */}
          <div className="border-b border-gray-200">
            <nav className="-mb-px flex space-x-8">
              <button
                onClick={() => { setActiveTab('monthly'); setPageNumber(1); }}
                className={`${
                  activeTab === 'monthly'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
              >
                📅 Monthly Report
              </button>
              <button
                onClick={() => { setActiveTab('low-attendance'); }}
                className={`${
                  activeTab === 'low-attendance'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
              >
                ⚠️ Low Attendance Alerts
              </button>
              <button
                onClick={() => { setActiveTab('class-summary'); }}
                className={`${
                  activeTab === 'class-summary'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
              >
                🏫 Class Summary
              </button>
            </nav>
          </div>

          {/* Tab Content */}
          {activeTab === 'monthly' && (
            <MonthlyReportTab
              data={monthlyReportQuery}
              monthName={monthName}
              onExport={handleExportMonthly}
              pageNumber={pageNumber}
              setPageNumber={setPageNumber}
              canLoadMore={Boolean(monthlyReportQuery.data && pageNumber * 15 < monthlyReportQuery.data.totalCount)}
            />
          )}
          {activeTab === 'low-attendance' && (
            <LowAttendanceTab data={lowAttendanceQuery} monthName={monthName} onExport={handleExportAlerts} />
          )}
          {activeTab === 'class-summary' && (
            <ClassSummaryTab data={classSummaryQuery} monthName={monthName} onExport={handleExportClassSummary} />
          )}
        </div>
      </div>
    </div>
  );
}

// Monthly Report Tab Component
function MonthlyReportTab({ data, monthName, onExport, pageNumber, setPageNumber, canLoadMore }: MonthlyReportTabProps) {
  const isLoading = data.isLoading;
  const reportData = data.data;
  const statusColor = (status: string) => {
    if (status === 'Good') return 'bg-green-100 text-green-800';
    if (status === 'Warning') return 'bg-yellow-100 text-yellow-800';
    return 'bg-red-100 text-red-800';
  };

  return (
    <div className="space-y-4">
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <div>
            <h3 className="text-xl font-semibold text-gray-800">Student Attendance Report</h3>
            <p className="text-gray-600 text-sm mt-1">For {monthName}</p>
          </div>
          <button
            onClick={onExport}
            disabled={isLoading}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors font-medium"
          >
            📥 Export CSV
          </button>
        </div>

        {reportData && (
          <div className="mb-4 grid grid-cols-1 sm:grid-cols-4 gap-4">
            <div className="bg-blue-50 p-4 rounded-lg border border-blue-200">
              <p className="text-sm text-gray-600">Avg Attendance</p>
              <p className="text-2xl font-bold text-blue-600">{reportData.averageAttendancePercentage.toFixed(1)}%</p>
            </div>
            <div className="bg-red-50 p-4 rounded-lg border border-red-200">
              <p className="text-sm text-gray-600">Low Attendance</p>
              <p className="text-2xl font-bold text-red-600">{reportData.lowAttendanceCount}</p>
            </div>
            <div className="bg-green-50 p-4 rounded-lg border border-green-200">
              <p className="text-sm text-gray-600">Total Students</p>
              <p className="text-2xl font-bold text-green-600">{reportData.totalCount}</p>
            </div>
            <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
              <p className="text-sm text-gray-600">Current Page</p>
              <p className="text-2xl font-bold text-gray-600">{pageNumber}</p>
            </div>
          </div>
        )}

        {isLoading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            <p className="text-gray-600 mt-4">Loading report...</p>
          </div>
        ) : reportData?.items && reportData.items.length > 0
          ? (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-100 border-b border-gray-300">
                <tr>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Student Name</th>
                  <th className="px-4 py-3 text-left text-sm font-semibold text-gray-700">Section</th>
                  <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700">Present</th>
                  <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700">Absent</th>
                  <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700">Late</th>
                  <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700">Leave</th>
                  <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700">Attendance %</th>
                  <th className="px-4 py-3 text-center text-sm font-semibold text-gray-700">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {reportData.items.map((item: MonthlyAttendanceReportItem) => (
                  <tr key={item.studentId} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm text-gray-800 font-medium">{item.studentName}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{item.sectionName}</td>
                    <td className="px-4 py-3 text-sm text-center text-green-600 font-semibold">{item.presentDays}</td>
                    <td className="px-4 py-3 text-sm text-center text-red-600 font-semibold">{item.absentDays}</td>
                    <td className="px-4 py-3 text-sm text-center text-yellow-600 font-semibold">{item.lateDays}</td>
                    <td className="px-4 py-3 text-sm text-center text-blue-600 font-semibold">{item.leaveDays}</td>
                    <td className={`px-4 py-3 text-sm text-center font-bold rounded ${getAttendanceColor(item.attendancePercentage)}`}>
                      {item.attendancePercentage.toFixed(1)}%
                    </td>
                    <td className="px-4 py-3 text-sm text-center">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold ${statusColor(item.attendanceStatus)}`}>
                        {item.attendanceStatus}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-12">
            <p className="text-gray-600">No attendance data available for this period</p>
          </div>
        )}

        {reportData && reportData.totalCount > 0 && (
          <div className="flex justify-between items-center mt-6">
            <p className="text-sm text-gray-600">
              Showing {(pageNumber - 1) * 15 + 1} to {Math.min(pageNumber * 15, reportData.totalCount)} of {reportData.totalCount} students
            </p>
            <div className="space-x-2">
              <button
                onClick={() => setPageNumber(Math.max(1, pageNumber - 1))}
                disabled={pageNumber === 1 || isLoading}
                className="px-4 py-2 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300 disabled:bg-gray-100 disabled:cursor-not-allowed"
              >
                ← Previous
              </button>
              <button
                onClick={() => setPageNumber(pageNumber + 1)}
                disabled={!canLoadMore || isLoading}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
              >
                Next →
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

// Low Attendance Alerts Tab Component
function LowAttendanceTab({ data, monthName, onExport }: LowAttendanceTabProps) {
  const isLoading = data.isLoading;
  const alerts = data.data;
  const getAlertBadgeColor = (level: string) => {
    return level === 'Critical' ? 'bg-red-600 text-white' : 'bg-yellow-600 text-white';
  };

  return (
    <div className="space-y-4">
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <div>
            <h3 className="text-xl font-semibold text-gray-800">Low Attendance Alerts</h3>
            <p className="text-gray-600 text-sm mt-1">Students below 75% attendance for {monthName}</p>
          </div>
          <button
            onClick={onExport}
            disabled={isLoading || !alerts || alerts.length === 0}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors font-medium"
          >
            📥 Export CSV
          </button>
        </div>

        {isLoading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            <p className="text-gray-600 mt-4">Loading alerts...</p>
          </div>
        ) : alerts && alerts.length > 0 ? (
          <div className="space-y-3">
            {alerts.map((alert: LowAttendanceAlertDto) => (
              <div key={alert.studentId} className={`p-4 rounded-lg border-l-4 ${getAlertColor(alert.alertLevel)}`}>
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2">
                  <div>
                    <p className="font-semibold text-gray-800">{alert.studentName}</p>
                    <p className="text-sm text-gray-600">{alert.sectionName} • {alert.enrollmentNumber}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-lg font-bold text-red-600">{alert.attendancePercentage.toFixed(1)}%</p>
                    <p className="text-xs text-gray-600">{alert.absentDays}/{alert.totalDays} absent</p>
                  </div>
                  <div>
                    <span className={`px-3 py-1 rounded-full text-xs font-bold ${getAlertBadgeColor(alert.alertLevel)}`}>
                      {alert.alertLevel}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-12 bg-green-50 rounded-lg">
            <p className="text-green-700 font-semibold">✅ No attendance concerns</p>
            <p className="text-green-600 text-sm mt-2">All students have good attendance</p>
          </div>
        )}
      </div>
    </div>
  );
}

// Class Summary Tab Component
function ClassSummaryTab({ data, monthName, onExport }: ClassSummaryTabProps) {
  const isLoading = data.isLoading;
  const summaries = data.data;

  return (
    <div className="space-y-4">
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
          <div>
            <h3 className="text-xl font-semibold text-gray-800">Class-wise Attendance Summary</h3>
            <p className="text-gray-600 text-sm mt-1">For {monthName}</p>
          </div>
          <button
            onClick={onExport}
            disabled={isLoading || !summaries || summaries.length === 0}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors font-medium"
          >
            📥 Export CSV
          </button>
        </div>

        {isLoading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            <p className="text-gray-600 mt-4">Loading summaries...</p>
          </div>
        ) : summaries && summaries.length > 0 ? (
          <div className="space-y-4">
            {summaries.map((summary: ClassAttendanceSummaryDto) => (
              <div key={summary.sectionId} className="border border-gray-200 rounded-lg p-4 hover:shadow-lg transition-shadow">
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-3">
                  <div>
                    <p className="font-semibold text-lg text-gray-800">{summary.className} - {summary.sectionName}</p>
                    <p className="text-sm text-gray-600">{summary.totalStudents} students</p>
                  </div>
                  <div className="text-right">
                    <p className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                      {summary.averageAttendancePercentage.toFixed(1)}%
                    </p>
                    <p className="text-sm text-gray-600">Average Attendance</p>
                  </div>
                </div>

                <div className="grid grid-cols-3 gap-3">
                  <div className="bg-green-50 p-3 rounded-lg border border-green-200">
                    <p className="text-xs text-green-600 font-semibold">GOOD (≥75%)</p>
                    <p className="text-2xl font-bold text-green-600">{summary.highAttendanceCount}</p>
                  </div>
                  <div className="bg-yellow-50 p-3 rounded-lg border border-yellow-200">
                    <p className="text-xs text-yellow-600 font-semibold">MEDIUM (50-75%)</p>
                    <p className="text-2xl font-bold text-yellow-600">{summary.mediumAttendanceCount}</p>
                  </div>
                  <div className="bg-red-50 p-3 rounded-lg border border-red-200">
                    <p className="text-xs text-red-600 font-semibold">LOW (&lt;50%)</p>
                    <p className="text-2xl font-bold text-red-600">{summary.lowAttendanceCount}</p>
                  </div>
                </div>

                {summary.lowAttendanceCount > 0 && (
                  <div className="mt-3 bg-red-50 border border-red-200 rounded p-2">
                    <p className="text-sm text-red-700 font-semibold">⚠️ {summary.lowAttendanceCount} students need attention</p>
                  </div>
                )}
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-12">
            <p className="text-gray-600">No attendance data available</p>
          </div>
        )}
      </div>
    </div>
  );
}

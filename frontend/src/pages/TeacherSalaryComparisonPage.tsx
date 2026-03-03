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
import './ReportPages.css';

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
      <div className="report-page">
        <div className="error-container" style={{
          backgroundColor: '#fee',
          border: '1px solid #fcc',
          borderRadius: '8px',
          padding: '20px',
          margin: '20px',
          color: '#c00'
        }}>
          <h2>Error Loading Data</h2>
          <p>{error instanceof Error ? error.message : 'Failed to load teacher salary comparison data'}</p>
          <button
            onClick={() => refetch()}
            className="btn-primary"
            style={{
              backgroundColor: '#007bff',
              color: 'white',
              padding: '10px 20px',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer',
              marginTop: '10px'
            }}
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="report-page">
      <div className="report-header">
        <div>
          <h1>Teacher Salary Comparison</h1>
          <p>Analyze and compare teacher salaries across periods</p>
        </div>
        <div className="header-actions">
          <button
            onClick={() => refetch()}
            disabled={isLoading}
            className="btn-icon"
            title="Refresh data"
          >
            <RefreshCw size={20} />
          </button>
          <button
            onClick={handleExport}
            disabled={salaryComparison.length === 0}
            className="btn-icon"
            title="Export to CSV"
          >
            <Download size={20} />
          </button>
        </div>
      </div>

      {/* Summary Statistics */}
      <div className="stats-grid">
        <div className="stat-card info">
          <div className="stat-icon">
            <BarChart3 size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Average Net Salary</p>
            <p className="stat-value">₹{statistics.averageNetSalary.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
          </div>
        </div>

        <div className="stat-card success">
          <div className="stat-icon">
            <TrendingUp size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Paid Status</p>
            <p className="stat-value">{statistics.paidCount}</p>
            <p className="stat-subtitle">Salaries paid</p>
          </div>
        </div>

        <div className="stat-card warning">
          <div className="stat-icon">
            <Calendar size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Pending Approval</p>
            <p className="stat-value">{statistics.pendingCount}</p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">
            <BarChart size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Total Bonus Paid</p>
            <p className="stat-value">₹{statistics.totalBonus.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="filter-panel">
        <div className="filter-title">
          <Filter size={20} />
          <h3>Filters</h3>
        </div>

        <div className="filter-grid">
          <div className="filter-group">
            <label>Start Date</label>
            <input
              type="date"
              value={filters.startDate}
              onChange={(e) => handleFilterChange('startDate', e.target.value)}
              className="filter-input"
            />
          </div>

          <div className="filter-group">
            <label>End Date</label>
            <input
              type="date"
              value={filters.endDate}
              onChange={(e) => handleFilterChange('endDate', e.target.value)}
              className="filter-input"
            />
          </div>

          <div className="filter-group">
            <label>Status</label>
            <select
              value={filters.status}
              onChange={(e) => handleFilterChange('status', e.target.value)}
              className="filter-input"
            >
              <option value="">All Statuses</option>
              <option value="Pending">Pending</option>
              <option value="Approved">Approved</option>
              <option value="Paid">Paid</option>
            </select>
          </div>

          <div className="filter-group">
            <label>Sort By</label>
            <select
              value={filters.sortBy}
              onChange={(e) => handleFilterChange('sortBy', e.target.value)}
              className="filter-input"
            >
              <option value="name">Name</option>
              <option value="netsalary">Net Salary</option>
              <option value="bonus">Bonus</option>
              <option value="deduction">Deduction</option>
            </select>
          </div>

          <div className="filter-group">
            <label>Sort Order</label>
            <select
              value={filters.descending ? 'desc' : 'asc'}
              onChange={(e) => handleFilterChange('descending', e.target.value === 'desc')}
              className="filter-input"
            >
              <option value="asc">Ascending</option>
              <option value="desc">Descending</option>
            </select>
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
            className="btn-secondary"
          >
            Reset Filters
          </button>
        </div>
      </div>

      {/* Data Table */}
      <div className="table-container">
        {isLoading ? (
          <div className="loading-state">
            <div className="spinner"></div>
            <p>Loading salary comparison data...</p>
          </div>
        ) : salaryComparison.length === 0 ? (
          <div className="empty-state">
            <BarChart size={48} />
            <h3>No salary records found</h3>
            <p>Try adjusting your filters</p>
          </div>
        ) : (
          <table className="report-table">
            <thead>
              <tr>
                <th>Teacher Name</th>
                <th className="amount">Base Salary</th>
                <th className="amount">Bonus</th>
                <th className="amount">Deduction</th>
                <th className="amount highlighted">Net Salary</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {salaryComparison.map((salary) => (
                <tr key={salary.teacherId}>
                  <td className="teacher-name">{salary.teacherName}</td>
                  <td className="amount">₹{salary.baseSalary.toFixed(2)}</td>
                  <td className="amount positive">+₹{salary.bonus.toFixed(2)}</td>
                  <td className="amount negative">-₹{salary.deductions.toFixed(2)}</td>
                  <td className="amount highlighted">
                    <strong>₹{salary.netSalary.toFixed(2)}</strong>
                  </td>
                  <td>
                    <span className={`status-badge ${getStatusBadgeClass(salary.status)}`}>
                      {salary.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
            <tfoot>
              <tr className="summary-row">
                <td className="teacher-name">
                  <strong>TOTAL / AVERAGE</strong>
                </td>
                <td className="amount">-</td>
                <td className="amount positive">
                  <strong>₹{statistics.totalBonus.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</strong>
                </td>
                <td className="amount negative">
                  <strong>₹{statistics.totalDeduction.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</strong>
                </td>
                <td className="amount highlighted">
                  <strong>₹{statistics.totalSalaries.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</strong>
                </td>
                <td>-</td>
                <td>-</td>
              </tr>
            </tfoot>
          </table>
        )}
      </div>

      {/* Summary Footer */}
      {salaryComparison.length > 0 && (
        <div className="report-footer">
          <p>Showing {salaryComparison.length} teacher records</p>
          <div className="footer-stats">
            <p>
              <strong>Highest Salary: </strong>₹{statistics.highestSalary.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
            </p>
            <p>
              <strong>Lowest Salary: </strong>₹{statistics.lowestSalary.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

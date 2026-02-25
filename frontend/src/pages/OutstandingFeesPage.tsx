import React, { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  AlertCircle,
  Calendar,
  Filter,
  Download,
  RefreshCw,
  TrendingUp,
} from 'lucide-react';
import { reportApi, type OutstandingFeeDto } from '../services/api';
import './ReportPages.css';

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
    staleTime: 5 * 60 * 1000, // 5 minutes
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
    };

    outstandingFees.forEach((fee) => {
      stats.totalDue += fee.dueAmount;
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
    // CSV export
    const headers = ['Student Name', 'Enrollment #', 'Class', 'Due Amount', 'Days Overdue', 'Aging Bucket', 'Last Payment'];
    const rows = outstandingFees.map((fee) => [
      fee.studentName,
      fee.enrollmentNumber,
      fee.className,
      fee.dueAmount.toFixed(2),
      fee.daysOverdue,
      fee.agingBucket,
      fee.lastPaymentDate || 'N/A',
    ]);

    const csv = [headers, ...rows].map((row) => row.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `outstanding-fees-${filters.asOfDate}.csv`;
    a.click();
  };

  return (
    <div className="report-page">
      <div className="report-header">
        <div>
          <h1>Outstanding Fees Report</h1>
          <p>Track overdue student fees and collection priorities</p>
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
            disabled={outstandingFees.length === 0}
            className="btn-icon"
            title="Export to CSV"
          >
            <Download size={20} />
          </button>
        </div>
      </div>

      {/* Summary Statistics */}
      <div className="stats-grid">
        <div className="stat-card total">
          <div className="stat-icon">
            <TrendingUp size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Total Outstanding Amount</p>
            <p className="stat-value">₹{statistics.totalDue.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</p>
          </div>
        </div>

        <div className="stat-card critical">
          <div className="stat-icon">
            <AlertCircle size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">90+ Days Overdue</p>
            <p className="stat-value">{statistics.overdue90Plus}</p>
            <p className="stat-subtitle">Requires immediate action</p>
          </div>
        </div>

        <div className="stat-card warning">
          <div className="stat-icon">
            <Calendar size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">61-90 Days Overdue</p>
            <p className="stat-value">{statistics.overdue61_90}</p>
          </div>
        </div>

        <div className="stat-card info">
          <div className="stat-icon">
            <Calendar size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">0-30 Days Overdue</p>
            <p className="stat-value">{statistics.overdue0_30}</p>
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
            <label>As Of Date</label>
            <input
              type="date"
              value={filters.asOfDate}
              onChange={(e) => handleFilterChange('asOfDate', e.target.value)}
              className="filter-input"
            />
          </div>

          <div className="filter-group">
            <label>Aging Bucket</label>
            <select
              value={filters.agingBucket}
              onChange={(e) => handleFilterChange('agingBucket', e.target.value)}
              className="filter-input"
            >
              <option value="">All Periods</option>
              <option value="0-30">0-30 Days</option>
              <option value="31-60">31-60 Days</option>
              <option value="61-90">61-90 Days</option>
              <option value="90+">90+ Days</option>
            </select>
          </div>

          <div className="filter-group">
            <label>Minimum Amount (₹)</label>
            <input
              type="number"
              value={filters.minAmount}
              onChange={(e) => handleFilterChange('minAmount', e.target.value)}
              placeholder="0"
              className="filter-input"
            />
          </div>

          <div className="filter-group">
            <label>Sort By</label>
            <select
              value={filters.sortBy}
              onChange={(e) => handleFilterChange('sortBy', e.target.value)}
              className="filter-input"
            >
              <option value="daysoverdue">Days Overdue</option>
              <option value="dueamount">Due Amount</option>
              <option value="name">Student Name</option>
              <option value="class">Class</option>
            </select>
          </div>

          <div className="filter-group">
            <label>Sort Order</label>
            <select
              value={filters.descending ? 'desc' : 'asc'}
              onChange={(e) => handleFilterChange('descending', e.target.value === 'desc')}
              className="filter-input"
            >
              <option value="desc">Descending</option>
              <option value="asc">Ascending</option>
            </select>
          </div>

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
            <p>Loading outstanding fees...</p>
          </div>
        ) : outstandingFees.length === 0 ? (
          <div className="empty-state">
            <AlertCircle size={48} />
            <h3>No outstanding fees found</h3>
            <p>All fees are up to date!</p>
          </div>
        ) : (
          <table className="report-table">
            <thead>
              <tr>
                <th>Student Name</th>
                <th>Enrollment #</th>
                <th>Class</th>
                <th>Due Amount</th>
                <th>Days Overdue</th>
                <th>Aging Bucket</th>
                <th>Last Payment Date</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {outstandingFees.map((fee) => (
                <tr key={`${fee.studentId}-${fee.dueAmount}`} className={`aging-${fee.agingBucket.replace('+', 'plus')}`}>
                  <td className="student-name">{fee.studentName}</td>
                  <td>{fee.enrollmentNumber}</td>
                  <td>{fee.className}</td>
                  <td className="amount">₹{fee.dueAmount.toFixed(2)}</td>
                  <td>
                    <span className={`badge aging-badge-${fee.agingBucket.replace('+', 'plus')}`}>
                      {fee.daysOverdue} days
                    </span>
                  </td>
                  <td>
                    <span className={`aging-label ${fee.agingBucket.replace('+', 'plus')}`}>
                      {fee.agingBucket} days
                    </span>
                  </td>
                  <td>{fee.lastPaymentDate ? new Date(fee.lastPaymentDate).toLocaleDateString() : 'N/A'}</td>
                  <td>
                    <button className="btn-action">Send Reminder</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Summary Footer */}
      {outstandingFees.length > 0 && (
        <div className="report-footer">
          <p>Showing {outstandingFees.length} records</p>
          <p className="total-amount">
            <strong>Total Outstanding: ₹{statistics.totalDue.toLocaleString('en-IN', { maximumFractionDigits: 2 })}</strong>
          </p>
        </div>
      )}
    </div>
  );
};

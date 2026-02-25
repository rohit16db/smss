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
import { reportApi, type BudgetVsActualDto } from '../services/api';
import './ReportPages.css';

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
          <p>{error instanceof Error ? error.message : 'Failed to load budget vs actual data'}</p>
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
          <h1>Budget vs Actual Report</h1>
          <p>Monitor {reportTypeLabel.toLowerCase()} variance analysis</p>
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
            disabled={budgetVsActual.length === 0}
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
            <Calendar size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Total Budgeted</p>
            <p className="stat-value">
              ₹{statistics.totalBudgeted.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
            </p>
          </div>
        </div>

        <div className="stat-card">
          <div className="stat-icon">
            <Calendar size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Total Actual</p>
            <p className="stat-value">
              ₹{statistics.totalActual.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
            </p>
          </div>
        </div>

        <div className={`stat-card ${statistics.totalVariance > 0 ? 'critical' : 'success'}`}>
          <div className="stat-icon">
            <AlertTriangle size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Total Variance</p>
            <p className="stat-value">
              ₹{Math.abs(statistics.totalVariance).toLocaleString('en-IN', { maximumFractionDigits: 2 })}
            </p>
            <p className="stat-subtitle">
              {statistics.totalVariance > 0 ? '↑ Over Budget' : '↓ Under Budget'} ({Math.abs(
                statistics.totalVariancePercentage
              ).toFixed(2)}%)
            </p>
          </div>
        </div>

        <div className="stat-card warning">
          <div className="stat-icon">
            <TrendingUp size={24} />
          </div>
          <div className="stat-content">
            <p className="stat-label">Over Budget</p>
            <p className="stat-value">{statistics.overBudgetCount}</p>
            <p className="stat-subtitle">periods</p>
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
            <label>Report Type</label>
            <select
              value={filters.reportType}
              onChange={(e) => handleFilterChange('reportType', e.target.value)}
              className="filter-input"
            >
              <option value="FeeCollection">Fee Collection</option>
              <option value="SalaryExpense">Salary Expense</option>
            </select>
          </div>

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
            <label>Group By</label>
            <select
              value={filters.groupBy}
              onChange={(e) => handleFilterChange('groupBy', e.target.value)}
              className="filter-input"
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
            <p>Loading budget vs actual data...</p>
          </div>
        ) : budgetVsActual.length === 0 ? (
          <div className="empty-state">
            <AlertTriangle size={48} />
            <h3>No data found</h3>
            <p>Try adjusting your filters</p>
          </div>
        ) : (
          <table className="report-table budget-table">
            <thead>
              <tr>
                <th>Period</th>
                <th className="amount">Budgeted Amount</th>
                <th className="amount">Actual Amount</th>
                <th className="amount variance">Variance</th>
                <th className="percentage">Variance %</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {budgetVsActual.map((item, index) => (
                <tr key={`${item.month}-${index}`} className={`variance-${getVarianceColor(item.variance)}`}>
                  <td className="period">{item.month}</td>
                  <td className="amount budgeted">
                    ₹{item.budgetedAmount.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                  </td>
                  <td className="amount actual">
                    ₹{item.actualAmount.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                  </td>
                  <td className={`amount variance ${item.variance > 0 ? 'positive' : 'negative'}`}>
                    {item.variance > 0 ? '+' : '-'}₹
                    {Math.abs(item.variance).toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                  </td>
                  <td className="percentage">
                    <span className={`variance-badge ${getVarianceColor(item.variance)}`}>
                      {item.variancePercentage > 0 ? '+' : ''}
                      {item.variancePercentage.toFixed(2)}%
                    </span>
                  </td>
                  <td>
                    <div className={`variance-status ${getVarianceColor(item.variance)}`}>
                      {getVarianceIcon(item.variance)}
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
            <tfoot>
              <tr className="summary-row">
                <td className="period">
                  <strong>TOTAL</strong>
                </td>
                <td className="amount">
                  <strong>
                    ₹{statistics.totalBudgeted.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                  </strong>
                </td>
                <td className="amount">
                  <strong>
                    ₹{statistics.totalActual.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                  </strong>
                </td>
                <td className={`amount variance ${statistics.totalVariance > 0 ? 'positive' : 'negative'}`}>
                  <strong>
                    {statistics.totalVariance > 0 ? '+' : '-'}₹
                    {Math.abs(statistics.totalVariance).toLocaleString('en-IN', {
                      maximumFractionDigits: 2,
                    })}
                  </strong>
                </td>
                <td className="percentage">
                  <strong>{statistics.totalVariancePercentage.toFixed(2)}%</strong>
                </td>
                <td>-</td>
              </tr>
            </tfoot>
          </table>
        )}
      </div>

      {/* Legend */}
      <div className="report-legend">
        <h3>Legend</h3>
        <div className="legend-items">
          <div className="legend-item">
            <span className="legend-color over-budget"></span>
            <span>Over Budget (Negative variance)</span>
          </div>
          <div className="legend-item">
            <span className="legend-color under-budget"></span>
            <span>Under Budget (Positive variance)</span>
          </div>
          <div className="legend-item">
            <span className="legend-color neutral"></span>
            <span>On Track (No variance)</span>
          </div>
        </div>
      </div>

      {/* Summary Footer */}
      {budgetVsActual.length > 0 && (
        <div className="report-footer">
          <p>Showing {budgetVsActual.length} periods</p>
          <p>
            <strong>Average Variance: </strong>
            {statistics.avgVariancePercentage.toFixed(2)}%
          </p>
        </div>
      )}
    </div>
  );
};

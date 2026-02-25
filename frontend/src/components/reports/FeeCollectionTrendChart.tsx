import React from 'react';
import {
  Line,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  ComposedChart,
} from 'recharts';
import type { MonthlyCollectionTrendDto } from '../../types/reports';

interface FeeCollectionTrendChartProps {
  data: MonthlyCollectionTrendDto[];
}

/**
 * Chart component displaying monthly fee collection trend
 */
export const FeeCollectionTrendChart: React.FC<FeeCollectionTrendChartProps> = ({ data }) => {
  // Format data for chart
  const chartData = data.map((item) => ({
    ...item,
    month: item.month.substring(5) + '-' + item.month.substring(0, 4), // Convert YYYY-MM to MM-YYYY
  }));

  return (
    <ResponsiveContainer width="100%" height={300}>
      <ComposedChart data={chartData} margin={{ top: 20, right: 30, left: 0, bottom: 20 }}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="month" tick={{ fontSize: 12 }} />
        <YAxis yAxisId="left" tick={{ fontSize: 12 }} />
        <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 12 }} />
        <Tooltip
          contentStyle={{
            backgroundColor: '#fff',
            border: '1px solid #ccc',
            borderRadius: '4px',
          }}
          formatter={(value: any) => {
            if (typeof value === 'number') {
              if (value > 100) {
                return `₹${value.toLocaleString('en-IN')}`;
              } else {
                return `${value.toFixed(1)}%`;
              }
            }
            return value;
          }}
        />
        <Legend />

        {/* Bars for collected and pending */}
        <Bar yAxisId="left" dataKey="collected" fill="#10b981" name="Collected" />
        <Bar yAxisId="left" dataKey="pending" fill="#f59e0b" name="Pending" />
        <Bar yAxisId="left" dataKey="overdue" fill="#ef4444" name="Overdue" />

        {/* Line for collection rate */}
        <Line
          yAxisId="right"
          type="monotone"
          dataKey="collectionRate"
          stroke="#3b82f6"
          strokeWidth={2}
          name="Collection Rate %"
          dot={{ fill: '#3b82f6', r: 4 }}
        />
      </ComposedChart>
    </ResponsiveContainer>
  );
};

import React from 'react';
import {
  ComposedChart,
  Bar,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import type { MonthlySalaryTrendDto } from '../../types/reports';

interface MonthlySalaryTrendChartProps {
  data: MonthlySalaryTrendDto[];
}

/**
 * Chart component displaying monthly salary trend
 */
export const MonthlySalaryTrendChart: React.FC<MonthlySalaryTrendChartProps> = ({
  data,
}) => {
  // Format data for chart
  const chartData = data.map((item) => ({
    ...item,
    month: item.month.substring(5) + '-' + item.month.substring(0, 4),
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
              return `₹${value.toLocaleString('en-IN')}`;
            }
            return value;
          }}
        />
        <Legend />

        {/* Bars for salary components */}
        <Bar yAxisId="left" dataKey="totalBaseSalary" fill="#3b82f6" name="Base Salary" />
        <Bar yAxisId="left" dataKey="totalBonus" fill="#10b981" name="Bonus" />
        <Bar yAxisId="left" dataKey="totalDeductions" fill="#ef4444" name="Deductions" />

        {/* Line for average salary */}
        <Line
          yAxisId="right"
          type="monotone"
          dataKey="averageSalary"
          stroke="#f59e0b"
          strokeWidth={2}
          name="Average Salary"
          dot={{ fill: '#f59e0b', r: 4 }}
        />
      </ComposedChart>
    </ResponsiveContainer>
  );
};

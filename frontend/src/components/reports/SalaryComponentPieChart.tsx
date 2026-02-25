import React from 'react';
import { PieChart, Pie, Cell, Legend, Tooltip, ResponsiveContainer } from 'recharts';
import type { SalaryComponentBreakdownDto } from '../../types/reports';

interface SalaryComponentPieChartProps {
  data: SalaryComponentBreakdownDto;
}

/**
 * Pie chart displaying salary component breakdown
 */
export const SalaryComponentPieChart: React.FC<SalaryComponentPieChartProps> = ({
  data,
}) => {
  // Prepare data for pie chart
  const pieData = [
    {
      name: 'Base Salary',
      value: data.basePercentage,
      amount: data.baseSalary,
    },
    {
      name: 'Bonus',
      value: data.bonusPercentage,
      amount: data.bonus,
    },
    {
      name: 'Deductions',
      value: data.deductionsPercentage,
      amount: data.deductions,
    },
  ];

  const COLORS = ['#3b82f6', '#10b981', '#ef4444'];

  return (
    <div className="space-y-6">
      {/* Pie Chart */}
      <div className="h-64">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={pieData}
              cx="50%"
              cy="50%"
              labelLine={false}
              label={({ name, value }) => `${name} ${value.toFixed(1)}%`}
              outerRadius={80}
              fill="#8884d8"
              dataKey="value"
            >
              {pieData.map((_, index) => (
                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
              ))}
            </Pie>
            <Tooltip formatter={(value: any) => `${value.toFixed(1)}%`} />
            <Legend />
          </PieChart>
        </ResponsiveContainer>
      </div>

      {/* Summary Table */}
      <div className="space-y-3">
        {pieData.map((item, index) => (
          <div key={item.name} className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div
                className="w-4 h-4 rounded"
                style={{ backgroundColor: COLORS[index] }}
              ></div>
              <span className="font-medium text-gray-700">{item.name}</span>
            </div>
            <div className="text-right">
              <p className="font-semibold text-gray-900">
                ₹{item.amount.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
              </p>
              <p className="text-sm text-gray-500">{item.value.toFixed(1)}%</p>
            </div>
          </div>
        ))}

        <div className="border-t pt-3 flex items-center justify-between">
          <span className="font-semibold text-gray-700">Net Salary</span>
          <p className="font-bold text-lg text-gray-900">
            ₹{data.netSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
          </p>
        </div>
      </div>

      {/* Record Count */}
      <p className="text-sm text-gray-500 text-center">
        Based on {data.recordCount} salary records
      </p>
    </div>
  );
};

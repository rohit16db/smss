import React from 'react';
import {
  PieChart,
  Pie,
  Cell,
  Legend,
  Tooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from 'recharts';
import type { FeeCollectionByCategoryDto } from '../../types/reports';

interface FeeCategoryBreakdownChartProps {
  data: FeeCollectionByCategoryDto[];
}

const COLORS = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6'];

/**
 * Chart component displaying fee collection by category
 */
export const FeeCategoryBreakdownChart: React.FC<FeeCategoryBreakdownChartProps> = ({
  data,
}) => {
  // Create data for pie chart (percentage of total)
  const pieData = data.map((item) => ({
    name: item.category,
    value: item.percentageOfTotal,
  }));

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

      {/* Bar Chart - Collection amounts by category */}
      <div className="h-64">
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data} margin={{ top: 20, right: 30, left: 0, bottom: 20 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="category" tick={{ fontSize: 12 }} angle={-45} textAnchor="end" height={80} />
            <YAxis tick={{ fontSize: 12 }} />
            <Tooltip
              formatter={(value: any) => `₹${value.toLocaleString('en-IN')}`}
              contentStyle={{
                backgroundColor: '#fff',
                border: '1px solid #ccc',
                borderRadius: '4px',
              }}
            />
            <Legend />
            <Bar dataKey="collected" fill="#10b981" name="Collected" />
            <Bar dataKey="pending" fill="#f59e0b" name="Pending" />
            <Bar dataKey="overdue" fill="#ef4444" name="Overdue" />
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* Category Summary Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-700">Category</th>
              <th className="px-4 py-3 text-right font-medium text-gray-700">Collected</th>
              <th className="px-4 py-3 text-right font-medium text-gray-700">Pending</th>
              <th className="px-4 py-3 text-right font-medium text-gray-700">Overdue</th>
              <th className="px-4 py-3 text-right font-medium text-gray-700">Collection %</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {data.map((category) => (
              <tr key={category.category} className="hover:bg-gray-50">
                <td className="px-4 py-3 font-medium">{category.category}</td>
                <td className="px-4 py-3 text-right text-green-600 font-medium">
                  ₹{category.collected.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                </td>
                <td className="px-4 py-3 text-right text-orange-600">
                  ₹{category.pending.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                </td>
                <td className="px-4 py-3 text-right text-red-600">
                  ₹{category.overdue.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                </td>
                <td className="px-4 py-3 text-right">
                  <span
                    className={`inline-block px-2 py-1 rounded text-white font-medium ${
                      category.collectionPercentage >= 75
                        ? 'bg-green-500'
                        : category.collectionPercentage >= 50
                        ? 'bg-orange-500'
                        : 'bg-red-500'
                    }`}
                  >
                    {category.collectionPercentage.toFixed(1)}%
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

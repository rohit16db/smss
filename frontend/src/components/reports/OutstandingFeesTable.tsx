import React, { useMemo } from 'react';
import { AlertCircle, Phone } from 'lucide-react';
import type { OutstandingFeeDto } from '../../types/reports';

interface OutstandingFeesTableProps {
  data: OutstandingFeeDto[];
}

/**
 * Table component displaying outstanding and overdue fees with aging analysis
 */
export const OutstandingFeesTable: React.FC<OutstandingFeesTableProps> = ({ data }) => {
  // Group by aging bucket
  const groupedData = useMemo(() => {
    const groups: Record<string, OutstandingFeeDto[]> = {
      '0-30': [],
      '31-60': [],
      '61-90': [],
      '90+': [],
    };

    data.forEach((fee) => {
      if (groups[fee.agingBucket]) {
        groups[fee.agingBucket].push(fee);
      }
    });

    return groups;
  }, [data]);

  // Calculate aging summary
  const agingSummary = Object.entries(groupedData).map(([bucket, fees]) => ({
    bucket,
    count: fees.length,
    total: fees.reduce((sum, fee) => sum + fee.dueAmount, 0),
  }));

  const getAgingBucketBadge = (bucket: string): string => {
    switch (bucket) {
      case '0-30':
        return 'bg-yellow-100 text-yellow-800';
      case '31-60':
        return 'bg-orange-100 text-orange-800';
      case '61-90':
        return 'bg-red-100 text-red-800';
      case '90+':
        return 'bg-red-200 text-red-900';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  if (data.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        <AlertCircle className="w-12 h-12 mx-auto mb-2 text-gray-400" />
        <p>No outstanding fees found for the selected criteria</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Aging Summary */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {agingSummary.map((summary) => (
          <div key={summary.bucket} className={`rounded-lg p-4 border ${getAgingBucketStatus(summary.bucket).bg}`}>
            <p className="text-sm text-gray-600 mb-1">{summary.bucket} Days</p>
            <p className="text-2xl font-bold text-gray-900">{summary.count}</p>
            <p className="text-xs text-gray-500 mt-1">
              ₹{summary.total.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
            </p>
          </div>
        ))}
      </div>

      {/* Detailed Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-700">Student</th>
              <th className="px-4 py-3 text-left font-medium text-gray-700">Class/Section</th>
              <th className="px-4 py-3 text-right font-medium text-gray-700">Due Amount</th>
              <th className="px-4 py-3 text-center font-medium text-gray-700">Days Overdue</th>
              <th className="px-4 py-3 text-center font-medium text-gray-700">Aging Bucket</th>
              <th className="px-4 py-3 text-left font-medium text-gray-700">Due Date</th>
              <th className="px-4 py-3 text-left font-medium text-gray-700">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {data.map((fee, index) => (
              <tr
                key={index}
                className={`hover:bg-gray-50 border-l-4 ${getAgingBucketBorder(fee.agingBucket)}`}
              >
                <td className="px-4 py-3">
                  <div className="font-medium text-gray-900">{fee.studentInfo}</div>
                  {fee.contactInfo && (
                    <div className="flex items-center gap-1 text-xs text-gray-500 mt-1">
                      <Phone className="w-3 h-3" />
                      {fee.contactInfo}
                    </div>
                  )}
                </td>
                <td className="px-4 py-3 text-gray-700">{fee.classSection}</td>
                <td className="px-4 py-3 text-right font-medium text-gray-900">
                  ₹{fee.dueAmount.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                </td>
                <td className="px-4 py-3 text-center">
                  <span className="text-red-600 font-medium">{fee.daysOverdue} days</span>
                </td>
                <td className="px-4 py-3 text-center">
                  <span className={`inline-block px-3 py-1 rounded-full text-xs font-medium ${getAgingBucketBadge(fee.agingBucket)}`}>
                    {fee.agingBucket}
                  </span>
                </td>
                <td className="px-4 py-3 text-gray-700">
                  {new Date(fee.dueDate).toLocaleDateString('en-IN')}
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-block px-2 py-1 rounded text-xs font-medium ${
                      fee.isActive
                        ? 'bg-green-100 text-green-800'
                        : 'bg-gray-100 text-gray-800'
                    }`}
                  >
                    {fee.isActive ? 'Active' : 'Inactive'}
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

/**
 * Helper function to get aging bucket status styling
 */
function getAgingBucketStatus(bucket: string) {
  switch (bucket) {
    case '0-30':
      return { bg: 'bg-yellow-50 border border-yellow-200', icon: 'text-yellow-600' };
    case '31-60':
      return { bg: 'bg-orange-50 border border-orange-200', icon: 'text-orange-600' };
    case '61-90':
      return { bg: 'bg-red-50 border border-red-200', icon: 'text-red-600' };
    case '90+':
      return { bg: 'bg-red-100 border border-red-300', icon: 'text-red-700' };
    default:
      return { bg: 'bg-gray-50 border border-gray-200', icon: 'text-gray-600' };
  }
}

/**
 * Helper function to get border color for aging bucket
 */
function getAgingBucketBorder(bucket: string): string {
  switch (bucket) {
    case '0-30':
      return 'border-yellow-400';
    case '31-60':
      return 'border-orange-400';
    case '61-90':
      return 'border-red-400';
    case '90+':
      return 'border-red-600';
    default:
      return 'border-gray-300';
  }
}

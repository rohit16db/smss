import React from 'react';
import type { FinancialSummary } from '../../types/dashboard';

interface FeesCollectionChartProps {
  data: FinancialSummary;
  isLoading?: boolean;
}

export const FeesCollectionChart: React.FC<FeesCollectionChartProps> = ({ data, isLoading = false }) => {
  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-6 animate-pulse">
        <div className="h-6 bg-gray-200 rounded w-1/3 mb-6"></div>
        <div className="h-64 bg-gray-200 rounded"></div>
      </div>
    );
  }

  const collectionPercentage = data.collectionPercentage;
  const outstandingPercentage = 100 - collectionPercentage;

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      {/* Header */}
      <h3 className="text-lg font-semibold text-gray-900 mb-6">Fee Collection Status</h3>

      {/* Chart Container */}
      <div className="space-y-6">
        {/* Collected vs Outstanding */}
        <div className="space-y-4">
          {/* Collected */}
          <div>
            <div className="flex justify-between items-center mb-2">
              <span className="text-sm font-medium text-gray-700">Collected</span>
              <span className="text-sm font-semibold text-green-600">
                ₹{data.totalFeesCollected.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-3">
              <div
                className="bg-green-500 h-3 rounded-full transition-all duration-300"
                style={{ width: `${collectionPercentage}%` }}
              ></div>
            </div>
            <span className="text-xs text-gray-500 mt-1">{collectionPercentage.toFixed(1)}% collected</span>
          </div>

          {/* Outstanding */}
          <div>
            <div className="flex justify-between items-center mb-2">
              <span className="text-sm font-medium text-gray-700">Outstanding</span>
              <span className="text-sm font-semibold text-red-600">
                ₹{data.totalOutstandingFees.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
              </span>
            </div>
            <div className="w-full bg-gray-200 rounded-full h-3">
              <div
                className="bg-red-500 h-3 rounded-full transition-all duration-300"
                style={{ width: `${outstandingPercentage}%` }}
              ></div>
            </div>
            <span className="text-xs text-gray-500 mt-1">{outstandingPercentage.toFixed(1)}% outstanding</span>
          </div>
        </div>

        {/* Statistics Grid */}
        <div className="grid grid-cols-2 gap-4 pt-4 border-t border-gray-200">
          <div>
            <p className="text-xs text-gray-600 font-medium mb-1">Total Expected</p>
            <p className="text-xl font-bold text-gray-900">
              ₹{data.totalExpectedFees.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
            </p>
          </div>
          <div>
            <p className="text-xs text-gray-600 font-medium mb-1">Avg per Student</p>
            <p className="text-xl font-bold text-gray-900">
              ₹{data.averagePaymentPerStudent.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
            </p>
          </div>
          <div>
            <p className="text-xs text-gray-600 font-medium mb-1">Total Students</p>
            <p className="text-xl font-bold text-gray-900">{data.totalStudents}</p>
          </div>
          <div>
            <p className="text-xs text-gray-600 font-medium mb-1">Collection Rate</p>
            <p className={`text-xl font-bold ${collectionPercentage >= 80 ? 'text-green-600' : 'text-orange-600'}`}>
              {collectionPercentage.toFixed(1)}%
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

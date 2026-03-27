import React from 'react';
import { AlertTriangle, AlertCircle, CheckCircle } from 'lucide-react';
import type { AttendanceToSalaryCorrelationDto } from '../../types/reports';

interface AttendanceCorrelationTableProps {
  data: AttendanceToSalaryCorrelationDto[];
}

/**
 * Table component for attendance to salary correlation analysis
 */
export const AttendanceCorrelationTable: React.FC<AttendanceCorrelationTableProps> = ({
  data,
}) => {
  if (data.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        <AlertCircle className="w-12 h-12 mx-auto mb-2 text-gray-400" />
        <p>No correlation data found for the selected period</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Summary Statistics */}
      <div className="grid grid-cols-3 gap-4">
        <div className="bg-blue-50 rounded-lg p-4 border border-blue-200">
          <p className="text-sm text-gray-700 mb-1">Total Staffs</p>
          <p className="text-2xl font-bold text-blue-900">{data.length}</p>
        </div>
        <div className="bg-green-50 rounded-lg p-4 border border-green-200">
          <p className="text-sm text-gray-700 mb-1">Bonus Eligible (≥90%)</p>
          <p className="text-2xl font-bold text-green-900">
            {data.filter((d) => d.bonusEligible).length}
          </p>
        </div>
        <div className="bg-red-50 rounded-lg p-4 border border-red-200">
          <p className="text-sm text-gray-700 mb-1">Discrepancies</p>
          <p className="text-2xl font-bold text-red-900">
            {data.filter((d) => d.hasDiscrepancy).length}
          </p>
        </div>
      </div>

      {/* Detailed Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-700">Staff Name</th>
              <th className="px-4 py-3 text-center font-medium text-gray-700">Attendance %</th>
              <th className="px-4 py-3 text-center font-medium text-gray-700">Present Days</th>
              <th className="px-4 py-3 text-center font-medium text-gray-700">Absent Days</th>
              <th className="px-4 py-3 text-right font-medium text-gray-700">Calc. Deduction</th>
              <th className="px-4 py-3 text-right font-medium text-gray-700">Actual Deduction</th>
              <th className="px-4 py-3 text-center font-medium text-gray-700">Bonus</th>
              <th className="px-4 py-3 text-center font-medium text-gray-700">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {data.map((record) => (
              <tr
                key={record.StaffId}
                className={`hover:bg-gray-50 ${
                  record.hasDiscrepancy ? 'bg-yellow-50' : ''
                }`}
              >
                <td className="px-4 py-3">
                  <p className="font-medium text-gray-900">{record.StaffName}</p>
                  <p className="text-xs text-gray-500">Base: ₹{record.baseSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 })}</p>
                </td>
                <td className="px-4 py-3 text-center">
                  <span
                    className={`inline-block px-2 py-1 rounded text-xs font-medium ${
                      record.attendancePercentage >= 90
                        ? 'bg-green-100 text-green-800'
                        : record.attendancePercentage >= 75
                        ? 'bg-blue-100 text-blue-800'
                        : 'bg-red-100 text-red-800'
                    }`}
                  >
                    {record.attendancePercentage.toFixed(1)}%
                  </span>
                </td>
                <td className="px-4 py-3 text-center text-gray-900">
                  {record.presentDays}/{record.totalDays}
                </td>
                <td className="px-4 py-3 text-center text-red-600 font-medium">
                  {record.absentDays}
                </td>
                <td className="px-4 py-3 text-right">
                  <p className="text-gray-700">
                    ₹{record.calculatedDeduction.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                  </p>
                </td>
                <td className="px-4 py-3 text-right">
                  <div className="flex items-center justify-end gap-2">
                    <p className="text-gray-700">
                      ₹{record.actualDeduction.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                    </p>
                    {record.hasDiscrepancy && (
                      <AlertTriangle className="w-4 h-4 text-yellow-600" />
                    )}
                  </div>
                </td>
                <td className="px-4 py-3 text-center">
                  {record.bonusEligible ? (
                    <div>
                      <CheckCircle className="w-5 h-5 text-green-600 mx-auto" />
                      <p className="text-xs text-green-600 mt-1">
                        ₹{record.bonusAmount.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                      </p>
                    </div>
                  ) : (
                    <span className="text-gray-400 text-xs">N/A</span>
                  )}
                </td>
                <td className="px-4 py-3 text-center">
                  <div className="flex flex-col items-center gap-1">
                    {record.hasDiscrepancy ? (
                      <>
                        <AlertTriangle className="w-5 h-5 text-yellow-600" />
                        <span className="text-xs text-yellow-700 font-medium">Discrepancy</span>
                      </>
                    ) : (
                      <>
                        <CheckCircle className="w-5 h-5 text-green-600" />
                        <span className="text-xs text-green-700 font-medium">Match</span>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Legend */}
      <div className="bg-gray-50 rounded-lg p-4 space-y-2">
        <p className="font-medium text-gray-700 mb-3">Legend:</p>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 bg-green-400 rounded-full"></div>
            <span className="text-sm text-gray-600">Attendance ≥ 90% (Bonus Eligible)</span>
          </div>
          <div className="flex items-center gap-2">
            <AlertTriangle className="w-4 h-4 text-yellow-600" />
            <span className="text-sm text-gray-600">Deduction Discrepancy Detected</span>
          </div>
          <div className="flex items-center gap-2">
            <CheckCircle className="w-4 h-4 text-green-600" />
            <span className="text-sm text-gray-600">Deduction Matches Policy</span>
          </div>
        </div>
      </div>
    </div>
  );
};

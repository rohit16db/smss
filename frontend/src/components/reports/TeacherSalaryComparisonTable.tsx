import React from 'react';
import { Award, AlertCircle } from 'lucide-react';
import type { TeacherSalaryComparisonDto } from '../../types/reports';

interface TeacherSalaryComparisonTableProps {
  data: TeacherSalaryComparisonDto[];
}

/**
 * Table component for teacher-wise salary comparison
 */
export const TeacherSalaryComparisonTable: React.FC<
  TeacherSalaryComparisonTableProps
> = ({ data }) => {
  if (data.length === 0) {
    return (
      <div className="text-center py-8 text-gray-500">
        <AlertCircle className="w-12 h-12 mx-auto mb-2 text-gray-400" />
        <p>No salary data found for the selected period</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead className="bg-gray-50 border-b">
          <tr>
            <th className="px-4 py-3 text-left font-medium text-gray-700">Teacher Name</th>
            <th className="px-4 py-3 text-right font-medium text-gray-700">Base Salary</th>
            <th className="px-4 py-3 text-right font-medium text-gray-700">Bonus</th>
            <th className="px-4 py-3 text-right font-medium text-gray-700">Deductions</th>
            <th className="px-4 py-3 text-right font-medium text-gray-700">Net Salary</th>
            <th className="px-4 py-3 text-center font-medium text-gray-700">Bonus</th>
            <th className="px-4 py-3 text-center font-medium text-gray-700">Status</th>
          </tr>
        </thead>
        <tbody className="divide-y">
          {data.map((teacher) => (
            <tr
              key={teacher.teacherId}
              className="hover:bg-gray-50"
            >
              <td className="px-4 py-3">
                <p className="font-medium text-gray-900">{teacher.teacherName}</p>
                {teacher.attendancePercentage !== undefined && (
                  <p className="text-xs text-gray-500 mt-1">
                    Attendance: {teacher.attendancePercentage.toFixed(1)}%
                  </p>
                )}
              </td>
              <td className="px-4 py-3 text-right text-gray-900">
                ₹{teacher.baseSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
              </td>
              <td className="px-4 py-3 text-right">
                <span className={teacher.bonus > 0 ? 'text-green-600 font-medium' : 'text-gray-500'}>
                  ₹{teacher.bonus.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                </span>
              </td>
              <td className="px-4 py-3 text-right text-red-600">
                ₹{teacher.deductions.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
              </td>
              <td className="px-4 py-3 text-right">
                <p className="font-semibold text-gray-900">
                  ₹{teacher.netSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                </p>
              </td>
              <td className="px-4 py-3 text-center">
                {teacher.bonusEligible ? (
                  <Award className="w-5 h-5 text-yellow-500 mx-auto" />
                ) : (
                  <span className="text-gray-400">-</span>
                )}
              </td>
              <td className="px-4 py-3 text-center">
                <span
                  className={`inline-block px-2 py-1 rounded text-xs font-medium ${
                    teacher.status === 'Paid'
                      ? 'bg-green-100 text-green-800'
                      : teacher.status === 'Approved'
                      ? 'bg-blue-100 text-blue-800'
                      : 'bg-yellow-100 text-yellow-800'
                  }`}
                >
                  {teacher.status}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

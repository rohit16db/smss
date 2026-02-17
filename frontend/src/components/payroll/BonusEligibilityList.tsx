import React from 'react';
import { CheckCircle, XCircle } from 'lucide-react';
import type { BonusEligibilityDto } from '../../types/payroll';

interface BonusEligibilityListProps {
  bonuses: BonusEligibilityDto[];
  isLoading?: boolean;
}

export const BonusEligibilityList: React.FC<BonusEligibilityListProps> = ({
  bonuses,
  isLoading = false,
}) => {
  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(5)].map((_, i) => (
          <div key={i} className="h-16 bg-gray-200 rounded animate-pulse" />
        ))}
      </div>
    );
  }

  if (bonuses.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-500">No bonus data available</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full">
        <thead>
          <tr className="border-b-2 border-gray-200">
            <th className="text-left py-3 px-4 font-semibold text-gray-900">
              Teacher Name
            </th>
            <th className="text-right py-3 px-4 font-semibold text-gray-900">
              Attendance
            </th>
            <th className="text-right py-3 px-4 font-semibold text-gray-900">
              Bonus %
            </th>
            <th className="text-right py-3 px-4 font-semibold text-gray-900">
              Bonus Amount
            </th>
            <th className="text-center py-3 px-4 font-semibold text-gray-900">
              Status
            </th>
            <th className="text-left py-3 px-4 font-semibold text-gray-900">
              Reason
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-200">
          {bonuses.map((bonus) => (
            <tr key={bonus.teacherId} className="hover:bg-gray-50">
              <td className="py-4 px-4 font-medium text-gray-900">
                {bonus.teacherName}
              </td>
              <td className="py-4 px-4 text-right">
                <span
                  className={`font-medium ${
                    bonus.attendancePercentage >= 90
                      ? 'text-green-600'
                      : bonus.attendancePercentage >= 75
                        ? 'text-blue-600'
                        : 'text-red-600'
                  }`}
                >
                  {bonus.attendancePercentage}%
                </span>
              </td>
              <td className="py-4 px-4 text-right font-medium">
                {bonus.bonusPercentage}%
              </td>
              <td className="py-4 px-4 text-right font-semibold text-gray-900">
                ₹{bonus.bonusAmount.toLocaleString()}
              </td>
              <td className="py-4 px-4 text-center">
                {bonus.isEligible ? (
                  <CheckCircle className="w-5 h-5 text-green-600 mx-auto" />
                ) : (
                  <XCircle className="w-5 h-5 text-red-600 mx-auto" />
                )}
              </td>
              <td className="py-4 px-4 text-sm text-gray-600">
                {bonus.reason}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

import React from 'react';
import { Clock, CheckCircle, AlertCircle, Trash2 } from 'lucide-react';
import type { SalaryPaymentDto } from '../../types/salary';

interface SalaryPaymentCardProps {
  salary: SalaryPaymentDto;
  onMarkAsPaid?: (id: string) => void;
  onDelete?: (id: string) => void;
  isLoading?: boolean;
}

export const SalaryPaymentCard: React.FC<SalaryPaymentCardProps> = ({
  salary,
  onMarkAsPaid,
  onDelete,
  isLoading = false,
}) => {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Paid':
        return 'bg-green-50 border-green-200';
      case 'Approved':
        return 'bg-blue-50 border-blue-200';
      case 'Pending':
        return 'bg-yellow-50 border-yellow-200';
      case 'Cancelled':
        return 'bg-red-50 border-red-200';
      case 'OnHold':
        return 'bg-orange-50 border-orange-200';
      default:
        return 'bg-gray-50 border-gray-200';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Paid':
        return <CheckCircle className="w-5 h-5 text-green-600" />;
      case 'Pending':
      case 'Approved':
        return <Clock className="w-5 h-5 text-yellow-600" />;
      case 'Cancelled':
        return <AlertCircle className="w-5 h-5 text-red-600" />;
      default:
        return <AlertCircle className="w-5 h-5 text-gray-600" />;
    }
  };

  return (
    <div className={`border-2 rounded-lg p-5 ${getStatusColor(salary.status)}`}>
      {/* Header */}
      <div className="flex justify-between items-start mb-3">
        <div>
          <h3 className="text-lg font-semibold text-gray-900">
            {salary.StaffName}
          </h3>
          <p className="text-sm text-gray-600 mt-1">
            {new Date(salary.periodStartDate).toLocaleDateString()} -{' '}
            {new Date(salary.periodEndDate).toLocaleDateString()}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {getStatusIcon(salary.status)}
          <span className="text-sm font-semibold text-gray-700">
            {salary.status}
          </span>
        </div>
      </div>

      {/* Salary Details */}
      <div className="grid grid-cols-2 gap-3 mb-4 p-3 bg-white bg-opacity-60 rounded">
        <div>
          <p className="text-xs text-gray-600">Base Salary</p>
          <p className="text-lg font-bold text-gray-900">
            ₹{salary.baseSalary.toLocaleString()}
          </p>
        </div>
        <div>
          <p className="text-xs text-gray-600">Deductions</p>
          <p className="text-lg font-bold text-red-600">
            -₹{salary.deductions.toLocaleString()}
          </p>
        </div>
        <div>
          <p className="text-xs text-gray-600">Bonus</p>
          <p className="text-lg font-bold text-green-600">
            +₹{salary.bonus.toLocaleString()}
          </p>
        </div>
        <div>
          <p className="text-xs text-gray-600">Net Salary</p>
          <p className="text-lg font-bold text-gray-900">
            ₹{salary.netSalary.toLocaleString()}
          </p>
        </div>
      </div>

      {/* Payment Details */}
      {(salary.paidDate || salary.referenceNumber) && (
        <div className="space-y-1 mb-4 text-sm">
          {salary.paidDate && (
            <p className="text-gray-700">
              <span className="font-medium">Paid Date:</span>{' '}
              {new Date(salary.paidDate).toLocaleDateString()}
            </p>
          )}
          {salary.referenceNumber && (
            <p className="text-gray-700">
              <span className="font-medium">Reference:</span> {salary.referenceNumber}
            </p>
          )}
          {salary.paymentMethod && (
            <p className="text-gray-700">
              <span className="font-medium">Method:</span> {salary.paymentMethod}
            </p>
          )}
        </div>
      )}

      {/* Actions */}
      {salary.status !== 'Paid' && (
        <div className="flex gap-2 pt-3 border-t border-gray-200 border-opacity-50">
          {salary.status !== 'Cancelled' && onMarkAsPaid && (
            <button
              onClick={() => onMarkAsPaid(salary.id)}
              disabled={isLoading}
              className="flex-1 px-3 py-2 bg-green-600 text-white rounded text-sm font-medium hover:bg-green-700 disabled:opacity-50 transition-colors"
            >
              Mark as Paid
            </button>
          )}
          {salary.status !== 'Approved' && salary.status !== 'Pending' && onDelete && (
            <button
              onClick={() => onDelete(salary.id)}
              disabled={isLoading}
              className="px-3 py-2 bg-red-100 text-red-700 rounded text-sm font-medium hover:bg-red-200 disabled:opacity-50 transition-colors"
            >
              <Trash2 className="w-4 h-4" />
            </button>
          )}
        </div>
      )}
    </div>
  );
};

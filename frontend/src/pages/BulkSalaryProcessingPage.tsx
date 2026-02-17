import React, { useState } from 'react';
import {
  useTeachersWithSalaryStructures,
  useBulkCreateSalaryPayments,
} from '../services/salaryStructureService';
import { AlertCircle, CheckCircle } from 'lucide-react';
import type { SalaryPaymentReportDto } from '../types/salary';

export const BulkSalaryProcessingPage: React.FC = () => {
  const { data: teachersWithAssignments, isLoading } = useTeachersWithSalaryStructures(true);
  const bulkCreateMutation = useBulkCreateSalaryPayments();

  const [formData, setFormData] = useState({
    periodStartDate: new Date().toISOString().split('T')[0],
    periodEndDate: new Date().toISOString().split('T')[0],
    fixedDeductions: 0,
  });

  const [result, setResult] = useState<SalaryPaymentReportDto | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.periodStartDate > formData.periodEndDate) {
      alert('Period start date must be before or equal to end date');
      return;
    }

    if (!teachersWithAssignments || teachersWithAssignments.length === 0) {
      alert('No teachers with assigned salary structures found');
      return;
    }

    try {
      const response = await bulkCreateMutation.mutateAsync({
        periodStartDate: formData.periodStartDate,
        periodEndDate: formData.periodEndDate,
        fixedDeductions: formData.fixedDeductions || 0,
      });
      setResult(response);
    } catch (error) {
      console.error('Error creating salary payments:', error);
      alert('Failed to create salary payments');
    }
  };

  const estimatedTotalSalary = (teachersWithAssignments || []).reduce(
    (sum, t) => sum + t.grossSalary,
    0
  );

  const totalDeductions =
    (teachersWithAssignments?.length || 0) * (formData.fixedDeductions || 0);
  const estimatedNetTotal = estimatedTotalSalary - totalDeductions;

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="animate-fadeIn">
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
              Bulk Salary Processing
            </h1>
            <p className="text-gray-600 mt-2">
              Create salary payments for all teachers with assigned salary structures
            </p>
          </div>

          {result ? (
            // Result View
            <div className="space-y-6">
              <div className="bg-gradient-to-r from-green-50 to-emerald-50 border-2 border-green-200 rounded-2xl p-6 shadow-lg">
                <div className="flex items-start gap-4">
                  <CheckCircle className="w-8 h-8 text-green-600 flex-shrink-0 mt-1" />
                  <div>
                    <h2 className="text-2xl font-bold text-green-900">
                      ✓ Salary Payments Created Successfully
                    </h2>
                    <p className="text-green-700 mt-2 font-medium">
                      {result.totalTeachers} salary payments have been created for the period{' '}
                      <span className="font-bold">{new Date(result.monthStart).toLocaleDateString('en-IN')}</span> to{' '}
                      <span className="font-bold">{new Date(result.monthEnd).toLocaleDateString('en-IN')}</span>
                    </p>
                  </div>
                </div>
              </div>

              {/* Summary Stats */}
              <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 hover:shadow-xl hover:scale-105 transition-all duration-300">
                  <p className="text-gray-600 text-sm font-semibold">Total Teachers</p>
                  <p className="text-3xl font-bold text-gray-900 mt-3">{result.totalTeachers}</p>
                </div>
                <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 hover:shadow-xl hover:scale-105 transition-all duration-300">
                  <p className="text-gray-600 text-sm font-semibold">Total Base Salary</p>
                  <p className="text-2xl font-bold text-blue-600 mt-3">
                    ₹{result.totalBaseSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                  </p>
                </div>
                <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 hover:shadow-xl hover:scale-105 transition-all duration-300">
                  <p className="text-gray-600 text-sm font-semibold">Total Deductions</p>
                  <p className="text-2xl font-bold text-red-600 mt-3">
                    ₹{result.totalDeductions.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                  </p>
                </div>
                <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 hover:shadow-xl hover:scale-105 transition-all duration-300">
                  <p className="text-gray-600 text-sm font-semibold">Total Net Salary</p>
                  <p className="text-2xl font-bold text-green-600 mt-3">
                    ₹{result.totalNetSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                  </p>
                </div>
              </div>

              {/* Payment Details */}
              <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
                <div className="p-6 border-b-2 border-gray-100 bg-gradient-to-r from-blue-50 to-indigo-50">
                  <h2 className="text-xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                    💳 Payment Details
                  </h2>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                      <tr>
                        <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">
                          Teacher
                        </th>
                        <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">
                          Base Salary
                        </th>
                        <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">
                          Deductions
                        </th>
                        <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">
                          Net Salary
                        </th>
                        <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">
                          Status
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100">
                      {result.paymentDetails.map((payment, idx: number) => (
                        <tr key={idx} className="hover:bg-blue-50 transition-colors duration-200">
                          <td className="px-6 py-4">
                            <p className="font-semibold text-gray-900">{payment.teacherName}</p>
                          </td>
                          <td className="px-6 py-4 text-sm text-blue-600 font-semibold">
                            ₹{payment.baseSalary.toLocaleString('en-IN')}
                          </td>
                          <td className="px-6 py-4 text-sm text-red-600 font-semibold">
                            ₹{payment.deductions.toLocaleString('en-IN')}
                          </td>
                          <td className="px-6 py-4 text-sm font-bold bg-gradient-to-r from-green-50 to-emerald-50 text-green-700 rounded-lg w-fit">
                            ₹{payment.netSalary.toLocaleString('en-IN')}
                          </td>
                          <td className="px-6 py-4">
                            <span className="px-3 py-1 rounded-full text-xs font-semibold bg-yellow-100 text-yellow-800 shadow-sm">
                              {payment.status}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Action */}
              <button
                onClick={() => setResult(null)}
                className="px-8 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-semibold"
              >
                🔄 Create Another Batch
              </button>
            </div>
          ) : (
            // Form View
            <>
              {/* Summary Info */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div className="bg-gradient-to-br from-blue-50 to-blue-100 border-2 border-blue-200 rounded-2xl p-6 hover:shadow-lg transition-all duration-300">
                  <p className="text-blue-800 text-sm font-bold mb-1">👥 Teachers Ready</p>
                  <p className="text-4xl font-bold text-blue-900">
                    {teachersWithAssignments?.length || 0}
                  </p>
                  <p className="text-xs text-blue-700 mt-2">
                    with assigned salary structures
                  </p>
                </div>

                <div className="bg-gradient-to-br from-green-50 to-green-100 border-2 border-green-200 rounded-2xl p-6 hover:shadow-lg transition-all duration-300">
                  <p className="text-green-800 text-sm font-bold mb-1">💰 Estimated Total Salary</p>
                  <p className="text-3xl font-bold text-green-900">
                    ₹
                    {estimatedTotalSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                  </p>
                </div>

                <div className="bg-gradient-to-br from-orange-50 to-orange-100 border-2 border-orange-200 rounded-2xl p-6 hover:shadow-lg transition-all duration-300">
                  <p className="text-orange-800 text-sm font-bold mb-1">💵 Estimated Net Total</p>
                  <p className="text-3xl font-bold text-orange-900">
                    ₹
                    {estimatedNetTotal.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                  </p>
                </div>
              </div>

              {/* Warning */}
              {!isLoading && (!teachersWithAssignments || teachersWithAssignments.length === 0) && (
                <div className="bg-yellow-50 border-2 border-yellow-200 rounded-2xl p-6 flex gap-3 shadow-md">
                  <AlertCircle className="w-6 h-6 text-yellow-600 flex-shrink-0 mt-0.5" />
                  <div>
                    <p className="font-bold text-yellow-900 text-lg">⚠️ No Teachers Available</p>
                    <p className="text-sm text-yellow-700 mt-2">
                      First assign salary structures to teachers before creating bulk salary payments.
                    </p>
                  </div>
                </div>
              )}

              {/* Form */}
              <form onSubmit={handleSubmit} className="bg-white rounded-2xl shadow-lg border border-gray-100 p-8 space-y-8">
                <div>
                  <h2 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent mb-6">📅 Salary Period</h2>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {/* Start Date */}
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-2">
                        Period Start Date *
                      </label>
                      <input
                        type="date"
                        value={formData.periodStartDate}
                        onChange={(e) =>
                          setFormData({ ...formData, periodStartDate: e.target.value })
                        }
                        className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors"
                        required
                      />
                    </div>

                    {/* End Date */}
                    <div>
                      <label className="block text-sm font-semibold text-gray-700 mb-2">
                        Period End Date *
                      </label>
                      <input
                        type="date"
                        value={formData.periodEndDate}
                        onChange={(e) =>
                          setFormData({ ...formData, periodEndDate: e.target.value })
                        }
                        className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors"
                        required
                      />
                    </div>
                  </div>
                </div>

                {/* Deductions */}
                <div>
                  <h2 className="text-2xl font-bold bg-gradient-to-r from-red-600 to-red-800 bg-clip-text text-transparent mb-6">⛔ Fixed Deductions</h2>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-3">
                      Fixed Deduction per Teacher (Optional)
                    </label>
                    <div className="flex items-center gap-4">
                      <div className="flex-1">
                        <input
                          type="number"
                          step="0.01"
                          value={formData.fixedDeductions}
                          onChange={(e) =>
                            setFormData({ ...formData, fixedDeductions: parseFloat(e.target.value) || 0 })
                          }
                          className="w-full px-4 py-3 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors text-lg"
                          placeholder="0"
                        />
                      </div>
                      <div className="text-right bg-gradient-to-br from-red-50 to-red-100 rounded-xl p-4 border border-red-200 min-w-max">
                        <p className="text-sm text-gray-600 mb-1 font-semibold">Total Deductions:</p>
                        <p className="text-3xl font-bold text-red-600">
                          ₹{totalDeductions.toLocaleString('en-IN')}
                        </p>
                      </div>
                    </div>
                    <p className="text-xs text-gray-500 mt-3 font-medium">
                      This amount will be deducted from each teacher's gross salary
                    </p>
                  </div>
                </div>

                {/* Preview */}
                <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-2xl p-6 border-2 border-blue-200">
                  <h3 className="font-bold text-gray-900 mb-6 text-lg">📊 Summary Preview</h3>
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                    <div className="bg-white rounded-lg p-3 shadow-sm">
                      <p className="text-gray-600 mb-1 font-semibold">Teachers</p>
                      <p className="font-bold text-gray-900 text-lg">
                        {teachersWithAssignments?.length || 0}
                      </p>
                    </div>
                    <div className="bg-white rounded-lg p-3 shadow-sm">
                      <p className="text-gray-600 mb-1 font-semibold">Avg Base Salary</p>
                      <p className="font-bold text-gray-900 text-lg">
                        ₹
                        {(
                          estimatedTotalSalary / (teachersWithAssignments?.length || 1)
                        ).toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                      </p>
                    </div>
                    <div className="bg-white rounded-lg p-3 shadow-sm">
                      <p className="text-gray-600 mb-1 font-semibold">Total Deductions</p>
                      <p className="font-bold text-red-600 text-lg">
                        ₹{totalDeductions.toLocaleString('en-IN')}
                      </p>
                    </div>
                    <div className="bg-white rounded-lg p-3 shadow-sm">
                      <p className="text-gray-600 mb-1 font-semibold">Net Payable</p>
                      <p className="font-bold text-green-600 text-lg">
                        ₹{estimatedNetTotal.toLocaleString('en-IN')}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Submit */}
                <button
                  type="submit"
                  disabled={
                    bulkCreateMutation.isPending ||
                    isLoading ||
                    !teachersWithAssignments ||
                    teachersWithAssignments.length === 0
                  }
                  className="w-full px-6 py-4 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:scale-100 transition-all duration-300 font-bold text-lg"
                >
                  {bulkCreateMutation.isPending
                    ? '⏳ Creating Salary Payments...'
                    : '✓ Create Salary Payments'}
                </button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

import React, { useState, useEffect } from 'react';
import {
  useStaffsWithSalaryStructures,
  useApplicableSalaryStructures,
  useAssignSalaryStructureToStaff,
} from '../services/salaryStructureService';
import { StaffApi } from '../services/api';
import { UserCheck, Plus } from 'lucide-react';
import type { Staff } from '../services/api';

interface AssignmentForm {
  staffId: string;
  salaryStructureId: string;
  effectiveDate: string;
}

export const StaffSalaryAssignmentPage: React.FC = () => {
  const { data: StaffsWithAssignments, refetch, isLoading } = useStaffsWithSalaryStructures(true);
  const [allStaffs, setAllStaffs] = useState<Staff[]>([]);
  const [loadingStaffs, setLoadingStaffs] = useState(true);
  const assignMutation = useAssignSalaryStructureToStaff();

  const [showDialog, setShowDialog] = useState(false);
  const [selectedStaffId, setSelectedStaffId] = useState('');
  const { data: applicableStructures } = useApplicableSalaryStructures(selectedStaffId);
  const [formData, setFormData] = useState<AssignmentForm>({
    staffId: '',
    salaryStructureId: '',
    effectiveDate: new Date().toISOString().split('T')[0],
  });

  // Fetch all active Staffs
  useEffect(() => {
    const fetchStaffs = async () => {
      try {
        const response = await StaffApi.getAll({ pageSize: 1000, isActive: true });
        setAllStaffs(response.items);
      } catch (error) {
        console.error('Error fetching Staffs:', error);
      } finally {
        setLoadingStaffs(false);
      }
    };

    fetchStaffs();
  }, []);

  const handleOpenDialog = () => {
    setFormData({
      staffId: '',
      salaryStructureId: '',
      effectiveDate: new Date().toISOString().split('T')[0],
    });
    setSelectedStaffId('');
    setShowDialog(true);
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
  };

  const handleStaffChange = (staffId: string) => {
    setSelectedStaffId(staffId);
    setFormData({ ...formData, staffId, salaryStructureId: '' });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.staffId || !formData.salaryStructureId) {
      alert('Please select both Staff and salary structure');
      return;
    }

    try {
      await assignMutation.mutateAsync(formData);
      handleCloseDialog();
      refetch();
    } catch (error) {
      console.error('Error assigning salary structure:', error);
      alert('Failed to assign salary structure');
    }
  };

  const unassignedStaffs = allStaffs?.filter(
    (t) => !StaffsWithAssignments?.some((a) => a.staffId === t.id)
  ) || [];

  if (loadingStaffs) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">Staff Salary Assignment</h1>
          <div className="mt-12 text-center py-12">
            <div className="inline-block animate-spin">
              <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full" />
            </div>
            <p className="text-gray-600 mt-4 font-medium">Loading Staffs...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 animate-fadeIn">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Staff Salary Assignment
              </h1>
              <p className="text-gray-600 mt-2">Assign salary structures to Staffs based on experience</p>
            </div>
            <button
              onClick={handleOpenDialog}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
            >
              <Plus className="w-5 h-5" />
              Assign Structure
            </button>
          </div>

          {/* Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 hover:shadow-xl hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">Total Staffs</p>
                  <p className="text-4xl font-bold text-gray-900 mt-2">
                    {allStaffs?.length || 0}
                  </p>
                </div>
                <div className="w-14 h-14 bg-gradient-to-br from-blue-100 to-blue-50 rounded-full flex items-center justify-center">
                  <UserCheck className="w-7 h-7 text-blue-600" />
                </div>
              </div>
            </div>

            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 hover:shadow-xl hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">Assigned</p>
                  <p className="text-4xl font-bold text-green-600 mt-2">
                    {StaffsWithAssignments?.length || 0}
                  </p>
                </div>
                <div className="w-14 h-14 bg-gradient-to-br from-green-100 to-green-50 rounded-full flex items-center justify-center">
                  <UserCheck className="w-7 h-7 text-green-600" />
                </div>
              </div>
            </div>

            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 hover:shadow-xl hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-600 text-sm font-semibold">Unassigned</p>
                  <p className="text-4xl font-bold text-orange-600 mt-2">
                    {(allStaffs?.length || 0) - (StaffsWithAssignments?.length || 0)}
                  </p>
                </div>
                <div className="w-14 h-14 bg-gradient-to-br from-orange-100 to-orange-50 rounded-full flex items-center justify-center">
                  <UserCheck className="w-7 h-7 text-orange-600" />
                </div>
              </div>
            </div>
          </div>

          {/* Staffs with Assignments Table */}
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
            <div className="p-6 border-b-2 border-gray-100 bg-gradient-to-r from-blue-50 to-indigo-50">
              <h2 className="text-xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                📋 Assignment Status
              </h2>
            </div>

            {isLoading ? (
              <div className="p-12 text-center">
                <div className="inline-block animate-spin">
                  <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full" />
                </div>
                <p className="text-gray-600 mt-4 font-medium">Loading assignments...</p>
              </div>
            ) : StaffsWithAssignments && StaffsWithAssignments.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                    <tr>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Staff</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Email</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Salary Structure</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Gross Salary</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Effective From</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {StaffsWithAssignments.map((assignment) => (
                      <tr key={assignment.staffId} className="hover:bg-blue-50 transition-colors duration-200">
                        <td className="px-6 py-4">
                          <div className="flex items-center gap-3">
                            {assignment.staffImagePath ? (
                              <div className="flex-shrink-0 h-10 w-10 rounded-full overflow-hidden shadow-md bg-gray-100">
                                <img
                                  src={`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${assignment.staffImagePath}`}
                                  alt={assignment.staffName}
                                  className="w-full h-full object-cover"
                                />
                              </div>
                            ) : (
                              <div className="flex-shrink-0 h-10 w-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-bold text-sm shadow-md">
                                {assignment.staffName.split(' ')[0][0]}{assignment.staffName.split(' ')[1]?.[0] || ''}
                              </div>
                            )}
                            <p className="font-semibold text-gray-900">{assignment.staffName}</p>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-600">{assignment.staffEmail}</td>
                        <td className="px-6 py-4">
                          <span className="inline-block bg-gradient-to-r from-blue-100 to-blue-50 text-blue-700 px-4 py-1 rounded-full text-sm font-semibold shadow-sm">
                            {assignment.salaryStructureName}
                          </span>
                        </td>
                        <td className="px-6 py-4 text-sm font-bold bg-gradient-to-r from-green-50 to-emerald-50 text-green-700 rounded-lg w-fit">
                          ₹{assignment.grossSalary.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-600 font-medium">
                          {new Date(assignment.effectiveDate).toLocaleDateString('en-IN')}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="p-12 text-center">
                <div className="w-16 h-16 bg-gradient-to-br from-blue-100 to-blue-50 rounded-full flex items-center justify-center mb-4">
                  <svg className="w-8 h-8 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                </div>
                <p className="text-gray-600 font-medium">No Staffs assigned yet. Click "Assign Structure" to get started.</p>
              </div>
            )}
          </div>

      {/* Dialog */}
      {showDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-40 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fadeIn">
          <div className="bg-white rounded-2xl shadow-2xl max-w-lg w-full transform transition-all duration-300">
            <div className="p-6 border-b-2 border-gray-100 bg-gradient-to-r from-blue-50 to-indigo-50">
              <h2 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Assign Salary Structure to Staff
              </h2>
            </div>

            <form onSubmit={handleSubmit} className="p-6 space-y-5">
              {/* Staff Selection */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  👨‍🏫 Select Staff *
                </label>
                <select
                  value={formData.staffId}
                  onChange={(e) => handleStaffChange(e.target.value)}
                  className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors font-medium"
                  required
                >
                  <option value="">-- Select a Staff --</option>
                  {unassignedStaffs.map((Staff) => (
                    <option key={Staff.id} value={Staff.id}>
                      {Staff.firstName} {Staff.lastName}
                    </option>
                  ))}
                </select>
              </div>

              {/* Salary Structure Selection */}
              {selectedStaffId && applicableStructures && (
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    💰 Select Salary Structure *
                  </label>
                  {applicableStructures.length > 0 ? (
                    <select
                      value={formData.salaryStructureId}
                      onChange={(e) =>
                        setFormData({ ...formData, salaryStructureId: e.target.value })
                      }
                      className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors font-medium"
                      required
                    >
                      <option value="">-- Select a salary structure --</option>
                      {applicableStructures.map((structure) => (
                        <option key={structure.id} value={structure.id}>
                          {structure.name} - ₹
                          {structure.grossSalary.toLocaleString('en-IN')}
                        </option>
                      ))}
                    </select>
                  ) : (
                    <div className="p-4 bg-yellow-50 border-2 border-yellow-200 rounded-lg text-yellow-700 text-sm font-medium">
                      ⚠️ No applicable salary structures found for this Staff. Check experience requirements.
                    </div>
                  )}
                </div>
              )}

              {/* Effective Date */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  📅 Effective From *
                </label>
                <input
                  type="date"
                  value={formData.effectiveDate}
                  onChange={(e) => setFormData({ ...formData, effectiveDate: e.target.value })}
                  className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors font-medium"
                  required
                />
              </div>

              {/* Actions */}
              <div className="flex gap-3 pt-6 border-t-2 border-gray-100">
                <button
                  type="button"
                  onClick={handleCloseDialog}
                  className="flex-1 px-4 py-3 border-2 border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 hover:border-gray-400 transition-all duration-200 font-semibold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={
                    assignMutation.isPending ||
                    !formData.staffId ||
                    !formData.salaryStructureId
                  }
                  className="flex-1 px-4 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-lg hover:shadow-lg hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:scale-100 transition-all duration-200 font-semibold"
                >
                  {assignMutation.isPending ? 'Assigning...' : 'Assign'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
        </div>
      </div>
    </div>
  );
};

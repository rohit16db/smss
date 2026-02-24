import React, { useState, useEffect } from 'react';
import {
  useTeachersWithSalaryStructures,
  useApplicableSalaryStructures,
  useAssignSalaryStructureToTeacher,
} from '../services/salaryStructureService';
import { teacherApi } from '../services/api';
import { UserCheck, Plus } from 'lucide-react';
import type { Teacher } from '../services/api';

interface AssignmentForm {
  teacherId: string;
  salaryStructureId: string;
  effectiveDate: string;
}

export const TeacherSalaryAssignmentPage: React.FC = () => {
  const { data: teachersWithAssignments, refetch, isLoading } = useTeachersWithSalaryStructures(true);
  const [allTeachers, setAllTeachers] = useState<Teacher[]>([]);
  const [loadingTeachers, setLoadingTeachers] = useState(true);
  const assignMutation = useAssignSalaryStructureToTeacher();

  const [showDialog, setShowDialog] = useState(false);
  const [selectedTeacherId, setSelectedTeacherId] = useState('');
  const { data: applicableStructures } = useApplicableSalaryStructures(selectedTeacherId);
  const [formData, setFormData] = useState<AssignmentForm>({
    teacherId: '',
    salaryStructureId: '',
    effectiveDate: new Date().toISOString().split('T')[0],
  });

  // Fetch all active teachers
  useEffect(() => {
    const fetchTeachers = async () => {
      try {
        const response = await teacherApi.getAll({ pageSize: 1000, isActive: true });
        setAllTeachers(response.items);
      } catch (error) {
        console.error('Error fetching teachers:', error);
      } finally {
        setLoadingTeachers(false);
      }
    };

    fetchTeachers();
  }, []);

  const handleOpenDialog = () => {
    setFormData({
      teacherId: '',
      salaryStructureId: '',
      effectiveDate: new Date().toISOString().split('T')[0],
    });
    setSelectedTeacherId('');
    setShowDialog(true);
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
  };

  const handleTeacherChange = (teacherId: string) => {
    setSelectedTeacherId(teacherId);
    setFormData({ ...formData, teacherId, salaryStructureId: '' });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!formData.teacherId || !formData.salaryStructureId) {
      alert('Please select both teacher and salary structure');
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

  const unassignedTeachers = allTeachers?.filter(
    (t) => !teachersWithAssignments?.some((a) => a.teacherId === t.id)
  ) || [];

  if (loadingTeachers) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">Teacher Salary Assignment</h1>
          <div className="mt-12 text-center py-12">
            <div className="inline-block animate-spin">
              <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full" />
            </div>
            <p className="text-gray-600 mt-4 font-medium">Loading teachers...</p>
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
                Teacher Salary Assignment
              </h1>
              <p className="text-gray-600 mt-2">Assign salary structures to teachers based on experience</p>
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
                  <p className="text-gray-600 text-sm font-semibold">Total Teachers</p>
                  <p className="text-4xl font-bold text-gray-900 mt-2">
                    {allTeachers?.length || 0}
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
                    {teachersWithAssignments?.length || 0}
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
                    {(allTeachers?.length || 0) - (teachersWithAssignments?.length || 0)}
                  </p>
                </div>
                <div className="w-14 h-14 bg-gradient-to-br from-orange-100 to-orange-50 rounded-full flex items-center justify-center">
                  <UserCheck className="w-7 h-7 text-orange-600" />
                </div>
              </div>
            </div>
          </div>

          {/* Teachers with Assignments Table */}
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
            ) : teachersWithAssignments && teachersWithAssignments.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                    <tr>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Teacher</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Email</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Salary Structure</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Gross Salary</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Effective From</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {teachersWithAssignments.map((assignment) => (
                      <tr key={assignment.teacherId} className="hover:bg-blue-50 transition-colors duration-200">
                        <td className="px-6 py-4">
                          <div className="flex items-center gap-3">
                            {assignment.teacherImagePath ? (
                              <div className="flex-shrink-0 h-10 w-10 rounded-full overflow-hidden shadow-md bg-gray-100">
                                <img
                                  src={`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${assignment.teacherImagePath}`}
                                  alt={assignment.teacherName}
                                  className="w-full h-full object-cover"
                                />
                              </div>
                            ) : (
                              <div className="flex-shrink-0 h-10 w-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-bold text-sm shadow-md">
                                {assignment.teacherName.split(' ')[0][0]}{assignment.teacherName.split(' ')[1]?.[0] || ''}
                              </div>
                            )}
                            <p className="font-semibold text-gray-900">{assignment.teacherName}</p>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-600">{assignment.teacherEmail}</td>
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
                <p className="text-gray-600 font-medium">No teachers assigned yet. Click "Assign Structure" to get started.</p>
              </div>
            )}
          </div>

      {/* Dialog */}
      {showDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-40 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fadeIn">
          <div className="bg-white rounded-2xl shadow-2xl max-w-lg w-full transform transition-all duration-300">
            <div className="p-6 border-b-2 border-gray-100 bg-gradient-to-r from-blue-50 to-indigo-50">
              <h2 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Assign Salary Structure to Teacher
              </h2>
            </div>

            <form onSubmit={handleSubmit} className="p-6 space-y-5">
              {/* Teacher Selection */}
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  👨‍🏫 Select Teacher *
                </label>
                <select
                  value={formData.teacherId}
                  onChange={(e) => handleTeacherChange(e.target.value)}
                  className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors font-medium"
                  required
                >
                  <option value="">-- Select a teacher --</option>
                  {unassignedTeachers.map((teacher) => (
                    <option key={teacher.id} value={teacher.id}>
                      {teacher.firstName} {teacher.lastName}
                    </option>
                  ))}
                </select>
              </div>

              {/* Salary Structure Selection */}
              {selectedTeacherId && applicableStructures && (
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
                      ⚠️ No applicable salary structures found for this teacher. Check experience requirements.
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
                    !formData.teacherId ||
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

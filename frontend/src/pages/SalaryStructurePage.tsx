import React, { useState } from 'react';
import {
  useAllSalaryStructures,
  useCreateSalaryStructure,
  useUpdateSalaryStructure,
  useDeleteSalaryStructure,
} from '../services/salaryStructureService';
import { Plus, Edit2, Trash2, X } from 'lucide-react';
import type { SalaryStructureDto, CreateSalaryStructureDto } from '../types/salaryStructure';

export const SalaryStructurePage: React.FC = () => {
  const { data: structures, isLoading, refetch } = useAllSalaryStructures(true);
  const createMutation = useCreateSalaryStructure();
  const updateMutation = useUpdateSalaryStructure();
  const deleteMutation = useDeleteSalaryStructure();

  const [showDialog, setShowDialog] = useState(false);
  const [editingStructure, setEditingStructure] = useState<SalaryStructureDto | null>(null);
  const [formData, setFormData] = useState<CreateSalaryStructureDto>({
    name: '',
    baseSalary: 0,
    hra: 0,
    da: 0,
    medicalAllowance: 0,
    conveyanceAllowance: 0,
    otherAllowances: 0,
    standardDeduction: 0,
    minExperienceYears: 0,
    effectiveFromDate: new Date().toISOString().split('T')[0],
  });

  const handleOpenDialog = (structure?: SalaryStructureDto) => {
    if (structure) {
      setEditingStructure(structure);
      setFormData({
        name: structure.name,
        description: structure.description,
        baseSalary: structure.baseSalary,
        hra: structure.hra,
        da: structure.da,
        medicalAllowance: structure.medicalAllowance,
        conveyanceAllowance: structure.conveyanceAllowance,
        otherAllowances: structure.otherAllowances,
        standardDeduction: structure.standardDeduction,
        minExperienceYears: structure.minExperienceYears,
        applicableQualifications: structure.applicableQualifications,
        effectiveFromDate: structure.effectiveFromDate,
        effectiveToDate: structure.effectiveToDate,
      });
    } else {
      setEditingStructure(null);
      setFormData({
        name: '',
        baseSalary: 0,
        hra: 0,
        da: 0,
        medicalAllowance: 0,
        conveyanceAllowance: 0,
        otherAllowances: 0,
        standardDeduction: 0,
        minExperienceYears: 0,
        effectiveFromDate: new Date().toISOString().split('T')[0],
      });
    }
    setShowDialog(true);
  };

  const handleCloseDialog = () => {
    setShowDialog(false);
    setEditingStructure(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingStructure) {
        await updateMutation.mutateAsync({
          id: editingStructure.id,
          data: { ...formData, id: editingStructure.id },
        });
      } else {
        await createMutation.mutateAsync(formData);
      }
      handleCloseDialog();
      refetch();
    } catch (error) {
      console.error('Error saving salary structure:', error);
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this salary structure?')) {
      try {
        await deleteMutation.mutateAsync(id);
        refetch();
      } catch (error) {
        console.error('Error deleting salary structure:', error);
      }
    }
  };

  const grossSalary =
    (formData.baseSalary || 0) +
    (formData.hra || 0) +
    (formData.da || 0) +
    (formData.medicalAllowance || 0) +
    (formData.conveyanceAllowance || 0) +
    (formData.otherAllowances || 0) -
    (formData.standardDeduction || 0);

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div className="animate-fadeIn">
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Salary Structures
              </h1>
              <p className="text-gray-600 mt-2">Create and manage salary scales for teachers</p>
            </div>
            <button
              onClick={() => handleOpenDialog()}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
            >
              <Plus className="w-5 h-5" />
              New Structure
            </button>
          </div>

          {/* Salary Structures Table */}
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
        {isLoading ? (
          <div className="p-12 text-center">
            <div className="inline-block animate-spin">
              <div className="w-8 h-8 border-4 border-blue-600 border-t-transparent rounded-full" />
            </div>
            <p className="text-gray-600 mt-4">Loading salary structures...</p>
          </div>
        ) : structures && structures.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                <tr>
                  <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Name</th>
                  <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Base Salary</th>
                  <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Allowances</th>
                  <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Deduction</th>
                  <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Gross Salary</th>
                  <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Min Exp (yrs)</th>
                  <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Status</th>
                  <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {structures.map((structure) => (
                  <tr key={structure.id} className="hover:bg-blue-50 transition-colors duration-200">
                    <td className="px-6 py-4">
                      <div>
                        <p className="font-semibold text-gray-900">{structure.name}</p>
                        {structure.description && (
                          <p className="text-sm text-gray-500">{structure.description}</p>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-900 font-medium">
                      ₹{structure.baseSalary.toLocaleString('en-IN')}
                    </td>
                    <td className="px-6 py-4 text-sm text-blue-600 font-semibold">
                      ₹{structure.totalAllowances.toLocaleString('en-IN')}
                    </td>
                    <td className="px-6 py-4 text-sm text-red-600 font-semibold">
                      ₹{structure.standardDeduction.toLocaleString('en-IN')}
                    </td>
                    <td className="px-6 py-4 text-sm font-bold bg-gradient-to-r from-green-50 to-emerald-50 text-green-700 rounded-lg">
                      ₹{structure.grossSalary.toLocaleString('en-IN')}
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-900 font-medium">{structure.minExperienceYears}</td>
                    <td className="px-6 py-4">
                      <span className={`px-3 py-1 rounded-full text-xs font-semibold transition-all ${
                        structure.isActive 
                          ? 'bg-green-100 text-green-800 shadow-sm' 
                          : 'bg-gray-100 text-gray-800 shadow-sm'
                      }`}>
                        {structure.isActive ? '✓ Active' : '○ Inactive'}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleOpenDialog(structure)}
                          className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                          title="Edit"
                        >
                          <Edit2 className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(structure.id)}
                          className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-all duration-200"
                          title="Delete"
                        >
                          <Trash2 className="w-4 h-4" />
                        </button>
                      </div>
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
            <p className="text-gray-600 font-medium">No salary structures found. Create one to get started.</p>
          </div>
        )}
      </div>

      {/* Dialog */}
      {showDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-40 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fadeIn">
          <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto transform transition-all duration-300">
            <div className="flex justify-between items-center p-6 border-b-2 border-gray-100 sticky top-0 bg-gradient-to-r from-blue-50 to-indigo-50">
              <h2 className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                {editingStructure ? 'Edit Salary Structure' : 'New Salary Structure'}
              </h2>
              <button
                onClick={handleCloseDialog}
                className="p-1 hover:bg-red-100 rounded-lg transition-colors duration-200"
              >
                <X className="w-6 h-6 text-gray-600" />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6 space-y-6">
              {/* Basic Info */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Name *
                  </label>
                  <input
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Min Experience (years)
                  </label>
                  <input
                    type="number"
                    value={formData.minExperienceYears || 0}
                    onChange={(e) =>
                      setFormData({ ...formData, minExperienceYears: parseInt(e.target.value) })
                    }
                    className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Description
                </label>
                <textarea
                  value={formData.description || ''}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  rows={2}
                  className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors"
                />
              </div>

              {/* Salary Components */}
              <div className="bg-gradient-to-br from-blue-50 to-indigo-50 rounded-xl p-6 border-2 border-blue-100 space-y-4">
                <h3 className="font-bold text-gray-900 text-lg">💰 Salary Components</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Base Salary *
                    </label>
                    <input
                      type="number"
                      step="0.01"
                      value={formData.baseSalary || 0}
                      onChange={(e) => setFormData({ ...formData, baseSalary: parseFloat(e.target.value) })}
                      className="w-full px-4 py-2 border-2 border-blue-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-400 transition-colors"
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">HRA</label>
                    <input
                      type="number"
                      step="0.01"
                      value={formData.hra || 0}
                      onChange={(e) => setFormData({ ...formData, hra: parseFloat(e.target.value) })}
                      className="w-full px-4 py-2 border-2 border-blue-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-400 transition-colors"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">DA</label>
                    <input
                      type="number"
                      step="0.01"
                      value={formData.da || 0}
                      onChange={(e) => setFormData({ ...formData, da: parseFloat(e.target.value) })}
                      className="w-full px-4 py-2 border-2 border-blue-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-400 transition-colors"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Medical Allowance
                    </label>
                    <input
                      type="number"
                      step="0.01"
                      value={formData.medicalAllowance || 0}
                      onChange={(e) =>
                        setFormData({ ...formData, medicalAllowance: parseFloat(e.target.value) })
                      }
                      className="w-full px-4 py-2 border-2 border-blue-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-400 transition-colors"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Conveyance Allowance
                    </label>
                    <input
                      type="number"
                      step="0.01"
                      value={formData.conveyanceAllowance || 0}
                      onChange={(e) =>
                        setFormData({ ...formData, conveyanceAllowance: parseFloat(e.target.value) })
                      }
                      className="w-full px-4 py-2 border-2 border-blue-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-400 transition-colors"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Other Allowances
                    </label>
                    <input
                      type="number"
                      step="0.01"
                      value={formData.otherAllowances || 0}
                      onChange={(e) =>
                        setFormData({ ...formData, otherAllowances: parseFloat(e.target.value) })
                      }
                      className="w-full px-4 py-2 border-2 border-blue-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-400 transition-colors"
                    />
                  </div>
                </div>
              </div>

              {/* Deductions */}
              <div className="bg-gradient-to-br from-red-50 to-orange-50 rounded-xl p-6 border-2 border-red-100">
                <h3 className="font-bold text-gray-900 text-lg mb-4">⛔ Deductions</h3>
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Standard Deduction
                  </label>
                  <input
                    type="number"
                    step="0.01"
                    value={formData.standardDeduction || 0}
                    onChange={(e) =>
                      setFormData({ ...formData, standardDeduction: parseFloat(e.target.value) })
                    }
                    className="w-full px-4 py-2 border-2 border-red-200 rounded-lg focus:ring-2 focus:ring-red-500 focus:border-transparent hover:border-red-400 transition-colors"
                  />
                </div>
              </div>

              {/* Effective Dates */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Effective From *
                  </label>
                  <input
                    type="date"
                    value={formData.effectiveFromDate}
                    onChange={(e) => setFormData({ ...formData, effectiveFromDate: e.target.value })}
                    className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Effective To
                  </label>
                  <input
                    type="date"
                    value={formData.effectiveToDate || ''}
                    onChange={(e) =>
                      setFormData({ ...formData, effectiveToDate: e.target.value || undefined })
                    }
                    className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent hover:border-blue-300 transition-colors"
                  />
                </div>
              </div>

              {/* Gross Salary Display */}
              <div className="bg-gradient-to-br from-green-50 to-emerald-50 rounded-xl p-6 border-2 border-green-200 transform transition-all hover:scale-105">
                <p className="text-sm font-semibold text-gray-600 mb-1">💵 Total Gross Salary</p>
                <p className="text-4xl font-bold text-green-700">
                  ₹{grossSalary.toLocaleString('en-IN', { maximumFractionDigits: 2 })}
                </p>
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
                  disabled={createMutation.isPending || updateMutation.isPending}
                  className="flex-1 px-4 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-lg hover:shadow-lg hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed disabled:scale-100 transition-all duration-200 font-semibold"
                >
                  {createMutation.isPending || updateMutation.isPending
                    ? 'Saving...'
                    : editingStructure
                    ? 'Update'
                    : 'Create'}
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

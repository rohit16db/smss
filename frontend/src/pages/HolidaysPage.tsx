import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { AxiosError } from 'axios';
import { holidayApi, type CreateHolidayDto, type UpdateHolidayDto, type Holiday } from '../services/api';

export function HolidaysPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [academicYearFilter, setAcademicYearFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedHoliday, setSelectedHoliday] = useState<Holiday | null>(null);
  const [formData, setFormData] = useState<CreateHolidayDto>({
    name: '',
    holidayDate: '',
    description: '',
    type: '',
    academicYear: new Date().getFullYear() + '-' + (new Date().getFullYear() + 1),
  });

  // Queries
  const { data: holidaysData, isLoading } = useQuery({
    queryKey: ['holidays', page, pageSize, academicYearFilter, typeFilter],
    queryFn: () =>
      holidayApi.getAll({
        pageNumber: page,
        pageSize,
        academicYear: academicYearFilter || undefined,
        type: typeFilter || undefined,
      }),
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: holidayApi.create,
    onSuccess: () => {
      toast.success('Holiday created successfully!');
      queryClient.invalidateQueries({ queryKey: ['holidays'] });
      handleCloseDialog();
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to create holiday');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateHolidayDto }) => holidayApi.update(id, data),
    onSuccess: () => {
      toast.success('Holiday updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['holidays'] });
      handleCloseDialog();
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to update holiday');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: holidayApi.delete,
    onSuccess: () => {
      toast.success('Holiday deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['holidays'] });
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to delete holiday');
    },
  });

  // Handlers
  const handleOpenDialog = (holiday?: Holiday) => {
    if (holiday) {
      setSelectedHoliday(holiday);
      setFormData({
        name: holiday.name,
        holidayDate: holiday.holidayDate.split('T')[0], // Extract date part
        description: holiday.description || '',
        type: holiday.type || '',
        academicYear: holiday.academicYear,
      });
    } else {
      setSelectedHoliday(null);
      setFormData({
        name: '',
        holidayDate: '',
        description: '',
        type: '',
        academicYear: new Date().getFullYear() + '-' + (new Date().getFullYear() + 1),
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedHoliday(null);
    setFormData({
      name: '',
      holidayDate: '',
      description: '',
      type: '',
      academicYear: new Date().getFullYear() + '-' + (new Date().getFullYear() + 1),
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    // Validation
    if (!formData.name || !formData.holidayDate || !formData.academicYear) {
      toast.error('Please fill in all required fields');
      return;
    }

    // Validate academic year format
    const academicYearRegex = /^\d{4}-\d{4}$/;
    if (!academicYearRegex.test(formData.academicYear)) {
      toast.error('Academic year must be in format YYYY-YYYY (e.g., 2025-2026)');
      return;
    }

    if (selectedHoliday) {
      updateMutation.mutate({
        id: selectedHoliday.id,
        data: formData,
      });
    } else {
      createMutation.mutate(formData);
    }
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this holiday?')) {
      deleteMutation.mutate(id);
    }
  };

  const totalPages = holidaysData ? Math.ceil(holidaysData.totalCount / pageSize) : 0;

  return (
    <div className="min-h-screen bg-gradient-to-br from-purple-50 via-pink-50 to-purple-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent flex items-center gap-3">
                <span>🏖️</span> Holiday Management
              </h1>
              <p className="text-gray-600 mt-2">Manage school holidays and events</p>
            </div>
            <button
              onClick={() => handleOpenDialog()}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Add Holiday
            </button>
          </div>

          {/* Filters */}
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Academic Year
                </label>
                <input
                  type="text"
                  placeholder="e.g., 2025-2026"
                  value={academicYearFilter}
                  onChange={(e) => {
                    setAcademicYearFilter(e.target.value);
                    setPage(1);
                  }}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Holiday Type
                </label>
                <input
                  type="text"
                  placeholder="e.g., National, Religious, School Event"
                  value={typeFilter}
                  onChange={(e) => {
                    setTypeFilter(e.target.value);
                    setPage(1);
                  }}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                />
              </div>
            </div>
          </div>

          {/* Content Card */}
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
            {isLoading ? (
              <div className="text-center py-12">
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600 mb-4"></div>
                <p className="text-gray-500 font-medium">Loading holidays...</p>
              </div>
            ) : holidaysData?.items.length === 0 ? (
              <div className="text-center py-12">
                <div className="text-gray-400 text-6xl mb-4">🗓️</div>
                <p className="text-gray-500 text-lg font-medium">No holidays found</p>
                <p className="text-gray-400 text-sm mt-2">Add a holiday to get started</p>
              </div>
            ) : (
              <>
                {/* Desktop Table */}
                <div className="hidden lg:block overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-gradient-to-r from-purple-50 to-pink-50 border-b-2 border-purple-200">
                      <tr>
                        <th className="px-6 py-4 text-left text-xs font-bold text-purple-900 uppercase tracking-wider">
                          Date
                        </th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-purple-900 uppercase tracking-wider">
                          Holiday Name
                        </th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-purple-900 uppercase tracking-wider">
                          Type
                        </th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-purple-900 uppercase tracking-wider">
                          Academic Year
                        </th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-purple-900 uppercase tracking-wider">
                          Description
                        </th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-purple-900 uppercase tracking-wider">
                          Actions
                        </th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                      {holidaysData?.items.map((holiday) => (
                        <tr key={holiday.id} className="hover:bg-purple-50/50 transition-colors duration-150">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm font-bold text-purple-900">
                              {new Date(holiday.holidayDate).toLocaleDateString('en-US', {
                                year: 'numeric',
                                month: 'short',
                                day: 'numeric',
                              })}
                            </div>
                            <div className="text-xs text-gray-500">
                              {new Date(holiday.holidayDate).toLocaleDateString('en-US', { weekday: 'long' })}
                            </div>
                          </td>
                          <td className="px-6 py-4">
                            <div className="text-sm font-semibold text-gray-900">{holiday.name}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            {holiday.type ? (
                              <span className="px-3 py-1 text-xs font-semibold rounded-full bg-purple-100 text-purple-800">
                                {holiday.type}
                              </span>
                            ) : (
                              <span className="text-gray-400 text-xs">-</span>
                            )}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className="text-sm font-medium text-gray-700">{holiday.academicYear}</span>
                          </td>
                          <td className="px-6 py-4">
                            <div className="text-sm text-gray-600 max-w-xs truncate">
                              {holiday.description || '-'}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex gap-2">
                              <button
                                onClick={() => handleOpenDialog(holiday)}
                                className="px-3 py-1.5 bg-blue-100 hover:bg-blue-200 text-blue-700 rounded-lg transition text-xs font-medium"
                              >
                                Edit
                              </button>
                              <button
                                onClick={() => handleDelete(holiday.id)}
                                className="px-3 py-1.5 bg-red-100 hover:bg-red-200 text-red-700 rounded-lg transition text-xs font-medium"
                              >
                                Delete
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>

                {/* Mobile Cards */}
                <div className="lg:hidden divide-y divide-gray-200">
                  {holidaysData?.items.map((holiday) => (
                    <div key={holiday.id} className="p-6 hover:bg-purple-50/50 transition-colors">
                      <div className="space-y-3">
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="text-lg font-bold text-gray-900 mb-1">{holiday.name}</div>
                            <div className="text-sm font-semibold text-purple-600">
                              {new Date(holiday.holidayDate).toLocaleDateString('en-US', {
                                year: 'numeric',
                                month: 'long',
                                day: 'numeric',
                                weekday: 'long',
                              })}
                            </div>
                          </div>
                        </div>
                        
                        <div className="grid grid-cols-2 gap-3 text-sm">
                          <div>
                            <span className="text-gray-500 font-medium">Type:</span>
                            <div className="mt-1">
                              {holiday.type ? (
                                <span className="px-2 py-1 text-xs font-semibold rounded-full bg-purple-100 text-purple-800">
                                  {holiday.type}
                                </span>
                              ) : (
                                <span className="text-gray-400">-</span>
                              )}
                            </div>
                          </div>
                          <div>
                            <span className="text-gray-500 font-medium">Academic Year:</span>
                            <div className="mt-1 font-semibold text-gray-700">{holiday.academicYear}</div>
                          </div>
                        </div>

                        {holiday.description && (
                          <div>
                            <span className="text-gray-500 font-medium text-sm">Description:</span>
                            <p className="text-sm text-gray-600 mt-1">{holiday.description}</p>
                          </div>
                        )}

                        <div className="flex gap-2 pt-2">
                          <button
                            onClick={() => handleOpenDialog(holiday)}
                            className="flex-1 px-4 py-2 bg-blue-100 hover:bg-blue-200 text-blue-700 rounded-lg transition font-medium"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleDelete(holiday.id)}
                            className="flex-1 px-4 py-2 bg-red-100 hover:bg-red-200 text-red-700 rounded-lg transition font-medium"
                          >
                            Delete
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="bg-gradient-to-r from-purple-50 to-pink-50 border-t border-purple-200 px-6 py-4 flex items-center justify-between">
                    <div className="text-sm text-gray-600">
                      Page {page} of {totalPages} • {holidaysData?.totalCount} total holidays
                    </div>
                    <div className="flex gap-2">
                      <button
                        onClick={() => setPage((p) => Math.max(1, p - 1))}
                        disabled={page === 1}
                        className="px-4 py-2 border border-purple-300 rounded-lg hover:bg-purple-100 disabled:opacity-50 disabled:cursor-not-allowed transition font-medium"
                      >
                        Previous
                      </button>
                      <button
                        onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                        disabled={page === totalPages}
                        className="px-4 py-2 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-lg hover:shadow-lg disabled:opacity-50 disabled:cursor-not-allowed transition font-medium"
                      >
                        Next
                      </button>
                    </div>
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </div>

      {/* Dialog */}
      {dialogOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="bg-gradient-to-r from-purple-600 to-pink-600 px-6 py-4 flex items-center justify-between">
              <h2 className="text-2xl font-bold text-white flex items-center gap-2">
                <span>🏖️</span>
                {selectedHoliday ? 'Edit Holiday' : 'Add Holiday'}
              </h2>
              <button
                onClick={handleCloseDialog}
                className="text-white hover:bg-white/20 rounded-lg p-2 transition"
              >
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6 space-y-6">
              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">
                  Holiday Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  maxLength={200}
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
                  placeholder="e.g., Independence Day, Spring Break"
                />
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">
                    Date <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="date"
                    required
                    value={formData.holidayDate}
                    onChange={(e) => setFormData({ ...formData, holidayDate: e.target.value })}
                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
                  />
                </div>

                <div>
                  <label className="block text-sm font-bold text-gray-700 mb-2">
                    Academic Year <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    pattern="\d{4}-\d{4}"
                    value={formData.academicYear}
                    onChange={(e) => setFormData({ ...formData, academicYear: e.target.value })}
                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
                    placeholder="e.g., 2025-2026"
                  />
                  <p className="text-xs text-gray-500 mt-1">Format: YYYY-YYYY</p>
                </div>
              </div>

              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">
                  Holiday Type
                </label>
                <input
                  type="text"
                  maxLength={50}
                  value={formData.type}
                  onChange={(e) => setFormData({ ...formData, type: e.target.value })}
                  className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-transparent transition"
                  placeholder="e.g., National, Religious, School Event"
                />
              </div>

              <div>
                <label className="block text-sm font-bold text-gray-700 mb-2">
                  Description
                </label>
                <textarea
                  rows={4}
                  maxLength={500}
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-transparent transition resize-none"
                  placeholder="Add additional details about this holiday..."
                />
                <p className="text-xs text-gray-500 mt-1">
                  {formData.description?.length || 0}/500 characters
                </p>
              </div>

              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={handleCloseDialog}
                  className="flex-1 px-6 py-3 border-2 border-gray-300 text-gray-700 rounded-xl hover:bg-gray-50 transition font-medium"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createMutation.isPending || updateMutation.isPending}
                  className="flex-1 px-6 py-3 bg-gradient-to-r from-purple-600 to-pink-600 text-white rounded-xl hover:shadow-lg transition font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {createMutation.isPending || updateMutation.isPending
                    ? 'Saving...'
                    : selectedHoliday
                    ? 'Update Holiday'
                    : 'Create Holiday'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

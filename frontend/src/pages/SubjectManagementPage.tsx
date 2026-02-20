import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type { AxiosError } from 'axios';
import { subjectApi, type CreateSubjectDto, type UpdateSubjectDto, type Subject, type SubjectListDto } from '../services/api';

export function SubjectManagementPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedSubject, setSelectedSubject] = useState<Subject | null>(null);
  const [formData, setFormData] = useState<CreateSubjectDto>({
    name: '',
    code: '',
    description: '',
    credits: undefined,
    displayOrder: 0,
  });

  // Queries
  const { data: subjectsData, isLoading } = useQuery({
    queryKey: ['subjects', page, pageSize],
    queryFn: () => subjectApi.getAll({ pageNumber: page, pageSize, searchTerm: undefined, isActive: undefined }),
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: subjectApi.create,
    onSuccess: () => {
      toast.success('Subject created successfully!');
      queryClient.invalidateQueries({ queryKey: ['subjects'] });
      handleCloseDialog();
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to create subject');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSubjectDto }) => subjectApi.update(id, data),
    onSuccess: () => {
      toast.success('Subject updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['subjects'] });
      handleCloseDialog();
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to update subject');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: subjectApi.delete,
    onSuccess: () => {
      toast.success('Subject deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['subjects'] });
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to delete subject');
    },
  });

  // Handlers
  const handleOpenDialog = (subject?: Subject) => {
    if (subject) {
      setSelectedSubject(subject);
      setFormData({
        name: subject.name,
        code: subject.code,
        description: subject.description,
        credits: subject.credits,
        displayOrder: subject.displayOrder,
      });
    } else {
      setSelectedSubject(null);
      setFormData({
        name: '',
        code: '',
        description: '',
        credits: undefined,
        displayOrder: 0,
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedSubject(null);
    setFormData({
      name: '',
      code: '',
      description: '',
      credits: undefined,
      displayOrder: 0,
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (selectedSubject) {
      updateMutation.mutate({
        id: selectedSubject.id,
        data: {
          ...formData,
          isActive: selectedSubject.isActive,
        },
      });
    } else {
      createMutation.mutate(formData);
    }
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this subject?')) {
      deleteMutation.mutate(id);
    }
  };

  const totalPages = subjectsData ? Math.ceil(subjectsData.totalCount / pageSize) : 0;

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Subject Management
              </h1>
              <p className="text-gray-600 mt-2">Manage all subjects and courses</p>
            </div>
            <button
              onClick={() => handleOpenDialog()}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Add Subject
            </button>
          </div>

          {/* Content Card */}
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
          {isLoading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
            </div>
          ) : subjectsData && subjectsData.items.length > 0 ? (
            <>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                    <tr>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Code</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Name</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Description</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Credits</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Status</th>
                      <th className="px-6 py-4 text-right text-sm font-bold text-gray-900">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {subjectsData.items.map((subject: SubjectListDto) => (
                      <tr key={subject.id} className="hover:bg-blue-50 transition-colors duration-200">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className="inline-block font-mono font-bold text-blue-600 bg-blue-50 px-3 py-1 rounded-lg">
                            {subject.code}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-bold text-gray-900">{subject.name}</div>
                        </td>
                        <td className="px-6 py-4">
                          <div className="text-sm text-gray-600 line-clamp-1">{subject.code || '-'}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-gray-700">
                            {subject.credits ? `${subject.credits} credit${subject.credits !== 1 ? 's' : ''}` : '-'}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span
                            className={`inline-block px-3 py-1 rounded-full text-xs font-semibold ${
                              subject.isActive
                                ? 'bg-green-100 text-green-700'
                                : 'bg-gray-100 text-gray-700'
                            }`}
                          >
                            {subject.isActive ? '✓ Active' : '○ Inactive'}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right">
                          <div className="flex items-center justify-end gap-1">
                            <button
                              onClick={() => {
                                subjectApi.getById(subject.id).then((data) => handleOpenDialog(data));
                              }}
                              className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                              title="Edit"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                              </svg>
                            </button>
                            <button
                              onClick={() => handleDelete(subject.id)}
                              className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-all duration-200"
                              title="Delete"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                              </svg>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          ) : (
            <div className="text-center py-16">
              <div className="w-16 h-16 bg-gradient-to-br from-blue-100 to-blue-50 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg className="w-8 h-8 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C6.5 6.253 2 10.585 2 15.667c0 5.082 4.5 9.414 10 9.414s10-4.332 10-9.414c0-5.082-4.5-9.414-10-9.414z" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">No subjects found</h3>
              <p className="text-gray-600 mb-6">Start by creating your first subject to get started.</p>
              <button
                onClick={() => handleOpenDialog()}
                className="inline-flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium"
              >
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Create First Subject
              </button>
            </div>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between px-6 py-4 border-t border-gray-100">
              <div className="text-sm text-gray-600">
                Showing {subjectsData?.items.length || 0} of {subjectsData?.totalCount || 0} subjects
              </div>
              <div className="flex gap-2">
                <button
                  onClick={() => page > 1 && setPage(page - 1)}
                  disabled={page === 1}
                  className="px-4 py-2 text-sm font-medium text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
                >
                  ← Previous
                </button>
                <div className="flex items-center gap-2">
                  <span className="text-sm text-gray-600">
                    Page {page} of {totalPages}
                  </span>
                </div>
                <button
                  onClick={() => page < totalPages && setPage(page + 1)}
                  disabled={page === totalPages}
                  className="px-4 py-2 text-sm font-medium text-blue-600 border border-blue-300 rounded-lg hover:bg-blue-50 disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
                >
                  Next →
                </button>
              </div>
            </div>
          )}
          </div>
        </div>
      </div>

      {/* Dialog */}
      {dialogOpen && (
        <div className="fixed inset-0 bg-black bg-opacity-60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fadeIn">
          <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full animate-slideUp">
            <div className="bg-gradient-to-r from-blue-600 to-blue-700 p-6 rounded-t-2xl">
              <h2 className="text-2xl font-bold text-white flex items-center gap-2">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C6.5 6.253 2 10.585 2 15.667c0 5.082 4.5 9.414 10 9.414s10-4.332 10-9.414c0-5.082-4.5-9.414-10-9.414z" />
                </svg>
                {selectedSubject ? 'Edit Subject' : 'New Subject'}
              </h2>
            </div>
            <form onSubmit={handleSubmit} className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Subject Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-600 focus:border-transparent"
                  placeholder="e.g., Mathematics"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Subject Code <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  required
                  value={formData.code}
                  onChange={(e) => setFormData({ ...formData, code: e.target.value.toUpperCase() })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-600 focus:border-transparent font-mono"
                  placeholder="e.g., MATH101"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  value={formData.description || ''}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-600 focus:border-transparent"
                  rows={3}
                  placeholder="Optional description"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Credits</label>
                  <input
                    type="number"
                    min="0"
                    value={formData.credits || ''}
                    onChange={(e) => setFormData({ ...formData, credits: e.target.value ? parseInt(e.target.value) : undefined })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-600 focus:border-transparent"
                    placeholder="0"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Display Order</label>
                  <input
                    type="number"
                    min="0"
                    value={formData.displayOrder}
                    onChange={(e) => setFormData({ ...formData, displayOrder: parseInt(e.target.value) || 0 })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-600 focus:border-transparent"
                  />
                </div>
              </div>

              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={handleCloseDialog}
                  className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createMutation.isPending || updateMutation.isPending}
                  className="flex-1 px-4 py-2 bg-gradient-to-r from-blue-600 to-blue-700 hover:shadow-lg text-white font-medium rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed transform hover:scale-105"
                >
                  {createMutation.isPending || updateMutation.isPending ? 'Saving...' : selectedSubject ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

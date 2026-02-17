import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { teacherApi, type CreateTeacherDto, type UpdateTeacherDto, type Teacher } from '../services/api';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';
import { EmptyState, NoDataIcon } from '../components/common/EmptyState';
import { TeacherAssignmentsDialog } from '../components/teachers/TeacherAssignmentsDialog';

export function TeachersPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [rowsPerPage] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedTeacher, setSelectedTeacher] = useState<Teacher | null>(null);
  const [openAssignmentsDialog, setOpenAssignmentsDialog] = useState(false);
  const [assignmentTeacher, setAssignmentTeacher] = useState<Teacher | null>(null);
  const [formData, setFormData] = useState<CreateTeacherDto>({
    userId: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    qualification: '',
    experienceYears: 0,
    joiningDate: new Date().toISOString().split('T')[0],
  });

  const { data: teachersData, isLoading } = useQuery({
    queryKey: ['teachers', page + 1, rowsPerPage, searchTerm],
    queryFn: () => teacherApi.getAll({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
      searchTerm: searchTerm || undefined,
    }),
  });

  const createMutation = useMutation({
    mutationFn: teacherApi.create,
    onSuccess: () => {
      toast.success('Teacher created successfully!');
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to create teacher');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTeacherDto }) =>
      teacherApi.update(id, data),
    onSuccess: () => {
      toast.success('Teacher updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to update teacher');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: teacherApi.delete,
    onSuccess: () => {
      toast.success('Teacher deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to delete teacher');
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      isActive ? teacherApi.deactivate(id) : teacherApi.activate(id),
    onSuccess: () => {
      toast.success('Teacher status updated!');
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to update status');
    },
  });

  const handleOpenDialog = (teacher?: Teacher) => {
    if (teacher) {
      setSelectedTeacher(teacher);
      setFormData({
        userId: teacher.userId,
        firstName: teacher.firstName,
        lastName: teacher.lastName,
        email: teacher.email,
        phone: teacher.phone || '',
        qualification: teacher.qualification || '',
        experienceYears: teacher.experienceYears,
        joiningDate: teacher.joiningDate.split('T')[0],
      });
    } else {
      setSelectedTeacher(null);
      setFormData({
        userId: '',
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        qualification: '',
        experienceYears: 0,
        joiningDate: new Date().toISOString().split('T')[0],
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedTeacher(null);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedTeacher) {
      updateMutation.mutate({
        id: selectedTeacher.id,
        data: {
          id: selectedTeacher.id,
          userId: formData.userId,
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phone: formData.phone || '',
          qualification: formData.qualification || '',
          experienceYears: formData.experienceYears,
          joiningDate: formData.joiningDate,
          isActive: selectedTeacher.isActive,
        },
      });
    } else {
      createMutation.mutate(formData);
    }
  };

  const handleDelete = (id: string) => {
    if (confirm('Are you sure you want to delete this teacher?')) {
      deleteMutation.mutate(id);
    }
  };

  const handleToggleActive = (teacher: Teacher) => {
    toggleActiveMutation.mutate({ id: teacher.id, isActive: teacher.isActive });
  };

  const totalPages = Math.ceil((teachersData?.totalCount || 0) / rowsPerPage);

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50 to-purple-50 pb-8">
      {/* Header Section */}
      <div className="bg-gradient-to-r from-amber-600 to-orange-600 shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="flex items-center gap-4 mb-2">
            <div className="p-3 bg-white/20 rounded-xl backdrop-blur-sm">
              <svg className="w-8 h-8 text-white" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
              </svg>
            </div>
            <div>
              <h1 className="text-3xl font-bold text-white">Teacher Management</h1>
              <p className="text-orange-100 mt-1">Manage and track comprehensive teacher information</p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="mb-8">
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div className="hidden"></div>
            <button onClick={() => handleOpenDialog()} className="btn-primary flex items-center gap-2 justify-center bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 shadow-lg\">
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Add Teacher
            </button>
          </div>
        </div>

        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 mb-8 hover:shadow-xl transition-shadow">
          <div className="flex flex-col lg:flex-row gap-6 items-start lg:items-center justify-between">
            <div className="flex-1 w-full lg:max-w-md">
              <label className="block text-sm font-semibold text-gray-700 mb-2">Search Teachers</label>
              <div className="relative">
                <input
                  type="text"
                  placeholder="Search by name, email, phone..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full px-4 py-3 pl-10 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-orange-500 focus:border-orange-500 transition-all"
                />
                <svg className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
            </div>
            <div className="flex gap-6 w-full lg:w-auto">
              <div className="group flex-1 lg:flex-none bg-gradient-to-br from-blue-50 to-blue-100 px-6 py-4 rounded-xl border-2 border-blue-200 hover:shadow-lg transition-all">
                <p className="text-3xl font-bold text-blue-600">{teachersData?.totalCount || 0}</p>
                <p className="text-sm text-blue-700 font-semibold mt-1">Total Teachers</p>
              </div>
              <div className="group flex-1 lg:flex-none bg-gradient-to-br from-green-50 to-green-100 px-6 py-4 rounded-xl border-2 border-green-200 hover:shadow-lg transition-all">
                <p className="text-3xl font-bold text-green-600">{teachersData?.items.filter(t => t.isActive).length || 0}</p>
                <p className="text-sm text-green-700 font-semibold mt-1">Active Teachers</p>
              </div>
            </div>
          </div>
        </div>

        {isLoading ? (
          <div className="card">
            <LoadingSkeleton rows={rowsPerPage} type="table" />
          </div>
        ) : !teachersData?.items || teachersData.items.length === 0 ? (
          <div className="card">
            <EmptyState
              icon={<NoDataIcon />}
              title="No teachers found"
              description={searchTerm ? "Try adjusting your search criteria" : "Get started by adding your first teacher to the system"}
              action={!searchTerm ? {
                label: "Add Teacher",
                onClick: () => handleOpenDialog()
              } : undefined}
            />
          </div>
        ) : (
          <>
            <div className="hidden lg:block bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden\">
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gradient-to-r from-amber-600 to-orange-600\">
                    <tr>
                      <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase tracking-wider">Teacher</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase tracking-wider">Contact</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase tracking-wider">Experience</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase tracking-wider">Joining Date</th>
                      <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase tracking-wider">Status</th>
                      <th className="px-6 py-4 text-right text-xs font-bold text-white uppercase tracking-wider">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-100\">
                    {isLoading ? (
                      <tr>
                        <td colSpan={6} className="px-6 py-12 text-center">
                          <div className="flex justify-center items-center">
                            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                          </div>
                        </td>
                      </tr>
                    ) : teachersData?.items.length === 0 ? (
                      <tr>
                        <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
                          <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                          </svg>
                          <p className="mt-2">No teachers found</p>
                        </td>
                      </tr>
                    ) : (
                      teachersData?.items.map((teacher) => (
                        <tr key={teacher.id} className="hover:bg-gradient-to-r hover:from-amber-50 hover:to-orange-50 transition-all border-b border-gray-100\">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex items-center gap-3">
                              <div className="flex-shrink-0 h-12 w-12 bg-gradient-to-br from-amber-500 to-orange-500 rounded-full flex items-center justify-center text-white font-bold text-lg shadow-md">
                                {teacher.firstName[0]}{teacher.lastName[0]}
                              </div>
                              <div>
                                <div className="text-sm font-bold text-gray-900">{teacher.firstName} {teacher.lastName}</div>
                                <div className="text-xs text-gray-600 mt-1">{teacher.qualification || 'No qualification specified'}</div>
                              </div>
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm font-medium text-gray-900">{teacher.email}</div>
                            <div className="text-xs text-gray-600 mt-1">{teacher.phone || 'No phone'}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm font-bold text-gray-900">{teacher.experienceYears}</div>
                            <div className="text-xs text-gray-600 mt-1">years</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-700\">
                            {new Date(teacher.joiningDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <button onClick={() => handleToggleActive(teacher)} className={`px-3 py-1.5 rounded-full text-xs font-bold transition-all ${
                              teacher.isActive 
                                ? 'bg-green-100 text-green-700 shadow-sm hover:shadow-md hover:bg-green-200' 
                                : 'bg-gray-100 text-gray-700 shadow-sm hover:shadow-md hover:bg-gray-200'
                            }`}>
                              {teacher.isActive ? '✓ Active' : '○ Inactive'}
                            </button>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                            <div className="flex gap-2 justify-end">
                              <button 
                                onClick={() => {
                                  setAssignmentTeacher(teacher);
                                  setOpenAssignmentsDialog(true);
                                }} 
                                className="text-purple-600 hover:text-purple-900 p-2 hover:bg-purple-50 rounded-lg transition-colors" 
                                title="Manage Assignments"
                              >
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                                </svg>
                              </button>
                              <button onClick={() => handleOpenDialog(teacher)} className="text-blue-600 hover:text-blue-900 p-2 hover:bg-blue-50 rounded-lg transition-colors" title="Edit">
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                </svg>
                              </button>
                              <button onClick={() => handleDelete(teacher.id)} className="text-red-600 hover:text-red-900 p-2 hover:bg-red-50 rounded-lg transition-colors" title="Delete">
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                </svg>
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>

              {teachersData && teachersData.totalCount > 0 && (
                <div className="bg-gray-50 px-6 py-4 border-t border-gray-200 flex items-center justify-between">
                  <div className="text-sm text-gray-700">
                    Showing <span className="font-medium">{page * rowsPerPage + 1}</span> to{' '}
                    <span className="font-medium">{Math.min((page + 1) * rowsPerPage, teachersData.totalCount)}</span> of{' '}
                    <span className="font-medium">{teachersData.totalCount}</span> teachers
                  </div>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => setPage(Math.max(0, page - 1))}
                      disabled={page === 0}
                      className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                      aria-label="Go to previous page"
                    >
                      Previous
                    </button>
                    <span className="text-sm text-gray-700">Page {page + 1} of {totalPages}</span>
                    <button
                      onClick={() => setPage(Math.min(totalPages - 1, page + 1))}
                      disabled={page >= totalPages - 1}
                      className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                      aria-label="Go to next page"
                    >
                      Next
                    </button>
                  </div>
                </div>
              )}
            </div>
          </>
        )}
      </div>

      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4 animate-fade-in" role="dialog" aria-modal="true" aria-labelledby="dialog-title">
          <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto animate-slide-up">
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between rounded-t-2xl">
              <h2 id="dialog-title" className="text-2xl font-bold text-gray-900">{selectedTeacher ? 'Edit Teacher' : 'Add New Teacher'}</h2>
              <button onClick={handleCloseDialog} className="text-gray-400 hover:text-gray-600 transition-colors" aria-label="Close dialog">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6">
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">User ID <span className="text-red-500">*</span></label>
                  <input type="text" value={formData.userId} onChange={(e) => setFormData({ ...formData, userId: e.target.value })} required disabled={!!selectedTeacher} className="input-field disabled:bg-gray-100 disabled:cursor-not-allowed" placeholder="Enter user ID" />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">First Name <span className="text-red-500">*</span></label>
                    <input type="text" value={formData.firstName} onChange={(e) => setFormData({ ...formData, firstName: e.target.value })} required className="input-field" placeholder="John" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Last Name <span className="text-red-500">*</span></label>
                    <input type="text" value={formData.lastName} onChange={(e) => setFormData({ ...formData, lastName: e.target.value })} required className="input-field" placeholder="Doe" />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Email <span className="text-red-500">*</span></label>
                  <input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} required className="input-field" placeholder="john.doe@school.com" />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
                  <input type="tel" value={formData.phone} onChange={(e) => setFormData({ ...formData, phone: e.target.value })} className="input-field" placeholder="+1 (555) 000-0000" />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Qualification</label>
                  <textarea value={formData.qualification} onChange={(e) => setFormData({ ...formData, qualification: e.target.value })} className="input-field resize-none" rows={3} placeholder="e.g., M.Ed in Mathematics, B.Sc in Physics" />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Experience (years)</label>
                    <input type="number" value={formData.experienceYears} onChange={(e) => setFormData({ ...formData, experienceYears: parseInt(e.target.value) || 0 })} className="input-field" min="0" placeholder="5" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Joining Date</label>
                    <input type="date" value={formData.joiningDate} onChange={(e) => setFormData({ ...formData, joiningDate: e.target.value })} className="input-field" />
                  </div>
                </div>
              </div>

              <div className="mt-6 flex gap-3">
                <button type="button" onClick={handleCloseDialog} className="flex-1 btn-secondary">Cancel</button>
                <button type="submit" disabled={createMutation.isPending || updateMutation.isPending} className="flex-1 btn-primary disabled:opacity-50 disabled:cursor-not-allowed">
                  {createMutation.isPending || updateMutation.isPending ? (
                    <span className="flex items-center justify-center gap-2">
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      Saving...
                    </span>
                  ) : (
                    selectedTeacher ? 'Update Teacher' : 'Create Teacher'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Teacher Assignments Dialog */}
      <TeacherAssignmentsDialog
        teacher={assignmentTeacher}
        open={openAssignmentsDialog}
        onClose={() => {
          setOpenAssignmentsDialog(false);
          setAssignmentTeacher(null);
        }}
      />
    </div>
  );
}

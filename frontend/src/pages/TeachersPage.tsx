import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { teacherApi, type CreateTeacherDto, type UpdateTeacherDto, type Teacher } from '../services/api';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';
import { TeacherAssignmentsDialog } from '../components/teachers/TeacherAssignmentsDialog';
import { ImageCropModal } from '../components/common/ImageCropModal';
import { formatDate } from '../utils/dateFormat';

export function TeachersPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [rowsPerPage] = useState(10);
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedTeacher, setSelectedTeacher] = useState<Teacher | null>(null);
  const [openAssignmentsDialog, setOpenAssignmentsDialog] = useState(false);
  const [assignmentTeacher, setAssignmentTeacher] = useState<Teacher | null>(null);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [cropModalOpen, setCropModalOpen] = useState(false);
  const [tempImageFile, setTempImageFile] = useState<File | null>(null);
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
    queryKey: ['teachers', page + 1, rowsPerPage],
    queryFn: () => teacherApi.getAll({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
      isActive: true, // Only show active teachers
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

  const uploadImageMutation = useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) =>
      teacherApi.uploadImage(id, file),
    onSuccess: () => {
      toast.success('Image uploaded successfully!');
      queryClient.invalidateQueries({ queryKey: ['teachers'] });
      setImageFile(null);
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to upload image');
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
      // Set image preview if image exists
      if (teacher.imagePath) {
        setImagePreview(`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${teacher.imagePath}`);
      } else {
        setImagePreview(null);
      }
      setImageFile(null);
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
      setImagePreview(null);
      setImageFile(null);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedTeacher(null);
    setImageFile(null);
    setImagePreview(null);
    setTempImageFile(null);
    setCropModalOpen(false);
  };

  const handleImageSelect = (file: File) => {
    setTempImageFile(file);
    setCropModalOpen(true);
  };

  const handleImageCropDone = (croppedFile: File) => {
    setImageFile(croppedFile);
    // Create preview
    const reader = new FileReader();
    reader.onloadend = () => {
      setImagePreview(reader.result as string);
    };
    reader.readAsDataURL(croppedFile);
    setCropModalOpen(false);
    setTempImageFile(null);
  };

  const handleCropCancel = () => {
    setCropModalOpen(false);
    setTempImageFile(null);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const submitForm = (teacherId: string) => {
      if (imageFile) {
        uploadImageMutation.mutate({ id: teacherId, file: imageFile });
      }
    };

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
      }, {
        onSuccess: () => submitForm(selectedTeacher.id)
      });
    } else {
      createMutation.mutate(formData, {
        onSuccess: (newTeacher) => submitForm(newTeacher.id)
      });
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
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Teacher Management
              </h1>
              <p className="text-gray-600 mt-2">Create and manage all teacher information</p>
            </div>
            <button
              onClick={() => handleOpenDialog()}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Add Teacher
            </button>
          </div>

          {/* Table */}
          {!isLoading && teachersData?.items && teachersData.items.length > 0 ? (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                    <tr>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Teacher</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Contact</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Experience</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Joining Date</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Status</th>
                      <th className="px-6 py-4 text-right text-sm font-bold text-gray-900">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {teachersData?.items.map((teacher) => (
                      <tr key={teacher.id} className="hover:bg-blue-50 transition-colors duration-200">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex items-center gap-3">
                            {teacher.imagePath ? (
                              <div className="flex-shrink-0 h-10 w-10 rounded-full overflow-hidden shadow-md bg-gray-100">
                                <img
                                  src={`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${teacher.imagePath}`}
                                  alt={`${teacher.firstName} ${teacher.lastName}`}
                                  className="w-full h-full object-cover"
                                />
                              </div>
                            ) : (
                              <div className="flex-shrink-0 h-10 w-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-bold text-sm shadow-md">
                                {teacher.firstName[0]}{teacher.lastName[0]}
                              </div>
                            )}
                            <div>
                              <div className="text-sm font-bold text-gray-900">{teacher.firstName} {teacher.lastName}</div>
                              <div className="text-xs text-gray-600">{teacher.qualification || 'No qualification'}</div>
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-gray-900">{teacher.email}</div>
                          <div className="text-xs text-gray-600">{teacher.phone || 'No phone'}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-bold text-gray-900">{teacher.experienceYears}</div>
                          <div className="text-xs text-gray-600">years</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-700">
                          {formatDate(teacher.joiningDate)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <button
                            onClick={() => handleToggleActive(teacher)}
                            className={`px-3 py-1 rounded-full text-xs font-semibold transition-all ${
                              teacher.isActive
                                ? 'bg-green-100 text-green-700 hover:bg-green-200'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                          >
                            {teacher.isActive ? '✓ Active' : '○ Inactive'}
                          </button>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right">
                          <div className="flex items-center justify-end gap-1">
                            <button
                              onClick={() => {
                                setAssignmentTeacher(teacher);
                                setOpenAssignmentsDialog(true);
                              }}
                              className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                              title="Manage assignments"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                              </svg>
                            </button>
                            <button
                              onClick={() => handleOpenDialog(teacher)}
                              className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                              title="Edit"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                              </svg>
                            </button>
                            <button
                              onClick={() => handleDelete(teacher.id)}
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
                      className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      Previous
                    </button>
                    <span className="text-sm text-gray-700">Page {page + 1} of {totalPages}</span>
                    <button
                      onClick={() => setPage(Math.min(totalPages - 1, page + 1))}
                      disabled={page >= totalPages - 1}
                      className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      Next
                    </button>
                  </div>
                </div>
              )}
            </div>
          ) : isLoading ? (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden">
              <LoadingSkeleton rows={rowsPerPage} type="table" />
            </div>
          ) : (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden">
              <div className="flex flex-col items-center justify-center py-12">
                <div className="w-16 h-16 bg-gradient-to-br from-blue-100 to-blue-50 rounded-full flex items-center justify-center mb-4">
                  <svg className="w-8 h-8 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
                  </svg>
                </div>
                <h3 className="text-gray-900 font-medium">No teachers yet</h3>
                <p className="text-gray-600 mt-1">Get started by adding a teacher</p>
              </div>
            </div>
          )}
        </div>
      </div>

      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4 animate-fade-in" role="dialog" aria-modal="true" aria-labelledby="dialog-title">
          <div className="bg-white rounded-2xl shadow-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto animate-slide-up">
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between rounded-t-2xl">
              <div>
                <h2 id="dialog-title" className="text-2xl font-bold text-gray-900">{selectedTeacher ? 'Edit Teacher' : 'Add New Teacher'}</h2>
                <p className="text-sm text-gray-600 mt-1">{selectedTeacher ? 'Update teacher information' : 'Fill in the details to create a new teacher'}</p>
              </div>
              <button onClick={handleCloseDialog} className="text-gray-400 hover:text-gray-600 transition-colors" aria-label="Close dialog">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6">
              <div className="space-y-6">
                {/* Account Information Section */}
                <div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-4 pb-2 border-b border-gray-200">Account Information</h3>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">User ID <span className="text-red-500">*</span></label>
                    <input type="text" value={formData.userId} onChange={(e) => setFormData({ ...formData, userId: e.target.value })} required disabled={!!selectedTeacher} className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed" placeholder="Enter user ID" />
                  </div>
                </div>

                {/* Personal Information Section */}
                <div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-4 pb-2 border-b border-gray-200">Personal Information</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">First Name <span className="text-red-500">*</span></label>
                      <input type="text" value={formData.firstName} onChange={(e) => setFormData({ ...formData, firstName: e.target.value })} required className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" placeholder="John" />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Last Name <span className="text-red-500">*</span></label>
                      <input type="text" value={formData.lastName} onChange={(e) => setFormData({ ...formData, lastName: e.target.value })} required className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" placeholder="Doe" />
                    </div>
                    <div className="col-span-2">
                      <label className="block text-sm font-medium text-gray-700 mb-1">Email <span className="text-red-500">*</span></label>
                      <input type="email" value={formData.email} onChange={(e) => setFormData({ ...formData, email: e.target.value })} required className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" placeholder="john.doe@school.com" />
                    </div>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
                      <input type="tel" value={formData.phone} onChange={(e) => setFormData({ ...formData, phone: e.target.value })} className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" placeholder="+1 (555) 000-0000" />
                    </div>
                  </div>
                </div>

                {/* Professional Information Section */}
                <div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-4 pb-2 border-b border-gray-200">Professional Information</h3>
                  <div className="space-y-4">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Qualification</label>
                      <textarea value={formData.qualification} onChange={(e) => setFormData({ ...formData, qualification: e.target.value })} className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none" rows={3} placeholder="e.g., M.Ed in Mathematics, B.Sc in Physics" />
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Experience (years)</label>
                        <input type="number" value={formData.experienceYears} onChange={(e) => setFormData({ ...formData, experienceYears: parseInt(e.target.value) || 0 })} className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" min="0" placeholder="5" />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Joining Date</label>
                        <input type="date" value={formData.joiningDate} onChange={(e) => setFormData({ ...formData, joiningDate: e.target.value })} className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" />
                      </div>
                    </div>
                  </div>
                </div>

                {/* Image Upload Section */}
                <div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-4 pb-2 border-b border-gray-200">Profile Image</h3>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label htmlFor="image" className="block text-sm font-medium text-gray-700 mb-2">Upload Image</label>
                      <div className="border-2 border-dashed border-gray-300 rounded-lg p-4 text-center hover:border-blue-400 transition cursor-pointer">
                        <input
                          id="image"
                          type="file"
                          accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                          onChange={(e) => {
                            const file = e.target.files?.[0];
                            if (file) {
                              // Check file size (5MB)
                              if (file.size > 5 * 1024 * 1024) {
                                toast.error('Image size must be less than 5MB');
                                return;
                              }
                              handleImageSelect(file);
                            }
                          }}
                          className="hidden"
                        />
                        <label htmlFor="image" className="cursor-pointer">
                          <svg className="w-8 h-8 mx-auto text-gray-400 mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                          </svg>
                          <p className="text-sm font-medium text-gray-700">Click to upload</p>
                          <p className="text-xs text-gray-500">PNG, JPG, GIF, WebP (Max 5MB)</p>
                        </label>
                      </div>
                    </div>
                    <div>
                      {imagePreview ? (
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Preview</label>
                          <div className="rounded-lg overflow-hidden bg-gray-100 flex items-center justify-center h-40">
                            <img
                              src={imagePreview}
                              alt="Preview"
                              className="max-w-full max-h-full object-contain"
                            />
                          </div>
                          <button
                            type="button"
                            onClick={() => {
                              setImageFile(null);
                              setImagePreview(null);
                            }}
                            className="mt-2 w-full text-sm px-3 py-2 border border-red-300 text-red-700 rounded-lg hover:bg-red-50 transition"
                          >
                            Remove Image
                          </button>
                        </div>
                      ) : (
                        <div className="h-40 rounded-lg bg-gray-100 border-2 border-gray-200 flex items-center justify-center">
                          <p className="text-sm text-gray-500 text-center">Image preview will appear here</p>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              <div className="mt-6 flex gap-3 border-t border-gray-200 pt-6">
                <button type="button" onClick={handleCloseDialog} className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition">Cancel</button>
                <button type="submit" disabled={createMutation.isPending || updateMutation.isPending} className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed">
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

      {/* Image Crop Modal */}
      <ImageCropModal
        isOpen={cropModalOpen}
        imageFile={tempImageFile}
        onCropDone={handleImageCropDone}
        onCancel={handleCropCancel}
      />

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

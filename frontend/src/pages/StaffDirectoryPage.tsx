import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { StaffApi, departmentApi, type CreateStaffDto, type UpdateStaffDto, type Staff } from '../services/api';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';
import { StaffAssignmentsDialog } from '../components/Staffs/StaffAssignmentsDialog';
import { ImageCropModal } from '../components/common/ImageCropModal';
import { formatDate } from '../utils/dateFormat';

export function StaffDirectoryPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [rowsPerPage] = useState(10);
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedStaff, setSelectedStaff] = useState<Staff | null>(null);
  const [openAssignmentsDialog, setOpenAssignmentsDialog] = useState(false);
  const [assignmentStaff, setAssignmentStaff] = useState<Staff | null>(null);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [cropModalOpen, setCropModalOpen] = useState(false);
  const [tempImageFile, setTempImageFile] = useState<File | null>(null);
  const [formData, setFormData] = useState<CreateStaffDto>({
    userId: '',
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    designation: '',
    roleType: 4, // Default to Teacher
    departmentId: '',
    qualification: '',
    experienceYears: 0,
    joiningDate: new Date().toISOString().split('T')[0],
  });

  const { data: departmentsData } = useQuery({
    queryKey: ['departments'],
    queryFn: () => departmentApi.getAll(), // Corrected: takes no params or string search term
  });

  const { data: StaffsData, isLoading } = useQuery({
    queryKey: ['Staffs', page + 1, rowsPerPage],
    queryFn: () => StaffApi.getAll({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
      isActive: true, // Only show active Staffs
    }),
  });

  const createMutation = useMutation({
    mutationFn: StaffApi.create,
    onSuccess: () => {
      toast.success('Staff created successfully!');
      queryClient.invalidateQueries({ queryKey: ['Staffs'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to create Staff');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateStaffDto }) =>
      StaffApi.update(id, data),
    onSuccess: () => {
      toast.success('Staff updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['Staffs'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to update Staff');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: StaffApi.delete,
    onSuccess: () => {
      toast.success('Staff deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['Staffs'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to delete Staff');
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      isActive ? StaffApi.deactivate(id) : StaffApi.activate(id),
    onSuccess: () => {
      toast.success('Staff status updated!');
      queryClient.invalidateQueries({ queryKey: ['Staffs'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to update status');
    },
  });

  const uploadImageMutation = useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) =>
      StaffApi.uploadImage(id, file),
    onSuccess: () => {
      toast.success('Image uploaded successfully!');
      queryClient.invalidateQueries({ queryKey: ['Staffs'] });
      setImageFile(null);
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to upload image');
    },
  });

  const handleOpenDialog = (Staff?: Staff) => {
    if (Staff) {
      setSelectedStaff(Staff);
      setFormData({
        userId: Staff.userId,
        firstName: Staff.firstName,
        lastName: Staff.lastName,
        email: Staff.email,
        phone: Staff.phone || '',
        designation: Staff.designation || '',
        roleType: Staff.roleType || 4,
        departmentId: Staff.departmentId || '',
        qualification: Staff.qualification || '',
        experienceYears: Staff.experienceYears,
        joiningDate: Staff.joiningDate.split('T')[0],
      });
      // Set image preview if image exists
      if (Staff.imagePath) {
        setImagePreview(`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${Staff.imagePath}`);
      } else {
        setImagePreview(null);
      }
      setImageFile(null);
    } else {
      setSelectedStaff(null);
      setFormData({
        userId: '',
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        designation: '',
        roleType: 4,
        departmentId: '',
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
    setSelectedStaff(null);
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

    const submitForm = (StaffId: string) => {
      if (imageFile) {
        uploadImageMutation.mutate({ id: StaffId, file: imageFile });
      }
    };

    if (selectedStaff) {
      updateMutation.mutate({
        id: selectedStaff.id,
        data: {
          id: selectedStaff.id,
          userId: formData.userId,
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phone: formData.phone || '',
          designation: formData.designation,
          roleType: formData.roleType,
          departmentId: formData.departmentId || undefined,
          qualification: formData.qualification || '',
          experienceYears: formData.experienceYears,
          joiningDate: formData.joiningDate,
          isActive: selectedStaff.isActive,
        },
      }, {
        onSuccess: () => submitForm(selectedStaff.id)
      });
    } else {
      createMutation.mutate({
        ...formData,
        departmentId: formData.departmentId || undefined,
      }, {
        onSuccess: (newStaff) => submitForm(newStaff.id)
      });
    }
  };

  const handleDelete = (id: string) => {
    if (confirm('Are you sure you want to delete this Staff?')) {
      deleteMutation.mutate(id);
    }
  };

  const handleToggleActive = (Staff: Staff) => {
    toggleActiveMutation.mutate({ id: Staff.id, isActive: Staff.isActive });
  };

  const totalPages = Math.ceil((StaffsData?.totalCount || 0) / rowsPerPage);

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Staff Management
              </h1>
              <p className="text-gray-600 mt-2">Create and manage all Staff information</p>
            </div>
            <button
              onClick={() => handleOpenDialog()}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Add Staff
            </button>
          </div>

          {/* Table */}
          {!isLoading && StaffsData?.items && StaffsData.items.length > 0 ? (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                    <tr>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Staff</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Contact</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Experience</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Joining Date</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Status</th>
                      <th className="px-6 py-4 text-right text-sm font-bold text-gray-900">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {StaffsData?.items.map((Staff) => (
                      <tr key={Staff.id} className="hover:bg-blue-50 transition-colors duration-200">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex items-center gap-3">
                            {Staff.imagePath ? (
                              <div className="flex-shrink-0 h-10 w-10 rounded-full overflow-hidden shadow-md bg-gray-100">
                                <img
                                  src={`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${Staff.imagePath}`}
                                  alt={`${Staff.firstName} ${Staff.lastName}`}
                                  className="w-full h-full object-cover"
                                />
                              </div>
                            ) : (
                              <div className="flex-shrink-0 h-10 w-10 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-bold text-sm shadow-md">
                                {Staff.firstName[0]}{Staff.lastName[0]}
                              </div>
                            )}
                            <div>
                              <div className="text-sm font-bold text-gray-900">{Staff.firstName} {Staff.lastName}</div>
                              <div className="text-xs font-semibold text-blue-600">{Staff.roleType === 4 ? (Staff.departmentName ? `${Staff.designation} (${Staff.departmentName})` : Staff.designation) : Staff.designation}</div>
                              <div className="text-xs text-gray-500">{Staff.qualification || 'No qualification'}</div>
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-gray-900">{Staff.email}</div>
                          <div className="text-xs text-gray-600">{Staff.phone || 'No phone'}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-bold text-gray-900">{Staff.experienceYears}</div>
                          <div className="text-xs text-gray-600">years</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-700">
                          {formatDate(Staff.joiningDate)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <button
                            onClick={() => handleToggleActive(Staff)}
                            className={`px-3 py-1 rounded-full text-xs font-semibold transition-all ${
                              Staff.isActive
                                ? 'bg-green-100 text-green-700 hover:bg-green-200'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                          >
                            {Staff.isActive ? '✓ Active' : '○ Inactive'}
                          </button>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right">
                          <div className="flex items-center justify-end gap-1">
                            <button
                              onClick={() => {
                                setAssignmentStaff(Staff);
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
                              onClick={() => handleOpenDialog(Staff)}
                              className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                              title="Edit"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                              </svg>
                            </button>
                            <button
                              onClick={() => handleDelete(Staff.id)}
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

              {StaffsData && StaffsData.totalCount > 0 && (
                <div className="bg-gray-50 px-6 py-4 border-t border-gray-200 flex items-center justify-between">
                  <div className="text-sm text-gray-700">
                    Showing <span className="font-medium">{page * rowsPerPage + 1}</span> to{' '}
                    <span className="font-medium">{Math.min((page + 1) * rowsPerPage, StaffsData.totalCount)}</span> of{' '}
                    <span className="font-medium">{StaffsData.totalCount}</span> Staffs
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
                <h3 className="text-gray-900 font-medium">No Staffs yet</h3>
                <p className="text-gray-600 mt-1">Get started by adding a Staff</p>
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
                <h2 id="dialog-title" className="text-2xl font-bold text-gray-900">{selectedStaff ? 'Edit Staff' : 'Add New Staff'}</h2>
                <p className="text-sm text-gray-600 mt-1">{selectedStaff ? 'Update Staff information' : 'Fill in the details to create a new Staff'}</p>
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
                    <input type="text" value={formData.userId} onChange={(e) => setFormData({ ...formData, userId: e.target.value })} required disabled={!!selectedStaff} className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed" placeholder="Enter user ID" />
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
                      <label className="block text-sm font-medium text-gray-700 mb-1">Phone <span className="text-red-500">*</span> <span className="text-[10px] text-gray-500">(E.164 e.g. +1234567890)</span></label>
                      <input type="tel" value={formData.phone} onChange={(e) => setFormData({ ...formData, phone: e.target.value })} required className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" placeholder="+1234567890" />
                    </div>
                  </div>
                </div>

                {/* Professional Information Section */}
                <div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-4 pb-2 border-b border-gray-200">Professional Information</h3>
                  <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Designation <span className="text-red-500">*</span></label>
                        <input 
                          type="text" 
                          value={formData.designation} 
                          onChange={(e) => setFormData({ ...formData, designation: e.target.value })} 
                          required 
                          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent" 
                          placeholder="e.g. Senior Teacher" 
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Role <span className="text-red-500">*</span></label>
                        <select 
                          value={formData.roleType} 
                          onChange={(e) => setFormData({ ...formData, roleType: parseInt(e.target.value) })} 
                          required
                          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        >
                          <option value={1}>Admin</option>
                          <option value={2}>Accountant</option>
                          <option value={3}>Clerk</option>
                          <option value={4}>Teacher</option>
                        </select>
                      </div>
                      <div className="col-span-2">
                        <label className="block text-sm font-medium text-gray-700 mb-1">Department</label>
                        <select 
                          value={formData.departmentId} 
                          onChange={(e) => setFormData({ ...formData, departmentId: e.target.value })} 
                          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        >
                          <option value="">No Department / General</option>
                          {departmentsData?.map((dept: any) => (
                            <option key={dept.id} value={dept.id}>{dept.name}</option>
                          ))}
                        </select>
                        <p className="text-xs text-gray-500 mt-1">Primarily for Teachers and HODs</p>
                      </div>
                    </div>
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
                    selectedStaff ? 'Update Staff' : 'Create Staff'
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

      {/* Staff Assignments Dialog */}
      <StaffAssignmentsDialog
        Staff={assignmentStaff}
        open={openAssignmentsDialog}
        onClose={() => {
          setOpenAssignmentsDialog(false);
          setAssignmentStaff(null);
        }}
      />
    </div>
  );
}

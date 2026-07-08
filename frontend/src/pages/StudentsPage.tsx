import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { studentApi, classApi, type CreateStudentDto, type UpdateStudentDto, type Student } from '../services/api';
import { useDebounce } from '../hooks/useDebounce';
import { transportService } from '../services/transportService';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';
import { ImageCropModal } from '../components/common/ImageCropModal';
import { CameraCaptureModal } from '../components/common/CameraCaptureModal';

export function StudentsPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [rowsPerPage] = useState(10);
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [sectionDialogOpen, setSectionDialogOpen] = useState(false);
  const [studentForSection, setStudentForSection] = useState<Student | null>(null);
  const [selectedClassId, setSelectedClassId] = useState('');
  const [selectedSectionId, setSelectedSectionId] = useState('');
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [cropModalOpen, setCropModalOpen] = useState(false);
  const [tempImageFile, setTempImageFile] = useState<File | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500);
  const [downloadingStudentId, setDownloadingStudentId] = useState<string | null>(null);
  const [cameraModalOpen, setCameraModalOpen] = useState(false);
  const [formData, setFormData] = useState<CreateStudentDto>({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: '',
    address: '',
    city: '',
    state: '',
    postalCode: '',
    guardianName: '',
    guardianPhone: '',
    guardianEmail: '',
    enrollmentDate: new Date().toISOString().split('T')[0],
  });

  const { data: studentsData, isLoading } = useQuery({
    queryKey: ['students', page + 1, rowsPerPage, debouncedSearchTerm],
    queryFn: () => studentApi.getAll({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
      isActive: true, // Only show active students
      searchTerm: debouncedSearchTerm || undefined,
    }),
  });

  // Query for classes
  const { data: classesData } = useQuery({
    queryKey: ['classes'],
    queryFn: () => classApi.getAll({ isActive: true }),
    enabled: sectionDialogOpen,
  });

  // Query for sections based on selected class
  const { data: sectionsData } = useQuery({
    queryKey: ['sections', selectedClassId],
    queryFn: () => classApi.getSectionsByClass(selectedClassId),
    enabled: !!selectedClassId && sectionDialogOpen,
  });
  
  // Transport Status Query
  const { data: transportStatus, isLoading: isLoadingTransport } = useQuery({
    queryKey: ['student-transport', selectedStudent?.id],
    queryFn: () => transportService.getStudentStatus(selectedStudent!.id),
    enabled: !!selectedStudent && openDialog,
  });

  // Query for student's current section
  const { data: currentSectionData } = useQuery({
    queryKey: ['student-section', studentForSection?.id],
    queryFn: () => classApi.getStudentCurrentSection(studentForSection!.id),
    enabled: !!studentForSection && sectionDialogOpen,
  });

  const createMutation = useMutation({
    mutationFn: studentApi.create,
    onSuccess: () => {
      toast.success('Student created successfully!');
      queryClient.invalidateQueries({ queryKey: ['students'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to create student');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateStudentDto }) =>
      studentApi.update(id, data),
    onSuccess: () => {
      toast.success('Student updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['students'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to update student');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: studentApi.delete,
    onSuccess: () => {
      toast.success('Student deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['students'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to delete student');
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      isActive ? studentApi.deactivate(id) : studentApi.activate(id),
    onSuccess: () => {
      toast.success('Student status updated!');
      queryClient.invalidateQueries({ queryKey: ['students'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to update status');
    },
  });

  const assignSectionMutation = useMutation({
    mutationFn: ({ studentId, sectionId }: { studentId: string; sectionId: string }) =>
      classApi.moveStudentToSection(studentId, sectionId),
    onSuccess: () => {
      toast.success('Student assigned to section successfully!');
      queryClient.invalidateQueries({ queryKey: ['students'] });
      queryClient.invalidateQueries({ queryKey: ['student-section'] });
      handleCloseSectionDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to assign section');
    },
  });

  const uploadImageMutation = useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) =>
      studentApi.uploadImage(id, file),
    onSuccess: () => {
      toast.success('Image uploaded successfully!');
      queryClient.invalidateQueries({ queryKey: ['students'] });
      setImageFile(null);
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to upload image');
    },
  });

  const handleOpenDialog = (student?: Student) => {
    if (student) {
      setSelectedStudent(student);
      
      const newFormData = {
        firstName: student.firstName,
        lastName: student.lastName,
        email: student.email,
        phoneNumber: student.phone || '',
        dateOfBirth: student.dateOfBirth.split('T')[0],
        address: student.address || '',
        city: student.city || '',
        state: student.state || '',
        postalCode: student.postalCode || '',
        guardianName: student.parentName || '',
        guardianPhone: student.parentPhone || '',
        guardianEmail: student.parentEmail || '',
        enrollmentDate: student.enrollmentDate.split('T')[0],
      };
      
      setFormData(newFormData);
      
      // Set image preview if image exists
      if (student.imagePath) {
        const baseUrl = (import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '');
        const imageUrl = `${baseUrl}${student.imagePath}`;
        setImagePreview(imageUrl);
      } else {
        setImagePreview(null);
      }
      setImageFile(null);
    } else {
      setSelectedStudent(null);
      setFormData({
        firstName: '',
        lastName: '',
        email: '',
        phoneNumber: '',
        dateOfBirth: '',
        address: '',
        city: '',
        state: '',
        postalCode: '',
        guardianName: '',
        guardianPhone: '',
        guardianEmail: '',
        enrollmentDate: new Date().toISOString().split('T')[0],
      });
      setImagePreview(null);
      setImageFile(null);
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedStudent(null);
    setImageFile(null);
    setImagePreview(null);
    setTempImageFile(null);
    setCropModalOpen(false);
    setCameraModalOpen(false);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    const submitForm = (studentId: string) => {
      if (imageFile) {
        uploadImageMutation.mutate({ id: studentId, file: imageFile });
      }
    };

    if (selectedStudent) {
      updateMutation.mutate({
        id: selectedStudent.id,
        data: {
          id: selectedStudent.id,
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phoneNumber: formData.phoneNumber || '',
          dateOfBirth: formData.dateOfBirth,
          address: formData.address || '',
          city: formData.city || '',
          state: formData.state || '',
          postalCode: formData.postalCode || '',
          guardianName: formData.guardianName || '',
          guardianPhone: formData.guardianPhone || '',
          guardianEmail: formData.guardianEmail || '',
          isActive: selectedStudent.isActive,
        },
      }, {
        onSuccess: () => submitForm(selectedStudent.id)
      });
    } else {
      createMutation.mutate(formData, {
        onSuccess: (newStudent) => submitForm(newStudent.id)
      });
    }
  };

  const handleDelete = (id: string) => {
    if (confirm('Are you sure you want to delete this student?')) {
      deleteMutation.mutate(id);
    }
  };

  const handleToggleActive = (student: Student) => {
    toggleActiveMutation.mutate({ id: student.id, isActive: student.isActive });
  };

  const handleOpenSectionDialog = (student: Student) => {
    setStudentForSection(student);
    setSelectedClassId('');
    setSelectedSectionId('');
    setSectionDialogOpen(true);
  };

  const handleCloseSectionDialog = () => {
    setSectionDialogOpen(false);
    setStudentForSection(null);
    setSelectedClassId('');
    setSelectedSectionId('');
  };

  const handleAssignSection = () => {
    if (studentForSection && selectedSectionId) {
      assignSectionMutation.mutate({
        studentId: studentForSection.id,
        sectionId: selectedSectionId,
      });
    }
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

  const handleChangePage = (newPage: number) => {
    setPage(newPage);
  };

  const handleDownloadRegistrationForm = async (studentId: string) => {
    try {
      setDownloadingStudentId(studentId);
      const response = await studentApi.downloadRegistrationFormPdf(studentId);
      
      const blob = new Blob([response], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `registration-form-${studentId}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      toast.success('Registration form downloaded successfully!');
    } catch (error) {
      console.error('Failed to download registration form:', error);
      toast.error('Failed to download registration form');
    } finally {
      setDownloadingStudentId(null);
    }
  };

  const totalPages = studentsData ? Math.ceil(studentsData.totalCount / rowsPerPage) : 0;
  const getInitials = (firstName: string, lastName: string) => {
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
  };

  const getAvatarColor = (index: number) => {
    const colors = ['bg-blue-500', 'bg-purple-500', 'bg-pink-500', 'bg-green-500', 'bg-orange-500', 'bg-red-500'];
    return colors[index % colors.length];
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Student Management
              </h1>
              <p className="text-gray-600 mt-2">Create and manage all student information</p>
            </div>
            <button
              onClick={() => handleOpenDialog()}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Add Student
            </button>
          </div>

          {/* Search bar */}
          <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-4">
            <div className="relative">
              <span className="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
                <svg className="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </span>
              <input
                type="text"
                placeholder="Search students by name, enrollment number, phone number..."
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setPage(0); // Reset page to 0 when search changes
                }}
                className="block w-full pl-10 pr-3 py-3 border border-gray-200 rounded-xl text-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all bg-gray-50/50"
              />
            </div>
          </div>

          {/* Table */}
          {!isLoading && studentsData?.items && studentsData.items.length > 0 ? (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                    <tr>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Student</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Contact</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Class & Section</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Enrollment Date</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Status</th>
                      <th className="px-6 py-4 text-right text-sm font-bold text-gray-900">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {studentsData.items.map((student, index) => (
                      <tr key={student.id} className="hover:bg-blue-50 transition-colors duration-200">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex items-center gap-3">
                            {student.imagePath ? (
                              <div className="flex-shrink-0 h-10 w-10 rounded-full overflow-hidden shadow-md bg-gray-100">
                                <img
                                  src={`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${student.imagePath}`}
                                  alt={`${student.firstName} ${student.lastName}`}
                                  className="w-full h-full object-cover"
                                />
                              </div>
                            ) : (
                              <div className={`flex-shrink-0 h-10 w-10 ${getAvatarColor(index)} rounded-full flex items-center justify-center text-white font-bold text-sm shadow-md`}>
                                {getInitials(student.firstName, student.lastName)}
                              </div>
                            )}
                            <div>
                              <div className="text-sm font-bold text-gray-900">{student.firstName} {student.lastName}</div>
                              <div className="text-xs text-gray-600">{student.enrollmentNumber}</div>
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-gray-900">{student.email}</div>
                          <div className="text-xs text-gray-600">{student.phone || 'No phone'}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          {student.currentClassName && student.currentSectionName ? (
                            <div className="flex items-center gap-2">
                              <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-800">
                                {student.currentClassName} - {student.currentSectionName}
                              </span>
                            </div>
                          ) : (
                            <span className="text-xs text-gray-500 italic">Not assigned</span>
                          )}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-700">
                          {new Date(student.enrollmentDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <button
                            onClick={() => handleToggleActive(student)}
                            className={`px-3 py-1 rounded-full text-xs font-semibold transition-all ${
                              student.isActive
                                ? 'bg-green-100 text-green-700 hover:bg-green-200'
                                : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                            }`}
                          >
                            {student.isActive ? '✓ Active' : '○ Inactive'}
                          </button>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right">
                          <div className="flex items-center justify-end gap-1">
                            <button
                              onClick={() => handleOpenSectionDialog(student)}
                              className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                              title="Assign section"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                              </svg>
                            </button>
                            <button
                              onClick={() => handleDownloadRegistrationForm(student.id)}
                              disabled={downloadingStudentId === student.id}
                              className="p-2 text-green-600 hover:bg-green-100 rounded-lg transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
                              title="Print Registration Form"
                            >
                              {downloadingStudentId === student.id ? (
                                <svg className="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
                                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                </svg>
                              ) : (
                                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                                </svg>
                              )}
                            </button>
                            <button
                              onClick={() => handleOpenDialog(student)}
                              className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                              title="Edit"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                              </svg>
                            </button>
                            <button
                              onClick={() => handleDelete(student.id)}
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

              {studentsData && studentsData.totalCount > 0 && (
                <div className="bg-gray-50 px-6 py-4 border-t border-gray-200 flex items-center justify-between">
                  <div className="text-sm text-gray-700">
                    Showing <span className="font-medium">{page * rowsPerPage + 1}</span> to{' '}
                    <span className="font-medium">{Math.min((page + 1) * rowsPerPage, studentsData.totalCount)}</span> of{' '}
                    <span className="font-medium">{studentsData.totalCount}</span> students
                  </div>
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() => handleChangePage(page - 1)}
                      disabled={page === 0}
                      className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                      Previous
                    </button>
                    <span className="text-sm text-gray-700">Page {page + 1} of {totalPages}</span>
                    <button
                      onClick={() => handleChangePage(page + 1)}
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
                <h3 className="text-gray-900 font-medium">No students yet</h3>
                <p className="text-gray-600 mt-1">Get started by adding a student</p>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Add/Edit Dialog */}
      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div key={selectedStudent?.id || 'new'} className="bg-white rounded-2xl shadow-2xl w-full max-w-7xl max-h-[95vh] overflow-y-auto">
            <div className="px-8 py-6 border-b-2 border-gray-200 sticky top-0 bg-gradient-to-r from-blue-600 to-blue-700 rounded-t-2xl">
              <h2 className="text-3xl font-bold text-white flex items-center gap-3">
                <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d={selectedStudent ? "M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" : "M12 4v16m8-8H4"} />
                </svg>
                {selectedStudent ? 'Edit Student' : 'Add New Student'}
              </h2>
              <p className="text-blue-100 mt-2 text-sm">
                {selectedStudent ? 'Update student information below' : 'Fill in all the details to create a new student record'}
              </p>
            </div>
            <form onSubmit={handleSubmit} className="p-8 space-y-6 bg-gray-50">{/* Personal Information Section */}
              <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 hover:shadow-lg transition-shadow">
                <h3 className="text-xl font-bold text-gray-800 mb-5 flex items-center gap-2">
                  <svg className="w-6 h-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                  Personal Information
                </h3>
                <div className="grid grid-cols-3 gap-5">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">First Name</label>
                    <input
                      type="text"
                      required
                      value={formData.firstName}
                      onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="John"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Last Name</label>
                    <input
                      type="text"
                      required
                      value={formData.lastName}
                      onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="Doe"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
                    <input
                      type="email"
                      required
                      value={formData.email}
                      onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="john.doe@example.com"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Phone</label>
                    <input
                      type="tel"
                      value={formData.phoneNumber}
                      onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="+1 234 567 8900"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Date of Birth</label>
                    <input
                      type="date"
                      required
                      value={formData.dateOfBirth}
                      onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    />
                  </div>
                </div>
              </div>

              {/* Parent/Guardian Information Section */}
              <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 hover:shadow-lg transition-shadow">
                <h3 className="text-xl font-bold text-gray-800 mb-5 flex items-center gap-2">
                  <svg className="w-6 h-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                  </svg>
                  Guardian Information
                </h3>
                <div className="grid grid-cols-3 gap-5">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Guardian Name</label>
                    <input
                      type="text"
                      value={formData.guardianName}
                      onChange={(e) => setFormData({ ...formData, guardianName: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="Jane Doe"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Guardian Phone</label>
                    <input
                      type="tel"
                      value={formData.guardianPhone}
                      onChange={(e) => setFormData({ ...formData, guardianPhone: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="+1 234 567 8900"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Guardian Email</label>
                    <input
                      type="email"
                      value={formData.guardianEmail}
                      onChange={(e) => setFormData({ ...formData, guardianEmail: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="jane.doe@example.com"
                    />
                  </div>
                </div>
              </div>

              {/* Transport Information Section (View Only) */}
              {selectedStudent && (
                <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 hover:shadow-lg transition-shadow">
                  <h3 className="text-xl font-bold text-gray-800 mb-5 flex items-center gap-2">
                    <svg className="w-6 h-6 text-orange-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
                    </svg>
                    Transport Details
                  </h3>
                  
                  {isLoadingTransport ? (
                    <div className="animate-pulse space-y-4">
                      <div className="h-4 bg-gray-200 rounded w-1/4"></div>
                      <div className="h-10 bg-gray-100 rounded"></div>
                    </div>
                  ) : transportStatus ? (
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                      <div className="bg-orange-50 p-4 rounded-xl border border-orange-100">
                        <div className="text-xs font-bold text-orange-600 uppercase mb-1">Assigned Route</div>
                        <div className="text-lg font-bold text-gray-900">{transportStatus.routeName}</div>
                      </div>
                      <div className="bg-blue-50 p-4 rounded-xl border border-blue-100">
                        <div className="text-xs font-bold text-blue-600 uppercase mb-1">Pick-up / Drop-off Stop</div>
                        <div className="text-lg font-bold text-gray-900">{transportStatus.stopName}</div>
                      </div>
                      <div className="bg-purple-50 p-4 rounded-xl border border-purple-100">
                        <div className="text-xs font-bold text-purple-600 uppercase mb-1">Vehicle Info</div>
                        <div className="text-lg font-bold text-gray-900">{transportStatus.vehicleReg}</div>
                        <div className="text-xs text-gray-500 mt-1">Bus / Van assigned to route</div>
                      </div>
                      
                      <div className="col-span-1 md:col-span-3 mt-2 flex items-center gap-2 text-sm text-gray-600">
                        <svg className="w-4 h-4 text-green-500" fill="currentColor" viewBox="0 0 20 20">
                          <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                        </svg>
                        Transport Subscription Active since {new Date(transportStatus.effectiveDate).toLocaleDateString()}
                      </div>
                    </div>
                  ) : (
                    <div className="bg-gray-50 border-2 border-dashed border-gray-200 rounded-xl p-8 text-center">
                      <div className="text-gray-400 mb-2 font-medium italic">No transport assignment found for this student.</div>
                      <p className="text-xs text-gray-500">You can enroll this student from the Transport Management dashboard.</p>
                    </div>
                  )}
                </div>
              )}

              {/* Address Information Section */}
              <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 hover:shadow-lg transition-shadow">
                <h3 className="text-xl font-bold text-gray-800 mb-5 flex items-center gap-2">
                  <svg className="w-6 h-6 text-purple-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  Address Information
                </h3>
                <div className="grid grid-cols-2 gap-5">
                  <div className="col-span-2">
                    <label className="block text-sm font-medium text-gray-700 mb-2">Street Address</label>
                    <input
                      type="text"
                      value={formData.address}
                      onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="123 Main Street, Apt 4B"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">City</label>
                    <input
                      type="text"
                      value={formData.city}
                      onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="New York"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">State</label>
                    <input
                      type="text"
                      value={formData.state}
                      onChange={(e) => setFormData({ ...formData, state: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="NY"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Postal Code</label>
                    <input
                      type="text"
                      value={formData.postalCode}
                      onChange={(e) => setFormData({ ...formData, postalCode: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                      placeholder="10001"
                    />
                  </div>
                </div>
              </div>

              {/* Enrollment Information Section */}
              <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 hover:shadow-lg transition-shadow">
                <h3 className="text-xl font-bold text-gray-800 mb-5 flex items-center gap-2">
                  <svg className="w-6 h-6 text-orange-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  Enrollment Information
                </h3>
                <div className="grid grid-cols-3 gap-5">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Enrollment Date</label>
                    <input
                      type="date"
                      required
                      value={formData.enrollmentDate}
                      onChange={(e) => setFormData({ ...formData, enrollmentDate: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all"
                    />
                  </div>
                  <div />
                </div>
              </div>

              {/* Image Upload Section */}
              <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 hover:shadow-lg transition-shadow">
                <h3 className="text-xl font-bold text-gray-800 mb-5 flex items-center gap-2">
                  <svg className="w-6 h-6 text-pink-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                  Profile Photograph
                </h3>
                <div className="flex flex-col md:flex-row items-center gap-6">
                  {/* Photo Avatar Preview */}
                  <div className="relative group flex-shrink-0">
                    <div className="w-32 h-32 rounded-2xl overflow-hidden bg-slate-100 border-2 border-slate-200 shadow-inner flex items-center justify-center transition-all duration-300 group-hover:border-blue-400">
                      {imagePreview ? (
                        <img
                          src={imagePreview}
                          alt="Student Profile"
                          className="w-full h-full object-cover"
                        />
                      ) : (
                        <div className="flex flex-col items-center justify-center text-slate-400">
                          <svg className="w-12 h-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                          </svg>
                          <span className="text-[10px] font-medium text-slate-400 mt-1">No Image</span>
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Upload / Capture Controls */}
                  <div className="flex-1 flex flex-col items-center md:items-start text-center md:text-left">
                    <h4 className="text-sm font-semibold text-gray-700 mb-1">Upload or capture photo</h4>
                    <p className="text-xs text-gray-500 mb-4">PNG, JPG or WebP (Max 5MB). Photo will be printed on the registration sheet.</p>
                    
                    <div className="flex flex-wrap items-center justify-center md:justify-start gap-3">
                      {/* Upload Button */}
                      <label className="flex items-center gap-2 px-4 py-2.5 bg-white border border-gray-300 rounded-xl text-sm font-semibold text-gray-700 shadow-sm hover:bg-gray-50 hover:border-gray-400 transition-all cursor-pointer">
                        <svg className="w-4 h-4 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
                        </svg>
                        Upload Photo
                        <input
                          id="image"
                          type="file"
                          accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                          onChange={(e) => {
                            const file = e.target.files?.[0];
                            if (file) {
                              if (file.size > 5 * 1024 * 1024) {
                                toast.error('Image size must be less than 5MB');
                                return;
                              }
                              handleImageSelect(file);
                            }
                          }}
                          className="hidden"
                        />
                      </label>

                      {/* Camera Button */}
                      <button
                        type="button"
                        onClick={() => setCameraModalOpen(true)}
                        className="flex items-center gap-2 px-4 py-2.5 bg-blue-50 border border-blue-200 rounded-xl text-sm font-semibold text-blue-700 shadow-sm hover:bg-blue-100 hover:border-blue-300 transition-all"
                      >
                        <svg className="w-4 h-4 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
                        </svg>
                        Take Live Photo
                      </button>

                      {/* Remove Button */}
                      {imagePreview && (
                        <button
                          type="button"
                          onClick={() => {
                            setImageFile(null);
                            setImagePreview(null);
                            const input = document.getElementById('image') as HTMLInputElement;
                            if (input) input.value = '';
                          }}
                          className="flex items-center gap-2 px-4 py-2.5 bg-red-50 border border-red-200 rounded-xl text-sm font-semibold text-red-600 shadow-sm hover:bg-red-100 hover:border-red-300 transition-all animate-fade-in"
                        >
                          <svg className="w-4 h-4 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                          </svg>
                          Remove
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              <div className="flex gap-4 pt-6 border-t-2 border-gray-200 bg-white sticky bottom-0 -mx-8 px-8 -mb-8 pb-8 rounded-b-2xl">
                <button
                  type="button"
                  onClick={handleCloseDialog}
                  className="flex-1 px-6 py-3 border-2 border-gray-300 text-gray-700 font-semibold rounded-xl hover:bg-gray-50 hover:border-gray-400 transition-all flex items-center justify-center gap-2"
                >
                  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={createMutation.isPending || updateMutation.isPending}
                  className="flex-1 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white font-semibold rounded-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg hover:shadow-xl flex items-center justify-center gap-2"
                >
                  {createMutation.isPending || updateMutation.isPending ? (
                    <>
                      <svg className="animate-spin h-5 w-5" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                      Saving...
                    </>
                  ) : (
                    <>
                      <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                      </svg>
                      {selectedStudent ? 'Update Student' : 'Create Student'}
                    </>
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Section Assignment Dialog */}
      {sectionDialogOpen && studentForSection && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">
                Assign Section
              </h2>
              <p className="text-sm text-gray-600 mt-1">
                Student: {studentForSection.firstName} {studentForSection.lastName}
              </p>
              {currentSectionData && (
                <p className="text-sm text-blue-600 mt-1">
                  Current: {currentSectionData.className} - {currentSectionData.sectionName}
                </p>
              )}
            </div>
            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Select Class</label>
                <select
                  value={selectedClassId}
                  onChange={(e) => {
                    setSelectedClassId(e.target.value);
                    setSelectedSectionId('');
                  }}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">-- Select Class --</option>
                  {classesData?.items.map((cls) => (
                    <option key={cls.id} value={cls.id}>
                      {cls.name} {cls.academicYear ? `(${cls.academicYear})` : ''}
                    </option>
                  ))}
                </select>
              </div>

              {selectedClassId && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Select Section</label>
                  <select
                    value={selectedSectionId}
                    onChange={(e) => setSelectedSectionId(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    <option value="">-- Select Section --</option>
                    {sectionsData?.map((section) => (
                      <option key={section.id} value={section.id}>
                        {section.sectionName} ({section.studentCount} students)
                      </option>
                    ))}
                  </select>
                </div>
              )}

              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={handleCloseSectionDialog}
                  className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  onClick={handleAssignSection}
                  disabled={!selectedSectionId || assignSectionMutation.isPending}
                  className="flex-1 px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white font-medium rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {assignSectionMutation.isPending ? 'Assigning...' : 'Assign Section'}
                </button>
              </div>
            </div>
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

      {/* Camera Capture Modal */}
      <CameraCaptureModal
        isOpen={cameraModalOpen}
        onCapture={(file) => {
          handleImageSelect(file);
          setCameraModalOpen(false);
        }}
        onCancel={() => setCameraModalOpen(false)}
      />
    </div>
  );
}

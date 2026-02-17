import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { studentApi, classApi, type CreateStudentDto, type UpdateStudentDto, type Student } from '../services/api';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';
import { EmptyState, NoDataIcon } from '../components/common/EmptyState';

export function StudentsPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [sectionDialogOpen, setSectionDialogOpen] = useState(false);
  const [studentForSection, setStudentForSection] = useState<Student | null>(null);
  const [selectedClassId, setSelectedClassId] = useState('');
  const [selectedSectionId, setSelectedSectionId] = useState('');
  const [formData, setFormData] = useState<CreateStudentDto>({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    dateOfBirth: '',
    parentName: '',
    parentPhone: '',
    enrollmentDate: new Date().toISOString().split('T')[0],
  });

  const { data: studentsData, isLoading } = useQuery({
    queryKey: ['students', page + 1, rowsPerPage, searchTerm],
    queryFn: () => studentApi.getAll({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
      searchTerm: searchTerm || undefined,
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

  const handleOpenDialog = (student?: Student) => {
    if (student) {
      setSelectedStudent(student);
      setFormData({
        firstName: student.firstName,
        lastName: student.lastName,
        email: student.email,
        phone: student.phone || '',
        dateOfBirth: student.dateOfBirth.split('T')[0],
        parentName: student.parentName || '',
        parentPhone: student.parentPhone || '',
        enrollmentDate: student.enrollmentDate.split('T')[0],
      });
    } else {
      setSelectedStudent(null);
      setFormData({
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        dateOfBirth: '',
        parentName: '',
        parentPhone: '',
        enrollmentDate: new Date().toISOString().split('T')[0],
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedStudent(null);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedStudent) {
      updateMutation.mutate({
        id: selectedStudent.id,
        data: {
          id: selectedStudent.id,
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phone: formData.phone || '',
          dateOfBirth: formData.dateOfBirth,
          parentName: formData.parentName || '',
          parentPhone: formData.parentPhone || '',
          enrollmentDate: formData.enrollmentDate,
          isActive: selectedStudent.isActive,
        },
      });
    } else {
      createMutation.mutate(formData);
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

  const handleChangePage = (newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
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
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50 to-purple-50 pb-8">
      {/* Header Section */}
      <div className="bg-gradient-to-r from-indigo-600 to-blue-600 shadow-lg">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="flex items-center gap-4">
            <div className="p-3 bg-white/20 rounded-xl backdrop-blur-sm">
              <svg className="w-8 h-8 text-white" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
              </svg>
            </div>
            <div>
              <h1 className="text-3xl font-bold text-white">Students Management</h1>
              <p className="text-blue-100 mt-1">Manage student information, enrollments, and details</p>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Search and Stats Section */}
        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 mb-8 hover:shadow-xl transition-shadow">
          <div className="flex flex-col lg:flex-row gap-6 items-start lg:items-center justify-between">
            <div className="flex-1 w-full lg:max-w-md">
              <label className="block text-sm font-semibold text-gray-700 mb-2">Search Students</label>
              <div className="relative">
                <input
                  type="text"
                  placeholder="Search by name, email, or ID..."
                  value={searchTerm}
                  onChange={(e) => {
                    setSearchTerm(e.target.value);
                    setPage(0);
                  }}
                  className="w-full px-4 py-3 pl-10 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all"
                />
                <svg className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
            </div>
            <button
              onClick={() => handleOpenDialog()}
              className="w-full sm:w-auto px-6 py-3 bg-gradient-to-r from-indigo-600 to-blue-600 hover:from-indigo-700 hover:to-blue-700 text-white font-semibold rounded-xl transition-all shadow-lg hover:shadow-xl transform hover:-translate-y-0.5"
            >
              + Add Student
            </button>
          </div>
        </div>

        {/* Desktop Table View */}
        <div className="hidden lg:block bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden\">
          {isLoading ? (
            <LoadingSkeleton rows={rowsPerPage} type="table" />
          ) : studentsData?.items.length === 0 ? (
            <EmptyState
              icon={<NoDataIcon />}
              title="No students found"
              description={searchTerm ? "Try adjusting your search criteria" : "Get started by adding your first student to the system"}
              action={!searchTerm ? {
                label: "Add Student",
                onClick: () => handleOpenDialog()
              } : undefined}
            />
          ) : (
            <>
              <table className="w-full">
                <thead className="bg-gradient-to-r from-indigo-600 to-blue-600">
                  <tr>
                    <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase">Student ID</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase">Name</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase">Email</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase">Phone</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase">Section</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase">Status</th>
                    <th className="px-6 py-4 text-left text-xs font-bold text-white uppercase">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {studentsData?.items.map((student, index) => (
                    <tr key={student.id} className="hover:bg-gradient-to-r hover:from-indigo-50 hover:to-blue-50 transition-all">
                      <td className="px-6 py-4 text-sm">
                        <span className="font-mono font-bold text-indigo-600 bg-indigo-50 px-3 py-1 rounded-lg">{student.enrollmentNumber}</span>
                      </td>
                      <td className="px-6 py-4 text-sm font-bold text-gray-900">
                        <div className="flex items-center gap-3">
                          <div className={`w-12 h-12 ${getAvatarColor(index)} rounded-full flex items-center justify-center text-white font-bold text-lg shadow-md`}>
                            {getInitials(student.firstName, student.lastName)}
                          </div>
                          <div>
                            <div>{student.firstName} {student.lastName}</div>
                          </div>
                        </div>
                      </td>
                      <td className="px-6 py-4 text-sm font-medium text-gray-900\">{student.email}</td>
                      <td className="px-6 py-4 text-sm text-gray-700\">{student.phone || '-'}</td>
                      <td className="px-6 py-4 text-sm">
                        <button
                          onClick={() => handleOpenSectionDialog(student)}
                          className="px-3 py-2 bg-gradient-to-r from-purple-100 to-purple-200 hover:from-purple-200 hover:to-purple-300 text-purple-700 rounded-lg transition text-xs font-bold shadow-sm hover:shadow-md"
                        >
                          📚 Assign
                        </button>
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <button
                          onClick={() => handleToggleActive(student)}
                          className={`px-3 py-1.5 rounded-full text-xs font-bold transition-all ${
                            student.isActive
                              ? 'bg-green-100 text-green-700 shadow-sm hover:shadow-md hover:bg-green-200'
                              : 'bg-gray-100 text-gray-700 shadow-sm hover:shadow-md hover:bg-gray-200'
                          }`}
                        >
                          {student.isActive ? '✓ Active' : '○ Inactive'}
                        </button>
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleOpenDialog(student)}
                            className="px-3 py-2 bg-gradient-to-r from-blue-100 to-blue-200 hover:from-blue-200 hover:to-blue-300 text-blue-700 rounded-lg transition text-xs font-bold shadow-sm hover:shadow-md"
                          >
                            ✏️ Edit
                          </button>
                          <button
                            onClick={() => handleDelete(student.id)}
                            className="px-3 py-2 bg-gradient-to-r from-red-100 to-red-200 hover:from-red-200 hover:to-red-300 text-red-700 rounded-lg transition text-xs font-bold shadow-sm hover:shadow-md"
                          >
                            🗑️ Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {/* Pagination */}
              <div className="bg-gradient-to-r from-gray-50 to-gray-100 border-t-2 border-gray-200 px-6 py-4 flex items-center justify-between">
                <div className="text-sm font-semibold text-gray-700">
                  Page <span className="font-bold text-indigo-600">{page + 1}</span> of <span className="font-bold text-indigo-600">{totalPages}</span> (Total: <span className="font-bold text-indigo-600\">{studentsData?.totalCount}</span> students)
                </div>
                <div className="flex gap-2 items-center">
                  <button
                    onClick={() => handleChangePage(page - 1)}
                    disabled={page === 0}
                    className="px-4 py-2 border-2 border-gray-300 rounded-lg hover:bg-white hover:border-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed font-semibold transition-all"
                  >
                    ← Previous
                  </button>
                  <select
                    value={rowsPerPage}
                    onChange={handleChangeRowsPerPage}
                    className="px-3 py-2 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-semibold"
                  >
                    <option value={5}>5 rows</option>
                    <option value={10}>10 rows</option>
                    <option value={25}>25 rows</option>
                    <option value={50}>50 rows</option>
                  </select>
                  <button
                    onClick={() => handleChangePage(page + 1)}
                    disabled={page >= totalPages - 1}
                    className="px-4 py-2 border-2 border-gray-300 rounded-lg hover:bg-white hover:border-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed font-semibold transition-all"
                  >
                    Next →
                  </button>
                </div>
              </div>
            </>
          )}
        </div>

        {/* Mobile Card View */}
        <div className="lg:hidden space-y-4">
          {isLoading ? (
            <div className="p-8 text-center text-gray-500">Loading students...</div>
          ) : studentsData?.items.length === 0 ? (
            <div className="p-8 text-center text-gray-500">No students found</div>
          ) : (
            <>
              {studentsData?.items.map((student, index) => (
                <div key={student.id} className="bg-white rounded-2xl shadow-lg border border-gray-100 p-5 hover:shadow-xl hover:border-indigo-200 transition-all">
                  <div className="flex items-start gap-4 mb-4">
                    <div className={`w-14 h-14 ${getAvatarColor(index)} rounded-full flex items-center justify-center text-white font-bold text-lg flex-shrink-0 shadow-md`}>
                      {getInitials(student.firstName, student.lastName)}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 mb-2">
                        <span className="font-mono text-xs font-bold text-indigo-600 bg-indigo-50 px-3 py-1 rounded-lg">{student.enrollmentNumber}</span>
                      </div>
                      <h3 className="font-bold text-gray-900">{student.firstName} {student.lastName}</h3>
                      <p className="text-xs text-gray-600 mt-1">{student.email}</p>
                    </div>
                    <button
                      onClick={() => handleToggleActive(student)}
                      className={`px-2 py-1.5 rounded-lg text-xs font-bold ${
                        student.isActive
                          ? 'bg-green-100 text-green-700 shadow-sm hover:shadow-md hover:bg-green-200'
                          : 'bg-gray-100 text-gray-700 shadow-sm hover:shadow-md hover:bg-gray-200'
                      }`}
                    >
                      {student.isActive ? '✓' : '○'}
                    </button>
                  </div>
                  <div className="text-xs text-gray-700 space-y-1.5 mb-4 bg-gray-50 p-3 rounded-lg border border-gray-200">
                    {student.phone && <p><span className="font-semibold">Phone:</span> {student.phone}</p>}
                    <p><span className="font-semibold">DOB:</span> {new Date(student.dateOfBirth).toLocaleDateString()}</p>
                    {student.parentName && <p><span className="font-semibold">Parent:</span> {student.parentName}</p>}
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleOpenSectionDialog(student)}
                      className="flex-1 px-3 py-2.5 bg-gradient-to-r from-purple-100 to-purple-200 hover:from-purple-200 hover:to-purple-300 text-purple-700 rounded-lg transition text-xs font-bold shadow-sm hover:shadow-md"
                    >
                      📚 Assign
                    </button>
                    <button
                      onClick={() => handleOpenDialog(student)}
                      className="flex-1 px-3 py-2.5 bg-gradient-to-r from-blue-100 to-blue-200 hover:from-blue-200 hover:to-blue-300 text-blue-700 rounded-lg transition text-xs font-bold shadow-sm hover:shadow-md"
                    >
                      ✏️ Edit
                    </button>
                    <button
                      onClick={() => handleDelete(student.id)}
                      className="flex-1 px-3 py-2.5 bg-gradient-to-r from-red-100 to-red-200 hover:from-red-200 hover:to-red-300 text-red-700 rounded-lg transition text-xs font-bold shadow-sm hover:shadow-md"
                    >
                      🗑️ Delete
                    </button>
                  </div>
                </div>
              ))}
              {/* Mobile Pagination */}
              <div className="flex gap-2 justify-between mt-4">
                <button
                  onClick={() => handleChangePage(page - 1)}
                  disabled={page === 0}
                  className="flex-1 px-3 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
                >
                  Previous
                </button>
                <button
                  onClick={() => handleChangePage(page + 1)}
                  disabled={page >= totalPages - 1}
                  className="flex-1 px-3 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
                >
                  Next
                </button>
              </div>
            </>
          )}
        </div>
      </div>

      {/* Add/Edit Dialog */}
      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">
                {selectedStudent ? 'Edit Student' : 'Add New Student'}
              </h2>
            </div>
            <form onSubmit={handleSubmit} className="p-6 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">First Name</label>
                  <input
                    type="text"
                    required
                    value={formData.firstName}
                    onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
                  <input
                    type="text"
                    required
                    value={formData.lastName}
                    onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
                <input
                  type="email"
                  required
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Phone</label>
                  <input
                    type="tel"
                    value={formData.phone}
                    onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Date of Birth</label>
                  <input
                    type="date"
                    required
                    value={formData.dateOfBirth}
                    onChange={(e) => setFormData({ ...formData, dateOfBirth: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Parent Name</label>
                <input
                  type="text"
                  value={formData.parentName}
                  onChange={(e) => setFormData({ ...formData, parentName: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Parent Phone</label>
                <input
                  type="tel"
                  value={formData.parentPhone}
                  onChange={(e) => setFormData({ ...formData, parentPhone: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Enrollment Date</label>
                <input
                  type="date"
                  required
                  value={formData.enrollmentDate}
                  onChange={(e) => setFormData({ ...formData, enrollmentDate: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
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
                  className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {createMutation.isPending || updateMutation.isPending ? 'Saving...' : 'Save'}
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
    </div>
  );
}

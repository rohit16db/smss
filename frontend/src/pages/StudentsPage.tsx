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
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4 sm:p-6 lg:p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-2">👨‍🎓 Students Management</h1>
          <p className="text-gray-600 mt-1">Manage student information, enrollments, and details</p>
        </div>

        {/* Action Bar */}
        <div className="bg-white rounded-lg shadow-md p-4 mb-6">
          <div className="flex flex-col sm:flex-row gap-4 justify-between items-start sm:items-center">
            <div className="flex-1 w-full sm:w-auto">
              <input
                type="text"
                placeholder="Search students..."
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setPage(0);
                }}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
            <button
              onClick={() => handleOpenDialog()}
              className="w-full sm:w-auto px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition"
            >
              + Add Student
            </button>
          </div>
        </div>

        {/* Desktop Table View */}
        <div className="hidden lg:block bg-white rounded-lg shadow-md overflow-hidden">
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
                <thead className="bg-gray-50 border-b border-gray-200">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Student ID</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Email</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Phone</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Section</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {studentsData?.items.map((student, index) => (
                    <tr key={student.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 text-sm">
                        <span className="font-mono font-semibold text-blue-600">{student.enrollmentNumber}</span>
                      </td>
                      <td className="px-6 py-4 text-sm font-medium text-gray-900">
                        <div className="flex items-center gap-3">
                          <div className={`w-10 h-10 ${getAvatarColor(index)} rounded-full flex items-center justify-center text-white font-semibold`}>
                            {getInitials(student.firstName, student.lastName)}
                          </div>
                          {student.firstName} {student.lastName}
                        </div>
                      </td>
                      <td className="px-6 py-4 text-sm text-gray-600">{student.email}</td>
                      <td className="px-6 py-4 text-sm text-gray-600">{student.phone || '-'}</td>
                      <td className="px-6 py-4 text-sm">
                        <button
                          onClick={() => handleOpenSectionDialog(student)}
                          className="px-3 py-1 bg-purple-100 hover:bg-purple-200 text-purple-700 rounded transition text-xs font-medium"
                        >
                          📚 Assign
                        </button>
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <button
                          onClick={() => handleToggleActive(student)}
                          className={`px-3 py-1 rounded-full text-xs font-semibold ${student.isActive
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
                            }`}
                        >
                          {student.isActive ? '✓ Active' : '○ Inactive'}
                        </button>
                      </td>
                      <td className="px-6 py-4 text-sm">
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleOpenDialog(student)}
                            className="px-3 py-1 bg-blue-100 hover:bg-blue-200 text-blue-700 rounded transition text-xs font-medium"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleDelete(student.id)}
                            className="px-3 py-1 bg-red-100 hover:bg-red-200 text-red-700 rounded transition text-xs font-medium"
                          >
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {/* Pagination */}
              <div className="bg-gray-50 border-t border-gray-200 px-6 py-4 flex items-center justify-between">
                <div className="text-sm text-gray-600">
                  Page {page + 1} of {totalPages} (Total: {studentsData?.totalCount} students)
                </div>
                <div className="flex gap-2 items-center">
                  <button
                    onClick={() => handleChangePage(page - 1)}
                    disabled={page === 0}
                    className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Previous
                  </button>
                  <select
                    value={rowsPerPage}
                    onChange={handleChangeRowsPerPage}
                    className="px-3 py-2 border border-gray-300 rounded"
                  >
                    <option value={5}>5</option>
                    <option value={10}>10</option>
                    <option value={25}>25</option>
                    <option value={50}>50</option>
                  </select>
                  <button
                    onClick={() => handleChangePage(page + 1)}
                    disabled={page >= totalPages - 1}
                    className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    Next
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
                <div key={student.id} className="bg-white rounded-lg shadow-md p-4">
                  <div className="flex items-start gap-3 mb-3">
                    <div className={`w-12 h-12 ${getAvatarColor(index)} rounded-full flex items-center justify-center text-white font-semibold flex-shrink-0`}>
                      {getInitials(student.firstName, student.lastName)}
                    </div>
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <span className="font-mono text-xs font-semibold text-blue-600 bg-blue-50 px-2 py-1 rounded">{student.enrollmentNumber}</span>
                      </div>
                      <h3 className="font-semibold text-gray-900">{student.firstName} {student.lastName}</h3>
                      <p className="text-sm text-gray-600">{student.email}</p>
                    </div>
                    <button
                      onClick={() => handleToggleActive(student)}
                      className={`px-2 py-1 rounded text-xs font-semibold ${student.isActive
                        ? 'bg-green-100 text-green-800'
                        : 'bg-gray-100 text-gray-800'
                        }`}
                    >
                      {student.isActive ? '✓' : '○'}
                    </button>
                  </div>
                  <div className="text-sm text-gray-600 space-y-1 mb-3">
                    {student.phone && <p>Phone: {student.phone}</p>}
                    <p>DOB: {new Date(student.dateOfBirth).toLocaleDateString()}</p>
                    {student.parentName && <p>Parent: {student.parentName}</p>}
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleOpenSectionDialog(student)}
                      className="flex-1 px-3 py-2 bg-purple-100 hover:bg-purple-200 text-purple-700 rounded transition text-sm font-medium"
                    >
                      📚 Section
                    </button>
                    <button
                      onClick={() => handleOpenDialog(student)}
                      className="flex-1 px-3 py-2 bg-blue-100 hover:bg-blue-200 text-blue-700 rounded transition text-sm font-medium"
                    >
                      Edit
                    </button>
                    <button
                      onClick={() => handleDelete(student.id)}
                      className="flex-1 px-3 py-2 bg-red-100 hover:bg-red-200 text-red-700 rounded transition text-sm font-medium"
                    >
                      Delete
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

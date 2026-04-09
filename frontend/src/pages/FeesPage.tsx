import { useState, useEffect, useRef } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { feeApi, studentApi, classApi, type CreateFeeStructureDto, type FeeStructure, type CreateStudentFeeDto, type StudentFee, type CreateFeePaymentDto, type Student, type BulkAssignStudentFeeDto, type SectionListDto } from '../services/api';
import { formatDate } from '../utils/dateFormat';
import type { AxiosError } from 'axios';
import { useAcademicYear } from '../hooks/useAcademicYear';

type TabType = 'structures' | 'assignments' | 'payments';

export function FeesPage() {
  const [activeTab, setActiveTab] = useState<TabType>('structures');

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-emerald-600 to-emerald-800 bg-clip-text text-transparent">
                💰 Fee Management
              </h1>
              <p className="text-gray-600 mt-2">Manage fee structures, student fees, and payments</p>
            </div>
          </div>

          {/* Tabs */}
          <div className="mb-6">
            <div className="border-b border-gray-200">
              <nav className="-mb-px flex space-x-8">
                <button
                  onClick={() => setActiveTab('structures')}
                  className={`${activeTab === 'structures'
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
                >
                  📋 Fee Structures
                </button>
                <button
                  onClick={() => setActiveTab('assignments')}
                  className={`${activeTab === 'assignments'
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
                >
                  🎓 Student Fee Assignments
                </button>
                <button
                  onClick={() => setActiveTab('payments')}
                  className={`${activeTab === 'payments'
                      ? 'border-blue-500 text-blue-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
                >
                  💳 Payments
                </button>
              </nav>
            </div>
          </div>

          {/* Tab Content */}
          {activeTab === 'structures' && <FeeStructuresTab />}
          {activeTab === 'assignments' && <StudentFeesTab />}
          {activeTab === 'payments' && <PaymentsTab />}
        </div>
      </div>
    </div>
  );
}

// Fee Structures Tab
function FeeStructuresTab() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [rowsPerPage] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedStructure, setSelectedStructure] = useState<FeeStructure | null>(null);
  const { activeYear, academicYears } = useAcademicYear();

  const [formData, setFormData] = useState<CreateFeeStructureDto>({
    name: '',
    academicYearId: activeYear?.id || '',
    frequency: 'Annual',
    totalAmount: 0,
    categories: [{ category: 'Tuition', amount: 0 }],
  });

  // Default academicYearId when activeYear changes
  useEffect(() => {
    if (activeYear && !selectedStructure) {
      setFormData(prev => ({ ...prev, academicYearId: activeYear.id }));
    }
  }, [activeYear, selectedStructure]);

  const { data, isLoading } = useQuery({
    queryKey: ['feeStructures', page + 1, rowsPerPage, searchTerm],
    queryFn: () => feeApi.getAllStructures({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
      searchTerm: searchTerm || undefined,
    }),
  });

  const createMutation = useMutation({
    mutationFn: feeApi.createStructure,
    onSuccess: () => {
      toast.success('Fee structure created successfully!');
      queryClient.invalidateQueries({ queryKey: ['feeStructures'] });
      handleCloseDialog();
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to create fee structure');
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<CreateFeeStructureDto> & { id: string } }) =>
      feeApi.updateStructure(id, data),
    onSuccess: () => {
      toast.success('Fee structure updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['feeStructures'] });
      handleCloseDialog();
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to update fee structure');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: feeApi.deleteStructure,
    onSuccess: () => {
      toast.success('Fee structure deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['feeStructures'] });
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to delete fee structure');
    },
  });

  const handleOpenDialog = (structure?: FeeStructure) => {
    if (structure) {
      setSelectedStructure(structure);
      setFormData({
        name: structure.name,
        academicYearId: structure.academicYearId,
        frequency: structure.frequency,
        totalAmount: structure.totalAmount,
        categories: structure.categories.map(c => ({ category: c.category, amount: c.amount })),
      });
    } else {
      setSelectedStructure(null);
      setFormData({
        name: '',
        academicYearId: activeYear?.id || '',
        frequency: 'Annual',
        totalAmount: 0,
        categories: [{ category: 'Tuition', amount: 0 }],
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedStructure(null);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedStructure) {
      updateMutation.mutate({
        id: selectedStructure.id,
        data: { ...formData, id: selectedStructure.id },
      });
    } else {
      createMutation.mutate(formData);
    }
  };

  const handleDelete = (id: string) => {
    if (confirm('Are you sure you want to delete this fee structure?')) {
      deleteMutation.mutate(id);
    }
  };

  const addCategory = () => {
    setFormData({
      ...formData,
      categories: [...formData.categories, { category: '', amount: 0 }],
    });
  };

  const removeCategory = (index: number) => {
    setFormData({
      ...formData,
      categories: formData.categories.filter((_, i) => i !== index),
    });
  };

  const updateCategory = (index: number, field: 'category' | 'amount', value: string | number) => {
    const newCategories = [...formData.categories];
    newCategories[index] = { ...newCategories[index], [field]: value };
    const total = newCategories.reduce((sum, cat) => sum + (Number(cat.amount) || 0), 0);
    setFormData({ ...formData, categories: newCategories, totalAmount: total });
  };

  const totalPages = Math.ceil((data?.totalCount || 0) / rowsPerPage);

  return (
    <div className="animate-slide-up">
      {/* Search and Add Button */}
      <div className="card mb-6">
        <div className="flex flex-col lg:flex-row gap-4 items-start lg:items-center justify-between">
          <div className="flex-1 w-full lg:max-w-md">
            <div className="relative">
              <input
                type="text"
                placeholder="Search fee structures..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="input-field pl-10"
              />
              <svg className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
            </div>
          </div>
          <button onClick={() => handleOpenDialog()} className="btn-primary flex items-center gap-2">
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            Create Fee Structure
          </button>
        </div>
      </div>

      {/* Desktop Table */}
      <div className="hidden lg:block card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Academic Year</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Frequency</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total Amount</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Categories</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center">
                    <div className="flex justify-center items-center">
                      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                    </div>
                  </td>
                </tr>
              ) : data?.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                    <p>No fee structures found</p>
                  </td>
                </tr>
              ) : (
                data?.items.map((structure) => (
                  <tr key={structure.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4">
                      <div className="text-sm font-medium text-gray-900">{structure.name}</div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">{structure.academicYearName}</td>
                    <td className="px-6 py-4 text-sm text-gray-500">{structure.frequency}</td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-semibold text-green-600">₹{structure.totalAmount.toFixed(2)}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-500">{structure.categories?.length ?? 0} categories</div>
                    </td>
                    <td className="px-6 py-4">
                      <span className={`badge ${structure.isActive ? 'badge-success' : 'badge-danger'}`}>
                        {structure.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right">
                      <div className="flex gap-2 justify-end">
                        <button onClick={() => handleOpenDialog(structure)} className="text-blue-600 hover:text-blue-900 p-2 hover:bg-blue-50 rounded-lg transition-colors" title="Edit">
                          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                          </svg>
                        </button>
                        <button onClick={() => handleDelete(structure.id)} className="text-red-600 hover:text-red-900 p-2 hover:bg-red-50 rounded-lg transition-colors" title="Delete">
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

        {data && data.totalCount > 0 && (
          <div className="bg-gray-50 px-6 py-4 border-t border-gray-200 flex items-center justify-between">
            <div className="text-sm text-gray-700">
              Showing {page * rowsPerPage + 1} to {Math.min((page + 1) * rowsPerPage, data.totalCount)} of {data.totalCount}
            </div>
            <div className="flex items-center gap-2">
              <button onClick={() => setPage(Math.max(0, page - 1))} disabled={page === 0} className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                Previous
              </button>
              <span className="text-sm text-gray-700">Page {page + 1} of {totalPages}</span>
              <button onClick={() => setPage(Math.min(totalPages - 1, page + 1))} disabled={page >= totalPages - 1} className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed">
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Dialog */}
      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4 animate-fade-in">
          <div className="bg-white rounded-2xl shadow-2xl max-w-3xl w-full max-h-[90vh] overflow-y-auto animate-slide-up">
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between rounded-t-2xl">
              <h2 className="text-2xl font-bold text-gray-900">{selectedStructure ? 'Edit Fee Structure' : 'Create Fee Structure'}</h2>
              <button onClick={handleCloseDialog} className="text-gray-400 hover:text-gray-600 transition-colors">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6">
              <div className="space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Name <span className="text-red-500">*</span></label>
                    <input type="text" value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} required className="input-field" placeholder="e.g., Elementary School Fees" />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Academic Year <span className="text-red-500">*</span></label>
                    <select
                      value={formData.academicYearId}
                      onChange={(e) => setFormData({ ...formData, academicYearId: e.target.value })}
                      required
                      className="input-field"
                    >
                      <option value="">Select Session</option>
                      {academicYears?.map(ay => (
                        <option key={ay.id} value={ay.id}>{ay.name}</option>
                      ))}
                    </select>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Frequency <span className="text-red-500">*</span></label>
                  <select value={formData.frequency} onChange={(e) => setFormData({ ...formData, frequency: e.target.value })} className="input-field">
                    <option value="Annual">Annual</option>
                    <option value="Semester">Semester</option>
                    <option value="Quarterly">Quarterly</option>
                    <option value="Monthly">Monthly</option>
                  </select>
                </div>

                <div>
                  <div className="flex items-center justify-between mb-2">
                    <label className="block text-sm font-medium text-gray-700">Categories <span className="text-red-500">*</span></label>
                    <button type="button" onClick={addCategory} className="text-sm text-blue-600 hover:text-blue-800 font-medium">+ Add Category</button>
                  </div>
                  <div className="space-y-2">
                    {formData.categories.map((category, index) => (
                      <div key={index} className="flex gap-2">
                        <input type="text" value={category.category} onChange={(e) => updateCategory(index, 'category', e.target.value)} placeholder="Category name" className="input-field flex-1" required />
                        <input type="number" value={category.amount} onChange={(e) => updateCategory(index, 'amount', parseFloat(e.target.value) || 0)} placeholder="Amount" className="input-field w-32" min="0" step="0.01" required />
                        {formData.categories.length > 1 && (
                          <button type="button" onClick={() => removeCategory(index)} className="text-red-600 hover:text-red-800 p-2">
                            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                          </button>
                        )}
                      </div>
                    ))}
                  </div>
                </div>

                <div className="bg-blue-50 p-4 rounded-lg">
                  <div className="flex justify-between items-center">
                    <span className="text-sm font-medium text-gray-700">Total Amount:</span>
                    <span className="text-2xl font-bold text-blue-600">₹{formData.totalAmount.toFixed(2)}</span>
                  </div>
                </div>
              </div>

              <div className="mt-6 flex gap-3">
                <button type="button" onClick={handleCloseDialog} className="flex-1 btn-secondary">Cancel</button>
                <button type="submit" disabled={createMutation.isPending || updateMutation.isPending} className="flex-1 btn-primary disabled:opacity-50 disabled:cursor-not-allowed">
                  {createMutation.isPending || updateMutation.isPending ? 'Saving...' : selectedStructure ? 'Update' : 'Create'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

// Student Fees Tab
function StudentFeesTab() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [rowsPerPage] = useState(10);
  const [openDialog, setOpenDialog] = useState(false);
  const [openTerminateDialog, setOpenTerminateDialog] = useState(false);
  const [openBulkDialog, setOpenBulkDialog] = useState(false);
  const [terminateFeeId, setTerminateFeeId] = useState<string | null>(null);
  const [terminateEndDate, setTerminateEndDate] = useState('');
  const [selectedSectionId, setSelectedSectionId] = useState(''); // Section filter
  const [studentSearch, setStudentSearch] = useState('');
  const [showStudentDropdown, setShowStudentDropdown] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const [formData, setFormData] = useState<CreateStudentFeeDto>({
    studentId: '',
    feeStructureId: '',
    startDate: new Date().toISOString().split('T')[0],
  });
  const [bulkFormData, setBulkFormData] = useState<BulkAssignStudentFeeDto>({
    feeStructureId: '',
    sectionId: '',
    startDate: new Date().toISOString().split('T')[0],
    skipAlreadyAssigned: false,
  });

  const { data, isLoading } = useQuery({
    queryKey: ['studentFees', page + 1, rowsPerPage],
    queryFn: () => feeApi.getAllStudentFees({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
    }),
  });

  // Load fee structures for dropdown
  const structuresQuery = useQuery({
    queryKey: ['feeStructures'],
    queryFn: () => feeApi.getAllStructures({ pageSize: 100 }),
  });

  // Load sections for bulk assignment
  const sectionsQuery = useQuery({
    queryKey: ['sections'],
    queryFn: async () => {
      const classesData = await classApi.getAll({ pageSize: 1000, isActive: true });
      const allSections: (SectionListDto & { className?: string })[] = [];
      for (const cls of classesData.items) {
        try {
          const sectionList = await classApi.getSectionsByClass(cls.id);
          sectionList.forEach((section) => {
            allSections.push({
              ...section,
              className: cls.name,
            });
          });
        } catch (err) {
          console.error(`Failed to load sections for class ${cls.id}`, err);
        }
      }
      return allSections;
    },
  });

  // Load students based on search term
  const studentsQuery = useQuery({
    queryKey: ['students', studentSearch],
    queryFn: () => studentApi.getAll({ 
      searchTerm: studentSearch || undefined, 
      pageSize: 10,
      isActive: true 
    }),
    enabled: openDialog && studentSearch.length >= 2,
  });

  // Handle click outside dropdown
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setShowStudentDropdown(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const assignMutation = useMutation({
    mutationFn: feeApi.assignFeeToStudent,
    onSuccess: () => {
      toast.success('Fee assigned to student successfully!');
      queryClient.invalidateQueries({ queryKey: ['studentFees'] });
      handleCloseDialog();
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to assign fee');
    },
  });

  const terminateMutation = useMutation({
    mutationFn: ({ id, endDate }: { id: string; endDate: string }) =>
      feeApi.terminateStudentFee(id, endDate),
    onSuccess: () => {
      toast.success('Fee assignment terminated successfully!');
      queryClient.invalidateQueries({ queryKey: ['studentFees'] });
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to terminate fee');
    },
  });

  const bulkAssignMutation = useMutation({
    mutationFn: feeApi.bulkAssignStudentFee,
    onSuccess: (result) => {
      toast.success(`Bulk assignment completed: ${result.successCount} students assigned, ${result.skippedCount} skipped`);
      if (result.failureCount > 0) {
        toast.error(`${result.failureCount} students failed: ${result.errors.map(e => e.studentName).join(', ')}`);
      }
      queryClient.invalidateQueries({ queryKey: ['studentFees'] });
      handleCloseBulkDialog();
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to perform bulk assignment');
    },
  });

  const handleOpenDialog = () => {
    setFormData({
      studentId: '',
      feeStructureId: '',
      startDate: new Date().toISOString().split('T')[0],
    });
    setStudentSearch('');
    setSelectedStudent(null);
    setShowStudentDropdown(false);
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setStudentSearch('');
    setSelectedStudent(null);
    setShowStudentDropdown(false);
  };

  const handleOpenBulkDialog = () => {
    setBulkFormData({
      feeStructureId: '',
      sectionId: '',
      startDate: new Date().toISOString().split('T')[0],
      skipAlreadyAssigned: false,
    });
    setOpenBulkDialog(true);
  };

  const handleCloseBulkDialog = () => {
    setOpenBulkDialog(false);
  };

  const handleBulkSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!bulkFormData.feeStructureId || !bulkFormData.sectionId) {
      toast.error('Please select both fee structure and section');
      return;
    }
    bulkAssignMutation.mutate(bulkFormData);
  };

  const handleSelectStudent = (student: Student) => {
    setSelectedStudent(student);
    setFormData({ ...formData, studentId: student.enrollmentNumber });
    setStudentSearch(`${student.firstName} ${student.lastName} (${student.enrollmentNumber})`);
    setShowStudentDropdown(false);
  };

  const handleStudentSearchChange = (value: string) => {
    setStudentSearch(value);
    setShowStudentDropdown(value.length >= 2);
    if (value.length < 2) {
      setSelectedStudent(null);
      setFormData({ ...formData, studentId: '' });
    }
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    assignMutation.mutate(formData);
  };

  const handleTerminate = (id: string) => {
    setTerminateFeeId(id);
    setTerminateEndDate(new Date().toISOString().split('T')[0]); // Default to today
    setOpenTerminateDialog(true);
  };

  const handleCloseTerminateDialog = () => {
    setOpenTerminateDialog(false);
    setTerminateFeeId(null);
    setTerminateEndDate('');
  };

  const handleConfirmTerminate = () => {
    if (terminateFeeId && terminateEndDate) {
      terminateMutation.mutate({ id: terminateFeeId, endDate: terminateEndDate });
      handleCloseTerminateDialog();
    }
  };

  const handleDownloadFeeDetails = async (id: string, studentName: string) => {
    try {
      const data = await feeApi.downloadStudentFeePdf(id);
      const blob = new Blob([data], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Fee_Details_${studentName.replace(/\s+/g, '_')}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.parentNode?.removeChild(link);
      window.URL.revokeObjectURL(url);
      toast.success('Fee details downloaded successfully!');
    } catch (error) {
      console.error('Failed to download fee details', error);
      toast.error('Failed to download fee details');
    }
  };

  const totalPages = Math.ceil((data?.totalCount || 0) / rowsPerPage);
  
  // Derive structures and sections from query data
  const structures = structuresQuery.data?.items || [];
  const sections = sectionsQuery.data || [];
  const structureMap = new Map(structures.map(s => [s.id, s.name]));

  return (
    <div className="animate-slide-up">
      {/* Action Button */}
      <div className="card mb-6">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-gray-900">Student Fee Assignments</h3>
          <div className="flex gap-2">
            <button onClick={handleOpenDialog} className="btn-primary flex items-center gap-2">
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Assign Fee
            </button>
            <button onClick={handleOpenBulkDialog} className="btn-secondary flex items-center gap-2">
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4M18 9l-6 6m0 0l-6-6" />
              </svg>
              Bulk Assign
            </button>
          </div>
        </div>
      </div>

      {/* Section Filter */}
      <div className="card mb-6">
        <div className="flex items-center gap-4">
          <label className="block text-sm font-medium text-gray-700">Filter by Section:</label>
          <select 
            value={selectedSectionId} 
            onChange={(e) => {
              setSelectedSectionId(e.target.value);
              setPage(0); // Reset to first page
            }}
            className="input-field flex-1 max-w-xs"
          >
            <option value="">All Sections</option>
            {sections.map(section => (
              <option key={section.id} value={section.id}>
                {section.className} - {section.sectionName}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Table */}
      <div className="hidden lg:block card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Student ID</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Fee Structure</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Start Date</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total Amount</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Paid/Balance</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center">
                    <div className="flex justify-center items-center">
                      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                    </div>
                  </td>
                </tr>
              ) : data?.items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                    <p>No student fee assignments found</p>
                  </td>
                </tr>
              ) : (
                (() => {
                  // Filter items by selected section
                  const filteredItems = selectedSectionId
                    ? data?.items.filter(fee => fee.sectionId === selectedSectionId) || []
                    : data?.items || [];
                  
                  return filteredItems.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="px-6 py-12 text-center text-gray-500">
                        <p>No student fee assignments found for the selected section</p>
                      </td>
                    </tr>
                  ) : (
                    filteredItems.map((fee) => (
                      <tr key={fee.id} className="hover:bg-gray-50 transition-colors">
                        <td className="px-6 py-4">
                          <div className="text-sm font-medium text-gray-900">{fee.studentName}</div>
                          <div className="text-xs font-mono text-blue-600">{fee.enrollmentNumber}</div>
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-500">{fee.feeStructureName || structureMap.get(fee.feeStructureId) || 'N/A'}</td>
                        <td className="px-6 py-4 text-sm text-gray-500">{formatDate(fee.startDate)}</td>
                        <td className="px-6 py-4">
                          <div className="text-sm font-semibold text-green-600">₹{fee.totalAmount.toFixed(2)}</div>
                        </td>
                        <td className="px-6 py-4">
                          <div className="text-sm">
                            <div className="font-medium text-gray-900">Paid: ₹{fee.paidAmount.toFixed(2)}</div>
                            <div className="text-gray-500">Balance: ₹{fee.balanceAmount.toFixed(2)}</div>
                          </div>
                        </td>
                        <td className="px-6 py-4">
                          <span className={`badge ${
                            !fee.isActive 
                              ? 'badge-danger' 
                              : fee.balanceAmount === 0 
                              ? 'badge-blue' 
                              : 'badge-success'
                          }`}>
                            {!fee.isActive 
                              ? 'Terminated' 
                              : fee.balanceAmount === 0 
                              ? 'Paid' 
                              : 'Active'}
                          </span>
                        </td>
                        <td className="px-6 py-4 text-right">
                          <div className="flex gap-2 justify-end">
                            <button
                              onClick={() => handleDownloadFeeDetails(fee.id, fee.studentName)}
                              className="text-blue-600 hover:text-blue-900 p-2 hover:bg-blue-50 rounded-lg transition-colors"
                              title="Download Fee Details"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                              </svg>
                            </button>
                            <button
                              onClick={() => handleTerminate(fee.id)}
                              disabled={!fee.isActive}
                              className="text-orange-600 hover:text-orange-900 p-2 hover:bg-orange-50 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                              title="Terminate"
                            >
                              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                              </svg>
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  );
                })()
              )}
            </tbody>
          </table>
        </div>

        {data && data.totalCount > 0 && (
          <div className="bg-gray-50 px-6 py-4 border-t border-gray-200 flex items-center justify-between">
            <div className="text-sm text-gray-700">
              Showing {page * rowsPerPage + 1} to {Math.min((page + 1) * rowsPerPage, data.totalCount)} of {data.totalCount}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setPage(Math.max(0, page - 1))}
                disabled={page === 0}
                className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="text-sm text-gray-700">Page {page + 1} of {totalPages}</span>
              <button
                onClick={() => setPage(Math.min(totalPages - 1, page + 1))}
                disabled={page >= totalPages - 1}
                className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Dialog */}
      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4 animate-fade-in">
          <div className="bg-white rounded-2xl shadow-2xl max-w-lg w-full max-h-[90vh] overflow-y-auto animate-slide-up">
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between rounded-t-2xl">
              <h2 className="text-2xl font-bold text-gray-900">Assign Fee to Student</h2>
              <button onClick={handleCloseDialog} className="text-gray-400 hover:text-gray-600 transition-colors">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6">
              <div className="space-y-4">
                <div className="relative" ref={dropdownRef}>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Student <span className="text-red-500">*</span></label>
                  <input
                    type="text"
                    value={studentSearch}
                    onChange={(e) => handleStudentSearchChange(e.target.value)}
                    onFocus={() => studentSearch.length >= 2 && setShowStudentDropdown(true)}
                    required={!selectedStudent}
                    className="input-field"
                    placeholder="Search by name or enrollment number..."
                    autoComplete="off"
                  />
                  {selectedStudent && (
                    <div className="mt-2 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                      <div className="text-sm">
                        <div className="font-semibold text-blue-900">{selectedStudent.firstName} {selectedStudent.lastName}</div>
                        <div className="text-blue-700 font-mono text-xs">{selectedStudent.enrollmentNumber}</div>
                      </div>
                    </div>
                  )}
                  {showStudentDropdown && studentSearch.length >= 2 && (
                    <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                      {studentsQuery.isLoading ? (
                        <div className="p-4 text-center text-gray-500">
                          <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mx-auto"></div>
                        </div>
                      ) : studentsQuery.data?.items.length === 0 ? (
                        <div className="p-4 text-center text-gray-500 text-sm">
                          No students found
                        </div>
                      ) : (
                        <ul className="py-1">
                          {studentsQuery.data?.items.map((student) => (
                            <li
                              key={student.id}
                              onClick={() => handleSelectStudent(student)}
                              className="px-4 py-3 hover:bg-blue-50 cursor-pointer border-b border-gray-100 last:border-b-0 transition-colors"
                            >
                              <div className="text-sm">
                                <div className="font-semibold text-gray-900">{student.firstName} {student.lastName}</div>
                                <div className="flex items-center gap-2 mt-1">
                                  <span className="text-blue-600 font-mono text-xs">{student.enrollmentNumber}</span>
                                  <span className="text-gray-500 text-xs">•</span>
                                  <span className="text-gray-600 text-xs">{student.email}</span>
                                </div>
                              </div>
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Fee Structure <span className="text-red-500">*</span></label>
                  <select
                    value={formData.feeStructureId}
                    onChange={(e) => setFormData({ ...formData, feeStructureId: e.target.value })}
                    required
                    className="input-field"
                  >
                    <option value="">Select a fee structure</option>
                    {structures.map((structure) => (
                      <option key={structure.id} value={structure.id}>
                        {structure.name} - {structure.academicYearName} (₹{structure.totalAmount.toFixed(2)})
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Start Date <span className="text-red-500">*</span></label>
                  <input
                    type="date"
                    value={formData.startDate}
                    onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
                    required
                    className="input-field"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">End Date (Optional)</label>
                  <input
                    type="date"
                    value={formData.endDate || ''}
                    onChange={(e) => setFormData({ ...formData, endDate: e.target.value || undefined })}
                    className="input-field"
                  />
                </div>
              </div>

              <div className="mt-6 flex gap-3">
                <button type="button" onClick={handleCloseDialog} className="flex-1 btn-secondary">
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={assignMutation.isPending}
                  className="flex-1 btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {assignMutation.isPending ? 'Assigning...' : 'Assign Fee'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Terminate Confirmation Dialog */}
      {openTerminateDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4 animate-fade-in">
          <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full animate-slide-up">
            <div className="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between rounded-t-2xl">
              <h2 className="text-xl font-bold text-gray-900">Terminate Fee Assignment</h2>
              <button onClick={handleCloseTerminateDialog} className="text-gray-400 hover:text-gray-600 transition-colors">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div className="p-6">
              <div className="mb-4">
                <div className="flex items-center gap-3 p-4 bg-orange-50 border border-orange-200 rounded-lg mb-4">
                  <svg className="w-6 h-6 text-orange-600 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                  <div>
                    <p className="text-sm font-semibold text-orange-900">Terminate this fee assignment?</p>
                    <p className="text-xs text-orange-700 mt-1">This will mark the assignment as inactive from the selected date.</p>
                  </div>
                </div>

                <label className="block text-sm font-medium text-gray-700 mb-2">
                  End Date <span className="text-red-500">*</span>
                </label>
                <input
                  type="date"
                  value={terminateEndDate}
                  onChange={(e) => setTerminateEndDate(e.target.value)}
                  className="input-field"
                  required
                />
                <p className="text-xs text-gray-500 mt-1">The date when this fee assignment ends</p>
              </div>

              <div className="flex gap-3">
                <button
                  type="button"
                  onClick={handleCloseTerminateDialog}
                  className="flex-1 btn-secondary"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  onClick={handleConfirmTerminate}
                  disabled={!terminateEndDate || terminateMutation.isPending}
                  className="flex-1 bg-orange-600 text-white px-4 py-2 rounded-lg hover:bg-orange-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {terminateMutation.isPending ? 'Terminating...' : 'Terminate'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Bulk Assignment Dialog */}
      {openBulkDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4 animate-fade-in">
          <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto animate-slide-up">
            <div className="bg-gradient-to-r from-green-600 to-emerald-600 px-6 py-4 flex items-center justify-between rounded-t-2xl">
              <div className="flex items-center gap-3">
                <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4M18 9l-6 6m0 0l-6-6" />
                </svg>
                <h2 className="text-xl font-bold text-white">Bulk Assign Fees to Class</h2>
              </div>
              <button onClick={handleCloseBulkDialog} className="text-white hover:text-gray-200 transition-colors">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div className="p-6">
              <form onSubmit={handleBulkSubmit} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Section <span className="text-red-500">*</span>
                  </label>
                  <select
                    value={bulkFormData.sectionId}
                    onChange={(e) => setBulkFormData({ ...bulkFormData, sectionId: e.target.value })}
                    className="input-field w-full"
                    required
                  >
                    <option value="">Select a section...</option>
                    {sections.map((section) => (
                      <option key={section.id} value={section.id}>
                        {section.className} - {section.sectionName} ({section.studentCount} students)
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Fee Structure <span className="text-red-500">*</span>
                  </label>
                  <select
                    value={bulkFormData.feeStructureId}
                    onChange={(e) => setBulkFormData({ ...bulkFormData, feeStructureId: e.target.value })}
                    className="input-field w-full"
                    required
                  >
                    <option value="">Select a fee structure...</option>
                    {structures.map((structure) => (
                      <option key={structure.id} value={structure.id}>
                        {structure.name} (₹{structure.totalAmount.toFixed(2)})
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Start Date <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="date"
                    value={bulkFormData.startDate}
                    onChange={(e) => setBulkFormData({ ...bulkFormData, startDate: e.target.value })}
                    className="input-field w-full"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    End Date (Optional)
                  </label>
                  <input
                    type="date"
                    value={bulkFormData.endDate || ''}
                    onChange={(e) => setBulkFormData({ ...bulkFormData, endDate: e.target.value || undefined })}
                    className="input-field w-full"
                  />
                </div>

                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <label className="flex items-center gap-3 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={bulkFormData.skipAlreadyAssigned}
                      onChange={(e) => setBulkFormData({ ...bulkFormData, skipAlreadyAssigned: e.target.checked })}
                      className="w-4 h-4 text-blue-600 rounded focus:ring-2 focus:ring-blue-500"
                    />
                    <div>
                      <p className="text-sm font-medium text-gray-900">Skip students already assigned</p>
                      <p className="text-xs text-gray-600 mt-1">If unchecked, existing assignments will be terminated and replaced</p>
                    </div>
                  </label>
                </div>

                <div className="bg-green-50 p-4 rounded-lg border border-green-200">
                  <p className="text-sm text-gray-700">
                    <span className="font-semibold">Summary:</span> This will assign <span className="font-bold text-green-700">{sections.find(s => s.id === bulkFormData.sectionId)?.studentCount || 0} students</span> from the selected section to the fee structure.
                  </p>
                </div>

                <div className="mt-6 flex gap-3">
                  <button
                    type="button"
                    onClick={handleCloseBulkDialog}
                    className="flex-1 btn-secondary disabled:opacity-50"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={bulkAssignMutation.isPending}
                    className="flex-1 bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed font-medium"
                  >
                    {bulkAssignMutation.isPending ? 'Assigning...' : 'Assign Fees'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// Payments Tab
function PaymentsTab() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [rowsPerPage] = useState(10);
  const [openDialog, setOpenDialog] = useState(false);
  const [studentSearch, setStudentSearch] = useState('');
  const [showStudentDropdown, setShowStudentDropdown] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(null);
  const [downloadingPaymentId, setDownloadingPaymentId] = useState<string | null>(null);
  const paymentDropdownRef = useRef<HTMLDivElement>(null);
  const [formData, setFormData] = useState<CreateFeePaymentDto>({
    studentFeeId: '',
    amountPaid: 0,
    paymentDate: new Date().toISOString().split('T')[0],
    paymentMethod: 'Cash',
    notes: '',
  });

  const { data, isLoading } = useQuery({
    queryKey: ['payments', page + 1, rowsPerPage],
    queryFn: () => feeApi.getAllPayments({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
    }),
  });

  // Load all student fees for the payment table display
  const studentFeesQuery = useQuery({
    queryKey: ['studentFees'],
    queryFn: () => feeApi.getAllStudentFees({ pageSize: 100, isActive: true }),
  });

  // Load students based on search term for payment dialog
  const studentsQuery = useQuery({
    queryKey: ['students-payment', studentSearch],
    queryFn: () => studentApi.getAll({ 
      searchTerm: studentSearch || undefined, 
      pageSize: 10,
      isActive: true 
    }),
    enabled: openDialog && studentSearch.length >= 2,
  });

  // Load fees for selected student
  const selectedStudentFeesQuery = useQuery({
    queryKey: ['student-fees-for-payment', selectedStudent?.id],
    queryFn: () => feeApi.getAllStudentFees({ 
      studentId: selectedStudent!.id,
      isActive: true,
      pageSize: 50
    }),
    enabled: !!selectedStudent,
  });

  // Handle click outside dropdown
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (paymentDropdownRef.current && !paymentDropdownRef.current.contains(event.target as Node)) {
        setShowStudentDropdown(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const recordMutation = useMutation({
    mutationFn: feeApi.recordPayment,
    onSuccess: (newPayment) => {
      toast.success('Payment recorded successfully!');
      queryClient.invalidateQueries({ queryKey: ['payments'] });
      queryClient.invalidateQueries({ queryKey: ['studentFees'] });
      handleCloseDialog();
      
      // Automatically download receipt
      if (newPayment && newPayment.id) {
        handleDownloadReceipt(newPayment.id);
      }
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to record payment');
    },
  });

  const handleOpenDialog = () => {
    setFormData({
      studentFeeId: '',
      amountPaid: 0,
      paymentDate: new Date().toISOString().split('T')[0],
      paymentMethod: 'Cash',
      notes: '',
    });
    setStudentSearch('');
    setSelectedStudent(null);
    setShowStudentDropdown(false);
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setStudentSearch('');
    setSelectedStudent(null);
    setShowStudentDropdown(false);
  };

  const handleSelectStudent = (student: Student) => {
    setSelectedStudent(student);
    setStudentSearch(`${student.firstName} ${student.lastName} (${student.enrollmentNumber})`);
    setShowStudentDropdown(false);
    setFormData({ ...formData, studentFeeId: '' }); // Reset selected fee
  };

  const handleStudentSearchChange = (value: string) => {
    setStudentSearch(value);
    setShowStudentDropdown(value.length >= 2);
    if (value.length < 2) {
      setSelectedStudent(null);
      setFormData({ ...formData, studentFeeId: '' });
    }
  };

  const handleSelectStudentFee = (fee: StudentFee) => {
    setFormData({ 
      ...formData, 
      studentFeeId: fee.id,
      amountPaid: fee.balanceAmount // Pre-fill with balance amount
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate fee selection
    if (!formData.studentFeeId) {
      toast.error('Please select a fee to pay');
      return;
    }

    // Validate amount
    if (formData.amountPaid <= 0) {
      toast.error('Payment amount must be greater than 0');
      return;
    }

    recordMutation.mutate(formData);
  };

  const totalPages = Math.ceil((data?.totalCount || 0) / rowsPerPage);

  // Derive student fees from query data
  const studentFees = studentFeesQuery.data?.items || [];

  // Helper function to get fee structure name by student fee ID
  const getFeeName = (studentFeeId: string) => {
    const fee = studentFees.find(f => f.id === studentFeeId);
    return fee ? fee.feeStructureName : studentFeeId;
  };

  // Download PDF receipt
  const handleDownloadReceipt = async (paymentId: string) => {
    try {
      setDownloadingPaymentId(paymentId);
      const response = await feeApi.downloadFeeReceipt(paymentId);
      
      // Create blob and trigger download
      const blob = new Blob([response], { type: 'application/pdf' });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `fee-receipt-${paymentId}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      toast.success('Receipt downloaded successfully!');
    } catch (error) {
      console.error('Failed to download receipt:', error);
      toast.error('Failed to download receipt');
    } finally {
      setDownloadingPaymentId(null);
    }
  };

  return (
    <div className="animate-slide-up">
      {/* Action Button */}
      <div className="card mb-6">
        <div className="flex items-center justify-between">
          <h3 className="text-lg font-semibold text-gray-900">Payment Records</h3>
          <button onClick={handleOpenDialog} className="btn-primary flex items-center gap-2">
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
            </svg>
            Record Payment
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="hidden lg:block card overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Receipt #</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Fee Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Amount Paid</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Payment Date</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Payment Method</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Notes</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {isLoading ? (
                <tr>
                  <td colSpan={6} className="px-6 py-12 text-center">
                    <div className="flex justify-center items-center">
                      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                    </div>
                  </td>
                </tr>
              ) : data?.items.length === 0 ? (
                <tr>
                  <td colSpan={6} className="px-6 py-12 text-center text-gray-500">
                    <p>No payments recorded yet</p>
                  </td>
                </tr>
              ) : (
                data?.items.map((payment) => (
                  <tr key={payment.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-6 py-4">
                      <div className="text-sm font-mono font-semibold text-blue-600">{payment.receiptNumber}</div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-900">{getFeeName(payment.studentFeeId)}</td>
                    <td className="px-6 py-4">
                      <div className="text-sm font-semibold text-green-600">₹{payment.amountPaid.toFixed(2)}</div>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">
                      {formatDate(payment.paymentDate)}
                    </td>
                    <td className="px-6 py-4">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                        {payment.paymentMethod}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-sm text-gray-500">{payment.notes || '-'}</td>
                    <td className="px-6 py-4">
                      <button
                        onClick={() => handleDownloadReceipt(payment.id)}
                        disabled={downloadingPaymentId === payment.id}
                        className="inline-flex items-center gap-1 px-3 py-1 rounded-lg bg-blue-50 text-blue-600 hover:bg-blue-100 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        title="Download receipt PDF"
                      >
                        {downloadingPaymentId === payment.id ? (
                          <>
                            <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 2v20m10-10H2" />
                            </svg>
                            <span className="text-xs font-medium">Downloading...</span>
                          </>
                        ) : (
                          <>
                            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                            </svg>
                            <span className="text-xs font-medium">Download</span>
                          </>
                        )}
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {data && data.totalCount > 0 && (
          <div className="bg-gray-50 px-6 py-4 border-t border-gray-200 flex items-center justify-between">
            <div className="text-sm text-gray-700">
              Showing {page * rowsPerPage + 1} to {Math.min((page + 1) * rowsPerPage, data.totalCount)} of {data.totalCount}
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={() => setPage(Math.max(0, page - 1))}
                disabled={page === 0}
                className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Previous
              </button>
              <span className="text-sm text-gray-700">Page {page + 1} of {totalPages}</span>
              <button
                onClick={() => setPage(Math.min(totalPages - 1, page + 1))}
                disabled={page >= totalPages - 1}
                className="px-3 py-1 rounded-lg border border-gray-300 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Dialog */}
      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center p-4 animate-fade-in">
          <div className="bg-white rounded-2xl shadow-2xl max-w-lg w-full max-h-[90vh] overflow-y-auto animate-slide-up">
            <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between rounded-t-2xl">
              <h2 className="text-2xl font-bold text-gray-900">Record Payment</h2>
              <button onClick={handleCloseDialog} className="text-gray-400 hover:text-gray-600 transition-colors">
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6">
              <div className="space-y-4">
                <div className="relative" ref={paymentDropdownRef}>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Search Student <span className="text-red-500">*</span></label>
                  <input
                    type="text"
                    value={studentSearch}
                    onChange={(e) => handleStudentSearchChange(e.target.value)}
                    onFocus={() => studentSearch.length >= 2 && setShowStudentDropdown(true)}
                    required={!selectedStudent}
                    className="input-field"
                    placeholder="Search by name or enrollment number..."
                    autoComplete="off"
                  />
                  {selectedStudent && (
                    <div className="mt-2 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                      <div className="text-sm">
                        <div className="font-semibold text-blue-900">{selectedStudent.firstName} {selectedStudent.lastName}</div>
                        <div className="text-blue-700 font-mono text-xs">{selectedStudent.enrollmentNumber}</div>
                      </div>
                    </div>
                  )}
                  {showStudentDropdown && studentSearch.length >= 2 && (
                    <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                      {studentsQuery.isLoading ? (
                        <div className="p-4 text-center text-gray-500">
                          <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mx-auto"></div>
                        </div>
                      ) : studentsQuery.data?.items.length === 0 ? (
                        <div className="p-4 text-center text-gray-500 text-sm">
                          No students found
                        </div>
                      ) : (
                        <ul className="py-1">
                          {studentsQuery.data?.items.map((student) => (
                            <li
                              key={student.id}
                              onClick={() => handleSelectStudent(student)}
                              className="px-4 py-3 hover:bg-blue-50 cursor-pointer border-b border-gray-100 last:border-b-0 transition-colors"
                            >
                              <div className="text-sm">
                                <div className="font-semibold text-gray-900">{student.firstName} {student.lastName}</div>
                                <div className="flex items-center gap-2 mt-1">
                                  <span className="text-blue-600 font-mono text-xs">{student.enrollmentNumber}</span>
                                  <span className="text-gray-500 text-xs">•</span>
                                  <span className="text-gray-600 text-xs">{student.email}</span>
                                </div>
                              </div>
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                  )}
                </div>

                {selectedStudent && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Select Fee to Pay <span className="text-red-500">*</span></label>
                    {selectedStudentFeesQuery.isLoading ? (
                      <div className="p-4 text-center">
                        <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mx-auto"></div>
                      </div>
                    ) : !selectedStudentFeesQuery.data || selectedStudentFeesQuery.data.items.length === 0 ? (
                      <div className="p-4 text-center text-gray-500 text-sm border border-gray-300 rounded-lg">
                        No active fees found for this student
                      </div>
                    ) : (
                      <>
                        <div className="space-y-2 max-h-60 overflow-y-auto border border-gray-300 rounded-lg p-2">
                        {selectedStudentFeesQuery.data.items.map((fee) => (
                          <div
                            key={fee.id}
                            onClick={() => handleSelectStudentFee(fee)}
                            className={`p-3 border-2 rounded-lg cursor-pointer transition-all ${
                              formData.studentFeeId === fee.id
                                ? 'border-blue-500 bg-blue-50'
                                : 'border-gray-200 hover:border-blue-300 hover:bg-gray-50'
                            }`}
                          >
                            <div className="flex justify-between items-start">
                              <div className="flex-1">
                                <div className="font-semibold text-gray-900">{fee.feeStructureName}</div>
                                <div className="text-xs text-gray-500 mt-1">
                                  {formatDate(fee.startDate)} {fee.endDate && `- ${formatDate(fee.endDate)}`}
                                </div>
                              </div>
                              <div className="text-right ml-4">
                                <div className="text-sm font-semibold text-gray-900">₹{fee.totalAmount.toFixed(2)}</div>
                                <div className="text-xs text-gray-500">Paid: ₹{fee.paidAmount.toFixed(2)}</div>
                                <div className="text-sm font-semibold text-orange-600">Balance: ₹{fee.balanceAmount.toFixed(2)}</div>
                              </div>
                            </div>
                            {formData.studentFeeId === fee.id && (
                              <div className="mt-2 flex items-center gap-1 text-blue-600 text-xs">
                                <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                  <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                                </svg>
                                <span>Selected</span>
                              </div>
                            )}
                          </div>
                        ))}
                      </div>
                      {!formData.studentFeeId && (
                        <p className="text-xs text-blue-600 mt-2">
                          <span className="font-medium">Click on a fee above to select it for payment</span>
                        </p>
                      )}
                    </>
                    )}
                  </div>
                )}

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Amount Paid <span className="text-red-500">*</span></label>
                  <input
                    type="number"
                    value={formData.amountPaid}
                    onChange={(e) => setFormData({ ...formData, amountPaid: parseFloat(e.target.value) || 0 })}
                    required
                    min="0.01"
                    step="0.01"
                    className="input-field"
                    placeholder="Enter amount greater than 0"
                  />
                  {formData.amountPaid <= 0 && (
                    <p className="text-xs text-orange-600 mt-1">Amount must be greater than 0</p>
                  )}
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Payment Date <span className="text-red-500">*</span></label>
                  <input
                    type="date"
                    value={formData.paymentDate}
                    onChange={(e) => setFormData({ ...formData, paymentDate: e.target.value })}
                    required
                    className="input-field"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Payment Method <span className="text-red-500">*</span></label>
                  <select
                    value={formData.paymentMethod}
                    onChange={(e) => setFormData({ ...formData, paymentMethod: e.target.value })}
                    required
                    className="input-field"
                  >
                    <option value="Cash">Cash</option>
                    <option value="Bank Transfer">Bank Transfer</option>
                    <option value="Check">Check</option>
                    <option value="Credit Card">Credit Card</option>
                    <option value="Online">Online</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Notes</label>
                  <textarea
                    value={formData.notes}
                    onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                    className="input-field resize-none"
                    rows={3}
                    placeholder="Additional notes (optional)"
                  />
                </div>
              </div>

              <div className="mt-6 flex gap-3">
                <button type="button" onClick={handleCloseDialog} className="flex-1 btn-secondary">
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={recordMutation.isPending || !formData.studentFeeId || formData.amountPaid <= 0}
                  className="flex-1 btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {recordMutation.isPending ? 'Recording...' : 'Record Payment'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

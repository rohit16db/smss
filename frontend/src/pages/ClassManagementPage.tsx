import { useState } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { classApi, type ClassListDto, type Section } from '../services/api';

export function ClassManagementPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedClass, setSelectedClass] = useState<string | null>(null);
  const [openDialog, setOpenDialog] = useState(false);
  const [dialogMode, setDialogMode] = useState<'class' | 'section'>('class');
  const [isEditing, setIsEditing] = useState(false);

  // Form states
  const [classForm, setClassForm] = useState({ name: '', academicYear: '' });
  const [sectionForm, setSectionForm] = useState({ sectionName: '' });
  const [editingId, setEditingId] = useState<string | null>(null);

  // Queries
  const { data: classesData, isLoading: classesLoading } = useQuery({
    queryKey: ['classes', page + 1, searchTerm],
    queryFn: () =>
      classApi.getAll({
        pageNumber: page + 1,
        pageSize: 10,
        searchTerm: searchTerm || undefined,
      }),
  });

  const { data: selectedClassData } = useQuery({
    queryKey: ['class', selectedClass],
    queryFn: () => classApi.getById(selectedClass as string),
    enabled: !!selectedClass,
  });

  // Mutations
  const createClassMutation = useMutation({
    mutationFn: () => classApi.create(classForm),
    onSuccess: () => {
      toast.success('Class created successfully!');
      queryClient.invalidateQueries({ queryKey: ['classes'] });
      setOpenDialog(false);
      setClassForm({ name: '', academicYear: '' });
    },
    onError: () => toast.error('Failed to create class'),
  });

  const updateClassMutation = useMutation({
    mutationFn: () => classApi.update(editingId as string, { ...classForm, isActive: true }),
    onSuccess: () => {
      toast.success('Class updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['classes'] });
      queryClient.invalidateQueries({ queryKey: ['class', selectedClass] });
      setOpenDialog(false);
      setClassForm({ name: '', academicYear: '' });
      setIsEditing(false);
      setEditingId(null);
    },
    onError: () => toast.error('Failed to update class'),
  });

  const deleteClassMutation = useMutation({
    mutationFn: (id: string) => classApi.delete(id),
    onSuccess: () => {
      toast.success('Class deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['classes'] });
      setSelectedClass(null);
    },
    onError: () => toast.error('Failed to delete class'),
  });

  const createSectionMutation = useMutation({
    mutationFn: () =>
      classApi.createSection({
        classId: selectedClass as string,
        sectionName: sectionForm.sectionName,
      }),
    onSuccess: () => {
      toast.success('Section created successfully!');
      queryClient.invalidateQueries({ queryKey: ['class', selectedClass] });
      setOpenDialog(false);
      setSectionForm({ sectionName: '' });
    },
    onError: () => toast.error('Failed to create section'),
  });

  const updateSectionMutation = useMutation({
    mutationFn: () =>
      classApi.updateSection(editingId as string, {
        sectionName: sectionForm.sectionName,
        isActive: true,
      }),
    onSuccess: () => {
      toast.success('Section updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['class', selectedClass] });
      setOpenDialog(false);
      setSectionForm({ sectionName: '' });
      setIsEditing(false);
      setEditingId(null);
    },
    onError: () => toast.error('Failed to update section'),
  });

  const deleteSectionMutation = useMutation({
    mutationFn: (id: string) => classApi.deleteSection(id),
    onSuccess: () => {
      toast.success('Section deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['class', selectedClass] });
    },
    onError: () => toast.error('Failed to delete section'),
  });

  const handleOpenClassDialog = (classItem?: ClassListDto) => {
    if (classItem) {
      setClassForm({ name: classItem.name, academicYear: classItem.academicYear || '' });
      setEditingId(classItem.id);
      setIsEditing(true);
    } else {
      setClassForm({ name: '', academicYear: '' });
      setEditingId(null);
      setIsEditing(false);
    }
    setDialogMode('class');
    setOpenDialog(true);
  };

  const handleOpenSectionDialog = (section?: Section) => {
    if (section) {
      setSectionForm({ sectionName: section.sectionName });
      setEditingId(section.id);
      setIsEditing(true);
    } else {
      setSectionForm({ sectionName: '' });
      setEditingId(null);
      setIsEditing(false);
    }
    setDialogMode('section');
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setClassForm({ name: '', academicYear: '' });
    setSectionForm({ sectionName: '' });
    setEditingId(null);
    setIsEditing(false);
  };

  const handleSubmitClass = (e: React.FormEvent) => {
    e.preventDefault();
    if (!classForm.name.trim()) {
      toast.error('Class name is required');
      return;
    }

    if (isEditing) {
      updateClassMutation.mutate();
    } else {
      createClassMutation.mutate();
    }
  };

  const handleSubmitSection = (e: React.FormEvent) => {
    e.preventDefault();
    if (!sectionForm.sectionName.trim()) {
      toast.error('Section name is required');
      return;
    }

    if (isEditing) {
      updateSectionMutation.mutate();
    } else {
      createSectionMutation.mutate();
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-50 via-purple-50 to-blue-50 p-4 sm:p-6 lg:p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-12 h-12 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-xl flex items-center justify-center shadow-lg">
              <svg className="w-7 h-7 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>
            </div>
            <h1 className="text-3xl sm:text-4xl font-bold text-gray-900">Classes & Sections</h1>
          </div>
          <p className="text-gray-600 mt-1 ml-15">Organize your school structure with classes and sections</p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Classes List */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden">
              <div className="bg-gradient-to-r from-indigo-500 to-purple-600 p-6">
                <div className="flex justify-between items-center text-white">
                  <div>
                    <h2 className="text-xl font-bold">Classes</h2>
                    <p className="text-indigo-100 text-sm mt-1">
                      {classesData?.totalCount || 0} total classes
                    </p>
                  </div>
                  <button
                    onClick={() => handleOpenClassDialog()}
                    className="px-4 py-2 bg-white text-indigo-600 rounded-lg text-sm font-semibold hover:bg-indigo-50 transition-all transform hover:scale-105 shadow-lg"
                  >
                    + New Class
                  </button>
                </div>
              </div>

              <div className="p-4">
                <div className="relative">
                  <input
                    type="text"
                    placeholder="Search classes..."
                    value={searchTerm}
                    onChange={(e) => {
                      setSearchTerm(e.target.value);
                      setPage(0);
                    }}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-xl focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition-all"
                  />
                  <svg
                    className="w-5 h-5 text-gray-400 absolute left-3 top-3.5"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                  >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
              </div>

              {classesLoading ? (
                <div className="p-8 text-center">
                  <div className="animate-spin w-10 h-10 border-4 border-indigo-500 border-t-transparent rounded-full mx-auto"></div>
                  <p className="text-gray-500 mt-3">Loading classes...</p>
                </div>
              ) : classesData?.items.length === 0 ? (
                <div className="p-8 text-center">
                  <svg className="w-20 h-20 text-gray-300 mx-auto mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                  </svg>
                  <p className="text-gray-500 font-medium">No classes found</p>
                  <p className="text-gray-400 text-sm mt-1">Create your first class to get started</p>
                </div>
              ) : (
                <div className="divide-y divide-gray-100 max-h-[500px] overflow-y-auto">
                  {classesData?.items.map((item) => (
                    <div
                      key={item.id}
                      onClick={() => setSelectedClass(item.id)}
                      className={`p-4 cursor-pointer transition-all duration-200 hover:bg-gradient-to-r hover:from-indigo-50 hover:to-purple-50 group ${
                        selectedClass === item.id
                          ? 'bg-gradient-to-r from-indigo-50 to-purple-50 border-l-4 border-indigo-600 shadow-md'
                          : 'border-l-4 border-transparent'
                      }`}
                    >
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          <p className="font-semibold text-gray-900 group-hover:text-indigo-600 transition-colors">
                            {item.name}
                          </p>
                          <div className="flex items-center gap-2 mt-1">
                            {item.academicYear && (
                              <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-indigo-100 text-indigo-700">
                                📅 {item.academicYear}
                              </span>
                            )}
                            <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-700">
                              📚 {item.sectionCount} sections
                            </span>
                          </div>
                        </div>
                        <svg
                          className={`w-5 h-5 transition-all ${
                            selectedClass === item.id ? 'text-indigo-600' : 'text-gray-400 group-hover:text-indigo-500'
                          }`}
                          fill="none"
                          viewBox="0 0 24 24"
                          stroke="currentColor"
                        >
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                        </svg>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {/* Pagination */}
              <div className="p-4 border-t border-gray-100 bg-gray-50 flex gap-2 justify-between">
                <button
                  onClick={() => setPage(Math.max(0, page - 1))}
                  disabled={page === 0}
                  className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed transition-all"
                >
                  ← Previous
                </button>
                <span className="px-4 py-2 text-sm text-gray-600 flex items-center">
                  Page {page + 1}
                </span>
                <button
                  onClick={() => setPage(page + 1)}
                  disabled={!classesData || classesData.items.length < 10}
                  className="flex-1 px-4 py-2 border border-gray-300 rounded-lg text-sm font-medium hover:bg-white disabled:opacity-50 disabled:cursor-not-allowed transition-all"
                >
                  Next →
                </button>
              </div>
            </div>
          </div>

          {/* Sections Details */}
          <div className="lg:col-span-2">
            {selectedClass && selectedClassData ? (
              <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden">
                <div className="bg-gradient-to-r from-purple-500 to-indigo-600 p-6">
                  <div className="flex justify-between items-start">
                    <div className="text-white">
                      <h2 className="text-2xl font-bold mb-2">{selectedClassData.name}</h2>
                      <div className="flex items-center gap-3">
                        {selectedClassData.academicYear && (
                          <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-white bg-opacity-20 backdrop-blur-sm">
                            📅 {selectedClassData.academicYear}
                          </span>
                        )}
                        <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-white bg-opacity-20 backdrop-blur-sm">
                          📚 {selectedClassData.sections.length} Sections
                        </span>
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <button
                        onClick={() => {
                          setClassForm({
                            name: selectedClassData.name,
                            academicYear: selectedClassData.academicYear || '',
                          });
                          setEditingId(selectedClassData.id);
                          setIsEditing(true);
                          setDialogMode('class');
                          setOpenDialog(true);
                        }}
                        className="px-4 py-2 bg-white text-purple-600 rounded-lg hover:bg-purple-50 text-sm font-semibold transition-all transform hover:scale-105 shadow-lg"
                      >
                        ✏️ Edit
                      </button>
                      <button
                        onClick={() => {
                          if (confirm('Delete this class and all its sections?')) {
                            deleteClassMutation.mutate(selectedClass as string);
                          }
                        }}
                        className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 text-sm font-semibold transition-all transform hover:scale-105 shadow-lg"
                      >
                        🗑️ Delete
                      </button>
                    </div>
                  </div>
                </div>

                <div className="p-6">
                  <div className="flex justify-between items-center mb-6">
                    <h3 className="text-xl font-bold text-gray-900 flex items-center gap-2">
                      <svg className="w-6 h-6 text-purple-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                      </svg>
                      Sections
                    </h3>
                    <button
                      onClick={() => handleOpenSectionDialog()}
                      className="px-4 py-2 bg-gradient-to-r from-purple-500 to-indigo-600 text-white rounded-lg text-sm font-semibold hover:from-purple-600 hover:to-indigo-700 transition-all transform hover:scale-105 shadow-lg"
                    >
                      + Add Section
                    </button>
                  </div>

                  {selectedClassData.sections.length === 0 ? (
                    <div className="text-center py-12">
                      <div className="w-24 h-24 bg-gradient-to-br from-purple-100 to-indigo-100 rounded-full flex items-center justify-center mx-auto mb-4">
                        <svg className="w-12 h-12 text-purple-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                        </svg>
                      </div>
                      <p className="text-gray-600 font-semibold text-lg">No sections yet</p>
                      <p className="text-gray-400 text-sm mt-2">Add your first section to organize students</p>
                      <button
                        onClick={() => handleOpenSectionDialog()}
                        className="mt-4 px-6 py-3 bg-gradient-to-r from-purple-500 to-indigo-600 text-white rounded-lg font-semibold hover:from-purple-600 hover:to-indigo-700 transition-all transform hover:scale-105 shadow-lg"
                      >
                        + Create First Section
                      </button>
                    </div>
                  ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      {selectedClassData.sections.map((section, index) => (
                        <div
                          key={section.id}
                          className="group relative bg-gradient-to-br from-purple-50 to-indigo-50 rounded-xl p-5 border-2 border-purple-200 hover:border-purple-400 hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1"
                        >
                          <div className="absolute top-3 right-3 w-8 h-8 bg-gradient-to-br from-purple-500 to-indigo-600 rounded-lg flex items-center justify-center text-white font-bold text-sm shadow-md">
                            {String.fromCharCode(65 + index)}
                          </div>
                          <div className="pr-10">
                            <div className="flex items-center gap-2 mb-2">
                              <div className="w-10 h-10 bg-gradient-to-br from-purple-500 to-indigo-600 rounded-lg flex items-center justify-center">
                                <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                                </svg>
                              </div>
                              <div>
                                <p className="font-bold text-lg text-gray-900">Section {section.sectionName}</p>
                                <div className="flex items-center gap-1 text-sm text-purple-600">
                                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                  </svg>
                                  <span className="font-semibold">{section.studentCount} students</span>
                                </div>
                              </div>
                            </div>
                            <div className="flex gap-2 mt-4">
                              <button
                                onClick={() => handleOpenSectionDialog(section)}
                                className="flex-1 px-3 py-2 bg-white text-purple-600 border border-purple-300 rounded-lg hover:bg-purple-50 text-sm font-medium transition-all"
                              >
                                ✏️ Edit
                              </button>
                              <button
                                onClick={() => {
                                  if (confirm(`Delete section ${section.sectionName}?`)) {
                                    deleteSectionMutation.mutate(section.id);
                                  }
                                }}
                                className="flex-1 px-3 py-2 bg-white text-red-600 border border-red-300 rounded-lg hover:bg-red-50 text-sm font-medium transition-all"
                              >
                                🗑️ Delete
                              </button>
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <div className="bg-white rounded-2xl shadow-xl border border-gray-100 p-12 text-center">
                <div className="w-32 h-32 bg-gradient-to-br from-purple-100 to-indigo-100 rounded-full flex items-center justify-center mx-auto mb-6">
                  <svg className="w-16 h-16 text-purple-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 5l7 7-7 7" />
                  </svg>
                </div>
                <p className="text-gray-600 font-semibold text-xl mb-2">Select a class to get started</p>
                <p className="text-gray-400">Choose a class from the left panel to view and manage its sections</p>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Dialog */}
      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-60 backdrop-blur-sm flex items-center justify-center z-50 p-4 animate-fadeIn">
          <div className="bg-white rounded-2xl shadow-2xl max-w-md w-full mx-4 transform transition-all animate-slideUp">
            <div className="bg-gradient-to-r from-purple-500 to-indigo-600 p-6 rounded-t-2xl">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-white bg-opacity-20 backdrop-blur-sm rounded-xl flex items-center justify-center">
                  <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    {dialogMode === 'class' ? (
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
                    ) : (
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                    )}
                  </svg>
                </div>
                <h3 className="text-xl font-bold text-white">
                  {dialogMode === 'class'
                    ? isEditing
                      ? '✏️ Edit Class'
                      : '➕ New Class'
                    : isEditing
                    ? '✏️ Edit Section'
                    : '➕ New Section'}
                </h3>
              </div>
            </div>

            <form
              onSubmit={dialogMode === 'class' ? handleSubmitClass : handleSubmitSection}
              className="p-6 space-y-5"
            >
              {dialogMode === 'class' ? (
                <>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Class Name <span className="text-red-500">*</span>
                    </label>
                    <input
                      type="text"
                      required
                      value={classForm.name}
                      onChange={(e) => setClassForm({ ...classForm, name: e.target.value })}
                      placeholder="e.g., Grade 10, Year 5, Form A"
                      className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-semibold text-gray-700 mb-2">
                      Academic Year
                    </label>
                    <input
                      type="text"
                      value={classForm.academicYear}
                      onChange={(e) => setClassForm({ ...classForm, academicYear: e.target.value })}
                      placeholder="e.g., 2024-2025, 2025"
                      className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
                    />
                    <p className="text-xs text-gray-500 mt-1">Optional: Specify the academic year for this class</p>
                  </div>
                </>
              ) : (
                <div>
                  <label className="block text-sm font-semibold text-gray-700 mb-2">
                    Section Name <span className="text-red-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    value={sectionForm.sectionName}
                    onChange={(e) => setSectionForm({ sectionName: e.target.value })}
                    placeholder="e.g., A, B, Alpha, Beta"
                    className="w-full px-4 py-3 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all"
                  />
                  <p className="text-xs text-gray-500 mt-1">Enter a unique identifier for this section</p>
                </div>
              )}

              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={handleCloseDialog}
                  className="flex-1 px-5 py-3 border-2 border-gray-300 text-gray-700 font-semibold rounded-xl hover:bg-gray-50 transition-all transform hover:scale-105"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={
                    dialogMode === 'class'
                      ? createClassMutation.isPending || updateClassMutation.isPending
                      : createSectionMutation.isPending || updateSectionMutation.isPending
                  }
                  className="flex-1 px-5 py-3 bg-gradient-to-r from-purple-500 to-indigo-600 text-white font-semibold rounded-xl hover:from-purple-600 hover:to-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition-all transform hover:scale-105 shadow-lg"
                >
                  {(dialogMode === 'class' ? createClassMutation.isPending || updateClassMutation.isPending : createSectionMutation.isPending || updateSectionMutation.isPending)
                    ? '⏳ Saving...'
                    : `💾 ${dialogMode === 'class' ? 'Save Class' : 'Save Section'}`}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

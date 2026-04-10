import React, { useState, useEffect } from "react";
import { Calendar } from "lucide-react";
import { useExams, useCreateExam, usePublishExam, useDeleteExam, useUpdateExam } from "../hooks/useExamHooks";
import { useNavigate, Outlet, useLocation } from "react-router-dom";
import { api } from "../services/api";
import { useAcademicYear } from "../hooks/useAcademicYear";
import { formatDate } from "../utils/dateFormat";
import "../styles/pages.css";

interface SubjectSelection {
  subjectId: string;
  maxMarks: number;
  passMarks: number;
}

interface CreateExamFormData {
  name: string;
  description: string;
  startDate: string;
  endDate: string;
  totalMarks: number;
  passMarks: number;
  subjectSelections: SubjectSelection[];
  classIds: string[];
}

interface ClassOption {
  id: string;
  name: string;
}

interface SubjectOption {
  id: string;
  name: string;
  code?: string;
}

export function ExamsPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { activeYear, academicYears } = useAcademicYear();
  const selectedYearId = localStorage.getItem('selectedAcademicYearId');
  const sessionName = academicYears?.find(y => y.id === selectedYearId)?.name || activeYear?.name || "current session";

  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingExamId, setEditingExamId] = useState<string | null>(null);
  const [isLoadingExamForEdit, setIsLoadingExamForEdit] = useState(false);
  const [formError, setFormError] = useState<Record<string, string[]> | null>(null);
  const [generalError, setGeneralError] = useState<string | null>(null);
  const [classes, setClasses] = useState<ClassOption[]>([]);
  const [subjects, setSubjects] = useState<SubjectOption[]>([]);
  const [loadingClassesSubjects, setLoadingClassesSubjects] = useState(false);
  const [formData, setFormData] = useState<CreateExamFormData>({
    name: "",
    description: "",
    startDate: "",
    endDate: "",
    totalMarks: 100,
    passMarks: 40,
    subjectSelections: [],
    classIds: [],
  });

  // Check if we're on a nested route (marks, report-cards, or analytics)
  const isNestedRoute = /\/marks|\/report-cards|\/analytics/.test(location.pathname);

  // Queries and Mutations
  const { data: examsData, isLoading, error } = useExams(page, pageSize);
  const createExamMutation = useCreateExam();
  const updateExamMutation = useUpdateExam();
  const publishExamMutation = usePublishExam();
  const deleteExamMutation = useDeleteExam();

  // Fetch classes and subjects when form opens
  useEffect(() => {
    if (showCreateForm && classes.length === 0) {
      setLoadingClassesSubjects(true);
      Promise.all([
        api.get<any>("/v1/classes?pageNumber=1&pageSize=100").then(res => res.data.items || []),
        api.get<SubjectOption[]>("/subjects/active").then(res => res.data),
      ])
        .then(([classesData, subjectsData]) => {
          const classOptions = classesData.map((c: any) => ({ id: c.id, name: c.name }));
          setClasses(classOptions);
          setSubjects(subjectsData || []);
        })
        .catch((err) => {
          console.error("Failed to fetch classes/subjects:", err);
          setGeneralError("Failed to load classes and subjects");
        })
        .finally(() => setLoadingClassesSubjects(false));
    }
  }, [showCreateForm, classes.length]);

  // Auto-calculate Total Marks when subjects change
  useEffect(() => {
    const calculatedTotal = formData.subjectSelections.reduce((sum, subject) => sum + (subject.maxMarks || 0), 0);
    setFormData(prev => ({
      ...prev,
      totalMarks: calculatedTotal || 100,
      passMarks: Math.ceil((calculatedTotal || 100) * 0.4)
    }));
  }, [formData.subjectSelections]);

  const handleToggleClass = (classId: string) => {
    setFormData((prev) => ({
      ...prev,
      classIds: prev.classIds.includes(classId)
        ? prev.classIds.filter((id) => id !== classId)
        : [...prev.classIds, classId],
    }));
  };

  const handleAddSubject = (subjectId: string) => {
    setFormData((prev) => {
      // Check if subject already added
      if (prev.subjectSelections.some(s => s.subjectId === subjectId)) {
        return prev;
      }
      return {
        ...prev,
        subjectSelections: [
          ...prev.subjectSelections,
          { subjectId, maxMarks: 50, passMarks: 20 }
        ],
      };
    });
  };

  const handleRemoveSubject = (subjectId: string) => {
    setFormData((prev) => ({
      ...prev,
      subjectSelections: prev.subjectSelections.filter(s => s.subjectId !== subjectId),
    }));
  };

  const handleUpdateSubjectMaxMarks = (subjectId: string, maxMarks: number) => {
    setFormData((prev) => ({
      ...prev,
      subjectSelections: prev.subjectSelections.map(s =>
        s.subjectId === subjectId ? { ...s, maxMarks } : s
      ),
    }));
  };

  const handleUpdateSubjectPassMarks = (subjectId: string, passMarks: number) => {
    setFormData((prev) => ({
      ...prev,
      subjectSelections: prev.subjectSelections.map(s =>
        s.subjectId === subjectId ? { ...s, passMarks } : s
      ),
    }));
  };

  const handleEditExam = async (examId: string) => {
    const examToEdit = examsData?.data.find(e => e.id === examId);
    if (!examToEdit) return;

    setEditingExamId(examId);
    setIsLoadingExamForEdit(true);
    setShowCreateForm(true);
    
    // Load the exam details
    try {
      const response = await api.get<any>(`/v1/exams/${examId}`);
      const exam = response.data;
      
      setFormData({
        name: exam.name,
        description: exam.description || "",
        startDate: exam.startDate?.split('T')[0] || "",
        endDate: exam.endDate?.split('T')[0] || "",
        totalMarks: exam.totalMarks,
        passMarks: exam.passingMarks || exam.passMarks,
        subjectSelections: exam.subjects?.map((s: any) => ({
          subjectId: s.subjectId,
          maxMarks: s.maxMarks,
          passMarks: s.passMarks || 40,
        })) || [],
        classIds: exam.classes?.map((c: any) => c.classId) || [],
      });
    } catch (err) {
      console.error("Failed to load exam for editing:", err);
      setGeneralError("Failed to load exam details. Please try again.");
      setEditingExamId(null);
      setShowCreateForm(false);
    } finally {
      setIsLoadingExamForEdit(false);
    }
  };

  const handleSaveExam = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setGeneralError(null);
    
    if (formData.classIds.length === 0) {
      setGeneralError("Please select at least one class");
      return;
    }
    if (formData.subjectSelections.length === 0) {
      setGeneralError("Please select at least one subject with max marks");
      return;
    }

    try {
      if (editingExamId) {
        // Update existing exam
        await updateExamMutation.mutateAsync({
          examId: editingExamId,
          data: {
            name: formData.name,
            description: formData.description || undefined,
            startDate: formData.startDate,
            endDate: formData.endDate,
            totalMarks: formData.totalMarks,
            passMarks: formData.passMarks,
            subjects: formData.subjectSelections,
            classIds: formData.classIds,
          }
        });
      } else {
        // Create new exam
        await createExamMutation.mutateAsync({
          name: formData.name,
          description: formData.description || undefined,
          startDate: formData.startDate,
          endDate: formData.endDate,
          totalMarks: formData.totalMarks,
          passMarks: formData.passMarks,
          subjects: formData.subjectSelections,
          classIds: formData.classIds,
        });
      }
      
      setShowCreateForm(false);
      setEditingExamId(null);
      setFormData({
        name: "",
        description: "",
        startDate: "",
        endDate: "",
        totalMarks: 100,
        passMarks: 40,
        subjectSelections: [],
        classIds: [],
      });
    } catch (err: any) {
      console.error("Failed to save exam:", err);
      
      if (err.response?.status === 400 && err.response?.data?.errors) {
        setFormError(err.response.data.errors);
      } else if (err.response?.data?.message) {
        setGeneralError(err.response.data.message);
      } else {
        setGeneralError(`Failed to ${editingExamId ? 'update' : 'create'} exam. Please try again.`);
      }
    }
  };

  const handlePublishExam = async (examId: string) => {
    try {
      await publishExamMutation.mutateAsync(examId);
    } catch (err) {
      console.error("Failed to publish exam:", err);
    }
  };

  const handleDeleteExam = async (examId: string) => {
    if (window.confirm("Are you sure you want to delete this exam? This action cannot be undone.")) {
      try {
        await deleteExamMutation.mutateAsync(examId);
        setGeneralError(null);
        // Success - exam will be removed from list automatically via invalidation
      } catch (err: any) {
        console.error("Failed to delete exam:", err);
        
        // Extract error message
        const errorMessage = err?.response?.data?.message 
          || err?.message 
          || "Failed to delete exam. Please try again.";
        
        setGeneralError(errorMessage);
      }
    }
  };

  const handleNavigateToMarks = (examId: string) => {
    navigate(`/exams/${examId}/marks`);
  };

  const handleNavigateToReportCards = (examId: string) => {
    navigate(`/exams/${examId}/report-cards`);
  };

  const handleNavigateToAnalytics = (examId: string) => {
    navigate(`/exams/${examId}/analytics`);
  };

  if (!activeYear) {
    return (
      <div className="min-h-screen bg-gray-50 p-8 flex items-center justify-center">
        <div className="bg-white rounded-2xl shadow-lg border border-yellow-100 p-8 text-center max-w-md">
          <div className="w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <Calendar className="w-8 h-8 text-yellow-600" />
          </div>
          <h2 className="text-xl font-bold text-gray-900 mb-2">No Academic Session Selected</h2>
          <p className="text-gray-600 mb-6">
            Please select an academic year from the header to manage exams and academic records.
          </p>
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-8 text-center text-gray-600">
            Loading exams...
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-white rounded-2xl shadow-lg border border-red-100 p-8 text-center text-red-700">
            Error loading exams: {error.message}
          </div>
        </div>
      </div>
    );
  }

  // Render nested route (marks or report cards)
  if (isNestedRoute) {
    return <Outlet />;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Exams Management - {sessionName}
              </h1>
              <p className="text-gray-600 mt-2">Create and manage exams and results for the current session</p>
            </div>
            <button
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
              onClick={() => setShowCreateForm(!showCreateForm)}
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              {showCreateForm ? "Close Form" : "Create New Exam"}
            </button>
          </div>

          {showCreateForm && (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6">
              <h2 className="text-xl font-bold text-gray-900">{editingExamId ? "Edit Exam" : "Create Exam"}</h2>
              <form onSubmit={handleSaveExam} className="create-exam-form space-y-6">
            {(generalError || Object.keys(formError || {}).length > 0) && (
              <div className="alert alert-error">
                {generalError && <p>{generalError}</p>}
                {formError && Object.entries(formError).map(([field, messages]) => (
                  <div key={field}>
                    <strong>{field}:</strong>
                    <ul>
                      {messages.map((msg, idx) => (
                        <li key={idx}>{msg}</li>
                      ))}
                    </ul>
                  </div>
                ))}
              </div>
            )}
            <div className="form-row">
              <div className="form-group">
                <label>Exam Name *</label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) =>
                    setFormData({ ...formData, name: e.target.value })
                  }
                  required
                  placeholder="e.g., Semester 1 Final Exam"
                />
              </div>
              <div className="form-group">
                <label>Exam Start Date *</label>
                <input
                  type="date"
                  value={formData.startDate}
                  onChange={(e) =>
                    setFormData({ ...formData, startDate: e.target.value })
                  }
                  required
                />
              </div>
              <div className="form-group">
                <label>Exam End Date *</label>
                <input
                  type="date"
                  value={formData.endDate}
                  onChange={(e) =>
                    setFormData({ ...formData, endDate: e.target.value })
                  }
                  required
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Total Marks <span style={{color: '#3b82f6', fontSize: '0.85em'}}>(Auto-calculated)</span></label>
                <input
                  type="number"
                  value={formData.totalMarks}
                  disabled
                  readOnly
                  className="form-input-disabled"
                  style={{
                    backgroundColor: '#f3f4f6',
                    color: '#6b7280',
                    cursor: 'not-allowed',
                    opacity: 0.7
                  }}
                />
                <small style={{color: '#6b7280', marginTop: '4px', display: 'block'}}>
                  Sum of all subject max marks
                </small>
              </div>
              <div className="form-group">
                <label>Pass Marks <span style={{color: '#3b82f6', fontSize: '0.85em'}}>(Suggested: {Math.ceil(formData.totalMarks * 0.4)})</span></label>
                <input
                  type="number"
                  value={formData.passMarks}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      passMarks: parseInt(e.target.value) || 0,
                    })
                  }
                  placeholder={Math.ceil(formData.totalMarks * 0.4).toString()}
                  required
                  min="0"
                />
                <small style={{color: '#6b7280', marginTop: '4px', display: 'block'}}>
                  40% of total marks is recommended
                </small>
              </div>
            </div>

            {formData.subjectSelections.length > 0 && (
              <div style={{
                marginTop: '16px',
                marginBottom: '16px',
                padding: '12px',
                backgroundColor: '#eff6ff',
                border: '1px solid #bfdbfe',
                borderRadius: '8px'
              }}>
                <p style={{fontSize: '16px', fontWeight: '600', color: '#1e40af'}}>
                  Performance Summary
                </p>
                <p style={{
                  fontSize: '18px',
                  fontWeight: 'bold',
                  color: '#1e40af',
                  marginTop: '8px'
                }}>
                  Total Marks: <span style={{fontSize: '24px'}}>{formData.totalMarks}</span>
                </p>
                <p style={{
                  fontSize: '14px',
                  color: '#1e40af',
                  marginTop: '8px'
                }}>
                  Suggested Pass Marks: <span style={{fontWeight: 'bold', fontSize: '16px'}}>{Math.ceil(formData.totalMarks * 0.4)}</span> (40% of total)
                </p>
              </div>
            )}

            <div className="form-group">
              <label>Description</label>
              <textarea
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
                placeholder="Exam description..."
                rows={2}
              />
            </div>

            <div className="form-group">
              <label>Select Classes *</label>
              {loadingClassesSubjects ? (
                <div className="loading">Loading classes...</div>
              ) : classes.length > 0 ? (
                <div className="checkbox-group">
                  {classes.map((cls) => (
                    <label key={cls.id} className="checkbox-label">
                      <input
                        type="checkbox"
                        checked={formData.classIds.includes(cls.id)}
                        onChange={() => handleToggleClass(cls.id)}
                      />
                      {cls.name}
                    </label>
                  ))}
                </div>
              ) : (
                <div className="alert alert-warning">No classes available</div>
              )}
              {formData.classIds.length === 0 && (
                <small className="text-danger">Please select at least one class</small>
              )}
            </div>

            <div className="form-group">
              <label>Exam Subjects & Max Marks *</label>
              
              {/* Add Subject Selector */}
              <div className="mb-4 p-4 bg-gray-50 rounded-lg">
                <label className="text-sm font-medium text-gray-700 block mb-2">Add Subject *</label>
                <div className="flex gap-2">
                  <select
                    className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    onChange={(e) => {
                      if (e.target.value) {
                        handleAddSubject(e.target.value);
                        e.target.value = "";
                      }
                    }}
                    defaultValue=""
                  >
                    <option value="">-- Select a Subject --</option>
                    {subjects
                      .filter(s => !formData.subjectSelections.some(sel => sel.subjectId === s.id))
                      .map((subject) => (
                        <option key={subject.id} value={subject.id}>
                          {subject.name} {subject.code && `(${subject.code})`}
                        </option>
                      ))}
                  </select>
                </div>
              </div>

              {/* Selected Subjects Table */}
              {formData.subjectSelections.length > 0 ? (
                <div className="overflow-x-auto border border-gray-200 rounded-lg">
                  <table className="w-full text-sm">
                    <thead className="bg-gray-100 border-b">
                      <tr>
                        <th className="px-4 py-2 text-left">Subject</th>
                        <th className="px-4 py-2 text-left">Max Marks</th>
                        <th className="px-4 py-2 text-left">Pass Marks</th>
                        <th className="px-4 py-2 text-center">Action</th>
                      </tr>
                    </thead>
                    <tbody>
                      {formData.subjectSelections.map((selection) => {
                        const subject = subjects.find(s => s.id === selection.subjectId);
                        return (
                          <tr key={selection.subjectId} className="border-b hover:bg-gray-50">
                            <td className="px-4 py-2 font-medium">
                              {subject?.name} {subject?.code && `(${subject.code})`}
                            </td>
                            <td className="px-4 py-2">
                              <input
                                type="number"
                                min="1"
                                max="1000"
                                value={selection.maxMarks}
                                onChange={(e) => handleUpdateSubjectMaxMarks(selection.subjectId, parseFloat(e.target.value) || 0)}
                                className="w-20 px-2 py-1 border border-gray-300 rounded"
                              />
                            </td>
                            <td className="px-4 py-2">
                              <input
                                type="number"
                                min="0"
                                max={selection.maxMarks}
                                value={selection.passMarks}
                                onChange={(e) => handleUpdateSubjectPassMarks(selection.subjectId, parseFloat(e.target.value) || 0)}
                                className="w-20 px-2 py-1 border border-gray-300 rounded"
                              />
                            </td>
                            <td className="px-4 py-2 text-center">
                              <button
                                type="button"
                                onClick={() => handleRemoveSubject(selection.subjectId)}
                                className="px-2 py-1 text-red-600 hover:bg-red-50 rounded transition-colors"
                              >
                                Remove
                              </button>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              ) : (
                <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg text-sm text-blue-700">
                  No subjects added yet. Select a subject above to get started.
                </div>
              )}
              {formData.subjectSelections.length === 0 && (
                <small className="text-danger block mt-2">Please add at least one subject with max marks</small>
              )}
            </div>

            <div className="flex flex-col sm:flex-row gap-3 sm:justify-end">
              <button
                type="submit"
                className="px-5 py-2.5 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg transition-all duration-300 font-medium"
                disabled={createExamMutation.isPending || updateExamMutation.isPending}
              >
                {editingExamId 
                  ? (updateExamMutation.isPending ? "Updating..." : "Update Exam")
                  : (createExamMutation.isPending ? "Creating..." : "Create Exam")
                }
              </button>
              <button
                type="button"
                className="px-5 py-2.5 bg-gray-100 text-gray-700 rounded-xl hover:bg-gray-200 transition-all duration-300 font-medium"
                onClick={() => {
                  setShowCreateForm(false);
                  setEditingExamId(null);
                }}
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {!showCreateForm && examsData && examsData.data.length > 0 ? (
        <>
          {/* Error Message Display */}
          {generalError && (
            <div className="mb-6 bg-red-50 border-l-4 border-red-500 p-4 rounded-lg">
              <div className="flex items-start gap-3">
                <span className="text-red-500 text-xl">⚠️</span>
                <div>
                  <p className="font-semibold text-red-800">Delete Failed</p>
                  <p className="text-red-700 text-sm mt-1">{generalError}</p>
                </div>
                <button
                  onClick={() => setGeneralError(null)}
                  className="ml-auto text-red-500 hover:text-red-700 font-bold"
                >
                  ✕
                </button>
              </div>
            </div>
          )}

          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                  <tr>
                    <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Exam</th>
                    <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Date</th>
                    <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Total</th>
                    <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Pass</th>
                    <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Status</th>
                    <th className="px-6 py-4 text-right text-sm font-bold text-gray-900">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                {examsData.data.map((exam) => {
                  const statusClass =
                    exam.status.toLowerCase() === "published"
                      ? "bg-green-100 text-green-700"
                      : "bg-yellow-100 text-yellow-700";

                  return (
                    <tr key={exam.id} className="hover:bg-blue-50 transition-colors duration-200">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <div className="text-sm font-bold text-gray-900">{exam.name}</div>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-700">
                        {formatDate(exam.startDate)}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-700">
                        {exam.totalMarks}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-700">
                        {exam.passMarks}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap">
                        <span className={`px-3 py-1 rounded-full text-xs font-semibold ${statusClass}`}>
                          {exam.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-right">
                        <div className="flex items-center justify-end gap-1">
                          <button
                            className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                            title="Enter Marks"
                            onClick={() => handleNavigateToMarks(exam.id)}
                          >
                            📝
                          </button>
                          <button
                            className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                            title="View Report Cards"
                            onClick={() => handleNavigateToReportCards(exam.id)}
                          >
                            📊
                          </button>
                          <button
                            className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                            title="View Analytics"
                            onClick={() => handleNavigateToAnalytics(exam.id)}
                          >
                            📈
                          </button>
                          {exam.status !== "Published" && (
                            <button
                              className="p-2 text-yellow-600 hover:bg-yellow-100 rounded-lg transition-all duration-200"
                              title="Edit Exam"
                              onClick={() => handleEditExam(exam.id)}
                              disabled={isLoadingExamForEdit}
                            >
                              ✏️
                            </button>
                          )}
                          {exam.status !== "Published" && (
                            <button
                              className="p-2 text-green-600 hover:bg-green-100 rounded-lg transition-all duration-200"
                              title="Publish Exam"
                              onClick={() => handlePublishExam(exam.id)}
                              disabled={publishExamMutation.isPending}
                            >
                              ✓
                            </button>
                          )}
                          <button
                            className={`p-2 rounded-lg transition-all duration-200 ${
                              exam.status === "Published"
                                ? "text-gray-400 cursor-not-allowed bg-gray-50"
                                : "text-red-600 hover:bg-red-100"
                            }`}
                            title={
                              exam.status === "Published"
                                ? "Cannot delete published exams. Only draft exams can be deleted."
                                : "Delete Exam"
                            }
                            onClick={() => handleDeleteExam(exam.id)}
                            disabled={deleteExamMutation.isPending || exam.status === "Published"}
                          >
                            🗑️
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {examsData.total > pageSize && (
            <div className="flex justify-center items-center gap-4 py-6 border-t border-gray-100">
              <button
                onClick={() => setPage(Math.max(1, page - 1))}
                disabled={page === 1}
                className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-all duration-300 font-medium"
              >
                Previous
              </button>
              <span className="text-sm text-gray-600 font-medium">
                Page {page} of {Math.ceil(examsData.total / pageSize)}
              </span>
              <button
                onClick={() =>
                  setPage(Math.min(Math.ceil(examsData.total / pageSize), page + 1))
                }
                disabled={page >= Math.ceil(examsData.total / pageSize)}
                className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-all duration-300 font-medium"
              >
                Next
              </button>
            </div>
          )}
        </div>
      </>
      ) : !showCreateForm ? (
        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-8 text-center text-gray-600">
          <p>No exams found. Create one to get started!</p>
        </div>
      ) : null}
        </div>
      </div>
    </div>
  );
};

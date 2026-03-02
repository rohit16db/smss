/**
 * ExamsPage Component
 * Single Responsibility: Display and manage exams
 */

import React, { useState, useEffect } from "react";
import { useExams, useCreateExam, usePublishExam, useDeleteExam } from "../hooks/useExamHooks";
import { useNavigate, Outlet, useLocation } from "react-router-dom";
import { api } from "../services/api";
import "../styles/pages.css";

interface CreateExamFormData {
  name: string;
  description: string;
  examDate: string;
  totalMarks: number;
  passMarks: number;
  subjectIds: string[];
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

export const ExamsPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [formError, setFormError] = useState<Record<string, string[]> | null>(null);
  const [generalError, setGeneralError] = useState<string | null>(null);
  const [classes, setClasses] = useState<ClassOption[]>([]);
  const [subjects, setSubjects] = useState<SubjectOption[]>([]);
  const [loadingClassesSubjects, setLoadingClassesSubjects] = useState(false);
  const [formData, setFormData] = useState<CreateExamFormData>({
    name: "",
    description: "",
    examDate: "",
    totalMarks: 100,
    passMarks: 40,
    subjectIds: [],
    classIds: [],
  });

  // Check if we're on a nested route (marks or report-cards)
  const isNestedRoute = /\/marks|\/report-cards/.test(location.pathname);

  // Queries and Mutations
  const { data: examsData, isLoading, error } = useExams(page, pageSize);
  const createExamMutation = useCreateExam();
  const publishExamMutation = usePublishExam();
  const deleteExamMutation = useDeleteExam();

  // Fetch classes and subjects when form opens
  useEffect(() => {
    if (showCreateForm && classes.length === 0) {
      setLoadingClassesSubjects(true);
      Promise.all([
        api.get<any>("/classes?pageNumber=1&pageSize=100").then(res => res.data.items || []),
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

  const handleToggleClass = (classId: string) => {
    setFormData((prev) => ({
      ...prev,
      classIds: prev.classIds.includes(classId)
        ? prev.classIds.filter((id) => id !== classId)
        : [...prev.classIds, classId],
    }));
  };

  const handleToggleSubject = (subjectId: string) => {
    setFormData((prev) => ({
      ...prev,
      subjectIds: prev.subjectIds.includes(subjectId)
        ? prev.subjectIds.filter((id) => id !== subjectId)
        : [...prev.subjectIds, subjectId],
    }));
  };

  const handleCreateExam = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    setGeneralError(null);
    
    // Validate that classes and subjects are selected
    if (formData.classIds.length === 0) {
      setGeneralError("Please select at least one class");
      return;
    }
    if (formData.subjectIds.length === 0) {
      setGeneralError("Please select at least one subject");
      return;
    }
    
    try {
      await createExamMutation.mutateAsync({
        name: formData.name,
        description: formData.description || undefined,
        examDate: formData.examDate,
        totalMarks: formData.totalMarks,
        passMarks: formData.passMarks,
        subjectIds: formData.subjectIds,
        classIds: formData.classIds,
      });
      setShowCreateForm(false);
      setFormData({
        name: "",
        description: "",
        examDate: "",
        totalMarks: 100,
        passMarks: 40,
        subjectIds: [],
        classIds: [],
      });
    } catch (err: any) {
      console.error("Failed to create exam:", err);
      
      // Check if it's a validation error response
      if (err.response?.status === 400 && err.response?.data?.errors) {
        setFormError(err.response.data.errors);
      } else if (err.response?.data?.message) {
        setGeneralError(err.response.data.message);
      } else {
        setGeneralError("Failed to create exam. Please try again.");
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
    if (window.confirm("Are you sure you want to delete this exam?")) {
      try {
        await deleteExamMutation.mutateAsync(examId);
      } catch (err) {
        console.error("Failed to delete exam:", err);
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
                Exams Management
              </h1>
              <p className="text-gray-600 mt-2">Create and manage exams and results</p>
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
              <h2 className="text-xl font-bold text-gray-900">Create Exam</h2>
              <form onSubmit={handleCreateExam} className="create-exam-form space-y-6">
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
                <label>Exam Date *</label>
                <input
                  type="date"
                  value={formData.examDate}
                  onChange={(e) =>
                    setFormData({ ...formData, examDate: e.target.value })
                  }
                  required
                />
              </div>
            </div>

            <div className="form-row">
              <div className="form-group">
                <label>Total Marks *</label>
                <input
                  type="number"
                  value={formData.totalMarks}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      totalMarks: parseInt(e.target.value),
                    })
                  }
                  required
                  min="1"
                />
              </div>
              <div className="form-group">
                <label>Pass Marks *</label>
                <input
                  type="number"
                  value={formData.passMarks}
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      passMarks: parseInt(e.target.value),
                    })
                  }
                  required
                  min="0"
                />
              </div>
            </div>

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
              <label>Select Subjects *</label>
              {loadingClassesSubjects ? (
                <div className="loading">Loading subjects...</div>
              ) : subjects.length > 0 ? (
                <div className="checkbox-group">
                  {subjects.map((subject) => (
                    <label key={subject.id} className="checkbox-label">
                      <input
                        type="checkbox"
                        checked={formData.subjectIds.includes(subject.id)}
                        onChange={() => handleToggleSubject(subject.id)}
                      />
                      {subject.name} {subject.code && `(${subject.code})`}
                    </label>
                  ))}
                </div>
              ) : (
                <div className="alert alert-warning">No subjects available</div>
              )}
              {formData.subjectIds.length === 0 && (
                <small className="text-danger">Please select at least one subject</small>
              )}
            </div>

            <div className="flex flex-col sm:flex-row gap-3 sm:justify-end">
              <button
                type="submit"
                className="px-5 py-2.5 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg transition-all duration-300 font-medium"
                disabled={createExamMutation.isPending}
              >
                {createExamMutation.isPending ? "Creating..." : "Create Exam"}
              </button>
              <button
                type="button"
                className="px-5 py-2.5 bg-gray-100 text-gray-700 rounded-xl hover:bg-gray-200 transition-all duration-300 font-medium"
                onClick={() => setShowCreateForm(false)}
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {!showCreateForm && examsData && examsData.data.length > 0 ? (
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
                        {new Date(exam.examDate).toLocaleDateString()}
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
                              className="p-2 text-green-600 hover:bg-green-100 rounded-lg transition-all duration-200"
                              title="Publish Exam"
                              onClick={() => handlePublishExam(exam.id)}
                              disabled={publishExamMutation.isPending}
                            >
                              ✓
                            </button>
                          )}
                          <button
                            className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-all duration-200"
                            title="Delete Exam"
                            onClick={() => handleDeleteExam(exam.id)}
                            disabled={deleteExamMutation.isPending}
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

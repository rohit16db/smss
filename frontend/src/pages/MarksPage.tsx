/**
 * MarksPage Component
 * Single Responsibility: Display marks entry form and allow entry/submission of marks
 */

import React, { useState, useMemo } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useExam } from "../hooks/useExamHooks";
import { useMarksEntryForm, useSaveMarks, useSubmitMarks, useClassSections } from "../hooks/useMarksHooks";
import type { StudentMarksEntryDto, SubjectMarkEntryDto } from "../services/examApi";
import "../styles/pages.css";

export const MarksPage: React.FC = () => {
  const { examId = "" } = useParams<{ examId: string }>();
  const navigate = useNavigate();
  const [classId, setClassId] = useState<string>("");
  const [sectionId, setSectionId] = useState<string>("");
  const [sectionName, setSectionName] = useState<string>("");
  const [marksData, setMarksData] = useState<Record<string, Record<string, SubjectMarkEntryDto>>>({});

  // Fetch exam details first (includes available classes)
  const { data: examDetails, isLoading: examLoading } = useExam(examId || null);
  
  // Get sections for selected class
  const { data: sections, isLoading: sectionsLoading } = useClassSections(classId || null);

  // Get available classes and form data
  const { data: formData, isLoading, error } = useMarksEntryForm(examId, classId, sectionId || null);
  const saveMutation = useSaveMarks();
  const submitMutation = useSubmitMarks();

  // Clear section when class changes
  React.useEffect(() => {
    setSectionId("");
    setSectionName("");
  }, [classId]);

  // Initialize marks data when form loads
  React.useEffect(() => {
    if (formData) {
      const initialData: Record<string, Record<string, SubjectMarkEntryDto>> = {};
      formData.students.forEach((student) => {
        initialData[student.studentId] = {};
        formData.subjects.forEach((subject) => {
          // Check if there are saved marks for this student-subject combination
          const savedMark = student.subjectMarks?.[subject.id];
          initialData[student.studentId][subject.id] = savedMark || {
            obtained: undefined,
            isAbsent: false,
          };
        });
      });
      setMarksData(initialData);
    }
  }, [formData]);

  const handleMarkChange = (
    studentId: string,
    subjectId: string,
    obtained: number | undefined
  ) => {
    setMarksData({
      ...marksData,
      [studentId]: {
        ...marksData[studentId],
        [subjectId]: {
          ...marksData[studentId][subjectId],
          obtained,
        },
      },
    });
  };

  const handleAbsentToggle = (studentId: string, subjectId: string) => {
    setMarksData({
      ...marksData,
      [studentId]: {
        ...marksData[studentId],
        [subjectId]: {
          ...marksData[studentId][subjectId],
          isAbsent: !marksData[studentId][subjectId].isAbsent,
        },
      },
    });
  };

  const handleSaveMarks = async () => {
    if (!formData) return;

    const marksDataForSubmit: StudentMarksEntryDto[] = Object.entries(marksData).map(
      ([studentId, subjectMarks]) => ({
        studentId,
        subjectMarks,
      })
    );

    try {
      await saveMutation.mutateAsync({
        examId,
        classId,
        sectionId,
        marksData: marksDataForSubmit,
      });
      alert("Marks saved successfully!");
    } catch (err) {
      console.error("Failed to save marks:", err);
    }
  };

  const handleSubmitMarks = async () => {
    if (!window.confirm("Are you sure you want to submit marks? This will generate report cards.")) {
      return;
    }

    try {
      await submitMutation.mutateAsync({ examId, classId, sectionId });
      alert("Marks submitted successfully! Report cards have been generated.");
      navigate("/report-cards/" + examId);
    } catch (err) {
      console.error("Failed to submit marks:", err);
    }
  };

  const statistics = useMemo(() => {
    if (!formData) return { marked: 0, unmarked: 0 };

    let marked = 0;
    let unmarked = 0;

    Object.values(marksData).forEach((studentSubjects) => {
      Object.values(studentSubjects).forEach((mark) => {
        if (mark.isAbsent || mark.obtained !== undefined) {
          marked++;
        } else {
          unmarked++;
        }
      });
    });

    return { marked, unmarked };
  }, [marksData, formData]);

  if (!classId || !sectionId) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="space-y-6">
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
              <div>
                <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                  Marks Entry
                </h1>
                <p className="text-gray-600 mt-2">Select a class and section to enter marks</p>
              </div>
              <button 
                className="px-6 py-3 bg-gray-600 text-white rounded-xl hover:bg-gray-700 transition-colors duration-200 font-medium"
                onClick={() => navigate("/exams")}
              >
                Back to Exams
              </button>
            </div>

            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-8">
              {examLoading && (
                <div className="text-center py-8">
                  <div className="inline-block">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                    <p className="text-gray-600 mt-4">Loading exam details...</p>
                  </div>
                </div>
              )}
              {examDetails && examDetails.classes && examDetails.classes.length > 0 ? (
                <div className="space-y-6">
                  {/* Class Selection */}
                  <div>
                    <label className="block text-lg font-semibold text-gray-900 mb-4">
                      Select Class:
                    </label>
                    <select
                      value={classId}
                      onChange={(e) => setClassId(e.target.value)}
                      className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 transition-colors"
                    >
                      <option value="">-- Select a Class --</option>
                      {examDetails.classes.map((cls) => (
                        <option key={cls.classId} value={cls.classId}>
                          {cls.className} ({cls.studentCount} students)
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Section Selection */}
                  {classId && (
                    <div>
                      <label className="block text-lg font-semibold text-gray-900 mb-4">
                        Select Section:
                      </label>
                      {sectionsLoading ? (
                        <div className="text-center py-4">
                          <div className="inline-block">
                            <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
                            <p className="text-gray-600 mt-2">Loading sections...</p>
                          </div>
                        </div>
                      ) : sections && sections.length > 0 ? (
                        <select
                          value={sectionId}
                          onChange={(e) => {
                            const selected = sections?.find(s => s.id === e.target.value);
                            setSectionId(e.target.value);
                            setSectionName(selected?.sectionName || "");
                          }}
                          className="w-full px-4 py-2 border-2 border-gray-200 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 transition-colors"
                        >
                          <option value="">-- Select a Section --</option>
                          {sections.map((section) => (
                            <option key={section.id} value={section.id}>
                              {section.sectionName}
                            </option>
                          ))}
                        </select>
                      ) : (
                        <div className="bg-amber-50 border-l-4 border-amber-400 p-4 text-amber-800">
                          <p className="font-medium">No sections found</p>
                          <p className="text-sm">This class has no sections.</p>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              ) : (
                !examLoading && (
                  <div className="bg-amber-50 border-l-4 border-amber-400 p-4 text-amber-800">
                    <p className="font-medium">No classes assigned</p>
                    <p className="text-sm">Please assign classes to this exam first.</p>
                  </div>
                )
              )}
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-8 text-center">
            <div className="inline-block">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
              <p className="text-gray-600 mt-4">Loading marks entry form...</p>
            </div>
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
            <p className="font-semibold">Error loading marks form</p>
            <p className="text-sm mt-2">{error.message}</p>
          </div>
        </div>
      </div>
    );
  }

  if (!formData) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-white rounded-2xl shadow-lg border border-red-100 p-8 text-center text-red-700">
            No marks form data available
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Marks Entry
              </h1>
              <p className="text-gray-600 mt-2">
                {formData.examName} • {formData.className} {sectionName && `• ${sectionName}`}
              </p>
            </div>
            <button 
              className="px-6 py-3 bg-gray-600 text-white rounded-xl hover:bg-gray-700 transition-colors duration-200 font-medium"
              onClick={() => navigate("/exams")}
            >
              Back to Exams
            </button>
          </div>

          {/* Statistics Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6">
              <p className="text-gray-600 text-sm font-medium">Total Students</p>
              <p className="text-3xl font-bold text-blue-600 mt-2">{formData.totalStudents}</p>
            </div>
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6">
              <p className="text-gray-600 text-sm font-medium">Marked</p>
              <p className="text-3xl font-bold text-green-600 mt-2">{statistics.marked}</p>
              <p className="text-xs text-gray-500 mt-1">
                {formData.totalStudents > 0 
                  ? `${Math.round((statistics.marked / (formData.totalStudents * formData.subjects.length)) * 100)}%`
                  : "0%"}
              </p>
            </div>
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6">
              <p className="text-gray-600 text-sm font-medium">Pending</p>
              <p className="text-3xl font-bold text-red-600 mt-2">{statistics.unmarked}</p>
              <p className="text-xs text-gray-500 mt-1">
                {formData.totalStudents > 0 
                  ? `${Math.round((statistics.unmarked / (formData.totalStudents * formData.subjects.length)) * 100)}%`
                  : "0%"}
              </p>
            </div>
          </div>

          {/* Marks Table */}
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100">
              <h2 className="text-xl font-bold text-gray-900">Student Marks</h2>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                    <th className="px-6 py-4 text-left text-sm font-semibold text-gray-900">Student Name</th>
                    <th className="px-6 py-4 text-left text-sm font-semibold text-gray-900">Roll #</th>
                    {formData.subjects.map((subject) => (
                      <th key={subject.id} className="px-6 py-4 text-center text-sm font-semibold text-gray-900">
                        <div>{subject.name}</div>
                        <div className="text-xs font-normal text-gray-600">Max: {subject.maxMarks}</div>
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {/* Group students by section */}
                  {(() => {
                    // Group students by section
                    const studentsBySection = formData.students.reduce((acc, student) => {
                      const sectionKey = student.sectionName || "Unassigned";
                      if (!acc[sectionKey]) {
                        acc[sectionKey] = [];
                      }
                      acc[sectionKey].push(student);
                      return acc;
                    }, {} as Record<string, typeof formData.students>);

                    return Object.entries(studentsBySection).map(([sectionName, sectionStudents]) => (
                      <React.Fragment key={sectionName}>
                        {/* Section Header Row */}
                        <tr className="bg-gradient-to-r from-blue-100 to-indigo-100 border-b-2 border-blue-200">
                          <td colSpan={2 + formData.subjects.length} className="px-6 py-3">
                            <h3 className="font-semibold text-gray-800 text-base">{sectionName}</h3>
                          </td>
                        </tr>
                        {/* Students in section */}
                        {sectionStudents.map((student) => (
                          <tr key={student.studentId} className="hover:bg-blue-50 transition-colors">
                            <td className="px-6 py-4 text-sm font-medium text-gray-900">{student.studentName}</td>
                            <td className="px-6 py-4 text-sm text-gray-600">{student.rollNumber || "-"}</td>
                            {formData.subjects.map((subject) => {
                              const markData = marksData[student.studentId]?.[subject.id];
                              return (
                                <td 
                                  key={`${student.studentId}-${subject.id}`} 
                                  className="px-6 py-4 text-center"
                                >
                                  <div className="space-y-2">
                                    {markData?.isAbsent ? (
                                      <div className="space-y-2">
                                        <span className="inline-block px-3 py-1 bg-red-100 text-red-700 rounded-lg text-sm font-medium">
                                          Absent
                                        </span>
                                        <div className="flex items-center justify-center gap-2">
                                          <input
                                            type="checkbox"
                                            checked={true}
                                            onChange={() => handleAbsentToggle(student.studentId, subject.id)}
                                            className="w-4 h-4 accent-blue-600 cursor-pointer"
                                            title="Mark as present"
                                          />
                                          <label className="text-xs text-gray-600 cursor-pointer">Absent</label>
                                        </div>
                                      </div>
                                    ) : (
                                      <>
                                        <input
                                          type="number"
                                          min="0"
                                          max={subject.maxMarks}
                                          value={markData?.obtained ?? ""}
                                          onChange={(e) =>
                                            handleMarkChange(
                                              student.studentId,
                                              subject.id,
                                              e.target.value ? parseInt(e.target.value) : undefined
                                            )
                                          }
                                          placeholder="Mark"
                                          className="w-full px-2 py-2 border-2 border-gray-200 rounded-lg focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 text-center"
                                        />
                                        <div className="flex items-center justify-center gap-2">
                                          <input
                                            type="checkbox"
                                            checked={false}
                                            onChange={() => handleAbsentToggle(student.studentId, subject.id)}
                                            className="w-4 h-4 accent-blue-600 cursor-pointer"
                                            title="Mark as absent"
                                          />
                                          <label className="text-xs text-gray-600 cursor-pointer">Absent</label>
                                        </div>
                                      </>
                                    )}
                                  </div>
                                </td>
                              );
                            })}
                          </tr>
                        ))}
                      </React.Fragment>
                    ));
                  })()}
                </tbody>
              </table>
            </div>
          </div>

          {/* Actions */}
          <div className="flex flex-col sm:flex-row gap-4 justify-end">
            <button
              className="px-6 py-3 bg-gray-600 text-white rounded-xl hover:bg-gray-700 transition-colors duration-200 font-medium"
              onClick={() => navigate("/exams")}
            >
              Cancel
            </button>
            <button
              className="px-6 py-3 bg-blue-600 text-white rounded-xl hover:shadow-lg hover:bg-blue-700 transition-all duration-200 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
              onClick={handleSaveMarks}
              disabled={saveMutation.isPending}
            >
              {saveMutation.isPending ? "Saving..." : "Save Marks (Draft)"}
            </button>
            <button
              className="px-6 py-3 bg-gradient-to-r from-green-600 to-green-700 text-white rounded-xl hover:shadow-lg transition-all duration-200 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
              onClick={handleSubmitMarks}
              disabled={submitMutation.isPending || statistics.unmarked > 0}
              title={statistics.unmarked > 0 ? "All marks must be entered before submitting" : ""}
            >
              {submitMutation.isPending ? "Submitting..." : "Submit & Generate Report Cards"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

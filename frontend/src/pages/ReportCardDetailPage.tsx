/**
 * ReportCardDetailPage Component
 * Displays a full report card with detailed marks, summary, and export options
 */

import React, { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useReportCard } from "../hooks/useReportCardHooks";
import { useMutation } from "@tanstack/react-query";
import examApi from "../services/examApi";
import "../styles/pages.css";

export const ReportCardDetailPage: React.FC = () => {
  const { examId, studentId } = useParams<{ examId: string; studentId: string }>();
  const navigate = useNavigate();
  const [isExporting, setIsExporting] = useState(false);

  // Fetch report card data by exam and student ID
  const { data: reportCard, isLoading, error } = useReportCard(examId || null, studentId || null);

  // Export PDF mutation
  const exportMutation = useMutation({
    mutationFn: (cardId: string) => examApi.reportCard.exportReportCardPdf(cardId),
    onSuccess: (data) => {
      // Create a blob and download
      const blob = new Blob([data], { type: "application/pdf" });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `report-card-${examId}-${studentId}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    },
    onError: (err) => {
      alert("Failed to export PDF. Please try again.");
      console.error("PDF export error:", err);
    },
  });

  const handleExportPdf = async () => {
    if (!reportCard?.id) return;
    setIsExporting(true);
    try {
      await exportMutation.mutateAsync(reportCard.id);
    } finally {
      setIsExporting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="flex items-center justify-center min-h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </div>
    );
  }

  if (error || !reportCard) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
          <p className="text-red-600 font-medium">
            {error?.message || "Report card not found"}
          </p>
          <button
            className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition"
            onClick={() => navigate("/exams")}
          >
            Back to Exams
          </button>
        </div>
      </div>
    );
  }

  const statusBg = reportCard.summary.status.toLowerCase() === "pass" ? "bg-green-50" : "bg-red-50";
  const statusTextColor =
    reportCard.summary.status.toLowerCase() === "pass" ? "text-green-600" : "text-red-600";
  const statusBgColor =
    reportCard.summary.status.toLowerCase() === "pass" ? "bg-green-200" : "bg-red-200";

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className={`${statusBg} rounded-2xl shadow-lg border border-gray-200`}>
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 p-6 sm:p-8 border-b border-gray-200">
          <div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
              Report Card
            </h1>
            <p className="text-gray-600 mt-2">{reportCard.examName} • {reportCard.studentName}</p>
          </div>
          <div className="flex gap-3">
            <button
              className="px-6 py-3 bg-gray-600 text-white rounded-xl hover:bg-gray-700 transition-colors font-medium"
              onClick={() => navigate("/exams")}
            >
              Back
            </button>
            <button
              className="px-6 py-3 bg-blue-600 text-white rounded-xl hover:bg-blue-700 transition-colors font-medium disabled:opacity-50"
              onClick={handleExportPdf}
              disabled={isExporting}
            >
              {isExporting ? "Exporting..." : "Export PDF"}
            </button>
          </div>
        </div>

        {/* Student Information */}
        <div className="p-6 sm:p-8 border-b border-gray-200">
          <h2 className="text-2xl font-bold text-gray-800 mb-4">Student Information</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
            <div>
              <p className="text-sm text-gray-600 font-medium">Name</p>
              <p className="text-lg font-semibold text-gray-900">{reportCard.studentName}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Roll Number</p>
              <p className="text-lg font-semibold text-gray-900">{reportCard.rollNumber}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Class</p>
              <p className="text-lg font-semibold text-gray-900">{reportCard.className}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600 font-medium">Exam Date</p>
              <p className="text-lg font-semibold text-gray-900">
                {new Date(reportCard.examDate).toLocaleDateString()}
              </p>
            </div>
          </div>
        </div>

        {/* Summary Cards */}
        <div className="p-6 sm:p-8 border-b border-gray-200">
          <h2 className="text-2xl font-bold text-gray-800 mb-6">Performance Summary</h2>
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
            {/* Total Marks */}
            <div className="bg-blue-50 rounded-xl p-4 border border-blue-200">
              <p className="text-xs text-gray-600 font-medium">Total Marks</p>
              <p className="text-2xl font-bold text-blue-600 mt-2">
                {reportCard.summary.totalObtained.toFixed(0)}
              </p>
              <p className="text-xs text-gray-500 mt-1">
                out of {reportCard.summary.totalMarks}
              </p>
            </div>

            {/* Percentage */}
            <div className="bg-green-50 rounded-xl p-4 border border-green-200">
              <p className="text-xs text-gray-600 font-medium">Percentage</p>
              <p className="text-2xl font-bold text-green-600 mt-2">
                {reportCard.summary.percentage.toFixed(2)}%
              </p>
            </div>

            {/* Grade */}
            <div className="bg-amber-50 rounded-xl p-4 border border-amber-200">
              <p className="text-xs text-gray-600 font-medium">Grade</p>
              <p className="text-2xl font-bold text-amber-600 mt-2">
                {reportCard.summary.overallGrade}
              </p>
            </div>

            {/* Class Rank */}
            <div className="bg-purple-50 rounded-xl p-4 border border-purple-200">
              <p className="text-xs text-gray-600 font-medium">Class Rank</p>
              <p className="text-2xl font-bold text-purple-600 mt-2">
                #{reportCard.summary.classPosition}
              </p>
            </div>

            {/* Status */}
            <div className={`${statusBgColor} rounded-xl p-4 border-2 border-current`}>
              <p className="text-xs text-gray-600 font-medium">Status</p>
              <p className={`text-2xl font-bold ${statusTextColor} mt-2`}>
                {reportCard.summary.status.toUpperCase()}
              </p>
            </div>
          </div>
        </div>

        {/* Subject-wise Marks */}
        <div className="p-6 sm:p-8">
          <h2 className="text-2xl font-bold text-gray-800 mb-6">Subject-wise Marks</h2>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="bg-gradient-to-r from-blue-600 to-blue-800">
                  <th className="px-6 py-4 text-left text-sm font-semibold text-white">
                    Subject
                  </th>
                  <th className="px-6 py-4 text-right text-sm font-semibold text-white">
                    Max Marks
                  </th>
                  <th className="px-6 py-4 text-right text-sm font-semibold text-white">
                    Obtained
                  </th>
                  <th className="px-6 py-4 text-right text-sm font-semibold text-white">
                    Percentage
                  </th>
                  <th className="px-6 py-4 text-center text-sm font-semibold text-white">
                    Grade
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {reportCard.subjectMarks.map((subject, index) => (
                  <tr
                    key={subject.subjectId}
                    className={index % 2 === 0 ? "bg-white" : "bg-gray-50"}
                  >
                    <td className="px-6 py-4 text-sm font-medium text-gray-900">
                      {subject.subjectName}
                    </td>
                    <td className="px-6 py-4 text-right text-sm text-gray-700">
                      {subject.maxMarks}
                    </td>
                    <td className="px-6 py-4 text-right text-sm font-semibold text-gray-900">
                      {subject.obtained.toFixed(1)}
                    </td>
                    <td className="px-6 py-4 text-right text-sm text-gray-700">
                      {subject.percentage.toFixed(2)}%
                    </td>
                    <td className="px-6 py-4 text-center">
                      <span className="inline-block px-3 py-1 rounded-full text-sm font-semibold bg-blue-100 text-blue-700">
                        {subject.grade || "—"}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Footer */}
        <div className="bg-gray-100 px-6 sm:px-8 py-4 text-center border-t border-gray-200 rounded-b-2xl">
          <p className="text-sm text-gray-600">
            Report Card Generated on {new Date(reportCard.generatedAt).toLocaleDateString()}
          </p>
        </div>
      </div>
    </div>
  );
};

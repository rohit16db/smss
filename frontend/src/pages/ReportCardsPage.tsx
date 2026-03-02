/**
 * ReportCardsPage Component
 * Single Responsibility: Display report cards with filtering, sorting, and export
 */

import React, { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  useExamReportCards,
  useReportCard,
  useExportReportCardPdf,
} from "../hooks/useReportCardHooks";

export const ReportCardsPage: React.FC = () => {
  const { examId = "" } = useParams<{ examId: string }>();
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [status, setStatus] = useState<string>();
  const [sortBy, setSortBy] = useState("classPosition");
  const [sortOrder, setSortOrder] = useState("asc");
  const [selectedStudentId, setSelectedStudentId] = useState<string | null>(null);

  // Queries
  const { data: reportCardsData, isLoading, error } = useExamReportCards(
    examId,
    undefined,
    status,
    sortBy,
    sortOrder,
    page,
    pageSize
  );

  const { data: selectedReportCard } = useReportCard(
    selectedStudentId ? examId : null,
    selectedStudentId
  );

  const exportMutation = useExportReportCardPdf();

  const handleExportPdf = async (cardId: string, studentName: string) => {
    try {
      await exportMutation.mutateAsync({ cardId, studentName, examId });
    } catch (err) {
      console.error("Failed to export PDF:", err);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-8 text-center">
            <div className="text-gray-600 text-lg font-medium">Loading report cards...</div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-red-50 rounded-2xl shadow-lg border border-red-200 p-8">
            <p className="text-red-700 font-medium">Error loading report cards: {error.message}</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Page Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
          <div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
              Report Cards
            </h1>
            <p className="text-gray-600 mt-2 text-sm">View and manage student report cards by marks and performance</p>
          </div>
          <button
            onClick={() => navigate("/exams")}
            className="px-6 py-3 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-all duration-300 font-medium shadow-md hover:shadow-lg"
          >
            ← Back to Exams
          </button>
        </div>

        {/* Filters Section */}
        <div className="bg-white rounded-2xl shadow-md border border-gray-100 p-6 mb-8">
          <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
            <div className="flex flex-col">
              <label className="text-sm font-semibold text-gray-700 mb-2">Status Filter</label>
              <select
                value={status || ""}
                onChange={(e) => {
                  setStatus(e.target.value || undefined);
                  setPage(1);
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white text-gray-700 font-medium"
              >
                <option value="">All Students</option>
                <option value="pass">Passed Only</option>
                <option value="fail">Failed Only</option>
              </select>
            </div>

            <div className="flex flex-col">
              <label className="text-sm font-semibold text-gray-700 mb-2">Sort By</label>
              <select
                value={sortBy}
                onChange={(e) => {
                  setSortBy(e.target.value);
                  setPage(1);
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white text-gray-700 font-medium"
              >
                <option value="classPosition">Class Rank</option>
                <option value="name">Student Name</option>
                <option value="percentage">Percentage</option>
              </select>
            </div>

            <div className="flex flex-col">
              <label className="text-sm font-semibold text-gray-700 mb-2">Order</label>
              <select
                value={sortOrder}
                onChange={(e) => {
                  setSortOrder(e.target.value);
                  setPage(1);
                }}
                className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white text-gray-700 font-medium"
              >
                <option value="asc">Ascending ↑</option>
                <option value="desc">Descending ↓</option>
              </select>
            </div>

            <div className="flex flex-col justify-end">
              <button
                onClick={() => {
                  setStatus(undefined);
                  setSortBy("classPosition");
                  setSortOrder("asc");
                  setPage(1);
                }}
                className="px-4 py-2 bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition-all duration-300 font-medium border border-blue-300"
              >
                Reset Filters
              </button>
            </div>
          </div>
        </div>

        {/* Report Cards Grid */}
        {reportCardsData && reportCardsData.data.length > 0 ? (
          <>
            {/* Table View */}
            <div className="bg-white rounded-2xl shadow-md border border-gray-100 overflow-hidden mb-8">
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="bg-gradient-to-r from-blue-50 to-blue-100 border-b border-gray-200">
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Rank</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Student Name</th>
                      <th className="px-6 py-4 text-left text-sm font-bold text-gray-900">Class</th>
                      <th className="px-6 py-4 text-right text-sm font-bold text-gray-900">Marks Obtained</th>
                      <th className="px-6 py-4 text-right text-sm font-bold text-gray-900">Percentage</th>
                      <th className="px-6 py-4 text-center text-sm font-bold text-gray-900">Grade</th>
                      <th className="px-6 py-4 text-center text-sm font-bold text-gray-900">Status</th>
                      <th className="px-6 py-4 text-center text-sm font-bold text-gray-900">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {reportCardsData.data.map((card) => {
                      const gradeColorMap: Record<string, string> = {
                        'A': 'bg-green-100 text-green-800',
                        'B': 'bg-blue-100 text-blue-800',
                        'C': 'bg-yellow-100 text-yellow-800',
                        'D': 'bg-orange-100 text-orange-800',
                        'F': 'bg-red-100 text-red-800',
                      };
                      const statusColor = card.status.toLowerCase() === 'pass' 
                        ? 'bg-green-100 text-green-800' 
                        : 'bg-red-100 text-red-800';

                      return (
                        <tr key={card.id} className="hover:bg-blue-50 transition-colors duration-200">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex items-center justify-center">
                              <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-gradient-to-r from-blue-500 to-blue-600 text-white font-bold text-sm">
                                {card.classPosition}
                              </span>
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm font-semibold text-gray-900">{card.studentName}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-700">
                            {card.className}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-right">
                            <div className="text-sm font-semibold text-gray-900">
                              {card.totalObtained} / {card.totalMarks}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-right">
                            <div className="flex items-center justify-end gap-2">
                              <div className="text-right">
                                <div className="text-sm font-bold text-gray-900">{card.percentage.toFixed(2)}%</div>
                                <div className="w-16 bg-gray-200 rounded-full h-2 mt-1">
                                  <div
                                    className="bg-gradient-to-r from-blue-500 to-blue-600 h-2 rounded-full"
                                    style={{ width: `${Math.min(card.percentage, 100)}%` }}
                                  ></div>
                                </div>
                              </div>
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-center">
                            <span className={`px-3 py-1 rounded-full text-sm font-bold ${gradeColorMap[card.overallGrade] || 'bg-gray-100 text-gray-800'}`}>
                              {card.overallGrade}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-center">
                            <span className={`px-3 py-1 rounded-full text-xs font-semibold ${statusColor}`}>
                              {card.status}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-center">
                            <div className="flex items-center justify-center gap-2">
                              <button
                                onClick={() => setSelectedStudentId(card.studentId)}
                                className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200 font-medium"
                                title="View Details"
                              >
                                👁️ View
                              </button>
                              <button
                                onClick={() => handleExportPdf(card.id, card.studentName)}
                                disabled={exportMutation.isPending}
                                className="p-2 text-green-600 hover:bg-green-100 rounded-lg transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed font-medium"
                                title="Download PDF"
                              >
                                📥 PDF
                              </button>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Pagination */}
            {reportCardsData.total > pageSize && (
              <div className="flex justify-center items-center gap-4 py-6">
                <button
                  onClick={() => setPage(Math.max(1, page - 1))}
                  disabled={page === 1}
                  className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-all duration-300 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  ← Previous
                </button>
                <span className="text-sm text-gray-600 font-semibold">
                  Page {page} of {Math.ceil(reportCardsData.total / pageSize)}
                </span>
                <button
                  onClick={() =>
                    setPage(Math.min(Math.ceil(reportCardsData.total / pageSize), page + 1))
                  }
                  disabled={page >= Math.ceil(reportCardsData.total / pageSize)}
                  className="px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-all duration-300 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Next →
                </button>
              </div>
            )}
          </>
        ) : (
          <div className="bg-white rounded-2xl shadow-md border border-gray-100 p-12 text-center">
            <p className="text-gray-600 text-lg">📭 No report cards found</p>
            <p className="text-gray-500 text-sm mt-2">Make sure marks have been submitted for this exam.</p>
          </div>
        )}

        {/* Detailed Report Card Modal */}
        {selectedReportCard && (
          <div 
            className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
            onClick={() => setSelectedStudentId(null)}
          >
            <div 
              className="bg-white rounded-2xl shadow-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto"
              onClick={(e) => e.stopPropagation()}
            >
              {/* Modal Header */}
              <div className="bg-gradient-to-r from-blue-600 to-blue-800 px-8 py-6 flex justify-between items-center border-b border-gray-200">
                <div>
                  <h2 className="text-2xl font-bold text-white">{selectedReportCard.studentName}</h2>
                  <p className="text-blue-100 text-sm mt-1">Report Card Details</p>
                </div>
                <button
                  onClick={() => setSelectedStudentId(null)}
                  className="text-white hover:bg-blue-700 rounded-lg p-2 transition-all duration-200 text-2xl"
                >
                  ✕
                </button>
              </div>

              {/* Modal Body */}
              <div className="p-8">
                {/* Summary Cards Grid */}
                <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
                  <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-lg p-4 border border-blue-200">
                    <p className="text-gray-600 text-sm font-medium mb-1">Class</p>
                    <p className="text-2xl font-bold text-blue-900">{selectedReportCard.className}</p>
                  </div>
                  <div className="bg-gradient-to-br from-purple-50 to-purple-100 rounded-lg p-4 border border-purple-200">
                    <p className="text-gray-600 text-sm font-medium mb-1">Rank</p>
                    <p className="text-2xl font-bold text-purple-900">{selectedReportCard.summary.classPosition}</p>
                  </div>
                  <div className="bg-gradient-to-br from-green-50 to-green-100 rounded-lg p-4 border border-green-200">
                    <p className="text-gray-600 text-sm font-medium mb-1">Status</p>
                    <p className={`text-lg font-bold ${selectedReportCard.summary.status === 'Pass' ? 'text-green-900' : 'text-red-900'}`}>
                      {selectedReportCard.summary.status}
                    </p>
                  </div>
                  <div className="bg-gradient-to-br from-yellow-50 to-yellow-100 rounded-lg p-4 border border-yellow-200">
                    <p className="text-gray-600 text-sm font-medium mb-1">Grade</p>
                    <p className="text-2xl font-bold text-yellow-900">{selectedReportCard.summary.overallGrade}</p>
                  </div>
                  <div className="bg-gradient-to-br from-orange-50 to-orange-100 rounded-lg p-4 border border-orange-200">
                    <p className="text-gray-600 text-sm font-medium mb-1">Percentage</p>
                    <p className="text-2xl font-bold text-orange-900">{selectedReportCard.summary.percentage.toFixed(2)}%</p>
                  </div>
                </div>

                {/* Overall Performance */}
                <div className="bg-gray-50 rounded-lg p-6 mb-8 border border-gray-200">
                  <h3 className="text-lg font-bold text-gray-900 mb-4">Overall Performance</h3>
                  <div className="grid grid-cols-2 gap-6">
                    <div>
                      <p className="text-gray-600 text-sm mb-2">Marks Obtained</p>
                      <p className="text-3xl font-bold text-blue-600">
                        {selectedReportCard.summary.totalObtained}/{selectedReportCard.summary.totalMarks}
                      </p>
                    </div>
                    <div>
                      <p className="text-gray-600 text-sm mb-2">Performance</p>
                      <div className="flex items-center gap-4">
                        <div className="flex-1">
                          <div className="w-full bg-gray-300 rounded-full h-3">
                            <div
                              className="bg-gradient-to-r from-blue-500 to-blue-600 h-3 rounded-full transition-all duration-500"
                              style={{ width: `${Math.min(selectedReportCard.summary.percentage, 100)}%` }}
                            ></div>
                          </div>
                        </div>
                        <span className="text-lg font-bold text-gray-900 min-w-fit">{selectedReportCard.summary.percentage.toFixed(2)}%</span>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Subject-wise Details */}
                <div>
                  <h3 className="text-lg font-bold text-gray-900 mb-4">Subject-wise Details</h3>
                  <div className="overflow-x-auto rounded-lg border border-gray-200">
                    <table className="w-full">
                      <thead>
                        <tr className="bg-gradient-to-r from-gray-100 to-gray-50 border-b border-gray-200">
                          <th className="px-6 py-3 text-left text-sm font-bold text-gray-900">Subject</th>
                          <th className="px-6 py-3 text-right text-sm font-bold text-gray-900">Marks Obtained</th>
                          <th className="px-6 py-3 text-right text-sm font-bold text-gray-900">Max Marks</th>
                          <th className="px-6 py-3 text-right text-sm font-bold text-gray-900">Percentage</th>
                          <th className="px-6 py-3 text-center text-sm font-bold text-gray-900">Grade</th>
                          <th className="px-6 py-3 text-center text-sm font-bold text-gray-900">Status</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-100">
                        {selectedReportCard.subjectMarks.map((subject) => {
                          const gradeColorMap: Record<string, string> = {
                            'A': 'bg-green-100 text-green-800',
                            'B': 'bg-blue-100 text-blue-800',
                            'C': 'bg-yellow-100 text-yellow-800',
                            'D': 'bg-orange-100 text-orange-800',
                            'F': 'bg-red-100 text-red-800',
                          };
                          const statusColor = subject.percentage >= 40
                            ? 'bg-green-100 text-green-800'
                            : 'bg-red-100 text-red-800';

                          return (
                            <tr key={subject.subjectId} className="hover:bg-blue-50 transition-colors duration-200">
                              <td className="px-6 py-4 text-sm font-semibold text-gray-900">{subject.subjectName}</td>
                              <td className="px-6 py-4 text-right text-sm font-medium text-gray-700">{subject.obtained}</td>
                              <td className="px-6 py-4 text-right text-sm font-medium text-gray-700">{subject.maxMarks}</td>
                              <td className="px-6 py-4 text-right text-sm font-bold text-gray-900">{subject.percentage.toFixed(2)}%</td>
                              <td className="px-6 py-4 text-center">
                                <span className={`px-3 py-1 rounded-full text-sm font-bold ${gradeColorMap[subject.grade] || 'bg-gray-100 text-gray-800'}`}>
                                  {subject.grade}
                                </span>
                              </td>
                              <td className="px-6 py-4 text-center">
                                <span className={`px-3 py-1 rounded-full text-xs font-semibold ${statusColor}`}>
                                  {subject.percentage >= 40 ? 'Pass' : 'Fail'}
                                </span>
                              </td>
                            </tr>
                          );
                        })}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>

              {/* Modal Footer */}
              <div className="bg-gray-50 border-t border-gray-200 px-8 py-4 flex justify-end gap-3">
                <button
                  onClick={() => setSelectedStudentId(null)}
                  className="px-6 py-2 bg-gray-300 text-gray-900 rounded-lg hover:bg-gray-400 transition-all duration-300 font-medium"
                >
                  Close
                </button>
                <button
                  onClick={() => handleExportPdf(selectedReportCard.id, selectedReportCard.studentName)}
                  disabled={exportMutation.isPending}
                  className="px-6 py-2 bg-gradient-to-r from-blue-600 to-blue-800 text-white rounded-lg hover:shadow-lg transition-all duration-300 font-medium disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {exportMutation.isPending ? "⏳ Exporting..." : "📥 Export as PDF"}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

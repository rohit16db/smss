/**
 * PerformanceAnalyticsPage Component (Phase 2)
 * Single Responsibility: Display comprehensive analytics and performance metrics dashboard
 */

import React, { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  useExamAnalytics,
  useClassPerformance,
  useMarksDistribution,
  useExamComparison,
  useClasses,
} from "../hooks/useAnalyticsHooks";
import {
  GradeDistributionChart,
  MarksDistributionChart,
  ExamTrendChart,
  PerformanceCard,
  TopPerformersTable,
  SubjectAnalysisTable,
} from "../components/analytics/AnalyticsComponents";
import type { ClassListDto } from "../services/api";
import "../styles/pages.css";
import "../components/analytics/analytics.css";

export const PerformanceAnalyticsPage: React.FC = () => {
  const { examId = "" } = useParams<{ examId: string }>();
  const navigate = useNavigate();
  const [classId, setClassId] = useState<string>("");
  const [selectedTab, setSelectedTab] = useState<"overview" | "class" | "trend">(
    "overview"
  );

  // Fetch available classes
  const { data: classesData } = useClasses();

  // Queries
  const { data: examAnalytics, isLoading: analyticsLoading, error: analyticsError } = useExamAnalytics(examId, classId || undefined);
  const { data: classPerformance, isLoading: classLoading } = useClassPerformance(
    classId || null,
    examId || null
  );
  const { data: marksDistribution, isLoading: distLoading } = useMarksDistribution(
    examId || null,
    classId || undefined
  );
  const { data: examComparison, isLoading: trendLoading } = useExamComparison(
    classId || null
  );

  const isLoading = analyticsLoading || classLoading || distLoading || trendLoading;

  // Determine if marks distribution should be shown
  const showMarksDistribution = marksDistribution && marksDistribution.total > 0 && marksDistribution.buckets && marksDistribution.buckets.length > 0;

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="flex justify-center items-center h-96">
            <div className="text-center">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
              <p className="text-gray-600">Loading analytics...</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (analyticsError) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
            <h2 className="text-xl font-bold text-red-800 mb-2">Error Loading Analytics</h2>
            <p className="text-red-700 mb-4">{analyticsError?.message || "Unknown error"}</p>
            <button
              className="px-6 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
              onClick={() => navigate("/exams")}
            >
              Back to Exams
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (!examAnalytics) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-6 text-center">
            <p className="text-yellow-800">No analytics data available</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Page Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Performance Analytics
              </h1>
              <p className="text-gray-600 mt-2">{examAnalytics.examName}</p>
            </div>
            <button
              onClick={() => navigate("/exams")}
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
            >
              ← Back to Exams
            </button>
          </div>

          {/* Class Selector */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
            <label htmlFor="classSelect" className="block text-sm font-medium text-gray-700 mb-2">
              Select Class (Optional - for detailed analysis)
            </label>
            <select
              id="classSelect"
              value={classId}
              onChange={(e) => setClassId(e.target.value)}
              className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-blue-500 focus:border-blue-500 bg-white text-gray-900"
            >
              <option value="">-- All Classes --</option>
              {classesData?.items?.map((cls: ClassListDto) => (
                <option key={cls.id} value={cls.id}>
                  {cls.name}
                </option>
              ))}
            </select>
            <p className="text-xs text-gray-500 mt-1">
              Select a specific class to view Class Performance and Exam Trends analysis
            </p>
          </div>

          {/* Summary Cards Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
            <PerformanceCard
              label="Total Students"
              value={examAnalytics.totalStudents}
              color="primary"
            />
            <PerformanceCard
              label="Passed"
              value={examAnalytics.passedStudents}
              color="success"
            />
            <PerformanceCard
              label="Failed"
              value={examAnalytics.failedStudents}
              color="danger"
            />
            <PerformanceCard
              label="Pass Rate"
              value={`${examAnalytics.passRate.toFixed(1)}%`}
              color="info"
            />
            <PerformanceCard
              label="Class Average"
              value={`${examAnalytics.classAverage.toFixed(2)}%`}
              color="primary"
            />
          </div>

          {/* Tabs Navigation */}
          <div className="bg-white rounded-lg shadow-sm border border-gray-200">
            <div className="flex gap-0 border-b border-gray-200">
              <button
                className={`flex-1 px-6 py-4 text-sm font-medium transition-colors ${
                  selectedTab === "overview"
                    ? "text-blue-600 border-b-2 border-blue-600 bg-blue-50"
                    : "text-gray-600 hover:text-gray-900"
                }`}
                onClick={() => setSelectedTab("overview")}
              >
                📊 Overview
              </button>
              <button
                className={`flex-1 px-6 py-4 text-sm font-medium transition-colors ${
                  selectedTab === "class"
                    ? "text-blue-600 border-b-2 border-blue-600 bg-blue-50"
                    : "text-gray-600 hover:text-gray-900"
                }`}
                onClick={() => setSelectedTab("class")}
              >
                🏫 Class Performance
              </button>
              <button
                className={`flex-1 px-6 py-4 text-sm font-medium transition-colors ${
                  selectedTab === "trend"
                    ? "text-blue-600 border-b-2 border-blue-600 bg-blue-50"
                    : "text-gray-600 hover:text-gray-900"
                }`}
                onClick={() => setSelectedTab("trend")}
              >
                📈 Exam Trends
              </button>
            </div>

            {/* Tab Content */}
            <div className="p-6">
              {selectedTab === "overview" && (
                <div className="space-y-6">
                  {/* Charts Grid */}
                  <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {/* Grade Distribution */}
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                      <h3 className="text-lg font-semibold text-gray-900 mb-4">Grade Distribution</h3>
                      <GradeDistributionChart
                        data={examAnalytics.gradeDistribution}
                      />
                    </div>

                    {/* Marks Distribution */}
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6 overflow-hidden">
                      <h3 className="text-lg font-semibold text-gray-900 mb-4">Marks Distribution</h3>
                      {distLoading && (
                        <div className="flex justify-center items-center h-64">
                          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                        </div>
                      )}
                      {!distLoading && showMarksDistribution && (
                        <div className="w-full overflow-x-auto">
                          <MarksDistributionChart
                            data={marksDistribution.buckets}
                          />
                        </div>
                      )}
                      {!distLoading && !showMarksDistribution && (
                        <div className="flex justify-center items-center h-64 text-gray-500">
                          <p>No marks distribution data available</p>
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Top & Bottom Performers */}
                  <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                      <h3 className="text-lg font-semibold text-gray-900 mb-4">🏆 Top 5 Performers</h3>
                      <TopPerformersTable
                        students={examAnalytics.topPerformers}
                        type="top"
                      />
                    </div>

                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                      <h3 className="text-lg font-semibold text-gray-900 mb-4">📉 Bottom 5 Performers</h3>
                      <TopPerformersTable
                        students={examAnalytics.bottomPerformers}
                        type="bottom"
                      />
                    </div>
                  </div>

                  {/* Subject Analysis */}
                  <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                    <h3 className="text-lg font-semibold text-gray-900 mb-4">Subject-wise Analysis</h3>
                    <SubjectAnalysisTable subjects={examAnalytics.subjectAnalysis} />
                  </div>
                </div>
              )}

              {selectedTab === "class" && (
                <div className="space-y-6">
                  {classId && classPerformance ? (
                    <div className="space-y-6">
                      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                        <div className="grid grid-cols-2 sm:grid-cols-5 gap-4">
                          <div>
                            <p className="text-sm text-gray-600">Class</p>
                            <p className="font-semibold text-gray-900">{classPerformance.className}</p>
                          </div>
                          <div>
                            <p className="text-sm text-gray-600">Enrolled</p>
                            <p className="font-semibold text-gray-900">{classPerformance.totalEnrolled}</p>
                          </div>
                          <div>
                            <p className="text-sm text-gray-600">Appeared</p>
                            <p className="font-semibold text-gray-900">{classPerformance.appearedCount}</p>
                          </div>
                          <div>
                            <p className="text-sm text-gray-600">Pass Rate</p>
                            <p className="font-semibold text-green-600">{classPerformance.passPercentage.toFixed(2)}%</p>
                          </div>
                          <div>
                            <p className="text-sm text-gray-600">Avg Score</p>
                            <p className="font-semibold text-blue-600">{classPerformance.classAveragePercentage.toFixed(2)}%</p>
                          </div>
                        </div>
                      </div>

                      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4">Subject-wise Performance</h3>
                        <SubjectAnalysisTable
                          subjects={classPerformance.subjectWiseAnalysis}
                        />
                      </div>
                    </div>
                  ) : (
                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 text-center">
                      <p className="text-blue-800 font-medium">Select a class to view detailed performance metrics</p>
                    </div>
                  )}
                </div>
              )}

              {selectedTab === "trend" && (
                <div className="space-y-6">
                  {classId && examComparison ? (
                    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
                      <h3 className="text-lg font-semibold text-gray-900 mb-4">Class Performance Trend - Previous Exams</h3>
                      <ExamTrendChart data={examComparison.examComparisons} />
                    </div>
                  ) : (
                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-6 text-center">
                      <p className="text-blue-800 font-medium">Select a class to view exam trends</p>
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

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
} from "../hooks/useAnalyticsHooks";
import {
  GradeDistributionChart,
  MarksDistributionChart,
  ExamTrendChart,
  PerformanceCard,
  TopPerformersTable,
  SubjectAnalysisTable,
} from "../components/analytics/AnalyticsComponents";
import "../components/analytics/analytics.css";

export const PerformanceAnalyticsPage: React.FC = () => {
  const { examId = "" } = useParams<{ examId: string }>();
  const navigate = useNavigate();
  const [classId] = useState<string>("");
  const [selectedTab, setSelectedTab] = useState<"overview" | "class" | "trend">(
    "overview"
  );

  // Queries
  const { data: examAnalytics, isLoading: analyticsLoading, error: analyticsError } = useExamAnalytics(examId);
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

  if (isLoading) {
    return (
      <div className="analytics-page">
        <div className="loading">Loading analytics...</div>
      </div>
    );
  }

  if (analyticsError) {
    return (
      <div className="analytics-page">
        <div className="error">Error loading analytics: {analyticsError.message}</div>
      </div>
    );
  }

  if (!examAnalytics) {
    return (
      <div className="analytics-page">
        <div className="error">No analytics data available</div>
      </div>
    );
  }

  return (
    <div className="analytics-page">
      <div className="page-header">
        <div>
          <h1>Performance Analytics</h1>
          <p className="subtitle">{examAnalytics.examName}</p>
        </div>
        <button className="btn btn-secondary" onClick={() => navigate("/exams")}>
          Back to Exams
        </button>
      </div>

      {/* Summary Cards */}
      <div className="analytics-summary">
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

      {/* Tabs */}
      <div className="analytics-tabs">
        <button
          className={`tab-btn ${selectedTab === "overview" ? "active" : ""}`}
          onClick={() => setSelectedTab("overview")}
        >
          📊 Overview
        </button>
        <button
          className={`tab-btn ${selectedTab === "class" ? "active" : ""}`}
          onClick={() => setSelectedTab("class")}
        >
          🏫 Class Performance
        </button>
        <button
          className={`tab-btn ${selectedTab === "trend" ? "active" : ""}`}
          onClick={() => setSelectedTab("trend")}
        >
          📈 Exam Trends
        </button>
      </div>

      {/* Tab Content */}
      {selectedTab === "overview" && (
        <div className="tab-content">
          <div className="charts-grid">
            {/* Grade Distribution */}
            <div className="chart-container">
              <h3>Grade Distribution</h3>
              <GradeDistributionChart
                data={examAnalytics.gradeDistribution}
              />
            </div>

            {/* Marks Distribution */}
            {marksDistribution && (
              <div className="chart-container">
                <h3>Marks Distribution (Histogram)</h3>
                <MarksDistributionChart
                  data={marksDistribution.buckets}
                />
              </div>
            )}
          </div>

          {/* Top & Bottom Performers */}
          <div className="performers-grid">
            <div className="performers-container">
              <h3>🏆 Top 5 Performers</h3>
              <TopPerformersTable
                students={examAnalytics.topPerformers}
                type="top"
              />
            </div>

            <div className="performers-container">
              <h3>📉 Bottom 5 Performers</h3>
              <TopPerformersTable
                students={examAnalytics.bottomPerformers}
                type="bottom"
              />
            </div>
          </div>

          {/* Subject Analysis */}
          <div className="analysis-section">
            <h3>Subject-wise Analysis</h3>
            <SubjectAnalysisTable subjects={examAnalytics.subjectAnalysis} />
          </div>
        </div>
      )}

      {selectedTab === "class" && (
        <div className="tab-content">
          {!classId ? (
            <div className="alert alert-info">
              <p>Select a class to view detailed performance metrics.</p>
            </div>
          ) : classPerformance ? (
            <div>
              <div className="class-summary">
                <p>
                  <strong>Class:</strong> {classPerformance.className}
                </p>
                <p>
                  <strong>Enrolled:</strong> {classPerformance.totalEnrolled}
                </p>
                <p>
                  <strong>Appeared:</strong> {classPerformance.appearedCount}
                </p>
                <p>
                  <strong>Pass Rate:</strong>{" "}
                  {classPerformance.passPercentage.toFixed(2)}%
                </p>
                <p>
                  <strong>Class Average:</strong>{" "}
                  {classPerformance.classAveragePercentage.toFixed(2)}%
                </p>
              </div>

              <div className="analysis-section">
                <h3>Subject-wise Performance</h3>
                <SubjectAnalysisTable
                  subjects={classPerformance.subjectWiseAnalysis}
                />
              </div>
            </div>
          ) : null}
        </div>
      )}

      {selectedTab === "trend" && (
        <div className="tab-content">
          {!classId ? (
            <div className="alert alert-info">
              <p>Select a class to view exam trends.</p>
            </div>
          ) : examComparison ? (
            <div className="trend-section">
              <h3>Class Performance Trend - Previous Exams</h3>
              <ExamTrendChart data={examComparison.examComparisons} />
            </div>
          ) : null}
        </div>
      )}
    </div>
  );
};

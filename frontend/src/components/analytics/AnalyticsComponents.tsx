/**
 * Analytics Chart and UI Components (Phase 2)
 */

import React from "react";
import type { GradeDistributionDto, MarkRangeBucketDto, StudentPerformanceDto, SubjectAnalysisDto, ExamComparisonItemDto } from "../../services/examApi";
import { formatDate } from "../../utils/dateFormat";

/**
 * GradeDistributionChart - Pie/Donut chart for grade distribution
 */
export const GradeDistributionChart: React.FC<{
  data: GradeDistributionDto[];
}> = ({ data }) => {
  const total = data.reduce((sum, item) => sum + item.count, 0);

  // For now, simplified text-based visualization
  // In production, use recharts or similar library
  return (
    <div className="chart-visualization">
      <div className="grade-list">
        {data.map((grade) => (
          <div key={grade.grade} className="grade-item">
            <div className="grade-label">
              <span className="grade-name">{grade.grade}</span>
              <span className="grade-count">{grade.count} students</span>
            </div>
            <div className="grade-bar">
              <div
                className={`bar-fill grade-${grade.grade}`}
                style={{ width: `${(grade.count / total) * 100}%` }}
              ></div>
            </div>
            <div className="grade-percentage">{grade.percentage.toFixed(1)}%</div>
          </div>
        ))}
      </div>
    </div>
  );
};

/**
 * MarksDistributionChart - Histogram visualization
 */
export const MarksDistributionChart: React.FC<{
  data: MarkRangeBucketDto[];
}> = ({ data }) => {
  const maxCount = Math.max(...data.map((d) => d.studentCount), 1);

  return (
    <div className="chart-visualization histogram">
      <div className="histogram-bars">
        {data.map((bucket) => (
          <div key={bucket.rangeLabel} className="histogram-bar-container">
            <div className="bar-wrapper">
              <div
                className="histogram-bar"
                style={{
                  height: `${(bucket.studentCount / maxCount) * 200}px`,
                }}
                title={`${bucket.studentCount} students (${bucket.percentage.toFixed(1)}%)`}
              ></div>
            </div>
            <div className="bar-label">{bucket.rangeLabel}</div>
            <div className="bar-count">{bucket.studentCount}</div>
          </div>
        ))}
      </div>
      <div className="histogram-axis">
        <span>Marks Obtained →</span>
      </div>
    </div>
  );
};

/**
 * ExamTrendChart - Line chart for exam performance trends
 */
export const ExamTrendChart: React.FC<{
  data: ExamComparisonItemDto[];
}> = ({ data }) => {
  if (data.length === 0) {
    return <div className="alert alert-info">No trend data available</div>;
  }

  const maxAverage = Math.max(...data.map((d) => d.classAverage), 100);

  return (
    <div className="chart-visualization trend-chart">
      <div className="trend-data">
        <table className="trend-table">
          <thead>
            <tr>
              <th>Exam Name</th>
              <th>Date</th>
              <th>Class Avg</th>
              <th>Pass Rate</th>
              <th>Visualization</th>
            </tr>
          </thead>
          <tbody>
            {data.map((exam) => (
              <tr key={exam.examId}>
                <td>{exam.examName}</td>
                <td>{formatDate(exam.startDate)}</td>
                <td className="average">
                  {exam.classAverage.toFixed(2)}%
                </td>
                <td className="pass-rate">
                  {exam.passPercentage.toFixed(1)}%
                  <br />
                  <span className="pass-count">
                    ({exam.passCount}/{exam.totalStudents})
                  </span>
                </td>
                <td>
                  <div className="trend-bar">
                    <div
                      className="trend-fill"
                      style={{
                        width: `${(exam.classAverage / maxAverage) * 100}%`,
                      }}
                    ></div>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

/**
 * PerformanceCard - Summary metric card
 */
export const PerformanceCard: React.FC<{
  label: string;
  value: string | number;
  color: string;
}> = ({ label, value, color }) => {
  const colorClasses: Record<string, { border: string; bg: string; textValue: string }> = {
    primary: {
      border: "border-l-blue-600",
      bg: "bg-blue-50",
      textValue: "text-blue-600"
    },
    success: {
      border: "border-l-green-600",
      bg: "bg-green-50",
      textValue: "text-green-600"
    },
    danger: {
      border: "border-l-red-600",
      bg: "bg-red-50",
      textValue: "text-red-600"
    },
    info: {
      border: "border-l-cyan-600",
      bg: "bg-cyan-50",
      textValue: "text-cyan-600"
    },
    warning: {
      border: "border-l-yellow-600",
      bg: "bg-yellow-50",
      textValue: "text-yellow-600"
    }
  };

  const { border, bg, textValue } = colorClasses[color] || colorClasses.primary;

  return (
    <div className={`${bg} border-l-4 ${border} rounded-lg p-6 shadow-sm border border-gray-200 hover:shadow-md transition-all`}>
      <p className="text-sm font-medium text-gray-600 mb-2">{label}</p>
      <p className={`text-3xl font-bold ${textValue}`}>{value}</p>
    </div>
  );
};

/**
 * TopPerformersTable - List of top/bottom performing students
 */
export const TopPerformersTable: React.FC<{
  students: StudentPerformanceDto[];
  type: "top" | "bottom";
}> = ({ students, type }) => {
  const getRankBadgeColor = (index: number): string => {
    if (index === 0) return 'bg-yellow-500';
    if (index === 1) return 'bg-gray-400';
    if (index === 2) return 'bg-orange-600';
    return 'bg-blue-500';
  };

  const getGradeColor = (grade: string): string => {
    switch (grade) {
      case 'A': return 'bg-green-600';
      case 'B': return 'bg-blue-600';
      case 'C': return 'bg-yellow-600';
      case 'D': return 'bg-orange-600';
      default: return 'bg-red-600';
    }
  };

  if (!students || students.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-600">No performer data available</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="data-table">
        <thead>
          <tr>
            <th className="center">#</th>
            <th>Student Name</th>
            <th className="center">Roll #</th>
            <th className="center">Marks</th>
            <th className="center">Grade</th>
          </tr>
        </thead>
        <tbody>
          {students.map((student, index) => (
            <tr key={student.studentId} className={type === 'top' ? 'border-l-4 border-l-green-500' : 'border-l-4 border-l-red-500'}>
              <td className="center">
                <span className={`inline-flex items-center justify-center w-8 h-8 rounded-full text-sm font-bold text-white ${getRankBadgeColor(index)}`}>
                  {index + 1}
                </span>
              </td>
              <td className="font-medium">{student.studentName}</td>
              <td className="center">{student.rollNumber}</td>
              <td className="center font-semibold">
                {student.marksObtained} <span className="text-xs font-normal">({student.percentage.toFixed(2)}%)</span>
              </td>
              <td className="center">
                <span className={`px-3 py-1 rounded-full text-xs font-bold text-white ${getGradeColor(student.grade)}`}>
                  {student.grade}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

/**
 * SubjectAnalysisTable - Subject-wise performance metrics
 */
export const SubjectAnalysisTable: React.FC<{
  subjects: SubjectAnalysisDto[];
}> = ({ subjects }) => {
  if (!subjects || subjects.length === 0) {
    return (
      <div className="text-center py-8">
        <p className="text-gray-600">No subject analysis data available</p>
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="data-table">
        <thead>
          <tr>
            <th>Subject Name</th>
            <th className="center">Max</th>
            <th className="center">Avg</th>
            <th className="center">Avg %</th>
            <th className="center">High</th>
            <th className="center">Low</th>
            <th className="center">Pass</th>
            <th className="center">Fail</th>
            <th className="center">Pass Rate</th>
          </tr>
        </thead>
        <tbody>
          {subjects.map((subject) => (
            <tr key={subject.subjectId}>
              <td className="font-medium">{subject.subjectName}</td>
              <td className="center">{subject.maxMarks}</td>
              <td className="center">{subject.averageMarks.toFixed(1)}</td>
              <td className="center">{subject.averagePercentage.toFixed(1)}%</td>
              <td className="center">
                <span className="font-semibold text-green-600">{subject.highestMarks}</span>
              </td>
              <td className="center">
                <span className="font-semibold text-red-600">{subject.lowestMarks}</span>
              </td>
              <td className="center">
                <span className="font-medium text-green-600">{subject.passCount}</span>
              </td>
              <td className="center">
                <span className="font-medium text-red-600">{subject.failCount}</span>
              </td>
              <td className="center">
                <span
                  className={`px-3 py-1 rounded-full text-sm font-semibold ${
                    subject.passPercentage >= 70
                      ? "bg-green-100 text-green-800"
                      : "bg-red-100 text-red-800"
                  }`}
                >
                  {subject.passPercentage.toFixed(1)}%
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

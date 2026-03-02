/**
 * Analytics Chart and UI Components (Phase 2)
 */

import React from "react";
import type { GradeDistributionDto, MarkRangeBucketDto, StudentPerformanceDto, SubjectAnalysisDto, ExamComparisonItemDto } from "../../services/examApi";

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
                <td>{new Date(exam.examDate).toLocaleDateString()}</td>
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
  return (
    <div className={`performance-card card-${color}`}>
      <div className="card-label">{label}</div>
      <div className="card-value">{value}</div>
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
  return (
    <div className="performers-table">
      <table className="data-table">
        <thead>
          <tr>
            <th className="rank-col">#</th>
            <th className="name-col">Student Name</th>
            <th className="roll-col">Roll #</th>
            <th className="marks-col">Marks</th>
            <th className="grade-col">Grade</th>
          </tr>
        </thead>
        <tbody>
          {students.map((student, index) => (
            <tr key={student.studentId} className={`performer-row ${type}`}>
              <td className="rank-col">
                <span className={`rank-badge rank-${index + 1}`}>
                  {index + 1}
                </span>
              </td>
              <td className="name-col">{student.studentName}</td>
              <td className="roll-col">{student.rollNumber}</td>
              <td className="marks-col">
                {student.marksObtained} ({student.percentage.toFixed(2)}%)
              </td>
              <td className="grade-col">
                <span className={`grade-badge grade-${student.grade}`}>
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
  return (
    <div className="subject-analysis-table">
      <table className="data-table">
        <thead>
          <tr>
            <th>Subject Name</th>
            <th>Max Marks</th>
            <th>Avg Marks</th>
            <th>Avg %</th>
            <th>Highest</th>
            <th>Lowest</th>
            <th>Passed</th>
            <th>Failed</th>
            <th>Pass Rate</th>
          </tr>
        </thead>
        <tbody>
          {subjects.map((subject) => (
            <tr key={subject.subjectId}>
              <td className="subject-name">{subject.subjectName}</td>
              <td className="center">{subject.maxMarks}</td>
              <td className="center">{subject.averageMarks.toFixed(2)}</td>
              <td className="center">{subject.averagePercentage.toFixed(2)}%</td>
              <td className="center highest">{subject.highestMarks}</td>
              <td className="center lowest">{subject.lowestMarks}</td>
              <td className="center pass">{subject.passCount}</td>
              <td className="center fail">{subject.failCount}</td>
              <td className="center">
                <span
                  className={`pass-badge ${
                    subject.passPercentage >= 70 ? "high" : "low"
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

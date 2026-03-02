/**
 * React Query Hooks for Analytics API Endpoints
 * Single Responsibility: Manage analytics data fetching and caching with React Query
 */

import {
  useQuery,
} from "@tanstack/react-query";
import type { UseQueryResult } from "@tanstack/react-query";
import examApi from "../services/examApi";
import type {
  ExamAnalyticsDto,
  ClassPerformanceDto,
  StudentPerformanceTrendDto,
  ClassComparativeAnalysisDto,
  MarksDistributionDto,
  ExamComparisonAnalysisDto,
  SubjectComparisonAnalysisDto,
  DetailedAnalyticsReportDto,
} from "../services/examApi";

const ANALYTICS_QUERY_KEYS = {
  all: ["analytics"] as const,
  exam: () => [...ANALYTICS_QUERY_KEYS.all, "exam"] as const,
  examAnalytics: (examId: string, classId?: string) =>
    [...ANALYTICS_QUERY_KEYS.exam(), { examId, classId }] as const,
  class: () => [...ANALYTICS_QUERY_KEYS.all, "class"] as const,
  classPerformance: (classId: string, examId: string) =>
    [...ANALYTICS_QUERY_KEYS.class(), { classId, examId }] as const,
  student: () => [...ANALYTICS_QUERY_KEYS.all, "student"] as const,
  studentTrend: (studentId: string) =>
    [...ANALYTICS_QUERY_KEYS.student(), { studentId }] as const,
  comparison: () => [...ANALYTICS_QUERY_KEYS.all, "comparison"] as const,
  classComparison: (examId: string) =>
    [...ANALYTICS_QUERY_KEYS.comparison(), { type: "class", examId }] as const,
  examComparison: (classId: string) =>
    [...ANALYTICS_QUERY_KEYS.comparison(), { type: "exam", classId }] as const,
  subjectComparison: (subjectId: string) =>
    [...ANALYTICS_QUERY_KEYS.comparison(), { type: "subject", subjectId }] as const,
  distribution: () => [...ANALYTICS_QUERY_KEYS.all, "distribution"] as const,
  marksDistribution: (examId: string, classId?: string) =>
    [...ANALYTICS_QUERY_KEYS.distribution(), { examId, classId }] as const,
  reports: () => [...ANALYTICS_QUERY_KEYS.all, "reports"] as const,
  detailedReport: (examId: string, classId?: string) =>
    [...ANALYTICS_QUERY_KEYS.reports(), { examId, classId, type: "detailed" }] as const,
};

// ============================================================================
// EXAM ANALYTICS HOOKS
// ============================================================================

/**
 * Fetch exam performance analytics (averages, pass rates, grade distribution)
 * @param examId - The exam ID
 * @param classId - Optional class ID to filter analytics
 * @returns Query result with exam analytics data
 */
export const useExamAnalytics = (
  examId: string | null,
  classId?: string
): UseQueryResult<ExamAnalyticsDto, Error> => {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.examAnalytics(examId || "", classId),
    queryFn: () => examApi.analytics.getExamAnalytics(examId!, classId),
    enabled: !!examId, // Don't fetch if no exam ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
  });
};

/**
 * Fetch class performance metrics for specific exam
 * @param classId - The class ID
 * @param examId - The exam ID
 * @returns Query result with class performance data
 */
export const useClassPerformance = (
  classId: string | null,
  examId: string | null
): UseQueryResult<ClassPerformanceDto, Error> => {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.classPerformance(
      classId || "",
      examId || ""
    ),
    queryFn: () => examApi.analytics.getClassPerformance(classId!, examId!),
    enabled: !!classId && !!examId, // Don't fetch if IDs missing
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
  });
};

// ============================================================================
// STUDENT PERFORMANCE HOOKS
// ============================================================================

/**
 * Fetch student performance trend across exams
 * @param studentId - The student ID
 * @returns Query result with performance trend data
 */
export const useStudentPerformanceTrend = (
  studentId: string | null
): UseQueryResult<StudentPerformanceTrendDto, Error> => {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.studentTrend(studentId || ""),
    queryFn: () => examApi.analytics.getStudentPerformanceTrend(studentId!),
    enabled: !!studentId, // Don't fetch if no student ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
  });
};

// ============================================================================
// COMPARATIVE ANALYSIS HOOKS
// ============================================================================

/**
 * Fetch comparative analysis across classes for same exam
 * @param examId - The exam ID
 * @returns Query result with class comparison data
 */
export const useClassComparison = (
  examId: string | null
): UseQueryResult<ClassComparativeAnalysisDto, Error> => {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.classComparison(examId || ""),
    queryFn: () => examApi.analytics.getClassComparison(examId!),
    enabled: !!examId, // Don't fetch if no exam ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
  });
};

/**
 * Fetch exam performance comparison trend in a class
 * @param classId - The class ID
 * @param limitToLastN - Optional: limit to last N exams
 * @returns Query result with exam comparison data
 */
export const useExamComparison = (
  classId: string | null,
  limitToLastN?: number
): UseQueryResult<ExamComparisonAnalysisDto, Error> => {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.examComparison(classId || ""),
    queryFn: () =>
      examApi.analytics.getExamComparison(classId!, limitToLastN),
    enabled: !!classId, // Don't fetch if no class ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
  });
};

/**
 * Fetch subject performance comparison across exams
 * @param subjectId - The subject ID
 * @param limitToLastN - Optional: limit to last N exams
 * @returns Query result with subject comparison data
 */
export const useSubjectComparison = (
  subjectId: string | null,
  limitToLastN?: number
): UseQueryResult<SubjectComparisonAnalysisDto, Error> => {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.subjectComparison(subjectId || ""),
    queryFn: () =>
      examApi.analytics.getSubjectComparison(subjectId!, limitToLastN),
    enabled: !!subjectId, // Don't fetch if no subject ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
  });
};

// ============================================================================
// DISTRIBUTION HOOKS
// ============================================================================

/**
 * Fetch marks distribution (histogram) data
 * @param examId - The exam ID
 * @param classId - Optional class ID for filtering
 * @returns Query result with marks distribution data
 */
export const useMarksDistribution = (
  examId: string | null,
  classId?: string
): UseQueryResult<MarksDistributionDto, Error> => {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.marksDistribution(examId || "", classId),
    queryFn: () => examApi.analytics.getMarksDistribution(examId!, classId),
    enabled: !!examId, // Don't fetch if no exam ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 30 * 60 * 1000, // 30 minutes
  });
};

// ============================================================================
// REPORT GENERATION HOOKS
// ============================================================================

/**
 * Fetch detailed analytics report for export
 * @param examId - The exam ID
 * @param classId - Optional class ID
 * @param includeStudents - Include student details
 * @param includeSubjects - Include subject analysis
 * @returns Query result with detailed report data
 */
export const useDetailedAnalyticsReport = (
  examId: string | null,
  classId?: string,
  includeStudents: boolean = true,
  includeSubjects: boolean = true
): UseQueryResult<DetailedAnalyticsReportDto, Error> => {
  return useQuery({
    queryKey: ANALYTICS_QUERY_KEYS.detailedReport(examId || "", classId),
    queryFn: () =>
      examApi.analytics.getDetailedAnalyticsReport(
        examId!,
        classId,
        includeStudents,
        includeSubjects
      ),
    enabled: !!examId, // Don't fetch if no exam ID
    staleTime: 15 * 60 * 1000, // 15 minutes (more expensive query)
    gcTime: 30 * 60 * 1000, // 30 minutes
  });
};

/**
 * HTTP Client for Exam Module API Endpoints
 * Single Responsibility: Handle all exam, marks, report card, and grade API calls
 */

import axios from "axios";
import type { AxiosInstance, AxiosError } from "axios";

const API_BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:5000/api";

// Create axios instance with base configuration
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

// Add token to every request
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("authToken");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Handle response errors
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("authToken");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

// ============================================================================
// EXAM API ENDPOINTS
// ============================================================================

export interface ExamSubjectInput {
  subjectId: string;
  maxMarks: number;
  passMarks: number;
}

export interface CreateExamRequest {
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  totalMarks: number;
  passMarks: number;
  subjects: ExamSubjectInput[];
  classIds: string[];
}

export interface ExamDto {
  id: string;
  name: string;
  description?: string;
  startDate: string;
  endDate: string;
  totalMarks: number;
  passMarks: number;
  status: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ExamDetailDto extends ExamDto {
  subjects: ExamSubjectDto[];
  classes: ExamClassDto[];
  marksEntryCount: number;
  publishedCount?: number;
}

export interface ExamSubjectDto {
  id: string;
  examId: string;
  subjectId: string;
  subjectName: string;
  maxMarks: number;
  minMarks: number;
}

export interface ExamClassDto {
  classId: string;
  className: string;
  studentCount: number;
  marksEntryStatus: string;
  submittedAt?: string;
}

const examApi = {
  // Create exam
  createExam: async (data: CreateExamRequest): Promise<ExamDto> => {
    const response = await apiClient.post<ExamDto>("/v1/exams", data);
    return response.data;
  },

  // Get all exams with pagination
  getExams: async (page: number = 1, pageSize: number = 10): Promise<{ data: ExamDto[]; total: number }> => {
    const response = await apiClient.get<{ data: ExamDto[]; totalCount: number }>("/v1/exams", {
      params: { page, pageSize },
    });
    return { data: response.data.data, total: response.data.totalCount };
  },

  // Get exam by ID
  getExamById: async (examId: string): Promise<ExamDetailDto> => {
    const response = await apiClient.get<ExamDetailDto>(`/v1/exams/${examId}`);
    return response.data;
  },

  // Update exam
  updateExam: async (examId: string, data: Partial<CreateExamRequest>): Promise<ExamDto> => {
    const response = await apiClient.put<ExamDto>(`/v1/exams/${examId}`, data);
    return response.data;
  },

  // Publish exam
  publishExam: async (examId: string): Promise<ExamDto> => {
    const response = await apiClient.post<ExamDto>(`/v1/exams/${examId}/publish`);
    return response.data;
  },

  // Delete exam
  deleteExam: async (examId: string): Promise<{ success: boolean; message: string }> => {
    await apiClient.delete(`/v1/exams/${examId}`);
    // DELETE returns 204 NoContent, so just return success
    return { success: true, message: "Exam deleted successfully" };
  },
};

// ============================================================================
// MARKS API ENDPOINTS
// ============================================================================

export interface SectionDto {
  id: string;
  name: string;
  sectionName: string;
}

export interface MarksEntryFormDto {
  examId: string;
  classId: string;
  examName: string;
  className: string;
  totalStudents: number;
  subjects: SubjectForMarksDto[];
  students: StudentForMarksDto[];
}

export interface SubjectForMarksDto {
  id: string;
  name: string;
  maxMarks: number;
}

export interface StudentForMarksDto {
  studentId: string;
  studentName: string;
  rollNumber: string;
  sectionId: string;
  sectionName: string;
  subjectMarks?: Record<string, SubjectMarkEntryDto>;
}

export interface SaveMarksDto {
  examId: string;
  classId: string;
  sectionId: string;
  marksData: StudentMarksEntryDto[];
}

export interface StudentMarksEntryDto {
  studentId: string;
  subjectMarks: Record<string, SubjectMarkEntryDto>;
}

export interface SubjectMarkEntryDto {
  obtained?: number;
  isAbsent: boolean;
}

export interface SaveMarksResponseDto {
  success: boolean;
  message: string;
  marksCount: number;
  validationResults: ValidationResultsDto;
}

export interface ValidationResultsDto {
  studentCount: number;
  markedCount: number;
  unmarkedCount: number;
  totalMarksObtained: number;
  averagePercentage: number;
}

export interface StudentMarksDto {
  studentId: string;
  studentName: string;
  rollNumber: string;
  subjectMarks: Record<string, SubjectMarkDto>;
  total?: number;
  percentage?: number;
  grade?: string;
}

export interface SubjectMarkDto {
  obtained?: number;
  isAbsent: boolean;
}

const marksApi = {
  // Get sections for a class
  getSectionsForClass: async (classId: string): Promise<SectionDto[]> => {
    const response = await apiClient.get<SectionDto[]>(`/v1/classes/${classId}/sections`);
    return response.data;
  },

  // Get marks entry form (populated with students, subjects, class details)
  getMarksEntryForm: async (examId: string, classId: string, sectionId: string): Promise<MarksEntryFormDto> => {
    const response = await apiClient.get<MarksEntryFormDto>(`/v1/exams/${examId}/marks/form/${classId}`, {
      params: { sectionId }
    });
    return response.data;
  },

  // Save marks (draft - not submitted)
  saveMarks: async (examId: string, classId: string, sectionId: string, marksData: StudentMarksEntryDto[]): Promise<SaveMarksResponseDto> => {
    const response = await apiClient.post<SaveMarksResponseDto>(`/v1/exams/${examId}/marks/save/${classId}?sectionId=${sectionId}`, marksData);
    return response.data;
  },

  // Get student marks for display
  getStudentMarks: async (examId: string, studentId: string): Promise<StudentMarksDto> => {
    const response = await apiClient.get<StudentMarksDto>(`/v1/exams/${examId}/marks/student/${studentId}`);
    return response.data;
  },

  // Get all student marks for a class
  getClassMarks: async (examId: string, classId: string): Promise<StudentMarksDto[]> => {
    const response = await apiClient.get<StudentMarksDto[]>(`/v1/exams/${examId}/marks/class/${classId}`);
    return response.data;
  },

  // Get all student marks for a class with pagination support
  getClassMarksWithPagination: async (
    examId: string,
    classId: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<{ data: StudentMarksDto[]; total: number }> => {
    const response = await apiClient.get<StudentMarksDto[]>(`/v1/exams/${examId}/marks/class/${classId}`);
    // Backend doesn't support pagination yet, so we handle it client-side
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = response.data.slice(startIndex, endIndex);
    return {
      data: paginatedData,
      total: response.data.length,
    };
  },

  // Submit marks (triggers report card generation)
  submitMarks: async (examId: string, classId: string, sectionId: string): Promise<SaveMarksResponseDto> => {
    const response = await apiClient.post<SaveMarksResponseDto>(`/v1/exams/${examId}/marks/submit/${classId}?sectionId=${sectionId}`);
    return response.data;
  },
};

// ============================================================================
// REPORT CARD API ENDPOINTS
// ============================================================================

export interface ReportCardDto {
  id: string;
  examId: string;
  examName: string;
  examDate: string;
  studentId: string;
  studentName: string;
  rollNumber: string;
  className: string;
  subjectMarks: SubjectReportCardDto[];
  summary: ReportCardSummaryDto;
  attendancePercentage: number;
  remarks: string;
  generatedAt: string;
}

export interface ReportCardSummaryDto {
  totalMarks: number;
  totalObtained: number;
  percentage: number;
  overallGrade: string;
  classPosition: number;
  totalStudents: number;
  status: string; // Pass/Fail
}

export interface SubjectReportCardDto {
  subjectId: string;
  subjectName: string;
  maxMarks: number;
  obtained: number;
  percentage: number;
  grade: string;
}

export interface ReportCardListDto {
  id: string;
  examId: string;
  examName: string;
  studentId: string;
  studentName: string;
  className: string;
  totalObtained: number;
  totalMarks: number;
  percentage: number;
  overallGrade: string;
  classPosition: number;
  status: string;
  generatedAt: string;
}

const reportCardApi = {
  // Get specific report card by ID
  getReportCardById: async (cardId: string): Promise<ReportCardDto> => {
    const response = await apiClient.get<ReportCardDto>(`/v1/reportcards/${cardId}`);
    return response.data;
  },

  // Get specific report card by exam and student
  getReportCard: async (examId: string, studentId: string): Promise<ReportCardDto> => {
    const response = await apiClient.get<ReportCardDto>(`/v1/reportcards/${examId}/${studentId}`);
    return response.data;
  },

  // Get all report cards for an exam (with filtering and pagination)
  getExamReportCards: async (
    examId: string,
    classId?: string,
    status?: string,
    sortBy: string = "classPosition",
    sortOrder: string = "asc",
    page: number = 1,
    pageSize: number = 20
  ): Promise<{ data: ReportCardListDto[]; total: number }> => {
    const response = await apiClient.get<ReportCardListDto[]>(`/v1/reportcards/exam/${examId}`, {
      params: { classId, status, sortBy, sortOrder },
    });
    // Backend doesn't support server-side pagination yet, so we handle it client-side
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = response.data.slice(startIndex, endIndex);
    return {
      data: paginatedData,
      total: response.data.length,
    };
  },

  // Get all report cards for a student
  getStudentReportCards: async (studentId: string): Promise<ReportCardListDto[]> => {
    const response = await apiClient.get<ReportCardListDto[]>(`/v1/reportcards/student/${studentId}`);
    return response.data;
  },

  // Export report card as PDF
  exportReportCardPdf: async (cardId: string): Promise<Blob> => {
    const response = await apiClient.post<Blob>(`/v1/reportcards/${cardId}/export-pdf`, {}, {
      responseType: "blob",
    });
    return response.data;
  },
};

// ============================================================================
// GRADES API ENDPOINTS
// ============================================================================

export interface GradeConfigurationDto {
  id: string;
  gradeName: string;
  minPercentage: number;
  maxPercentage: number;
  description?: string;
}

export interface UpdateGradeConfigurationDto {
  id: string;
  minPercentage: number;
  maxPercentage: number;
  description?: string;
}

const gradesApi = {
  // Get all grade configurations
  getGradeConfigurations: async (): Promise<GradeConfigurationDto[]> => {
    const response = await apiClient.get<GradeConfigurationDto[]>("/v1/grades");
    return response.data;
  },

  // Update grade configuration
  updateGradeConfiguration: async (data: UpdateGradeConfigurationDto[]): Promise<{ success: boolean; message: string }> => {
    const response = await apiClient.put("/v1/grades", data);
    return response.data;
  },
};

// ============================================================================
// ANALYTICS API ENDPOINTS (PHASE 2)
// ============================================================================

export interface ExamAnalyticsDto {
  examId: string;
  examName: string;
  startDate: string;
  endDate: string;
  totalStudents: number;
  passedStudents: number;
  failedStudents: number;
  passRate: number;
  classAverage: number;
  classAverageMarks: number;
  gradeDistribution: GradeDistributionDto[];
  topPerformers: StudentPerformanceDto[];
  bottomPerformers: StudentPerformanceDto[];
  subjectAnalysis: SubjectAnalysisDto[];
}

export interface GradeDistributionDto {
  grade: string;
  count: number;
  percentage: number;
}

export interface StudentPerformanceDto {
  studentId: string;
  studentName: string;
  rollNumber: string;
  marksObtained: number;
  percentage: number;
  grade: string;
  classPosition: number;
  passed: boolean;
}

export interface SubjectAnalysisDto {
  subjectId: string;
  subjectName: string;
  averageMarks: number;
  averagePercentage: number;
  highestMarks: number;
  lowestMarks: number;
  passCount: number;
  failCount: number;
  passPercentage: number;
  maxMarks: number;
}

export interface ClassPerformanceDto {
  classId: string;
  className: string;
  examId: string;
  examName: string;
  totalEnrolled: number;
  appearedCount: number;
  absentCount: number;
  passCount: number;
  failCount: number;
  passPercentage: number;
  classAverage: number;
  classAveragePercentage: number;
  highestMarks: number;
  lowestMarks: number;
  subjectWiseAnalysis: SubjectAnalysisDto[];
  studentsPassed: number;
  studentsFailed: number;
}

export interface StudentPerformanceTrendDto {
  studentId: string;
  studentName: string;
  rollNumber: string;
  classId: string;
  className: string;
  performanceHistory: ExamPerformancePointDto[];
  averagePercentage: number;
  lowestPercentage: number;
  highestPercentage: number;
  performanceTrend: string; // improving, declining, stable
}

export interface ExamPerformancePointDto {
  examId: string;
  examName: string;
  startDate: string;
  endDate: string;
  marksObtained: number;
  percentage: number;
  grade: string;
  classPosition: number;
  passed: boolean;
}

export interface ClassComparativeAnalysisDto {
  examId: string;
  examName: string;
  classComparisons: ClassComparisonItemDto[];
}

export interface ClassComparisonItemDto {
  classId: string;
  className: string;
  classAverage: number;
  passPercentage: number;
  enrolledCount: number;
  passCount: number;
}

export interface MarksDistributionDto {
  examId: string;
  buckets: MarkRangeBucketDto[];
  total: number;
}

export interface MarkRangeBucketDto {
  rangeLabel: string;
  startMark: number;
  endMark: number;
  studentCount: number;
  percentage: number;
}

export interface ExamComparisonAnalysisDto {
  classId: string;
  className: string;
  examComparisons: ExamComparisonItemDto[];
}

export interface ExamComparisonItemDto {
  examId: string;
  examName: string;
  startDate: string;
  endDate: string;
  classAverage: number;
  passPercentage: number;
  passCount: number;
  totalStudents: number;
}

export interface SubjectComparisonAnalysisDto {
  subjectId: string;
  subjectName: string;
  examPerformance: SubjectExamComparisonDto[];
}

export interface SubjectExamComparisonDto {
  examId: string;
  examName: string;
  startDate: string;
  endDate: string;
  averageMarks: number;
  averagePercentage: number;
  passCount: number;
  failCount: number;
}

export interface DetailedAnalyticsReportDto {
  reportTitle: string;
  generatedDate: string;
  examId: string;
  examName: string;
  classId?: string;
  className?: string;
  reportPeriodStart: string;
  reportPeriodEnd: string;
  totalStudents: number;
  studentsAppeared: number;
  studentsAbsent: number;
  overallPassPercentage: number;
  overallClassAverage: number;
  examAnalytics?: ExamAnalyticsDto;
  classPerformance?: ClassPerformanceDto;
  allStudentPerformance: StudentPerformanceDto[];
  allSubjectsAnalysis: SubjectAnalysisDto[];
  marksDistribution?: MarksDistributionDto;
  examTrendData: ExamComparisonItemDto[];
}

const analyticsApi = {
  // Get exam performance analytics
  getExamAnalytics: async (
    examId: string,
    classId?: string
  ): Promise<ExamAnalyticsDto> => {
    const response = await apiClient.get<ExamAnalyticsDto>(
      "/analytics/exams/" + examId,
      { params: { classId } }
    );
    return response.data;
  },

  // Get class performance metrics
  getClassPerformance: async (
    classId: string,
    examId: string
  ): Promise<ClassPerformanceDto> => {
    const response = await apiClient.get<ClassPerformanceDto>(
      `/analytics/classes/${classId}/exams/${examId}`
    );
    return response.data;
  },

  // Get student performance trend
  getStudentPerformanceTrend: async (
    studentId: string
  ): Promise<StudentPerformanceTrendDto> => {
    const response = await apiClient.get<StudentPerformanceTrendDto>(
      `/analytics/students/${studentId}/trend`
    );
    return response.data;
  },

  // Get class comparison analysis
  getClassComparison: async (
    examId: string
  ): Promise<ClassComparativeAnalysisDto> => {
    const response = await apiClient.get<ClassComparativeAnalysisDto>(
      `/analytics/exams/${examId}/class-comparison`
    );
    return response.data;
  },

  // Get exam comparison analysis
  getExamComparison: async (
    classId: string,
    limitToLastN?: number
  ): Promise<ExamComparisonAnalysisDto> => {
    const response = await apiClient.get<ExamComparisonAnalysisDto>(
      `/analytics/classes/${classId}/exam-comparison`,
      { params: { limitToLastN } }
    );
    return response.data;
  },

  // Get subject comparison analysis
  getSubjectComparison: async (
    subjectId: string,
    limitToLastN?: number
  ): Promise<SubjectComparisonAnalysisDto> => {
    const response = await apiClient.get<SubjectComparisonAnalysisDto>(
      `/analytics/subjects/${subjectId}/comparison`,
      { params: { limitToLastN } }
    );
    return response.data;
  },

  // Get marks distribution
  getMarksDistribution: async (
    examId: string,
    classId?: string
  ): Promise<MarksDistributionDto> => {
    const response = await apiClient.get<MarksDistributionDto>(
      `/analytics/exams/${examId}/marks-distribution`,
      { params: { classId } }
    );
    return response.data;
  },

  // Get detailed analytics report
  getDetailedAnalyticsReport: async (
    examId: string,
    classId?: string,
    includeStudents: boolean = true,
    includeSubjects: boolean = true
  ): Promise<DetailedAnalyticsReportDto> => {
    const response = await apiClient.get<DetailedAnalyticsReportDto>(
      "/analytics/reports/detailed",
      {
        params: {
          examId,
          classId,
          includeStudents,
          includeSubjects,
        },
      }
    );
    return response.data;
  },

  // Export analytics as JSON
  exportAnalyticsJson: async (
    examId: string,
    classId?: string
  ): Promise<Blob> => {
    const response = await apiClient.get(
      "/analytics/reports/export-json",
      {
        params: { examId, classId },
        responseType: "blob",
      }
    );
    return response.data;
  },
};

// ============================================================================
// Error Handling Utility
// ============================================================================

export const getErrorMessage = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    return error.response?.data?.message || error.message || "An error occurred";
  }
  if (error instanceof Error) {
    return error.message;
  }
  return "An unexpected error occurred";
};

// Export all API functions grouped by domain
export default {
  exam: examApi,
  marks: marksApi,
  reportCard: reportCardApi,
  grades: gradesApi,
  analytics: analyticsApi,
};

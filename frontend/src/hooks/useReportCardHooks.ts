/**
 * React Query Hooks for Report Card API Endpoints
 * Single Responsibility: Manage report card data fetching and caching with React Query
 */

import {
  useQuery,
  useMutation,
} from "@tanstack/react-query";
import type { UseQueryResult, UseMutationResult } from "@tanstack/react-query";
import examApi from "../services/examApi";
import type {
  ReportCardDto,
  ReportCardListDto,
} from "../services/examApi";
import { getErrorMessage } from "../services/examApi";

const REPORT_CARD_QUERY_KEYS = {
  all: ["reportCards"] as const,
  cards: () => [...REPORT_CARD_QUERY_KEYS.all, "cards"] as const,
  cardById: (cardId: string) =>
    [...REPORT_CARD_QUERY_KEYS.cards(), { cardId }] as const,
  card: (examId: string, studentId: string) =>
    [...REPORT_CARD_QUERY_KEYS.cards(), { examId, studentId }] as const,
  exam: () => [...REPORT_CARD_QUERY_KEYS.all, "exam"] as const,
  examCards: (
    examId: string,
    classId: string | undefined,
    status: string | undefined,
    sortBy: string,
    sortOrder: string,
    page: number,
    pageSize: number
  ) =>
    [
      ...REPORT_CARD_QUERY_KEYS.exam(),
      { examId, classId, status, sortBy, sortOrder, page, pageSize },
    ] as const,
  student: () => [...REPORT_CARD_QUERY_KEYS.all, "student"] as const,
  studentCards: (studentId: string) =>
    [...REPORT_CARD_QUERY_KEYS.student(), { studentId }] as const,
};

// ============================================================================
// QUERY HOOKS
// ============================================================================

/**
 * Fetch a specific report card by ID
 * @param cardId - The report card ID
 * @returns Query result with report card details
 */
export const useReportCardById = (
  cardId: string | null
): UseQueryResult<ReportCardDto, Error> => {
  return useQuery({
    queryKey: REPORT_CARD_QUERY_KEYS.cardById(cardId || ""),
    queryFn: () => examApi.reportCard.getReportCardById(cardId!),
    enabled: !!cardId, // Don't fetch if no ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 20 * 60 * 1000, // 20 minutes
  });
};

/**
 * Fetch a specific report card for a student and exam
 * @param examId - The exam ID
 * @param studentId - The student ID
 * @returns Query result with report card details
 */
export const useReportCard = (
  examId: string | null,
  studentId: string | null
): UseQueryResult<ReportCardDto, Error> => {
  return useQuery({
    queryKey: REPORT_CARD_QUERY_KEYS.card(examId || "", studentId || ""),
    queryFn: () => examApi.reportCard.getReportCard(examId!, studentId!),
    enabled: !!examId && !!studentId, // Don't fetch if IDs missing
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 20 * 60 * 1000, // 20 minutes
  });
};

/**
 * Fetch all report cards for an exam with filtering and pagination
 * @param examId - The exam ID
 * @param classId - Optional class ID for filtering
 * @param status - Optional status filter (pass/fail)
 * @param sortBy - Sort field (classPosition, name, percentage)
 * @param sortOrder - Sort order (asc, desc)
 * @param page - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @returns Query result with report cards list
 */
export const useExamReportCards = (
  examId: string | null,
  classId?: string,
  status?: string,
  sortBy: string = "classPosition",
  sortOrder: string = "asc",
  page: number = 1,
  pageSize: number = 20
): UseQueryResult<{ data: ReportCardListDto[]; total: number }, Error> => {
  return useQuery({
    queryKey: REPORT_CARD_QUERY_KEYS.examCards(
      examId || "",
      classId,
      status,
      sortBy,
      sortOrder,
      page,
      pageSize
    ),
    queryFn: () =>
      examApi.reportCard.getExamReportCards(
        examId!,
        classId,
        status,
        sortBy,
        sortOrder,
        page,
        pageSize
      ),
    enabled: !!examId, // Don't fetch if no exam ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 20 * 60 * 1000, // 20 minutes
  });
};

/**
 * Fetch all report cards for a student
 * @param studentId - The student ID
 * @returns Query result with report cards list
 */
export const useStudentReportCards = (
  studentId: string | null
): UseQueryResult<ReportCardListDto[], Error> => {
  return useQuery({
    queryKey: REPORT_CARD_QUERY_KEYS.studentCards(studentId || ""),
    queryFn: () => examApi.reportCard.getStudentReportCards(studentId!),
    enabled: !!studentId, // Don't fetch if no student ID
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 20 * 60 * 1000, // 20 minutes
  });
};

// ============================================================================
// MUTATION HOOKS
// ============================================================================

/**
 * Export report card as PDF
 * @returns Mutation result
 */
export const useExportReportCardPdf = (): UseMutationResult<
  Blob,
  Error,
  { cardId: string; studentName: string; examId: string },
  unknown
> => {
  return useMutation({
    mutationFn: ({ cardId }) =>
      examApi.reportCard.exportReportCardPdf(cardId),
    onSuccess: (pdfBlob, { studentName, examId }) => {
      // Create download link for the PDF
      const url = window.URL.createObjectURL(pdfBlob);
      const link = document.createElement("a");
      link.href = url;
      link.download = `report-card-${studentName}-${examId}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    },
    onError: (error) => {
      console.error("Failed to export report card:", getErrorMessage(error));
    },
  });
};

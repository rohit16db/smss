/**
 * React Query Hooks for Exam API Endpoints
 * Single Responsibility: Manage exam data fetching and caching with React Query
 */

import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import type {
  UseQueryResult,
  UseMutationResult,
} from "@tanstack/react-query";
import examApi from "../services/examApi";
import type {
  ExamDto,
  ExamDetailDto,
  CreateExamRequest,
} from "../services/examApi";
import { getErrorMessage } from "../services/examApi";

const EXAM_QUERY_KEYS = {
  all: ["exams"] as const,
  lists: () => [...EXAM_QUERY_KEYS.all, "list"] as const,
  list: (page: number, pageSize: number) =>
    [...EXAM_QUERY_KEYS.lists(), { page, pageSize }] as const,
  details: () => [...EXAM_QUERY_KEYS.all, "detail"] as const,
  detail: (id: string) => [...EXAM_QUERY_KEYS.details(), id] as const,
};

// ============================================================================
// QUERY HOOKS
// ============================================================================

/**
 * Fetch all exams with pagination
 * @param page - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @returns Query result with exams and total count
 */
export const useExams = (
  page: number = 1,
  pageSize: number = 10
): UseQueryResult<{ data: ExamDto[]; total: number }, Error> => {
  return useQuery({
    queryKey: EXAM_QUERY_KEYS.list(page, pageSize),
    queryFn: () => examApi.exam.getExams(page, pageSize),
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });
};

/**
 * Fetch a single exam with details
 * @param examId - The exam ID to fetch
 * @returns Query result with exam details
 */
export const useExam = (
  examId: string | null
): UseQueryResult<ExamDetailDto, Error> => {
  return useQuery({
    queryKey: EXAM_QUERY_KEYS.detail(examId || ""),
    queryFn: () => examApi.exam.getExamById(examId!),
    enabled: !!examId, // Don't fetch if no ID provided
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });
};

// ============================================================================
// MUTATION HOOKS
// ============================================================================

/**
 * Create a new exam
 * @returns Mutation result with created exam
 */
export const useCreateExam = (): UseMutationResult<
  ExamDto,
  Error,
  CreateExamRequest,
  unknown
> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateExamRequest) => examApi.exam.createExam(data),
    onSuccess: (newExam) => {
      // Invalidate exam list to refetch
      queryClient.invalidateQueries({
        queryKey: EXAM_QUERY_KEYS.lists(),
      });
      // Cache the new exam
      queryClient.setQueryData(
        EXAM_QUERY_KEYS.detail(newExam.id),
        newExam
      );
    },
    onError: (error) => {
      console.error("Failed to create exam:", getErrorMessage(error));
    },
  });
};

/**
 * Update an existing exam
 * @returns Mutation result with updated exam
 */
export const useUpdateExam = (): UseMutationResult<
  ExamDto,
  Error,
  { examId: string; data: Partial<CreateExamRequest> },
  unknown
> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ examId, data }) =>
      examApi.exam.updateExam(examId, data),
    onSuccess: (updatedExam) => {
      // Update cache
      queryClient.setQueryData(
        EXAM_QUERY_KEYS.detail(updatedExam.id),
        updatedExam
      );
      // Invalidate list to refetch if needed
      queryClient.invalidateQueries({
        queryKey: EXAM_QUERY_KEYS.lists(),
      });
    },
    onError: (error) => {
      console.error("Failed to update exam:", getErrorMessage(error));
    },
  });
};

/**
 * Publish an exam
 * @returns Mutation result
 */
export const usePublishExam = (): UseMutationResult<
  ExamDto,
  Error,
  string,
  unknown
> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (examId: string) => examApi.exam.publishExam(examId),
    onSuccess: (_, examId) => {
      // Invalidate exam details to refetch updated status
      queryClient.invalidateQueries({
        queryKey: EXAM_QUERY_KEYS.detail(examId),
      });
      // Invalidate list to refetch
      queryClient.invalidateQueries({
        queryKey: EXAM_QUERY_KEYS.lists(),
      });
    },
    onError: (error) => {
      console.error("Failed to publish exam:", getErrorMessage(error));
    },
  });
};

/**
 * Delete an exam
 * @returns Mutation result
 */
export const useDeleteExam = (): UseMutationResult<
  { success: boolean; message: string },
  Error,
  string,
  unknown
> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (examId: string) => examApi.exam.deleteExam(examId),
    onSuccess: (_, examId) => {
      // Remove from cache
      queryClient.removeQueries({
        queryKey: EXAM_QUERY_KEYS.detail(examId),
      });
      // Invalidate list to refetch
      queryClient.invalidateQueries({
        queryKey: EXAM_QUERY_KEYS.lists(),
      });
    },
    onError: (error) => {
      console.error("Failed to delete exam:", getErrorMessage(error));
    },
  });
};

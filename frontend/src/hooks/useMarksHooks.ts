/**
 * React Query Hooks for Marks API Endpoints
 * Single Responsibility: Manage marks data fetching and caching with React Query
 */

import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import type { UseQueryResult, UseMutationResult } from "@tanstack/react-query";
import examApi from "../services/examApi";
import type {
  MarksEntryFormDto,
  SaveMarksDto,
  SaveMarksResponseDto,
  StudentMarksDto,
  SectionDto,
} from "../services/examApi";
import { getErrorMessage } from "../services/examApi";

const MARKS_QUERY_KEYS = {
  all: ["marks"] as const,
  entryForms: () => [...MARKS_QUERY_KEYS.all, "entryForm"] as const,
  entryForm: (examId: string, classId: string, sectionId: string = "") =>
    [...MARKS_QUERY_KEYS.entryForms(), { examId, classId, sectionId }] as const,
  students: () => [...MARKS_QUERY_KEYS.all, "students"] as const,
  student: (studentId: string, examId: string) =>
    [...MARKS_QUERY_KEYS.students(), { studentId, examId }] as const,
  classes: () => [...MARKS_QUERY_KEYS.all, "classes"] as const,
  class: (classId: string, examId: string, page: number, pageSize: number) =>
    [...MARKS_QUERY_KEYS.classes(), { classId, examId, page, pageSize }] as const,
  sections: () => [...MARKS_QUERY_KEYS.all, "sections"] as const,
  classSection: (classId: string) =>
    [...MARKS_QUERY_KEYS.sections(), { classId }] as const,
};

// ============================================================================
// QUERY HOOKS
// ============================================================================

/**
 * Fetch sections for a class
 * @param classId - The class ID
 * @returns Query result with sections
 */
export const useClassSections = (
  classId: string | null
): UseQueryResult<SectionDto[], Error> => {
  return useQuery({
    queryKey: MARKS_QUERY_KEYS.classSection(classId || ""),
    queryFn: () => examApi.marks.getSectionsForClass(classId!),
    enabled: !!classId,
    staleTime: 10 * 60 * 1000, // 10 minutes
    gcTime: 15 * 60 * 1000, // 15 minutes
  });
};

/**
 * Fetch marks entry form for a class and section
 * @param examId - The exam ID
 * @param classId - The class ID
 * @param sectionId - The section ID
 * @returns Query result with marks entry form data
 */
export const useMarksEntryForm = (
  examId: string | null,
  classId: string | null,
  sectionId: string | null
): UseQueryResult<MarksEntryFormDto, Error> => {
  return useQuery({
    queryKey: MARKS_QUERY_KEYS.entryForm(examId || "", classId || "", sectionId || ""),
    queryFn: () => examApi.marks.getMarksEntryForm(examId!, classId!, sectionId!),
    enabled: !!examId && !!classId && !!sectionId, // Don't fetch if IDs missing
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });
};

/**
 * Fetch marks for a specific student and exam
 * @param studentId - The student ID
 * @param examId - The exam ID
 * @returns Query result with student marks
 */
export const useStudentMarks = (
  studentId: string | null,
  examId: string | null
): UseQueryResult<StudentMarksDto, Error> => {
  return useQuery({
    queryKey: MARKS_QUERY_KEYS.student(studentId || "", examId || ""),
    queryFn: () => examApi.marks.getStudentMarks(examId!, studentId!),
    enabled: !!studentId && !!examId, // Don't fetch if IDs missing
    staleTime: 3 * 60 * 1000, // 3 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });
};

/**
 * Fetch all marks for a class and exam with pagination
 * @param classId - The class ID
 * @param examId - The exam ID
 * @param page - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @returns Query result with class marks list
 */
export const useClassMarks = (
  classId: string | null,
  examId: string | null,
  page: number = 1,
  pageSize: number = 20
): UseQueryResult<{ data: StudentMarksDto[]; total: number }, Error> => {
  return useQuery({
    queryKey: MARKS_QUERY_KEYS.class(
      classId || "",
      examId || "",
      page,
      pageSize
    ),
    queryFn: () =>
      examApi.marks.getClassMarksWithPagination(classId!, examId!, page, pageSize),
    enabled: !!classId && !!examId, // Don't fetch if IDs missing
    staleTime: 3 * 60 * 1000, // 3 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes
  });
};

// ============================================================================
// MUTATION HOOKS
// ============================================================================

/**
 * Save marks (draft - not submitted)
 * @returns Mutation result
 */
export const useSaveMarks = (): UseMutationResult<
  SaveMarksResponseDto,
  Error,
  SaveMarksDto,
  unknown
> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SaveMarksDto) =>
      examApi.marks.saveMarks(data.examId, data.classId, data.sectionId, data.marksData),
    onSuccess: (_, data) => {
      // Invalidate class marks to refetch updated data
      queryClient.invalidateQueries({
        queryKey: MARKS_QUERY_KEYS.classes(),
      });
      // Invalidate entry form to show updated state
      queryClient.invalidateQueries({
        queryKey: MARKS_QUERY_KEYS.entryForm(data.examId, data.classId, data.sectionId),
      });
    },
    onError: (error) => {
      console.error("Failed to save marks:", getErrorMessage(error));
    },
  });
};

/**
 * Submit marks (triggers report card generation)
 * @returns Mutation result
 */
export const useSubmitMarks = (): UseMutationResult<
  SaveMarksResponseDto,
  Error,
  { examId: string; classId: string; sectionId: string },
  unknown
> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ examId, classId, sectionId }) =>
      examApi.marks.submitMarks(examId, classId, sectionId),
    onSuccess: (_, { examId, classId, sectionId }) => {
      // Invalidate all marks related queries
      queryClient.invalidateQueries({
        queryKey: MARKS_QUERY_KEYS.entryForm(examId, classId, sectionId),
      });
      queryClient.invalidateQueries({
        queryKey: MARKS_QUERY_KEYS.classes(),
      });
      // Invalidate report cards since they should be regenerated
      queryClient.invalidateQueries({
        queryKey: ["reportCards"],
      });
    },
    onError: (error) => {
      console.error("Failed to submit marks:", getErrorMessage(error));
    },
  });
};

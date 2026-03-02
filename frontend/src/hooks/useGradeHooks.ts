/**
 * React Query Hooks for Grades API Endpoints
 * Single Responsibility: Manage grade configuration fetching and caching with React Query
 */

import {
  useQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import type { UseQueryResult, UseMutationResult } from "@tanstack/react-query";
import examApi from "../services/examApi";
import type {
  GradeConfigurationDto,
  UpdateGradeConfigurationDto,
} from "../services/examApi";
import { getErrorMessage } from "../services/examApi";

const GRADES_QUERY_KEYS = {
  all: ["grades"] as const,
  configurations: () => [...GRADES_QUERY_KEYS.all, "configurations"] as const,
};

// ============================================================================
// QUERY HOOKS
// ============================================================================

/**
 * Fetch all grade configurations
 * @returns Query result with grade configurations
 */
export const useGradeConfigurations = (): UseQueryResult<
  GradeConfigurationDto[],
  Error
> => {
  return useQuery({
    queryKey: GRADES_QUERY_KEYS.configurations(),
    queryFn: () => examApi.grades.getGradeConfigurations(),
    staleTime: 60 * 60 * 1000, // 1 hour - grades rarely change
    gcTime: 24 * 60 * 60 * 1000, // 24 hours
  });
};

// ============================================================================
// MUTATION HOOKS
// ============================================================================

/**
 * Update grade configurations
 * @returns Mutation result
 */
export const useUpdateGradeConfigurations = (): UseMutationResult<
  { success: boolean; message: string },
  Error,
  UpdateGradeConfigurationDto[],
  unknown
> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: UpdateGradeConfigurationDto[]) =>
      examApi.grades.updateGradeConfiguration(data),
    onSuccess: () => {
      // Invalidate grade configurations cache to refetch
      queryClient.invalidateQueries({
        queryKey: GRADES_QUERY_KEYS.configurations(),
      });
      // Invalidate report cards since grades have changed
      queryClient.invalidateQueries({
        queryKey: ["reportCards"],
      });
      // Invalidate marks since grades are calculated from marks
      queryClient.invalidateQueries({
        queryKey: ["marks"],
      });
    },
    onError: (error) => {
      console.error(
        "Failed to update grade configurations:",
        getErrorMessage(error)
      );
    },
  });
};

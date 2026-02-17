import { useQuery } from '@tanstack/react-query';
import { feeApi, type StudentFee } from '../api';

// React Query hook for getting student fees by section
export const useStudentFeesBySection = (sectionId: string, isActive?: boolean) => {
  return useQuery<StudentFee[]>({
    queryKey: ['studentFees', sectionId, isActive],
    queryFn: async () => {
      const response = await feeApi.getStudentFeesBySection(sectionId, isActive);
      return response;
    },
    enabled: !!sectionId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

// React Query hook for getting a single student fee
export const useStudentFeeById = (id: string) => {
  return useQuery<StudentFee | null>({
    queryKey: ['studentFee', id],
    queryFn: async () => {
      const response = await feeApi.getStudentFeeById(id);
      return response;
    },
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

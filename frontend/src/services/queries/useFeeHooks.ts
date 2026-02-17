import { useQuery } from '@tanstack/react-query';
import { api } from '../api';
import type { StudentFee } from '../api';

// React Query hook for getting student fees by section
export const useStudentFeesBySection = (sectionId: string, isActive?: boolean) => {
  return useQuery<StudentFee[]>({
    queryKey: ['studentFees', sectionId, isActive],
    queryFn: () => api.fees.getStudentFeesBySection(sectionId, isActive),
    enabled: !!sectionId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

// React Query hook for getting a single student fee
export const useStudentFeeById = (id: string) => {
  return useQuery<StudentFee>({
    queryKey: ['studentFee', id],
    queryFn: () => api.fees.getStudentFeeById(id),
    enabled: !!id,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};

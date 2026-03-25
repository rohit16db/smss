import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { settingsApi, type AcademicYearDto } from '../services/api';

export function useAcademicYear() {
  const queryClient = useQueryClient();

  const { data: academicYears, isLoading, error, refetch } = useQuery({
    queryKey: ['academic-years'],
    queryFn: () => settingsApi.getAcademicYears(),
  });

  const { data: activeYear } = useQuery({
    queryKey: ['academic-year-active'],
    queryFn: () => settingsApi.getActiveAcademicYear(),
  });

  const createYear = useMutation({
    mutationFn: (data: Omit<AcademicYearDto, 'id'>) => settingsApi.createAcademicYear(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['academic-years'] });
    },
  });

  const toggleStatus = useMutation({
    mutationFn: (id: string) => settingsApi.toggleAcademicYearStatus(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['academic-years'] });
      queryClient.invalidateQueries({ queryKey: ['academic-year-active'] });
    },
  });

  return {
    academicYears,
    activeYear,
    isLoading,
    error,
    refetch,
    createYear,
    toggleStatus,
  };
}

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { settingsApi, type SchoolDto } from '../services/api';

export function useSchool() {
  const queryClient = useQueryClient();

  const { data: school, isLoading, error, refetch } = useQuery({
    queryKey: ['school-settings'],
    queryFn: () => settingsApi.getSchoolSettings(),
    staleTime: 1000 * 60 * 5, // 5 minutes
  });

  const updateSchool = useMutation({
    mutationFn: (data: Partial<SchoolDto>) => settingsApi.updateSchoolSettings(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['school-settings'] });
    },
  });

  const uploadLogo = useMutation({
    mutationFn: (file: File) => settingsApi.uploadLogo(file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['school-settings'] });
    },
  });

  return {
    school,
    isLoading,
    error,
    refetch,
    updateSchool,
    uploadLogo,
  };
}

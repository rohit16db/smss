import { useQuery } from '@tanstack/react-query';
import { healthApi } from '../api/healthApi';
import type { HealthStatus } from '../api/healthApi';

// React Query hook for health check
export const useHealth = () => {
  return useQuery<HealthStatus>({
    queryKey: ['health'],
    queryFn: healthApi.getHealth,
    refetchInterval: 30000, // Refetch every 30 seconds
  });
};

import { apiClient } from './apiClient';

export interface HealthStatus {
  status: string;
  timestamp: string;
  service: string;
  version: string;
}

// Health check API functions
export const healthApi = {
  // Get basic health status
  getHealth: async (): Promise<HealthStatus> => {
    const response = await apiClient.get<HealthStatus>('/health');
    return response.data;
  },

  // Get readiness status
  getReadiness: async (): Promise<void> => {
    await apiClient.get('/health/ready');
  },

  // Get liveness status
  getLiveness: async (): Promise<void> => {
    await apiClient.get('/health/live');
  },
};

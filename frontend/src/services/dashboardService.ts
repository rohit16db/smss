import { api } from './api';
import type { DashboardSummaryResponse } from '../types/dashboard';

export const dashboardApi = {
  /**
   * Get dashboard summary with KPIs and metrics
   */
  getSummary: async (startDate?: Date, endDate?: Date): Promise<DashboardSummaryResponse> => {
    const params = new URLSearchParams();
    if (startDate) {
      params.append('startDate', startDate.toISOString());
    }
    if (endDate) {
      params.append('endDate', endDate.toISOString());
    }

    const response = await api.get<DashboardSummaryResponse>('/dashboard/summary', {
      params,
    });
    return response.data;
  },
};

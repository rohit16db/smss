import { useQuery } from '@tanstack/react-query';
import { api } from './api';
import type {
  PayrollPeriodReportDto,
  BonusEligibilityDto,
  StaffAttendanceSummaryDto,
} from '../types/payroll';

export const payrollApi = {
  getPayrollReport: async (
    startDate: string,
    endDate: string
  ): Promise<PayrollPeriodReportDto> => {
    const response = await api.get<PayrollPeriodReportDto>(
      '/v1/payroll/report',
      {
        params: {
          startDate,
          endDate,
        },
      }
    );
    return response.data;
  },

  getBonusEligibility: async (
    startDate: string,
    endDate: string,
    bonusThresholdPercentage: number = 90
  ): Promise<BonusEligibilityDto[]> => {
    const response = await api.get<BonusEligibilityDto[]>(
      '/v1/payroll/bonus-eligibility',
      {
        params: {
          startDate,
          endDate,
          bonusThresholdPercentage,
        },
      }
    );
    return response.data;
  },

  getAttendanceSummary: async (
    startDate: string,
    endDate: string
  ): Promise<StaffAttendanceSummaryDto[]> => {
    const response = await api.get<StaffAttendanceSummaryDto[]>(
      '/v1/payroll/attendance-summary',
      {
        params: {
          startDate,
          endDate,
        },
      }
    );
    return response.data;
  },
};

// React Query Hooks
export const usePayrollReport = (startDate: string, endDate: string) => {
  return useQuery({
    queryKey: ['payrollReport', startDate, endDate],
    queryFn: () => payrollApi.getPayrollReport(startDate, endDate),
    enabled: !!(startDate && endDate),
  });
};

export const useBonusEligibility = (
  startDate: string,
  endDate: string,
  bonusThresholdPercentage?: number
) => {
  return useQuery({
    queryKey: ['bonusEligibility', startDate, endDate, bonusThresholdPercentage],
    queryFn: () =>
      payrollApi.getBonusEligibility(
        startDate,
        endDate,
        bonusThresholdPercentage
      ),
    enabled: !!(startDate && endDate),
  });
};

export const useAttendanceSummary = (startDate: string, endDate: string) => {
  return useQuery({
    queryKey: ['attendanceSummary', startDate, endDate],
    queryFn: () => payrollApi.getAttendanceSummary(startDate, endDate),
    enabled: !!(startDate && endDate),
  });
};

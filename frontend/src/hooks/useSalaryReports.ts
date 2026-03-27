import { useQuery } from '@tanstack/react-query';
import type { UseQueryResult } from '@tanstack/react-query';
import type {
  SalaryExpenseSummaryDto,
  MonthlySalaryTrendDto,
  SalaryComponentBreakdownDto,
  StaffSalaryComparisonDto,
  AttendanceToSalaryCorrelationDto,
  BudgetVsActualDto,
} from '../types/reports';
import { api } from '../services/api';

/**
 * Hook to fetch salary expense summary
 */
export function useSalaryExpenseSummary(
  startDate: Date,
  endDate: Date,
  options?: {
    prevStartDate?: Date;
    prevEndDate?: Date;
    enabled?: boolean;
  }
): UseQueryResult<SalaryExpenseSummaryDto> {
  return useQuery({
    queryKey: [
      'salaryExpenseSummary',
      startDate.toISOString(),
      endDate.toISOString(),
      options?.prevStartDate?.toISOString(),
      options?.prevEndDate?.toISOString(),
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      if (options?.prevStartDate) {
        params.append('prevStartDate', options.prevStartDate.toISOString().split('T')[0]);
      }
      if (options?.prevEndDate) {
        params.append('prevEndDate', options.prevEndDate.toISOString().split('T')[0]);
      }

      const response = await api.get(`/salary-reports/expense-summary?${params}`);
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook to fetch monthly salary trend
 */
export function useMonthlySalaryTrend(
  startDate: Date,
  endDate: Date,
  options?: {
    enabled?: boolean;
  }
): UseQueryResult<MonthlySalaryTrendDto[]> {
  return useQuery({
    queryKey: [
      'monthlySalaryTrend',
      startDate.toISOString(),
      endDate.toISOString(),
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      const response = await api.get(`/salary-reports/monthly-trend?${params}`);
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook to fetch salary component breakdown
 */
export function useSalaryComponentBreakdown(
  startDate: Date,
  endDate: Date,
  options?: {
    enabled?: boolean;
  }
): UseQueryResult<SalaryComponentBreakdownDto> {
  return useQuery({
    queryKey: [
      'salaryComponentBreakdown',
      startDate.toISOString(),
      endDate.toISOString(),
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      const response = await api.get(
        `/salary-reports/component-breakdown?${params}`
      );
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook to fetch Staff salary comparison
 */
export function useStaffSalaryComparison(
  startDate: Date,
  endDate: Date,
  options?: {
    status?: string;
    sortBy?: string;
    descending?: boolean;
    enabled?: boolean;
  }
): UseQueryResult<StaffSalaryComparisonDto[]> {
  return useQuery({
    queryKey: [
      'StaffSalaryComparison',
      startDate.toISOString(),
      endDate.toISOString(),
      options?.status,
      options?.sortBy,
      options?.descending,
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      if (options?.status) {
        params.append('status', options.status);
      }
      if (options?.sortBy) {
        params.append('sortBy', options.sortBy);
      }
      if (options?.descending !== undefined) {
        params.append('descending', options.descending.toString());
      }

      const response = await api.get(
        `/salary-reports/staff-comparison?${params}`
      );
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook to fetch attendance to salary correlation
 */
export function useAttendanceToSalaryCorrelation(
  month: Date,
  options?: {
    onlyDiscrepancies?: boolean;
    enabled?: boolean;
  }
): UseQueryResult<AttendanceToSalaryCorrelationDto[]> {
  return useQuery({
    queryKey: [
      'attendanceToSalaryCorrelation',
      month.toISOString(),
      options?.onlyDiscrepancies,
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        month: month.toISOString().split('T')[0],
      });

      if (options?.onlyDiscrepancies) {
        params.append('onlyDiscrepancies', 'true');
      }

      const response = await api.get(
        `/salary-reports/attendance-correlation?${params}`
      );
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook to fetch budget vs actual comparison
 */
export function useBudgetVsActual(
  reportType: 'FeeCollection' | 'SalaryExpense',
  startDate: Date,
  endDate: Date,
  options?: {
    groupBy?: string;
    enabled?: boolean;
  }
): UseQueryResult<BudgetVsActualDto[]> {
  return useQuery({
    queryKey: [
      'budgetVsActual',
      reportType,
      startDate.toISOString(),
      endDate.toISOString(),
      options?.groupBy,
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        reportType,
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      if (options?.groupBy) {
        params.append('groupBy', options.groupBy);
      }

      const response = await api.get(`/salary-reports/budget-vs-actual?${params}`);
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

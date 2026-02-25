import { useQuery } from '@tanstack/react-query';
import type { UseQueryResult } from '@tanstack/react-query';
import type {
  FeeCollectionSummaryDto,
  MonthlyCollectionTrendDto,
  FeeCollectionByCategoryDto,
  OutstandingFeeDto,
  StudentPaymentHistoryDto,
} from '../types/reports';
import { api } from '../services/api';

/**
 * Hook to fetch fee collection summary
 */
export function useFeeCollectionSummary(
  startDate: Date,
  endDate: Date,
  options?: {
    category?: string;
    prevStartDate?: Date;
    prevEndDate?: Date;
    enabled?: boolean;
  }
): UseQueryResult<FeeCollectionSummaryDto> {
  return useQuery({
    queryKey: [
      'feeCollectionSummary',
      startDate.toISOString(),
      endDate.toISOString(),
      options?.category,
      options?.prevStartDate?.toISOString(),
      options?.prevEndDate?.toISOString(),
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      if (options?.category) {
        params.append('category', options.category);
      }
      if (options?.prevStartDate) {
        params.append('prevStartDate', options.prevStartDate.toISOString().split('T')[0]);
      }
      if (options?.prevEndDate) {
        params.append('prevEndDate', options.prevEndDate.toISOString().split('T')[0]);
      }

      const response = await api.get(`/fee-reports/collection-summary?${params}`);
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

/**
 * Hook to fetch monthly fee collection trend
 */
export function useMonthlyFeeCollectionTrend(
  startDate: Date,
  endDate: Date,
  options?: {
    category?: string;
    enabled?: boolean;
  }
): UseQueryResult<MonthlyCollectionTrendDto[]> {
  return useQuery({
    queryKey: [
      'monthlyFeeCollectionTrend',
      startDate.toISOString(),
      endDate.toISOString(),
      options?.category,
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      if (options?.category) {
        params.append('category', options.category);
      }

      const response = await api.get(`/fee-reports/monthly-trend?${params}`);
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook to fetch fee collection by category
 */
export function useFeeCollectionByCategory(
  startDate: Date,
  endDate: Date,
  options?: {
    enabled?: boolean;
  }
): UseQueryResult<FeeCollectionByCategoryDto[]> {
  return useQuery({
    queryKey: [
      'feeCollectionByCategory',
      startDate.toISOString(),
      endDate.toISOString(),
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      const response = await api.get(`/fee-reports/by-category?${params}`);
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook to fetch outstanding fees
 */
export function useOutstandingFees(
  options?: {
    asOfDate?: Date;
    agingBucket?: string;
    minAmount?: number;
    sortBy?: string;
    descending?: boolean;
    enabled?: boolean;
  }
): UseQueryResult<OutstandingFeeDto[]> {
  return useQuery({
    queryKey: [
      'outstandingFees',
      options?.asOfDate?.toISOString(),
      options?.agingBucket,
      options?.minAmount,
      options?.sortBy,
      options?.descending,
    ],
    queryFn: async () => {
      const params = new URLSearchParams();

      if (options?.asOfDate) {
        params.append('asOfDate', options.asOfDate.toISOString().split('T')[0]);
      }
      if (options?.agingBucket) {
        params.append('agingBucket', options.agingBucket);
      }
      if (options?.minAmount !== undefined) {
        params.append('minAmount', options.minAmount.toString());
      }
      if (options?.sortBy) {
        params.append('sortBy', options.sortBy);
      }
      if (options?.descending !== undefined) {
        params.append('descending', options.descending.toString());
      }

      const response = await api.get(`/fee-reports/outstanding?${params}`);
      return response.data;
    },
    enabled: options?.enabled !== false,
    staleTime: 5 * 60 * 1000,
  });
}

/**
 * Hook to fetch student payment history
 */
export function useStudentPaymentHistory(
  studentId: string,
  startDate: Date,
  endDate: Date,
  options?: {
    enabled?: boolean;
  }
): UseQueryResult<StudentPaymentHistoryDto[]> {
  return useQuery({
    queryKey: [
      'studentPaymentHistory',
      studentId,
      startDate.toISOString(),
      endDate.toISOString(),
    ],
    queryFn: async () => {
      const params = new URLSearchParams({
        startDate: startDate.toISOString().split('T')[0],
        endDate: endDate.toISOString().split('T')[0],
      });

      const response = await api.get(
        `/fee-reports/student/${studentId}/payment-history?${params}`
      );
      return response.data;
    },
    enabled: options?.enabled !== false && !!studentId,
    staleTime: 5 * 60 * 1000,
  });
}

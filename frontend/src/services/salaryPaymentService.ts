import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from './api';
import type {
  SalaryPaymentDto,
  SalaryPaymentSummaryDto,
  UpdateSalaryPaymentStatusDto,
  MarkSalaryAsPaidDto,
  UpdateSalaryPaymentDto,
  SalaryHistoryDto
} from '../types/salaryPayment';

// API endpoints
const SALARY_PAYMENT_ENDPOINTS = {
  base: '/v1/salary-management',
  byId: (id: string) => `/v1/salary-management/${id}`,
  byStaff: (staffId: string) => `/v1/salary-management/staff/${staffId}`,
  summary: '/v1/salary-management/summary',
  updateStatus: (id: string) => `/v1/salary-management/${id}/status`,
  markPaid: (id: string) => `/v1/salary-management/${id}/pay`
};

// API functions
export const salaryPaymentApi = {
  // Get all salary payments with optional filters
  getAll: async (params?: {
    status?: string;
    staffId?: string;
    periodStartDate?: string;
    periodEndDate?: string;
  }): Promise<SalaryPaymentDto[]> => {
    const queryParams = new URLSearchParams();
    if (params?.status) queryParams.append('status', params.status);
    if (params?.staffId) queryParams.append('staffId', params.staffId);
    if (params?.periodStartDate) queryParams.append('periodStartDate', params.periodStartDate);
    if (params?.periodEndDate) queryParams.append('periodEndDate', params.periodEndDate);

    const url = queryParams.toString() 
      ? `${SALARY_PAYMENT_ENDPOINTS.base}?${queryParams.toString()}`
      : SALARY_PAYMENT_ENDPOINTS.base;

    const response = await api.get<SalaryPaymentDto[]>(url);
    return response.data;
  },

  // Get salary payment by ID
  getById: async (id: string): Promise<SalaryPaymentDto> => {
    const response = await api.get<SalaryPaymentDto>(SALARY_PAYMENT_ENDPOINTS.byId(id));
    return response.data;
  },

  // Get salary payments for a specific Staff
  getByStaff: async (staffId: string): Promise<SalaryHistoryDto> => {
    const response = await api.get<SalaryHistoryDto>(SALARY_PAYMENT_ENDPOINTS.byStaff(staffId));
    return response.data;
  },

  // Get salary payments summary
  getSummary: async (params?: {
    periodStartDate?: string;
    periodEndDate?: string;
  }): Promise<SalaryPaymentSummaryDto> => {
    const queryParams = new URLSearchParams();
    if (params?.periodStartDate) queryParams.append('periodStartDate', params.periodStartDate);
    if (params?.periodEndDate) queryParams.append('periodEndDate', params.periodEndDate);

    const url = queryParams.toString()
      ? `${SALARY_PAYMENT_ENDPOINTS.summary}?${queryParams.toString()}`
      : SALARY_PAYMENT_ENDPOINTS.summary;

    const response = await api.get<SalaryPaymentSummaryDto>(url);
    return response.data;
  },

  // Update salary payment status
  updateStatus: async (id: string, data: UpdateSalaryPaymentStatusDto): Promise<SalaryPaymentDto> => {
    const response = await api.put<SalaryPaymentDto>(SALARY_PAYMENT_ENDPOINTS.updateStatus(id), data);
    return response.data;
  },

  // Mark salary payment as paid
  markAsPaid: async (id: string, data: MarkSalaryAsPaidDto): Promise<SalaryPaymentDto> => {
    const response = await api.put<SalaryPaymentDto>(SALARY_PAYMENT_ENDPOINTS.markPaid(id), data);
    return response.data;
  },

  // Update salary payment details
  update: async (id: string, data: UpdateSalaryPaymentDto): Promise<SalaryPaymentDto> => {
    const response = await api.put<SalaryPaymentDto>(SALARY_PAYMENT_ENDPOINTS.byId(id), data);
    return response.data;
  },

  // Delete salary payment
  delete: async (id: string): Promise<void> => {
    await api.delete(SALARY_PAYMENT_ENDPOINTS.byId(id));
  }
};

// React Query hooks
export const useSalaryPayments = (params?: {
  status?: string;
  staffId?: string;
  periodStartDate?: string;
  periodEndDate?: string;
}, enabled = true) => {
  return useQuery({
    queryKey: ['salaryPayments', params],
    queryFn: () => salaryPaymentApi.getAll(params),
    enabled
  });
};

export const useSalaryPaymentById = (id: string, enabled = true) => {
  return useQuery({
    queryKey: ['salaryPayment', id],
    queryFn: () => salaryPaymentApi.getById(id),
    enabled: enabled && !!id
  });
};

export const useSalaryPaymentsByStaff = (staffId: string, enabled = true) => {
  return useQuery({
    queryKey: ['salaryPayments', 'Staff', staffId],
    queryFn: () => salaryPaymentApi.getByStaff(staffId),
    enabled: enabled && !!staffId
  });
};

export const useSalaryPaymentsSummary = (params?: {
  periodStartDate?: string;
  periodEndDate?: string;
}, enabled = true) => {
  return useQuery({
    queryKey: ['salaryPaymentsSummary', params],
    queryFn: () => salaryPaymentApi.getSummary(params),
    enabled
  });
};

// Mutation hooks
export const useUpdateSalaryPaymentStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSalaryPaymentStatusDto }) =>
      salaryPaymentApi.updateStatus(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salaryPayments'] });
      queryClient.invalidateQueries({ queryKey: ['salaryPaymentsSummary'] });
    }
  });
};

export const useMarkSalaryAsPaid = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: MarkSalaryAsPaidDto }) =>
      salaryPaymentApi.markAsPaid(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salaryPayments'] });
      queryClient.invalidateQueries({ queryKey: ['salaryPaymentsSummary'] });
    }
  });
};

export const useUpdateSalaryPayment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSalaryPaymentDto }) =>
      salaryPaymentApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salaryPayments'] });
    }
  });
};

export const useDeleteSalaryPayment = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => salaryPaymentApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salaryPayments'] });
      queryClient.invalidateQueries({ queryKey: ['salaryPaymentsSummary'] });
    }
  });
};

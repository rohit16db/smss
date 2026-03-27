import { useQuery } from '@tanstack/react-query';
import type {
  SalaryPaymentDto,
  SalaryPaymentReportDto,
  SalaryHistoryDto,
  SalarySummaryDto,
  CreateSalaryPaymentDto,
  UpdateSalaryPaymentStatusDto,
} from '../types/salary';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5208/api';

export const salaryApi = {
  getSalaryPayment: async (id: string): Promise<SalaryPaymentDto> => {
    const response = await fetch(`${API_BASE_URL}/v1/salary/${id}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch salary payment');
    }

    return response.json();
  },

  getSalaryPaymentsByPeriod: async (
    startDate: string,
    endDate: string
  ): Promise<SalaryPaymentReportDto> => {
    const params = new URLSearchParams();
    params.append('startDate', startDate);
    params.append('endDate', endDate);

    const response = await fetch(
      `${API_BASE_URL}/v1/salary/period/report?${params}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch salary payments');
    }

    return response.json();
  },

  getStaffSalaryHistory: async (
    StaffId: string,
    startDate?: string,
    endDate?: string
  ): Promise<SalaryHistoryDto> => {
    const params = new URLSearchParams();
    if (startDate) params.append('startDate', startDate);
    if (endDate) params.append('endDate', endDate);

    const response = await fetch(
      `${API_BASE_URL}/v1/salary/Staff/${StaffId}?${params}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch Staff salary history');
    }

    return response.json();
  },

  getPendingSalaries: async (asOfDate?: string): Promise<SalaryPaymentDto[]> => {
    const params = new URLSearchParams();
    if (asOfDate) params.append('asOfDate', asOfDate);

    const response = await fetch(`${API_BASE_URL}/v1/salary/pending?${params}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch pending salaries');
    }

    return response.json();
  },

  getSalarySummary: async (
    month?: number,
    year?: number
  ): Promise<SalarySummaryDto> => {
    const params = new URLSearchParams();
    if (month) params.append('month', month.toString());
    if (year) params.append('year', year.toString());

    const response = await fetch(`${API_BASE_URL}/v1/salary/summary?${params}`, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch salary summary');
    }

    return response.json();
  },

  createSalaryPayment: async (
    dto: CreateSalaryPaymentDto
  ): Promise<SalaryPaymentDto> => {
    const response = await fetch(`${API_BASE_URL}/v1/salary`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
      },
      body: JSON.stringify(dto),
    });

    if (!response.ok) {
      throw new Error('Failed to create salary payment');
    }

    return response.json();
  },

  updateSalaryStatus: async (
    id: string,
    dto: UpdateSalaryPaymentStatusDto
  ): Promise<SalaryPaymentDto> => {
    const response = await fetch(`${API_BASE_URL}/v1/salary/${id}/status`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
      },
      body: JSON.stringify(dto),
    });

    if (!response.ok) {
      throw new Error('Failed to update salary payment status');
    }

    return response.json();
  },

  markSalaryAsPaid: async (
    id: string,
    paidDate: string,
    paymentMethod?: string,
    referenceNumber?: string
  ): Promise<SalaryPaymentDto> => {
    const response = await fetch(`${API_BASE_URL}/v1/salary/${id}/mark-paid`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
      },
      body: JSON.stringify({
        paidDate,
        paymentMethod,
        referenceNumber,
      }),
    });

    if (!response.ok) {
      throw new Error('Failed to mark salary as paid');
    }

    return response.json();
  },

  deleteSalaryPayment: async (id: string): Promise<boolean> => {
    const response = await fetch(`${API_BASE_URL}/v1/salary/${id}`, {
      method: 'DELETE',
      headers: {
        Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
      },
    });

    if (!response.ok) {
      throw new Error('Failed to delete salary payment');
    }

    return response.json();
  },
};

// React Query Hooks
export const useSalaryPayment = (id: string) => {
  return useQuery({
    queryKey: ['salaryPayment', id],
    queryFn: () => salaryApi.getSalaryPayment(id),
    enabled: !!id,
  });
};

export const useSalaryPaymentsByPeriod = (startDate: string, endDate: string) => {
  return useQuery({
    queryKey: ['salaryPaymentsByPeriod', startDate, endDate],
    queryFn: () => salaryApi.getSalaryPaymentsByPeriod(startDate, endDate),
    enabled: !!(startDate && endDate),
  });
};

export const useStaffSalaryHistory = (
  StaffId: string,
  startDate?: string,
  endDate?: string
) => {
  return useQuery({
    queryKey: ['StaffSalaryHistory', StaffId, startDate, endDate],
    queryFn: () =>
      salaryApi.getStaffSalaryHistory(StaffId, startDate, endDate),
    enabled: !!StaffId,
  });
};

export const usePendingSalaries = (asOfDate?: string) => {
  return useQuery({
    queryKey: ['pendingSalaries', asOfDate],
    queryFn: () => salaryApi.getPendingSalaries(asOfDate),
  });
};

export const useSalarySummary = (month?: number, year?: number) => {
  return useQuery({
    queryKey: ['salarySummary', month, year],
    queryFn: () => salaryApi.getSalarySummary(month, year),
  });
};

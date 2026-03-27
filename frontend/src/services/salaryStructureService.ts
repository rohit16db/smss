import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type {
  SalaryStructureDto,
  CreateSalaryStructureDto,
  UpdateSalaryStructureDto,
  StaffSalaryAssignmentDto,
  AssignSalaryStructureDto,
  BulkCreateFromStructureDto,
} from '../types/salaryStructure';
import type { SalaryPaymentReportDto } from '../types/salary';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5208/api';

export const salaryStructureApi = {
  getAllSalaryStructures: async (isActive?: boolean): Promise<SalaryStructureDto[]> => {
    const params = new URLSearchParams();
    if (isActive !== undefined) params.append('isActive', isActive.toString());

    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure?${params}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch salary structures');
    }

    return response.json();
  },

  getSalaryStructureById: async (id: string): Promise<SalaryStructureDto> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/${id}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch salary structure');
    }

    return response.json();
  },

  getApplicableSalaryStructures: async (
    StaffId: string
  ): Promise<SalaryStructureDto[]> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/applicable/${StaffId}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch applicable salary structures');
    }

    return response.json();
  },

  getStaffCurrentSalaryStructure: async (
    StaffId: string
  ): Promise<StaffSalaryAssignmentDto> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/Staff/${StaffId}/current`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch Staff salary structure');
    }

    return response.json();
  },

  getStaffsWithSalaryStructures: async (
    isActive?: boolean
  ): Promise<StaffSalaryAssignmentDto[]> => {
    const params = new URLSearchParams();
    if (isActive !== undefined) params.append('isActive', isActive.toString());

    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/Staffs/assignments?${params}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch Staff assignments');
    }

    return response.json();
  },

  createSalaryStructure: async (
    data: CreateSalaryStructureDto
  ): Promise<SalaryStructureDto> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
        body: JSON.stringify(data),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to create salary structure');
    }

    return response.json();
  },

  updateSalaryStructure: async (
    id: string,
    data: UpdateSalaryStructureDto
  ): Promise<SalaryStructureDto> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/${id}`,
      {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
        body: JSON.stringify(data),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to update salary structure');
    }

    return response.json();
  },

  deleteSalaryStructure: async (id: string): Promise<void> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/${id}`,
      {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to delete salary structure');
    }
  },

  assignSalaryStructureToStaff: async (
    data: AssignSalaryStructureDto
  ): Promise<StaffSalaryAssignmentDto> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/assign-to-Staff`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
        body: JSON.stringify(data),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to assign salary structure to Staff');
    }

    return response.json();
  },

  bulkCreateSalaryPayments: async (
    data: BulkCreateFromStructureDto
  ): Promise<SalaryPaymentReportDto> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/bulk-create-salaries`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
        body: JSON.stringify(data),
      }
    );

    if (!response.ok) {
      throw new Error('Failed to bulk create salary payments');
    }

    return response.json();
  },
};

// React Query Hooks
export const useAllSalaryStructures = (isActive?: boolean) => {
  return useQuery({
    queryKey: ['salaryStructures', isActive],
    queryFn: () => salaryStructureApi.getAllSalaryStructures(isActive),
  });
};

export const useSalaryStructureById = (id: string) => {
  return useQuery({
    queryKey: ['salaryStructure', id],
    queryFn: () => salaryStructureApi.getSalaryStructureById(id),
    enabled: !!id,
  });
};

export const useApplicableSalaryStructures = (StaffId: string) => {
  return useQuery({
    queryKey: ['applicableSalaryStructures', StaffId],
    queryFn: () => salaryStructureApi.getApplicableSalaryStructures(StaffId),
    enabled: !!StaffId,
  });
};

export const useStaffCurrentSalaryStructure = (StaffId: string) => {
  return useQuery({
    queryKey: ['StaffSalaryStructure', StaffId],
    queryFn: () => salaryStructureApi.getStaffCurrentSalaryStructure(StaffId),
    enabled: !!StaffId,
  });
};

export const useStaffsWithSalaryStructures = (isActive?: boolean) => {
  return useQuery({
    queryKey: ['StaffsWithAssignments', isActive],
    queryFn: () => salaryStructureApi.getStaffsWithSalaryStructures(isActive),
  });
};

export const useCreateSalaryStructure = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: salaryStructureApi.createSalaryStructure,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salaryStructures'] });
    },
  });
};

export const useUpdateSalaryStructure = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSalaryStructureDto }) =>
      salaryStructureApi.updateSalaryStructure(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salaryStructures'] });
    },
  });
};

export const useDeleteSalaryStructure = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: salaryStructureApi.deleteSalaryStructure,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salaryStructures'] });
    },
  });
};

export const useAssignSalaryStructureToStaff = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: salaryStructureApi.assignSalaryStructureToStaff,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['StaffsWithAssignments'] });
    },
  });
};

export const useBulkCreateSalaryPayments = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: salaryStructureApi.bulkCreateSalaryPayments,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['salaryPayments'] });
    },
  });
};

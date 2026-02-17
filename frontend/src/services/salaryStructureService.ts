import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import type {
  SalaryStructureDto,
  CreateSalaryStructureDto,
  UpdateSalaryStructureDto,
  TeacherSalaryAssignmentDto,
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
    teacherId: string
  ): Promise<SalaryStructureDto[]> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/applicable/${teacherId}`,
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

  getTeacherCurrentSalaryStructure: async (
    teacherId: string
  ): Promise<TeacherSalaryAssignmentDto> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/teacher/${teacherId}/current`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch teacher salary structure');
    }

    return response.json();
  },

  getTeachersWithSalaryStructures: async (
    isActive?: boolean
  ): Promise<TeacherSalaryAssignmentDto[]> => {
    const params = new URLSearchParams();
    if (isActive !== undefined) params.append('isActive', isActive.toString());

    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/teachers/assignments?${params}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('authToken') || ''}`,
        },
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch teacher assignments');
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

  assignSalaryStructureToTeacher: async (
    data: AssignSalaryStructureDto
  ): Promise<TeacherSalaryAssignmentDto> => {
    const response = await fetch(
      `${API_BASE_URL}/v1/salarystructure/assign-to-teacher`,
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
      throw new Error('Failed to assign salary structure to teacher');
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

export const useApplicableSalaryStructures = (teacherId: string) => {
  return useQuery({
    queryKey: ['applicableSalaryStructures', teacherId],
    queryFn: () => salaryStructureApi.getApplicableSalaryStructures(teacherId),
    enabled: !!teacherId,
  });
};

export const useTeacherCurrentSalaryStructure = (teacherId: string) => {
  return useQuery({
    queryKey: ['teacherSalaryStructure', teacherId],
    queryFn: () => salaryStructureApi.getTeacherCurrentSalaryStructure(teacherId),
    enabled: !!teacherId,
  });
};

export const useTeachersWithSalaryStructures = (isActive?: boolean) => {
  return useQuery({
    queryKey: ['teachersWithAssignments', isActive],
    queryFn: () => salaryStructureApi.getTeachersWithSalaryStructures(isActive),
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

export const useAssignSalaryStructureToTeacher = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: salaryStructureApi.assignSalaryStructureToTeacher,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['teachersWithAssignments'] });
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

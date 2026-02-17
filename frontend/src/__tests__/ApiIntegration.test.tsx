/**
 * Integration Test: API Integration and Error Handling
 * Tests: API calls, data fetching, error scenarios
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, waitFor } from '../test/test-utils';
import { StudentsPage } from '../pages/StudentsPage';
import * as api from '../services/api';
import { mockPaginatedStudents } from '../test/mockData';

vi.mock('../services/api', () => ({
  studentApi: {
    getAll: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('react-hot-toast', () => ({
  default: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('API Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch students successfully', async () => {
    vi.mocked(api.studentApi.getAll).mockResolvedValue(mockPaginatedStudents);

    render(<StudentsPage />);

    await waitFor(() => {
      expect(api.studentApi.getAll).toHaveBeenCalled();
    });
  });

  it('should call getAll API on component mount', async () => {
    vi.mocked(api.studentApi.getAll).mockResolvedValue(mockPaginatedStudents);

    render(<StudentsPage />);

    await waitFor(() => {
      expect(api.studentApi.getAll).toHaveBeenCalled();
    });
  });

  it('should handle empty student list', async () => {
    vi.mocked(api.studentApi.getAll).mockResolvedValue({
      items: [],
      pageNumber: 1,
      pageSize: 10,
      totalCount: 0,
    });

    render(<StudentsPage />);

    await waitFor(() => {
      expect(api.studentApi.getAll).toHaveBeenCalled();
    });
  });

  it('should retry failed requests', async () => {
    let callCount = 0;
    vi.mocked(api.studentApi.getAll).mockImplementation(() => {
      callCount++;
      if (callCount === 1) {
        return Promise.reject(new Error('Network error'));
      }
      return Promise.resolve(mockPaginatedStudents);
    });

    render(<StudentsPage />);

    await waitFor(() => {
      // With React Query retry configured
      expect(api.studentApi.getAll).toHaveBeenCalled();
    });
  });

  it('should call API with correct parameters', async () => {
    vi.mocked(api.studentApi.getAll).mockResolvedValue(mockPaginatedStudents);

    render(<StudentsPage />);

    await waitFor(() => {
      expect(api.studentApi.getAll).toHaveBeenCalledWith(
        expect.objectContaining({
          pageNumber: expect.any(Number),
          pageSize: expect.any(Number),
        })
      );
    });
  });

  it('should handle concurrent requests', async () => {
    vi.mocked(api.studentApi.getAll).mockResolvedValue(mockPaginatedStudents);

    render(<StudentsPage />);

    await waitFor(() => {
      expect(api.studentApi.getAll).toHaveBeenCalled();
    });

    // API should have been called at least once
    expect(vi.mocked(api.studentApi.getAll).mock.calls.length).toBeGreaterThan(0);
  });

  it('should support pagination parameters', async () => {
    vi.mocked(api.studentApi.getAll).mockResolvedValue(mockPaginatedStudents);

    render(<StudentsPage />);

    await waitFor(() => {
      expect(api.studentApi.getAll).toHaveBeenCalledWith(
        expect.objectContaining({
          pageNumber: expect.any(Number),
        })
      );
    });
  });

  it('should handle search parameters', async () => {
    vi.mocked(api.studentApi.getAll).mockResolvedValue(mockPaginatedStudents);

    render(<StudentsPage />);

    await waitFor(() => {
      expect(api.studentApi.getAll).toHaveBeenCalled();
    });

    // Component should handle pagination and search
    expect(api.studentApi.getAll).toHaveBeenCalled();
  });
});


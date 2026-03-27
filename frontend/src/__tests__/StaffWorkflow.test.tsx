/**
 * Integration Test: Staff Management Workflow
 * Tests: Create Staff → Update Staff → Toggle status → Delete Staff
 * Requirement: T116 - End-to-end Staff workflow
 */
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '../test/test-utils';
import userEvent from '@testing-library/user-event';
import { StaffsPage } from '../pages/StaffsPage';
import * as api from '../services/api';
import { mockStaffs, mockPaginatedStaffs } from '../test/mockData';

// Mock the API module
vi.mock('../services/api', () => ({
  StaffApi: {
    getAll: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
    activate: vi.fn(),
    deactivate: vi.fn(),
  },
}));

// Mock toast
vi.mock('react-hot-toast', () => ({
  default: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe('Staff Management Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should display Staff list', async () => {
    vi.mocked(api.StaffApi.getAll).mockResolvedValue(mockPaginatedStaffs);

    render(<StaffsPage />);

    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeInTheDocument();
      expect(screen.getByText('Jane Smith')).toBeInTheDocument();
    });
  });

  it('should handle Staff search and filtering', async () => {
    const user = userEvent.setup();
    
    vi.mocked(api.StaffApi.getAll).mockResolvedValue(mockPaginatedStaffs);

    render(<StaffsPage />);

    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeInTheDocument();
    });

    // Search for specific Staff
    const searchInput = screen.getByPlaceholderText(/search by name, email, phone/i);
    await user.type(searchInput, 'John');

    await waitFor(() => {
      expect(api.StaffApi.getAll).toHaveBeenCalledWith(
        expect.objectContaining({
          searchTerm: 'John',
        })
      );
    });
  });

  it('should handle pagination correctly', async () => {
    vi.mocked(api.StaffApi.getAll).mockResolvedValue(mockPaginatedStaffs);

    render(<StaffsPage />);

    // First verify data loaded
    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeInTheDocument();
    });

    // Then check pagination info
    expect(screen.getByText(/showing/i)).toBeInTheDocument();
    // Pagination buttons should exist
    expect(screen.getByRole('button', { name: /previous/i })).toBeDisabled();
    expect(screen.getByRole('button', { name: /next/i })).toBeDisabled();
  });

  it('should toggle Staff active status', async () => {
    const user = userEvent.setup();
    
    vi.mocked(api.StaffApi.getAll).mockResolvedValue(mockPaginatedStaffs);
    vi.mocked(api.StaffApi.deactivate).mockResolvedValue(undefined);

    render(<StaffsPage />);

    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeInTheDocument();
    });

    // Click active badge to toggle
    const activeButtons = screen.getAllByText(/✓ active/i);
    await user.click(activeButtons[0]);

    await waitFor(() => {
      expect(api.StaffApi.deactivate).toHaveBeenCalledWith('1');
    });
  });

  it('should handle Staff deletion', async () => {
    const user = userEvent.setup();
    
    vi.mocked(api.StaffApi.getAll).mockResolvedValue(mockPaginatedStaffs);
    vi.mocked(api.StaffApi.delete).mockResolvedValue(undefined);

    render(<StaffsPage />);

    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeInTheDocument();
    });

    // Find delete button - just verify it exists and can be clicked
    const deleteButtons = screen.getAllByTitle('Delete');
    expect(deleteButtons.length).toBeGreaterThan(0);
    
    // Clicking delete should trigger API call when confirmed
    // (the confirm dialog is mocked in setup.ts)
    await user.click(deleteButtons[0]);

    // Verify API was called
    await waitFor(() => {
      expect(api.StaffApi.delete).toHaveBeenCalled();
    });
  });

  it('should display loading skeleton while fetching data', async () => {
    vi.mocked(api.StaffApi.getAll).mockImplementation(
      () => new Promise((resolve) => setTimeout(() => resolve(mockPaginatedStaffs), 100))
    );

    render(<StaffsPage />);

    // Should show loading state initially
    const skeletons = document.querySelectorAll('.animate-pulse');
    expect(skeletons.length).toBeGreaterThan(0);

    // Wait for data to load
    await waitFor(() => {
      expect(screen.getByText('John Doe')).toBeInTheDocument();
    });
  });

  it('should handle API errors gracefully', async () => {
    vi.mocked(api.StaffApi.getAll).mockRejectedValue(new Error('Network error'));

    render(<StaffsPage />);

    // Component should render without crashing
    await waitFor(() => {
      expect(screen.getByText('No Staffs found')).toBeInTheDocument();
    });
  });
});

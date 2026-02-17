# Integration Testing Documentation

## Overview
This document describes the integration testing setup for the School Management System frontend application.

## Test Infrastructure

### Technologies
- **Vitest**: Modern, fast testing framework compatible with Vite
- **React Testing Library**: Component testing with user-centric approach
- **@testing-library/jest-dom**: Custom matchers for DOM assertions
- **@testing-library/user-event**: Simulate user interactions

### Configuration
- **Test Runner**: Vitest with jsdom environment
- **Setup File**: `src/test/setup.ts` - Global test configuration
- **Test Utils**: `src/test/test-utils.tsx` - Custom render with providers
- **Mock Data**: `src/test/mockData.ts` - Reusable test data

## Test Structure

### Test Files Location
```
src/
├── __tests__/
│   ├── Components.test.tsx      # UI component tests ✅
│   ├── TeacherWorkflow.test.tsx # Teacher CRUD workflow (T116)
│   ├── FeeWorkflow.test.tsx     # Fee management workflow (T117)
│   ├── Dashboard.test.tsx       # Dashboard integration (T118)
│   └── ApiIntegration.test.tsx  # API error handling
├── test/
│   ├── setup.ts                 # Test configuration
│   ├── test-utils.tsx           # Custom render function
│   └── mockData.ts              # Mock API responses
```

## Test Coverage

### 1. Component Tests (Components.test.tsx) ✅ PASSING
**Status**: 12/12 tests passing

Tests for UI components:
- LoadingSkeleton (table, card, form variants)
- EmptyState (title, description, action button)
- NoDataIcon (SVG rendering, accessibility)

**Run**: `npm run test:run -- Components.test.tsx`

### 2. Teacher Workflow Tests (TeacherWorkflow.test.tsx)
**Requirement**: T116 - End-to-end teacher workflow

Tests cover:
- ✅ Create teacher → View → Update → Toggle status
- ✅ Teacher search and filtering
- ✅ Pagination controls
- ✅ Toggle active/inactive status
- ✅ Delete teacher
- ✅ Loading skeleton display
- ✅ API error handling

**Key Scenarios**:
```typescript
// Full workflow test
create new teacher
→ verify appears in list
→ edit teacher details
→ toggle active status
→ delete teacher
→ verify removed from list
```

### 3. Fee Workflow Tests (FeeWorkflow.test.tsx)
**Requirement**: T117 - End-to-end fee workflow

Tests cover:
- ✅ Create fee structure
- ✅ Assign fee to student
- ✅ Record payment
- ✅ Verify payment status (Paid/Partial/Pending)
- ✅ Calculate balance correctly
- ✅ Filter by status
- ✅ Handle multiple payment methods

**Key Scenarios**:
```typescript
// Full fee workflow
create fee structure (e.g., "Annual Tuition - $5000")
→ assign to student
→ record partial payment ($2500)
→ verify status = "Partial"
→ verify balance = $2500
→ record remaining payment
→ verify status = "Paid"
```

### 4. Dashboard Integration Tests (Dashboard.test.tsx)
**Requirement**: T118 - Dashboard aggregation

Tests cover:
- ✅ Display correct statistics from all modules
- ✅ Show loading state before data fetch
- ✅ Handle API errors gracefully
- ✅ Display navigation links to all modules
- ✅ Update statistics when data changes
- ✅ Render hero section
- ✅ Display module cards in correct order

**Key Validations**:
- Student count matches API response
- Teacher count matches API response
- Fee structure count matches API response
- All 4 module cards rendered
- Statistics update on data change

### 5. API Integration Tests (ApiIntegration.test.tsx)

Tests cover:
- ✅ Handle 404 errors
- ✅ Handle 500 server errors
- ✅ Handle network timeout
- ✅ Retry failed requests (React Query)
- ✅ Validation errors on create
- ✅ Concurrent requests handling
- ✅ Cache invalidation on mutation
- ✅ Pagination parameters

## Running Tests

### Commands
```bash
# Run all tests once
npm run test:run

# Run tests in watch mode
npm test

# Run specific test file
npm run test:run -- Components.test.tsx

# Run with UI (interactive)
npm run test:ui

# Run with coverage report
npm run test:coverage
```

### Test Output Example
```
✓ src/__tests__/Components.test.tsx (12 tests) 286ms
  ✓ LoadingSkeleton Component (4)
    ✓ should render table skeleton by default
    ✓ should render card skeleton when type is card
    ✓ should render form skeleton when type is form
    ✓ should render specified number of rows
  ✓ EmptyState Component (6)
  ✓ NoDataIcon Component (2)

Test Files  1 passed (1)
     Tests  12 passed (12)
  Duration  1.92s
```

## Mock Data

### Available Mocks
- `mockTeachers`: 2 teacher records
- `mockStudents`: 2 student records
- `mockFeeStructures`: 2 fee structures
- `mockStudentFees`: 1 student fee assignment
- `mockFeePayments`: 1 payment record
- `mockPaginatedTeachers`: Paginated teacher response
- `mockPaginatedStudents`: Paginated student response
- `mockPaginatedFeeStructures`: Paginated fee structure response

### Usage
```typescript
import { mockTeachers, mockPaginatedTeachers } from '../test/mockData';

vi.mocked(api.teacherApi.getAll).mockResolvedValue(mockPaginatedTeachers);
```

## Test Utilities

### Custom Render
The custom render function wraps components with necessary providers:
- QueryClientProvider (React Query)
- BrowserRouter (React Router)

```typescript
import { render, screen } from '../test/test-utils';

render(<MyComponent />);
```

### User Events
Simulate user interactions:
```typescript
import userEvent from '@testing-library/user-event';

const user = userEvent.setup();
await user.click(button);
await user.type(input, 'text');
```

## Integration Test Patterns

### 1. Full Workflow Pattern
```typescript
it('should complete full workflow', async () => {
  // 1. Setup mocks
  vi.mocked(api.method).mockResolvedValue(mockData);
  
  // 2. Render component
  render(<Component />);
  
  // 3. Wait for initial load
  await waitFor(() => {
    expect(screen.getByText('expected')).toBeInTheDocument();
  });
  
  // 4. User interaction
  const button = screen.getByRole('button');
  await user.click(button);
  
  // 5. Verify mutation called
  expect(api.method).toHaveBeenCalledWith(expected);
  
  // 6. Verify UI updated
  expect(screen.getByText('new state')).toBeInTheDocument();
});
```

### 2. Error Handling Pattern
```typescript
it('should handle errors', async () => {
  vi.mocked(api.method).mockRejectedValue(new Error('Failed'));
  
  render(<Component />);
  
  await waitFor(() => {
    expect(screen.getByText('Empty state')).toBeInTheDocument();
  });
});
```

### 3. Loading State Pattern
```typescript
it('should show loading state', async () => {
  vi.mocked(api.method).mockImplementation(
    () => new Promise((resolve) => 
      setTimeout(() => resolve(data), 100)
    )
  );
  
  render(<Component />);
  
  // Check loading
  expect(document.querySelector('.animate-pulse')).toBeInTheDocument();
  
  // Wait for data
  await waitFor(() => {
    expect(screen.getByText('data')).toBeInTheDocument();
  });
});
```

## CI/CD Integration

### GitHub Actions Example
```yaml
- name: Run tests
  run: npm run test:run

- name: Generate coverage
  run: npm run test:coverage

- name: Upload coverage
  uses: codecov/codecov-action@v3
```

## Best Practices

1. **Test User Behavior**: Test what users see and do, not implementation details
2. **Mock External Dependencies**: Always mock API calls
3. **Use Semantic Queries**: Prefer `getByRole`, `getByLabelText` over `getByTestId`
4. **Wait for Async Updates**: Use `waitFor` for async state changes
5. **Clean Up**: Tests automatically clean up with `afterEach(cleanup)`
6. **Descriptive Test Names**: Use "should..." pattern for clarity
7. **Arrange-Act-Assert**: Structure tests clearly

## Known Issues & Limitations

1. **Page Tests**: Some tests fail due to missing mocks for sub-components
   - Solution: Add comprehensive mocks for all API endpoints used in pages

2. **React Query Retries**: Default retry behavior may cause test delays
   - Solution: Configure test QueryClient with `retry: false`

3. **Dialog Tests**: Dialog components may not mount in test environment
   - Solution: Mock dialog state or use `screen.getByRole('dialog')`

## Future Enhancements

- [ ] Add E2E tests with Playwright
- [ ] Add visual regression tests
- [ ] Increase coverage to 80%+
- [ ] Add performance tests
- [ ] Add accessibility (a11y) tests with jest-axe

## Resources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/react)
- [Testing Best Practices](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)

---

**Last Updated**: January 13, 2026
**Test Status**: 12/12 Component Tests Passing ✅
**Coverage**: Component tests validated, integration tests created

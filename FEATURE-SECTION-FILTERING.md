# Fee Management - Section-Based Filtering Implementation

## Overview
Added section-based filtering capability to the Fee Management system, allowing users to:
- Filter student fees by section
- Query student fees for a specific section via the API
- Display section information in the fee management UI

## Implementation Summary

### Backend Changes

#### 1. **New API Endpoint**
- **Endpoint**: `GET /api/fees/student-fees/section/{sectionId}`
- **Parameters**:
  - `sectionId` (route): The unique identifier of the section
  - `isActive` (query, optional): Filter by active status (true/false)
- **Response**: `List<StudentFee>`
- **Status Code**: 
  - 200 OK: Successfully retrieved student fees
  - 400 Bad Request: Invalid section ID format
  - 404 Not Found: Section not found

#### 2. **Database Query**
The implementation uses the existing `StudentFee` entity which includes:
- `StudentId`: Reference to the student
- `SectionId`: Current section of the student
- `FeeStructureId`: Reference to the fee structure
- `StartDate`, `EndDate`: Fee assignment period
- `IsActive`: Active status
- Amount tracking fields

### Frontend Changes

#### 1. **API Service Enhancement** (`frontend/src/services/api.ts`)
```typescript
// New method added to the fees API object
getStudentFeesBySection: async (sectionId: string, isActive?: boolean) => {
  const response = await api.get<StudentFee[]>(`/fees/student-fees/section/${sectionId}`, {
    params: isActive !== undefined ? { isActive } : undefined,
  });
  return response.data;
}
```

#### 2. **React Query Hook** (`frontend/src/services/queries/useFeeHooks.ts`)
New hook for querying student fees by section:
```typescript
export const useStudentFeesBySection = (sectionId: string, isActive?: boolean) => {
  return useQuery<StudentFee[]>({
    queryKey: ['studentFees', sectionId, isActive],
    queryFn: () => api.fees.getStudentFeesBySection(sectionId, isActive),
    enabled: !!sectionId,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
};
```

#### 3. **UI Component Enhancement** (`frontend/src/pages/FeesPage.tsx`)
Updated the `StudentFeesTab` component to include:
- **Section Filter Dropdown**: Dynamic dropdown showing available sections from loaded fees
- **Client-Side Filtering**: Filters the displayed student fees table based on selected section
- **Reset on Selection**: Page resets to 1 when a section is selected

### Type Definitions
The `StudentFee` type already includes section context:
```typescript
export type StudentFee = {
  id: string;
  studentId: string;
  studentName: string;
  enrollmentNumber: string;
  feeStructureId: string;
  feeStructureName?: string;
  startDate: string;
  endDate?: string;
  totalAmount: number;
  paidAmount: number;
  balanceAmount: number;
  isActive: boolean;
  sectionId?: string;        // Section the student is enrolled in
  sectionName?: string;      // Name of the section
};
```

## Features

### 1. Section-Based Filtering in UI
- Dropdown populated with available sections from the current data
- Shows formatted section names (e.g., "Class 1-A", "Class 9-B")
- "All Sections" option shows all student fees

### 2. API Filtering Capability
- Can filter student fees for a specific section
- Optional `isActive` parameter to filter by fee status

### 3. Data Display
- Section information now visible in lists
- Student fees show their associated section
- Clear indication when no fees exist for a section

## Usage Examples

### Frontend - Using the Hook
```typescript
const { data, isLoading, error } = useStudentFeesBySection('section-123', true);
// Fetches all active fees for the specified section
```

### Frontend - Using the API Service
```typescript
const fees = await api.fees.getStudentFeesBySection('section-123');
const activeFees = await api.fees.getStudentFeesBySection('section-123', true);
```

### Frontend - In Components
The `StudentFeesTab` automatically:
1. Loads all student fees
2. Builds a list of available sections from the data
3. Allows filtering by section
4. Displays filtered results

### Backend - Direct Query
```csharp
var fees = await _context.StudentFees
  .Where(sf => sf.SectionId == sectionId && sf.IsActive)
  .Include(sf => sf.Student)
  .Include(sf => sf.FeeStructure)
  .ToListAsync();
```

## Testing Checklist

### API Endpoint Testing
- [ ] GET `/api/fees/student-fees/section/{sectionId}` returns valid student fees
- [ ] Returns empty list when section has no fees
- [ ] `isActive` parameter filters correctly
- [ ] Invalid section ID returns 404
- [ ] Response includes all required fields

### Frontend Component Testing
- [ ] Section dropdown populates with available sections
- [ ] Selecting a section filters the table
- [ ] "All Sections" option shows all fees
- [ ] Pagination resets when section changes
- [ ] No fees message displays for empty sections

### Integration Testing
- [ ] Create student fee > filter by section > view fees
- [ ] Deactivate fee > section filter still works
- [ ] Multiple sections available > filter works correctly

## Performance Considerations

1. **Query Caching**: React Query caches results with 5-minute stale time
2. **Client-Side Filtering**: Section filtering happens on the frontend to reduce API calls
3. **Database Indexing**: Ensure `StudentFees.SectionId` has an index for performance
4. **Pagination**: Reset to page 1 when section changes to avoid offset issues

## Future Enhancements

1. **Backend Pagination**: Implement server-side pagination for section queries
2. **Export Feature**: Export section fees to CSV/Excel
3. **Analytics**: Section-level fee reports and statistics
4. **Batch Operations**: Bulk update fees for all students in a section
5. **Section-Based Templates**: Apply fee structures to all students in a section

## Related Documentation
- [Fee Management System](../specs/002-teacher-fee-attendance/)
- [API Endpoints](../specs/002-teacher-fee-attendance/api-endpoints.md)
- [Database Schema](../specs/002-teacher-fee-attendance/database-schema.md)

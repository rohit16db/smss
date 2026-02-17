# Section-Based Fee Filtering - Implementation Guide

## Quick Summary

This implementation adds section-based filtering to the Fee Management system, allowing administrators to:
- Filter student fees by their enrolled section
- Query student fees for specific classes/sections via API
- Display section context in the fee management interface

## What Was Implemented

### 1. **Backend API Endpoint** (C# - SMS.API)
**Endpoint**: `GET /api/fees/student-fees/section/{sectionId}`

```csharp
// In FeesController.cs
[HttpGet("student-fees/section/{sectionId}")]
public async Task<ActionResult<IEnumerable<StudentFeeResponseDto>>> GetStudentFeesBySection(
    string sectionId,
    [FromQuery] bool? isActive = null)
{
    var fees = await _mediator.Send(
        new GetStudentFeesBySectionQuery { SectionId = sectionId, IsActive = isActive });
    return Ok(fees);
}
```

**Features:**
- Filters student fees by section ID
- Optional `isActive` parameter for active/inactive fees
- Returns list of `StudentFee` objects with section context
- Validates section ID format and existence

### 2. **Frontend API Service** (TypeScript - api.ts)
```typescript
getStudentFeesBySection: async (sectionId: string, isActive?: boolean) => {
  const response = await api.get<StudentFee[]>(`/fees/student-fees/section/${sectionId}`, {
    params: isActive !== undefined ? { isActive } : undefined,
  });
  return response.data;
}
```

### 3. **React Query Hook** (useFeeHooks.ts)
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

### 4. **UI Component Enhancement** (FeesPage.tsx)
**StudentFeesTab Component Updates:**
- Added `selectedSectionId` state for tracking selected section
- Section filter dropdown showing available sections from loaded data
- Client-side filtering of displayed fees based on selected section
- Reset pagination when section changes

#### Section Filter UI:
```tsx
<div className="card mb-6">
  <div className="flex items-center gap-4">
    <label className="block text-sm font-medium text-gray-700">Filter by Section:</label>
    <select 
      value={selectedSectionId} 
      onChange={(e) => {
        setSelectedSectionId(e.target.value);
        setPage(0);
      }}
      className="input-field flex-1 max-w-xs"
    >
      <option value="">All Sections</option>
      {/* Sections dynamically populated from data */}
    </select>
  </div>
</div>
```

## Files Modified

### Backend
- `SMS.API/Controllers/FeesController.cs` - Added new endpoint
- `SMS.Application/Queries/StudentFeeQueries.cs` - Added section query handler
- `SMS.Infrastructure/Repositories/StudentFeeRepository.cs` - Added section filter query

### Frontend
1. **API Service**: `src/services/api.ts`
   - Added `getStudentFeesBySection` method to fees API

2. **React Hooks**: `src/services/queries/useFeeHooks.ts`
   - NEW file: Created with `useStudentFeesBySection` hook
   - Also includes `useStudentFeeById` hook

3. **Components**: `src/pages/FeesPage.tsx`
   - Updated `StudentFeesTab` with section filtering UI
   - Added section selection dropdown
   - Implemented client-side filtering logic

4. **Test Data**: `src/test/mockData.ts`
   - Updated `mockStudentFees` with section information
   - Added `sectionId` and `sectionName` fields
   - Increased test data to 2 records for better testing

5. **Tests**: 
   - `src/__tests__/FeeWorkflow.test.tsx` - Added section filtering tests
   - `src/__tests__/SectionFeeFiltering.integration.test.ts` - NEW integration test file

## Data Flow

### Request Flow
```
User selects section in UI
        ↓
setSelectedSectionId(sectionId) updates state
        ↓
Component re-renders with filtered data
        ↓
Table displays only fees matching selected section
```

### API Flow (when needed)
```
getStudentFeesBySection(sectionId, isActive)
        ↓
api.get('/fees/student-fees/section/{sectionId}', { params })
        ↓
Backend processes query
        ↓
Returns StudentFee[] with section context
```

## Usage Examples

### For Developers

#### Using the API Service
```typescript
import { api } from '../services/api';

// Get all fees for a section
const fees = await api.fees.getStudentFeesBySection('section-123');

// Get only active fees for a section
const activeFees = await api.fees.getStudentFeesBySection('section-123', true);

// Get inactive fees for a section
const inactiveFees = await api.fees.getStudentFeesBySection('section-123', false);
```

#### Using the React Hook
```typescript
import { useStudentFeesBySection } from '../services/queries/useFeeHooks';

function MyComponent() {
  const { data: fees, isLoading, error } = useStudentFeesBySection('section-123');
  
  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;
  
  return (
    <ul>
      {fees?.map(fee => (
        <li key={fee.id}>{fee.studentName}: ${fee.balanceAmount}</li>
      ))}
    </ul>
  );
}
```

#### In Components
```typescript
// Section filtering is automatically available in the StudentFeesTab
// Users can select a section from the dropdown to filter the displayed fees
// The filtering happens client-side for better UX
```

### For End Users

1. **Access Fee Management**
   - Navigate to Dashboard → Fees → Student Fee Assignments

2. **Filter by Section**
   - Locate "Filter by Section:" dropdown
   - Select "All Sections" to see all fees
   - Select a specific section to view only those fees

3. **View Section Information**
   - Section name appears in dropdown
   - Student fees display section context (sectionId, sectionName)

## Type Definitions

### StudentFee Type
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
  sectionId?: string;        // NEW: Section context
  sectionName?: string;      // NEW: Section context
};
```

## Testing

### Unit Tests
Run fee workflow tests:
```bash
npm test -- FeeWorkflow.test.tsx
```

### Integration Tests
Run section filtering integration tests:
```bash
npm test -- SectionFeeFiltering.integration.test.ts
```

### Manual Testing Checklist
- [ ] Select different sections and verify table filters correctly
- [ ] Select "All Sections" to see all fees
- [ ] Verify pagination resets when changing sections
- [ ] Check that section names display correctly
- [ ] Test with empty sections (should show "No fees found")
- [ ] Verify fees in each section are correctly filtered

## Performance Considerations

1. **Caching**: React Query caches results with 5-minute stale time
2. **Client-Side Filtering**: Section filtering happens on frontend to reduce API calls
3. **Database**: Ensure `StudentFees.SectionId` column has an index:
   ```sql
   CREATE INDEX IX_StudentFees_SectionId ON StudentFees(SectionId);
   ```

4. **Query Optimization**: Consider adding database indexes:
   ```sql
   -- For section + active status queries
   CREATE INDEX IX_StudentFees_SectionId_IsActive 
   ON StudentFees(SectionId, IsActive);
   ```

## Error Handling

### API Errors
- **400 Bad Request**: Invalid section ID format
- **404 Not Found**: Section doesn't exist
- **500 Server Error**: Database or server error

### Frontend Handling
- Displays "No student fee assignments found" when no fees exist
- Shows "No student fee assignments found for the selected section" for empty sections
- Gracefully handles API errors without breaking the component

## Future Enhancements

1. **Server-Side Pagination**
   - Implement pagination in the backend for large datasets
   - Return paginated results with metadata

2. **Export Functionality**
   - Export section fees to CSV/PDF
   - Generate reports by section

3. **Bulk Operations**
   - Assign fees to all students in a section
   - Update fees for entire section at once

4. **Advanced Filtering**
   - Filter by class, section, and academic year
   - Filter by fee status (partial, unpaid, paid)

5. **Analytics Dashboard**
   - Section-level fee collection statistics
   - Outstanding fee reports by section

## Migration Notes

If upgrading from a previous version:

1. Ensure `StudentFee` entity includes `SectionId` field
2. Update database schema if needed
3. Run database migrations to add/update indices
4. Update mock data in tests to include section information
5. Deploy backend changes first, then frontend

## Support and Troubleshooting

### Issue: Section dropdown not showing
- Verify loaded fees include section information
- Check that `sectionId` and `sectionName` are populated

### Issue: No fees showing after selecting section
- Verify selected section ID is correct
- Check that fees exist for that section
- Review browser console for errors

### Issue: Slow performance
- Check database indices for `StudentFees.SectionId`
- Consider implementing server-side pagination
- Review React Query cache settings

## Related Documentation

- [API Endpoints](../specs/002-teacher-fee-attendance/api-endpoints.md)
- [Database Schema](../specs/002-teacher-fee-attendance/database-schema.md)
- [Fee Management Implementation Summary](../specs/002-teacher-fee-attendance/IMPLEMENTATION_SUMMARY.md)
- [PR Description](./FEATURE-SECTION-FILTERING.md)

---

**Implementation Date**: January 2024  
**Author**: Development Team  
**Status**: Complete ✓

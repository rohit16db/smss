# UI Component Design Specifications
**Feature**: 002-teacher-fee-attendance  
**Created**: January 12, 2026  
**Phase**: Planning / Design  
**Framework**: React 18 + Vite  
**Component Library**: Material-UI (MUI) v5  
**State Management**: React Query (TanStack Query)

---

## Overview

This document specifies all React components required for Teacher, Fee, and Attendance management UI. Component specifications include:
- Component purpose and responsibility
- Props interface (TypeScript)
- Internal state management
- Child components and composition
- Event handlers
- Validation and error handling
- Accessibility requirements
- API integration points

---

## Layout Components

### 1. DashboardLayout

**Purpose**: Main layout wrapper for dashboard pages with sidebar navigation  
**Path**: `src/layouts/DashboardLayout.tsx`

**Props**:
```typescript
interface DashboardLayoutProps {
  children: ReactNode;
  sidebarOpen?: boolean;
  onSidebarToggle?: (open: boolean) => void;
  title?: string;
  breadcrumbs?: BreadcrumbItem[];
}

interface BreadcrumbItem {
  label: string;
  path: string;
}
```

**Child Components**:
- `Sidebar`: Navigation menu
- `TopBar`: Header with user menu
- `Breadcrumb`: Navigation path display
- `MainContent`: Children wrapper with responsive grid

**Features**:
- Responsive sidebar (collapses on mobile)
- User profile menu with logout
- Breadcrumb navigation
- Dark/light mode toggle support
- Mobile-friendly hamburger menu

**Example Usage**:
```tsx
<DashboardLayout title="Teacher Management" breadcrumbs={[
  { label: 'Home', path: '/dashboard' },
  { label: 'Teachers', path: '/teachers' }
]}>
  <TeacherListPage />
</DashboardLayout>
```

---

## Teacher Management Components

### 2. TeacherListPage

**Purpose**: Display paginated list of teachers with search, filter, and actions  
**Path**: `src/pages/teachers/TeacherListPage.tsx`

**Props**:
```typescript
interface TeacherListPageProps {
  onTeacherSelect?: (teacherId: string) => void;
}
```

**State Management**:
```typescript
const [pageNumber, setPageNumber] = useState(1);
const [pageSize, setPageSize] = useState(20);
const [searchTerm, setSearchTerm] = useState('');
const [isActive, setIsActive] = useState<boolean | undefined>(true);
const [sortBy, setSortBy] = useState('name');
const [sortOrder, setSortOrder] = useState('asc');
```

**API Integration**:
- `useQuery` hook → `GET /api/v1/teachers?pageNumber=...&pageSize=...`
- Refetch on filter/search/sort change

**Child Components**:
- `TeacherSearchBar`: Search input + filters
- `TeacherTable`: Data table with columns
- `TeacherTableActions`: Row action buttons (Edit, View, Delete, Assign)
- `Pagination`: Page navigation controls
- `CreateTeacherButton`: FAB for creating new teacher

**Features**:
- Debounced search (300ms)
- Column sorting
- Filter by active/inactive status
- Pagination with page size selector
- Bulk actions (select multiple teachers)
- Loading skeleton during data fetch
- Empty state message

---

### 3. TeacherForm

**Purpose**: Reusable form for creating and editing teachers  
**Path**: `src/components/forms/TeacherForm.tsx`

**Props**:
```typescript
interface TeacherFormProps {
  initialData?: TeacherFormData;
  isLoading?: boolean;
  onSubmit: (data: TeacherFormData) => Promise<void>;
  onCancel?: () => void;
  mode: 'create' | 'edit';
}

interface TeacherFormData {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  qualification?: string;
  experienceYears?: number;
  joiningDate: string;
  isActive?: boolean;
}
```

**State Management**:
```typescript
const form = useForm<TeacherFormData>({
  defaultValues: initialData,
  resolver: zodResolver(teacherFormSchema)
});
```

**Form Fields**:
- firstName (required, text input, max 50)
- lastName (required, text input, max 50)
- email (required, email input, unique validation)
- phone (optional, tel input)
- qualification (optional, textarea, max 500)
- experienceYears (optional, number input, min 0)
- joiningDate (required, date picker, not future)
- isActive (optional, toggle switch)

**Validation**:
- Client-side: React Hook Form + Zod schema
- Server-side: POST/PUT response validation
- Show field-level error messages

**Features**:
- Auto-save draft to localStorage
- Unsaved changes warning on navigation
- Image avatar upload (optional)
- Clear/Reset form button
- Submit button with loading state

**API Integration**:
- Create: `POST /api/v1/teachers`
- Edit: `PUT /api/v1/teachers/{id}`
- Success toast notification
- Error handling with retry option

---

### 4. TeacherDetailCard

**Purpose**: Display detailed teacher information with action buttons  
**Path**: `src/components/cards/TeacherDetailCard.tsx`

**Props**:
```typescript
interface TeacherDetailCardProps {
  teacherId: string;
  isLoading?: boolean;
  onEdit?: () => void;
  onDelete?: () => void;
}
```

**Displayed Data**:
- Profile picture, name, email, phone
- Qualification, experience years, joining date
- Active status badge
- Current assignments (with class names, subjects)
- Recent attendance summary (% last 30 days)
- Action buttons: Edit, Delete, Assign to Class, View Attendance

**API Integration**:
- `useQuery` hook → `GET /api/v1/teachers/{id}`

**Features**:
- Responsive card layout
- Copy email/phone to clipboard
- Expand/collapse additional info
- Link to teacher attendance report

---

### 5. TeacherAssignmentModal

**Purpose**: Modal for assigning teacher to class/subject combination  
**Path**: `src/components/modals/TeacherAssignmentModal.tsx`

**Props**:
```typescript
interface TeacherAssignmentModalProps {
  teacherId: string;
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}
```

**Form Fields**:
- classId (required, dropdown - fetch from API)
- subjectId (required, dropdown - fetch from API, filtered by class)
- assignmentDate (required, date picker)

**Features**:
- Prevent duplicate assignments (check on submit)
- Show teacher's existing assignments
- Warn if teacher has overlapping assignments
- Cascade dropdown: class selection updates subject list

**API Integration**:
- Create: `POST /api/v1/teachers/{id}/assignments`
- Fetch classes: `GET /api/v1/classes` (from existing system)
- Fetch subjects: `GET /api/v1/classes/{classId}/subjects`
- Success: Show confirmation modal with assignment details
- Error: Display error toast with retry option

---

### 6. TeacherAssignmentList

**Purpose**: Display table of teacher assignments with removal action  
**Path**: `src/components/tables/TeacherAssignmentList.tsx`

**Props**:
```typescript
interface TeacherAssignmentListProps {
  teacherId: string;
  isLoading?: boolean;
  onAssignmentRemove?: (assignmentId: string) => void;
}
```

**Columns**:
- Class Name
- Subject Name
- Assignment Date
- Status (Active / Ended)
- Actions (Remove)

**Features**:
- Group assignments by class
- Show removal date if assignment ended
- Confirm remove action with dialog
- Disable removal for active assignments (show modal to set end date)

**API Integration**:
- Fetch: Included in teacher detail query
- Remove: `DELETE /api/v1/teachers/{id}/assignments/{assignmentId}`

---

## Fee Management Components

### 7. FeeStructureListPage

**Purpose**: Display fee structures with CRUD operations  
**Path**: `src/pages/fees/FeeStructureListPage.tsx`

**Props**:
```typescript
interface FeeStructureListPageProps {
  onStructureSelect?: (structureId: string) => void;
}
```

**State Management**:
- Pagination, search, filters (by academic year, active status)
- Selected structure for detailed view

**Child Components**:
- `FeeStructureSearchBar`: Filter by year, active status
- `FeeStructureTable`: Display structures with summary info
- `FeeStructureActions`: Create, Edit, Delete, View Details
- `Pagination`

**Features**:
- List all fee structures with filter options
- Show structure name, frequency, total amount, students assigned
- Quick edit inline (name, status)
- Bulk activate/deactivate
- Search by structure name

**API Integration**:
- List: `GET /api/v1/fee-structures`

---

### 8. FeeStructureForm

**Purpose**: Create and edit fee structures with dynamic category management  
**Path**: `src/components/forms/FeeStructureForm.tsx`

**Props**:
```typescript
interface FeeStructureFormProps {
  initialData?: FeeStructureFormData;
  isLoading?: boolean;
  onSubmit: (data: FeeStructureFormData) => Promise<void>;
  onCancel?: () => void;
  mode: 'create' | 'edit';
}

interface FeeStructureFormData {
  name: string;
  academicYear: number;
  frequency: 'monthly' | 'quarterly' | 'yearly';
  categories: FeeCategory[];
}

interface FeeCategory {
  category: string;
  amount: number;
}
```

**Dynamic Sections**:
- **Basic Info**: Name, academic year, frequency
- **Categories**: 
  - Add/remove category rows
  - Category dropdown (tuition, transport, uniform, activities, etc.)
  - Amount input with real-time total calculation
  - Show summary: Total Amount

**Validation**:
- Name required, max 100 chars
- Academic year >= 2020
- Frequency required
- At least 1 category
- Each category amount > 0
- Categories must be unique (no duplicate category types)

**Features**:
- Real-time total calculation as categories updated
- Preset category suggestions
- Can't edit structure if already assigned to students (show alert)

**API Integration**:
- Create: `POST /api/v1/fee-structures`
- Edit: `PUT /api/v1/fee-structures/{id}`

---

### 9. StudentFeeAssignmentModal

**Purpose**: Modal for assigning fee structure to student(s)  
**Path**: `src/components/modals/StudentFeeAssignmentModal.tsx`

**Props**:
```typescript
interface StudentFeeAssignmentModalProps {
  studentIds: string[];
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}
```

**Form Fields**:
- feeStructureId (required, dropdown)
- startDate (required, date picker)
- endDate (required, date picker)
- customAmount (optional, number - override default)
- bulkAssign (checkbox - apply to all selected students)

**Features**:
- Show selected student count
- Preview fee schedule for selected structure
- Calculate total obligation (periods × amount)
- Warn if student already has fees assigned
- Support bulk assignment to multiple students

**API Integration**:
- Fetch structures: `GET /api/v1/fee-structures`
- Assign: `POST /api/v1/students/{studentId}/fees` (called in loop for bulk)
- Show progress bar for bulk operations

---

### 10. StudentFeeStatusCard

**Purpose**: Display student fee status with payment details  
**Path**: `src/components/cards/StudentFeeStatusCard.tsx`

**Props**:
```typescript
interface StudentFeeStatusCardProps {
  studentId: string;
  showPaymentHistory?: boolean;
}
```

**Display Sections**:
- **Summary**: Total due, paid, outstanding, status badge
- **Current Fees**: List of active fee structures
  - For each fee: due amount, paid amount, remaining balance
  - Period breakdown (monthly periods with due dates)
- **Payment History**: Recent 5 payments (reverse chronological)
- **Action Buttons**: Record Payment, Print Receipt, View Full History

**Visual Indicators**:
- Red badge: Amount overdue
- Yellow badge: Due soon (<7 days)
- Green badge: Current on payments
- Progress bar: % of total fee paid

**API Integration**:
- Fetch: `GET /api/v1/students/{studentId}/fee-status`

---

### 11. PaymentRecordingModal

**Purpose**: Modal for recording student fee payments  
**Path**: `src/components/modals/PaymentRecordingModal.tsx`

**Props**:
```typescript
interface PaymentRecordingModalProps {
  studentId: string;
  studentFeeId?: string;
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}
```

**Form Fields**:
- studentFeeId (dropdown if not provided)
- amountPaid (number, max = current balance)
- paymentDate (date picker, default today, allow past dates)
- receiptNumber (required, text, auto-generate prefix)
- paymentMethod (dropdown: cash, check, bank_transfer)
- notes (textarea, optional)

**Display**:
- Student name, fee structure name
- Outstanding amount for selected fee
- Calculate balance after payment
- Warn if trying to pay more than owed

**Features**:
- Auto-generate receipt number with date prefix
- Quick presets: Full payment, Half payment, Custom amount
- Print receipt after successful payment

**API Integration**:
- Submit: `POST /api/v1/students/{studentId}/fee-payments`
- Generate receipt: Included in response
- Success: Show receipt preview with print option

---

### 12. OutstandingFeesReport

**Purpose**: Dashboard report of students with outstanding fees  
**Path**: `src/components/reports/OutstandingFeesReport.tsx`

**Props**:
```typescript
interface OutstandingFeesReportProps {
  daysOverdueFilter?: number;
  classFilter?: string;
  isLoading?: boolean;
}
```

**Display**:
- Summary metrics: Total outstanding, number of students, average days overdue
- Table with columns:
  - Student Name
  - Class
  - Outstanding Amount
  - Days Overdue
  - Last Payment Date
  - Action: Send Reminder, Record Payment

**Features**:
- Sort by: amount (desc), daysOverdue (desc), name
- Filter by days overdue (show only > 30 days past due)
- Filter by class
- Export to CSV/Excel
- Color code rows: Red (>60 days), Orange (30-60 days), Yellow (< 30 days)
- Bulk send reminder emails to selected students

**API Integration**:
- Fetch: `GET /api/v1/fees/outstanding?sortBy=...&daysOverdue=...`
- Open payment modal on action click

---

## Attendance Management Components

### 13. AttendanceMarkingPage

**Purpose**: Page for marking daily class attendance  
**Path**: `src/pages/attendance/AttendanceMarkingPage.tsx`

**Props**:
```typescript
interface AttendanceMarkingPageProps {
  selectedClassId?: string;
  selectedDate?: string;
}
```

**Sections**:
- **Date & Class Selection**: 
  - Date picker (default today)
  - Class dropdown
  - Show class strength, current period
- **Student Checklist**: 
  - List of all students in class
  - Radio buttons: Present / Absent / Leave / Unexcused
  - Reason text field (shows for Absent/Leave/Unexcused)
  - Batch actions: Mark All Present, Mark All Absent
- **Summary**: 
  - Count of each status
  - Attendance percentage
  - Alerts for 100% absent (likely error)

**Features**:
- Save draft automatically (localStorage)
- Restore draft on page reload
- Keyboard shortcuts: P=Present, A=Absent, L=Leave, U=Unexcused
- Search student by name/roll number
- Tab key navigates between fields
- Save only after confirmation

**API Integration**:
- Fetch students: From existing class data
- Submit: `POST /api/v1/attendance/student`
- Show success toast with summary
- Allow undo for last 5 minutes

---

### 14. AttendanceEditModal

**Purpose**: Modal for editing previous attendance records  
**Path**: `src/components/modals/AttendanceEditModal.tsx`

**Props**:
```typescript
interface AttendanceEditModalProps {
  attendanceId: string;
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
}
```

**Fields**:
- status (dropdown: present, absent, leave, unexcused)
- reason (textarea)
- editReason (required, textarea - why changing)

**Audit Trail**:
- Show previous values
- Display edit history (who changed it, when, why)

**API Integration**:
- Fetch: Included in attendance detail
- Submit: `PUT /api/v1/attendance/student/{attendanceId}`

---

### 15. StudentAttendanceReport

**Purpose**: Display student attendance for a specific month/year  
**Path**: `src/components/reports/StudentAttendanceReport.tsx`

**Props**:
```typescript
interface StudentAttendanceReportProps {
  studentId: string;
  month?: string; // YYYY-MM
  year?: number;
  showMonthSelector?: boolean;
}
```

**Display**:
- **Summary**: 
  - Working days, present, absent, leave, unexcused, percentage
  - Status badge: Normal, Low Attendance, Absent Frequently
- **Calendar View**: 
  - Month calendar showing each date
  - Color coded: Green (present), Red (absent), Yellow (leave), Gray (holiday/weekend)
  - Click date to see details
- **Details Table**: 
  - List all daily records
  - Show class, date, status, reason, marked by

**Features**:
- Month/year selector
- Toggle between calendar and list view
- Export attendance certificate (if meets threshold)
- Show trend: attendance last 3 months
- Highlight low attendance weeks

**API Integration**:
- Fetch: `GET /api/v1/students/{studentId}/attendance?month=...&year=...`

---

### 16. ClassAttendanceSummary

**Purpose**: Summary of attendance for entire class  
**Path**: `src/components/reports/ClassAttendanceSummary.tsx`

**Props**:
```typescript
interface ClassAttendanceSummaryProps {
  classId: string;
  month?: string;
  showStudentBreakdown?: boolean;
}
```

**Display**:
- **Class Summary**:
  - Average attendance percentage
  - Highest/lowest attending students
  - Total working days, overall present count
- **Student Breakdown** (if enabled):
  - Table: Student name, present days, absent days, leave, percentage
  - Sort by: name, percentage
  - Filter: Show only low attendance (<75%)
  - Highlight: Students with < 75% attendance in red

**Features**:
- Month/year selector
- Export to CSV for office records
- Print-friendly view
- Trend chart: Class average last 3 months

**API Integration**:
- Fetch: `GET /api/v1/classes/{classId}/attendance-summary?month=...`

---

### 17. TeacherAttendancePage

**Purpose**: Mark and view teacher attendance  
**Path**: `src/pages/attendance/TeacherAttendancePage.tsx`

**Child Components**:
- `TeacherAttendanceForm`: Form to mark teacher attendance
- `TeacherAttendanceHistory`: Recent teacher attendance records

**TeacherAttendanceForm Fields**:
- Teacher (dropdown or search)
- Date (date picker)
- Status (radio: present, absent, leave)
- Reason (textarea, optional)

**TeacherAttendanceHistory**:
- Table with: Teacher name, date, status, reason, recorded by
- Pagination
- Filter by teacher, date range
- Sort by date (desc)

**Features**:
- Bulk mark attendance (multiple teachers at once)
- Import from CSV (teacher id, date, status)

**API Integration**:
- Mark: `POST /api/v1/attendance/teacher`
- Report: `GET /api/v1/teachers/{teacherId}/attendance-report`

---

### 18. TeacherAttendanceReport

**Purpose**: Teacher attendance report for salary/bonus calculations  
**Path**: `src/components/reports/TeacherAttendanceReport.tsx`

**Props**:
```typescript
interface TeacherAttendanceReportProps {
  teacherId: string;
  period?: 'month' | 'quarter' | 'year';
  month?: number;
  year?: number;
}
```

**Display**:
- **Summary**:
  - Working days, present, absent, leave
  - Attendance percentage
  - Bonus eligibility (if >= 90%)
- **Daily Records**: Calendar or table view
- **Bonus Calculation**:
  - Minimum required: 90%
  - Achieved: X%
  - Eligible: Yes/No
  - Bonus notes

**Features**:
- Period selector: Month, Quarter, Year
- Export attendance certificate
- Generate salary report (with bonus calculation)

**API Integration**:
- Fetch: `GET /api/v1/teachers/{teacherId}/attendance-report?period=...`

---

## Dashboard Components

### 19. DashboardSummaryCards

**Purpose**: Top-level KPI cards on dashboard  
**Path**: `src/components/dashboard/DashboardSummaryCards.tsx`

**Cards Displayed**:
1. **Teachers**: Total count, active count, new this month
2. **Students**: Total count, active count, new admissions
3. **Fees**: Expected this month, collected, %, outstanding
4. **Attendance**: Student avg, teacher avg, low attendance count

**Visual Design**:
- Each card shows metric with icon
- Trend indicator (up/down arrow)
- Click to navigate to detail page

**API Integration**:
- Fetch: `GET /api/v1/dashboard/summary`
- Auto-refetch every 5 minutes

---

### 20. FeesCollectionChart

**Purpose**: Visual representation of fee collection progress  
**Path**: `src/components/dashboard/FeesCollectionChart.tsx`

**Chart Types**:
- **Monthly Collection**: Bar chart showing target vs actual collection
- **Outstanding Breakdown**: Pie chart (by days overdue: <30, 30-60, 60+)
- **Collection Trend**: Line chart (last 6 months)

**Features**:
- Interactive tooltips
- Drill-down: Click bar to see student details
- Export chart as image

**API Integration**:
- Fetch data from dashboard summary
- Optional: Separate endpoint for detailed historical data

---

### 21. AttendanceTrendChart

**Purpose**: Visualize attendance trends  
**Path**: `src/components/dashboard/AttendanceTrendChart.tsx`

**Chart Types**:
- **Student Attendance**: Line chart (last 30 days average)
- **Teacher Attendance**: Line chart (last 30 days average)
- **By Status**: Stacked bar chart (present, absent, leave breakdown)

**Features**:
- Compare student vs teacher
- Hover to see daily values
- Toggle between views

---

## Shared UI Components

### 22. FormInputField

**Purpose**: Wrapper around MUI TextField with consistent styling  
**Path**: `src/components/form-inputs/FormInputField.tsx`

**Features**:
- Error message display
- Required indicator
- Character count for text areas
- Consistent validation styling

---

### 23. FormDatePicker

**Purpose**: Date selection with validation  
**Path**: `src/components/form-inputs/FormDatePicker.tsx`

**Features**:
- Restrict dates (e.g., not in future)
- Disable weekends if needed
- Show inline validation

---

### 24. FormSelectDropdown

**Purpose**: Dropdown with async data loading  
**Path**: `src/components/form-inputs/FormSelectDropdown.tsx`

**Features**:
- Async data loading from API
- Search/filter functionality
- Cascade dropdowns (when one selection affects another)
- Loading skeleton

---

### 25. ConfirmDialog

**Purpose**: Confirmation modal for dangerous actions  
**Path**: `src/components/dialogs/ConfirmDialog.tsx`

**Props**:
```typescript
interface ConfirmDialogProps {
  isOpen: boolean;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  isDangerous?: boolean; // Shows red button
  isLoading?: boolean;
  onConfirm: () => Promise<void>;
  onCancel: () => void;
}
```

---

### 26. ErrorBoundary

**Purpose**: Catch React errors and display fallback UI  
**Path**: `src/components/error-handling/ErrorBoundary.tsx`

**Features**:
- Catch render errors
- Display error details (dev) or friendly message (prod)
- Retry button
- Log to error tracking service

---

## State Management Patterns

### React Query Integration

All data fetching uses React Query:

```typescript
// List data
const { data, isLoading, error, refetch } = useQuery({
  queryKey: ['teachers', { pageNumber, pageSize, searchTerm }],
  queryFn: () => fetchTeachers({ pageNumber, pageSize, searchTerm })
});

// Detail data
const { data: teacher } = useQuery({
  queryKey: ['teacher', teacherId],
  queryFn: () => fetchTeacher(teacherId),
  enabled: !!teacherId
});

// Mutations
const { mutate, isPending } = useMutation({
  mutationFn: (data) => createTeacher(data),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['teachers'] });
    toast.success('Teacher created successfully');
  },
  onError: (error) => {
    toast.error(error.message);
  }
});
```

---

## Accessibility Requirements

All components must follow:
- WCAG 2.1 AA standards
- Proper heading hierarchy
- ARIA labels for icons and buttons
- Keyboard navigation (Tab, Enter, Escape)
- Color contrast ratios
- Focus indicators
- Error announcements

---

## Responsive Design

Breakpoints:
- Mobile: < 600px
- Tablet: 600px - 1024px
- Desktop: > 1024px

All components responsive across all breakpoints.

---

## Form Validation Schema (Zod)

Example for teacher form:

```typescript
const teacherFormSchema = z.object({
  firstName: z.string().min(1).max(50),
  lastName: z.string().min(1).max(50),
  email: z.string().email(),
  phone: z.string().optional(),
  qualification: z.string().max(500).optional(),
  experienceYears: z.number().int().min(0).optional(),
  joiningDate: z.string().pipe(z.coerce.date()),
  isActive: z.boolean().optional()
});
```

---

## Mock Data Examples

### Teacher Mock:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "firstName": "Priya",
  "lastName": "Kumar",
  "email": "priya.kumar@school.edu",
  "phone": "+91-9876543210",
  "qualification": "M.Sc. Mathematics, B.Ed.",
  "experienceYears": 5,
  "joiningDate": "2026-01-15",
  "status": "Active",
  "isActive": true,
  "assignedClasses": [
    { "classId": "cls-1", "className": "10A", "subjectName": "Mathematics" }
  ]
}
```

### Fee Structure Mock:
```json
{
  "id": "fee-struct-1",
  "name": "Regular Monthly 2026",
  "academicYear": 2026,
  "frequency": "monthly",
  "totalAmount": 5500,
  "categories": [
    { "category": "tuition", "amount": 5000 },
    { "category": "transport", "amount": 500 }
  ],
  "isActive": true,
  "studentsAssigned": 145
}
```

---

## Integration Checklist

- [ ] All components use React Query for data fetching
- [ ] All forms use React Hook Form + Zod validation
- [ ] All API calls use JWT authentication
- [ ] All modals have close handlers (ESC key)
- [ ] All lists support pagination and sorting
- [ ] All date inputs use consistent date picker
- [ ] All responses show toast notifications
- [ ] All errors display user-friendly messages
- [ ] All components have loading states
- [ ] All components are responsive
- [ ] All components follow accessibility guidelines
- [ ] All components have proper TypeScript types
- [ ] All components have proper error boundaries


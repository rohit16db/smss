import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5208/api';

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor to add auth token and log requests (for debugging)
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    // Add active academic year ID from localStorage
    const academicYearId = localStorage.getItem('selectedAcademicYearId');
    if (academicYearId) {
      config.headers['X-Academic-Year-Id'] = academicYearId;
    }

    console.log('API Request:', {
      baseURL: config.baseURL,
      url: config.url,
      fullURL: `${config.baseURL}${config.url}`,
      hasAuth: !!token,
      academicYearId: academicYearId,
    });
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add response interceptor to handle 401 responses
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid, redirect to login
      localStorage.removeItem('authToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Student Types
export type Student = {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  dateOfBirth: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  parentName?: string;
  parentPhone?: string;
  parentEmail?: string;
  enrollmentDate: string;
  enrollmentNumber: string;
  isActive: boolean;
  currentSectionId?: string;
  currentSectionName?: string;
  currentClassName?: string;
  imagePath?: string;
};

export type CreateStudentDto = {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  guardianName?: string;
  guardianPhone?: string;
  guardianEmail?: string;
  enrollmentDate: string;
};

export type UpdateStudentDto = {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  guardianName?: string;
  guardianPhone?: string;
  guardianEmail?: string;
  isActive: boolean;
};

export type PaginatedStudentList = {
  items: Student[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export const studentApi = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; searchTerm?: string; isActive?: boolean }) => {
    const response = await api.get<PaginatedStudentList>('/students', { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Student>(`/students/${id}`);
    return response.data;
  },

  create: async (data: CreateStudentDto) => {
    const response = await api.post<Student>('/students', data);
    return response.data;
  },

  update: async (id: string, data: UpdateStudentDto) => {
    const response = await api.put<Student>(`/students/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/students/${id}`);
  },

  activate: async (id: string) => {
    const response = await api.patch<Student>(`/students/${id}/activate`);
    return response.data;
  },

  deactivate: async (id: string) => {
    const response = await api.patch<Student>(`/students/${id}/deactivate`);
    return response.data;
  },

  uploadImage: async (id: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post<{ message: string; imagePath: string }>(`/students/${id}/upload-image`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};

// Staff Types
export type Staff = {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  designation: string;
  roleType: number;
  departmentId?: string;
  departmentName?: string;
  qualification?: string;
  experienceYears: number;
  joiningDate: string;
  isActive: boolean;
  imagePath?: string;
};

export type CreateStaffDto = {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  designation: string;
  roleType: number;
  departmentId?: string;
  qualification?: string;
  experienceYears: number;
  joiningDate: string;
  imagePath?: string;
};

export type UpdateStaffDto = {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  designation: string;
  roleType: number;
  departmentId?: string;
  qualification?: string;
  experienceYears: number;
  joiningDate: string;
  isActive: boolean;
  imagePath?: string;
};

export type PaginatedStaffList = {
  items: Staff[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type StaffAssignment = {
  id: string;
  staffId: string;
  staffName?: string;
  classId: string;
  sectionId: string;
  subjectId: string;
  assignmentDate: string;
  removalDate?: string;
  className?: string;
  sectionName?: string;
  subjectName?: string;
  subjectCode?: string;
  isActive: boolean;
};

export type CreateStaffAssignmentDto = {
  classId: string;
  sectionId: string;
  subjectId: string;
  assignmentDate?: string;
};

export const StaffApi = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; searchTerm?: string; isActive?: boolean }) => {
    const response = await api.get<PaginatedStaffList>('/staff', { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Staff>(`/staff/${id}`);
    return response.data;
  },

  create: async (data: CreateStaffDto) => {
    const response = await api.post<Staff>('/staff', data);
    return response.data;
  },

  update: async (id: string, data: UpdateStaffDto) => {
    const response = await api.put<Staff>(`/staff/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/staff/${id}`);
  },

  activate: async (id: string) => {
    const response = await api.patch<Staff>(`/staff/${id}/activate`);
    return response.data;
  },

  deactivate: async (id: string) => {
    const response = await api.patch<Staff>(`/staff/${id}/deactivate`);
    return response.data;
  },

  uploadImage: async (id: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post<{ message: string; imagePath: string }>(`/staff/${id}/upload-image`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  // Staff Assignment APIs
  getAssignments: async (StaffId: string, activeOnly?: boolean) => {
    const response = await api.get<StaffAssignment[]>(`/staff/${StaffId}/assignments`, {
      params: { activeOnly }
    });
    return response.data;
  },

  getAssignmentsBySection: async (sectionId: string, academicYearId: string) => {
    const response = await api.get<StaffAssignment[]>(`/staff/section/${sectionId}/${academicYearId}`);
    return response.data;
  },

  createAssignment: async (StaffId: string, data: CreateStaffAssignmentDto) => {
    const response = await api.post<StaffAssignment>(`/staff/${StaffId}/assignments`, data);
    return response.data;
  },

  removeAssignment: async (StaffId: string, assignmentId: string, removalDate?: string) => {
    await api.delete(`/staff/${StaffId}/assignments/${assignmentId}`, {
      data: { assignmentId, removalDate }
    });
  },
};

// Fee Types
export type FeeStructure = {
  id: string;
  name: string;
  academicYearId: string;
  academicYearName: string;
  frequency: string;
  totalAmount: number;
  isActive: boolean;
  categories: FeeCategory[];
};

export type FeeCategory = {
  id: string;
  category: string;
  amount: number;
};

export type CreateFeeStructureDto = {
  name: string;
  academicYearId: string;
  frequency: string;
  totalAmount: number;
  categories: { category: string; amount: number }[];
};

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
  // Section context (student's current enrolled section)
  sectionId?: string;
  sectionName?: string;
};

export type CreateStudentFeeDto = {
  studentId: string;
  feeStructureId: string;
  startDate: string;
  endDate?: string;
};

export type BulkAssignStudentFeeDto = {
  feeStructureId: string;
  sectionId: string;
  startDate: string;
  endDate?: string;
  skipAlreadyAssigned: boolean;
};

export type AssignmentErrorDto = {
  studentId: string;
  studentName: string;
  errorMessage: string;
};

export type BulkAssignmentResultDto = {
  successCount: number;
  skippedCount: number;
  failureCount: number;
  totalAssignedAmount: number;
  errors: AssignmentErrorDto[];
  assignedAt: string;
};

export type FeePayment = {
  id: string;
  studentFeeId: string;
  amountPaid: number;
  paymentDate: string;
  receiptNumber: string;
  paymentMethod: string;
  notes?: string;
  createdAt: string;
};

export type CreateFeePaymentDto = {
  studentFeeId: string;
  amountPaid: number;
  paymentDate: string;
  paymentMethod: string;
  notes?: string;
};

export type PaginatedFeeStructureList = {
  items: FeeStructure[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type PaginatedStudentFeeList = {
  items: StudentFee[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type PaginatedFeePaymentList = {
  items: FeePayment[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type FeeReport = {
  id: string;
  studentId: string;
  studentName: string;
  enrollmentNumber: string;
  sectionId?: string;
  sectionName?: string;
  feeStructureId: string;
  feeStructureName: string;
  totalAmount: number;
  paidAmount: number;
  balanceAmount: number;
  status: 'Paid' | 'Partial' | 'Due' | 'Overdue';
  lastPaymentDate?: string;
  startDate: string;
  dueDate?: string;
};

export type PaginatedFeeReportList = {
  items: FeeReport[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalDueAmount: number;
  totalPaidAmount: number;
  totalBalanceAmount: number;
  paidCount: number;
  partialCount: number;
  dueCount: number;
  overdueCount: number;
};

export const feeApi = {
  // Fee Structures
  getAllStructures: async (params?: { pageNumber?: number; pageSize?: number; searchTerm?: string; isActive?: boolean }) => {
    const response = await api.get<PaginatedFeeStructureList>('/fees/structures', { params });
    return response.data;
  },

  getStructureById: async (id: string) => {
    const response = await api.get<FeeStructure>(`/fees/structures/${id}`);
    return response.data;
  },

  createStructure: async (data: CreateFeeStructureDto) => {
    const response = await api.post<FeeStructure>('/fees/structures', data);
    return response.data;
  },

  updateStructure: async (id: string, data: Partial<CreateFeeStructureDto> & { id: string }) => {
    const response = await api.put<FeeStructure>(`/fees/structures/${id}`, data);
    return response.data;
  },

  deleteStructure: async (id: string) => {
    await api.delete(`/fees/structures/${id}`);
  },

  // Student Fees
  getAllStudentFees: async (params?: { pageNumber?: number; pageSize?: number; studentId?: string; isActive?: boolean }) => {
    const response = await api.get<PaginatedStudentFeeList>('/fees/student-fees', { params });
    return response.data;
  },

  getStudentFeeById: async (id: string) => {
    const response = await api.get<StudentFee>(`/fees/student-fees/${id}`);
    return response.data;
  },

  getStudentFeesBySection: async (sectionId: string, isActive?: boolean) => {
    const response = await api.get<StudentFee[]>(`/fees/student-fees/section/${sectionId}`, {
      params: isActive !== undefined ? { isActive } : undefined,
    });
    return response.data;
  },

  assignFeeToStudent: async (data: CreateStudentFeeDto) => {
    const response = await api.post<StudentFee>('/fees/student-fees', data);
    return response.data;
  },

  terminateStudentFee: async (id: string, endDate: string) => {
    const response = await api.patch<StudentFee>(`/fees/student-fees/${id}/terminate`, { endDate });
    return response.data;
  },

  bulkAssignStudentFee: async (data: BulkAssignStudentFeeDto) => {
    const response = await api.post<BulkAssignmentResultDto>('/fees/student-fees/bulk-assign', data);
    return response.data;
  },

  downloadStudentFeePdf: async (id: string) => {
    const response = await api.get(`/fees/student-fees/${id}/pdf`, {
      responseType: 'arraybuffer',
    });
    return response.data;
  },

  // Payments
  getAllPayments: async (params?: { pageNumber?: number; pageSize?: number; studentFeeId?: string }) => {
    const response = await api.get<PaginatedFeePaymentList>('/fees/payments', { params });
    return response.data;
  },

  getPaymentById: async (id: string) => {
    const response = await api.get<FeePayment>(`/fees/payments/${id}`);
    return response.data;
  },

  recordPayment: async (data: CreateFeePaymentDto) => {
    const response = await api.post<FeePayment>('/fees/payments', data);
    return response.data;
  },

  downloadFeeReceipt: async (paymentId: string) => {
    const response = await api.get(`/fees/payments/${paymentId}/receipt`, {
      responseType: 'arraybuffer',
    });
    return response.data;
  },

  // Fee Report
  getReport: async (params?: { 
    pageNumber?: number; 
    pageSize?: number; 
    studentId?: string; 
    sectionId?: string; 
    status?: string;
    startDate?: string;
    endDate?: string;
  }) => {
    const response = await api.get<PaginatedFeeReportList>('/fees/report', { params });
    return response.data;
  },
};

// Attendance Types
export type StudentAttendance = {
  id: string;
  studentId: string;
  sectionId: string;  // Kept in response - shows which section was recorded
  studentEnrollmentNumber?: string;
  studentName?: string;
  attendanceDate: string;
  status: 'Present' | 'Absent' | 'Late' | 'Leave';
  reason?: string;
  remarks?: string;
};

export type StaffAttendance = {
  id: string;
  staffId: string;
  staffName?: string;
  staffEmail?: string;
  attendanceDate: string;
  status: 'Present' | 'Absent' | 'Late' | 'Leave';
  reason?: string;
  remarks?: string;
};

export type CreateStudentAttendanceDto = {
  studentId: string;
  // sectionId removed - auto-detected from student's current enrollment
  attendanceDate: string;
  status: 'Present' | 'Absent' | 'Late' | 'Leave';
  reason?: string;
};

export type CreateStaffAttendanceDto = {
  staffId: string;
  attendanceDate: string;
  status: 'Present' | 'Absent' | 'Late' | 'Leave';
  reason?: string;
};

export type PaginatedStudentAttendanceList = {
  items: StudentAttendance[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type PaginatedStaffAttendanceList = {
  items: StaffAttendance[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

// Attendance Report Types
export type MonthlyAttendanceReportItem = {
  studentId: string;
  studentName: string;
  enrollmentNumber: string;
  sectionId: string;
  sectionName: string;
  year: number;
  month: number;
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  leaveDays: number;
  attendancePercentage: number;
  attendanceStatus: 'Good' | 'Warning' | 'Critical';
};

export type PaginatedMonthlyAttendanceReportDto = {
  items: MonthlyAttendanceReportItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  averageAttendancePercentage: number;
  lowAttendanceCount: number;
};

export type LowAttendanceAlertDto = {
  studentId: string;
  studentName: string;
  enrollmentNumber: string;
  sectionId: string;
  sectionName: string;
  attendancePercentage: number;
  absentDays: number;
  totalDays: number;
  alertLevel: 'Warning' | 'Critical';
  lastAbsentDate: string;
};

export type ClassAttendanceSummaryDto = {
  sectionId: string;
  sectionName: string;
  className: string;
  totalStudents: number;
  averageAttendancePercentage: number;
  highAttendanceCount: number;
  mediumAttendanceCount: number;
  lowAttendanceCount: number;
  year: number;
  month: number;
};

export const attendanceApi = {
  // Student Attendance
  getAllStudentAttendance: async (params?: {
    pageNumber?: number;
    pageSize?: number;
    studentId?: string;
    attendanceDate?: string;
    startDate?: string;
    endDate?: string;
    status?: string;
  }) => {
    const response = await api.get<PaginatedStudentAttendanceList>('/attendance/students/history', { params });
    return response.data;
  },

  getStudentAttendanceById: async (id: string) => {
    const response = await api.get<StudentAttendance>(`/attendance/students/${id}`);
    return response.data;
  },

  recordStudentAttendance: async (data: CreateStudentAttendanceDto) => {
    const response = await api.post<StudentAttendance>('/attendance/students', data);
    return response.data;
  },

  updateStudentAttendance: async (id: string, data: Partial<CreateStudentAttendanceDto> & { id: string }) => {
    const response = await api.put<StudentAttendance>(`/attendance/students/${id}`, data);
    return response.data;
  },

  deleteStudentAttendance: async (id: string) => {
    await api.delete(`/attendance/students/${id}`);
  },

  // Staff Attendance
  getAllStaffAttendance: async (params?: {
    pageNumber?: number;
    pageSize?: number;
    staffId?: string;
    attendanceDate?: string;
    startDate?: string;
    endDate?: string;
    status?: string;
  }) => {
    const response = await api.get<PaginatedStaffAttendanceList>('/attendance/staff/history', { params });
    return response.data;
  },

  getStaffAttendanceById: async (id: string) => {
    const response = await api.get<StaffAttendance>(`/attendance/staff/${id}`);
    return response.data;
  },

  recordStaffAttendance: async (data: CreateStaffAttendanceDto) => {
    const response = await api.post<StaffAttendance>('/attendance/staff', data);
    return response.data;
  },

  updateStaffAttendance: async (id: string, data: Partial<CreateStaffAttendanceDto> & { id: string }) => {
    const response = await api.put<StaffAttendance>(`/attendance/staff/${id}`, data);
    return response.data;
  },

  deleteStaffAttendance: async (id: string) => {
    await api.delete(`/attendance/staff/${id}`);
  },

  // Report Methods
  getMonthlyAttendanceReport: async (year: number, month: number, params?: { pageNumber?: number; pageSize?: number; studentId?: string; sectionId?: string }) => {
    const response = await api.get<PaginatedMonthlyAttendanceReportDto>('/attendance/reports/monthly', {
      params: { year, month, ...params }
    });
    return response.data;
  },

  getLowAttendanceAlerts: async (year: number, month: number, params?: { sectionId?: string; threshold?: number }) => {
    const response = await api.get<LowAttendanceAlertDto[]>('/attendance/reports/low-attendance', {
      params: { year, month, ...params }
    });
    return response.data;
  },

  getClassAttendanceSummary: async (year: number, month: number, sectionId?: string) => {
    const response = await api.get<ClassAttendanceSummaryDto[]>('/attendance/reports/class-summary', {
      params: { year, month, sectionId }
    });
    return response.data;
  },
};

// Class & Section Types
export type Class = {
  id: string;
  name: string;
  academicYear?: string;
  isActive: boolean;
  sections: Section[];
  createdAt: Date;
  updatedAt: Date;
};

export type ClassListDto = {
  id: string;
  name: string;
  academicYear?: string;
  isActive: boolean;
  sectionCount: number;
};

export type PaginatedClassListDto = {
  items: ClassListDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type Section = {
  id: string;
  classId: string;
  sectionName: string;
  isActive: boolean;
  studentCount: number;
  createdAt: Date;
  updatedAt: Date;
};

export type SectionListDto = {
  id: string;
  sectionName: string;
  isActive: boolean;
  studentCount: number;
};

export type StudentSection = {
  id: string;
  studentId: string;
  studentName: string;
  enrollmentNumber: string;
  sectionId: string;
  sectionName: string;
  className: string;
  joinedDate: Date;
  leftDate?: Date;
  isCurrent: boolean;
  rollNumber?: number;
};

export type StudentSectionHistoryDto = {
  items: StudentSection[];
  totalCount: number;
};

// Class API Client
export const classApi = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; searchTerm?: string; isActive?: boolean }) => {
    const response = await api.get<PaginatedClassListDto>('/v1/classes', { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Class>(`/v1/classes/${id}`);
    return response.data;
  },

  create: async (data: { name: string; academicYear?: string }) => {
    const response = await api.post<Class>('/v1/classes', data);
    return response.data;
  },

  update: async (id: string, data: { name: string; academicYear?: string; isActive: boolean }) => {
    const response = await api.put<Class>(`/v1/classes/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/v1/classes/${id}`);
  },

  getSectionsByClass: async (classId: string) => {
    const response = await api.get<SectionListDto[]>(`/v1/classes/${classId}/sections`);
    return response.data;
  },

  getSectionById: async (id: string) => {
    const response = await api.get<Section>(`/v1/classes/sections/${id}`);
    return response.data;
  },

  createSection: async (data: { classId: string; sectionName: string }) => {
    const response = await api.post<Section>('/v1/classes/sections', data);
    return response.data;
  },

  updateSection: async (id: string, data: { sectionName: string; isActive: boolean }) => {
    const response = await api.put<Section>(`/v1/classes/sections/${id}`, data);
    return response.data;
  },

  deleteSection: async (id: string) => {
    await api.delete(`/v1/classes/sections/${id}`);
  },

  getStudentSectionHistory: async (studentId: string) => {
    const response = await api.get<StudentSectionHistoryDto>(`/v1/classes/students/${studentId}/section-history`);
    return response.data;
  },

  getStudentCurrentSection: async (studentId: string) => {
    const response = await api.get<StudentSection>(`/v1/classes/students/${studentId}/current-section`);
    return response.data;
  },

  moveStudentToSection: async (studentId: string, newSectionId: string) => {
    const response = await api.post<StudentSection>(`/v1/classes/students/${studentId}/move-section`, {
      newSectionId,
    });
    return response.data;
  },
  
  // Roll Number Management
  getRollNumbers: async (sectionId: string) => {
    const response = await api.get<StudentSection[]>(`/v1/classes/sections/${sectionId}/roll-numbers`);
    return response.data;
  },
  
  autoAssignRollNumbers: async (sectionId: string) => {
    const response = await api.post<{ message: string }>(`/v1/classes/sections/${sectionId}/auto-assign-roll-numbers`);
    return response.data;
  },
  
  updateRollNumber: async (studentSectionId: string, rollNumber: number) => {
    const response = await api.put<{ message: string }>(`/v1/classes/student-sections/${studentSectionId}/roll-number`, {
      rollNumber,
    });
    return response.data;
  },
  
  bulkUpdateRollNumbers: async (sectionId: string, rollNumberUpdates: Record<string, number>) => {
    const response = await api.put<{ message: string }>(`/v1/classes/sections/${sectionId}/bulk-update-roll-numbers`, {
      rollNumberUpdates,
    });
    return response.data;
  },
};

// Subject Types
export type Subject = {
  id: string;
  name: string;
  code: string;
  description?: string;
  credits?: number;
  isActive: boolean;
  displayOrder: number;
  createdAt: string;
  updatedAt: string;
};

export type SubjectListDto = {
  id: string;
  name: string;
  code: string;
  credits?: number;
  isActive: boolean;
  displayOrder: number;
};

export type PaginatedSubjectListDto = {
  items: SubjectListDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type CreateSubjectDto = {
  name: string;
  code: string;
  description?: string;
  credits?: number;
  displayOrder: number;
};

export type UpdateSubjectDto = {
  name: string;
  code: string;
  description?: string;
  credits?: number;
  isActive: boolean;
  displayOrder: number;
};

// Subject API
export const subjectApi = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; searchTerm?: string; isActive?: boolean }) => {
    const response = await api.get<PaginatedSubjectListDto>('/subjects', { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Subject>(`/subjects/${id}`);
    return response.data;
  },

  getActive: async () => {
    const response = await api.get<SubjectListDto[]>('/subjects/active');
    return response.data;
  },

  create: async (data: CreateSubjectDto) => {
    const response = await api.post<Subject>('/subjects', data);
    return response.data;
  },

  update: async (id: string, data: UpdateSubjectDto) => {
    const response = await api.put<Subject>(`/subjects/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/subjects/${id}`);
  },
};

// Holiday Types
export type Holiday = {
  id: string;
  name: string;
  holidayDate: string; // Date string in ISO format
  description?: string;
  type?: string;
  academicYearId: string; // Guid
  academicYearName?: string;
  createdAt: string;
  updatedAt: string;
  createdBy?: string;
  updatedBy?: string;
};

export type CreateHolidayDto = {
  name: string;
  holidayDate: string; // Date string in ISO format
  description?: string;
  type?: string;
  academicYearId: string; // Guid
};

export type UpdateHolidayDto = {
  name: string;
  holidayDate: string;
  description?: string;
  type?: string;
  academicYearId: string;
};

export type PaginatedHolidayListDto = {
  items: Holiday[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
};

// Holiday API
export const holidayApi = {
  getAll: async (params?: {
    pageNumber?: number;
    pageSize?: number;
    academicYearId?: string;
    startDate?: string;
    endDate?: string;
    type?: string;
  }) => {
    const response = await api.get<PaginatedHolidayListDto>('/holidays', { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Holiday>(`/holidays/${id}`);
    return response.data;
  },

  getHolidaysByMonth: async (year: number, month: number) => {
    const response = await api.get<Holiday[]>(`/holidays/month/${year}/${month}`);
    return response.data;
  },

  create: async (data: CreateHolidayDto) => {
    const response = await api.post<Holiday>('/holidays', data);
    return response.data;
  },

  update: async (id: string, data: UpdateHolidayDto) => {
    const response = await api.put<Holiday>(`/holidays/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/holidays/${id}`);
  },
};

// Report Types
export type OutstandingFeeDto = {
  studentId: string;
  studentInfo: string;
  classSection: string;
  dueAmount: number;
  daysOverdue: number;
  dueDate: string;
  lastPaymentDate?: string;
  agingBucket: string;
  remarks?: string;
  contactInfo?: string;
  isActive: boolean;
};

export type FeeCollectionSummaryDto = {
  currentPeriod: {
    totalCollected: number;
    numberOfTransactions: number;
    averagePerTransaction: number;
    completionPercentage: number;
  };
  previousPeriod?: {
    totalCollected: number;
    numberOfTransactions: number;
    averagePerTransaction: number;
    completionPercentage: number;
  };
  growth?: {
    percentageChange: number;
    absoluteChange: number;
  };
};

export type MonthlyTrendDto = {
  month: string;
  collected: number;
  pending: number;
  overdue: number;
  trend: number;
};

export type FeeCollectionByCategoryDto = {
  category: string;
  amount: number;
  collected: number;
  pending: number;
  percentageOfTotal: number;
};

export type StudentPaymentHistoryDto = {
  paymentDate: string;
  amountPaid: number;
  receiptNumber: string;
  paymentMethod: string;
  notes?: string;
  balanceAfterPayment: number;
};

export type SalaryExpenseSummaryDto = {
  currentPeriod: {
    totalExpense: number;
    baseSalary: number;
    bonuses: number;
    deductions: number;
    numberOfEmployees: number;
  };
  previousPeriod?: {
    totalExpense: number;
    baseSalary: number;
    bonuses: number;
    deductions: number;
    numberOfEmployees: number;
  };
  growth?: {
    percentageChange: number;
    absoluteChange: number;
  };
};

export type MonthlySalaryTrendDto = {
  month: string;
  baseSalary: number;
  bonuses: number;
  deductions: number;
  totalExpense: number;
  trend: number;
};

export type SalaryComponentBreakdownDto = {
  baseSalary: {
    amount: number;
    percentage: number;
    headCount: number;
  };
  bonuses: {
    amount: number;
    percentage: number;
    headCount: number;
  };
  deductions: {
    amount: number;
    percentage: number;
    description: string[];
  };
};

export type StaffSalaryComparisonDto = {
  StaffId: string;
  StaffName: string;
  baseSalary: number;
  bonus: number;
  deductions: number;
  netSalary: number;
  attendancePercentage?: number;
  bonusEligible: boolean;
  status: string;
};

export type AttendanceToSalaryCorrelationDto = {
  StaffId: string;
  StaffName: string;
  expectedDeduction: number;
  actualDeduction: number;
  discrepancy: number;
  attendancePercentage: number;
  workingDays: number;
  presentDays: number;
  absentDays: number;
};

export type BudgetVsActualDto = {
  budgetedAmount: number;
  actualAmount: number;
  variance: number;
  variancePercentage: number;
  month: string;
  category: string;
};

// Report API
export const reportApi = {
  // Fee Reports
  getOutstandingFees: async (params?: {
    asOfDate?: string;
    agingBucket?: string;
    minAmount?: number;
    sortBy?: string;
    descending?: boolean;
  }) => {
    const response = await api.get<OutstandingFeeDto[]>('/feereports/outstanding', { params });
    return response.data;
  },

  getFeeCollectionSummary: async (params: {
    startDate: string;
    endDate: string;
    category?: string;
    prevStartDate?: string;
    prevEndDate?: string;
  }) => {
    const response = await api.get<FeeCollectionSummaryDto>('/feereports/collection-summary', { params });
    return response.data;
  },

  getMonthlyFeeTrend: async (params: {
    startDate: string;
    endDate: string;
    category?: string;
  }) => {
    const response = await api.get<MonthlyTrendDto[]>('/feereports/monthly-trend', { params });
    return response.data;
  },

  getFeeCollectionByCategory: async (params: {
    startDate: string;
    endDate: string;
  }) => {
    const response = await api.get<FeeCollectionByCategoryDto[]>('/feereports/by-category', { params });
    return response.data;
  },

  getStudentPaymentHistory: async (studentId: string, params: {
    startDate: string;
    endDate: string;
  }) => {
    const response = await api.get<StudentPaymentHistoryDto[]>(`/feereports/student/${studentId}/payment-history`, { params });
    return response.data;
  },

  // Salary Reports
  getSalaryExpenseSummary: async (params: {
    startDate: string;
    endDate: string;
    prevStartDate?: string;
    prevEndDate?: string;
  }) => {
    const response = await api.get<SalaryExpenseSummaryDto>('/salary-reports/expense-summary', { params });
    return response.data;
  },

  getMonthlySalaryTrend: async (params: {
    startDate: string;
    endDate: string;
  }) => {
    const response = await api.get<MonthlySalaryTrendDto[]>('/salary-reports/monthly-trend', { params });
    return response.data;
  },

  getSalaryComponentBreakdown: async (params: {
    startDate: string;
    endDate: string;
  }) => {
    const response = await api.get<SalaryComponentBreakdownDto>('/salary-reports/component-breakdown', { params });
    return response.data;
  },

  getStaffSalaryComparison: async (params: {
    startDate: string;
    endDate: string;
    status?: string;
    sortBy?: string;
    descending?: boolean;
  }) => {
    const response = await api.get<StaffSalaryComparisonDto[]>('/salary-reports/staff-comparison', { params });
    return response.data;
  },

  getAttendanceToSalaryCorrelation: async (params: {
    month: string;
    onlyDiscrepancies?: boolean;
  }) => {
    const response = await api.get<AttendanceToSalaryCorrelationDto[]>('/salary-reports/attendance-correlation', { params });
    return response.data;
  },

  getBudgetVsActual: async (params: {
    reportType: string;
    startDate: string;
    endDate: string;
    groupBy?: string;
  }) => {
    const response = await api.get<BudgetVsActualDto[]>('/salary-reports/budget-vs-actual', { params });
    return response.data;
  },
};
// School Configuration Types
export type SchoolDto = {
  id: string;
  name: string;
  code: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  phoneNumber?: string;
  emailAddress?: string;
  website?: string;
  logoBase64?: string;
  establishedDate: Date;
  isActive: boolean;
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  headerText?: string;
  footerText?: string;
  dateFormat: string;
  currencyCode: string;
  currencySymbol: string;
};

export type AcademicYearDto = {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
};

// School/Settings API
export const settingsApi = {
  getSchoolSettings: async (): Promise<SchoolDto> => {
    const response = await api.get<SchoolDto>('/v1/settings/school');
    return response.data;
  },

  updateSchoolSettings: async (data: Partial<SchoolDto>): Promise<SchoolDto> => {
    const response = await api.put<SchoolDto>('/v1/settings/school', data);
    return response.data;
  },

  uploadLogo: async (file: File): Promise<SchoolDto> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await api.post<SchoolDto>('/v1/settings/school/logo', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return response.data;
  },

  getAcademicYears: async (): Promise<AcademicYearDto[]> => {
    const response = await api.get<AcademicYearDto[]>('/v1/settings/academic-years');
    return response.data;
  },

  getActiveAcademicYear: async (): Promise<AcademicYearDto> => {
    const response = await api.get<AcademicYearDto>('/v1/settings/academic-years/active');
    return response.data;
  },

  createAcademicYear: async (data: Omit<AcademicYearDto, 'id'>): Promise<AcademicYearDto> => {
    const response = await api.post<AcademicYearDto>('/v1/settings/academic-years', data);
    return response.data;
  },

  toggleAcademicYearStatus: async (id: string): Promise<boolean> => {
    const response = await api.patch<boolean>(`/v1/settings/academic-years/${id}/toggle-status`);
    return response.data;
  },
};

// Promotion Types
export type PromoteStudentsDto = {
  sourceAcademicYearId: string;
  targetAcademicYearId: string;
  studentIds: string[];
  targetClassId: string;
  targetSectionId?: string;
  markSourceAsPromoted?: boolean;
};

export type PromotionResultDto = {
  success: boolean;
  message: string;
  promotedCount: number;
  errors: string[];
};

// Promotion API
export const promotionApi = {
  promoteBulk: async (data: PromoteStudentsDto) => {
    const response = await api.post<PromotionResultDto>('/v1/promotions/bulk', data);
    return response.data;
  },
};

// Timetable Types
export type TimeSlot = {
  id: string;
  dayOfWeek: number; // 0=Sunday, 1=Monday...
  startTime: string; // "HH:mm:ss"
  endTime: string;
  name: string;
  isBreak: boolean;
  academicYearId: string;
};

export type CreateTimeSlotDto = Omit<TimeSlot, 'id'>;

export type TimetableEntry = {
  id: string;
  timeSlotId: string;
  timeSlotName: string;
  startTime: string;
  endTime: string;
  dayOfWeek: number;
  subjectId: string;
  subjectName: string;
  staffId: string;
  staffName: string;
  sectionId: string;
  sectionName: string;
  classId: string;
  className: string;
  roomNumber?: string;
  academicYearId: string;
};

export type CreateTimetableEntryDto = {
  timeSlotId: string;
  StaffAssignmentId: string;
  roomNumber?: string;
  academicYearId: string;
};

export type BulkCopyResultDto = {
  successCount: number;
  skippedCount: number;
  errors: string[];
};

export type BulkCopyRoutineDto = {
  academicYearId: string;
  sectionId?: string;
  staffId?: string;
  sourceDay: number;
  targetDays: number[];
};

// Timetable API
export const timetableApi = {
  // TimeSlots
  getTimeSlots: async (academicYearId: string) => {
    const response = await api.get<TimeSlot[]>(`/timetable/timeslots/${academicYearId}`);
    return response.data;
  },

  createTimeSlot: async (data: CreateTimeSlotDto) => {
    const response = await api.post<string>(`/timetable/timeslots`, data);
    return response.data;
  },

  updateTimeSlot: async (id: string, data: CreateTimeSlotDto) => {
    const response = await api.put<boolean>(`/timetable/timeslots/${id}`, data);
    return response.data;
  },

  deleteTimeSlot: async (id: string) => {
    await api.delete(`/timetable/timeslots/${id}`);
  },

  // Timetable Entries
  getSectionTimetable: async (sectionId: string, academicYearId: string) => {
    const response = await api.get<TimetableEntry[]>(`/timetable/entries/section/${sectionId}/${academicYearId}`);
    return response.data;
  },

  getStaffTimetable: async (staffId: string, academicYearId: string) => {
    const response = await api.get<TimetableEntry[]>(`/timetable/entries/staff/${staffId}/${academicYearId}`);
    return response.data;
  },

  createEntry: async (data: CreateTimetableEntryDto) => {
    const response = await api.post<string>(`/timetable/entries`, data);
    return response.data;
  },

  updateEntry: async (id: string, data: CreateTimetableEntryDto) => {
    const response = await api.put<boolean>(`/timetable/entries/${id}`, data);
    return response.data;
  },

  deleteEntry: async (id: string) => {
    await api.delete(`/timetable/entries/${id}`);
  },

  exportSectionTimetable: async (sectionId: string, academicYearId: string) => {
    const response = await api.get(`/timetable/entries/section/${sectionId}/${academicYearId}/export`, {
      responseType: 'blob'
    });
    return response.data;
  },

  exportStaffTimetable: async (staffId: string, academicYearId: string) => {
    const response = await api.get(`/timetable/entries/staff/${staffId}/${academicYearId}/export`, {
      responseType: 'blob'
    });
    return response.data;
  },

  bulkCreateTimeSlots: async (data: { academicYearId: string, sourceDay: number, targetDays: number[] }) => {
    const response = await api.post<boolean>(`/timetable/timeslots/bulk`, data);
    return response.data;
  },

  bulkCopyRoutine: async (data: BulkCopyRoutineDto) => {
    const response = await api.post<BulkCopyResultDto>(`/timetable/entries/bulk-copy`, data);
    return response.data;
  },
};

// Department Types
export type Department = {
  id: string;
  name: string;
  description?: string;
  headOfDepartmentId?: string;
  headOfDepartmentName?: string;
  staffCount: number;
  createdAt: string;
  updatedAt: string;
};

export type DepartmentListItem = {
  id: string;
  name: string;
  description?: string;
  headOfDepartmentName?: string;
  staffCount: number;
};

export type CreateDepartmentDto = {
  name: string;
  description?: string;
  headOfDepartmentId?: string;
};

export type UpdateDepartmentDto = {
  name: string;
  description?: string;
  headOfDepartmentId?: string;
};

// Department API
export const departmentApi = {
  getAll: async (searchTerm?: string) => {
    const response = await api.get<DepartmentListItem[]>('/departments', {
      params: searchTerm ? { searchTerm } : undefined,
    });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Department>(`/departments/${id}`);
    return response.data;
  },

  create: async (data: CreateDepartmentDto) => {
    const response = await api.post<Department>('/departments', data);
    return response.data;
  },

  update: async (id: string, data: UpdateDepartmentDto) => {
    const response = await api.put<Department>(`/departments/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/departments/${id}`);
  },
};

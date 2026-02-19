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
    console.log('API Request:', {
      baseURL: config.baseURL,
      url: config.url,
      fullURL: `${config.baseURL}${config.url}`,
      hasAuth: !!token,
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
  parentName?: string;
  parentPhone?: string;
  enrollmentDate: string;
  enrollmentNumber: string;
  isActive: boolean;
};

export type CreateStudentDto = {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  dateOfBirth: string;
  parentName?: string;
  parentPhone?: string;
  enrollmentDate: string;
};

export type UpdateStudentDto = {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  dateOfBirth: string;
  parentName?: string;
  parentPhone?: string;
  enrollmentDate: string;
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
};

// Teacher Types
export type Teacher = {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  qualification?: string;
  experienceYears: number;
  joiningDate: string;
  isActive: boolean;
};

export type CreateTeacherDto = {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  qualification?: string;
  experienceYears: number;
  joiningDate: string;
};

export type UpdateTeacherDto = {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  qualification?: string;
  experienceYears: number;
  joiningDate: string;
  isActive: boolean;
};

export type PaginatedTeacherList = {
  items: Teacher[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
};

export type TeacherAssignment = {
  id: string;
  teacherId: string;
  classId: string;
  subjectId: string;
  assignmentDate: string;
  removalDate?: string;
  className?: string;
  subjectName?: string;
  subjectCode?: string;
  isActive: boolean;
};

export type CreateTeacherAssignmentDto = {
  classId: string;
  subjectId: string;
  assignmentDate?: string;
};

export const teacherApi = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; searchTerm?: string; isActive?: boolean }) => {
    const response = await api.get<PaginatedTeacherList>('/teachers', { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Teacher>(`/teachers/${id}`);
    return response.data;
  },

  create: async (data: CreateTeacherDto) => {
    const response = await api.post<Teacher>('/teachers', data);
    return response.data;
  },

  update: async (id: string, data: UpdateTeacherDto) => {
    const response = await api.put<Teacher>(`/teachers/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/teachers/${id}`);
  },

  activate: async (id: string) => {
    const response = await api.patch<Teacher>(`/teachers/${id}/activate`);
    return response.data;
  },

  deactivate: async (id: string) => {
    const response = await api.patch<Teacher>(`/teachers/${id}/deactivate`);
    return response.data;
  },

  // Teacher Assignment APIs
  getAssignments: async (teacherId: string, activeOnly?: boolean) => {
    const response = await api.get<TeacherAssignment[]>(`/teachers/${teacherId}/assignments`, {
      params: { activeOnly }
    });
    return response.data;
  },

  createAssignment: async (teacherId: string, data: CreateTeacherAssignmentDto) => {
    const response = await api.post<TeacherAssignment>(`/teachers/${teacherId}/assignments`, data);
    return response.data;
  },

  removeAssignment: async (teacherId: string, assignmentId: string, removalDate?: string) => {
    await api.delete(`/teachers/${teacherId}/assignments/${assignmentId}`, {
      data: { assignmentId, removalDate }
    });
  },
};

// Fee Types
export type FeeStructure = {
  id: string;
  name: string;
  academicYear: string;
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
  academicYear: string;
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

export type TeacherAttendance = {
  id: string;
  teacherId: string;
  teacherName?: string;
  teacherEmail?: string;
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

export type CreateTeacherAttendanceDto = {
  teacherId: string;
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

export type PaginatedTeacherAttendanceList = {
  items: TeacherAttendance[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
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

  // Teacher Attendance
  getAllTeacherAttendance: async (params?: {
    pageNumber?: number;
    pageSize?: number;
    teacherId?: string;
    attendanceDate?: string;
    startDate?: string;
    endDate?: string;
    status?: string;
  }) => {
    const response = await api.get<PaginatedTeacherAttendanceList>('/attendance/teachers/history', { params });
    return response.data;
  },

  getTeacherAttendanceById: async (id: string) => {
    const response = await api.get<TeacherAttendance>(`/attendance/teachers/${id}`);
    return response.data;
  },

  recordTeacherAttendance: async (data: CreateTeacherAttendanceDto) => {
    const response = await api.post<TeacherAttendance>('/attendance/teachers', data);
    return response.data;
  },

  updateTeacherAttendance: async (id: string, data: Partial<CreateTeacherAttendanceDto> & { id: string }) => {
    const response = await api.put<TeacherAttendance>(`/attendance/teachers/${id}`, data);
    return response.data;
  },

  deleteTeacherAttendance: async (id: string) => {
    await api.delete(`/attendance/teachers/${id}`);
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
  sectionId: string;
  sectionName: string;
  className: string;
  joinedDate: Date;
  leftDate?: Date;
  isCurrent: boolean;
};

export type StudentSectionHistoryDto = {
  items: StudentSection[];
  totalCount: number;
};

// Class API Client
export const classApi = {
  getAll: async (params?: { pageNumber?: number; pageSize?: number; searchTerm?: string; isActive?: boolean }) => {
    const response = await api.get<PaginatedClassListDto>('/classes', { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Class>(`/classes/${id}`);
    return response.data;
  },

  create: async (data: { name: string; academicYear?: string }) => {
    const response = await api.post<Class>('/classes', data);
    return response.data;
  },

  update: async (id: string, data: { name: string; academicYear?: string; isActive: boolean }) => {
    const response = await api.put<Class>(`/classes/${id}`, data);
    return response.data;
  },

  delete: async (id: string) => {
    await api.delete(`/classes/${id}`);
  },

  getSectionsByClass: async (classId: string) => {
    const response = await api.get<SectionListDto[]>(`/classes/${classId}/sections`);
    return response.data;
  },

  getSectionById: async (id: string) => {
    const response = await api.get<Section>(`/classes/sections/${id}`);
    return response.data;
  },

  createSection: async (data: { classId: string; sectionName: string }) => {
    const response = await api.post<Section>('/classes/sections', data);
    return response.data;
  },

  updateSection: async (id: string, data: { sectionName: string; isActive: boolean }) => {
    const response = await api.put<Section>(`/classes/sections/${id}`, data);
    return response.data;
  },

  deleteSection: async (id: string) => {
    await api.delete(`/classes/sections/${id}`);
  },

  getStudentSectionHistory: async (studentId: string) => {
    const response = await api.get<StudentSectionHistoryDto>(`/classes/students/${studentId}/section-history`);
    return response.data;
  },

  getStudentCurrentSection: async (studentId: string) => {
    const response = await api.get<StudentSection>(`/classes/students/${studentId}/current-section`);
    return response.data;
  },

  moveStudentToSection: async (studentId: string, newSectionId: string) => {
    const response = await api.post<StudentSection>(`/classes/students/${studentId}/move-section`, {
      newSectionId,
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
  academicYear: string; // Format: YYYY-YYYY
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
  academicYear: string; // Format: YYYY-YYYY
};

export type UpdateHolidayDto = {
  name: string;
  holidayDate: string;
  description?: string;
  type?: string;
  academicYear: string;
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
    academicYear?: string;
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

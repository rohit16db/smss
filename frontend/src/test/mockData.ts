import type { Teacher, Student, FeeStructure, StudentFee, FeePayment } from '../services/api';

export const mockTeachers: Teacher[] = [
  {
    id: '1',
    userId: 'user-1',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@school.com',
    phone: '+1234567890',
    qualification: 'M.Ed in Mathematics',
    experienceYears: 5,
    joiningDate: '2020-01-15',
    isActive: true,
    createdAt: '2020-01-15T00:00:00Z',
    updatedAt: '2020-01-15T00:00:00Z',
  },
  {
    id: '2',
    userId: 'user-2',
    firstName: 'Jane',
    lastName: 'Smith',
    email: 'jane.smith@school.com',
    phone: '+1234567891',
    qualification: 'B.Sc in Physics',
    experienceYears: 3,
    joiningDate: '2021-08-01',
    isActive: true,
    createdAt: '2021-08-01T00:00:00Z',
    updatedAt: '2021-08-01T00:00:00Z',
  },
];

export const mockStudents: Student[] = [
  {
    id: '1',
    userId: 'user-3',
    firstName: 'Alice',
    lastName: 'Johnson',
    email: 'alice.j@school.com',
    phone: '+1234567892',
    dateOfBirth: '2010-05-15',
    parentName: 'Robert Johnson',
    parentPhone: '+1234567893',
    parentEmail: 'robert.j@email.com',
    enrollmentDate: '2022-09-01',
    isActive: true,
    createdAt: '2022-09-01T00:00:00Z',
    updatedAt: '2022-09-01T00:00:00Z',
  },
  {
    id: '2',
    userId: 'user-4',
    firstName: 'Bob',
    lastName: 'Williams',
    email: 'bob.w@school.com',
    phone: '+1234567894',
    dateOfBirth: '2011-03-22',
    parentName: 'Sarah Williams',
    parentPhone: '+1234567895',
    parentEmail: 'sarah.w@email.com',
    enrollmentDate: '2023-01-10',
    isActive: true,
    createdAt: '2023-01-10T00:00:00Z',
    updatedAt: '2023-01-10T00:00:00Z',
  },
];

export const mockFeeStructures: FeeStructure[] = [
  {
    id: '1',
    name: 'Annual Tuition Fee',
    description: 'Standard annual tuition fee',
    totalAmount: 5000,
    frequency: 'Annual',
    academicYear: '2023-2024',
    categories: ['Tuition', 'Books'],
    isActive: true,
    createdAt: '2023-01-01T00:00:00Z',
    updatedAt: '2023-01-01T00:00:00Z',
  },
  {
    id: '2',
    name: 'Monthly Tuition Fee',
    description: 'Monthly payment plan',
    totalAmount: 500,
    frequency: 'Monthly',
    academicYear: '2023-2024',
    categories: ['Tuition'],
    isActive: true,
    createdAt: '2023-01-01T00:00:00Z',
    updatedAt: '2023-01-01T00:00:00Z',
  },
];

export const mockStudentFees: StudentFee[] = [
  {
    id: '1',
    studentId: '1',
    studentName: 'Alice Johnson',
    enrollmentNumber: 'ENR-001',
    feeStructureId: '1',
    feeStructureName: 'Annual Tuition Fee',
    startDate: '2023-09-01',
    endDate: undefined,
    totalAmount: 5000,
    paidAmount: 2500,
    balanceAmount: 2500,
    isActive: true,
    sectionId: 'section-123',
    sectionName: 'Class 1-A',
  },
  {
    id: '2',
    studentId: '2',
    studentName: 'Bob Smith',
    enrollmentNumber: 'ENR-002',
    feeStructureId: '1',
    feeStructureName: 'Annual Tuition Fee',
    startDate: '2023-09-01',
    endDate: undefined,
    totalAmount: 5000,
    paidAmount: 0,
    balanceAmount: 5000,
    isActive: true,
    sectionId: 'section-456',
    sectionName: 'Class 2-B',
  },
];

export const mockFeePayments: FeePayment[] = [
  {
    id: '1',
    studentFeeId: '1',
    studentId: '1',
    studentName: 'Alice Johnson',
    amount: 2500,
    paymentDate: '2023-09-15',
    paymentMethod: 'Credit Card',
    receiptNumber: 'RCP-2023-001',
    remarks: 'First installment',
    createdAt: '2023-09-15T00:00:00Z',
  },
];

export const mockPaginatedTeachers = {
  items: mockTeachers,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 2,
  totalPages: 1,
};

export const mockPaginatedStudents = {
  items: mockStudents,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 2,
  totalPages: 1,
};

export const mockPaginatedFeeStructures = {
  items: mockFeeStructures,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 2,
  totalPages: 1,
};

export const mockPaginatedStudentFees = {
  items: mockStudentFees,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 2,
  totalPages: 1,
};

export const mockPaginatedFeePayments = {
  items: mockFeePayments,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 1,
  totalPages: 1,
};

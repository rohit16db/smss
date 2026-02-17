export interface SalaryStructureDto {
  id: string;
  name: string;
  description?: string;
  baseSalary: number;
  hra: number;
  da: number;
  medicalAllowance: number;
  conveyanceAllowance: number;
  otherAllowances: number;
  standardDeduction: number;
  grossSalary: number;
  totalAllowances: number;
  minExperienceYears: number;
  applicableQualifications?: string;
  isActive: boolean;
  effectiveFromDate: string;
  effectiveToDate?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateSalaryStructureDto {
  name: string;
  description?: string;
  baseSalary: number;
  hra?: number;
  da?: number;
  medicalAllowance?: number;
  conveyanceAllowance?: number;
  otherAllowances?: number;
  standardDeduction?: number;
  minExperienceYears?: number;
  applicableQualifications?: string;
  effectiveFromDate: string;
  effectiveToDate?: string;
}

export interface UpdateSalaryStructureDto extends CreateSalaryStructureDto {
  id: string;
}

export interface TeacherSalaryAssignmentDto {
  teacherId: string;
  teacherName: string;
  teacherEmail: string;
  salaryStructureId: string;
  salaryStructureName: string;
  grossSalary: number;
  effectiveDate: string;
  assignedAt: string;
}

export interface AssignSalaryStructureDto {
  teacherId: string;
  salaryStructureId: string;
  effectiveDate: string;
}

export interface BulkCreateFromStructureDto {
  periodStartDate: string;
  periodEndDate: string;
  fixedDeductions?: number;
}

import { api } from './api';

export interface Vehicle {
  id: string;
  registrationNumber: string;
  model: string;
  capacity: number;
  driverName: string;
  driverPhone: string;
  isActive: boolean;
}

export interface RouteStop {
  id: string;
  stopName: string;
  pickupTime: string;
  dropoffTime: string;
  sequence: number;
}

export interface TransportRoute {
  id: string;
  routeName: string;
  description: string;
  vehicleId?: string;
  vehicleRegistrationNumber?: string;
  monthlyFee: number;
  isActive: boolean;
  stops: RouteStop[];
}

export interface AssignTransportDto {
  enrollmentId: string;
  routeId: string;
  routeStopId?: string;
  effectiveDate: string;
}

export interface CreateVehicleDto {
  registrationNumber: string;
  model: string;
  capacity: number;
  driverName: string;
  driverPhone: string;
  isActive: boolean;
}

export interface RouteStopDto {
  id?: string;
  stopName: string;
  pickupTime: string;
  dropoffTime: string;
  sequence: number;
}

export interface CreateRouteDto {
  routeName: string;
  description: string;
  vehicleId?: string;
  monthlyFee: number;
  isActive: boolean;
  stops: RouteStopDto[];
}

export interface UpdateRouteDto extends CreateRouteDto {
  id: string;
}

export interface StudentTransportAssignment {
  id: string;
  studentName: string;
  enrollmentNumber: string;
  classSection: string;
  routeName: string;
  stopName: string;
  vehicleReg: string;
  monthlyFee: number;
  effectiveDate: string;
  guardianPhone?: string;
  isActive: boolean;
}

export const transportService = {
  getVehicles: async () => {
    const response = await api.get<Vehicle[]>('/transport/vehicles');
    return response.data;
  },

  addVehicle: async (vehicle: Omit<Vehicle, 'id'>) => {
    const response = await api.post<string>('/transport/vehicles', vehicle);
    return response.data;
  },

  updateVehicle: async (id: string, vehicle: Partial<Vehicle>) => {
    const response = await api.put<boolean>(`/transport/vehicles/${id}`, vehicle);
    return response.data;
  },

  deleteVehicle: async (id: string) => {
    const response = await api.delete<boolean>(`/transport/vehicles/${id}`);
    return response.data;
  },

  getRoutes: async () => {
    const response = await api.get<TransportRoute[]>('/transport/routes');
    return response.data;
  },

  addRoute: async (route: Omit<TransportRoute, 'id' | 'vehicleRegistrationNumber'>) => {
    const response = await api.post<string>('/transport/routes', route);
    return response.data;
  },

  updateRoute: async (id: string, route: Partial<TransportRoute>) => {
    const response = await api.put<boolean>(`/transport/routes/${id}`, route);
    return response.data;
  },

  deleteRoute: async (id: string) => {
    const response = await api.delete<boolean>(`/transport/routes/${id}`);
    return response.data;
  },

  assignStudent: async (data: { enrollmentId: string; routeId: string; routeStopId?: string; effectiveDate: string }) => {
    const response = await api.post<string>('/transport/assign', data);
    return response.data;
  },

  getAssignments: async (activeOnly = true) => {
    const response = await api.get<StudentTransportAssignment[]>(`/transport/assignments?activeOnly=${activeOnly}`);
    return response.data;
  },

  deactivateAssignment: async (id: string) => {
    const response = await api.delete<boolean>(`/transport/assignments/${id}`);
    return response.data;
  },

  syncFees: async (data: { enrollmentId?: string; allStudents: boolean }) => {
    const response = await api.post<boolean>('/transport/sync-fees', data);
    return response.data;
  },

  getStudentStatus: async (enrollmentId: string) => {
    const response = await api.get<StudentTransportAssignment | null>(`/transport/student/${enrollmentId}`);
    return response.data;
  }
};

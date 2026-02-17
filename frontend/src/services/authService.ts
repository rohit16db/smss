import { api } from './api';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken?: string;
  expiresAt?: string;
  user?: {
    id: string;
    username: string;
    email: string;
    firstName?: string;
    lastName?: string;
    role: string | number;
    isActive?: boolean;
  };
}

export const authService = {
  login: async (credentials: LoginRequest): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/login', credentials);
    return response.data;
  },

  register: async (userData: any): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/register', userData);
    return response.data;
  },

  logout: () => {
    localStorage.removeItem('authToken');
  },
};

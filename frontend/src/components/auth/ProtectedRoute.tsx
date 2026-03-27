import type { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';

interface ProtectedRouteProps {
  children: ReactNode;
  allowedRoles?: string[];
}

const roleMap: Record<number, string> = {
  1: 'Admin',
  2: 'Accountant',
  3: 'Clerk',
  4: 'Staff',
};

const normalizeRole = (role?: string | number) => {
  if (typeof role === 'number') {
    return roleMap[role] || '';
  }

  return role || '';
};

export const ProtectedRoute = ({ children, allowedRoles }: ProtectedRouteProps) => {
  const token = localStorage.getItem('authToken');

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && allowedRoles.length > 0) {
    const rawUser = localStorage.getItem('user');
    let parsedUser: { role?: string | number } | null = null;

    if (rawUser) {
      try {
        parsedUser = JSON.parse(rawUser) as { role?: string | number };
      } catch {
        parsedUser = null;
      }
    }

    const roleName = normalizeRole(parsedUser?.role).toLowerCase();
    const isAllowed = allowedRoles.some((role) => role.toLowerCase() === roleName);

    if (!isAllowed) {
      return <Navigate to="/" replace />;
    }
  }

  return <>{children}</>;
};

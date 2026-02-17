# SMS Frontend - React + TypeScript + Vite

The administrative dashboard for the School Management System, built with React 19, TypeScript, and Material UI.

## 📋 Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Integration Patterns](#api-integration-patterns)
- [Development Guidelines](#development-guidelines)
- [Troubleshooting](#troubleshooting)

## 🎯 Overview

This is the frontend application for the School Management System (SMS). It provides an intuitive administrative interface for managing students, courses, staff, attendance, and other school operations.

**Key Features:**
- Material UI-based responsive design
- Type-safe API integration with React Query
- Automatic token-based authentication
- Real-time system health monitoring (auto-refresh every 30s)
- Modular Clean Architecture-inspired structure

## 🛠 Tech Stack

| Technology | Version | Purpose |
|-----------|---------|---------|
| React | 19.2.0 | UI framework |
| TypeScript | 5.9.3 | Type safety |
| Vite | 7.3.1 | Build tool & dev server |
| Material UI | v5 | Component library |
| React Router | latest | Client-side routing |
| React Query (TanStack Query) | v5 | Server state management |
| Axios | latest | HTTP client |
| ESLint | 9.39.1 | Code linting |

## 🚀 Getting Started

### Prerequisites

- **Node.js 20 LTS** or higher
- **npm** (comes with Node.js)
- **Backend API** running on http://localhost:5208

### Installation

```powershell
cd frontend
npm install
```

### Development Server

```powershell
npm run dev
```

The application will be available at **http://localhost:5173**

### Environment Configuration

Environment variables are configured in `.env.development`:

```bash
VITE_API_URL=http://localhost:5208
VITE_APP_TITLE=School Management System
```

### Verify Integration

Open http://localhost:5173 and check the "System Status" card:
- ✅ Green "Healthy" status chip
- ✅ Service name and version displayed
- ✅ Timestamp updates every 30 seconds
- ✅ No errors in browser console

## 📁 Project Structure

```
frontend/
├── src/
│   ├── components/          # Reusable UI components
│   │   ├── common/         # Generic components (buttons, forms, etc.)
│   │   └── layout/         # Layout components (Header, MainLayout)
│   ├── pages/              # Route-level page components
│   │   └── HomePage.tsx    # Dashboard with system status
│   ├── services/           # API integration layer
│   │   ├── api/           # API client and endpoint functions
│   │   │   ├── apiClient.ts    # Axios instance with interceptors
│   │   │   └── healthApi.ts    # Health check API functions
│   │   └── queries/       # React Query hooks
│   │       └── useHealth.ts    # Health check query hook
│   ├── theme/             # Material UI theming
│   │   └── theme.ts       # Theme configuration (colors, typography)
│   ├── App.tsx            # Root component with routing
│   ├── main.tsx           # Application entry point
│   └── vite-env.d.ts      # TypeScript environment declarations
├── public/                # Static assets
├── .env.development       # Development environment variables
├── index.html            # HTML entry point
├── package.json          # Dependencies and scripts
├── tsconfig.json         # TypeScript configuration
└── vite.config.ts        # Vite configuration
```

## 🔌 API Integration Patterns

### Pattern 1: Axios Client Configuration

**Location:** `src/services/api/apiClient.ts`

All API requests go through a centralized Axios instance:

```typescript
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5208';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000, // 10 seconds
  headers: {
    'Content-Type': 'application/json',
  },
});
```

**Benefits:**
- Single source of truth for base URL
- Consistent timeout across all requests
- Easy to modify headers globally

### Pattern 2: Request Interceptor (Authentication)

The request interceptor automatically adds authentication tokens:

```typescript
apiClient.interceptors.request.use(
  (config) => {
    // Inject JWT token if available
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);
```

**When to use:**
- Token stored in localStorage after login
- All authenticated requests automatically include Authorization header
- No need to manually add token to each request

### Pattern 3: Response Interceptor (Error Handling)

The response interceptor handles global error scenarios:

```typescript
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Unauthorized - clear token and redirect to login
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

**Error handling strategy:**
- **401 Unauthorized:** Auto-redirect to login, clear invalid token
- **Network errors:** Caught by React Query, displayed to user
- **500 Server errors:** Logged and displayed as user-friendly messages

### Pattern 4: API Function Layer

Create typed API functions for each endpoint:

```typescript
// src/services/api/healthApi.ts
import { apiClient } from './apiClient';

export interface HealthStatus {
  status: string;
  timestamp: string;
  service: string;
  version: string;
}

export const healthApi = {
  getHealth: async (): Promise<HealthStatus> => {
    const response = await apiClient.get<HealthStatus>('/health');
    return response.data;
  },
  
  getReadiness: async (): Promise<void> => {
    await apiClient.get('/health/ready');
  },
  
  getLiveness: async (): Promise<void> => {
    await apiClient.get('/health/live');
  },
};
```

**Benefits:**
- Type safety with TypeScript interfaces
- Reusable API functions across components
- Easy to mock for testing
- Centralized endpoint definitions

### Pattern 5: React Query Hooks

Wrap API functions in React Query hooks for state management:

```typescript
// src/services/queries/useHealth.ts
import { useQuery } from '@tanstack/react-query';
import { healthApi } from '../api/healthApi';

export const useHealth = () => {
  return useQuery({
    queryKey: ['health'], // Cache key
    queryFn: healthApi.getHealth, // Fetch function
    refetchInterval: 30000, // Auto-refresh every 30 seconds
  });
};
```

**Benefits:**
- Automatic caching and deduplication
- Built-in loading and error states
- Automatic retries on failure
- Background refetching

### Pattern 6: Component Integration

Use the React Query hook in components:

```typescript
// src/pages/HomePage.tsx
import { useHealth } from '../services/queries/useHealth';

const HomePage = () => {
  const { data: health, isLoading, error } = useHealth();
  
  // Loading state
  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center">
        <CircularProgress />
      </Box>
    );
  }
  
  // Error state
  if (error) {
    return (
      <Typography color="error">
        Unable to connect to backend API
      </Typography>
    );
  }
  
  // Success state
  return (
    <Card>
      <Chip label={health.status} color="success" />
      <Typography>{health.service}</Typography>
      <Typography variant="caption">{health.version}</Typography>
    </Card>
  );
};
```

**State handling:**
- **isLoading:** Show spinner or skeleton
- **error:** Display user-friendly error message
- **data:** Render the actual content

### Pattern 7: Creating New Endpoints

**Step 1:** Define TypeScript interface

```typescript
// src/services/api/studentApi.ts
export interface Student {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  enrollmentDate: string;
}

export interface CreateStudentRequest {
  firstName: string;
  lastName: string;
  email: string;
}
```

**Step 2:** Create API functions

```typescript
export const studentApi = {
  getStudents: async (): Promise<Student[]> => {
    const response = await apiClient.get<Student[]>('/students');
    return response.data;
  },
  
  getStudentById: async (id: string): Promise<Student> => {
    const response = await apiClient.get<Student>(`/students/${id}`);
    return response.data;
  },
  
  createStudent: async (data: CreateStudentRequest): Promise<Student> => {
    const response = await apiClient.post<Student>('/students', data);
    return response.data;
  },
  
  updateStudent: async (id: string, data: Partial<Student>): Promise<Student> => {
    const response = await apiClient.put<Student>(`/students/${id}`, data);
    return response.data;
  },
  
  deleteStudent: async (id: string): Promise<void> => {
    await apiClient.delete(`/students/${id}`);
  },
};
```

**Step 3:** Create React Query hooks

```typescript
// src/services/queries/useStudents.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { studentApi } from '../api/studentApi';

// Query: Fetch all students
export const useStudents = () => {
  return useQuery({
    queryKey: ['students'],
    queryFn: studentApi.getStudents,
  });
};

// Query: Fetch single student
export const useStudent = (id: string) => {
  return useQuery({
    queryKey: ['students', id],
    queryFn: () => studentApi.getStudentById(id),
    enabled: !!id, // Only fetch if ID is provided
  });
};

// Mutation: Create student
export const useCreateStudent = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: studentApi.createStudent,
    onSuccess: () => {
      // Invalidate cache to trigger refetch
      queryClient.invalidateQueries({ queryKey: ['students'] });
    },
  });
};

// Mutation: Update student
export const useUpdateStudent = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Student> }) =>
      studentApi.updateStudent(id, data),
    onSuccess: (_, variables) => {
      // Invalidate both list and detail caches
      queryClient.invalidateQueries({ queryKey: ['students'] });
      queryClient.invalidateQueries({ queryKey: ['students', variables.id] });
    },
  });
};

// Mutation: Delete student
export const useDeleteStudent = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: studentApi.deleteStudent,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
    },
  });
};
```

**Step 4:** Use in component

```typescript
// src/pages/StudentsPage.tsx
const StudentsPage = () => {
  const { data: students, isLoading } = useStudents();
  const createStudent = useCreateStudent();
  
  const handleCreate = async () => {
    try {
      await createStudent.mutateAsync({
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
      });
      alert('Student created successfully!');
    } catch (error) {
      alert('Failed to create student');
    }
  };
  
  if (isLoading) return <CircularProgress />;
  
  return (
    <div>
      <Button onClick={handleCreate}>Create Student</Button>
      <List>
        {students?.map(student => (
          <ListItem key={student.id}>{student.firstName}</ListItem>
        ))}
      </List>
    </div>
  );
};
```

### Pattern 8: Optimistic Updates

For better UX, update UI immediately before server confirms:

```typescript
export const useUpdateStudent = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<Student> }) =>
      studentApi.updateStudent(id, data),
    
    // Optimistically update cache before API call
    onMutate: async ({ id, data }) => {
      // Cancel ongoing queries
      await queryClient.cancelQueries({ queryKey: ['students', id] });
      
      // Get current data
      const previousStudent = queryClient.getQueryData(['students', id]);
      
      // Optimistically update cache
      queryClient.setQueryData(['students', id], (old: Student) => ({
        ...old,
        ...data,
      }));
      
      // Return context with previous value
      return { previousStudent };
    },
    
    // Rollback on error
    onError: (err, variables, context) => {
      if (context?.previousStudent) {
        queryClient.setQueryData(
          ['students', variables.id],
          context.previousStudent
        );
      }
    },
    
    // Refetch after mutation
    onSettled: (_, __, variables) => {
      queryClient.invalidateQueries({ queryKey: ['students', variables.id] });
    },
  });
};
```

### Pattern 9: Pagination and Infinite Scrolling

```typescript
export const useStudentsPaginated = (page: number, pageSize: number) => {
  return useQuery({
    queryKey: ['students', 'paginated', page, pageSize],
    queryFn: () => studentApi.getStudentsPaginated(page, pageSize),
    keepPreviousData: true, // Keep old data while fetching new
  });
};

// Infinite scroll variant
export const useStudentsInfinite = () => {
  return useInfiniteQuery({
    queryKey: ['students', 'infinite'],
    queryFn: ({ pageParam = 1 }) => 
      studentApi.getStudentsPaginated(pageParam, 20),
    getNextPageParam: (lastPage, pages) => {
      return lastPage.hasMore ? pages.length + 1 : undefined;
    },
  });
};
```

### Pattern 10: Error Boundaries

Catch React errors and display fallback UI:

```typescript
// src/components/common/ErrorBoundary.tsx
import { Component, ReactNode } from 'react';
import { Alert, Button, Box } from '@mui/material';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }
  
  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }
  
  handleReset = () => {
    this.setState({ hasError: false, error: undefined });
  };
  
  render() {
    if (this.state.hasError) {
      return (
        <Box p={4}>
          <Alert severity="error">
            <strong>Something went wrong!</strong>
            <p>{this.state.error?.message}</p>
            <Button onClick={this.handleReset}>Try Again</Button>
          </Alert>
        </Box>
      );
    }
    
    return this.props.children;
  }
}
```

## 📝 Development Guidelines

### Code Style

- Follow TypeScript ESLint rules
- Use functional components with hooks
- Prefer named exports for components
- Use PascalCase for component names
- Use camelCase for variables and functions

### State Management

- **Server State:** Use React Query for API data
- **Client State:** Use React hooks (useState, useReducer)
- **Global State:** Context API or Zustand (if needed)
- **Forms:** React Hook Form with Zod validation

### Error Handling

- Use React Query's error states for API errors
- Implement error boundaries for component errors
- Show user-friendly error messages (not stack traces)
- Log errors to console for debugging

### Performance

- Use React Query caching to minimize API calls
- Implement pagination for large lists
- Use React.memo for expensive components
- Lazy load routes with React.lazy()

## 🐛 Troubleshooting

### Backend Connection Failed

**Symptom:** "Unable to connect to backend API"

**Solutions:**
1. Verify backend is running: http://localhost:5208/health
2. Check VITE_API_URL in `.env.development`
3. Verify CORS configured in backend
4. Check browser console for network errors

### CORS Errors

**Symptom:** "Access-Control-Allow-Origin" error in console

**Solutions:**
1. Verify backend CORS policy includes http://localhost:5173
2. Check backend is using `app.UseCors("Development")`
3. Restart backend API

### Port Already in Use

**Symptom:** "Port 5173 is already in use"

**Solutions:**
```powershell
# Find and kill process using port 5173
Get-NetTCPConnection -LocalPort 5173 | 
  Select-Object -ExpandProperty OwningProcess | 
  ForEach-Object { Stop-Process -Id $_ -Force }

# Or use a different port
npm run dev -- --port 3000
```

### Module Not Found

**Solutions:**
```powershell
# Clear node_modules and reinstall
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json
npm install
```

## 📚 Additional Resources

- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs)
- [Vite Documentation](https://vite.dev)
- [Material UI Documentation](https://mui.com)
- [TanStack Query Documentation](https://tanstack.com/query)
- [Backend API Documentation](../backend/README.md)
- [Integration Testing](../specs/001-project-setup/integration-tests.md)

---

**Need Help?** Check the [main project README](../README.md) or contact the development team.

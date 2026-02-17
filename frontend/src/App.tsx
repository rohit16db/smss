import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { theme } from './theme/theme';
import { MainLayout } from './components/layout/MainLayout';
import { HomePage } from './pages/HomePage';
import { TeachersPage } from './pages/TeachersPage';
import { FeesPage } from './pages/FeesPage';
import { StudentsPage } from './pages/StudentsPage';
import { AttendancePage } from './pages/AttendancePage';
import { PayrollPage } from './pages/PayrollPage';
import { SalaryPage } from './pages/SalaryPage';
import { ClassManagementPage } from './pages/ClassManagementPage';
import { SubjectManagementPage } from './pages/SubjectManagementPage';
import { SalaryStructurePage } from './pages/SalaryStructurePage';
import { TeacherSalaryAssignmentPage } from './pages/TeacherSalaryAssignmentPage';
import { BulkSalaryProcessingPage } from './pages/BulkSalaryProcessingPage';
import { LoginPage } from './pages/LoginPage';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { ToastProvider } from './components/providers/ToastProvider';
import { ErrorBoundary } from './components/common/ErrorBoundary';

// Create React Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    },
  },
});

function App() {
  return (
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <ToastProvider />
          <BrowserRouter>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route
                path="/"
                element={
                  <ProtectedRoute>
                    <MainLayout />
                  </ProtectedRoute>
                }
              >
                <Route index element={<HomePage />} />
                <Route path="teachers" element={<TeachersPage />} />
                <Route path="students" element={<StudentsPage />} />
                <Route path="fees" element={<FeesPage />} />
                <Route path="attendance" element={<AttendancePage />} />
                <Route path="payroll" element={<PayrollPage />} />
                <Route path="salary" element={<SalaryPage />} />
                <Route path="salary-structures" element={<SalaryStructurePage />} />
                <Route path="teacher-salary-assignment" element={<TeacherSalaryAssignmentPage />} />
                <Route path="bulk-salary-processing" element={<BulkSalaryProcessingPage />} />
                <Route path="classes" element={<ClassManagementPage />} />
                <Route path="subjects" element={<SubjectManagementPage />} />
              </Route>
            </Routes>
          </BrowserRouter>
        </ThemeProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}

export default App;


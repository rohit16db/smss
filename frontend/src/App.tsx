import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { theme } from './theme/theme';
import { MainLayout } from './components/layout/MainLayout';
import { HomePage } from './pages/HomePage';
import { TeachersPage } from './pages/TeachersPage';
import { FeesPage } from './pages/FeesPage';
import { FeeReportPage } from './pages/FeeReportPage';
import { FeeReportsPage } from './pages/FeeReportsPage';
import { SalaryReportsPage } from './pages/SalaryReportsPage';
import { OutstandingFeesPage } from './pages/OutstandingFeesPage';
import { TeacherSalaryComparisonPage } from './pages/TeacherSalaryComparisonPage';
import { BudgetVsActualPage } from './pages/BudgetVsActualPage';
import { StudentsPage } from './pages/StudentsPage';
import { AttendancePage } from './pages/AttendancePage';
import { AttendanceReportPage } from './pages/AttendanceReportPage';
import { PayrollPage } from './pages/PayrollPage';
import { SalaryPage } from './pages/SalaryPage';
import { ClassManagementPage } from './pages/ClassManagementPage';
import { SubjectManagementPage } from './pages/SubjectManagementPage';
import { RollNumberManagementPage } from './pages/RollNumberManagementPage';
import { HolidaysPage } from './pages/HolidaysPage';
import { ExamsPage } from './pages/ExamsPage';
import { MarksPage } from './pages/MarksPage';
import { SettingsPage } from './pages/SettingsPage';
import { ReportCardsPage } from './pages/ReportCardsPage';
import { ReportCardDetailPage } from './pages/ReportCardDetailPage';
import { PerformanceAnalyticsPage } from './pages/PerformanceAnalyticsPage';
import { SalaryStructurePage } from './pages/SalaryStructurePage';
import { TeacherSalaryAssignmentPage } from './pages/TeacherSalaryAssignmentPage';
import { BulkSalaryProcessingPage } from './pages/BulkSalaryProcessingPage';
import SalaryPaymentPage from './pages/SalaryPaymentPage';
import { LoginPage } from './pages/LoginPage';
import { ChangePasswordPage } from './pages/ChangePasswordPage';
import { ForgotPasswordPage } from './pages/ForgotPasswordPage';
import { ResetPasswordPage } from './pages/ResetPasswordPage';
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
              <Route path="/forgot-password" element={<ForgotPasswordPage />} />
              <Route path="/reset-password" element={<ResetPasswordPage />} />
              <Route
                path="/"
                element={
                  <ProtectedRoute>
                    <MainLayout />
                  </ProtectedRoute>
                }
              >
                <Route index element={<HomePage />} />
                <Route
                  path="teachers"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk"]}>
                      <TeachersPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="students"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk"]}>
                      <StudentsPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="fees"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <FeesPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="fee-report"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <FeeReportPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="fee-reports"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <FeeReportsPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="outstanding-fees"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <OutstandingFeesPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="attendance"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk", "Teacher"]}>
                      <AttendancePage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="attendance-reports"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk"]}>
                      <AttendanceReportPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="payroll"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <PayrollPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="salary"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant", "Teacher"]}>
                      <SalaryPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="salary-reports"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <SalaryReportsPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="teacher-salary-comparison"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <TeacherSalaryComparisonPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="budget-vs-actual"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <BudgetVsActualPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="salary-structures"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <SalaryStructurePage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="teacher-salary-assignment"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <TeacherSalaryAssignmentPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="bulk-salary-processing"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <BulkSalaryProcessingPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="salary-payments"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Accountant"]}>
                      <SalaryPaymentPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="change-password"
                  element={
                    <ProtectedRoute>
                      <ChangePasswordPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="classes"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk"]}>
                      <ClassManagementPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="subjects"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk"]}>
                      <SubjectManagementPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="exams"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk", "Teacher"]}>
                      <ExamsPage />
                    </ProtectedRoute>
                  }
                >
                  <Route
                    path=":examId/marks"
                    element={
                      <ProtectedRoute allowedRoles={["Admin", "Clerk", "Teacher"]}>
                        <MarksPage />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path=":examId/report-cards"
                    element={
                      <ProtectedRoute allowedRoles={["Admin", "Clerk", "Teacher"]}>
                        <ReportCardsPage />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path=":examId/analytics"
                    element={
                      <ProtectedRoute allowedRoles={["Admin", "Clerk", "Teacher"]}>
                        <PerformanceAnalyticsPage />
                      </ProtectedRoute>
                    }
                  />
                </Route>
                <Route
                  path="report-cards/:examId/:studentId"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk", "Teacher"]}>
                      <ReportCardDetailPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="roll-numbers"
                  element={
                    <ProtectedRoute allowedRoles={["Admin", "Clerk"]}>
                      <RollNumberManagementPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="holidays"
                  element={
                    <ProtectedRoute allowedRoles={["Admin"]}>
                      <HolidaysPage />
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="admin/settings"
                  element={
                    <ProtectedRoute allowedRoles={["Admin"]}>
                      <SettingsPage />
                    </ProtectedRoute>
                  }
                />
              </Route>
            </Routes>
          </BrowserRouter>
        </ThemeProvider>
      </QueryClientProvider>
    </ErrorBoundary>
  );
}

export default App;


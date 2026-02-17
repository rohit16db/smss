import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { studentApi, teacherApi, feeApi } from '../services/api';
import { dashboardApi } from '../services/dashboardService';
import { DashboardSummaryCards } from '../components/dashboard/DashboardSummaryCards';
import { FeesCollectionChart } from '../components/dashboard/FeesCollectionChart';
import { AttendanceTrendChart } from '../components/dashboard/AttendanceTrendChart';

export const HomePage = () => {
  const navigate = useNavigate();

  // Fetch dashboard summary
  const { data: dashboardData, isLoading: isDashboardLoading } = useQuery({
    queryKey: ['dashboard', 'summary'],
    queryFn: () => dashboardApi.getSummary(),
  });

  // Fetch statistics
  const { data: studentsData } = useQuery({
    queryKey: ['students', 1, 1],
    queryFn: () => studentApi.getAll({ pageNumber: 1, pageSize: 1 }),
  });

  const { data: teachersData } = useQuery({
    queryKey: ['teachers', 1, 1],
    queryFn: () => teacherApi.getAll({ pageNumber: 1, pageSize: 1 }),
  });

  const { data: feesData } = useQuery({
    queryKey: ['fees', 1, 1],
    queryFn: () => feeApi.getAllStructures({ pageNumber: 1, pageSize: 1 }),
  });

  const totalStudents = studentsData?.totalCount || 0;
  const totalTeachers = teachersData?.totalCount || 0;
  const totalFeeStructures = feesData?.totalCount || 0;

  const modules = [
    {
      title: 'Teacher Management',
      description: 'Manage teacher records, assignments, and profiles',
      icon: '👨‍🏫',
      path: '/teachers',
      color: 'from-blue-500 to-blue-600',
      stats: { label: 'Active Teachers', value: totalTeachers.toString() }
    },
    {
      title: 'Student Management',
      description: 'Manage student records, enrollments, and information',
      icon: '👨‍🎓',
      path: '/students',
      color: 'from-cyan-500 to-cyan-600',
      stats: { label: 'Total Students', value: totalStudents.toString() }
    },
    {
      title: 'Fee Management',
      description: 'Manage fee structures, payments, and student fees',
      icon: '💰',
      path: '/fees',
      color: 'from-green-500 to-green-600',
      stats: { label: 'Fee Structures', value: totalFeeStructures.toString() }
    },
    {
      title: 'Attendance Tracking',
      description: 'Record and monitor student and teacher attendance',
      icon: '📊',
      path: '/attendance',
      color: 'from-purple-500 to-purple-600',
      stats: { label: 'Today\'s Attendance', value: '0%' }
    },
  ];

  return (
    <div className="min-h-[calc(100vh-64px)] bg-gradient-to-br from-gray-50 to-blue-50">
      {/* Hero Section */}
      <div className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
          <div className="text-center animate-fade-in">
            <h1 className="text-4xl sm:text-5xl font-bold text-gray-900 mb-4">
              Welcome to{' '}
              <span className="bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
                School Management System
              </span>
            </h1>
            <p className="text-lg text-gray-600 max-w-2xl mx-auto">
              Streamline your educational institution's operations with our comprehensive management platform
            </p>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {/* Module Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
          {modules.map((module, index) => (
            <div
              key={module.path}
              onClick={() => navigate(module.path)}
              className="group cursor-pointer animate-slide-up"
              style={{ animationDelay: `${index * 100}ms` }}
            >
              <div className="bg-white rounded-2xl shadow-lg hover:shadow-2xl transition-all duration-300 transform hover:-translate-y-2 overflow-hidden border border-gray-100">
                {/* Gradient Header */}
                <div className={`h-32 bg-gradient-to-r ${module.color} p-6 flex items-center justify-center`}>
                  <span className="text-6xl drop-shadow-lg">{module.icon}</span>
                </div>
                
                {/* Card Content */}
                <div className="p-6">
                  <h3 className="text-xl font-bold text-gray-900 mb-2 group-hover:text-blue-600 transition-colors">
                    {module.title}
                  </h3>
                  <p className="text-gray-600 text-sm mb-4">
                    {module.description}
                  </p>
                  
                  {/* Stats */}
                  <div className="flex items-center justify-between pt-4 border-t border-gray-100">
                    <div>
                      <p className="text-xs text-gray-500">{module.stats.label}</p>
                      <p className="text-2xl font-bold text-gray-900">{module.stats.value}</p>
                    </div>
                    <svg
                      className="w-6 h-6 text-gray-400 group-hover:text-blue-600 group-hover:translate-x-1 transition-all"
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                    </svg>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Quick Actions */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* System Status */}
          <div className="card animate-fade-in">
            <div className="flex items-center mb-4">
              <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center mr-4">
                <svg className="w-6 h-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900">System Status</h3>
                <p className="text-sm text-gray-500">All systems operational</p>
              </div>
            </div>
            <div className="flex items-center">
              <div className="flex-1 bg-gray-100 rounded-full h-2 mr-4">
                <div className="bg-green-500 h-2 rounded-full" style={{ width: '100%' }}></div>
              </div>
              <span className="text-sm font-medium text-green-600">100%</span>
            </div>
          </div>

          {/* Quick Stats */}
          <div className="card animate-fade-in" style={{ animationDelay: '100ms' }}>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Quick Overview</h3>
            <div className="space-y-3">
              <div className="flex items-center justify-between p-3 bg-blue-50 rounded-lg">
                <span className="text-sm font-medium text-gray-700">Total Students</span>
                <span className="text-lg font-bold text-blue-600">{dashboardData?.academicSummary?.totalStudents || 0}</span>
              </div>
              <div className="flex items-center justify-between p-3 bg-green-50 rounded-lg">
                <span className="text-sm font-medium text-gray-700">Total Teachers</span>
                <span className="text-lg font-bold text-green-600">{dashboardData?.academicSummary?.activeTeachers || 0}</span>
              </div>
              <div className="flex items-center justify-between p-3 bg-purple-50 rounded-lg">
                <span className="text-sm font-medium text-gray-700">Collection Rate</span>
                <span className="text-lg font-bold text-purple-600">{dashboardData?.financialSummary?.collectionPercentage.toFixed(1) || 0}%</span>
              </div>
            </div>
          </div>
        </div>

        {/* Dashboard Summary Section */}
        {dashboardData && (
          <>
            {/* Summary Cards */}
            <div className="mt-12">
              <h2 className="text-2xl font-bold text-gray-900 mb-6">Dashboard Overview</h2>
              <DashboardSummaryCards cards={dashboardData.summaryCards} isLoading={isDashboardLoading} />
            </div>

            {/* Charts Section */}
            <div className="mt-12 grid grid-cols-1 lg:grid-cols-2 gap-6">
              <FeesCollectionChart data={dashboardData.financialSummary} isLoading={isDashboardLoading} />
              <AttendanceTrendChart data={dashboardData.attendanceSummary} isLoading={isDashboardLoading} />
            </div>
          </>
        )}
      </div>
    </div>
  );
};


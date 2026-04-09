import { useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { authService } from '../../services/authService';
import { settingsApi, type AcademicYearDto } from '../../services/api';
import toast from 'react-hot-toast';

interface HeaderProps {
  onMenuClick?: () => void;
}

interface StoredUser {
  username?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  role?: string | number;
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

export const Header = ({ onMenuClick }: HeaderProps) => {
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [academicMenuOpen, setAcademicMenuOpen] = useState(false);
  const [financeMenuOpen, setFinanceMenuOpen] = useState(false);
  const [payrollMenuOpen, setPayrollMenuOpen] = useState(false);
  const [reportsMenuOpen, setReportsMenuOpen] = useState(false);
  const [mobileAcademicOpen, setMobileAcademicOpen] = useState(false);
  const [mobileFinanceOpen, setMobileFinanceOpen] = useState(false);
  const [mobilePayrollOpen, setMobilePayrollOpen] = useState(false);
  const [mobileReportsOpen, setMobileReportsOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const [sessionMenuOpen, setSessionMenuOpen] = useState(false);
  const [currentUser, setCurrentUser] = useState<StoredUser | null>(null);
  
  const academicTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const financeTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const payrollTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const reportsTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const userMenuTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const sessionMenuTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  const [academicYears, setAcademicYears] = useState<AcademicYearDto[]>([]);
  const [selectedYearId, setSelectedYearId] = useState<string>(localStorage.getItem('selectedAcademicYearId') || '');

  useEffect(() => {
    const fetchYears = async () => {
      try {
        const years = await settingsApi.getAcademicYears();
        setAcademicYears(years);
        
        if (!localStorage.getItem('selectedAcademicYearId')) {
          const activeYear = years.find(y => y.isActive);
          if (activeYear) {
            setSelectedYearId(activeYear.id);
            localStorage.setItem('selectedAcademicYearId', activeYear.id);
          }
        }
      } catch (error) {
        console.error('Failed to fetch academic years', error);
      }
    };
    fetchYears();
  }, []);

  const handleYearChange = (id: string) => {
    setSelectedYearId(id);
    localStorage.setItem('selectedAcademicYearId', id);
    toast.success('Academic year switched. Reloading...');
    setTimeout(() => {
      window.location.reload();
    }, 500);
  };

  useEffect(() => {
    const readUser = () => {
      const raw = localStorage.getItem('user');
      if (!raw) {
        setCurrentUser(null);
        return;
      }

      try {
        setCurrentUser(JSON.parse(raw) as StoredUser);
      } catch (error) {
        console.error('Failed to parse stored user:', error);
        setCurrentUser(null);
      }
    };

    readUser();
    globalThis.addEventListener('storage', readUser);
    return () => globalThis.removeEventListener('storage', readUser);
  }, []);

  const displayName =
    currentUser?.firstName || currentUser?.lastName
      ? `${currentUser.firstName || ''} ${currentUser.lastName || ''}`.trim()
      : currentUser?.username || currentUser?.email || 'User';
  const roleName = normalizeRole(currentUser?.role);
  const role = roleName.toLowerCase();
  const isAdmin = role === 'admin' || role === '';
  const isAccountant = role === 'accountant';
  const isClerk = role === 'clerk';
  const isStaff = role === 'staff';
  const canViewAcademic = isAdmin || isClerk;
  const canViewFinance = isAdmin || isAccountant || isStaff;
  const canViewFees = isAdmin || isAccountant;
  const canViewSalary = isAdmin || isAccountant || isStaff;
  const canViewPayroll = isAdmin || isAccountant;
  const canManageSalary = isAdmin || isAccountant;
  const canViewAttendance = isAdmin || isClerk || isStaff;
  const canViewAttendanceReports = isAdmin || isClerk;

  const handleAcademicMouseLeave = () => {
    academicTimeoutRef.current = setTimeout(() => {
      setAcademicMenuOpen(false);
    }, 150);
  };

  const handleFinanceMouseLeave = () => {
    financeTimeoutRef.current = setTimeout(() => {
      setFinanceMenuOpen(false);
    }, 150);
  };

  const handleAcademicMouseEnter = () => {
    if (academicTimeoutRef.current) clearTimeout(academicTimeoutRef.current);
    setAcademicMenuOpen(true);
  };

  const handleFinanceMouseEnter = () => {
    if (financeTimeoutRef.current) clearTimeout(financeTimeoutRef.current);
    setFinanceMenuOpen(true);
  };

  const handlePayrollMouseLeave = () => {
    payrollTimeoutRef.current = setTimeout(() => {
      setPayrollMenuOpen(false);
    }, 150);
  };

  const handlePayrollMouseEnter = () => {
    if (payrollTimeoutRef.current) clearTimeout(payrollTimeoutRef.current);
    setPayrollMenuOpen(true);
  };

  const handleReportsMouseLeave = () => {
    reportsTimeoutRef.current = setTimeout(() => {
      setReportsMenuOpen(false);
    }, 150);
  };

  const handleReportsMouseEnter = () => {
    if (reportsTimeoutRef.current) clearTimeout(reportsTimeoutRef.current);
    setReportsMenuOpen(true);
  };

  const handleUserMenuMouseLeave = () => {
    userMenuTimeoutRef.current = setTimeout(() => {
      setUserMenuOpen(false);
    }, 150);
  };

  const handleUserMenuMouseEnter = () => {
    if (userMenuTimeoutRef.current) clearTimeout(userMenuTimeoutRef.current);
    setUserMenuOpen(true);
  };

  const handleSessionMenuMouseLeave = () => {
    sessionMenuTimeoutRef.current = setTimeout(() => {
      setSessionMenuOpen(false);
    }, 150);
  };

  const handleSessionMenuMouseEnter = () => {
    if (sessionMenuTimeoutRef.current) clearTimeout(sessionMenuTimeoutRef.current);
    setSessionMenuOpen(true);
  };

  const handleLogout = async () => {
    try {
      await authService.logout();
      navigate('/login');
    } catch (error) {
      console.error('Logout error:', error);
      // Clear local storage even if API call fails
      localStorage.removeItem('authToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      navigate('/login');
    }
  };
  
  return (
    <header className="bg-gradient-to-r from-blue-600 to-blue-700 shadow-lg sticky top-0 z-50">
      <nav className="px-4 lg:px-12">
        <div className="flex h-16 items-center justify-between gap-2">
          {/* Logo and Brand */}
          <div className="flex items-center">
            <button
              onClick={onMenuClick}
              className="lg:hidden mr-3 p-2 rounded-md text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-white"
            >
              <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
            <button
              className="flex items-center cursor-pointer group hover:opacity-80 transition-opacity"
              onClick={() => navigate('/')}
              type="button"
              title="Go to home"
            >
              <div className="bg-white rounded-lg p-2 mr-3 group-hover:scale-105 transition-transform">
                <svg className="h-6 w-6 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                </svg>
              </div>
              <h1 className="text-xl font-bold text-white hidden sm:block">
                School Management System
              </h1>
              <h1 className="text-xl font-bold text-white sm:hidden">SMS</h1>
            </button>
          </div>

          {/* Desktop Navigation */}
          <div className="hidden lg:flex items-center space-x-1">
            {/* Academic Dropdown */}
            {canViewAcademic && (
              <div
                className="relative"
                onMouseEnter={handleAcademicMouseEnter}
                onMouseLeave={handleAcademicMouseLeave}
              >
                <button className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium flex items-center gap-1">
                  🎓 Academic
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                {academicMenuOpen && (
                  <div className="absolute top-full left-0 mt-1 w-48 bg-white rounded-lg shadow-xl border border-gray-200 py-2 animate-fadeIn">
                    <button
                      onClick={() => navigate('/students')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      👨‍🎓 Students
                    </button>
                    <button
                      onClick={() => navigate('/staff')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      👨‍🏫 Staff Management
                    </button>
                    <button
                      onClick={() => navigate('/departments')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      🏢 Departments
                    </button>
                    <button
                      onClick={() => navigate('/classes')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      📚 Classes
                    </button>
                    <button
                      onClick={() => navigate('/subjects')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      📖 Subjects
                    </button>
                    <button
                      onClick={() => navigate('/exams')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      📝 Exams
                    </button>
                    <button
                      onClick={() => navigate('/timetable')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      📅 Timetable
                    </button>
                    <button
                      onClick={() => navigate('/transport')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      🚌 Transport Management
                    </button>
                    {(isAdmin || isClerk) && (
                      <button
                        onClick={() => navigate('/roll-numbers')}
                        className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                      >
                        🔢 Roll Numbers
                      </button>
                    )}
                    {isAdmin && (
                      <button
                        onClick={() => navigate('/holidays')}
                        className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                      >
                        🏖️ Holidays
                      </button>
                    )}
                    {(isAdmin || isClerk) && (
                      <button
                        onClick={() => navigate('/students/promote')}
                        className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                      >
                        🔄 Student Promotion
                      </button>
                    )}
                  </div>
                )}
              </div>
            )}

            {/* Finance Dropdown */}
            {canViewFinance && (
              <div
                className="relative"
                onMouseEnter={handleFinanceMouseEnter}
                onMouseLeave={handleFinanceMouseLeave}
              >
                <button className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium flex items-center gap-1">
                  💰 Finance
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                {financeMenuOpen && (
                  <div className="absolute top-full left-0 mt-1 w-56 bg-white rounded-lg shadow-xl border border-gray-200 py-2 animate-fadeIn">
                    {canViewFees && (
                      <button
                        onClick={() => navigate('/fees')}
                        className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                      >
                        💰 Fees
                      </button>
                    )}
                    {canViewFees && (
                      <button
                        onClick={() => navigate('/fee-report')}
                        className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                      >
                        📊 Fee Report
                      </button>
                    )}
                    {canViewSalary && (
                      <button
                        onClick={() => navigate('/salary')}
                        className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                      >
                        🧾 Salary
                      </button>
                    )}
                    {canViewPayroll && (
                      <button
                        onClick={() => navigate('/payroll')}
                        className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                      >
                        💼 Payroll
                      </button>
                    )}
                  </div>
                )}
              </div>
            )}

            {/* Payroll Dropdown */}
            {canManageSalary && (
              <div
                className="relative"
                onMouseEnter={handlePayrollMouseEnter}
                onMouseLeave={handlePayrollMouseLeave}
              >
                <button className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium flex items-center gap-1">
                  💼 Payroll
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                {payrollMenuOpen && (
                  <div className="absolute top-full left-0 mt-1 w-56 bg-white rounded-lg shadow-xl border border-gray-200 py-2 animate-fadeIn">
                    <button
                      onClick={() => navigate('/salary-structures')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      📊 Salary Structures
                    </button>
                    <button
                      onClick={() => navigate('/staff-salary-assignment')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      👥 Staff Assignments
                    </button>
                    <button
                      onClick={() => navigate('/bulk-salary-processing')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      🔄 Bulk Processing
                    </button>
                    <hr className="my-1 border-gray-200" />
                    <button
                      onClick={() => navigate('/salary-payments')}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      💰 Payment Management
                    </button>
                  </div>
                )}
              </div>
            )}

            {/* Reports Dropdown */}
            {(canViewFees || canViewAttendanceReports) && (
              <div
                className="relative"
                onMouseEnter={handleReportsMouseEnter}
                onMouseLeave={handleReportsMouseLeave}
              >
                <button className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium flex items-center gap-1">
                  📊 Reports
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>
                {reportsMenuOpen && (
                  <div className="absolute top-full left-0 mt-1 w-56 bg-white rounded-lg shadow-xl border border-gray-200 py-2 animate-fadeIn">
                    {canViewFees && (
                      <>
                        <button
                          onClick={() => navigate('/outstanding-fees')}
                          className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                        >
                          ⚠️ Outstanding Fees
                        </button>
                        <button
                          onClick={() => navigate('/staff-salary-comparison')}
                          className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                        >
                          📈 Salary Comparison
                        </button>
                        <button
                          onClick={() => navigate('/budget-vs-actual')}
                          className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                        >
                          📊 Budget vs Actual
                        </button>
                      </>
                    )}
                    {canViewAttendanceReports && (
                      <>
                        {canViewFees && <hr className="my-1 border-gray-200" />}
                        <button
                          onClick={() => navigate('/attendance-reports')}
                          className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                        >
                          📅 Attendance Reports
                        </button>
                      </>
                    )}
                  </div>
                )}
              </div>
            )}

            {/* Attendance Direct Link */}
            {canViewAttendance && (
              <button
                onClick={() => navigate('/attendance')}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
              >
                📊 Attendance
              </button>
            )}
          </div>

            {/* Academic Year Selector */}
            <div 
              className="relative"
              onMouseEnter={handleSessionMenuMouseEnter}
              onMouseLeave={handleSessionMenuMouseLeave}
            >
              <button className="flex items-center gap-2 bg-white/10 hover:bg-white/20 px-4 py-2 rounded-xl border border-white/20 shadow-lg backdrop-blur-sm transition-all group">
                <div className="flex flex-col items-start">
                  <span className="text-[9px] uppercase font-black text-blue-100 tracking-[0.2em] leading-none mb-1">Current Session</span>
                  <div className="flex items-center gap-2">
                    <span className="text-white text-sm font-bold truncate max-w-[120px]">
                      {academicYears.find(y => y.id === selectedYearId)?.name || 'Select Session'}
                    </span>
                    <svg className={`w-4 h-4 text-blue-200 transition-transform duration-300 ${sessionMenuOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M19 9l-7 7-7-7" />
                    </svg>
                  </div>
                </div>
              </button>
              
              {sessionMenuOpen && (
                <div className="absolute top-full right-0 mt-2 w-64 bg-white rounded-2xl shadow-2xl border border-gray-100 py-3 animate-fadeIn overflow-hidden z-50">
                  <div className="px-4 pb-2 mb-2 border-b border-gray-100">
                    <span className="text-[10px] font-bold text-gray-400 uppercase tracking-widest">Select Academic Year</span>
                  </div>
                  <div className="max-h-64 overflow-y-auto custom-scrollbar">
                    {academicYears.length > 0 ? (
                      academicYears.map((year) => (
                        <button
                          key={year.id}
                          onClick={() => {
                            handleYearChange(year.id);
                            setSessionMenuOpen(false);
                          }}
                          className={`w-full text-left px-4 py-3 flex items-center justify-between transition-all group ${
                            selectedYearId === year.id 
                              ? 'bg-blue-50 text-blue-700' 
                              : 'hover:bg-gray-50 text-gray-700'
                          }`}
                        >
                          <div className="flex flex-col">
                            <span className="font-bold flex items-center gap-2">
                              {year.name}
                              {year.isActive && (
                                <span className="px-1.5 py-0.5 rounded-md bg-green-100 text-green-700 text-[9px] font-black uppercase tracking-tighter">Active</span>
                              )}
                            </span>
                            <span className="text-[10px] text-gray-400 font-medium">
                              {year.isActive ? 'Current primary session' : 'Previous/Future session'}
                            </span>
                          </div>
                          {selectedYearId === year.id && (
                            <div className="w-2 h-2 rounded-full bg-blue-600 shadow-sm shadow-blue-200"></div>
                          )}
                        </button>
                      ))
                    ) : (
                      <div className="px-4 py-3 text-sm text-gray-500 italic">No years found...</div>
                    )}
                  </div>
                </div>
              )}
            </div>

          {/* User Profile Dropdown */}
          <div 
            className="hidden lg:block relative"
            onMouseEnter={handleUserMenuMouseEnter}
            onMouseLeave={handleUserMenuMouseLeave}
          >
            <button className="px-3 py-2 rounded-full text-white hover:bg-blue-700 transition-colors flex items-center gap-2">
              <svg className="h-8 w-8" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
              </svg>
              <span className="hidden md:block text-sm font-semibold text-white">
                {displayName}
              </span>
            </button>
            {userMenuOpen && (
              <div className="absolute right-0 mt-2 w-56 bg-white rounded-lg shadow-xl py-2 z-50">
                <div className="px-4 py-2">
                  <div className="text-sm font-semibold text-gray-900">
                    {displayName}
                  </div>
                  {currentUser?.email && (
                    <div className="text-xs text-gray-500 truncate">
                      {currentUser.email}
                    </div>
                  )}
                </div>
                <hr className="my-1 border-gray-200" />
                {isAdmin && (
                  <>
                    <button
                      onClick={() => {
                        navigate('/admin/academic-years');
                        setUserMenuOpen(false);
                      }}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      📅 Academic Years
                    </button>
                    <button
                      onClick={() => {
                        navigate('/admin/settings');
                        setUserMenuOpen(false);
                      }}
                      className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                    >
                      ⚙️ Settings
                    </button>
                    <hr className="my-1 border-gray-200" />
                  </>
                )}
                <button
                  onClick={() => {
                    navigate('/change-password');
                    setUserMenuOpen(false);
                  }}
                  className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                >
                  🔒 Change Password
                </button>
                <hr className="my-1 border-gray-200" />
                <button
                  onClick={() => {
                    setUserMenuOpen(false);
                    handleLogout();
                  }}
                  className="w-full text-left px-4 py-2 hover:bg-red-50 text-gray-700 hover:text-red-600 transition-colors flex items-center gap-2"
                >
                  🚪 Logout
                </button>
              </div>
            )}
          </div>

          {/* Mobile menu button */}
          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="lg:hidden p-2 rounded-md text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-white"
          >
            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              {mobileMenuOpen ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              )}
            </svg>
          </button>
        </div>

        {/* Mobile Navigation */}
        {mobileMenuOpen && (
          <div className="lg:hidden pb-4 animate-fade-in">
            <div className="flex flex-col space-y-2 px-4 mb-4">
              <div className="bg-blue-800/40 rounded-xl p-3 border border-blue-400/20 shadow-inner">
                <p className="text-[10px] font-black text-blue-200 uppercase tracking-widest mb-2">Current Session</p>
                <div className="flex flex-wrap gap-2">
                  {academicYears.map((year) => (
                    <button
                      key={year.id}
                      onClick={() => handleYearChange(year.id)}
                      className={`px-3 py-1.5 rounded-lg text-xs font-bold transition-all ${
                        selectedYearId === year.id
                          ? 'bg-white text-blue-700 shadow-md'
                          : 'bg-blue-700/50 text-blue-100 hover:bg-blue-700/70'
                      }`}
                    >
                      {year.name}
                      {year.isActive && selectedYearId !== year.id && ' 📍'}
                    </button>
                  ))}
                </div>
              </div>
            </div>
            <div className="flex flex-col space-y-2">
              {/* Academic Section */}
              {canViewAcademic && (
                <div className="border-b border-blue-700 pb-2">
                  <button
                    onClick={() => setMobileAcademicOpen(!mobileAcademicOpen)}
                    className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left flex items-center justify-between"
                  >
                    <span>🎓 Academic</span>
                    <svg
                      className={`w-5 h-5 transition-transform ${mobileAcademicOpen ? 'rotate-180' : ''}`}
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </button>
                  {mobileAcademicOpen && (
                    <div className="mt-2 ml-4 space-y-1 animate-fade-in">
                      <button
                        onClick={() => {
                          navigate('/students');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        👨‍🎓 Students
                      </button>
                      <button
                        onClick={() => {
                          navigate('/staff');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        👨‍🏫 Staff Management
                      </button>
                      <button
                        onClick={() => {
                          navigate('/departments');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        🏢 Departments
                      </button>
                      <button
                        onClick={() => {
                          navigate('/classes');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        📚 Classes
                      </button>
                      <button
                        onClick={() => {
                          navigate('/subjects');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        📖 Subjects
                      </button>
                      <button
                        onClick={() => {
                          navigate('/exams');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        📝 Exams
                      </button>
                      <button
                        onClick={() => {
                          navigate('/timetable');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        📅 Timetable
                      </button>
                      <button
                        onClick={() => {
                          navigate('/transport');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        🚌 Transport Management
                      </button>
                      {(isAdmin || isClerk) && (
                        <button
                          onClick={() => {
                            navigate('/roll-numbers');
                            setMobileMenuOpen(false);
                          }}
                          className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                        >
                          🔢 Roll Numbers
                        </button>
                      )}
                      {isAdmin && (
                        <button
                          onClick={() => {
                            navigate('/holidays');
                            setMobileMenuOpen(false);
                          }}
                          className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                        >
                          🏖️ Holidays
                        </button>
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* Finance Section */}
              {canViewFinance && (
                <div className="border-b border-blue-700 pb-2">
                  <button
                    onClick={() => setMobileFinanceOpen(!mobileFinanceOpen)}
                    className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left flex items-center justify-between"
                  >
                    <span>💰 Finance</span>
                    <svg
                      className={`w-5 h-5 transition-transform ${mobileFinanceOpen ? 'rotate-180' : ''}`}
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </button>
                  {mobileFinanceOpen && (
                    <div className="mt-2 ml-4 space-y-1 animate-fade-in">
                      {canViewFees && (
                        <button
                          onClick={() => {
                            navigate('/fees');
                            setMobileMenuOpen(false);
                          }}
                          className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                        >
                          💰 Fees
                        </button>
                      )}
                      {canViewFees && (
                        <button
                          onClick={() => {
                            navigate('/fee-report');
                            setMobileMenuOpen(false);
                          }}
                          className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                        >
                          📊 Fee Report
                        </button>
                      )}
                      {canViewSalary && (
                        <button
                          onClick={() => {
                            navigate('/salary');
                            setMobileMenuOpen(false);
                          }}
                          className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                        >
                          🧾 Salary
                        </button>
                      )}
                      {canViewPayroll && (
                        <button
                          onClick={() => {
                            navigate('/payroll');
                            setMobileMenuOpen(false);
                          }}
                          className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                        >
                          💼 Payroll
                        </button>
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* Payroll Section */}
              {canManageSalary && (
                <div className="border-b border-blue-700 pb-2">
                  <button
                    onClick={() => setMobilePayrollOpen(!mobilePayrollOpen)}
                    className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left flex items-center justify-between"
                  >
                    <span>💼 Payroll</span>
                    <svg
                      className={`w-5 h-5 transition-transform ${mobilePayrollOpen ? 'rotate-180' : ''}`}
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </button>
                  {mobilePayrollOpen && (
                    <div className="mt-2 ml-4 space-y-1 animate-fade-in">
                      <button
                        onClick={() => {
                          navigate('/salary-structures');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        📊 Salary Structures
                      </button>
                      <button
                        onClick={() => {
                          navigate('/staff-salary-assignment');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        👥 Staff Assignments
                      </button>
                      <button
                        onClick={() => {
                          navigate('/bulk-salary-processing');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        🔄 Bulk Processing
                      </button>
                      <div className="my-1 border-t border-blue-700"></div>
                      <button
                        onClick={() => {
                          navigate('/salary-payments');
                          setMobileMenuOpen(false);
                        }}
                        className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                      >
                        💰 Payment Management
                      </button>
                    </div>
                  )}
                </div>
              )}

              {/* Reports Section */}
              {(canViewFees || canViewAttendanceReports) && (
                <div className="border-b border-blue-700 pb-2">
                  <button
                    onClick={() => setMobileReportsOpen(!mobileReportsOpen)}
                    className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left flex items-center justify-between"
                  >
                    <span>📊 Reports</span>
                    <svg
                      className={`w-5 h-5 transition-transform ${mobileReportsOpen ? 'rotate-180' : ''}`}
                      fill="none"
                      viewBox="0 0 24 24"
                      stroke="currentColor"
                    >
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                    </svg>
                  </button>
                  {mobileReportsOpen && (
                    <div className="mt-2 ml-4 space-y-1 animate-fade-in">
                      {canViewFees && (
                        <>
                          <button
                            onClick={() => {
                              navigate('/outstanding-fees');
                              setMobileMenuOpen(false);
                            }}
                            className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                          >
                            ⚠️ Outstanding Fees
                          </button>
                          <button
                            onClick={() => {
                              navigate('/staff-salary-comparison');
                              setMobileMenuOpen(false);
                            }}
                            className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                          >
                            📈 Salary Comparison
                          </button>
                          <button
                            onClick={() => {
                              navigate('/budget-vs-actual');
                              setMobileMenuOpen(false);
                            }}
                            className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                          >
                            📊 Budget vs Actual
                          </button>
                        </>
                      )}
                      {canViewAttendanceReports && (
                        <>
                          {canViewFees && <div className="my-1 border-t border-blue-700"></div>}
                          <button
                            onClick={() => {
                              navigate('/attendance-reports');
                              setMobileMenuOpen(false);
                            }}
                            className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                          >
                            📅 Attendance Reports
                          </button>
                        </>
                      )}
                    </div>
                  )}
                </div>
              )}

              {/* Attendance Direct Link */}
              {canViewAttendance && (
                <button
                  onClick={() => {
                    navigate('/attendance');
                    setMobileMenuOpen(false);
                  }}
                  className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
                >
                  📊 Attendance
                </button>
              )}

              {/* Account Section */}
              <div className="border-t border-blue-700 pt-2 space-y-1">
                {isAdmin && (
                  <>
                    <button
                      onClick={() => {
                        navigate('/admin/academic-years');
                        setMobileMenuOpen(false);
                      }}
                      className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                    >
                      📅 Academic Years
                    </button>
                    <button
                      onClick={() => {
                        navigate('/admin/settings');
                        setMobileMenuOpen(false);
                      }}
                      className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                    >
                      ⚙️ Settings
                    </button>
                  </>
                )}
                <button
                  onClick={() => {
                    navigate('/change-password');
                    setMobileMenuOpen(false);
                  }}
                  className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                >
                  🔒 Change Password
                </button>
                <button
                  onClick={() => {
                    setMobileMenuOpen(false);
                    handleLogout();
                  }}
                  className="w-full px-4 py-2 rounded-lg text-white hover:bg-red-600 transition-colors duration-200 text-left"
                >
                  🚪 Logout
                </button>
              </div>
            </div>
          </div>
        )}
      </nav>
    </header>
  );
};

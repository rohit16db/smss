import { useNavigate } from 'react-router-dom';
import { useState, useRef, useEffect } from 'react';
import { authService } from '../../services/authService';

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

export const Header = ({ onMenuClick }: HeaderProps) => {
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [academicMenuOpen, setAcademicMenuOpen] = useState(false);
  const [financeMenuOpen, setFinanceMenuOpen] = useState(false);
  const [mobileAcademicOpen, setMobileAcademicOpen] = useState(false);
  const [mobileFinanceOpen, setMobileFinanceOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const [currentUser, setCurrentUser] = useState<StoredUser | null>(null);
  
  const academicTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const financeTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
  const userMenuTimeoutRef = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

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
    window.addEventListener('storage', readUser);
    return () => window.removeEventListener('storage', readUser);
  }, []);

  const displayName =
    currentUser?.firstName || currentUser?.lastName
      ? `${currentUser.firstName || ''} ${currentUser.lastName || ''}`.trim()
      : currentUser?.username || currentUser?.email || 'User';

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

  const handleUserMenuMouseLeave = () => {
    userMenuTimeoutRef.current = setTimeout(() => {
      setUserMenuOpen(false);
    }, 150);
  };

  const handleUserMenuMouseEnter = () => {
    if (userMenuTimeoutRef.current) clearTimeout(userMenuTimeoutRef.current);
    setUserMenuOpen(true);
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
      <nav className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="flex h-16 items-center justify-between">
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
            <div 
              className="flex items-center cursor-pointer group" 
              onClick={() => navigate('/')}
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
            </div>
          </div>

          {/* Desktop Navigation */}
          <div className="hidden lg:flex items-center space-x-1">
            {/* Academic Dropdown */}
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
                    onClick={() => navigate('/teachers')}
                    className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                  >
                    👨‍🏫 Teachers
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
                </div>
              )}
            </div>

            {/* Finance Dropdown */}
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
                  <button
                    onClick={() => navigate('/fees')}
                    className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                  >
                    💰 Fees
                  </button>
                  <button
                    onClick={() => navigate('/salary')}
                    className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                  >
                    🧾 Salary
                  </button>
                  <button
                    onClick={() => navigate('/payroll')}
                    className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                  >
                    💼 Payroll
                  </button>
                  <hr className="my-1 border-gray-200" />
                  <button
                    onClick={() => navigate('/salary-structures')}
                    className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                  >
                    📊 Salary Structures
                  </button>
                  <button
                    onClick={() => navigate('/teacher-salary-assignment')}
                    className="w-full text-left px-4 py-2 hover:bg-blue-50 text-gray-700 hover:text-blue-600 transition-colors flex items-center gap-2"
                  >
                    👥 Teacher Assignments
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

            {/* Attendance Direct Link */}
            <button
              onClick={() => navigate('/attendance')}
              className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
            >
              📊 Attendance
            </button>
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
            <div className="flex flex-col space-y-2">
              {/* Academic Section */}
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
                        navigate('/teachers');
                        setMobileMenuOpen(false);
                      }}
                      className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                    >
                      👨‍🏫 Teachers
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
                  </div>
                )}
              </div>

              {/* Finance Section */}
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
                    <button
                      onClick={() => {
                        navigate('/fees');
                        setMobileMenuOpen(false);
                      }}
                      className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                    >
                      💰 Fees
                    </button>
                    <button
                      onClick={() => {
                        navigate('/salary');
                        setMobileMenuOpen(false);
                      }}
                      className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                    >
                      🧾 Salary
                    </button>
                    <button
                      onClick={() => {
                        navigate('/payroll');
                        setMobileMenuOpen(false);
                      }}
                      className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                    >
                      💼 Payroll
                    </button>
                    <div className="my-1 border-t border-blue-700"></div>
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
                        navigate('/teacher-salary-assignment');
                        setMobileMenuOpen(false);
                      }}
                      className="w-full px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 text-left"
                    >
                      👥 Teacher Assignments
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

              {/* Attendance Direct Link */}
              <button
                onClick={() => {
                  navigate('/attendance');
                  setMobileMenuOpen(false);
                }}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
              >
                📊 Attendance
              </button>

              {/* Account Section */}
              <div className="border-t border-blue-700 pt-2 space-y-1">
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

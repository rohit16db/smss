import { useNavigate } from 'react-router-dom';
import { useState } from 'react';

interface HeaderProps {
  onMenuClick?: () => void;
}

export const Header = ({ onMenuClick }: HeaderProps) => {
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  
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
          <div className="hidden lg:flex items-center space-x-2">
            <button
              onClick={() => navigate('/teachers')}
              className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
            >
              👨‍🏫 Teachers
            </button>
            <button
              onClick={() => navigate('/students')}
              className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
            >
              👨‍🎓 Students
            </button>
            <button
              onClick={() => navigate('/fees')}
              className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
            >
              💰 Fees
            </button>
            <button
              onClick={() => navigate('/attendance')}
              className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
            >
              📊 Attendance
            </button>
            <button
              onClick={() => navigate('/payroll')}
              className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
            >
              💼 Payroll
            </button>
            <button
              onClick={() => navigate('/salary')}
              className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
            >
              🧾 Salary
            </button>
            <button
              onClick={() => navigate('/classes')}
              className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium"
            >
              📚 Classes
            </button>
          </div>

          {/* User Menu */}
          <div className="flex items-center">
            <button className="p-2 rounded-full text-white hover:bg-blue-700 transition-colors">
              <svg className="h-8 w-8" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
              </svg>
            </button>
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
              <button
                onClick={() => {
                  navigate('/teachers');
                  setMobileMenuOpen(false);
                }}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
              >
                👨‍🏫 Teachers
              </button>
              <button
                onClick={() => {
                  navigate('/students');
                  setMobileMenuOpen(false);
                }}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
              >
                👨‍🎓 Students
              </button>
              <button
                onClick={() => {
                  navigate('/fees');
                  setMobileMenuOpen(false);
                }}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
              >
                💰 Fees
              </button>
              <button
                onClick={() => {
                  navigate('/attendance');
                  setMobileMenuOpen(false);
                }}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
              >
                📊 Attendance
              </button>
              <button
                onClick={() => {
                  navigate('/payroll');
                  setMobileMenuOpen(false);
                }}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
              >
                💼 Payroll
              </button>
              <button
                onClick={() => {
                  navigate('/salary');
                  setMobileMenuOpen(false);
                }}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
              >
                🧾 Salary
              </button>
              <button
                onClick={() => {
                  navigate('/classes');
                  setMobileMenuOpen(false);
                }}
                className="px-4 py-2 rounded-lg text-white hover:bg-blue-700 transition-colors duration-200 font-medium text-left"
              >
                📚 Classes
              </button>
            </div>
          </div>
        )}
      </nav>
    </header>
  );
};

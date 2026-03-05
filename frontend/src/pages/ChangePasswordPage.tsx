import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { AxiosError } from 'axios';
import { Eye, EyeOff, Lock, ArrowLeft, CheckCircle, AlertCircle } from 'lucide-react';
import { authService } from '../services/authService';

export const ChangePasswordPage = () => {
  const navigate = useNavigate();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  // Password strength calculation
  const calculatePasswordStrength = (password: string) => {
    if (!password) return 0;
    let strength = 0;
    if (password.length >= 8) strength += 25;
    if (password.length >= 12) strength += 10;
    if (/[a-z]/.test(password)) strength += 15;
    if (/[A-Z]/.test(password)) strength += 15;
    if (/[0-9]/.test(password)) strength += 15;
    if (/[^A-Za-z0-9]/.test(password)) strength += 20;
    return Math.min(strength, 100);
  };

  const passwordStrength = calculatePasswordStrength(newPassword);

  const getPasswordStrengthColor = () => {
    if (passwordStrength < 40) return 'bg-red-500';
    if (passwordStrength < 70) return 'bg-yellow-500';
    return 'bg-green-500';
  };

  const getPasswordStrengthText = () => {
    if (passwordStrength < 40) return 'Weak';
    if (passwordStrength < 70) return 'Fair';
    return 'Strong';
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSuccess('');

    if (newPassword !== confirmPassword) {
      setError('New passwords do not match');
      return;
    }

    if (newPassword.length < 6) {
      setError('New password must be at least 6 characters');
      return;
    }

    setLoading(true);

    try {
      await authService.changePassword(currentPassword, newPassword);
      setSuccess('Password changed successfully!');
      setTimeout(() => {
        navigate('/');
      }, 2000);
    } catch (err: unknown) {
      setError((err as AxiosError<{ message?: string }>).response?.data?.message || 'Failed to change password. Please check your current password.');
      console.error('Change password error:', err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-900 to-slate-900 flex items-center justify-center p-4 py-4">
      <div className="w-full max-w-md">
        {/* Back Button */}
        <button
          onClick={() => navigate(-1)}
          className="flex items-center gap-2 text-blue-200 hover:text-white transition-colors mb-4 group"
          disabled={loading}
        >
          <ArrowLeft size={20} className="group-hover:-translate-x-1 transition-transform" />
          <span>Back</span>
        </button>

        {/* Main Card */}
        <div className="bg-white rounded-2xl shadow-2xl overflow-hidden">
          {/* Header */}
          <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-5">
            <div className="flex items-center gap-3 mb-2">
              <div className="bg-blue-500 bg-opacity-40 p-2 rounded-full">
                <Lock size={20} className="text-white" />
              </div>
              <div>
                <h1 className="text-xl font-bold text-white">Change Password</h1>
                <p className="text-blue-100 text-xs mt-0.5">Update your account security</p>
              </div>
            </div>
          </div>

          {/* Content */}
          <div className="p-4 sm:p-6">
            {/* Error Alert */}
            {error && (
              <div className="mb-3 p-3 bg-red-50 border border-red-200 rounded-lg flex gap-2 animate-slide-down">
                <AlertCircle size={16} className="text-red-600 flex-shrink-0 mt-0.5" />
                <p className="text-red-800 text-xs font-medium">{error}</p>
              </div>
            )}

            {/* Success Alert */}
            {success && (
              <div className="mb-3 p-3 bg-green-50 border border-green-200 rounded-lg flex gap-2 animate-slide-down">
                <CheckCircle size={16} className="text-green-600 flex-shrink-0 mt-0.5" />
                <p className="text-green-800 text-xs font-medium">{success}</p>
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-3">
              {/* Current Password */}
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1.5">Current Password</label>
                <div className="relative">
                  <input
                    type={showCurrentPassword ? 'text' : 'password'}
                    value={currentPassword}
                    onChange={(e) => setCurrentPassword(e.target.value)}
                    disabled={loading}
                    placeholder="Enter current password"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200 disabled:bg-gray-50 disabled:cursor-not-allowed"
                  />
                  <button
                    type="button"
                    onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                    disabled={loading}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 transition-colors disabled:cursor-not-allowed"
                  >
                    {showCurrentPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              {/* New Password */}
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1.5">New Password</label>
                <div className="relative">
                  <input
                    type={showNewPassword ? 'text' : 'password'}
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    disabled={loading}
                    placeholder="Create strong password"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200 disabled:bg-gray-50 disabled:cursor-not-allowed"
                  />
                  <button
                    type="button"
                    onClick={() => setShowNewPassword(!showNewPassword)}
                    disabled={loading}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 transition-colors disabled:cursor-not-allowed"
                  >
                    {showNewPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>

                {/* Password Strength Indicator */}
                {newPassword && (
                  <div className="mt-2 space-y-1.5 animate-fade-in">
                    <div className="flex items-center justify-between text-xs">
                      <span className="text-gray-600">Strength:</span>
                      <span className={`font-semibold ${
                        passwordStrength < 40 ? 'text-red-600' :
                        passwordStrength < 70 ? 'text-yellow-600' :
                        'text-green-600'
                      }`}>
                        {getPasswordStrengthText()}
                      </span>
                    </div>
                    <div className="w-full h-1.5 bg-gray-200 rounded-full overflow-hidden">
                      <div
                        className={`h-full transition-all duration-300 ${getPasswordStrengthColor()}`}
                        style={{ width: `${passwordStrength}%` }}
                      ></div>
                    </div>
                  </div>
                )}
              </div>

              {/* Confirm Password */}
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1.5">Confirm New Password</label>
                <div className="relative">
                  <input
                    type={showConfirmPassword ? 'text' : 'password'}
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    disabled={loading}
                    placeholder="Re-enter password"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200 disabled:bg-gray-50 disabled:cursor-not-allowed"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    disabled={loading}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 transition-colors disabled:cursor-not-allowed"
                  >
                    {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>

                {/* Password Match Indicator */}
                {newPassword && confirmPassword && (
                  <div className="mt-1.5 text-xs animate-fade-in">
                    {newPassword === confirmPassword ? (
                      <div className="flex items-center gap-1.5 text-green-600">
                        <CheckCircle size={14} />
                        <span>Passwords match</span>
                      </div>
                    ) : (
                      <div className="flex items-center gap-1.5 text-red-600">
                        <AlertCircle size={14} />
                        <span>Passwords do not match</span>
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* Buttons */}
              <div className="pt-2 space-y-2">
                <button
                  type="submit"
                  disabled={loading || !currentPassword || !newPassword || !confirmPassword}
                  className="w-full py-2.5 px-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white text-sm font-semibold rounded-lg hover:from-blue-700 hover:to-blue-800 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg hover:shadow-xl flex items-center justify-center gap-2"
                >
                  {loading ? (
                    <>
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                      <span>Updating...</span>
                    </>
                  ) : (
                    <>
                      <Lock size={16} />
                      <span>Change Password</span>
                    </>
                  )}
                </button>

                <button
                  type="button"
                  onClick={() => navigate(-1)}
                  disabled={loading}
                  className="w-full py-2.5 px-3 border-2 border-gray-300 text-gray-700 text-sm font-semibold rounded-lg hover:border-gray-400 hover:bg-gray-50 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

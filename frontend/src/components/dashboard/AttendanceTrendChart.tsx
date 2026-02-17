import React from 'react';
import type { AttendanceSummary } from '../../types/dashboard';
import { TrendingUp, AlertCircle } from 'lucide-react';

interface AttendanceTrendChartProps {
  data: AttendanceSummary;
  isLoading?: boolean;
}

export const AttendanceTrendChart: React.FC<AttendanceTrendChartProps> = ({ data, isLoading = false }) => {
  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-6 animate-pulse">
        <div className="h-6 bg-gray-200 rounded w-1/3 mb-6"></div>
        <div className="h-64 bg-gray-200 rounded"></div>
      </div>
    );
  }

  const getAttendanceColor = (percentage: number) => {
    if (percentage >= 90) return 'text-green-600';
    if (percentage >= 75) return 'text-blue-600';
    if (percentage >= 60) return 'text-orange-600';
    return 'text-red-600';
  };

  const getAttendanceBgColor = (percentage: number) => {
    if (percentage >= 90) return 'bg-green-100';
    if (percentage >= 75) return 'bg-blue-100';
    if (percentage >= 60) return 'bg-orange-100';
    return 'bg-red-100';
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      {/* Header */}
      <h3 className="text-lg font-semibold text-gray-900 mb-6">Attendance Overview</h3>

      {/* Attendance Metrics */}
      <div className="space-y-6">
        {/* Student Attendance */}
        <div className={`${getAttendanceBgColor(data.averageStudentAttendance)} rounded-lg p-4`}>
          <div className="flex items-start justify-between mb-3">
            <div>
              <p className="text-sm font-medium text-gray-700">Average Student Attendance</p>
              <p className={`text-3xl font-bold mt-2 ${getAttendanceColor(data.averageStudentAttendance)}`}>
                {data.averageStudentAttendance.toFixed(1)}%
              </p>
            </div>
            {data.averageStudentAttendance >= 75 && (
              <TrendingUp className={`w-8 h-8 ${getAttendanceColor(data.averageStudentAttendance)}`} />
            )}
            {data.averageStudentAttendance < 75 && (
              <AlertCircle className={`w-8 h-8 ${getAttendanceColor(data.averageStudentAttendance)}`} />
            )}
          </div>
          <p className="text-xs text-gray-600">
            {Math.round((data.presentStudentsTodayCount / (data.totalStudents || 1)) * 100)}% present today ({data.presentStudentsTodayCount}/{data.totalStudents})
          </p>
        </div>

        {/* Teacher Attendance */}
        <div className={`${getAttendanceBgColor(data.averageTeacherAttendance)} rounded-lg p-4`}>
          <div className="flex items-start justify-between mb-3">
            <div>
              <p className="text-sm font-medium text-gray-700">Average Teacher Attendance</p>
              <p className={`text-3xl font-bold mt-2 ${getAttendanceColor(data.averageTeacherAttendance)}`}>
                {data.averageTeacherAttendance.toFixed(1)}%
              </p>
            </div>
            {data.averageTeacherAttendance >= 75 && (
              <TrendingUp className={`w-8 h-8 ${getAttendanceColor(data.averageTeacherAttendance)}`} />
            )}
            {data.averageTeacherAttendance < 75 && (
              <AlertCircle className={`w-8 h-8 ${getAttendanceColor(data.averageTeacherAttendance)}`} />
            )}
          </div>
          <p className="text-xs text-gray-600">
            {data.totalTeachers} teachers tracked
          </p>
        </div>

        {/* Today's Snapshot */}
        <div className="grid grid-cols-2 gap-4 pt-4 border-t border-gray-200">
          <div className="p-3 bg-green-50 rounded-lg">
            <p className="text-xs text-gray-600 font-medium mb-1">Present Today</p>
            <p className="text-2xl font-bold text-green-600">{data.presentStudentsTodayCount}</p>
            <p className="text-xs text-gray-500 mt-1">students</p>
          </div>
          <div className="p-3 bg-red-50 rounded-lg">
            <p className="text-xs text-gray-600 font-medium mb-1">Absent Today</p>
            <p className="text-2xl font-bold text-red-600">{data.absentStudentsTodayCount}</p>
            <p className="text-xs text-gray-500 mt-1">students</p>
          </div>
        </div>

        {/* Attendance Standards */}
        <div className="pt-4 border-t border-gray-200">
          <p className="text-xs font-medium text-gray-700 mb-3">Performance Standard</p>
          <div className="space-y-2 text-xs text-gray-600">
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 bg-green-500 rounded-full"></div>
              <span>90%+ : Excellent</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 bg-blue-500 rounded-full"></div>
              <span>75-90% : Good</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 bg-orange-500 rounded-full"></div>
              <span>60-75% : Fair</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 bg-red-500 rounded-full"></div>
              <span>&lt;60% : Poor</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

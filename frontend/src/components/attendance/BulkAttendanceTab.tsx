import { useState, useEffect, useMemo } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { classApi, attendanceApi, type StudentSection, type BulkAttendanceEntry, type ClassListDto, type SectionListDto, type BulkAttendanceResult, type BulkMarkAttendanceRequest } from '../../services/api';

const ATTENDANCE_STATUSES = [
  { value: 'present', label: 'Present', icon: '✓', color: 'bg-green-500', bgLight: 'bg-green-50 border-green-300 text-green-800', ring: 'ring-green-500' },
  { value: 'absent', label: 'Absent', icon: '✗', color: 'bg-red-500', bgLight: 'bg-red-50 border-red-300 text-red-800', ring: 'ring-red-500' },
  { value: 'late', label: 'Late', icon: '⏰', color: 'bg-yellow-500', bgLight: 'bg-yellow-50 border-yellow-300 text-yellow-800', ring: 'ring-yellow-500' },
  { value: 'leave', label: 'Leave', icon: '📋', color: 'bg-blue-500', bgLight: 'bg-blue-50 border-blue-300 text-blue-800', ring: 'ring-blue-500' },
  { value: 'unexcused', label: 'Unexcused', icon: '⚠', color: 'bg-orange-500', bgLight: 'bg-orange-50 border-orange-300 text-orange-800', ring: 'ring-orange-500' },
];

type AttendanceMap = Record<string, { status: string; reason?: string }>;

export function BulkAttendanceTab() {
  const queryClient = useQueryClient();
  const [selectedClassId, setSelectedClassId] = useState<string>('');
  const [selectedSectionId, setSelectedSectionId] = useState<string>('');
  const [attendanceDate, setAttendanceDate] = useState<string>(
    new Date().toISOString().split('T')[0]
  );
  const [attendanceMap, setAttendanceMap] = useState<AttendanceMap>({});
  const [hasExistingData, setHasExistingData] = useState(false);

  // Fetch all classes
  const { data: classesData } = useQuery({
    queryKey: ['classes-bulk'],
    queryFn: () => classApi.getAll({ pageSize: 100, isActive: true }),
  });

  // Fetch sections for selected class
  const { data: sectionsData } = useQuery({
    queryKey: ['sections-bulk', selectedClassId],
    queryFn: () => classApi.getSectionsByClass(selectedClassId),
    enabled: !!selectedClassId,
  });

  // Fetch students (roll numbers) for selected section
  const { data: studentsData, isLoading: studentsLoading } = useQuery({
    queryKey: ['rollNumbers-bulk', selectedSectionId],
    queryFn: () => classApi.getRollNumbers(selectedSectionId),
    enabled: !!selectedSectionId,
  });

  // Fetch existing attendance for this section + date
  const { data: existingAttendance, isLoading: existingLoading } = useQuery({
    queryKey: ['existingAttendance', selectedSectionId, attendanceDate],
    queryFn: () => attendanceApi.getStudentAttendanceByDate(selectedSectionId, attendanceDate),
    enabled: !!selectedSectionId && !!attendanceDate,
  });

  // Sort students by roll number
  const sortedStudents = useMemo(() => {
    if (!studentsData) return [];
    return [...studentsData].sort((a, b) => (a.rollNumber || 999) - (b.rollNumber || 999));
  }, [studentsData]);

  // Initialize/reset attendance map when students or existing data loads
  useEffect(() => {
    if (!sortedStudents.length) return;

    const newMap: AttendanceMap = {};
    const existingMap = new Map<string, {status: string; reason?: string}>(
      (existingAttendance || []).map(a => [a.studentId, { status: a.status?.toLowerCase(), reason: a.reason || a.remarks }])
    );

    let foundExisting = false;
    sortedStudents.forEach((student) => {
      const existing = existingMap.get(student.studentId);
      if (existing) {
        newMap[student.studentId] = { status: existing.status, reason: existing.reason };
        foundExisting = true;
      } else {
        newMap[student.studentId] = { status: 'present', reason: '' };
      }
    });

    setHasExistingData(foundExisting);
    setAttendanceMap(newMap);
  }, [sortedStudents, existingAttendance]);

  // Reset section when class changes
  useEffect(() => {
    setSelectedSectionId('');
    setAttendanceMap({});
  }, [selectedClassId]);

  // Bulk save mutation
  const bulkSaveMutation = useMutation<BulkAttendanceResult, any, BulkMarkAttendanceRequest>({
    mutationFn: (data: BulkMarkAttendanceRequest) => attendanceApi.bulkMarkStudentAttendance(data),
    onSuccess: (result: BulkAttendanceResult) => {
      if (result.failed > 0) {
        toast.error(`${result.failed} records failed. ${result.errors.join(', ')}`);
      } else {
        toast.success(`Attendance saved! ${result.created} created, ${result.updated} updated.`);
      }
      queryClient.invalidateQueries({ queryKey: ['existingAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['studentAttendance'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to save attendance');
    },
  });

  const handleStatusChange = (studentId: string, status: string) => {
    setAttendanceMap(prev => ({
      ...prev,
      [studentId]: { ...prev[studentId], status },
    }));
  };

  const handleMarkAll = (status: string) => {
    setAttendanceMap(prev => {
      const newMap: AttendanceMap = {};
      Object.keys(prev).forEach(studentId => {
        newMap[studentId] = { ...prev[studentId], status };
      });
      return newMap;
    });
  };

  const handleSave = () => {
    if (!selectedSectionId || !attendanceDate) {
      toast.error('Please select a section and date');
      return;
    }

    const entries: BulkAttendanceEntry[] = Object.entries(attendanceMap).map(
      ([studentId, data]) => ({
        studentId,
        status: data.status,
        reason: data.reason || undefined,
      })
    );

    bulkSaveMutation.mutate({
      sectionId: selectedSectionId,
      attendanceDate: attendanceDate,
      entries,
    });
  };

  // Summary counts
  const summary = useMemo(() => {
    const counts: Record<string, number> = {};
    ATTENDANCE_STATUSES.forEach(s => (counts[s.value] = 0));
    Object.values(attendanceMap).forEach(({ status }) => {
      counts[status] = (counts[status] || 0) + 1;
    });
    return counts;
  }, [attendanceMap]);

  const totalStudents = sortedStudents.length;
  const selectedClassName = classesData?.items?.find((c: ClassListDto) => c.id === selectedClassId)?.name || '';
  const selectedSectionName = sectionsData?.find((s: SectionListDto) => s.id === selectedSectionId)?.sectionName || '';

  return (
    <div className="space-y-6">
      {/* Selection Bar */}
      <div className="bg-white rounded-xl shadow-md p-6 border border-gray-100">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Class Selector */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">📚 Select Class</label>
            <select
              value={selectedClassId}
              onChange={(e) => setSelectedClassId(e.target.value)}
              className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white text-gray-900 font-medium"
            >
              <option value="">-- Choose Class --</option>
              {classesData?.items?.map((cls: ClassListDto) => (
                <option key={cls.id} value={cls.id}>
                  {cls.name} ({cls.sectionCount} sections)
                </option>
              ))}
            </select>
          </div>

          {/* Section Selector */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">🏫 Select Section</label>
            <select
              value={selectedSectionId}
              onChange={(e) => setSelectedSectionId(e.target.value)}
              disabled={!selectedClassId}
              className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white text-gray-900 font-medium disabled:bg-gray-100 disabled:text-gray-400"
            >
              <option value="">-- Choose Section --</option>
              {sectionsData?.map((section: SectionListDto) => (
                <option key={section.id} value={section.id}>
                  {section.sectionName} ({section.studentCount} students)
                </option>
              ))}
            </select>
          </div>

          {/* Date Picker */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">📅 Attendance Date</label>
            <input
              type="date"
              value={attendanceDate}
              onChange={(e) => setAttendanceDate(e.target.value)}
              max={new Date().toISOString().split('T')[0]}
              className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-white text-gray-900 font-medium"
            />
          </div>
        </div>
      </div>

      {/* Content Area */}
      {!selectedSectionId ? (
        <div className="bg-white rounded-xl shadow-md p-12 text-center border border-gray-100">
          <div className="text-6xl mb-4">📋</div>
          <h3 className="text-xl font-semibold text-gray-700 mb-2">Select a Class & Section</h3>
          <p className="text-gray-500">Choose a class and section above to load the student list for bulk attendance marking.</p>
        </div>
      ) : studentsLoading || existingLoading ? (
        <div className="bg-white rounded-xl shadow-md p-12 text-center border border-gray-100">
          <div className="animate-spin text-4xl mb-4">⏳</div>
          <p className="text-gray-600 font-medium">Loading students...</p>
        </div>
      ) : sortedStudents.length === 0 ? (
        <div className="bg-white rounded-xl shadow-md p-12 text-center border border-gray-100">
          <div className="text-6xl mb-4">📭</div>
          <h3 className="text-xl font-semibold text-gray-700 mb-2">No Students Found</h3>
          <p className="text-gray-500">No enrolled students found in this section. Please enroll students first.</p>
        </div>
      ) : (
        <>
          {/* Action Bar + Summary */}
          <div className="bg-white rounded-xl shadow-md p-4 border border-gray-100">
            <div className="flex flex-col lg:flex-row items-start lg:items-center justify-between gap-4">
              {/* Quick Actions */}
              <div className="flex flex-wrap items-center gap-2">
                <span className="text-sm font-semibold text-gray-600 mr-2">Quick Actions:</span>
                {ATTENDANCE_STATUSES.map((status) => (
                  <button
                    key={status.value}
                    onClick={() => handleMarkAll(status.value)}
                    className={`px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all hover:scale-105 ${status.bgLight}`}
                  >
                    {status.icon} Mark All {status.label}
                  </button>
                ))}
              </div>

              {/* Summary Counts */}
              <div className="flex flex-wrap items-center gap-3">
                <span className="text-sm font-semibold text-gray-600">Total: {totalStudents}</span>
                <span className="text-gray-300">|</span>
                {ATTENDANCE_STATUSES.map((status) => (
                  <span key={status.value} className="flex items-center gap-1 text-sm">
                    <span className={`w-3 h-3 rounded-full ${status.color}`}></span>
                    <span className="font-medium">{summary[status.value] || 0}</span>
                  </span>
                ))}
              </div>
            </div>

            {hasExistingData && (
              <div className="mt-3 px-3 py-2 bg-amber-50 border border-amber-200 rounded-lg text-sm text-amber-800 flex items-center gap-2">
                <span>⚠️</span>
                <span>Attendance already exists for <strong>{selectedClassName} - {selectedSectionName}</strong> on <strong>{new Date(attendanceDate).toLocaleDateString()}</strong>. Changes will overwrite existing records.</span>
              </div>
            )}
          </div>

          {/* Student Grid */}
          <div className="bg-white rounded-xl shadow-md border border-gray-100 overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white">
                    <th className="px-4 py-3 text-left text-sm font-semibold w-16">#</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold w-20">Roll No</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold">Student Name</th>
                    <th className="px-4 py-3 text-left text-sm font-semibold w-28">Enrollment</th>
                    {ATTENDANCE_STATUSES.map((status) => (
                      <th key={status.value} className="px-3 py-3 text-center text-sm font-semibold w-24">
                        {status.icon} {status.label}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {sortedStudents.map((student: StudentSection, index: number) => {
                    const currentStatus = attendanceMap[student.studentId]?.status || 'present';
                    return (
                      <tr
                        key={student.studentId}
                        className={`border-b border-gray-100 transition-colors ${
                          index % 2 === 0 ? 'bg-white' : 'bg-gray-50/50'
                        } hover:bg-blue-50/30`}
                      >
                        <td className="px-4 py-3 text-sm text-gray-500 font-mono">{index + 1}</td>
                        <td className="px-4 py-3">
                          <span className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-gradient-to-br from-blue-500 to-indigo-600 text-white text-xs font-bold">
                            {student.rollNumber || '—'}
                          </span>
                        </td>
                        <td className="px-4 py-3">
                          <span className="font-medium text-gray-900">{student.studentName}</span>
                        </td>
                        <td className="px-4 py-3 text-sm text-gray-500 font-mono">
                          {student.enrollmentNumber}
                        </td>
                        {ATTENDANCE_STATUSES.map((status) => (
                          <td key={status.value} className="px-3 py-3 text-center">
                            <button
                              onClick={() => handleStatusChange(student.studentId, status.value)}
                              className={`w-9 h-9 rounded-full border-2 transition-all duration-200 flex items-center justify-center mx-auto hover:scale-110 ${
                                currentStatus === status.value
                                  ? `${status.color} text-white border-transparent ring-2 ${status.ring} ring-offset-1 shadow-md`
                                  : 'border-gray-300 text-gray-400 hover:border-gray-400 bg-white'
                              }`}
                              title={`Mark ${status.label}`}
                            >
                              <span className="text-sm font-bold">{status.icon}</span>
                            </button>
                          </td>
                        ))}
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>

          {/* Save Button */}
          <div className="flex justify-end">
            <button
              onClick={handleSave}
              disabled={bulkSaveMutation.isPending || totalStudents === 0}
              className="px-8 py-3 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white font-semibold rounded-xl shadow-lg transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed hover:shadow-xl hover:scale-[1.02] flex items-center gap-2"
            >
              {bulkSaveMutation.isPending ? (
                <>
                  <span className="animate-spin">⏳</span> Saving...
                </>
              ) : (
                <>
                  💾 Save Attendance ({totalStudents} students)
                </>
              )}
            </button>
          </div>
        </>
      )}
    </div>
  );
}

import { useState, useRef, useEffect } from 'react';
import toast from 'react-hot-toast';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { attendanceApi, studentApi, StaffApi, holidayApi, notificationApi, type CreateStudentAttendanceDto, type CreateStaffAttendanceDto, type StudentAttendance, type StaffAttendance, type Student, type Staff } from '../services/api';
import { useAcademicYear } from '../hooks/useAcademicYear';
import { WhatsAppIcon } from '../components/WhatsAppIcon';

export function AttendancePage() {
  const queryClient = useQueryClient();
  const { activeYear } = useAcademicYear();
  const [activeTab, setActiveTab] = useState<'student' | 'Staff'>('student');
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStudent, setSelectedStudent] = useState<Student | null>(() => {
    const saved = localStorage.getItem('attendance_selectedStudent');
    return saved ? JSON.parse(saved) : null;
  });
  const [showStudentDropdown, setShowStudentDropdown] = useState(false);
  const [selectedStaff, setSelectedStaff] = useState<Staff | null>(() => {
    const saved = localStorage.getItem('attendance_selectedStaff');
    return saved ? JSON.parse(saved) : null;
  });
  const [StaffSearchTerm, setStaffSearchTerm] = useState('');
  const [showStaffDropdown, setShowStaffDropdown] = useState(false);
  const [dialogSearchTerm, setDialogSearchTerm] = useState('');
  const [dialogSelectedStudent, setDialogSelectedStudent] = useState<Student | null>(null);
  const [showDialogStudentDropdown, setShowDialogStudentDropdown] = useState(false);
  const [dialogStaffSearchTerm, setDialogStaffSearchTerm] = useState('');
  const [dialogSelectedStaff, setDialogSelectedStaff] = useState<Staff | null>(null);
  const [showDialogStaffDropdown, setShowDialogStaffDropdown] = useState(false);
  const [openDialog, setOpenDialog] = useState(false);
  const [selectedRecord, setSelectedRecord] = useState<StudentAttendance | StaffAttendance | null>(null);
  const [viewMode, setViewMode] = useState<'list' | 'calendar'>('list');
  const [calendarMonth, setCalendarMonth] = useState(() => new Date().toISOString().slice(0, 7));
  const dropdownRef = useRef<HTMLDivElement>(null);
  const StaffDropdownRef = useRef<HTMLDivElement>(null);
  const dialogDropdownRef = useRef<HTMLDivElement>(null);
  const dialogStaffDropdownRef = useRef<HTMLDivElement>(null);
  
  const [studentFormData, setStudentFormData] = useState<CreateStudentAttendanceDto>({
    studentId: '',
    // sectionId removed - auto-detected from student enrollment
    attendanceDate: new Date().toISOString().split('T')[0],
    status: 'Present',
    reason: '',
  });

  const [StaffFormData, setStaffFormData] = useState<CreateStaffAttendanceDto>({
    staffId: '',
    attendanceDate: new Date().toISOString().split('T')[0],
    status: 'Present',
    reason: '',
  });

  // Student search query for filter
  const { data: studentsData } = useQuery({
    queryKey: ['students', searchTerm],
    queryFn: () => studentApi.getAll({ searchTerm: searchTerm || undefined, pageSize: 50, isActive: true }),
    enabled: activeTab === 'student' && searchTerm.length >= 2,
  });

  // Student search query for dialog
  const { data: dialogStudentsData } = useQuery({
    queryKey: ['dialogStudents', dialogSearchTerm],
    queryFn: () => studentApi.getAll({ searchTerm: dialogSearchTerm || undefined, pageSize: 50, isActive: true }),
    enabled: openDialog && activeTab === 'student' && dialogSearchTerm.length >= 2,
  });

  // Staff search query for filter
  const { data: StaffsData } = useQuery({
    queryKey: ['Staffs', StaffSearchTerm],
    queryFn: () => StaffApi.getAll({ searchTerm: StaffSearchTerm || undefined, pageSize: 50, isActive: true }),
    enabled: activeTab === 'Staff' && StaffSearchTerm.length >= 2,
  });

  // Staff search query for dialog
  const { data: dialogStaffsData } = useQuery({
    queryKey: ['dialogStaffs', dialogStaffSearchTerm],
    queryFn: () => StaffApi.getAll({ searchTerm: dialogStaffSearchTerm || undefined, pageSize: 50, isActive: true }),
    enabled: openDialog && activeTab === 'Staff' && dialogStaffSearchTerm.length >= 2,
  });

  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setShowStudentDropdown(false);
      }
      if (StaffDropdownRef.current && !StaffDropdownRef.current.contains(event.target as Node)) {
        setShowStaffDropdown(false);
      }
      if (dialogDropdownRef.current && !dialogDropdownRef.current.contains(event.target as Node)) {
        setShowDialogStudentDropdown(false);
      }
      if (dialogStaffDropdownRef.current && !dialogStaffDropdownRef.current.contains(event.target as Node)) {
        setShowDialogStaffDropdown(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Persist selected student to localStorage
  useEffect(() => {
    if (selectedStudent) {
      localStorage.setItem('attendance_selectedStudent', JSON.stringify(selectedStudent));
    } else {
      localStorage.removeItem('attendance_selectedStudent');
    }
  }, [selectedStudent]);

  // Persist selected Staff to localStorage
  useEffect(() => {
    if (selectedStaff) {
      localStorage.setItem('attendance_selectedStaff', JSON.stringify(selectedStaff));
    } else {
      localStorage.removeItem('attendance_selectedStaff');
    }
  }, [selectedStaff]);

  // Parse month filter to get date range
  const parseMonthFilter = (monthStr: string) => {
    const [year, month] = monthStr.split('-');
    const startDate = `${year}-${month}-01`;
    const lastDay = new Date(Number(year), Number(month), 0).getDate();
    const endDate = `${year}-${month}-${String(lastDay).padStart(2, '0')}`;
    return { startDate, endDate };
  };

  const monthRange = parseMonthFilter(calendarMonth);

  // Student Attendance Queries
  const { data: studentAttendanceData, isLoading: studentLoading } = useQuery({
    queryKey: ['studentAttendance', page + 1, rowsPerPage, selectedStudent?.id, calendarMonth, activeTab],
    enabled: activeTab === 'student',
    queryFn: () => attendanceApi.getAllStudentAttendance({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
      studentId: selectedStudent?.id || undefined,
      startDate: monthRange.startDate,
      endDate: monthRange.endDate,
    }),
  });

  const studentCreateMutation = useMutation({
    mutationFn: attendanceApi.recordStudentAttendance,
    onSuccess: () => {
      toast.success('Student attendance recorded successfully!');
      queryClient.invalidateQueries({ queryKey: ['studentAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['studentAttendanceCalendar'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      const status = error?.response?.status;
      const responseData = error?.response?.data;
      
      // Extract message from different possible response formats
      const message = typeof responseData === 'string' 
        ? responseData 
        : responseData?.message || error?.message || 'Failed to record attendance';
      
      // Log the error for debugging
      console.error('Attendance error:', { status, message, responseData });
      
      if (status === 409) {
        // Conflict - attendance already marked
        toast.error(message || 'Attendance already marked for this student on this date');
      } else if (status === 400) {
        toast.error(message);
      } else {
        toast.error(message);
      }
    },
  });

  const studentUpdateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<CreateStudentAttendanceDto> & { id: string } }) =>
      attendanceApi.updateStudentAttendance(id, data),
    onSuccess: () => {
      toast.success('Attendance updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['studentAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['studentAttendanceCalendar'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to update attendance');
    },
  });

  const studentDeleteMutation = useMutation({
    mutationFn: attendanceApi.deleteStudentAttendance,
    onSuccess: () => {
      toast.success('Attendance record deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['studentAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['studentAttendanceCalendar'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to delete record');
    },
  });

  // Staff Attendance Queries
  const { data: StaffAttendanceData, isLoading: StaffLoading } = useQuery({
    queryKey: ['StaffAttendance', page + 1, rowsPerPage, selectedStaff?.id, calendarMonth, activeTab],
    enabled: activeTab === 'Staff',
    queryFn: () => attendanceApi.getAllStaffAttendance({
      pageNumber: page + 1,
      pageSize: rowsPerPage,
      staffId: selectedStaff?.id || undefined,
      startDate: monthRange.startDate,
      endDate: monthRange.endDate,
    }),
  });

  const StaffCreateMutation = useMutation({
    mutationFn: attendanceApi.recordStaffAttendance,
    onSuccess: () => {
      toast.success('Staff attendance recorded successfully!');
      queryClient.invalidateQueries({ queryKey: ['StaffAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['StaffAttendanceCalendar'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      const status = error?.response?.status;
      const responseData = error?.response?.data;
      
      // Extract message from different possible response formats
      const message = typeof responseData === 'string' 
        ? responseData 
        : responseData?.message || error?.message || 'Failed to record attendance';
      
      // Log the error for debugging
      console.error('Attendance error:', { status, message, responseData });
      
      if (status === 409) {
        // Conflict - attendance already marked
        toast.error(message || 'Attendance already marked for this Staff on this date');
      } else if (status === 400) {
        toast.error(message);
      } else {
        toast.error(message);
      }
    },
  });

  const StaffUpdateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Partial<CreateStaffAttendanceDto> & { id: string } }) =>
      attendanceApi.updateStaffAttendance(id, data),
    onSuccess: () => {
      toast.success('Attendance updated successfully!');
      queryClient.invalidateQueries({ queryKey: ['StaffAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['StaffAttendanceCalendar'] });
      handleCloseDialog();
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to update attendance');
    },
  });

  const StaffDeleteMutation = useMutation({
    mutationFn: attendanceApi.deleteStaffAttendance,
    onSuccess: () => {
      toast.success('Attendance record deleted successfully!');
      queryClient.invalidateQueries({ queryKey: ['StaffAttendance'] });
      queryClient.invalidateQueries({ queryKey: ['StaffAttendanceCalendar'] });
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Failed to delete record');
    },
  });

  const handleOpenDialog = (record?: StudentAttendance | StaffAttendance) => {
    if (record) {
      setSelectedRecord(record);
      if (activeTab === 'student') {
        const studentRec = record as StudentAttendance;
        setStudentFormData({
          studentId: studentRec.studentId,
          // sectionId removed - auto-detected
          attendanceDate: studentRec.attendanceDate.split('T')[0],
          status: studentRec.status as any,
          reason: studentRec.remarks || '',
        });
        // Set the student info in read-only mode for edit
        const nameParts = studentRec.studentName?.split(' ') || ['', ''];
        setDialogSelectedStudent({
          id: studentRec.studentId,
          firstName: nameParts[0],
          lastName: nameParts.slice(1).join(' '),
          enrollmentNumber: studentRec.studentEnrollmentNumber || '',
          email: '',
          dateOfBirth: new Date().toISOString().split('T')[0],
          isActive: true,
          enrollmentDate: new Date().toISOString().split('T')[0],
        } as any);
      } else {
        const StaffRec = record as StaffAttendance;
        setStaffFormData({
          staffId: StaffRec.staffId,
          attendanceDate: StaffRec.attendanceDate.split('T')[0],
          status: StaffRec.status as any,
          reason: StaffRec.remarks || '',
        });
        // Set the Staff info in read-only mode for edit
        const nameParts = StaffRec.staffName?.split(' ') || ['', ''];
        setDialogSelectedStaff({
          id: StaffRec.staffId,
          firstName: nameParts[0],
          lastName: nameParts.slice(1).join(' '),
          userId: '',
          email: '',
          experienceYears: 0,
          joiningDate: new Date().toISOString(),
          isActive: true,
        } as Staff);
      }
    } else {
      setSelectedRecord(null);
      setDialogSelectedStudent(null);
      setDialogSelectedStaff(null);
      setDialogSearchTerm('');
      setDialogStaffSearchTerm('');
      setStudentFormData({
        studentId: '',
        // sectionId removed - auto-detected
        attendanceDate: new Date().toISOString().split('T')[0],
        status: 'Present',
        reason: '',
      });
      setStaffFormData({
        staffId: '',
        attendanceDate: new Date().toISOString().split('T')[0],
        status: 'Present',
        reason: '',
      });
    }
    setOpenDialog(true);
  };

  const handleCloseDialog = () => {
    setOpenDialog(false);
    setSelectedRecord(null);
    setDialogSelectedStudent(null);
    setDialogSelectedStaff(null);
    setDialogSearchTerm('');
    setDialogStaffSearchTerm('');
    setShowDialogStudentDropdown(false);
    setShowDialogStaffDropdown(false);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (activeTab === 'student') {
      if (selectedRecord) {
        studentUpdateMutation.mutate({
          id: selectedRecord.id,
          data: { ...studentFormData, id: selectedRecord.id },
        });
      } else {
        studentCreateMutation.mutate(studentFormData);
      }
    } else {
      if (selectedRecord) {
        StaffUpdateMutation.mutate({
          id: selectedRecord.id,
          data: { ...StaffFormData, id: selectedRecord.id },
        });
      } else {
        StaffCreateMutation.mutate(StaffFormData);
      }
    }
  };

  const handleDelete = (id: string) => {
    if (confirm('Are you sure you want to delete this attendance record?')) {
      if (activeTab === 'student') {
        studentDeleteMutation.mutate(id);
      } else {
        StaffDeleteMutation.mutate(id);
      }
    }
  };

  const handleNotifyParent = async (record: StudentAttendance, channel: 'SMS' | 'WhatsApp') => {
    if (!record.guardianPhone) {
      toast.error('No guardian phone number found for this student');
      return;
    }

    const templateName = channel === 'WhatsApp' ? 'ATTENDANCE_ABSENT_WA' : 'ATTENDANCE_ABSENT_SMS';
    
    try {
      const result = await notificationApi.sendNotification({
        templateName,
        recipientPhone: record.guardianPhone,
        placeholders: {
          'StudentName': record.studentName || 'Student',
          'Date': new Date(record.attendanceDate).toLocaleDateString(),
          'Status': record.status,
          'SchoolName': 'Our School'
        },
        relatedEntityType: 'StudentAttendance',
        relatedEntityId: record.id
      });

      if (result.success) {
        toast.success(`Notification sent via ${channel}`);
      } else {
        toast.error(result.errorMessage || `Failed to send ${channel} notification`);
      }
    } catch (error) {
      toast.error(`Failed to trigger notification: ${error instanceof Error ? error.message : 'Unknown error'}`);
    }
  };

  const handleChangePage = (newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLSelectElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const attendanceData = activeTab === 'student' ? studentAttendanceData : StaffAttendanceData;
  const isLoading = activeTab === 'student' ? studentLoading : StaffLoading;
  const totalPages = attendanceData ? Math.ceil(attendanceData.totalCount / rowsPerPage) : 0;

  const parseCalendarMonth = (value: string) => {
    const [yearText, monthText] = value.split('-');
    const year = Number(yearText);
    const monthIndex = Number(monthText) - 1;
    const start = new Date(year, monthIndex, 1);
    const end = new Date(year, monthIndex + 1, 0);
    // Format dates as YYYY-MM-DD strings
    const formatDate = (date: Date) => {
      const y = date.getFullYear();
      const m = String(date.getMonth() + 1).padStart(2, '0');
      const d = String(date.getDate()).padStart(2, '0');
      return `${y}-${m}-${d}`;
    };
    return { 
      year, 
      monthIndex, 
      start, 
      end, 
      startStr: formatDate(start), 
      endStr: formatDate(end) 
    };
  };

  const calendarRange = parseCalendarMonth(calendarMonth);

  const { data: studentCalendarData, isLoading: studentCalendarLoading } = useQuery({
    queryKey: ['studentAttendanceCalendar', selectedStudent?.id, calendarMonth, activeTab],
    enabled: viewMode === 'calendar' && activeTab === 'student' && !!selectedStudent?.id,
    queryFn: async () => {
      const result = await attendanceApi.getAllStudentAttendance({
        pageNumber: 1,
        pageSize: 500,
        studentId: selectedStudent?.id,
        startDate: calendarRange.startStr,
        endDate: calendarRange.endStr,
      });
      return result;
    },
  });

  const { data: StaffCalendarData, isLoading: StaffCalendarLoading } = useQuery({
    queryKey: ['StaffAttendanceCalendar', selectedStaff?.id, calendarMonth, activeTab],
    enabled: viewMode === 'calendar' && activeTab === 'Staff' && !!selectedStaff?.id,
    queryFn: async () => {
      const result = await attendanceApi.getAllStaffAttendance({
        pageNumber: 1,
        pageSize: 500,
        staffId: selectedStaff?.id,
        startDate: calendarRange.startStr,
        endDate: calendarRange.endStr,
      });
      return result;
    },
  });

  // Fetch holidays for calendar month
  const { data: holidaysData } = useQuery({
    queryKey: ['holidays', calendarMonth],
    enabled: viewMode === 'calendar',
    queryFn: async () => {
      const [year, month] = calendarMonth.split('-').map(Number);
      const result = await holidayApi.getHolidaysByMonth(year, month);
      return result;
    },
  });

  const calendarRecords = activeTab === 'student'
    ? studentCalendarData?.items || []
    : StaffCalendarData?.items || [];
  const calendarLoading = activeTab === 'student' ? studentCalendarLoading : StaffCalendarLoading;

  const getStatusColor = (status: string) => {
    // Normalize status to lowercase for comparison
    const normalizedStatus = status?.toLowerCase();
    switch (normalizedStatus) {
      case 'present':
        return 'bg-green-100 text-green-800';
      case 'absent':
        return 'bg-red-100 text-red-800';
      case 'late':
        return 'bg-yellow-100 text-yellow-800';
      case 'leave':
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const getCalendarStatusStyle = (status?: string) => {
    // Normalize status to lowercase for comparison
    const normalizedStatus = status?.toLowerCase();
    switch (normalizedStatus) {
      case 'present':
        return 'bg-gradient-to-br from-green-100 to-emerald-200 text-green-800 border-green-300 shadow-md';
      case 'absent':
        return 'bg-gradient-to-br from-red-100 to-rose-200 text-red-800 border-red-300 shadow-md';
      case 'late':
        return 'bg-gradient-to-br from-yellow-100 to-amber-200 text-yellow-800 border-yellow-300 shadow-md';
      case 'leave':
        return 'bg-gradient-to-br from-blue-100 to-cyan-200 text-blue-800 border-blue-300 shadow-md';
      case 'unexcused':
        return 'bg-gradient-to-br from-orange-100 to-orange-200 text-orange-800 border-orange-300 shadow-md';
      default:
        return 'bg-gradient-to-br from-gray-50 to-gray-100 text-gray-600 border-gray-300 shadow-sm';
    }
  };

  const getStatusDisplay = (status?: string) => {
    // Normalize status to lowercase for comparison, but display with proper case
    const normalizedStatus = status?.toLowerCase();
    switch (normalizedStatus) {
      case 'present':
        return { icon: '✓', text: 'Present', color: 'text-green-800' };
      case 'absent':
        return { icon: '✗', text: 'Absent', color: 'text-red-800' };
      case 'late':
        return { icon: '⏰', text: 'Late', color: 'text-yellow-800' };
      case 'leave':
        return { icon: '📋', text: 'Leave', color: 'text-blue-800' };
      case 'unexcused':
        return { icon: '⚠', text: 'Unexcused', color: 'text-orange-800' };
      default:
        return { icon: '—', text: 'No record', color: 'text-gray-600' };
    }
  };

  // Format date as YYYY-MM-DD without timezone conversion
  const calendarDateKey = (date: Date) => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  };
  
  const calendarLookup = new Map(
    calendarRecords.map((record) => {
      // Extract just the date part from the API response (YYYY-MM-DD)
      const dateStr = record.attendanceDate.split('T')[0];
      return [dateStr, record];
    })
  );

  // Create holiday lookup by date
  const holidayLookup = new Map(
    (holidaysData || []).map((holiday) => {
      // Extract date part (YYYY-MM-DD)
      const dateStr = holiday.holidayDate.split('T')[0];
      return [dateStr, holiday];
    })
  );
  
  const calendarDays = Array.from(
    { length: calendarRange.end.getDate() },
    (_, index) => new Date(calendarRange.year, calendarRange.monthIndex, index + 1)
  );
  const calendarStartOffset = new Date(calendarRange.year, calendarRange.monthIndex, 1).getDay();
  const calendarCells = [
    ...Array.from({ length: calendarStartOffset }, () => null),
    ...calendarDays,
  ];
  const weekDayLabels = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  const calendarLegend = [
    { label: '✓ Present', className: 'bg-gradient-to-r from-green-100 to-emerald-200 text-green-800 border-green-300' },
    { label: '✗ Absent', className: 'bg-gradient-to-r from-red-100 to-rose-200 text-red-800 border-red-300' },
    { label: '⏰ Late', className: 'bg-gradient-to-r from-yellow-100 to-amber-200 text-yellow-800 border-yellow-300' },
    { label: '📋 Leave', className: 'bg-gradient-to-r from-blue-100 to-cyan-200 text-blue-800 border-blue-300' },
    { label: '🏖️ Holiday', className: 'bg-gradient-to-r from-purple-100 to-pink-200 text-purple-900 border-purple-400' },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-4 sm:p-6 lg:p-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl sm:text-4xl font-bold text-gray-900 mb-2">📊 Attendance Management - {activeYear?.name || "Loading..."}</h1>
          <p className="text-gray-600 mt-1">Track student and Staff attendance records for the current session</p>
        </div>

        {/* Tabs */}
        <div className="bg-white rounded-lg shadow-md p-4 mb-6 border-b border-gray-200">
          <div className="flex gap-4">
            <button
              onClick={() => {
                setActiveTab('student');
                setPage(0);
              }}
              className={`px-6 py-3 font-medium rounded-lg transition-colors ${
                activeTab === 'student'
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              👨‍🎓 Student Attendance
            </button>
            <button
              onClick={() => {
                setActiveTab('Staff');
                setPage(0);
              }}
              className={`px-6 py-3 font-medium rounded-lg transition-colors ${
                activeTab === 'Staff'
                  ? 'bg-blue-600 text-white'
                  : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              👨‍🏫 Staff Attendance
            </button>
          </div>
        </div>

        {/* Filter and Action Bar */}
        <div className="bg-white rounded-lg shadow-md p-4 mb-6">
          {/* Filters Row */}
          <div className="flex flex-col lg:flex-row gap-4 mb-4">
            {activeTab === 'student' ? (
              <div className="flex-1 relative" ref={dropdownRef}>
                <label className="text-sm font-medium text-gray-700 mb-1 block">Search Student</label>
                <input
                  type="text"
                  placeholder="Type name or enrollment number..."
                  value={selectedStudent ? `${selectedStudent.firstName} ${selectedStudent.lastName} (${selectedStudent.enrollmentNumber})` : searchTerm}
                  onChange={(e) => {
                    setSearchTerm(e.target.value);
                    setSelectedStudent(null);
                    setShowStudentDropdown(true);
                    setPage(0);
                  }}
                  onFocus={() => setShowStudentDropdown(true)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                {selectedStudent && (
                  <button
                    onClick={() => {
                      setSelectedStudent(null);
                      setSearchTerm('');
                      setPage(0);
                    }}
                    className="absolute right-3 top-9 text-gray-400 hover:text-gray-600"
                  >
                    ✕
                  </button>
                )}
                {showStudentDropdown && searchTerm.length >= 2 && studentsData && studentsData.items.length > 0 && !selectedStudent && (
                  <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                    {studentsData.items.map((student) => (
                      <button
                        key={student.id}
                        onClick={() => {
                          setSelectedStudent(student);
                          setSearchTerm('');
                          setShowStudentDropdown(false);
                          setPage(0);
                        }}
                        className="w-full text-left px-4 py-2 hover:bg-blue-50 flex justify-between items-center"
                      >
                        <span className="font-medium">{student.firstName} {student.lastName}</span>
                        <span className="text-sm text-gray-500">{student.enrollmentNumber}</span>
                      </button>
                    ))}
                  </div>
                )}
                {selectedStudent && (
                  <p className="text-sm text-blue-600 mt-1">Showing attendance for {selectedStudent.firstName} {selectedStudent.lastName}</p>
                )}
              </div>
            ) : (
              <div className="flex-1 relative" ref={StaffDropdownRef}>
                <label className="text-sm font-medium text-gray-700 mb-1 block">Search Staff</label>
                <input
                  type="text"
                  placeholder="Type name..."
                  value={selectedStaff ? `${selectedStaff.firstName} ${selectedStaff.lastName}` : StaffSearchTerm}
                  onChange={(e) => {
                    setStaffSearchTerm(e.target.value);
                    setSelectedStaff(null);
                    setShowStaffDropdown(true);
                    setPage(0);
                  }}
                  onFocus={() => setShowStaffDropdown(true)}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                {selectedStaff && (
                  <button
                    onClick={() => {
                      setSelectedStaff(null);
                      setStaffSearchTerm('');
                      setPage(0);
                    }}
                    className="absolute right-3 top-9 text-gray-400 hover:text-gray-600"
                  >
                    ✕
                  </button>
                )}
                {showStaffDropdown && StaffSearchTerm.length >= 2 && StaffsData && StaffsData.items.length > 0 && !selectedStaff && (
                  <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                    {StaffsData.items.map((Staff) => (
                      <button
                        key={Staff.id}
                        onClick={() => {
                          setSelectedStaff(Staff);
                          setStaffSearchTerm('');
                          setShowStaffDropdown(false);
                          setPage(0);
                        }}
                        className="w-full text-left px-3 py-2 hover:bg-blue-50 flex items-center gap-3 border-b border-gray-100 last:border-b-0"
                      >
                        {Staff.imagePath ? (
                          <div className="flex-shrink-0 h-8 w-8 rounded-full overflow-hidden bg-gray-100">
                            <img
                              src={`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${Staff.imagePath}`}
                              alt={`${Staff.firstName} ${Staff.lastName}`}
                              className="w-full h-full object-cover"
                            />
                          </div>
                        ) : (
                          <div className="flex-shrink-0 h-8 w-8 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-bold text-xs">
                            {Staff.firstName[0]}{Staff.lastName[0]}
                          </div>
                        )}
                        <div className="flex-1">
                          <p className="font-medium text-gray-900 text-sm">{Staff.firstName} {Staff.lastName}</p>
                          <p className="text-xs text-gray-500">{Staff.email}</p>
                        </div>
                      </button>
                    ))}
                  </div>
                )}
                {selectedStaff && (
                  <p className="text-sm text-blue-600 mt-1">Showing attendance for {selectedStaff.firstName} {selectedStaff.lastName}</p>
                )}
              </div>
            )}
            
            <div className="w-full lg:w-48">
              <label className="text-sm font-medium text-gray-700 mb-1 block">Filter by Month</label>
              <input
                type="month"
                value={calendarMonth}
                onChange={(e) => {
                  setCalendarMonth(e.target.value);
                  setPage(0);
                }}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>

          {/* Actions Row */}
          <div className="flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-4 pt-4 border-t border-gray-200">
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => setViewMode('list')}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition ${
                  viewMode === 'list'
                    ? 'bg-blue-600 text-white shadow'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                📋 List View
              </button>
              <button
                type="button"
                onClick={() => setViewMode('calendar')}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition ${
                  viewMode === 'calendar'
                    ? 'bg-blue-600 text-white shadow'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                📅 Calendar View
              </button>
            </div>
            
            <button
              onClick={() => handleOpenDialog()}
              className="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition whitespace-nowrap shadow-md hover:shadow-lg"
            >
              + Record Attendance
            </button>
          </div>
        </div>

        {viewMode === 'calendar' && (
          <div className="bg-gradient-to-br from-blue-50 via-white to-indigo-50 rounded-xl shadow-lg p-6 mb-6 border border-blue-100">
            {/* Calendar Header */}
            <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 mb-6 pb-4 border-b border-blue-200">
              <div className="flex items-center gap-3">
                <div className="p-3 bg-blue-600 rounded-lg shadow-md">
                  <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                </div>
                <div>
                  <h2 className="text-2xl font-bold text-gray-900">Monthly Attendance</h2>
                  <p className="text-sm text-gray-600 mt-0.5">
                    {activeTab === 'student'
                      ? (selectedStudent ? `${selectedStudent.firstName} ${selectedStudent.lastName}` : 'Select a student to view the calendar')
                      : (selectedStaff ? `${selectedStaff.firstName} ${selectedStaff.lastName}` : 'Select a Staff to view the calendar')}
                  </p>
                </div>
              </div>
              
              {/* Legend */}
              <div className="flex flex-wrap items-center gap-2">
                {calendarLegend.map((item) => (
                  <div key={item.label} className={`px-3 py-1.5 rounded-lg text-xs font-semibold shadow-sm transition-transform hover:scale-105 ${item.className}`}>
                    {item.label}
                  </div>
                ))}
              </div>
            </div>

            {/* Calendar Content */}
            {(!selectedStudent && activeTab === 'student') || (!selectedStaff && activeTab === 'Staff') ? (
              <div className="text-center py-16">
                <div className="inline-flex items-center justify-center w-20 h-20 bg-gradient-to-br from-blue-100 to-indigo-100 rounded-full mb-4">
                  <svg className="w-10 h-10 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                  </svg>
                </div>
                <p className="text-gray-500 text-lg font-medium">Select a {activeTab} to view monthly attendance</p>
              </div>
            ) : calendarLoading ? (
              <div className="text-center py-16">
                <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4"></div>
                <p className="text-gray-500">Loading calendar...</p>
              </div>
            ) : (
              <>
                {/* Weekday Headers */}
                <div className="grid grid-cols-7 gap-3 mb-3">
                  {weekDayLabels.map((label) => (
                    <div key={label} className="text-center py-3 bg-gradient-to-br from-blue-600 to-indigo-600 text-white font-bold text-sm rounded-lg shadow-md">
                      {label}
                    </div>
                  ))}
                </div>
                
                {/* Calendar Grid */}
<div className="grid grid-cols-7 gap-3">
                  {calendarCells.map((date, index) => {
                    if (!date) {
                      return (
                        <div
                          key={`empty-${index}`}
                          className="h-28 rounded-xl border-2 border-dashed border-gray-200 bg-gray-50/50"
                        ></div>
                      );
                    }

                    const dateKey = calendarDateKey(date);
                    const record = calendarLookup.get(dateKey);
                    const holiday = holidayLookup.get(dateKey);
                    const status = record?.status;
                    const isToday = date.toDateString() === new Date().toDateString();
                    const statusDisplay = getStatusDisplay(status);

                    // If it's a holiday, use special styling
                    const cellStyle = holiday
                      ? 'bg-gradient-to-br from-purple-100 to-pink-200 text-purple-900 border-purple-400 shadow-md'
                      : getCalendarStatusStyle(status);

                    return (
                      <div
                        key={dateKey}
                        className={`h-28 rounded-xl border-2 p-3 text-left transition-all duration-200 hover:shadow-lg hover:scale-105 cursor-pointer ${
                          isToday ? 'ring-2 ring-blue-400 ring-offset-2' : ''
                        } ${cellStyle}`}
                      >
                        <div className="flex items-center justify-between mb-2">
                          <div className={`text-sm font-bold ${isToday ? 'text-blue-600' : holiday ? 'text-purple-700' : 'text-gray-700'}`}>
                            {date.getDate()}
                          </div>
                          {isToday && (
                            <div className="w-2 h-2 bg-blue-600 rounded-full animate-pulse"></div>
                          )}
                        </div>
                        
                        {holiday ? (
                          <div className="space-y-1">
                            <div className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-xs font-bold bg-purple-600 text-white">
                              <span>🏖️</span>
                              <span>Holiday</span>
                            </div>
                            <div className="text-[10px] text-purple-900 line-clamp-2 leading-tight font-bold">
                              {holiday.name}
                            </div>
                            {holiday.type && (
                              <div className="text-[9px] text-purple-700 font-medium">
                                {holiday.type}
                              </div>
                            )}
                          </div>
                        ) : status ? (
                          <div className="space-y-1">
                            <div className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-xs font-bold ${statusDisplay.color}`}>
                              <span>{statusDisplay.icon}</span>
                              <span>{statusDisplay.text}</span>
                            </div>
                            {record?.reason && (
                              <div className="text-[10px] text-gray-700 line-clamp-2 leading-tight font-medium">
                                {record.reason}
                              </div>
                            )}
                          </div>
                        ) : (
                          <div className="text-xs text-gray-500 font-medium">{statusDisplay.icon} {statusDisplay.text}</div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </>
            )}
          </div>
        )}

        {viewMode === 'list' && (
          <>
            {/* Desktop Table View */}
            <div className="hidden lg:block bg-white rounded-lg shadow-md overflow-hidden">
              {isLoading ? (
                <div className="p-8 text-center text-gray-500">Loading attendance records...</div>
              ) : attendanceData?.items.length === 0 ? (
                <div className="p-8 text-center text-gray-500">No attendance records found</div>
              ) : (
                <>
                  <table className="w-full">
                    <thead className="bg-gray-50 border-b border-gray-200">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                          {activeTab === 'student' ? 'Enrollment No' : 'Email'}
                        </th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Remarks</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                      {attendanceData?.items.map((record) => (
                        <tr key={record.id} className="hover:bg-gray-50">
                          <td className="px-6 py-4 text-sm font-medium text-gray-900">
                            {activeTab === 'student' ? (record as StudentAttendance).studentName : (record as StaffAttendance).staffName}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-600">
                            {activeTab === 'student' ? ((record as StudentAttendance).studentEnrollmentNumber || (record as StudentAttendance).studentId) : ((record as StaffAttendance).staffEmail || '-')}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-600">
                            {new Date(record.attendanceDate).toLocaleDateString()}
                          </td>
                          <td className="px-6 py-4 text-sm">
                            <span className={`px-3 py-1 rounded-full text-xs font-semibold ${getStatusColor(record.status)}`}>
                              {record.status}
                            </span>
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-600">{record.reason || record.remarks || '-'}</td>
                          <td className="px-6 py-4 text-sm">
                            <div className="flex gap-2">
                              <button
                                onClick={() => handleOpenDialog(record)}
                                className="px-3 py-1 bg-blue-100 hover:bg-blue-200 text-blue-700 rounded transition text-xs font-medium"
                              >
                                Edit
                              </button>
                              <button
                                onClick={() => handleDelete(record.id)}
                                className="px-3 py-1 bg-red-100 hover:bg-red-200 text-red-700 rounded transition text-xs font-medium"
                              >
                                Delete
                              </button>
                              {activeTab === 'student' && record.status.toLowerCase() === 'absent' && (
                                <>
                                  <button
                                    onClick={() => handleNotifyParent(record as StudentAttendance, 'SMS')}
                                    className="p-1 text-orange-600 hover:bg-orange-50 rounded transition"
                                    title="Notify Parent (SMS)"
                                  >
                                    📱
                                  </button>
                                  <button
                                    onClick={() => handleNotifyParent(record as StudentAttendance, 'WhatsApp')}
                                    className="p-1 text-green-600 hover:bg-green-50 rounded transition"
                                    title="Notify Parent (WhatsApp)"
                                  >
                                    <WhatsAppIcon size={16} className="text-[#25D366]" />
                                  </button>
                                </>
                              )}
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                  {/* Pagination */}
                  <div className="bg-gray-50 border-t border-gray-200 px-6 py-4 flex items-center justify-between">
                    <div className="text-sm text-gray-600">
                      Page {page + 1} of {totalPages} (Total: {attendanceData?.totalCount} records)
                    </div>
                    <div className="flex gap-2 items-center">
                      <button
                        onClick={() => handleChangePage(page - 1)}
                        disabled={page === 0}
                        className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        Previous
                      </button>
                      <select
                        value={rowsPerPage}
                        onChange={handleChangeRowsPerPage}
                        className="px-3 py-2 border border-gray-300 rounded"
                      >
                        <option value={5}>5</option>
                        <option value={10}>10</option>
                        <option value={25}>25</option>
                        <option value={50}>50</option>
                      </select>
                      <button
                        onClick={() => handleChangePage(page + 1)}
                        disabled={page >= totalPages - 1}
                        className="px-4 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        Next
                      </button>
                    </div>
                  </div>
                </>
              )}
            </div>

            {/* Mobile Card View */}
            <div className="lg:hidden space-y-4">
              {isLoading ? (
                <div className="p-8 text-center text-gray-500">Loading attendance records...</div>
              ) : attendanceData?.items.length === 0 ? (
                <div className="p-8 text-center text-gray-500">No attendance records found</div>
              ) : (
                <>
                  {attendanceData?.items.map((record) => (
                    <div key={record.id} className="bg-white rounded-lg shadow-md p-4">
                      <div className="flex items-start justify-between mb-3">
                        <div>
                          <h3 className="font-semibold text-gray-900">
                            {activeTab === 'student' ? (record as StudentAttendance).studentName : (record as StaffAttendance).staffName || 'N/A'}
                          </h3>
                          <p className="text-sm text-gray-600">
                            {activeTab === 'student' ? `ID: ${(record as StudentAttendance).studentEnrollmentNumber || (record as StudentAttendance).studentId}` : `Email: ${(record as StaffAttendance).staffEmail || '-'}`}
                          </p>
                        </div>
                        <span className={`px-3 py-1 rounded-full text-xs font-semibold ${getStatusColor(record.status)}`}>
                          {record.status}
                        </span>
                      </div>
                      <div className="text-sm text-gray-600 space-y-1 mb-3">
                        <p>Date: {new Date(record.attendanceDate).toLocaleDateString()}</p>
                        {(record.reason || record.remarks) && <p>Reason: {record.reason || record.remarks}</p>}
                      </div>
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleOpenDialog(record)}
                          className="flex-1 px-3 py-2 bg-blue-100 hover:bg-blue-200 text-blue-700 rounded transition text-sm font-medium"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(record.id)}
                          className="flex-1 px-3 py-2 bg-red-100 hover:bg-red-200 text-red-700 rounded transition text-sm font-medium"
                        >
                          Delete
                        </button>
                        {activeTab === 'student' && record.status.toLowerCase() === 'absent' && (
                          <div className="flex gap-2 ml-auto">
                            <button
                              onClick={() => handleNotifyParent(record as StudentAttendance, 'SMS')}
                              className="p-2 text-orange-600 hover:bg-orange-50 rounded-lg border border-orange-100 transition"
                              title="Notify SMS"
                            >
                              📱
                            </button>
                            <button
                              onClick={() => handleNotifyParent(record as StudentAttendance, 'WhatsApp')}
                              className="p-2 text-green-600 hover:bg-green-50 rounded-lg border border-green-100 transition"
                              title="Notify WhatsApp"
                            >
                              <WhatsAppIcon size={18} className="text-[#25D366]" />
                            </button>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                  {/* Mobile Pagination */}
                  <div className="flex gap-2 justify-between mt-4">
                    <button
                      onClick={() => handleChangePage(page - 1)}
                      disabled={page === 0}
                      className="flex-1 px-3 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
                    >
                      Previous
                    </button>
                    <button
                      onClick={() => handleChangePage(page + 1)}
                      disabled={page >= totalPages - 1}
                      className="flex-1 px-3 py-2 border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
                    >
                      Next
                    </button>
                  </div>
                </>
              )}
            </div>
          </>
        )}
      </div>

      {/* Add/Edit Dialog */}
      {openDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-lg max-w-md w-full">
            <div className="p-6 border-b border-gray-200">
              <h2 className="text-2xl font-bold text-gray-900">
                {selectedRecord ? 'Edit Attendance Record' : 'Record Attendance'}
              </h2>
            </div>
            <form onSubmit={handleSubmit} className="p-6 space-y-4">
              {activeTab === 'student' ? (
                <>
                  {selectedRecord ? (
                    // Edit mode - show student info as read-only
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Student</label>
                      <div className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-700">
                        {dialogSelectedStudent 
                          ? `${dialogSelectedStudent.firstName} ${dialogSelectedStudent.lastName} (${dialogSelectedStudent.enrollmentNumber})`
                          : 'Loading student info...'}
                      </div>
                    </div>
                  ) : (
                    // Create mode - show searchable student dropdown
                    <div className="relative" ref={dialogDropdownRef}>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Search Student</label>
                      <input
                        type="text"
                        placeholder="Type name or enrollment number..."
                        value={dialogSelectedStudent ? `${dialogSelectedStudent.firstName} ${dialogSelectedStudent.lastName} (${dialogSelectedStudent.enrollmentNumber})` : dialogSearchTerm}
                        onChange={(e) => {
                          setDialogSearchTerm(e.target.value);
                          setDialogSelectedStudent(null);
                          setShowDialogStudentDropdown(true);
                        }}
                        onFocus={() => setShowDialogStudentDropdown(true)}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                        required={!dialogSelectedStudent}
                      />
                      {dialogSelectedStudent && (
                        <button
                          type="button"
                          onClick={() => {
                            setDialogSelectedStudent(null);
                            setDialogSearchTerm('');
                            setStudentFormData({ ...studentFormData, studentId: '' });
                          }}
                          className="absolute right-3 top-9 text-gray-400 hover:text-gray-600"
                        >
                          ✕
                        </button>
                      )}
                      {showDialogStudentDropdown && dialogSearchTerm.length >= 2 && dialogStudentsData && dialogStudentsData.items.length > 0 && !dialogSelectedStudent && (
                        <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-lg max-h-60 overflow-y-auto">
                          {dialogStudentsData.items.map((student) => (
                            <button
                              type="button"
                              key={student.id}
                              onClick={() => {
                                setDialogSelectedStudent(student);
                                setDialogSearchTerm('');
                                setShowDialogStudentDropdown(false);
                                setStudentFormData({ ...studentFormData, studentId: student.id });
                              }}
                              className="w-full text-left px-4 py-2 hover:bg-blue-50 flex justify-between items-center"
                            >
                              <span className="font-medium">{student.firstName} {student.lastName}</span>
                              <span className="text-sm text-gray-500">{student.enrollmentNumber}</span>
                            </button>
                          ))}
                        </div>
                      )}
                      {dialogSelectedStudent && (
                        <p className="text-sm text-blue-600 mt-1">✓ Selected: {dialogSelectedStudent.firstName} {dialogSelectedStudent.lastName}</p>
                      )}
                    </div>
                  )}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Date</label>
                    <input
                      type="date"
                      required
                      value={studentFormData.attendanceDate}
                      onChange={(e) => setStudentFormData({ ...studentFormData, attendanceDate: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                    <select
                      required
                      value={studentFormData.status}
                      onChange={(e) => setStudentFormData({ ...studentFormData, status: e.target.value as any })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="Present">Present</option>
                      <option value="Absent">Absent</option>
                      <option value="Late">Late</option>
                      <option value="Leave">Leave</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Reason/Remarks (Optional)</label>
                    <textarea
                      value={studentFormData.reason}
                      onChange={(e) => setStudentFormData({ ...studentFormData, reason: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      rows={3}
                    />
                  </div>
                </>
              ) : (
                <>
                  {selectedRecord ? (
                    // Edit mode - show Staff info as read-only
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Staff</label>
                      <div className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-gray-50 text-gray-700">
                        {(selectedRecord as StaffAttendance)?.staffName || 'Loading Staff info...'}
                      </div>
                    </div>
                  ) : (
                    // Create mode - searchable Staff dropdown
                    <div ref={dialogStaffDropdownRef}>
                      <label className="block text-sm font-medium text-gray-700 mb-1">Staff</label>
                      <div className="relative">
                        <div className="flex items-center gap-2">
                          <input
                            type="text"
                            placeholder="Search Staff by name..."
                            value={dialogStaffSearchTerm}
                            onChange={(e) => {
                              setDialogStaffSearchTerm(e.target.value);
                              setShowDialogStaffDropdown(true);
                              setDialogSelectedStaff(null);
                            }}
                            onFocus={() => setShowDialogStaffDropdown(true)}
                            className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                          />
                          {dialogSelectedStaff && (
                            <button
                              type="button"
                              onClick={() => {
                                setDialogSelectedStaff(null);
                                setDialogStaffSearchTerm('');
                                setStaffFormData({ ...StaffFormData, staffId: '' });
                              }}
                              className="text-gray-500 hover:text-gray-700 text-xl"
                            >
                              ×
                            </button>
                          )}
                        </div>
                        {showDialogStaffDropdown && dialogStaffSearchTerm.length >= 2 && (
                          <div className="absolute top-full left-0 right-0 mt-1 bg-white border border-gray-300 rounded-lg shadow-lg z-10 max-h-40 overflow-y-auto">
                            {dialogStaffsData?.items && dialogStaffsData.items.length > 0 ? (
                              dialogStaffsData.items.map((Staff) => (
                                <div
                                  key={Staff.id}
                                  onClick={() => {
                                    setDialogSelectedStaff(Staff);
                                    setStaffFormData({ ...StaffFormData, staffId: Staff.id });
                                    setDialogStaffSearchTerm('');
                                    setShowDialogStaffDropdown(false);
                                  }}
                                  className="px-3 py-2 cursor-pointer hover:bg-blue-50 border-b border-gray-100 last:border-b-0 flex items-center gap-3"
                                >
                                  {Staff.imagePath ? (
                                    <div className="flex-shrink-0 h-8 w-8 rounded-full overflow-hidden bg-gray-100">
                                      <img
                                        src={`${(import.meta.env.VITE_API_URL || 'http://localhost:5208/api').replace('/api', '')}${Staff.imagePath}`}
                                        alt={`${Staff.firstName} ${Staff.lastName}`}
                                        className="w-full h-full object-cover"
                                      />
                                    </div>
                                  ) : (
                                    <div className="flex-shrink-0 h-8 w-8 bg-gradient-to-br from-blue-500 to-blue-600 rounded-full flex items-center justify-center text-white font-bold text-xs">
                                      {Staff.firstName[0]}{Staff.lastName[0]}
                                    </div>
                                  )}
                                  <div className="flex-1">
                                    <p className="font-medium text-gray-900 text-sm">{Staff.firstName} {Staff.lastName}</p>
                                    <p className="text-xs text-gray-500">{Staff.email}</p>
                                  </div>
                                </div>
                              ))
                            ) : (
                              <div className="px-3 py-2 text-gray-500 text-sm">No Staffs found</div>
                            )}
                          </div>
                        )}
                        {dialogSelectedStaff && (
                          <p className="text-sm text-blue-600 mt-1">✓ Selected: {dialogSelectedStaff.firstName} {dialogSelectedStaff.lastName}</p>
                        )}
                      </div>
                    </div>
                  )}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Date</label>
                    <input
                      type="date"
                      required
                      value={StaffFormData.attendanceDate}
                      onChange={(e) => setStaffFormData({ ...StaffFormData, attendanceDate: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
                    <select
                      required
                      value={StaffFormData.status}
                      onChange={(e) => setStaffFormData({ ...StaffFormData, status: e.target.value as any })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    >
                      <option value="Present">Present</option>
                      <option value="Absent">Absent</option>
                      <option value="Late">Late</option>
                      <option value="Leave">Leave</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">Reason/Remarks (Optional)</label>
                    <textarea
                      value={StaffFormData.reason}
                      onChange={(e) => setStaffFormData({ ...StaffFormData, reason: e.target.value })}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                      rows={3}
                    />
                  </div>
                </>
              )}

              <div className="flex gap-3 pt-4">
                <button
                  type="button"
                  onClick={handleCloseDialog}
                  className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={studentCreateMutation.isPending || studentUpdateMutation.isPending || StaffCreateMutation.isPending || StaffUpdateMutation.isPending}
                  className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {studentCreateMutation.isPending || studentUpdateMutation.isPending || StaffCreateMutation.isPending || StaffUpdateMutation.isPending ? 'Saving...' : 'Save'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

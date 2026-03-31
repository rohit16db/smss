import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import {
  timetableApi,
  classApi,
  StaffApi,
  settingsApi,
  type TimeSlot,
  type CreateTimeSlotDto,
  type CreateTimetableEntryDto,
  type SchoolDto,
  type BulkCopyResultDto
} from '../services/api';
import { useAcademicYear } from '../hooks/useAcademicYear';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';

const DAYS_OF_WEEK = [
  { value: 1, label: 'Monday' },
  { value: 2, label: 'Tuesday' },
  { value: 3, label: 'Wednesday' },
  { value: 4, label: 'Thursday' },
  { value: 5, label: 'Friday' },
  { value: 6, label: 'Saturday' },
];

export function TimetablePage() {
  const queryClient = useQueryClient();
  const { activeYear } = useAcademicYear();
  const todayDayIndex = new Date().getDay();

  const [viewMode, setViewMode] = useState<'section' | 'staff'>('section');
  const [selectedClassId, setSelectedClassId] = useState<string>('');
  const [selectedSectionId, setSelectedSectionId] = useState<string>('');
  const [selectedStaffId, setSelectedStaffId] = useState<string>('');
  const [isTimeSlotDialogOpen, setIsTimeSlotDialogOpen] = useState(false);
  const [isEntryDialogOpen, setIsEntryDialogOpen] = useState(false);
  const [selectedDay, setSelectedDay] = useState<number>(1);
  const [selectedSlot, setSelectedSlot] = useState<TimeSlot | null>(null);
  const [currentTime, setCurrentTime] = useState(new Date());
  const [isResultDialogOpen, setIsResultDialogOpen] = useState(false);
  const [bulkSyncResult, setBulkSyncResult] = useState<BulkCopyResultDto | null>(null);

  // Update current time every minute to refresh "active" slot highlights
  useMemo(() => {
    const timer = setInterval(() => setCurrentTime(new Date()), 60000);
    return () => clearInterval(timer);
  }, []);

  const isSlotActive = (day: number, start: string, end: string) => {
    const dayOfWeek = currentTime.getDay();
    if (dayOfWeek !== day) return false;

    const nowStr = currentTime.toTimeString().slice(0, 8);
    return nowStr >= start && nowStr <= end;
  };

  // Queries
  const { data: classes } = useQuery({
    queryKey: ['classes'],
    queryFn: () => classApi.getAll({ pageSize: 100 }),
  });

  const { data: sections } = useQuery({
    queryKey: ['sections', selectedClassId],
    queryFn: () => classApi.getSectionsByClass(selectedClassId),
    enabled: !!selectedClassId,
  });

  const { data: schoolInfo } = useQuery<SchoolDto>({
    queryKey: ['schoolSettings'],
    queryFn: () => settingsApi.getSchoolSettings(),
  });

  const { data: allStaff } = useQuery({
    queryKey: ['staff'],
    queryFn: () => StaffApi.getAll(),
    enabled: viewMode === 'staff',
  });

  const { data: timeSlots, isLoading: isLoadingSlots } = useQuery({
    queryKey: ['timeSlots', activeYear?.id],
    queryFn: () => timetableApi.getTimeSlots(activeYear!.id),
    enabled: !!activeYear,
  });

  const { data: entries, isLoading: isLoadingEntries } = useQuery({
    queryKey: ['timetableEntries', viewMode, selectedSectionId, selectedStaffId, activeYear?.id],
    queryFn: () => viewMode === 'section'
      ? timetableApi.getSectionTimetable(selectedSectionId, activeYear!.id)
      : timetableApi.getStaffTimetable(selectedStaffId, activeYear!.id),
    enabled: (viewMode === 'section' ? !!selectedSectionId : !!selectedStaffId) && !!activeYear,
  });

  const { data: staffAssignments, isLoading: isLoadingAssignments } = useQuery({
    queryKey: ['staffAssignments', selectedSectionId, activeYear?.id],
    queryFn: () => StaffApi.getAssignmentsBySection(selectedSectionId, activeYear!.id),
    enabled: viewMode === 'section' && !!selectedSectionId && !!activeYear,
  });

  const isGridLoading = isLoadingSlots || (viewMode === 'section' ? (!!selectedSectionId && (isLoadingEntries || isLoadingAssignments)) : (!!selectedStaffId && isLoadingEntries));

  // Mutations
  const createSlotMutation = useMutation({
    mutationFn: timetableApi.createTimeSlot,
    onSuccess: () => {
      toast.success('Time slot created');
      queryClient.invalidateQueries({ queryKey: ['timeSlots'] });
      setIsTimeSlotDialogOpen(false);
    },
    onError: (error: any) => toast.error(error.response?.data || 'Failed to create slot'),
  });

  const createEntryMutation = useMutation({
    mutationFn: timetableApi.createEntry,
    onSuccess: () => {
      toast.success('Assignment added');
      queryClient.invalidateQueries({ queryKey: ['timetableEntries'] });
      setIsEntryDialogOpen(false);
    },
    onError: (error: any) => toast.error(error.response?.data || 'Scheduling conflict detected'),
  });

  const deleteEntryMutation = useMutation({
    mutationFn: timetableApi.deleteEntry,
    onSuccess: () => {
      toast.success('Assignment removed');
      queryClient.invalidateQueries({ queryKey: ['timetableEntries'] });
    },
  });

  const exportMutation = useMutation({
    mutationFn: () => viewMode === 'section'
      ? timetableApi.exportSectionTimetable(selectedSectionId, activeYear!.id)
      : timetableApi.exportStaffTimetable(selectedStaffId, activeYear!.id),
    onSuccess: (data) => {
      const url = window.URL.createObjectURL(new Blob([data]));
      const link = document.createElement('a');
      link.href = url;
      const fileName = viewMode === 'section' ? 'Section_Timetable.pdf' : 'Staff_Timetable.pdf';
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      link.remove();
      toast.success('PDF generated successfully');
    },
    onError: () => toast.error('Failed to export PDF'),
  });

  const syncSlotsMutation = useMutation({
    mutationFn: (sourceDay: number) => timetableApi.bulkCreateTimeSlots({
      academicYearId: activeYear!.id,
      sourceDay,
      targetDays: DAYS_OF_WEEK.map(d => d.value).filter(v => v !== sourceDay)
    }),
    onSuccess: () => {
      toast.success('Day structure synced to all other days');
      queryClient.invalidateQueries({ queryKey: ['timeSlots'] });
    },
    onError: () => toast.error('Failed to sync slots')
  });

  const syncRoutineMutation = useMutation({
    mutationFn: (sourceDay: number) => timetableApi.bulkCopyRoutine({
      academicYearId: activeYear!.id,
      sectionId: viewMode === 'section' ? selectedSectionId : undefined,
      staffId: viewMode === 'staff' ? selectedStaffId : undefined,
      sourceDay,
      targetDays: DAYS_OF_WEEK.map(d => d.value).filter(v => v !== sourceDay)
    }),
    onSuccess: (result) => {
      setBulkSyncResult(result);
      if (result.errors.length > 0) {
        setIsResultDialogOpen(true);
      } else {
        toast.success(`Routine duplicated successfully! (${result.successCount} slots)`);
      }
      queryClient.invalidateQueries({ queryKey: ['timetableEntries'] });
      queryClient.invalidateQueries({ queryKey: ['timeSlots'] });
    },
    onError: () => toast.error('Failed to sync routine')
  });

  const getEntryFor = (day: number, timeSlotId: string) => {
    return entries?.find(e => e.dayOfWeek === day && e.timeSlotId === timeSlotId);
  };

  // Group slots by time/name for rows
  const timeRows = useMemo(() => {
    if (!timeSlots) return [];

    const groups: Record<string, TimeSlot[]> = {};
    timeSlots.forEach(slot => {
      // Normalize times to HH:mm for reliable grouping
      const start = slot.startTime.slice(0, 5);
      const end = slot.endTime.slice(0, 5);
      const key = `${start}-${end}-${slot.name.trim()}`;
      if (!groups[key]) groups[key] = [];
      groups[key].push(slot);
    });

    return Object.values(groups).sort((a, b) => a[0].startTime.localeCompare(b[0].startTime));
  }, [timeSlots]);

  const sortedSlots = useMemo(() => {
    if (!timeSlots) return [];
    return [...timeSlots].sort((a, b) => {
      const dayDiff = a.dayOfWeek - b.dayOfWeek;
      if (dayDiff !== 0) return dayDiff;
      return a.startTime.localeCompare(b.startTime);
    });
  }, [timeSlots]);

  return(
    <div className = "min-h-screen bg-gradient-to-br from-slate-50 to-slate-100" >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Print Header (Visible only on print) */}
          {schoolInfo && (
            <div className="hidden print:block border-b-2 border-blue-600 pb-6 mb-8">
              <div className="flex justify-between items-start">
                <div className="flex gap-6 items-center">
                  {schoolInfo.logoBase64 && (
                    <img
                      src={`data:image/png;base64,${schoolInfo.logoBase64}`}
                      alt="School Logo"
                      className="w-20 h-20 object-contain rounded-xl shadow-sm"
                    />
                  )}
                  <div>
                    <h1 className="text-3xl font-extrabold text-blue-900 tracking-tight">{schoolInfo.name}</h1>
                    <p className="text-slate-600 text-sm mt-1 max-w-md font-medium leading-relaxed">{schoolInfo.address}</p>
                    <div className="flex gap-4 mt-2 text-xs font-semibold text-slate-500 uppercase tracking-wider">
                      <span>{schoolInfo.phoneNumber}</span>
                      <span>•</span>
                      <span>{schoolInfo.emailAddress}</span>
                    </div>
                  </div>
                </div>
                <div className="text-right">
                  <h2 className="text-xl font-bold text-blue-800 uppercase tracking-widest italic font-serif">
                    Weekly Timetable
                  </h2>
                  <p className="text-slate-500 text-xs mt-1 font-bold">Academic Year: {activeYear?.name}</p>
                  <div className="mt-2 inline-block px-3 py-1 bg-blue-50 border border-blue-100 rounded-lg text-blue-700 text-sm font-bold shadow-sm">
                    {viewMode === 'section'
                      ? `Class: ${classes?.items.find(c => c.id === selectedClassId)?.name || ''} - ${sections?.find(s => s.id === selectedSectionId)?.sectionName || ''}`
                      : `Teacher: ${allStaff?.items.find(s => s.id === selectedStaffId)?.firstName} ${allStaff?.items.find(s => s.id === selectedStaffId)?.lastName}`
                    }
                  </div>
                </div>
              </div>
            </div>
          )}
          {/* Header */}
          <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4 print:hidden">
            <div>
              <h1 className="text-4xl font-black bg-gradient-to-r from-blue-700 via-blue-800 to-indigo-900 bg-clip-text text-transparent tracking-tight">
                Academic Timetable
              </h1>
              <p className="text-slate-500 mt-2 font-medium">Manage and view weekly instruction schedules</p>
            </div>

              <div className="flex items-center gap-3 w-full lg:w-auto">
                <button
                  onClick={() => exportMutation.mutate()}
                  disabled={exportMutation.isPending || (viewMode === 'section' ? !selectedSectionId : !selectedStaffId)}
                  className="flex items-center justify-center gap-2 px-5 py-3 bg-white border border-slate-200 text-slate-700 rounded-xl hover:bg-slate-50 hover:border-blue-300 hover:text-blue-700 hover:shadow-md transition-all duration-300 font-bold disabled:opacity-40 disabled:cursor-not-allowed group min-w-[140px]"
                >
                  {exportMutation.isPending ? (
                    <div className="w-5 h-5 border-2 border-slate-300 border-t-blue-600 rounded-full animate-spin" />
                  ) : (
                    <svg className="w-5 h-5 text-slate-400 group-hover:text-blue-500 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M4 16v1a2 2 0 002 2h12a2 2 0 002-2v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                    </svg>
                  )}
                  {exportMutation.isPending ? 'Generating...' : 'Export PDF'}
                </button>

                <button
                  onClick={() => {
                    const sourceDay = todayDayIndex === 0 || todayDayIndex === 7 ? 1 : todayDayIndex;
                    if (confirm(`Duplicate entire ${DAYS_OF_WEEK.find(d => d.value === sourceDay)?.label} routine to the rest of the week?`)) {
                      syncRoutineMutation.mutate(sourceDay);
                    }
                  }}
                  disabled={syncRoutineMutation.isPending || (viewMode === 'section' ? !selectedSectionId : !selectedStaffId)}
                  className="flex items-center justify-center gap-2 px-5 py-3 bg-white border border-slate-200 text-slate-700 rounded-xl hover:bg-slate-50 hover:border-indigo-300 hover:text-indigo-700 hover:shadow-md transition-all duration-300 font-bold disabled:opacity-40 disabled:cursor-not-allowed group min-w-[140px]"
                >
                  {syncRoutineMutation.isPending ? (
                    <div className="w-5 h-5 border-2 border-slate-300 border-t-indigo-600 rounded-full animate-spin" />
                  ) : (
                    <svg className="w-5 h-5 text-indigo-400 group-hover:text-indigo-600 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
                    </svg>
                  )}
                  {syncRoutineMutation.isPending ? 'Syncing...' : 'Sync Routine'}
                </button>

                <button
                  onClick={() => setIsTimeSlotDialogOpen(true)}
                  className="flex items-center justify-center gap-2 px-6 py-3 bg-gradient-to-br from-blue-600 to-indigo-700 text-white rounded-xl hover:shadow-xl hover:scale-[1.02] active:scale-95 transition-all duration-300 font-bold shadow-lg shadow-blue-500/20"
                >
                  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                  Configure Slots
                </button>
              </div>
            </div>

            {/* View Mode Toggle & Filter */}
            <div className="bg-white rounded-3xl shadow-xl shadow-slate-200/50 border border-slate-100 p-2 print:hidden">
              <div className="flex flex-col md:flex-row gap-2">
                {/* Mode Tabs */}
                <div className="bg-slate-100 p-1.5 rounded-2xl flex gap-1">
                  <button
                    onClick={() => setViewMode('section')}
                    className={`flex-1 px-6 py-2.5 rounded-xl text-sm font-bold transition-all duration-300 ${viewMode === 'section' ? 'bg-white text-blue-700 shadow-md ring-1 ring-black/5' : 'text-slate-500 hover:text-slate-700 hover:bg-slate-200/50'}`}
                  >
                    Section View
                  </button>
                  <button
                    onClick={() => setViewMode('staff')}
                    className={`flex-1 px-6 py-2.5 rounded-xl text-sm font-bold transition-all duration-300 ${viewMode === 'staff' ? 'bg-white text-blue-700 shadow-md ring-1 ring-black/5' : 'text-slate-500 hover:text-slate-700 hover:bg-slate-200/50'}`}
                  >
                    Teacher View
                  </button>
                </div>

                {/* Dynamic Filter Bar */}
                <div className="flex-1 flex flex-col md:flex-row gap-4 p-2">
                  {viewMode === 'section' ? (
                    <>
                      <div className="flex-1 flex flex-col gap-1.5">
                        <label className="px-1 text-[10px] font-black text-slate-400 uppercase tracking-widest">Target Class</label>
                        <select
                          value={selectedClassId}
                          onChange={(e) => {
                            setSelectedClassId(e.target.value);
                            setSelectedSectionId('');
                          }}
                          className="input-field-new"
                        >
                          <option value="">-- Select Class --</option>
                          {classes?.items.map(cls => (
                            <option key={cls.id} value={cls.id}>{cls.name}</option>
                          ))}
                        </select>
                      </div>
                      <div className="flex-1 flex flex-col gap-1.5">
                        <label className="px-1 text-[10px] font-black text-slate-400 uppercase tracking-widest">Section / Room</label>
                        <select
                          value={selectedSectionId}
                          onChange={(e) => setSelectedSectionId(e.target.value)}
                          disabled={!selectedClassId}
                          className="input-field-new disabled:opacity-40"
                        >
                          <option value="">-- Select Section --</option>
                          {sections?.map(sec => (
                            <option key={sec.id} value={sec.id}>{sec.sectionName}</option>
                          ))}
                        </select>
                      </div>
                    </>
                  ) : (
                    <div className="flex-1 flex flex-col gap-1.5 px-2">
                      <label className="px-1 text-[10px] font-black text-slate-400 uppercase tracking-widest">Select Faculty Member</label>
                      <select
                        value={selectedStaffId}
                        onChange={(e) => setSelectedStaffId(e.target.value)}
                        className="input-field-new"
                      >
                        <option value="">-- Select Teacher --</option>
                        {allStaff?.items.map(s => (
                          <option key={s.id} value={s.id}>{s.firstName} {s.lastName} ({s.designation})</option>
                        ))}
                      </select>
                    </div>
                  )}
                </div>
              </div>
            </div>


            {/* Timetable Grid */}
            {(viewMode === 'section' ? selectedSectionId : selectedStaffId) ? (
              isGridLoading ? (
                <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden p-6">
                  <LoadingSkeleton rows={5} type="table" />
                </div>
              ) : (
                <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300 print:shadow-none print:border-slate-300">
                  <div className="overflow-x-auto">
                    <table className="w-full border-collapse">
                      <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                        <tr>
                          <th className="p-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider border-r border-gray-200 w-32">
                            Time Slot
                          </th>
                          {DAYS_OF_WEEK.map(day => (
                            <th
                              key={day.value}
                              className={`p-4 text-center text-xs font-bold uppercase tracking-wider border-r border-gray-200 transition-colors ${day.value === todayDayIndex ? 'bg-blue-100 text-blue-900 border-b-2 border-b-blue-500 shadow-inner' : 'text-gray-900'}`}
                            >
                              <div className="flex items-center justify-center gap-2">
                                {day.label}
                                {day.value === todayDayIndex && (
                                  <span className="relative flex h-2.5 w-2.5">
                                    <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75"></span>
                                    <span className="relative inline-flex rounded-full h-2.5 w-2.5 bg-blue-500"></span>
                                  </span>
                                )}
                              </div>
                            </th>
                          ))}
                        </tr>
                      </thead>
                      <tbody>
                        {timeRows.length === 0 ? (
                          <tr>
                            <td colSpan={7} className="p-12 text-center text-gray-400">
                              No time slots configured for {activeYear?.name}.
                              <button onClick={() => setIsTimeSlotDialogOpen(true)} className="text-blue-600 hover:underline ml-1">Create one now.</button>
                            </td>
                          </tr>
                        ) : (
                          timeRows.map(rowSlots => {
                            const first = rowSlots[0];
                            return (
                              <tr key={first.id} className="border-b border-gray-200 hover:bg-gray-50 transition-colors">
                                <td className="p-4 border-r border-gray-200">
                                  <div className="font-bold text-gray-900 text-sm">{first.name}</div>
                                  <div className="text-xs text-gray-500 font-mono">{first.startTime.slice(0, 5)} - {first.endTime.slice(0, 5)}</div>
                                  {first.isBreak && <span className="mt-1 inline-block px-1.5 py-0.5 rounded text-[10px] font-bold uppercase bg-orange-100 text-orange-700">Break</span>}
                                </td>

                                {DAYS_OF_WEEK.map(day => {
                                  const daySlot = rowSlots.find(s => s.dayOfWeek === day.value);
                                  const entry = daySlot ? getEntryFor(day.value, daySlot.id) : null;
                                  const isActive = daySlot ? isSlotActive(day.value, daySlot.startTime, daySlot.endTime) : false;

                                  return (
                                    <td
                                      key={`${day.value}-${first.id}`}
                                      className={`p-2 border-r border-gray-200 relative group min-h-[100px] transition-colors duration-200 ${first.isBreak ? 'bg-orange-50/30' : ''} ${day.value === todayDayIndex && !first.isBreak ? 'bg-blue-50/20' : ''} ${isActive ? 'bg-blue-50/50' : ''}`}
                                    >
                                      {first.isBreak ? (
                                        <div className="flex items-center justify-center h-full min-h-[60px]">
                                          <div className="text-center text-orange-400/70 font-bold text-xs italic tracking-widest uppercase">Interval</div>
                                        </div>
                                      ) : entry ? (
                                        <div className={`p-3 rounded-xl border shadow-sm relative group/card hover:shadow-md hover:scale-[1.03] hover:-translate-y-0.5 transition-all duration-300 z-10 hover:z-20 cursor-default flex flex-col h-full min-h-[80px] ring-1 hover:ring-blue-400 ${isActive ? 'bg-gradient-to-br from-blue-100 to-indigo-100 border-blue-400 ring-2 ring-blue-500 shadow-blue-200' : 'bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200 ring-black/5'}`}>
                                          {isActive && (
                                            <div className="absolute -top-2 left-1/2 -translate-x-1/2 px-2 py-0.5 bg-blue-600 text-[8px] font-black text-white rounded-full flex items-center gap-1 shadow-lg animate-bounce z-30">
                                              <span className="w-1.5 h-1.5 bg-white rounded-full animate-pulse"></span>
                                              LIVE NOW
                                            </div>
                                          )}
                                          {viewMode === 'section' ? (
                                            <button
                                              onClick={() => deleteEntryMutation.mutate(entry.id)}
                                              className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full p-1.5 opacity-0 group-hover/card:opacity-100 transition-opacity shadow-lg hover:bg-red-600 hover:scale-110 focus:opacity-100 print:hidden"
                                              title="Remove Assignment"
                                            >
                                              <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M6 18L18 6M6 6l12 12" />
                                              </svg>
                                            </button>
                                          ) : (
                                            <div className="absolute top-2 right-2 px-1.5 py-0.5 rounded bg-blue-600 text-[8px] font-black text-white uppercase tracking-tighter shadow-sm">
                                              Teaching
                                            </div>
                                          )}
                                          <div className="font-bold text-blue-900 text-sm truncate group-hover/card:whitespace-normal group-hover/card:overflow-visible transition-all leading-tight">
                                            {entry.subjectName}
                                          </div>
                                          <div className="text-xs text-blue-700 mt-1.5 flex items-center gap-1.5 opacity-90 font-medium">
                                            <svg className="w-3.5 h-3.5 opacity-70 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                                              <path fillRule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clipRule="evenodd" />
                                            </svg>
                                            <span className="truncate">
                                              {viewMode === 'section' ? entry.staffName : `${entry.className} - ${entry.sectionName}`}
                                            </span>
                                          </div>
                                          {entry.roomNumber && (
                                            <div className="mt-auto pt-2.5 flex items-center gap-1">
                                              <span className="inline-flex items-center gap-1 px-1.5 py-0.5 rounded-md bg-white/60 text-[10px] text-blue-800 font-bold uppercase tracking-wider backdrop-blur-sm border border-blue-200/50 shadow-sm print:bg-slate-50 print:border-slate-300">
                                                <svg className="w-3 h-3 text-blue-500 print:text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                                                </svg>
                                                {entry.roomNumber}
                                              </span>
                                            </div>
                                          )}
                                        </div>
                                      ) : (
                                        <button
                                          onClick={async (e) => {
                                            e.stopPropagation();
                                            if (daySlot) {
                                              setSelectedDay(day.value);
                                              setSelectedSlot(daySlot);
                                              setIsEntryDialogOpen(true);
                                            } else {
                                              // Auto-create slot for this day
                                              try {
                                                const newSlotId = await createSlotMutation.mutateAsync({
                                                  name: first.name,
                                                  dayOfWeek: day.value,
                                                  startTime: first.startTime,
                                                  endTime: first.endTime,
                                                  isBreak: first.isBreak,
                                                  academicYearId: activeYear!.id
                                                });

                                                // Immediately open entry dialog with the new slot
                                                setSelectedDay(day.value);
                                                setSelectedSlot({
                                                  id: newSlotId,
                                                  name: first.name,
                                                  startTime: first.startTime,
                                                  endTime: first.endTime,
                                                  dayOfWeek: day.value,
                                                  isBreak: first.isBreak,
                                                  academicYearId: activeYear!.id
                                                });
                                                setIsEntryDialogOpen(true);
                                                toast.success(`Initialized ${first.name} for ${day.label}`);
                                              } catch (e) {
                                                toast.error("Failed to auto-create time slot");
                                              }
                                            }
                                          }}
                                          disabled={createSlotMutation.isPending}
                                          className={`absolute inset-1.5 flex items-center justify-center rounded-xl border-2 border-dashed transition-all duration-300 z-0 ${daySlot ? 'border-transparent hover:border-blue-300 hover:bg-blue-50/60 opacity-0 group-hover:opacity-100' : 'border-gray-200 opacity-40 hover:opacity-100 hover:border-blue-300'} ${createSlotMutation.isPending ? 'cursor-not-allowed' : ''}`}
                                          title={daySlot ? "Assign Subject" : `Initialize ${first.name} for ${day.label}`}
                                        >
                                          <div className="w-8 h-8 rounded-full bg-white flex items-center justify-center text-blue-500 shadow-sm group-hover:scale-110 transition-transform duration-300 print:hidden">
                                            {createSlotMutation.isPending ? (
                                              <div className="w-4 h-4 border-2 border-blue-500 border-t-transparent rounded-full animate-spin" />
                                            ) : (
                                              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M12 4v16m8-8H4" />
                                              </svg>
                                            )}
                                          </div>
                                        </button>
                                      )}
                                    </td>
                                  );
                                })}
                              </tr>
                            );
                          })
                        )}
                      </tbody>
                    </table>
                  </div>
                </div>
              )
            ) : (<div className="flex flex-col items-center justify-center p-24 bg-white rounded-3xl shadow-lg border border-gray-100 hover:shadow-xl transition-shadow duration-500 group relative overflow-hidden print:hidden">
              <div className="absolute inset-0 bg-gradient-to-br from-blue-50/50 to-indigo-50/50 transform scale-[0.98] group-hover:scale-100 transition-transform duration-700 rounded-3xl -z-10"></div>

              <div className="w-24 h-24 bg-gradient-to-br from-blue-100 to-indigo-100 text-blue-600 rounded-3xl flex items-center justify-center mb-6 shadow-inner relative group-hover:shadow-lg transition-shadow duration-500">
                <div className="absolute inset-0 bg-blue-400 rounded-3xl opacity-0 group-hover:opacity-20 transition-opacity duration-300 animate-pulse delay-100"></div>
                <svg className="w-12 h-12 group-hover:scale-110 group-hover:rotate-6 transition-transform duration-500 ease-out relative z-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>

              <h3 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent mb-3">
                {viewMode === 'section' ? 'Configure Section Timetable' : 'Faculty Workload View'}
              </h3>
              <p className="text-gray-500 max-w-md text-center font-medium leading-relaxed">
                {viewMode === 'section'
                  ? 'Select a class and section from the filters above to view or manage its weekly instruction schedule.'
                  : 'Select a faculty member to see their consolidated teaching schedule across all classes and sessions.'}
              </p>
            </div>
            )}

            <style dangerouslySetInnerHTML={{
              __html: `
            @media print {
              @page {
                size: A4 landscape;
                margin: 10mm;
              }
              body {
                background: white !important;
                padding: 0 !important;
              }
              .min-h-screen {
                min-height: auto !important;
                background: white !important;
              }
              .max-w-7xl {
                max-width: 100% !important;
                padding: 0 !important;
                margin: 0 !important;
              }
              table {
                border: 1px solid #e2e8f0 !important;
                width: 100% !important;
                table-layout: fixed !important;
              }
              th, td {
                border: 1px solid #e2e8f0 !important;
                padding: 6px !important;
              }
              .bg-gradient-to-br, .bg-gradient-to-r {
                background: #f8fafc !important;
                -webkit-print-color-adjust: exact;
              }
              .text-transparent {
                color: #1e3a8a !important;
                -webkit-fill-color: initial !important;
              }
              .rounded-2xl, .rounded-xl, .rounded-3xl {
                border-radius: 4px !important;
              }
              .shadow-lg, .shadow-xl, .shadow-sm {
                shadow: none !important;
                box-shadow: none !important;
              }
              button, .print-hidden {
                display: none !important;
              }
            }
            .input-field-new {
              @apply w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm font-semibold text-slate-700 focus:bg-white focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 transition-all outline-none appearance-none;
              background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%2364748b'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'%3E%3C/path%3E%3C/svg%3E");
              background-repeat: no-repeat;
              background-position: right 1rem center;
              background-size: 1.25rem;
            }
          `}} />
        </div>

        {/* TimeSlot Management Dialog */}
        {isTimeSlotDialogOpen && (
          <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-3xl shadow-2xl max-w-2xl w-full p-8 animate-slide-up">
              <div className="flex justify-between items-center mb-6">
                <h2 className="text-2xl font-bold text-gray-900">Manage Time Slots</h2>
                <button onClick={() => setIsTimeSlotDialogOpen(false)} className="text-gray-400 hover:text-gray-600">
                  <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                </button>
              </div>

              <form onSubmit={(e) => {
                e.preventDefault();
                const formData = new FormData(e.currentTarget);
                const data: CreateTimeSlotDto = {
                  name: formData.get('name') as string,
                  dayOfWeek: parseInt(formData.get('dayOfWeek') as string),
                  startTime: formData.get('startTime') as string + ":00",
                  endTime: formData.get('endTime') as string + ":00",
                  isBreak: formData.get('isBreak') === 'on',
                  academicYearId: activeYear!.id
                };
                createSlotMutation.mutate(data);
              }} className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700">Slot Name</label>
                    <input name="name" required className="input-field" placeholder="e.g. Period 1" />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700">Day</label>
                    <select name="dayOfWeek" required className="input-field">
                      {DAYS_OF_WEEK.map(d => <option key={d.value} value={d.value}>{d.label}</option>)}
                    </select>
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-semibold text-gray-700">Start Time</label>
                    <input name="startTime" type="time" required className="input-field" />
                  </div>
                  <div>
                    <label className="block text-sm font-semibold text-gray-700">End Time</label>
                    <input name="endTime" type="time" required className="input-field" />
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <input name="isBreak" type="checkbox" className="w-4 h-4 rounded border-gray-300 text-blue-600" />
                  <label className="text-sm font-semibold text-gray-700">This is a break/interval</label>
                </div>

                <div className="pt-4 flex gap-3">
                  <button type="button" onClick={() => setIsTimeSlotDialogOpen(false)} className="flex-1 btn-secondary">Cancel</button>
                  <button type="submit" className="flex-1 btn-primary">Create Slot</button>
                </div>
              </form>

              <div className="mt-8 border-t border-gray-100 pt-6">
                <div className="flex justify-between items-center mb-4">
                  <h3 className="text-sm font-bold text-gray-500 uppercase tracking-wider">Existing Slots ({activeYear?.name})</h3>
                  <div className="flex gap-2">
                    {DAYS_OF_WEEK.map(day => {
                      const hasSlots = sortedSlots.some(s => s.dayOfWeek === day.value);
                      if (!hasSlots) return null;
                      return (
                        <button
                          key={day.value}
                          onClick={() => {
                            if (confirm(`Clone ${day.label}'s structure to all other empty days?`)) {
                              syncSlotsMutation.mutate(day.value);
                            }
                          }}
                          disabled={syncSlotsMutation.isPending}
                          className="px-2 py-1 text-[10px] font-black bg-blue-50 text-blue-700 rounded-md hover:bg-blue-600 hover:text-white transition-all border border-blue-100 flex items-center gap-1"
                        >
                          <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" /></svg>
                          Sync {day.label}
                        </button>
                      );
                    })}
                  </div>
                </div>
                <div className="max-h-60 overflow-y-auto space-y-2">
                  {sortedSlots.map(slot => (
                    <div key={slot.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-xl hover:bg-gray-100 transition-colors">
                      <div>
                        <span className="font-bold text-gray-900">{slot.name}</span>
                        <span className="text-xs text-gray-500 ml-2">{DAYS_OF_WEEK.find(d => d.value === slot.dayOfWeek)?.label} • {slot.startTime.slice(0, 5)} - {slot.endTime.slice(0, 5)}</span>
                      </div>
                      <button
                        onClick={() => timetableApi.deleteTimeSlot(slot.id).then(() => queryClient.invalidateQueries({ queryKey: ['timeSlots'] }))}
                        className="text-red-500 hover:text-red-700 p-1"
                      >
                        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Timetable Entry Assignment Dialog */}
        {isEntryDialogOpen && selectedSlot && (
          <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
            <div className="bg-white rounded-3xl shadow-2xl max-w-md w-full p-8 animate-slide-up">
              <h2 className="text-2xl font-bold text-gray-900 mb-2">Assign Subject</h2>
              <p className="text-gray-500 text-sm mb-6">
                {DAYS_OF_WEEK.find(d => d.value === selectedDay)?.label} @ {selectedSlot.name} ({selectedSlot.startTime.slice(0, 5)})
              </p>

          <form onSubmit={(e) => {
            e.preventDefault();
            const formData = new FormData(e.currentTarget);
            const data: CreateTimetableEntryDto = {
              timeSlotId: selectedSlot.id,
              StaffAssignmentId: formData.get('staffAssignmentId') as string,
              roomNumber: formData.get('roomNumber') as string || undefined,
              academicYearId: activeYear!.id
            };
            createEntryMutation.mutate(data);
          }} className="space-y-4">
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-1">Subject Assignment</label>
              <select name="staffAssignmentId" required className="input-field">
                <option value="">-- Select Subject & Teacher --</option>
                {staffAssignments?.map(a => (
                  <option key={a.id} value={a.id}>
                    {a.subjectName} - {a.staffName}
                  </option>
                ))}
              </select>
              {staffAssignments?.length === 0 && (
                <p className="mt-1 text-xs text-amber-600 font-medium">
                  No teaching assignments found for this section. Please create assignments in Staff Management first.
                </p>
              )}
            </div>

            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-1">Room Number (Optional)</label>
              <input name="roomNumber" className="input-field" placeholder="e.g. Lab 1, Room 202" />
            </div>

            <div className="pt-4 flex gap-3">
              <button type="button" onClick={() => setIsEntryDialogOpen(false)} className="flex-1 btn-secondary">Cancel</button>
              <button
                type="submit"
                disabled={createEntryMutation.isPending || !staffAssignments?.length}
                className="flex-1 btn-primary"
              >
                {createEntryMutation.isPending ? 'Assigning...' : 'Assign Subject'}
              </button>
            </div>
          </form>
        </div>
      </div>
    )}

    {/* Bulk Sync Results Dialog */}
    {isResultDialogOpen && bulkSyncResult && (
      <div className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-3xl shadow-2xl max-w-2xl w-full p-8 animate-slide-up max-h-[90vh] flex flex-col">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-2xl font-bold text-gray-900">Sync Results</h2>
            <button onClick={() => setIsResultDialogOpen(false)} className="text-gray-400 hover:text-gray-600">
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
            </button>
          </div>

          <div className="grid grid-cols-2 gap-4 mb-6">
            <div className="p-4 bg-green-50 rounded-2xl border border-green-100">
              <div className="text-green-600 text-xs font-black uppercase tracking-widest mb-1">Copied Successfully</div>
              <div className="text-3xl font-black text-green-700">{bulkSyncResult.successCount}</div>
            </div>
            <div className="p-4 bg-amber-50 rounded-2xl border border-amber-100">
              <div className="text-amber-600 text-xs font-black uppercase tracking-widest mb-1">Skipped (Conflicts)</div>
              <div className="text-3xl font-black text-amber-700">{bulkSyncResult.skippedCount}</div>
            </div>
          </div>

          {bulkSyncResult.errors.length > 0 && (
            <div className="flex-1 overflow-y-auto">
              <h3 className="text-sm font-bold text-gray-500 uppercase tracking-wider mb-3">Conflict Details</h3>
              <div className="space-y-2">
                {bulkSyncResult.errors.map((error, idx) => (
                  <div key={idx} className="p-3 bg-red-50 text-red-700 text-sm rounded-xl border border-red-100 flex gap-3">
                    <svg className="w-5 h-5 text-red-400 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    {error}
                  </div>
                ))}
              </div>
            </div>
          )}

          <div className="pt-6 mt-6 border-t border-gray-100">
            <button onClick={() => setIsResultDialogOpen(false)} className="w-full btn-primary">Close Details</button>
          </div>
        </div>
      </div>
    )}
    </div>
  </div>
 );
}

import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { 
  timetableApi, 
  classApi, 
  StaffApi, 
  subjectApi,
  type TimeSlot, 
  type CreateTimeSlotDto,
  type CreateTimetableEntryDto
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
  
  const [selectedClassId, setSelectedClassId] = useState<string>('');
  const [selectedSectionId, setSelectedSectionId] = useState<string>('');
  const [isTimeSlotDialogOpen, setIsTimeSlotDialogOpen] = useState(false);
  const [isEntryDialogOpen, setIsEntryDialogOpen] = useState(false);
  const [selectedDay, setSelectedDay] = useState<number>(1);
  const [selectedSlot, setSelectedSlot] = useState<TimeSlot | null>(null);

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

  const { data: timeSlots, isLoading: isLoadingSlots } = useQuery({
    queryKey: ['timeSlots', activeYear?.id],
    queryFn: () => timetableApi.getTimeSlots(activeYear!.id),
    enabled: !!activeYear,
  });

  const { data: entries, isLoading: isLoadingEntries } = useQuery({
    queryKey: ['timetableEntries', selectedSectionId, activeYear?.id],
    queryFn: () => timetableApi.getSectionTimetable(selectedSectionId, activeYear!.id),
    enabled: !!selectedSectionId && !!activeYear,
  });

  const isGridLoading = isLoadingSlots || (!!selectedSectionId && isLoadingEntries);

  const { data: Staffs } = useQuery({
    queryKey: ['Staffs'],
    queryFn: () => StaffApi.getAll({ pageSize: 100, isActive: true }),
  });

  const { data: subjects } = useQuery({
    queryKey: ['subjects'],
    queryFn: () => subjectApi.getActive(),
  });

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

  // Helper to find entry for a day/slot
  const getEntryFor = (day: number, slotId: string) => {
    return entries?.find(e => e.dayOfWeek === day && e.timeSlotId === slotId);
  };

  const sortedSlots = useMemo(() => {
    if (!timeSlots) return [];
    return [...timeSlots].sort((a, b) => a.startTime.localeCompare(b.startTime));
  }, [timeSlots]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                Timetable Management
              </h1>
              <p className="text-gray-600 mt-2">Configure and manage weekly schedules for classes</p>
            </div>
            
            <button 
              onClick={() => setIsTimeSlotDialogOpen(true)}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-60h6" />
              </svg>
              Manage Time Slots
            </button>
          </div>

          {/* Main Filter */}
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-6 flex flex-col md:flex-row gap-6 hover:shadow-xl transition-shadow duration-300">
        <div className="flex-1">
          <label className="block text-sm font-semibold text-gray-700 mb-2">Select Class</label>
          <select 
            value={selectedClassId}
            onChange={(e) => {
              setSelectedClassId(e.target.value);
              setSelectedSectionId('');
            }}
            className="input-field w-full"
          >
           <option value="">-- Select Class --</option>
            {classes?.items.map(cls => (
              <option key={cls.id} value={cls.id}>{cls.name}</option>
            ))}
          </select>
        </div>

        <div className="flex-1">
          <label className="block text-sm font-semibold text-gray-700 mb-2">Select Section</label>
          <select 
            value={selectedSectionId}
            onChange={(e) => setSelectedSectionId(e.target.value)}
            disabled={!selectedClassId}
            className="input-field w-full disabled:opacity-50"
          >
            <option value="">-- Select Section --</option>
            {sections?.map(sec => (
              <option key={sec.id} value={sec.id}>{sec.sectionName}</option>
            ))}
          </select>
        </div>
      </div>

      {/* Timetable Grid */}
      {selectedSectionId ? (
        isGridLoading ? (
          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden p-6">
            <LoadingSkeleton rows={5} type="table" />
          </div>
        ) : (
        <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
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
                {sortedSlots.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="p-12 text-center text-gray-400">
                      No time slots configured for {activeYear?.name}. 
                      <button onClick={() => setIsTimeSlotDialogOpen(true)} className="text-blue-600 hover:underline ml-1">Create one now.</button>
                    </td>
                  </tr>
                ) : (
                  sortedSlots.map(slot => (
                    <tr key={slot.id} className="border-b border-gray-200 hover:bg-gray-50 transition-colors">
                      <td className="p-4 border-r border-gray-200">
                        <div className="font-bold text-gray-900 text-sm">{slot.name}</div>
                        <div className="text-xs text-gray-500 font-mono">{slot.startTime.slice(0, 5)} - {slot.endTime.slice(0, 5)}</div>
                        {slot.isBreak && <span className="mt-1 inline-block px-1.5 py-0.5 rounded text-[10px] font-bold uppercase bg-orange-100 text-orange-700">Break</span>}
                      </td>
                      
                      {DAYS_OF_WEEK.map(day => {
                        const entry = getEntryFor(day.value, slot.id);
                        return (
                          <td 
                            key={`${day.value}-${slot.id}`} 
                            className={`p-2 border-r border-gray-200 relative group min-h-[100px] transition-colors duration-200 ${slot.isBreak ? 'bg-orange-50/30' : ''} ${day.value === todayDayIndex && !slot.isBreak ? 'bg-blue-50/20' : ''}`}
                          >
                            {slot.isBreak ? (
                              <div className="flex items-center justify-center h-full min-h-[60px]">
                                <div className="text-center text-orange-400/70 font-bold text-xs italic tracking-widest uppercase">Interval</div>
                              </div>
                            ) : entry ? (
                              <div className="p-3 rounded-xl bg-gradient-to-br from-blue-50 to-blue-100 border border-blue-200 shadow-sm relative group/card hover:shadow-md hover:scale-[1.03] hover:-translate-y-0.5 transition-all duration-300 z-10 hover:z-20 cursor-default flex flex-col h-full min-h-[80px] ring-1 ring-black/5 hover:ring-blue-400">
                                <button 
                                  onClick={() => deleteEntryMutation.mutate(entry.id)}
                                  className="absolute -top-2 -right-2 bg-red-500 text-white rounded-full p-1.5 opacity-0 group-hover/card:opacity-100 transition-opacity shadow-lg hover:bg-red-600 hover:scale-110 focus:opacity-100"
                                  title="Remove Assignment"
                                >
                                  <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M6 18L18 6M6 6l12 12" />
                                  </svg>
                                </button>
                                <div className="font-bold text-blue-900 text-sm truncate group-hover/card:whitespace-normal group-hover/card:overflow-visible transition-all leading-tight">
                                  {entry.subjectName}
                                </div>
                                <div className="text-xs text-blue-700 mt-1.5 flex items-center gap-1.5 opacity-90">
                                  <svg className="w-3.5 h-3.5 opacity-70 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clipRule="evenodd" />
                                  </svg>
                                  <span className="truncate">{entry.StaffName}</span>
                                </div>
                                {entry.roomNumber && (
                                  <div className="mt-auto pt-2.5 flex items-center gap-1">
                                    <span className="inline-flex items-center gap-1 px-1.5 py-0.5 rounded-md bg-white/60 text-[10px] text-blue-800 font-bold uppercase tracking-wider backdrop-blur-sm border border-blue-200/50 shadow-sm">
                                      <svg className="w-3 h-3 text-blue-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                                      </svg>
                                      {entry.roomNumber}
                                    </span>
                                  </div>
                                )}
                              </div>
                            ) : (
                              <button 
                                onClick={() => {
                                  setSelectedDay(day.value);
                                  setSelectedSlot(slot);
                                  setIsEntryDialogOpen(true);
                                }}
                                className="absolute inset-1.5 flex items-center justify-center rounded-xl border-2 border-dashed border-transparent hover:border-blue-300 hover:bg-blue-50/60 opacity-0 group-hover:opacity-100 transition-all duration-300 z-0"
                                title="Assign Subject"
                              >
                                <div className="w-8 h-8 rounded-full bg-white flex items-center justify-center text-blue-500 shadow-sm group-hover:scale-110 transition-transform duration-300">
                                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M12 4v16m8-8H4" />
                                  </svg>
                                </div>
                              </button>
                            )}
                          </td>
                        );
                      })}
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>
        )
      ) : (
        <div className="flex flex-col items-center justify-center p-24 bg-white rounded-3xl shadow-lg border border-gray-100 hover:shadow-xl transition-shadow duration-500 group relative overflow-hidden">
          <div className="absolute inset-0 bg-gradient-to-br from-blue-50/50 to-indigo-50/50 transform scale-[0.98] group-hover:scale-100 transition-transform duration-700 rounded-3xl -z-10"></div>
          
          <div className="w-24 h-24 bg-gradient-to-br from-blue-100 to-indigo-100 text-blue-600 rounded-3xl flex items-center justify-center mb-6 shadow-inner relative group-hover:shadow-lg transition-shadow duration-500">
            <div className="absolute inset-0 bg-blue-400 rounded-3xl opacity-0 group-hover:opacity-20 transition-opacity duration-300 animate-pulse delay-100"></div>
            <svg className="w-12 h-12 group-hover:scale-110 group-hover:rotate-6 transition-transform duration-500 ease-out relative z-10" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
          
          <h3 className="text-2xl font-bold bg-gradient-to-r from-gray-900 to-gray-700 bg-clip-text text-transparent mb-3">
            Configure Timetable
          </h3>
          <p className="text-gray-500 max-w-md text-center font-medium leading-relaxed">
            Select a class and section from the filters above to view, modify, or manage its weekly instruction schedule.
          </p>
        </div>
      )}

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
              <h3 className="text-sm font-bold text-gray-500 uppercase tracking-wider mb-4">Existing Slots ({activeYear?.name})</h3>
              <div className="max-h-60 overflow-y-auto space-y-2">
                {sortedSlots.map(slot => (
                  <div key={slot.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-xl hover:bg-gray-100 transition-colors">
                    <div>
                      <span className="font-bold text-gray-900">{slot.name}</span>
                      <span className="text-xs text-gray-500 ml-2">{DAYS_OF_WEEK.find(d => d.value === slot.dayOfWeek)?.label} • {slot.startTime.slice(0,5)} - {slot.endTime.slice(0,5)}</span>
                    </div>
                    <button 
                      onClick={() => timetableApi.deleteTimeSlot(slot.id).then(() => queryClient.invalidateQueries({queryKey: ['timeSlots']}))}
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
              {DAYS_OF_WEEK.find(d => d.value === selectedDay)?.label} @ {selectedSlot.name} ({selectedSlot.startTime.slice(0,5)})
            </p>
            
            <form onSubmit={(e) => {
              e.preventDefault();
              const formData = new FormData(e.currentTarget);
              const data: CreateTimetableEntryDto = {
                timeSlotId: selectedSlot.id,
                sectionId: selectedSectionId,
                subjectId: formData.get('subjectId') as string,
                StaffId: formData.get('StaffId') as string,
                roomNumber: formData.get('roomNumber') as string || undefined,
                academicYearId: activeYear!.id
              };
              createEntryMutation.mutate(data);
            }} className="space-y-4">
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1">Subject</label>
                <select name="subjectId" required className="input-field">
                  <option value="">-- Select Subject --</option>
                  {subjects?.map(s => <option key={s.id} value={s.id}>{s.name} ({s.code})</option>)}
                </select>
              </div>
              
              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1">Staff</label>
                <select name="StaffId" required className="input-field">
                  <option value="">-- Select Staff --</option>
                  {Staffs?.items.map(t => <option key={t.id} value={t.id}>{t.firstName} {t.lastName}</option>)}
                </select>
              </div>

              <div>
                <label className="block text-sm font-semibold text-gray-700 mb-1">Room Number (Optional)</label>
                <input name="roomNumber" className="input-field" placeholder="e.g. Lab 1, Room 202" />
              </div>

              <div className="pt-4 flex gap-3">
                <button type="button" onClick={() => setIsEntryDialogOpen(false)} className="flex-1 btn-secondary">Cancel</button>
                <button 
                  type="submit" 
                  disabled={createEntryMutation.isPending}
                  className="flex-1 btn-primary"
                >
                  {createEntryMutation.isPending ? 'Assigning...' : 'Assign Subject'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
        </div>
      </div>
    </div>
  );
}


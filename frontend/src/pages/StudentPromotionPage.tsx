import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { classApi, settingsApi } from '../services/api';
import { usePromotion } from '../hooks/usePromotion';
import toast from 'react-hot-toast';

export function StudentPromotionPage() {
  const { promoteBulk } = usePromotion();
  
  // State for source
  const [sourceYearId, setSourceYearId] = useState<string>('');
  const [sourceClassId, setSourceClassId] = useState<string>('');
  const [sourceSectionId, setSourceSectionId] = useState<string>('');
  
  // State for target
  const [targetYearId, setTargetYearId] = useState<string>('');
  const [targetClassId, setTargetClassId] = useState<string>('');
  const [targetSectionId, setTargetSectionId] = useState<string>('');
  
  // Selection
  const [selectedStudentIds, setSelectedStudentIds] = useState<string[]>([]);
  const [markAsPromoted, setMarkAsPromoted] = useState<boolean>(true);

  // Fetch Academic Years
  const { data: academicYears } = useQuery({
    queryKey: ['academic-years'],
    queryFn: () => settingsApi.getAcademicYears(),
  });

  // Fetch Classes (Global since we filter in code or use the header for source)
  const { data: classesData } = useQuery({
    queryKey: ['classes'],
    queryFn: () => classApi.getAll({ pageSize: 100 }),
  });

  // Fetch Source Sections
  const { data: sourceSections } = useQuery({
    queryKey: ['sections', sourceClassId],
    queryFn: () => classApi.getSectionsByClass(sourceClassId),
    enabled: !!sourceClassId,
  });

  // Fetch Target Sections
  const { data: targetSections } = useQuery({
    queryKey: ['sections', targetClassId],
    queryFn: () => classApi.getSectionsByClass(targetClassId),
    enabled: !!targetClassId,
  });

  // Fetch Students in Source Section
  const { data: sourceStudents, isLoading: isStudentsLoading } = useQuery({
    queryKey: ['section-students', sourceSectionId],
    queryFn: () => classApi.getRollNumbers(sourceSectionId),
    enabled: !!sourceSectionId,
  });

  // Handle source switch (This is tricky because the global context might be different)
  // For now, we assume the user is viewing the SOURCE year in the application.
  useEffect(() => {
    const activeYearId = localStorage.getItem('selectedAcademicYearId');
    if (activeYearId) {
      setSourceYearId(activeYearId);
    }
  }, []);

  const handleToggleStudent = (studentId: string) => {
    setSelectedStudentIds(prev => 
      prev.includes(studentId) 
        ? prev.filter(id => id !== studentId) 
        : [...prev, studentId]
    );
  };

  const handleSelectAll = () => {
    if (sourceStudents && selectedStudentIds.length === sourceStudents.length) {
      setSelectedStudentIds([]);
    } else if (sourceStudents) {
      setSelectedStudentIds(sourceStudents.map(s => s.studentId));
    }
  };

  const handlePromote = async () => {
    if (!sourceYearId || !targetYearId || selectedStudentIds.length === 0 || !targetClassId) {
      toast.error('Please complete all selections');
      return;
    }

    if (sourceYearId === targetYearId) {
      toast.error('Source and Target academic years must be different');
      return;
    }

    const confirmResult = window.confirm(`Are you sure you want to promote ${selectedStudentIds.length} students?`);
    if (!confirmResult) return;

    try {
      const result = await promoteBulk.mutateAsync({
        sourceAcademicYearId: sourceYearId,
        targetAcademicYearId: targetYearId,
        studentIds: selectedStudentIds,
        targetClassId: targetClassId,
        targetSectionId: targetSectionId || undefined,
        markSourceAsPromoted: markAsPromoted
      });

      if (result.success) {
        toast.success(result.message);
        setSelectedStudentIds([]);
      } else {
        toast.error(result.message);
      }
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Promotion failed');
    }
  };

  const activeYear = academicYears?.find(y => y.id === sourceYearId);

  return (
    <div className="min-h-screen bg-gray-50 pb-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <header className="mb-8">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-indigo-700 bg-clip-text text-transparent flex items-center gap-3">
            <span>🔄</span> Student Promotion Center
          </h1>
          <p className="text-gray-600 mt-2">Bulk transition students between academic years with enrollment management</p>
        </header>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Configuration Sidebar */}
          <div className="lg:col-span-1 space-y-6">
            {/* Source Config */}
            <div className="bg-white/70 backdrop-blur-md rounded-2xl shadow-xl border border-white/20 p-6">
              <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                <span className="p-1.5 bg-blue-100 text-blue-600 rounded-lg">📤</span> Source (FROM)
              </h2>
              <div className="space-y-4">
                <div>
                  <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Academic Year</label>
                  <div className="p-3 bg-blue-50 text-blue-800 rounded-xl border border-blue-100 font-semibold ring-1 ring-blue-200">
                    {activeYear?.name || 'Loading...'} (Current Context)
                  </div>
                  <p className="text-[10px] text-gray-500 mt-1 italic">* Switch session in header to change source year</p>
                </div>
                
                <div>
                  <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Class</label>
                  <select 
                    value={sourceClassId} 
                    onChange={(e) => setSourceClassId(e.target.value)}
                    className="w-full p-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                  >
                    <option value="">Select Class</option>
                    {classesData?.items.map(c => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Section</label>
                  <select 
                    value={sourceSectionId} 
                    onChange={(e) => setSourceSectionId(e.target.value)}
                    className="w-full p-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                    disabled={!sourceClassId}
                  >
                    <option value="">Select Section</option>
                    {sourceSections?.map(s => (
                      <option key={s.id} value={s.id}>{s.sectionName}</option>
                    ))}
                  </select>
                </div>
              </div>
            </div>

            {/* Target Config */}
            <div className="bg-white/70 backdrop-blur-md rounded-2xl shadow-xl border border-white/20 p-6 overflow-hidden relative">
              <div className="absolute top-0 right-0 p-8 opacity-10 rotate-12 -mr-4 -mt-4">
                <span className="text-8xl">🚀</span>
              </div>
              
              <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
                <span className="p-1.5 bg-green-100 text-green-600 rounded-lg">📥</span> Target (TO)
              </h2>
              <div className="space-y-4 relative z-10">
                <div>
                  <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Academic Year</label>
                  <select 
                    value={targetYearId} 
                    onChange={(e) => setTargetYearId(e.target.value)}
                    className="w-full p-3 bg-green-50/50 border border-green-200 rounded-xl focus:ring-2 focus:ring-green-500 outline-none transition-all font-semibold"
                  >
                    <option value="">Select Next Year</option>
                    {academicYears?.filter(y => y.id !== sourceYearId).map(y => (
                      <option key={y.id} value={y.id}>{y.name}</option>
                    ))}
                  </select>
                </div>
                
                <div>
                  <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Promoted Class</label>
                  <select 
                    value={targetClassId} 
                    onChange={(e) => setTargetClassId(e.target.value)}
                    className="w-full p-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                  >
                    <option value="">Select Target Class</option>
                    {classesData?.items.map(c => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-xs font-bold text-gray-400 uppercase tracking-wider mb-1">Target Section (Optional)</label>
                  <select 
                    value={targetSectionId} 
                    onChange={(e) => setTargetSectionId(e.target.value)}
                    className="w-full p-3 bg-gray-50 border border-gray-200 rounded-xl focus:ring-2 focus:ring-blue-500 outline-none transition-all"
                    disabled={!targetClassId}
                  >
                    <option value="">Same as Source / Best Match</option>
                    {targetSections?.map(s => (
                      <option key={s.id} value={s.id}>{s.sectionName}</option>
                    ))}
                  </select>
                </div>

                <div className="pt-4 border-t border-gray-100">
                  <label className="flex items-center gap-3 cursor-pointer group">
                    <input 
                      type="checkbox" 
                      checked={markAsPromoted}
                      onChange={(e) => setMarkAsPromoted(e.target.checked)}
                      className="w-5 h-5 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <span className="text-sm font-medium text-gray-700 group-hover:text-blue-600 transition-colors">
                      Mark source enrollment as "Promoted"
                    </span>
                  </label>
                </div>
              </div>
            </div>

            {/* Action Card */}
            <div className="bg-gradient-to-br from-blue-600 to-indigo-700 rounded-2xl shadow-xl p-6 text-white">
              <div className="flex justify-between items-center mb-6">
                <div>
                  <p className="text-blue-100 text-xs font-bold uppercase tracking-widest">Total Selected</p>
                  <p className="text-4xl font-black">{selectedStudentIds.length}</p>
                </div>
                <div className="bg-white/20 p-3 rounded-full backdrop-blur-sm">
                  <span className="text-2xl">⚡</span>
                </div>
              </div>
              
              <button
                onClick={handlePromote}
                disabled={promoteBulk.isPending || selectedStudentIds.length === 0}
                className="w-full py-4 bg-white text-blue-700 font-bold rounded-xl shadow-lg hover:bg-blue-50 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed uppercase tracking-widest text-sm"
              >
                {promoteBulk.isPending ? 'Processing...' : 'Run Promotion Workflow'}
              </button>
            </div>
          </div>

          {/* Student List Section */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-2xl shadow-xl h-full border border-gray-100 overflow-hidden flex flex-col">
              <div className="p-6 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
                <div>
                  <h3 className="text-xl font-extrabold text-gray-900">Eligible Students</h3>
                  <p className="text-sm text-gray-500">Showing students from current session for migration</p>
                </div>
                {sourceStudents && sourceStudents.length > 0 && (
                  <button 
                    onClick={handleSelectAll}
                    className="text-sm font-bold text-blue-600 hover:text-blue-800 transition-colors bg-blue-50 px-4 py-2 rounded-lg"
                  >
                    {selectedStudentIds.length === sourceStudents.length ? 'Deselect All' : 'Select All Section'}
                  </button>
                )}
              </div>

              <div className="flex-1 overflow-y-auto min-h-[500px]">
                {!sourceSectionId ? (
                  <div className="flex flex-col items-center justify-center h-full text-gray-400 p-12 text-center">
                    <div className="text-6xl mb-4">🔍</div>
                    <h4 className="text-lg font-bold text-gray-500">Pick a Source Section</h4>
                    <p className="text-sm">Select a class and section from the left to load student data</p>
                  </div>
                ) : isStudentsLoading ? (
                  <div className="flex items-center justify-center h-full p-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent shadow-md"></div>
                  </div>
                ) : sourceStudents && sourceStudents.length > 0 ? (
                  <table className="w-full text-left">
                    <thead className="bg-gray-50 text-[10px] uppercase font-bold text-gray-400 tracking-wider sticky top-0 z-10">
                      <tr>
                        <th className="px-6 py-4 w-12"></th>
                        <th className="px-6 py-4">Roll</th>
                        <th className="px-6 py-4">Student Name</th>
                        <th className="px-6 py-4">Enrollment</th>
                        <th className="px-6 py-4">Status</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-50">
                      {sourceStudents.map((s) => (
                        <tr 
                          key={s.id} 
                          className={`hover:bg-blue-50/50 transition-colors cursor-pointer group ${selectedStudentIds.includes(s.studentId) ? 'bg-blue-50/30' : ''}`}
                          onClick={() => handleToggleStudent(s.studentId)}
                        >
                          <td className="px-6 py-4">
                            <input 
                              type="checkbox" 
                              checked={selectedStudentIds.includes(s.studentId)}
                              onChange={() => {}} // Handled by tr click
                              className="w-5 h-5 rounded border-gray-300 text-blue-600 transition-all pointer-events-none"
                            />
                          </td>
                          <td className="px-6 py-4 text-sm font-black text-gray-900">
                            {s.rollNumber || '--'}
                          </td>
                          <td className="px-6 py-4">
                            <div className="flex items-center gap-3">
                              <div className="h-9 w-9 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center font-bold text-xs ring-4 ring-white shadow-sm capitalize">
                                {s.studentName?.charAt(0) || 'S'}
                              </div>
                              <span className="font-semibold text-gray-700">{s.studentName}</span>
                            </div>
                          </td>
                          <td className="px-6 py-4 text-sm font-mono text-gray-500">
                            {s.enrollmentNumber}
                          </td>
                          <td className="px-6 py-4">
                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-green-100 text-green-700">
                              Active
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                ) : (
                  <div className="flex flex-col items-center justify-center h-full text-gray-400 p-12 text-center">
                    <div className="text-6xl mb-4">🍃</div>
                    <h4 className="text-lg font-bold">No students found</h4>
                    <p className="text-sm">There are no active enrollments in this section for the selected year</p>
                  </div>
                )}
              </div>
              
              <div className="p-4 bg-gray-50 border-t border-gray-100 flex items-center justify-between text-xs font-bold text-gray-400 uppercase tracking-widest">
                <span>Verification required before promotion</span>
                <span>Idempotent Execution</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

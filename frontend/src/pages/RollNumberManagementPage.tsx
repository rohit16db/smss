import { useState, useEffect } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { classApi } from '../services/api';

export function RollNumberManagementPage() {
  const [selectedSectionId, setSelectedSectionId] = useState<string>('');
  const [rollNumbers, setRollNumbers] = useState<{ [key: string]: number }>({});
  const [isEditing, setIsEditing] = useState(false);

  // Get classes
  const { data: classesData } = useQuery({
    queryKey: ['classes'],
    queryFn: () => classApi.getAll({ pageNumber: 1, pageSize: 100 }),
  });

  // Get sections for all classes
  const { data: allSections = [] } = useQuery({
    queryKey: ['sections', classesData?.items?.map(c => c.id)],
    queryFn: async () => {
      if (!classesData?.items || classesData.items.length === 0) return [];
      
      const sectionPromises = classesData.items.map(c =>
        classApi.getSectionsByClass(c.id).then(sections =>
          sections.map(s => ({
            ...s,
            className: c.name,
          }))
        )
      );
      
      const allSecs = await Promise.all(sectionPromises);
      return allSecs.flat();
    },
    enabled: !!classesData?.items && classesData.items.length > 0,
  });

  const sections = allSections;

  // Get roll numbers for selected section
  const { data: studentsWithRollNumbers, isLoading: isLoadingStudents, refetch } = useQuery({
    queryKey: ['roll-numbers', selectedSectionId],
    queryFn: () => selectedSectionId ? classApi.getRollNumbers(selectedSectionId) : Promise.resolve([]),
    enabled: !!selectedSectionId,
  });

  useEffect(() => {
    if (studentsWithRollNumbers) {
      const rollNumbersMap: { [key: string]: number } = {};
      studentsWithRollNumbers.forEach(student => {
        if (student.rollNumber) {
          rollNumbersMap[student.id] = student.rollNumber;
        }
      });
      setRollNumbers(rollNumbersMap);
    }
  }, [studentsWithRollNumbers]);

  // Auto-assign mutation
  const autoAssignMutation = useMutation({
    mutationFn: () => selectedSectionId ? classApi.autoAssignRollNumbers(selectedSectionId) : Promise.reject('No section selected'),
    onSuccess: () => {
      toast.success('Roll numbers assigned successfully');
      refetch();
      setIsEditing(false);
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to assign roll numbers');
    },
  });

  // Bulk update mutation
  const bulkUpdateMutation = useMutation({
    mutationFn: () => selectedSectionId ? classApi.bulkUpdateRollNumbers(selectedSectionId, rollNumbers) : Promise.reject('No section selected'),
    onSuccess: () => {
      toast.success('Roll numbers updated successfully');
      refetch();
      setIsEditing(false);
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to update roll numbers');
    },
  });

  const handleRollNumberChange = (studentId: string, newRollNumber: number) => {
    setRollNumbers(prev => ({
      ...prev,
      [studentId]: newRollNumber,
    }));
  };

  return (
    <div className="p-8">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Roll Number Management</h1>
          <p className="text-gray-600">Manage and assign roll numbers to students in each section</p>
        </div>

        {/* Section Selection */}
        <div className="bg-white rounded-xl shadow-md p-6 mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">Select Section</label>
          <select
            value={selectedSectionId}
            onChange={(e) => {
              setSelectedSectionId(e.target.value);
              setRollNumbers({});
              setIsEditing(false);
            }}
            className="w-full px-4 py-2.5 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-blue-500 transition-colors"
          >
            <option value="">-- Select a Section --</option>
            {sections.map(section => (
              <option key={section.id} value={section.id}>
                {section.className} - {section.sectionName}
              </option>
            ))}
          </select>
        </div>

        {/* Students Table */}
        {selectedSectionId && (
          <div className="bg-white rounded-xl shadow-md overflow-hidden">
            <div className="p-6 border-b-2 border-gray-200">
              <div className="flex justify-between items-center">
                <h2 className="text-xl font-bold text-gray-900">
                  Students in {sections.find(s => s.id === selectedSectionId)?.sectionName}
                </h2>
                <div className="flex gap-3">
                  {isEditing ? (
                    <>
                      <button
                        onClick={() => {
                          setIsEditing(false);
                          setRollNumbers({});
                          refetch();
                        }}
                        className="px-4 py-2 border-2 border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition"
                      >
                        Cancel
                      </button>
                      <button
                        onClick={() => bulkUpdateMutation.mutate()}
                        disabled={bulkUpdateMutation.isPending}
                        className="px-4 py-2 bg-green-600 hover:bg-green-700 text-white font-medium rounded-lg transition disabled:opacity-50"
                      >
                        {bulkUpdateMutation.isPending ? 'Saving...' : 'Save Changes'}
                      </button>
                    </>
                  ) : (
                    <>
                      <button
                        onClick={() => autoAssignMutation.mutate()}
                        disabled={autoAssignMutation.isPending}
                        className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition disabled:opacity-50"
                      >
                        {autoAssignMutation.isPending ? 'Assigning...' : '🔄 Auto Assign'}
                      </button>
                      <button
                        onClick={() => setIsEditing(true)}
                        className="px-4 py-2 bg-amber-600 hover:bg-amber-700 text-white font-medium rounded-lg transition"
                      >
                        ✏️ Edit
                      </button>
                    </>
                  )}
                </div>
              </div>
            </div>

            {isLoadingStudents ? (
              <div className="p-6 text-center text-gray-500">Loading students...</div>
            ) : studentsWithRollNumbers && studentsWithRollNumbers.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gray-100 border-b-2 border-gray-200">
                    <tr>
                      <th className="px-6 py-3 text-left text-sm font-bold text-gray-900">Student Name</th>
                      <th className="px-6 py-3 text-center text-sm font-bold text-gray-900">Roll Number</th>
                      <th className="px-6 py-3 text-center text-sm font-bold text-gray-900">Joined Date</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-200">
                    {studentsWithRollNumbers.map((student) => (
                      <tr key={student.id} className="hover:bg-blue-50 transition-colors">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-gray-900">{student.studentName}</div>
                          <div className="text-xs text-gray-500">{student.studentId}</div>
                        </td>
                        <td className="px-6 py-4 text-center">
                          {isEditing ? (
                            <input
                              type="number"
                              min="1"
                              value={rollNumbers[student.id] || ''}
                              onChange={(e) => handleRollNumberChange(student.id, parseInt(e.target.value) || 0)}
                              className="w-20 px-3 py-1.5 border-2 border-gray-300 rounded-lg text-center focus:outline-none focus:border-blue-500"
                            />
                          ) : (
                            <span className="inline-flex items-center justify-center w-10 h-10 bg-blue-100 text-blue-700 font-bold rounded-full">
                              {student.rollNumber || '-'}
                            </span>
                          )}
                        </td>
                        <td className="px-6 py-4 text-center text-sm text-gray-600">
                          {new Date(student.joinedDate).toLocaleDateString('en-US', { 
                            year: 'numeric', 
                            month: 'short', 
                            day: 'numeric' 
                          })}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="p-6 text-center text-gray-500">No students in this section</div>
            )}
          </div>
        )}

        {/* Info Box */}
        {!selectedSectionId && (
          <div className="bg-blue-50 border-2 border-blue-200 rounded-xl p-6 text-center">
            <p className="text-gray-700">Select a section above to manage roll numbers</p>
          </div>
        )}
      </div>
    </div>
  );
}

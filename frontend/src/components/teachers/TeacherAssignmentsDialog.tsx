import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import type { AxiosError } from 'axios';
import { teacherApi, classApi, subjectApi, type Teacher, type TeacherAssignment, type CreateTeacherAssignmentDto } from '../../services/api';

interface TeacherAssignmentsDialogProps {
  teacher: Teacher | null;
  open: boolean;
  onClose: () => void;
}

export function TeacherAssignmentsDialog({ teacher, open, onClose }: TeacherAssignmentsDialogProps) {
  const queryClient = useQueryClient();
  const [showAssignForm, setShowAssignForm] = useState(false);
  const [formData, setFormData] = useState<CreateTeacherAssignmentDto>({
    classId: '',
    subjectId: '',
    assignmentDate: new Date().toISOString().split('T')[0],
  });

  // Reset form when dialog opens
  useEffect(() => {
    if (open && teacher) {
      setShowAssignForm(false);
      setFormData({
        classId: '',
        subjectId: '',
        assignmentDate: new Date().toISOString().split('T')[0],
      });
    }
  }, [open, teacher]);

  // Fetch teacher assignments
  const { data: assignments = [], isLoading: loadingAssignments } = useQuery({
    queryKey: ['teacher-assignments', teacher?.id],
    queryFn: () => teacher ? teacherApi.getAssignments(teacher.id, false) : Promise.resolve([]),
    enabled: !!teacher,
  });

  // Fetch classes for dropdown
  const { data: classesData } = useQuery({
    queryKey: ['classes', 'active'],
    queryFn: async () => {
      const result = await classApi.getAll({ isActive: true, pageSize: 100 });
      return result.items;
    },
  });

  // Fetch subjects for dropdown
  const { data: subjectsData } = useQuery({
    queryKey: ['subjects', 'active'],
    queryFn: () => subjectApi.getActive(),
  });

  // Create assignment mutation
  const createAssignmentMutation = useMutation({
    mutationFn: (data: CreateTeacherAssignmentDto) =>
      teacher ? teacherApi.createAssignment(teacher.id, data) : Promise.reject('No teacher selected'),
    onSuccess: () => {
      toast.success('Assignment created successfully!');
      queryClient.invalidateQueries({ queryKey: ['teacher-assignments', teacher?.id] });
      setShowAssignForm(false);
      setFormData({
        classId: '',
        subjectId: '',
        assignmentDate: new Date().toISOString().split('T')[0],
      });
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to create assignment');
    },
  });

  // Remove assignment mutation
  const removeAssignmentMutation = useMutation({
    mutationFn: ({ assignmentId }: { assignmentId: string }) =>
      teacher ? teacherApi.removeAssignment(teacher.id, assignmentId) : Promise.reject('No teacher selected'),
    onSuccess: () => {
      toast.success('Assignment removed successfully!');
      queryClient.invalidateQueries({ queryKey: ['teacher-assignments', teacher?.id] });
    },
    onError: (error: AxiosError<{ message?: string }>) => {
      toast.error(error.response?.data?.message || 'Failed to remove assignment');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.classId || !formData.subjectId) {
      toast.error('Please select both class and subject');
      return;
    }
    createAssignmentMutation.mutate(formData);
  };

  const handleRemove = (assignmentId: string) => {
    if (window.confirm('Are you sure you want to remove this assignment?')) {
      removeAssignmentMutation.mutate({ assignmentId });
    }
  };

  if (!open || !teacher) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
      <div className="bg-white rounded-2xl shadow-2xl max-w-2xl w-full max-h-[90vh] overflow-auto animate-fadeIn">
        {/* Header */}
        <div className="sticky top-0 bg-gradient-to-r from-indigo-600 to-purple-600 p-6 text-white z-10">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-2xl font-bold">Teacher Assignments</h2>
              <p className="text-indigo-100 mt-1">{teacher.firstName} {teacher.lastName}</p>
            </div>
            <button
              onClick={onClose}
              className="text-white hover:bg-white/20 rounded-full p-2 transition-colors"
            >
              <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        {/* Content */}
        <div className="p-6">
          {/* Add Assignment Button */}
          {!showAssignForm && (
            <button
              onClick={() => setShowAssignForm(true)}
              className="mb-6 w-full px-4 py-3 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors font-medium flex items-center justify-center gap-2"
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              Add New Assignment
            </button>
          )}

          {/* Assignment Form */}
          {showAssignForm && (
            <form onSubmit={handleSubmit} className="mb-6 p-4 bg-gray-50 rounded-lg border-2 border-indigo-200">
              <h3 className="text-lg font-semibold text-gray-800 mb-4">New Assignment</h3>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Class <span className="text-red-500">*</span>
                  </label>
                  <select
                    value={formData.classId}
                    onChange={(e) => setFormData({ ...formData, classId: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    required
                  >
                    <option value="">Select Class</option>
                    {classesData?.map((cls) => (
                      <option key={cls.id} value={cls.id}>
                        {cls.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Subject <span className="text-red-500">*</span>
                  </label>
                  <select
                    value={formData.subjectId}
                    onChange={(e) => setFormData({ ...formData, subjectId: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    required
                  >
                    <option value="">Select Subject</option>
                    {subjectsData?.map((subject) => (
                      <option key={subject.id} value={subject.id}>
                        {subject.name} ({subject.code})
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Assignment Date
                  </label>
                  <input
                    type="date"
                    value={formData.assignmentDate}
                    onChange={(e) => setFormData({ ...formData, assignmentDate: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  />
                </div>
              </div>

              <div className="flex gap-2 mt-4">
                <button
                  type="submit"
                  disabled={createAssignmentMutation.isPending}
                  className="flex-1 px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 transition-colors disabled:opacity-50"
                >
                  {createAssignmentMutation.isPending ? 'Creating...' : 'Create Assignment'}
                </button>
                <button
                  type="button"
                  onClick={() => setShowAssignForm(false)}
                  className="px-4 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          )}

          {/* Assignments List */}
          <div>
            <h3 className="text-lg font-semibold text-gray-800 mb-4">
              Current Assignments ({assignments.filter((a: TeacherAssignment) => a.isActive).length})
            </h3>

            {loadingAssignments ? (
              <div className="text-center py-8 text-gray-500">Loading assignments...</div>
            ) : assignments.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                <p>No assignments yet.</p>
                <p className="text-sm mt-1">Click "Add New Assignment" to get started.</p>
              </div>
            ) : (
              <div className="space-y-3">
                {assignments.map((assignment: TeacherAssignment) => (
                  <div
                    key={assignment.id}
                    className={`p-4 rounded-lg border-2 transition-all ${
                      assignment.isActive
                        ? 'bg-green-50 border-green-300'
                        : 'bg-gray-50 border-gray-300 opacity-60'
                    }`}
                  >
                    <div className="flex justify-between items-start">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <span className="text-lg font-semibold text-gray-800">
                            {assignment.className}
                          </span>
                          {assignment.isActive && (
                            <span className="px-2 py-1 bg-green-500 text-white text-xs rounded-full">
                              Active
                            </span>
                          )}
                          {!assignment.isActive && (
                            <span className="px-2 py-1 bg-gray-500 text-white text-xs rounded-full">
                              Removed
                            </span>
                          )}
                        </div>
                        <div className="text-gray-700">
                          <span className="font-medium">Subject:</span> {assignment.subjectName}
                          {assignment.subjectCode && ` (${assignment.subjectCode})`}
                        </div>
                        <div className="text-sm text-gray-600 mt-1">
                          Assigned: {new Date(assignment.assignmentDate).toLocaleDateString()}
                        </div>
                        {assignment.removalDate && (
                          <div className="text-sm text-gray-600">
                            Removed: {new Date(assignment.removalDate).toLocaleDateString()}
                          </div>
                        )}
                      </div>
                      {assignment.isActive && (
                        <button
                          onClick={() => handleRemove(assignment.id)}
                          disabled={removeAssignmentMutation.isPending}
                          className="ml-4 px-3 py-1 bg-red-500 text-white rounded-lg hover:bg-red-600 transition-colors text-sm disabled:opacity-50"
                        >
                          Remove
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

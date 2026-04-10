import { useState, useEffect, useCallback } from 'react';
import {
  Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Tooltip, CircularProgress, Alert,
  MenuItem, Select, FormControl, InputLabel, Avatar,
  type SelectChangeEvent,
} from '@mui/material';
import {
  Plus, Edit2, Trash2, Search, Building2, Users, User,
  RefreshCcw, Building, MoreVertical
} from 'lucide-react';
import toast from 'react-hot-toast';
import { departmentApi, StaffApi, type DepartmentListItem, type CreateDepartmentDto, type UpdateDepartmentDto, type Staff } from '../services/api';

export const DepartmentPage = () => {
  const [departments, setDepartments] = useState<DepartmentListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedDepartment, setSelectedDepartment] = useState<DepartmentListItem | null>(null);
  const [staffList, setStaffList] = useState<Staff[]>([]);
  const [formData, setFormData] = useState<CreateDepartmentDto>({
    name: '',
    description: '',
    headOfDepartmentId: undefined,
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchDepartments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await departmentApi.getAll(searchTerm || undefined);
      setDepartments(data);
    } catch (err) {
      setError('Failed to load departments');
      console.error(err);
    } finally {
      setLoading(false);
    }
  }, [searchTerm]);

  const fetchStaff = async () => {
    try {
      const result = await StaffApi.getAll({ isActive: true, pageSize: 200 });
      setStaffList(result.items || []);
    } catch {
      console.error('Failed to load staff');
    }
  };

  useEffect(() => {
    fetchDepartments();
  }, [fetchDepartments]);

  const handleOpenCreate = () => {
    setSelectedDepartment(null);
    setFormData({ name: '', description: '', headOfDepartmentId: undefined });
    fetchStaff();
    setDialogOpen(true);
  };

  const handleOpenEdit = (dept: DepartmentListItem) => {
    setSelectedDepartment(dept);
    setFormData({
      name: dept.name,
      description: dept.description || '',
      headOfDepartmentId: undefined,
    });
    // Fetch full details to get headOfDepartmentId
    departmentApi.getById(dept.id).then((full) => {
      if (full) {
        setFormData({
          name: full.name,
          description: full.description || '',
          headOfDepartmentId: full.headOfDepartmentId || undefined,
        });
      }
    });
    fetchStaff();
    setDialogOpen(true);
  };

  const handleOpenDelete = (dept: DepartmentListItem, e: React.MouseEvent) => {
    e.stopPropagation();
    setSelectedDepartment(dept);
    setDeleteDialogOpen(true);
  };

  const handleSave = async () => {
    if (!formData.name.trim()) {
      toast.error('Department name is required');
      return;
    }
    setSaving(true);
    try {
      if (selectedDepartment) {
        const updateData: UpdateDepartmentDto = {
          name: formData.name,
          description: formData.description,
          headOfDepartmentId: formData.headOfDepartmentId,
        };
        await departmentApi.update(selectedDepartment.id, updateData);
        toast.success('Department updated successfully');
      } else {
        await departmentApi.create(formData);
        toast.success('Department created successfully');
      }
      setDialogOpen(false);
      fetchDepartments();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      toast.error(axiosErr?.response?.data?.message || 'Failed to save department');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async () => {
    if (!selectedDepartment) return;
    setSaving(true);
    try {
      await departmentApi.delete(selectedDepartment.id);
      toast.success('Department deleted successfully');
      setDeleteDialogOpen(false);
      fetchDepartments();
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { message?: string } } };
      toast.error(axiosErr?.response?.data?.message || 'Failed to delete department');
    } finally {
      setSaving(false);
    }
  };

  const getInitials = (name: string) => {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().substring(0, 2);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 py-8">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="space-y-6">
          {/* Header Section */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent flex items-center gap-3">
                <Building2 size={36} className="text-blue-600" />
                Department Management
              </h1>
              <p className="text-gray-600 mt-2 font-medium">Create and organize school departments and faculty</p>
            </div>
            <button
              onClick={handleOpenCreate}
              className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-bold shadow-md whitespace-nowrap"
            >
              <Plus size={20} strokeWidth={3} />
              Add Department
            </button>
          </div>

          {/* Search Bar Container */}
          <div className="bg-white p-2 rounded-2xl shadow-sm border border-gray-100 flex items-center gap-2 max-w-2xl">
            <div className="flex-grow relative">
              <div className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                <Search size={20} className="text-gray-400" />
              </div>
              <input
                type="text"
                placeholder="Search departments..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2 bg-slate-50 border-none rounded-xl focus:ring-2 focus:ring-blue-500 transition-all font-medium text-gray-700"
              />
            </div>
            <Tooltip title="Refresh">
              <button 
                onClick={fetchDepartments}
                className="p-2 text-blue-600 hover:bg-blue-50 rounded-xl transition-colors"
              >
                <RefreshCcw size={20} />
              </button>
            </Tooltip>
          </div>

          {/* Content Table Container */}
          {loading ? (
            <div className="flex items-center justify-center p-20 bg-white rounded-2xl shadow-lg border border-gray-100">
              <CircularProgress size={50} thickness={5} sx={{ color: '#2563eb' }} />
            </div>
          ) : error ? (
            <Alert severity="error" className="rounded-2xl font-bold">{error}</Alert>
          ) : departments.length === 0 ? (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-20 text-center">
              <div className="w-20 h-20 bg-blue-50 rounded-full flex items-center justify-center mx-auto mb-6">
                <Building size={40} className="text-blue-200" />
              </div>
              <h3 className="text-xl font-bold text-gray-900">No Departments Found</h3>
              <p className="text-gray-500 mt-2 max-w-sm mx-auto">Click the button above to create your institution's first department.</p>
            </div>
          ) : (
            <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-xl transition-shadow duration-300">
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                    <tr>
                      <th className="px-6 py-5 text-left text-sm font-bold text-gray-900">Department</th>
                      <th className="px-6 py-5 text-left text-sm font-bold text-gray-900">Head of Department</th>
                      <th className="px-6 py-5 text-left text-sm font-bold text-gray-900">Staff Count</th>
                      <th className="px-6 py-5 text-left text-sm font-bold text-gray-900">Description</th>
                      <th className="px-6 py-5 text-right text-sm font-bold text-gray-900">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {departments.map((dept) => (
                      <tr key={dept.id} className="hover:bg-blue-50/50 transition-colors duration-200 group">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="flex items-center gap-3">
                            <div className="w-10 h-10 bg-blue-100 rounded-xl flex items-center justify-center text-blue-600 group-hover:bg-blue-600 group-hover:text-white transition-all duration-300">
                              <Building2 size={22} />
                            </div>
                            <span className="text-sm font-bold text-gray-900 tracking-tight">{dept.name}</span>
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          {dept.headOfDepartmentName ? (
                            <div className="flex items-center gap-3">
                              <Avatar sx={{ 
                                width: 34, height: 34, fontSize: '0.8rem', fontWeight: 800,
                                bgcolor: 'white', color: '#1e293b', border: '1px solid #e2e8f0', shadow: 'sm'
                              }}>
                                {getInitials(dept.headOfDepartmentName)}
                              </Avatar>
                              <div className="text-sm font-semibold text-gray-700">{dept.headOfDepartmentName}</div>
                            </div>
                          ) : (
                            <div className="flex items-center gap-2 text-gray-400">
                              <User size={16} />
                              <span className="text-xs font-medium italic">Not assigned</span>
                            </div>
                          )}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`px-4 py-1 rounded-full text-xs font-bold ring-1 ring-inset ${
                            dept.staffCount > 0 ? 'bg-blue-50 text-blue-700 ring-blue-700/20' : 'bg-gray-50 text-gray-500 ring-gray-500/20'
                          }`}>
                            <Users size={12} className="inline mr-1.5" strokeWidth={3} />
                            {dept.staffCount} Staff{dept.staffCount !== 1 ? 's' : ''}
                          </span>
                        </td>
                        <td className="px-6 py-4">
                          <p className="text-sm text-gray-500 line-clamp-1 max-w-xs font-medium">
                            {dept.description || 'No description provided.'}
                          </p>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right">
                          <div className="flex items-center justify-end gap-1">
                            <button
                              onClick={() => handleOpenEdit(dept)}
                              className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all duration-200"
                              title="Edit"
                            >
                              <Edit2 size={18} />
                            </button>
                            <button
                              onClick={(e) => handleOpenDelete(dept, e)}
                              className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-all duration-200"
                              title="Delete"
                            >
                              <Trash2 size={18} />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        {/* Create/Edit Dialog */}
        <Dialog 
          open={dialogOpen} 
          onClose={() => setDialogOpen(false)} 
          maxWidth="sm" 
          fullWidth
          PaperProps={{
            sx: { borderRadius: 6, p: 1 }
          }}
        >
          <DialogTitle className="flex items-center gap-3">
            <div className={`p-2 rounded-xl ${selectedDepartment ? 'bg-blue-50 text-blue-600' : 'bg-green-50 text-green-600'}`}>
              {selectedDepartment ? <Edit2 size={24} /> : <Plus size={24} />}
            </div>
            <span className="text-2xl font-bold text-gray-900">
              {selectedDepartment ? 'Edit Department' : 'Add New Department'}
            </span>
          </DialogTitle>
          <DialogContent>
            <p className="text-sm text-gray-500 mb-6 font-medium">Configure the core details of your institution's department.</p>
            <div className="space-y-6 pt-2">
              <TextField
                label="Department Name"
                fullWidth
                required
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                variant="outlined"
                InputProps={{ sx: { borderRadius: 3, fontWeight: 600 } }}
              />
              <TextField
                label="Description"
                fullWidth
                multiline
                rows={4}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                variant="outlined"
                InputProps={{ sx: { borderRadius: 3, fontWeight: 500 } }}
              />
              <FormControl fullWidth variant="outlined">
                <InputLabel sx={{ fontWeight: 600 }}>Head of Department</InputLabel>
                <Select
                  value={formData.headOfDepartmentId || ''}
                  label="Head of Department"
                  onChange={(e: SelectChangeEvent) =>
                    setFormData({ ...formData, headOfDepartmentId: e.target.value || undefined })
                  }
                  sx={{ borderRadius: 3, fontWeight: 600 }}
                >
                  <MenuItem value="">
                    <em className="text-gray-400 font-medium">Unassigned (Vacant)</em>
                  </MenuItem>
                  {staffList.map((s) => (
                    <MenuItem key={s.id} value={s.id} className="font-semibold">
                      {s.firstName} {s.lastName}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </div>
          </DialogContent>
          <DialogActions className="p-6 pt-2">
            <Button 
              onClick={() => setDialogOpen(false)} 
              disabled={saving}
              className="px-6 py-2 text-gray-600 font-bold hover:bg-gray-50 rounded-xl"
              sx={{ textTransform: 'none', fontWeight: 800, color: '#64748b' }}
            >
              Cancel
            </Button>
            <Button 
              variant="contained" 
              onClick={handleSave} 
              disabled={saving}
              className="px-8 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-xl shadow-md"
              sx={{ textTransform: 'none', fontWeight: 800, borderRadius: 3 }}
            >
              {saving ? <CircularProgress size={20} color="inherit" /> : selectedDepartment ? 'Update Department' : 'Create Department'}
            </Button>
          </DialogActions>
        </Dialog>

        {/* Delete Confirmation */}
        <Dialog 
          open={deleteDialogOpen} 
          onClose={() => setDeleteDialogOpen(false)}
          PaperProps={{ sx: { borderRadius: 6, p: 1 } }}
        >
          <DialogTitle className="flex items-center gap-3 text-red-600 font-bold">
            <div className="p-2 bg-red-50 rounded-xl">
              <Trash2 size={24} />
            </div>
            Confirm Deletion
          </DialogTitle>
          <DialogContent className="mt-2">
            <p className="text-gray-600 font-medium leading-relaxed">
              Are you sure you want to delete the <span className="text-gray-900 font-bold">{selectedDepartment?.name}</span> department? This action is irreversible.
            </p>
            {selectedDepartment && selectedDepartment.staffCount > 0 && (
              <div className="mt-4 p-4 bg-red-50 border border-red-100 rounded-2xl flex gap-3 text-red-800">
                <MoreVertical size={20} className="flex-shrink-0" />
                <p className="text-sm font-bold">
                  DEPENDENCY WARNING: This department still has {selectedDepartment.staffCount} staff members. Please reassign them before termination.
                </p>
              </div>
            )}
          </DialogContent>
          <DialogActions className="p-6">
            <Button onClick={() => setDeleteDialogOpen(false)} sx={{ fontWeight: 800, color: '#64748b' }}>Cancel</Button>
            <Button 
              variant="contained" 
              color="error" 
              onClick={handleDelete} 
              disabled={saving || (selectedDepartment?.staffCount ?? 0) > 0}
              sx={{ fontWeight: 800, borderRadius: 3, px: 4 }}
            >
              {saving ? <CircularProgress size={20} color="inherit" /> : 'Delete Permanently'}
            </Button>
          </DialogActions>
        </Dialog>
      </div>
    </div>
  );
};

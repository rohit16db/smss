import React, { useState, useEffect, useRef } from 'react';
import { 
  Dialog, 
  DialogContent, 
  DialogActions, 
  TextField,
  Autocomplete
} from '@mui/material';
import { 
  DirectionsBus, 
  Add, 
  Edit, 
  Delete, 
  Sync, 
  Route, 
  People,
  LocalShipping
} from '@mui/icons-material';
import { transportService, type Vehicle, type TransportRoute } from '../services/transportService';
import { studentApi, type Student } from '../services/api';
import toast from 'react-hot-toast';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`transport-tabpanel-${index}`}
      aria-labelledby={`transport-tab-${index}`}
      {...other}
      className={value === index ? 'block animate-in fade-in duration-500' : 'hidden'}
    >
      {value === index && (
        <div className="py-6">
          {children}
        </div>
      )}
    </div>
  );
}

export const TransportManagementPage: React.FC = () => {
  const [tabValue, setTabValue] = useState(0);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [routes, setRoutes] = useState<TransportRoute[]>([]);
  const [assignments, setAssignments] = useState<any[]>([]);
  const [students, setStudents] = useState<Student[]>([]);
  const [loading, setLoading] = useState(true);
  
  // Searchable Student State (Reference from Fees module)
  const [studentSearch, setStudentSearch] = useState('');
  const [showStudentDropdown, setShowStudentDropdown] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const [selectedStudentData, setSelectedStudentData] = useState<Student | null>(null);
  
  // Assignment Modal State
  const [assignModalOpen, setAssignModalOpen] = useState(false);
  const [selectedRoute, setSelectedRoute] = useState<string>('');
  const [selectedStop, setSelectedStop] = useState<string>('');
  const [effectiveDate, setEffectiveDate] = useState(new Date().toISOString().split('T')[0]);


  // Vehicle Modal State
  const [vehicleModalOpen, setVehicleModalOpen] = useState(false);
  const [editingVehicleId, setEditingVehicleId] = useState<string | null>(null);
  const [newVehicle, setNewVehicle] = useState({
    registrationNumber: '',
    model: '',
    capacity: 15,
    driverName: '',
    driverPhone: '',
    isActive: true
  });

  // Route Modal State
  const [routeModalOpen, setRouteModalOpen] = useState(false);
  const [editingRouteId, setEditingRouteId] = useState<string | null>(null);
  const [newRoute, setNewRoute] = useState({
    routeName: '',
    description: '',
    vehicleId: '',
    monthlyFee: 0,
    isActive: true,
    stops: [] as any[]
  });

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [vData, rData, sData, aData] = await Promise.all([
        transportService.getVehicles(),
        transportService.getRoutes(),
        studentApi.getAll({ pageSize: 100 }),
        transportService.getAssignments()
      ]);
      setVehicles(vData);
      setRoutes(rData);
      setStudents(sData.items || []);
      setAssignments(aData || []);
    } catch (error) {
      console.error('Failed to fetch transport data', error);
      toast.error('Failed to load transport data');
    } finally {
      setLoading(false);
    }
  };

  // Handle click outside dropdown (Reference from Fees module)
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setShowStudentDropdown(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleStudentSearchChange = async (value: string) => {
    setStudentSearch(value);
    if (value.length >= 2) {
      setShowStudentDropdown(true);
      try {
        const response = await studentApi.getAll({ searchTerm: value, pageSize: 10, isActive: true });
        setStudents(response.items || []);
      } catch (error) {
        console.error('Failed to search students', error);
      }
    } else {
      setShowStudentDropdown(false);
      setSelectedStudentData(null);
    }
  };

  const handleSelectStudent = (student: Student) => {
    setSelectedStudentData(student);
    setStudentSearch(`${student.firstName} ${student.lastName} (${student.enrollmentNumber})`);
    setShowStudentDropdown(false);
  };

  const handleDeactivateAssignment = async (id: string) => {
    if (window.confirm('Are you sure you want to remove this student from transport?')) {
      try {
        await transportService.deactivateAssignment(id);
        toast.success('Student removed from transport successfully');
        fetchData();
      } catch (error) {
        console.error('Failed to deactivate assignment', error);
        toast.error('Failed to remove student from transport');
      }
    }
  };

  const handleAssignStudent = async () => {
    if (!selectedStudentData || !selectedRoute) {
      toast.error('Please select both a student and a route');
      return;
    }

    try {
      await transportService.assignStudent({
        enrollmentId: selectedStudentData.id,
        routeId: selectedRoute,
        routeStopId: selectedStop || undefined,
        effectiveDate: new Date(effectiveDate).toISOString()
      });
      toast.success('Student assigned to transport successfully');
      setAssignModalOpen(false);
      setSelectedStudentData(null);
      setStudentSearch('');
      setSelectedRoute('');
      setSelectedStop('');
      fetchData();
    } catch (error) {
      console.error('Failed to assign student', error);
      toast.error('Failed to assign student to transport');
    }
  };

  const handleAddVehicle = async () => {
    if (!newVehicle.registrationNumber || !newVehicle.model) {
      toast.error('Registration number and model are required');
      return;
    }

    try {
      if (editingVehicleId) {
        await transportService.updateVehicle(editingVehicleId, newVehicle);
        toast.success('Vehicle updated successfully');
      } else {
        await transportService.addVehicle(newVehicle);
        toast.success('Vehicle added successfully');
      }
      setVehicleModalOpen(false);
      setEditingVehicleId(null);
      setNewVehicle({
        registrationNumber: '',
        model: '',
        capacity: 15,
        driverName: '',
        driverPhone: '',
        isActive: true
      });
      fetchData(); // Refresh list
    } catch (error) {
      console.error('Failed to save vehicle', error);
      toast.error('Failed to save vehicle');
    }
  };

  const handleEditVehicle = (vehicle: Vehicle) => {
    setEditingVehicleId(vehicle.id);
    setNewVehicle({
      registrationNumber: vehicle.registrationNumber,
      model: vehicle.model,
      capacity: vehicle.capacity,
      driverName: vehicle.driverName,
      driverPhone: vehicle.driverPhone,
      isActive: vehicle.isActive
    });
    setVehicleModalOpen(true);
  };

  const handleDeleteVehicle = async (id: string) => {
    if (!confirm('Are you sure you want to delete this vehicle?')) return;
    try {
      await transportService.deleteVehicle(id);
      toast.success('Vehicle deleted successfully');
      fetchData();
    } catch (error) {
      toast.error('Failed to delete vehicle');
    }
  };

  const handleAddRoute = async () => {
    if (!newRoute.routeName || !newRoute.monthlyFee) {
      toast.error('Route name and monthly fee are required');
      return;
    }

    try {
      if (editingRouteId) {
        await transportService.updateRoute(editingRouteId, { ...newRoute, id: editingRouteId } as any);
        toast.success('Route updated successfully');
      } else {
        await transportService.addRoute(newRoute as any);
        toast.success('Route added successfully');
      }
      setRouteModalOpen(false);
      setEditingRouteId(null);
      setNewRoute({
        routeName: '',
        description: '',
        vehicleId: '',
        monthlyFee: 0,
        isActive: true,
        stops: []
      });
      fetchData();
    } catch (error) {
      console.error('Failed to save route', error);
      toast.error('Failed to save route');
    }
  };

  const handleEditRoute = (route: TransportRoute) => {
    setEditingRouteId(route.id);
    setNewRoute({
      routeName: route.routeName,
      description: route.description,
      vehicleId: route.vehicleId || '',
      monthlyFee: route.monthlyFee,
      isActive: route.isActive,
      stops: route.stops.map(s => ({ ...s }))
    });
    setRouteModalOpen(true);
  };

  const handleDeleteRoute = async (id: string) => {
    if (!confirm('Are you sure you want to delete this route?')) return;
    try {
      await transportService.deleteRoute(id);
      toast.success('Route deleted successfully');
      fetchData();
    } catch (error) {
      toast.error('Failed to delete route');
    }
  };

  const handleSyncFees = async () => {
    try {
      await transportService.syncFees({ allStudents: true });
      toast.success('Transport fees recalculated for all students');
    } catch (error) {
      toast.error('Failed to recalculate transport fees');
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  const getTabIcon = (index: number) => {
    switch (index) {
      case 0: return <LocalShipping className="w-5 h-5" />;
      case 1: return <Route className="w-5 h-5" />;
      case 2: return <People className="w-5 h-5" />;
      default: return null;
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-8">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-6">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent flex items-center gap-3">
                <DirectionsBus className="w-10 h-10 text-blue-600" />
                Transport Management
              </h1>
              <p className="text-gray-600 mt-2">Manage school vehicles, routes, and student assignments</p>
            </div>
            <div className="flex items-center gap-3 w-full sm:w-auto">
              <button 
                onClick={handleSyncFees}
                className="flex items-center gap-2 px-5 py-2.5 bg-white text-gray-700 border border-gray-200 rounded-xl hover:bg-gray-50 hover:shadow-md transition-all duration-300 font-medium whitespace-nowrap"
              >
                <Sync className="w-5 h-5" />
                Recalculate Fees
              </button>
              <button 
                onClick={() => {
                  if (tabValue === 0) {
                    setEditingVehicleId(null);
                    setNewVehicle({ registrationNumber: '', model: '', capacity: 15, driverName: '', driverPhone: '', isActive: true });
                    setVehicleModalOpen(true);
                  }
                  else if (tabValue === 1) {
                    setEditingRouteId(null);
                    setNewRoute({ routeName: '', description: '', vehicleId: '', monthlyFee: 0, isActive: true, stops: [] });
                    setRouteModalOpen(true);
                  }
                  else setAssignModalOpen(true);
                }}
                className="flex items-center gap-2 px-6 py-2.5 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap shadow-blue-200 shadow-lg"
              >
                <Add className="w-5 h-5" />
                {tabValue === 0 ? 'Add Vehicle' : tabValue === 1 ? 'Add Route' : 'Assign Student'}
              </button>
            </div>
          </div>

          {/* Custom Tabs */}
          <div className="bg-white p-1.5 rounded-2xl shadow-md border border-gray-100 flex gap-2 w-full max-w-2xl mx-auto overflow-x-auto">
            {['Vehicles', 'Routes', 'Active Assignments'].map((label, idx) => (
              <button
                key={idx}
                onClick={() => setTabValue(idx)}
                className={`flex-1 flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl text-sm font-semibold transition-all duration-300 whitespace-nowrap ${
                  tabValue === idx 
                    ? 'bg-gradient-to-r from-blue-600 to-blue-700 text-white shadow-md scale-[1.02]' 
                    : 'text-gray-500 hover:bg-slate-50 hover:text-gray-700'
                }`}
              >
                {getTabIcon(idx)}
                {label}
              </button>
            ))}
          </div>

          <div className="mt-8 transition-all duration-500">
            {/* Vehicles Panel */}
            <TabPanel value={tabValue} index={0}>
              <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden hover:shadow-2xl transition-shadow duration-300">
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                      <tr>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Registration #</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Model</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Capacity</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Driver Info</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Status</th>
                        <th className="px-6 py-4 text-right text-xs font-bold text-gray-900 uppercase tracking-wider">Actions</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100 bg-white">
                      {vehicles.map((vehicle) => (
                        <tr key={vehicle.id} className="hover:bg-blue-50/50 transition-colors duration-200 group">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className="text-sm font-bold text-blue-700 bg-blue-50 px-3 py-1 rounded-lg border border-blue-100">
                              {vehicle.registrationNumber}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-700">
                            {vehicle.model}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="flex items-center gap-1.5">
                              <span className="text-sm font-medium text-gray-900">{vehicle.capacity}</span>
                              <span className="text-xs text-gray-500 uppercase">seats</span>
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="text-sm font-semibold text-gray-900">{vehicle.driverName}</div>
                            <div className="text-xs text-gray-500 flex items-center gap-1">
                              <span className="text-blue-500">☎</span> {vehicle.driverPhone}
                            </div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className={`px-3 py-1 rounded-full text-xs font-bold ${
                              vehicle.isActive 
                                ? 'bg-green-100 text-green-700 border border-green-200' 
                                : 'bg-gray-100 text-gray-700 border border-gray-200'
                            }`}>
                              {vehicle.isActive ? 'Active' : 'Inactive'}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                            <div className="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-all duration-200">
                              <button
                                onClick={() => handleEditVehicle(vehicle)}
                                className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-all"
                                title="Edit"
                              >
                                <Edit className="w-5 h-5" />
                              </button>
                              <button
                                onClick={() => handleDeleteVehicle(vehicle.id)}
                                className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-all"
                                title="Delete"
                              >
                                <Delete className="w-5 h-5" />
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                      {vehicles.length === 0 && (
                        <tr>
                          <td colSpan={6} className="px-6 py-12 text-center text-gray-500 italic">
                            No vehicles found. Click "Add Vehicle" to register one.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </div>
            </TabPanel>

            <TabPanel value={tabValue} index={1}>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {routes.map((route) => (
                  <div 
                    key={route.id}
                    className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden hover:shadow-2xl hover:-translate-y-1 transition-all duration-300 group"
                  >
                    <div className="bg-gradient-to-r from-blue-50 to-indigo-50 px-6 py-4 border-b border-blue-100 flex justify-between items-center">
                      <div className="flex flex-col">
                        <h3 className="text-lg font-bold text-gray-900 group-hover:text-blue-700 transition-colors">
                          {route.routeName}
                        </h3>
                        <span className="text-xs font-semibold text-blue-600 bg-blue-100/50 px-2 py-0.5 rounded w-fit">
                          ₹{route.monthlyFee}/month
                        </span>
                      </div>
                      <div className="flex gap-1">
                        <button
                          onClick={() => handleEditRoute(route)}
                          className="p-1.5 text-blue-600 hover:bg-white rounded-lg shadow-sm transition-all"
                        >
                          <Edit className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => handleDeleteRoute(route.id)}
                          className="p-1.5 text-red-600 hover:bg-white rounded-lg shadow-sm transition-all"
                        >
                          <Delete className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                    
                    <div className="p-6 space-y-4">
                      <p className="text-sm text-gray-600 italic line-clamp-2 min-h-[2.5rem]">
                        {route.description || 'No description provided'}
                      </p>
                      
                      <div className="flex items-center gap-3 text-sm text-gray-700 bg-slate-50 p-3 rounded-xl border border-slate-100">
                        <LocalShipping className="w-5 h-5 text-blue-500" />
                        <div>
                          <p className="text-xs text-gray-500 font-bold uppercase tracking-wider">Assigned Vehicle</p>
                          <p className="font-semibold">{route.vehicleRegistrationNumber || 'Not assigned'}</p>
                        </div>
                      </div>

                      <div className="space-y-2">
                        <p className="text-xs font-bold text-gray-500 uppercase tracking-wider flex justify-between">
                          Route Stops <span>{route.stops.length} stops</span>
                        </p>
                        <div className="flex flex-wrap gap-1.5">
                          {route.stops.slice(0, 3).map((stop, sIdx) => (
                            <span key={sIdx} className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700 border border-blue-100">
                              {stop.stopName}
                            </span>
                          ))}
                          {route.stops.length > 3 && (
                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-50 text-gray-500 border border-gray-100">
                              +{route.stops.length - 3} more
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
                {routes.length === 0 && (
                  <div className="col-span-full py-12 text-center text-gray-500 bg-white rounded-2xl border border-dashed border-gray-300">
                    No routes found. Click "Add Route" to define one.
                  </div>
                )}
              </div>
            </TabPanel>

            <TabPanel value={tabValue} index={2}>
              <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden hover:shadow-2xl transition-shadow duration-300">
                <div className="px-6 py-4 border-b border-gray-100 bg-gray-50/50 flex justify-between items-center">
                  <h3 className="text-lg font-bold text-gray-900">Current Transport Users</h3>
                  <div className="text-sm text-gray-500 font-medium">
                    Total: {assignments.length} students
                  </div>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead className="bg-gradient-to-r from-blue-50 to-indigo-50 border-b-2 border-blue-100">
                      <tr>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Student</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Class/Section</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Route & Vehicle</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Monthly Fee</th>
                        <th className="px-6 py-4 text-left text-xs font-bold text-gray-900 uppercase tracking-wider">Effective Since</th>
                        <th className="px-6 py-4 text-right text-xs font-bold text-gray-900 uppercase tracking-wider">Actions</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100 bg-white text-sm">
                      {assignments.map((assignment) => (
                        <tr key={assignment.id} className="hover:bg-blue-50/50 transition-colors duration-200">
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="font-bold text-gray-900">{assignment.studentName}</div>
                            <div className="text-xs text-gray-500">{assignment.enrollmentNumber}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <span className="bg-slate-100 text-slate-700 px-2.5 py-1 rounded-lg font-medium text-xs">
                              {assignment.classSection}
                            </span>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap">
                            <div className="font-semibold text-blue-700">{assignment.routeName}</div>
                            <div className="text-xs text-gray-500 italic">{assignment.vehicleReg}</div>
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap font-bold text-gray-900">
                            ₹{assignment.monthlyFee}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-gray-600">
                            {new Date(assignment.effectiveDate).toLocaleDateString()}
                          </td>
                          <td className="px-6 py-4 whitespace-nowrap text-right">
                            <button
                              onClick={() => handleDeactivateAssignment(assignment.id)}
                              className="text-red-600 hover:bg-red-100 px-3 py-1.5 rounded-lg border border-red-200 transition-all text-xs font-bold uppercase tracking-wider"
                            >
                              Remove
                            </button>
                          </td>
                        </tr>
                      ))}
                      {assignments.length === 0 && (
                        <tr>
                          <td colSpan={6} className="px-6 py-12 text-center text-gray-500 italic">
                            No students currently assigned to transport.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              </div>
            </TabPanel>
          </div>
        </div>
      </div>

      {/* Modals Updated with Branded Headers */}
      <Dialog 
        open={assignModalOpen} 
        onClose={() => setAssignModalOpen(false)} 
        maxWidth="sm" 
        fullWidth
        PaperProps={{ sx: { borderRadius: '20px', overflow: 'hidden' } }}
      >
        <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
          <h2 className="text-xl font-bold text-white flex items-center gap-2">
            <Add className="w-6 h-6" />
            Assign Student to Transport
          </h2>
        </div>
        <DialogContent sx={{ p: 4 }}>
          <div className="space-y-6 pt-2">
            <div className="relative" ref={dropdownRef}>
              <label className="block text-sm font-bold text-gray-700 mb-2 uppercase tracking-wide">
                Search Student
              </label>
              <div className="relative group">
                <People className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-blue-500 transition-colors" />
                <input
                  type="text"
                  value={studentSearch}
                  onChange={(e) => handleStudentSearchChange(e.target.value)}
                  placeholder="Type Name or Enrollment ID..."
                  className="w-full pl-12 pr-4 py-4 bg-gray-50 border-2 border-gray-100 rounded-2xl focus:bg-white focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 transition-all outline-none text-gray-900 font-medium"
                />
              </div>

              {showStudentDropdown && students.length > 0 && (
                <div className="absolute z-50 w-full mt-2 bg-white rounded-2xl shadow-2xl border border-gray-100 overflow-hidden animate-in fade-in slide-in-from-top-2 duration-200">
                  <div className="max-h-64 overflow-y-auto">
                    {students.map((student) => (
                      <div
                        key={student.id}
                        onClick={() => handleSelectStudent(student)}
                        className="px-6 py-4 hover:bg-blue-50 cursor-pointer border-b border-gray-50 last:border-0 transition-colors group"
                      >
                        <div className="font-bold text-gray-900 group-hover:text-blue-700">{student.firstName} {student.lastName}</div>
                        <div className="text-xs text-gray-500 flex items-center gap-2 mt-1">
                          <span className="bg-slate-100 text-slate-600 px-2 py-0.5 rounded uppercase font-bold tracking-tighter">
                            {student.enrollmentNumber}
                          </span>
                          <span>{student.currentClassName} {student.currentSectionName}</span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>

            <div>
              <label className="block text-sm font-bold text-gray-700 mb-2 uppercase tracking-wide">
                Select Route
              </label>
              <div className="relative group">
                <Route className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-blue-500 transition-colors" />
                <select
                  value={selectedRoute}
                  onChange={(e) => {
                    setSelectedRoute(e.target.value);
                    setSelectedStop(''); // Reset stop when route changes
                  }}
                  className="w-full pl-12 pr-4 py-4 bg-gray-50 border-2 border-gray-100 rounded-2xl focus:bg-white focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 transition-all outline-none text-gray-900 font-medium appearance-none"
                >
                  <option value="">Select a transport route...</option>
                  {routes.filter(r => r.isActive).map((route) => (
                    <option key={route.id} value={route.id}>
                      {route.routeName} (₹{route.monthlyFee}/month)
                    </option>
                  ))}
                </select>
                <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-gray-400">
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7" />
                  </svg>
                </div>
              </div>
            </div>

            {selectedRoute && (
              <div className="animate-in fade-in slide-in-from-top-2 duration-300">
                <label className="block text-sm font-bold text-gray-700 mb-2 uppercase tracking-wide">
                  Select Stop Point
                </label>
                <div className="relative group">
                  <DirectionsBus className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400 group-focus-within:text-blue-500 transition-colors" />
                  <select
                    value={selectedStop}
                    onChange={(e) => setSelectedStop(e.target.value)}
                    className="w-full pl-12 pr-4 py-4 bg-gray-50 border-2 border-gray-100 rounded-2xl focus:bg-white focus:border-blue-500 focus:ring-4 focus:ring-blue-500/10 transition-all outline-none text-gray-900 font-medium appearance-none"
                  >
                    <option value="">Choose a stop...</option>
                    {routes.find(r => r.id === selectedRoute)?.stops.map((stop) => (
                      <option key={stop.id} value={stop.id}>
                        {stop.stopName} (Pickup: {stop.pickupTime})
                      </option>
                    ))}
                  </select>
                  <div className="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-gray-400">
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7" />
                    </svg>
                  </div>
                </div>
              </div>
            )}

            <TextField
              label="Effective Date"
              type="date"
              value={effectiveDate}
              onChange={(e) => setEffectiveDate(e.target.value)}
              fullWidth
              InputLabelProps={{ shrink: true }}
            />
          </div>
        </DialogContent>
        <DialogActions sx={{ p: 3, bgcolor: '#f8fafc', borderTop: '1px solid #e2e8f0' }}>
          <button 
            onClick={() => setAssignModalOpen(false)}
            className="px-6 py-2 text-gray-600 font-semibold hover:text-gray-800"
          >
            Cancel
          </button>
          <button 
            onClick={handleAssignStudent}
            className="px-8 py-2 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 shadow-lg shadow-blue-200 transition-all"
          >
            Save Assignment
          </button>
        </DialogActions>
      </Dialog>

      <Dialog 
        open={vehicleModalOpen} 
        onClose={() => { setVehicleModalOpen(false); setEditingVehicleId(null); }} 
        maxWidth="sm" 
        fullWidth
        PaperProps={{ sx: { borderRadius: '20px', overflow: 'hidden' } }}
      >
        <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
          <h2 className="text-xl font-bold text-white flex items-center gap-2">
            {editingVehicleId ? <Edit className="w-6 h-6" /> : <Add className="w-6 h-6" />}
            {editingVehicleId ? 'Edit Vehicle' : 'Add New Vehicle'}
          </h2>
        </div>
        <DialogContent sx={{ p: 4 }}>
          <div className="space-y-4 pt-4">
            <TextField
              label="Registration Number"
              fullWidth
              placeholder="e.g. MH-12-AB-1234"
              value={newVehicle.registrationNumber}
              onChange={(e) => setNewVehicle({ ...newVehicle, registrationNumber: e.target.value })}
            />
            <TextField
              label="Model"
              fullWidth
              placeholder="e.g. Force Traveller / Toyota Coaster"
              value={newVehicle.model}
              onChange={(e) => setNewVehicle({ ...newVehicle, model: e.target.value })}
            />
            <TextField
              label="Capacity (Seats)"
              type="number"
              fullWidth
              value={newVehicle.capacity}
              onChange={(e) => setNewVehicle({ ...newVehicle, capacity: parseInt(e.target.value) || 0 })}
            />
            <div className="grid grid-cols-2 gap-4">
              <TextField
                label="Driver Name"
                fullWidth
                value={newVehicle.driverName}
                onChange={(e) => setNewVehicle({ ...newVehicle, driverName: e.target.value })}
              />
              <TextField
                label="Driver Phone"
                fullWidth
                value={newVehicle.driverPhone}
                onChange={(e) => setNewVehicle({ ...newVehicle, driverPhone: e.target.value })}
              />
            </div>
          </div>
        </DialogContent>
        <DialogActions sx={{ p: 3, bgcolor: '#f8fafc', borderTop: '1px solid #e2e8f0' }}>
          <button 
            onClick={() => { setVehicleModalOpen(false); setEditingVehicleId(null); }}
            className="px-6 py-2 text-gray-600 font-semibold hover:text-gray-800"
          >
            Cancel
          </button>
          <button 
            onClick={handleAddVehicle}
            className="px-8 py-2 bg-blue-600 text-white rounded-xl font-bold hover:bg-blue-700 shadow-lg shadow-blue-200 transition-all"
          >
            {editingVehicleId ? 'Update Vehicle' : 'Save Vehicle'}
          </button>
        </DialogActions>
      </Dialog>

      <Dialog 
        open={routeModalOpen} 
        onClose={() => { setRouteModalOpen(false); setEditingRouteId(null); }} 
        maxWidth="md" 
        fullWidth
        PaperProps={{ sx: { borderRadius: '20px', overflow: 'hidden' } }}
      >
        <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4">
          <h2 className="text-xl font-bold text-white flex items-center gap-2">
            {editingRouteId ? <Edit className="w-6 h-6" /> : <Add className="w-6 h-6" />}
            {editingRouteId ? 'Edit Route' : 'Add New Route'}
          </h2>
        </div>
        <DialogContent sx={{ p: 4 }}>
          <div className="space-y-6 pt-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <TextField
                label="Route Name"
                fullWidth
                value={newRoute.routeName}
                onChange={(e) => setNewRoute({ ...newRoute, routeName: e.target.value })}
              />
              <TextField
                label="Monthly Fee (₹)"
                type="number"
                fullWidth
                value={newRoute.monthlyFee}
                onChange={(e) => setNewRoute({ ...newRoute, monthlyFee: parseFloat(e.target.value) || 0 })}
              />
            </div>
            <TextField
              label="Description"
              fullWidth
              multiline
              rows={2}
              value={newRoute.description}
              onChange={(e) => setNewRoute({ ...newRoute, description: e.target.value })}
            />
            <Autocomplete
              options={vehicles}
              getOptionLabel={(option) => `${option.registrationNumber} (${option.model})`}
              fullWidth
              value={vehicles.find(v => v.id === newRoute.vehicleId) || null}
              onChange={(_, newValue) => setNewRoute({ ...newRoute, vehicleId: newValue?.id || '' })}
              renderInput={(params) => <TextField {...params} label="Assign Vehicle" />}
            />
            
            <div className="pt-4 border-t border-gray-100">
              <div className="flex justify-between items-center mb-4">
                <h4 className="font-bold text-gray-800 flex items-center gap-2">
                  <Route className="w-5 h-5 text-blue-500" />
                  Route Stops
                </h4>
                <button 
                  onClick={() => {
                    setNewRoute({ ...newRoute, stops: [...newRoute.stops, { stopName: '', pickupTime: '08:00', dropoffTime: '15:00', sequence: newRoute.stops.length + 1 }] });
                  }}
                  className="text-sm font-bold text-blue-600 hover:text-blue-800"
                >
                  + Add Stop
                </button>
              </div>

              <div className="space-y-3 max-h-60 overflow-y-auto pr-2 custom-scrollbar">
                {newRoute.stops.map((stop, index) => (
                  <div key={index} className="flex gap-3 items-center bg-slate-50 p-3 rounded-xl border border-slate-200">
                    <div className="flex-grow">
                      <TextField 
                        label="Stop Name" 
                        size="small" 
                        fullWidth
                        value={stop.stopName} 
                        onChange={(e) => {
                          const updatedStops = [...newRoute.stops];
                          updatedStops[index].stopName = e.target.value;
                          setNewRoute({ ...newRoute, stops: updatedStops });
                        }}
                      />
                    </div>
                    <div className="w-32">
                      <TextField 
                        label="Pickup" 
                        size="small" 
                        type="time" 
                        InputLabelProps={{ shrink: true }}
                        value={stop.pickupTime}
                        onChange={(e) => {
                          const updatedStops = [...newRoute.stops];
                          updatedStops[index].pickupTime = e.target.value;
                          setNewRoute({ ...newRoute, stops: updatedStops });
                        }}
                      />
                    </div>
                    <div className="w-32">
                      <TextField 
                        label="Dropoff" 
                        size="small" 
                        type="time" 
                        InputLabelProps={{ shrink: true }}
                        value={stop.dropoffTime}
                        onChange={(e) => {
                          const updatedStops = [...newRoute.stops];
                          updatedStops[index].dropoffTime = e.target.value;
                          setNewRoute({ ...newRoute, stops: updatedStops });
                        }}
                      />
                    </div>
                    <button 
                      onClick={() => {
                        const updatedStops = newRoute.stops.filter((_, i) => i !== index);
                        setNewRoute({ ...newRoute, stops: updatedStops });
                      }}
                      className="p-2 text-red-500 hover:bg-red-50 rounded-lg"
                    >
                      <Delete className="w-5 h-5" />
                    </button>
                  </div>
                ))}
                {newRoute.stops.length === 0 && (
                  <div className="text-center py-6 text-gray-500 italic text-sm">
                    No stops added. Click "+ Add Stop" to start.
                  </div>
                )}
              </div>
            </div>
          </div>
        </DialogContent>
        <DialogActions sx={{ p: 3, bgcolor: '#f8fafc', borderTop: '1px solid #e2e8f0' }}>
          <button 
            onClick={() => { setRouteModalOpen(false); setEditingRouteId(null); }}
            className="px-6 py-2 text-gray-600 font-semibold hover:text-gray-800"
          >
            Cancel
          </button>
          <button 
            onClick={handleAddRoute}
            className="px-10 py-2 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl font-bold hover:shadow-xl transition-all"
          >
            {editingRouteId ? 'Update Route' : 'Save Route'}
          </button>
        </DialogActions>
      </Dialog>
    </div>
  );
};

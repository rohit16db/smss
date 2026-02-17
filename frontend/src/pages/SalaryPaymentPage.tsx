import { useState, useMemo } from 'react';
import {
  CheckCircle,
  Clock,
  XCircle,
  PauseCircle,
  DollarSign,
  Filter,
  Edit,
  Trash2,
  AlertCircle,
  RefreshCw
} from 'lucide-react';
import {
  useSalaryPayments,
  useSalaryPaymentsSummary,
  useUpdateSalaryPaymentStatus,
  useMarkSalaryAsPaid,
  useUpdateSalaryPayment,
  useDeleteSalaryPayment
} from '../services/salaryPaymentService';
import type {
  SalaryPaymentDto,
  UpdateSalaryPaymentStatusDto,
  MarkSalaryAsPaidDto,
  UpdateSalaryPaymentDto
} from '../types/salaryPayment';

export default function SalaryPaymentPage() {
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [periodStartDate, setPeriodStartDate] = useState<string>('');
  const [periodEndDate, setPeriodEndDate] = useState<string>('');
  const [selectedPayments, setSelectedPayments] = useState<Set<string>>(new Set());
  const [showStatusDialog, setShowStatusDialog] = useState(false);
  const [showPayDialog, setShowPayDialog] = useState(false);
  const [showEditDialog, setShowEditDialog] = useState(false);
  const [currentPayment, setCurrentPayment] = useState<SalaryPaymentDto | null>(null);
  const [bulkAction, setBulkAction] = useState(false);

  // Form states
  const [newStatus, setNewStatus] = useState<string>('');
  const [statusRemarks, setStatusRemarks] = useState('');
  const [paidDate, setPaidDate] = useState('');
  const [referenceNumber, setReferenceNumber] = useState('');
  const [paymentMethod, setPaymentMethod] = useState<string>('');
  const [editBaseSalary, setEditBaseSalary] = useState<number>(0);
  const [editDeductions, setEditDeductions] = useState<number>(0);
  const [editBonus, setEditBonus] = useState<number>(0);
  const [editRemarks, setEditRemarks] = useState('');

  // Fetch data
  const { data: payments = [], isLoading, refetch } = useSalaryPayments({
    status: statusFilter,
    periodStartDate,
    periodEndDate
  });

  const { data: summary } = useSalaryPaymentsSummary({
    periodStartDate,
    periodEndDate
  });

  // Mutations
  const updateStatusMutation = useUpdateSalaryPaymentStatus();
  const markPaidMutation = useMarkSalaryAsPaid();
  const updatePaymentMutation = useUpdateSalaryPayment();
  const deletePaymentMutation = useDeleteSalaryPayment();

  // Status badge styling
  const getStatusBadge = (status: string) => {
    const badges: Record<string, { bg: string; text: string; icon: React.ReactElement }> = {
      Pending: { bg: 'bg-yellow-100 text-yellow-800', text: 'Pending', icon: <Clock className="w-4 h-4" /> },
      Approved: { bg: 'bg-blue-100 text-blue-800', text: 'Approved', icon: <CheckCircle className="w-4 h-4" /> },
      Paid: { bg: 'bg-green-100 text-green-800', text: 'Paid', icon: <DollarSign className="w-4 h-4" /> },
      OnHold: { bg: 'bg-orange-100 text-orange-800', text: 'On Hold', icon: <PauseCircle className="w-4 h-4" /> },
      Cancelled: { bg: 'bg-red-100 text-red-800', text: 'Cancelled', icon: <XCircle className="w-4 h-4" /> }
    };
    const badge = badges[status] || badges.Pending;
    return (
      <span className={`inline-flex items-center gap-1 px-3 py-1 rounded-full text-sm font-medium ${badge.bg}`}>
        {badge.icon}
        {badge.text}
      </span>
    );
  };

  // Format currency
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 0
    }).format(amount);
  };

  // Handle status update
  const handleStatusUpdate = async () => {
    if (!newStatus) return;

    const paymentsToUpdate = bulkAction 
      ? Array.from(selectedPayments)
      : currentPayment ? [currentPayment.id] : [];

    try {
      for (const id of paymentsToUpdate) {
        const data: UpdateSalaryPaymentStatusDto = {
          status: newStatus,
          remarks: statusRemarks || undefined
        };
        await updateStatusMutation.mutateAsync({ id, data });
      }
      setShowStatusDialog(false);
      setSelectedPayments(new Set());
      setBulkAction(false);
      setNewStatus('');
      setStatusRemarks('');
    } catch (error) {
      console.error('Error updating status:', error);
    }
  };

  // Handle mark as paid
  const handleMarkAsPaid = async () => {
    if (!paidDate || !referenceNumber) return;

    const paymentsToUpdate = bulkAction
      ? Array.from(selectedPayments)
      : currentPayment ? [currentPayment.id] : [];

    try {
      for (const id of paymentsToUpdate) {
        const data: MarkSalaryAsPaidDto = {
          paidDate,
          referenceNumber: referenceNumber + (paymentsToUpdate.length > 1 ? `-${id.substring(0, 4)}` : ''),
          paymentMethod: paymentMethod || undefined
        };
        await markPaidMutation.mutateAsync({ id, data });
      }
      setShowPayDialog(false);
      setSelectedPayments(new Set());
      setBulkAction(false);
      setPaidDate('');
      setReferenceNumber('');
      setPaymentMethod('');
    } catch (error) {
      console.error('Error marking as paid:', error);
    }
  };

  // Handle edit payment
  const handleEditPayment = async () => {
    if (!currentPayment) return;

    try {
      const data: UpdateSalaryPaymentDto = {
        baseSalary: editBaseSalary !== currentPayment.baseSalary ? editBaseSalary : undefined,
        deductions: editDeductions !== currentPayment.deductions ? editDeductions : undefined,
        bonus: editBonus !== currentPayment.bonus ? editBonus : undefined,
        remarks: editRemarks || undefined
      };
      await updatePaymentMutation.mutateAsync({ id: currentPayment.id, data });
      setShowEditDialog(false);
      setCurrentPayment(null);
    } catch (error) {
      console.error('Error updating payment:', error);
    }
  };

  // Handle delete
  const handleDeletePayment = async (id: string) => {
    if (!confirm('Are you sure you want to delete this salary payment?')) return;

    try {
      await deletePaymentMutation.mutateAsync(id);
    } catch (error) {
      console.error('Error deleting payment:', error);
    }
  };

  // Open edit dialog
  const openEditDialog = (payment: SalaryPaymentDto) => {
    setCurrentPayment(payment);
    setEditBaseSalary(payment.baseSalary);
    setEditDeductions(payment.deductions);
    setEditBonus(payment.bonus);
    setEditRemarks(payment.remarks || '');
    setShowEditDialog(true);
  };

  // Calculate net salary preview
  const calculatedNetSalary = useMemo(() => {
    return editBaseSalary + editBonus - editDeductions;
  }, [editBaseSalary, editBonus, editDeductions]);

  // Toggle selection
  const toggleSelection = (id: string) => {
    const newSelection = new Set(selectedPayments);
    if (newSelection.has(id)) {
      newSelection.delete(id);
    } else {
      newSelection.add(id);
    }
    setSelectedPayments(newSelection);
  };

  // Select all
  const toggleSelectAll = () => {
    if (selectedPayments.size === payments.length) {
      setSelectedPayments(new Set());
    } else {
      setSelectedPayments(new Set(payments.map(p => p.id)));
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 p-6">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Header */}
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
              Salary Payment Management
            </h1>
            <p className="text-gray-600 mt-2">Manage and track all salary payments</p>
          </div>
          <button
            onClick={() => refetch()}
            className="flex items-center gap-2 px-4 py-2 bg-white border-2 border-blue-500 text-blue-600 rounded-xl hover:bg-blue-50 transition-all duration-300 hover:scale-105 shadow-lg"
          >
            <RefreshCw className="w-5 h-5" />
            Refresh
          </button>
        </div>

        {/* Summary Cards */}
        {summary && (
          <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
            <div className="bg-gradient-to-br from-yellow-50 to-yellow-100 rounded-2xl shadow-lg p-4 hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-yellow-600 font-medium">Pending</p>
                  <p className="text-3xl font-bold text-yellow-800">{summary.pendingCount}</p>
                </div>
                <Clock className="w-10 h-10 text-yellow-500" />
              </div>
            </div>

            <div className="bg-gradient-to-br from-blue-50 to-blue-100 rounded-2xl shadow-lg p-4 hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-blue-600 font-medium">Approved</p>
                  <p className="text-3xl font-bold text-blue-800">{summary.approvedCount}</p>
                </div>
                <CheckCircle className="w-10 h-10 text-blue-500" />
              </div>
            </div>

            <div className="bg-gradient-to-br from-green-50 to-green-100 rounded-2xl shadow-lg p-4 hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-green-600 font-medium">Paid</p>
                  <p className="text-3xl font-bold text-green-800">{summary.paidCount}</p>
                </div>
                <DollarSign className="w-10 h-10 text-green-500" />
              </div>
            </div>

            <div className="bg-gradient-to-br from-orange-50 to-orange-100 rounded-2xl shadow-lg p-4 hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-orange-600 font-medium">On Hold</p>
                  <p className="text-3xl font-bold text-orange-800">{summary.onHoldCount}</p>
                </div>
                <PauseCircle className="w-10 h-10 text-orange-500" />
              </div>
            </div>

            <div className="bg-gradient-to-br from-red-50 to-red-100 rounded-2xl shadow-lg p-4 hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-red-600 font-medium">Cancelled</p>
                  <p className="text-3xl font-bold text-red-800">{summary.cancelledCount}</p>
                </div>
                <XCircle className="w-10 h-10 text-red-500" />
              </div>
            </div>

            <div className="bg-gradient-to-br from-purple-50 to-purple-100 rounded-2xl shadow-lg p-4 hover:scale-105 transition-all duration-300">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-purple-600 font-medium">Total Net</p>
                  <p className="text-2xl font-bold text-purple-800">{formatCurrency(summary.totalNetSalary)}</p>
                </div>
                <DollarSign className="w-10 h-10 text-purple-500" />
              </div>
            </div>
          </div>
        )}

        {/* Filters and Actions */}
        <div className="bg-white rounded-2xl shadow-lg p-6">
          <div className="flex items-center gap-2 mb-4">
            <Filter className="w-5 h-5 text-blue-600" />
            <h2 className="text-lg font-bold text-gray-800">Filters & Actions</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Status</label>
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                <option value="">All Status</option>
                <option value="Pending">Pending</option>
                <option value="Approved">Approved</option>
                <option value="Paid">Paid</option>
                <option value="OnHold">On Hold</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Period Start</label>
              <input
                type="date"
                value={periodStartDate}
                onChange={(e) => setPeriodStartDate(e.target.value)}
                className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Period End</label>
              <input
                type="date"
                value={periodEndDate}
                onChange={(e) => setPeriodEndDate(e.target.value)}
                className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>

            <div className="flex items-end">
              <button
                onClick={() => {
                  setStatusFilter('');
                  setPeriodStartDate('');
                  setPeriodEndDate('');
                }}
                className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-xl hover:bg-gray-200 transition-all duration-300"
              >
                Clear Filters
              </button>
            </div>
          </div>

          {/* Bulk Actions */}
          {selectedPayments.size > 0 && (
            <div className="mt-4 flex items-center gap-3 p-4 bg-blue-50 rounded-xl border-2 border-blue-200">
              <AlertCircle className="w-5 h-5 text-blue-600" />
              <span className="text-sm font-medium text-blue-800">
                {selectedPayments.size} payment(s) selected
              </span>
              <div className="flex gap-2 ml-auto">
                <button
                  onClick={() => {
                    setBulkAction(true);
                    setShowStatusDialog(true);
                  }}
                  className="px-4 py-2 bg-blue-600 text-white rounded-xl hover:bg-blue-700 transition-all duration-300 hover:scale-105"
                >
                  Update Status
                </button>
                <button
                  onClick={() => {
                    setBulkAction(true);
                    setShowPayDialog(true);
                  }}
                  className="px-4 py-2 bg-green-600 text-white rounded-xl hover:bg-green-700 transition-all duration-300 hover:scale-105"
                >
                  Mark as Paid
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Payments Table */}
        <div className="bg-white rounded-2xl shadow-lg overflow-hidden">
          <div className="overflow-x-auto">
            {isLoading ? (
              <div className="flex items-center justify-center p-12">
                <RefreshCw className="w-8 h-8 text-blue-600 animate-spin" />
                <span className="ml-3 text-gray-600">Loading payments...</span>
              </div>
            ) : payments.length === 0 ? (
              <div className="flex flex-col items-center justify-center p-12">
                <DollarSign className="w-16 h-16 text-gray-300 mb-4" />
                <p className="text-gray-500 text-lg">No salary payments found</p>
              </div>
            ) : (
              <table className="w-full">
                <thead className="bg-gradient-to-r from-blue-50 to-indigo-50">
                  <tr>
                    <th className="px-4 py-4 text-left">
                      <input
                        type="checkbox"
                        checked={selectedPayments.size === payments.length && payments.length > 0}
                        onChange={toggleSelectAll}
                        className="w-4 h-4 text-blue-600 rounded"
                      />
                    </th>
                    <th className="px-4 py-4 text-left text-sm font-bold text-gray-700">Teacher</th>
                    <th className="px-4 py-4 text-left text-sm font-bold text-gray-700">Period</th>
                    <th className="px-4 py-4 text-right text-sm font-bold text-gray-700">Base Salary</th>
                    <th className="px-4 py-4 text-right text-sm font-bold text-gray-700">Deductions</th>
                    <th className="px-4 py-4 text-right text-sm font-bold text-gray-700">Bonus</th>
                    <th className="px-4 py-4 text-right text-sm font-bold text-gray-700">Net Salary</th>
                    <th className="px-4 py-4 text-center text-sm font-bold text-gray-700">Status</th>
                    <th className="px-4 py-4 text-center text-sm font-bold text-gray-700">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {payments.map((payment) => (
                    <tr key={payment.id} className="hover:bg-blue-50 transition-colors duration-200">
                      <td className="px-4 py-3">
                        <input
                          type="checkbox"
                          checked={selectedPayments.has(payment.id)}
                          onChange={() => toggleSelection(payment.id)}
                          className="w-4 h-4 text-blue-600 rounded"
                        />
                      </td>
                      <td className="px-4 py-3">
                        <div>
                          <p className="font-medium text-gray-900">{payment.teacherName}</p>
                          <p className="text-sm text-gray-500">ID: {payment.teacherId.substring(0, 8)}</p>
                        </div>
                      </td>
                      <td className="px-4 py-3 text-sm text-gray-600">
                        {new Date(payment.periodStartDate).toLocaleDateString()} - {new Date(payment.periodEndDate).toLocaleDateString()}
                      </td>
                      <td className="px-4 py-3 text-right font-medium text-blue-600">
                        {formatCurrency(payment.baseSalary)}
                      </td>
                      <td className="px-4 py-3 text-right font-medium text-red-600">
                        -{formatCurrency(payment.deductions)}
                      </td>
                      <td className="px-4 py-3 text-right font-medium text-green-600">
                        +{formatCurrency(payment.bonus)}
                      </td>
                      <td className="px-4 py-3 text-right font-bold text-purple-600">
                        {formatCurrency(payment.netSalary)}
                      </td>
                      <td className="px-4 py-3 text-center">
                        {getStatusBadge(payment.status)}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex items-center justify-center gap-2">
                          {payment.status === 'Approved' && (
                            <button
                              onClick={() => {
                                setCurrentPayment(payment);
                                setBulkAction(false);
                                setShowPayDialog(true);
                              }}
                              className="p-2 text-green-600 hover:bg-green-100 rounded-lg transition-colors"
                              title="Mark as Paid"
                            >
                              <DollarSign className="w-4 h-4" />
                            </button>
                          )}
                          {(payment.status === 'Pending' || payment.status === 'Approved') && (
                            <button
                              onClick={() => openEditDialog(payment)}
                              className="p-2 text-blue-600 hover:bg-blue-100 rounded-lg transition-colors"
                              title="Edit"
                            >
                              <Edit className="w-4 h-4" />
                            </button>
                          )}
                          <button
                            onClick={() => {
                              setCurrentPayment(payment);
                              setBulkAction(false);
                              setShowStatusDialog(true);
                            }}
                            className="p-2 text-orange-600 hover:bg-orange-100 rounded-lg transition-colors"
                            title="Update Status"
                          >
                            <RefreshCw className="w-4 h-4" />
                          </button>
                          {payment.status !== 'Paid' && (
                            <button
                              onClick={() => handleDeletePayment(payment.id)}
                              className="p-2 text-red-600 hover:bg-red-100 rounded-lg transition-colors"
                              title="Delete"
                            >
                              <Trash2 className="w-4 h-4" />
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>

        {/* Status Update Dialog */}
        {showStatusDialog && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 backdrop-blur-sm">
            <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md">
              <div className="flex items-center justify-between mb-6">
                <h3 className="text-xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent">
                  Update Status
                </h3>
                <button
                  onClick={() => {
                    setShowStatusDialog(false);
                    setBulkAction(false);
                    setNewStatus('');
                    setStatusRemarks('');
                  }}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <XCircle className="w-6 h-6" />
                </button>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">New Status</label>
                  <select
                    value={newStatus}
                    onChange={(e) => setNewStatus(e.target.value)}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="">Select Status</option>
                    <option value="Pending">Pending</option>
                    <option value="Approved">Approved</option>
                    <option value="OnHold">On Hold</option>
                    <option value="Cancelled">Cancelled</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Remarks (Optional)</label>
                  <textarea
                    value={statusRemarks}
                    onChange={(e) => setStatusRemarks(e.target.value)}
                    rows={3}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    placeholder="Add remarks..."
                  />
                </div>

                <div className="flex gap-3 pt-4">
                  <button
                    onClick={() => {
                      setShowStatusDialog(false);
                      setBulkAction(false);
                      setNewStatus('');
                      setStatusRemarks('');
                    }}
                    className="flex-1 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-xl hover:bg-gray-100 transition-all duration-300"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleStatusUpdate}
                    disabled={!newStatus || updateStatusMutation.isPending}
                    className="flex-1 px-4 py-2 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:scale-105 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {updateStatusMutation.isPending ? 'Updating...' : 'Update Status'}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Mark as Paid Dialog */}
        {showPayDialog && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 backdrop-blur-sm">
            <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md">
              <div className="flex items-center justify-between mb-6">
                <h3 className="text-xl font-bold bg-gradient-to-r from-green-600 to-green-800 bg-clip-text text-transparent">
                  Mark as Paid
                </h3>
                <button
                  onClick={() => {
                    setShowPayDialog(false);
                    setBulkAction(false);
                    setPaidDate('');
                    setReferenceNumber('');
                    setPaymentMethod('');
                  }}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <XCircle className="w-6 h-6" />
                </button>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Paid Date *</label>
                  <input
                    type="date"
                    value={paidDate}
                    onChange={(e) => setPaidDate(e.target.value)}
                    max={new Date().toISOString().split('T')[0]}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-green-500 focus:border-green-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Reference Number *</label>
                  <input
                    type="text"
                    value={referenceNumber}
                    onChange={(e) => setReferenceNumber(e.target.value)}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-green-500 focus:border-green-500"
                    placeholder="Transaction/Check number"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Payment Method</label>
                  <select
                    value={paymentMethod}
                    onChange={(e) => setPaymentMethod(e.target.value)}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-green-500 focus:border-green-500"
                  >
                    <option value="">Select Method</option>
                    <option value="Cash">Cash</option>
                    <option value="BankTransfer">Bank Transfer</option>
                    <option value="Cheque">Cheque</option>
                    <option value="MobilePayment">Mobile Payment</option>
                    <option value="Other">Other</option>
                  </select>
                </div>

                <div className="flex gap-3 pt-4">
                  <button
                    onClick={() => {
                      setShowPayDialog(false);
                      setBulkAction(false);
                      setPaidDate('');
                      setReferenceNumber('');
                      setPaymentMethod('');
                    }}
                    className="flex-1 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-xl hover:bg-gray-100 transition-all duration-300"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleMarkAsPaid}
                    disabled={!paidDate || !referenceNumber || markPaidMutation.isPending}
                    className="flex-1 px-4 py-2 bg-gradient-to-r from-green-600 to-green-700 text-white rounded-xl hover:scale-105 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {markPaidMutation.isPending ? 'Processing...' : 'Mark as Paid'}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Edit Payment Dialog */}
        {showEditDialog && currentPayment && (
          <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 backdrop-blur-sm">
            <div className="bg-white rounded-2xl shadow-2xl p-6 w-full max-w-md">
              <div className="flex items-center justify-between mb-6">
                <h3 className="text-xl font-bold bg-gradient-to-r from-purple-600 to-purple-800 bg-clip-text text-transparent">
                  Edit Payment - {currentPayment.teacherName}
                </h3>
                <button
                  onClick={() => {
                    setShowEditDialog(false);
                    setCurrentPayment(null);
                  }}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <XCircle className="w-6 h-6" />
                </button>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Base Salary</label>
                  <input
                    type="number"
                    value={editBaseSalary}
                    onChange={(e) => setEditBaseSalary(Number(e.target.value))}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Deductions</label>
                  <input
                    type="number"
                    value={editDeductions}
                    onChange={(e) => setEditDeductions(Number(e.target.value))}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Bonus</label>
                  <input
                    type="number"
                    value={editBonus}
                    onChange={(e) => setEditBonus(Number(e.target.value))}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                  />
                </div>

                <div className="p-4 bg-gradient-to-r from-purple-50 to-indigo-50 rounded-xl">
                  <div className="flex items-center justify-between">
                    <span className="text-sm font-medium text-purple-700">Net Salary:</span>
                    <span className="text-xl font-bold text-purple-900">{formatCurrency(calculatedNetSalary)}</span>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">Remarks</label>
                  <textarea
                    value={editRemarks}
                    onChange={(e) => setEditRemarks(e.target.value)}
                    rows={2}
                    className="w-full px-4 py-2 border-2 border-gray-300 rounded-xl focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                    placeholder="Optional remarks..."
                  />
                </div>

                <div className="flex gap-3 pt-4">
                  <button
                    onClick={() => {
                      setShowEditDialog(false);
                      setCurrentPayment(null);
                    }}
                    className="flex-1 px-4 py-2 border-2 border-gray-300 text-gray-700 rounded-xl hover:bg-gray-100 transition-all duration-300"
                  >
                    Cancel
                  </button>
                  <button
                    onClick={handleEditPayment}
                    disabled={updatePaymentMutation.isPending}
                    className="flex-1 px-4 py-2 bg-gradient-to-r from-purple-600 to-purple-700 text-white rounded-xl hover:scale-105 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {updatePaymentMutation.isPending ? 'Saving...' : 'Save Changes'}
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

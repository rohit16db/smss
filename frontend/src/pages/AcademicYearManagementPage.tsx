import { useState } from 'react';
import { useAcademicYear } from '../hooks/useAcademicYear';
import toast from 'react-hot-toast';
import { format } from 'date-fns';

export function AcademicYearManagementPage() {
  const { academicYears, isLoading, createYear, toggleStatus } = useAcademicYear();
  const [isModalOpen, setIsModalOpen] = useState(false);
  
  // Form State
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [isActive, setIsActive] = useState(true);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name || !startDate || !endDate) {
      toast.error('Please fill all required fields');
      return;
    }

    try {
      await createYear.mutateAsync({
        name,
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        isActive
      });
      toast.success('Academic Year created successfully');
      setIsModalOpen(false);
      setName('');
      setStartDate('');
      setEndDate('');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to create academic year');
    }
  };

  const handleToggle = async (id: string) => {
    try {
      await toggleStatus.mutateAsync(id);
      toast.success('Status updated');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to update status');
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 pb-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <header className="mb-8 flex justify-between items-end">
          <div>
            <h1 className="text-4xl font-black bg-gradient-to-r from-blue-600 to-indigo-700 bg-clip-text text-transparent flex items-center gap-3">
              <span>📅</span> Academic Sessions
            </h1>
            <p className="text-gray-500 mt-2 font-medium">Configure school years and active operational periods</p>
          </div>
          <button
            onClick={() => setIsModalOpen(true)}
            className="px-6 py-3 bg-blue-600 text-white font-bold rounded-xl shadow-lg hover:shadow-blue-200/50 hover:bg-blue-700 transition-all flex items-center gap-2 group"
          >
            <span className="text-xl group-hover:rotate-90 transition-transform">+</span>
            New Session
          </button>
        </header>

        {isLoading ? (
          <div className="flex items-center justify-center p-24">
            <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent"></div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {academicYears?.map((year) => (
              <div 
                key={year.id} 
                className={`bg-white rounded-2xl shadow-xl overflow-hidden border-2 transition-all ${year.isActive ? 'border-blue-500 ring-2 ring-blue-100' : 'border-transparent hover:border-gray-200'}`}
              >
                <div className="p-6">
                  <div className="flex justify-between items-start mb-4">
                    <div className={`p-3 rounded-xl ${year.isActive ? 'bg-blue-100 text-blue-600' : 'bg-gray-100 text-gray-400'}`}>
                      <span className="text-2xl">⏳</span>
                    </div>
                    <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${year.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                      {year.isActive ? 'Active Session' : 'Locked'}
                    </span>
                  </div>
                  
                  <h3 className="text-2xl font-black text-gray-900 mb-1">{year.name}</h3>
                  <p className="text-gray-400 text-sm font-medium mb-6">Operational lifecycle</p>

                  <div className="space-y-3 mb-8">
                    <div className="flex items-center gap-3 text-sm font-bold text-gray-600">
                      <span className="w-8 h-8 rounded-lg bg-gray-50 flex items-center justify-center text-xs">🚀</span>
                      <div>
                        <p className="text-[10px] uppercase text-gray-400">Starts</p>
                        {format(new Date(year.startDate), 'MMMM dd, yyyy')}
                      </div>
                    </div>
                    <div className="flex items-center gap-3 text-sm font-bold text-gray-600">
                      <span className="w-8 h-8 rounded-lg bg-gray-50 flex items-center justify-center text-xs">🛑</span>
                      <div>
                        <p className="text-[10px] uppercase text-gray-400">Ends</p>
                        {format(new Date(year.endDate), 'MMMM dd, yyyy')}
                      </div>
                    </div>
                  </div>

                  <button
                    onClick={() => handleToggle(year.id)}
                    disabled={toggleStatus.isPending}
                    className={`w-full py-3 rounded-xl font-bold text-sm transition-all flex items-center justify-center gap-2 ${year.isActive ? 'bg-red-50 text-red-600 hover:bg-red-100' : 'bg-blue-600 text-white hover:bg-blue-700'}`}
                  >
                    {year.isActive ? 'Archive Session' : 'Activate Session'}
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Create Modal */}
        {isModalOpen && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/60 backdrop-blur-sm animate-fadeIn">
            <div className="bg-white rounded-3xl shadow-2xl w-full max-w-md p-8 relative overflow-hidden">
              <div className="absolute top-0 left-0 w-full h-2 bg-gradient-to-r from-blue-500 to-indigo-600"></div>
              
              <div className="flex justify-between items-center mb-8">
                <h2 className="text-2xl font-black text-gray-900">New Academic Year</h2>
                <button onClick={() => setIsModalOpen(false)} className="text-gray-400 hover:text-gray-600 p-2 uppercase text-xs font-black">Close</button>
              </div>

              <form onSubmit={handleSubmit} className="space-y-6">
                <div>
                  <label className="block text-xs font-black text-gray-400 uppercase tracking-widest mb-2">Display Name</label>
                  <input
                    type="text"
                    placeholder="e.g., 2024-2025"
                    className="w-full px-4 py-3 bg-gray-50 border-2 border-transparent focus:border-blue-500 rounded-xl outline-none transition-all font-bold"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-black text-gray-400 uppercase tracking-widest mb-2">Start Date</label>
                    <input
                      type="date"
                      className="w-full px-4 py-3 bg-gray-50 border-2 border-transparent focus:border-blue-500 rounded-xl outline-none transition-all font-bold"
                      value={startDate}
                      onChange={(e) => setStartDate(e.target.value)}
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-black text-gray-400 uppercase tracking-widest mb-2">End Date</label>
                    <input
                      type="date"
                      className="w-full px-4 py-3 bg-gray-50 border-2 border-transparent focus:border-blue-500 rounded-xl outline-none transition-all font-bold"
                      value={endDate}
                      onChange={(e) => setEndDate(e.target.value)}
                    />
                  </div>
                </div>

                <label className="flex items-center gap-3 p-4 bg-blue-50 rounded-2xl cursor-pointer group">
                  <input
                    type="checkbox"
                    checked={isActive}
                    onChange={(e) => setIsActive(e.target.checked)}
                    className="w-6 h-6 rounded-lg text-blue-600"
                  />
                  <div>
                    <p className="text-sm font-black text-blue-900">Set as Primary Active</p>
                    <p className="text-[10px] text-blue-600 font-bold uppercase">Archive all other years automatically</p>
                  </div>
                </label>

                <button
                  type="submit"
                  disabled={createYear.isPending}
                  className="w-full py-4 bg-blue-600 text-white font-black rounded-2xl shadow-xl hover:bg-blue-700 transition-all uppercase tracking-widest text-sm"
                >
                  {createYear.isPending ? 'Processing...' : 'Deploy Session'}
                </button>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

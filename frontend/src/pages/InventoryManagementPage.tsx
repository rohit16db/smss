import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { inventoryApi, type InventoryItem, type InventoryCategory, type InventoryTransaction, type CreateInventoryItemDto, type CreateInventoryCategoryDto, type StockTransactionDto } from '../services/api';
import { useAcademicYear } from '../hooks/useAcademicYear';

export function InventoryManagementPage() {
  const queryClient = useQueryClient();
  const { activeYear } = useAcademicYear();
  
  // State
  const [activeTab, setActiveTab] = useState<'dashboard' | 'items' | 'history' | 'categories'>('dashboard');
  
  // Tab Filters & Pagination State
  const [itemsSearchQuery, setItemsSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('');
  const [itemsPage, setItemsPage] = useState(1);
  
  const [transactionsSearchQuery, setTransactionsSearchQuery] = useState('');
  const [transactionsPage, setTransactionsPage] = useState(1);
  
  // Dialog States
  const [itemDialogOpen, setItemDialogOpen] = useState(false);
  const [categoryDialogOpen, setCategoryDialogOpen] = useState(false);
  const [transactionDialogOpen, setTransactionDialogOpen] = useState(false);
  const [selectedItem, setSelectedItem] = useState<InventoryItem | null>(null);
  const [selectedCategoryData, setSelectedCategoryData] = useState<InventoryCategory | null>(null);
  const [transactionType, setTransactionType] = useState<'StockIn' | 'StockOut'>('StockIn');

  // Queries
  const { data: summary, isLoading: summaryLoading } = useQuery({
    queryKey: ['inventory', 'summary'],
    queryFn: inventoryApi.getSummary
  });

  const { data: paginatedItems, isLoading: itemsLoading } = useQuery({
    queryKey: ['inventory', 'items', selectedCategory, itemsPage, itemsSearchQuery],
    queryFn: () => inventoryApi.getItems(itemsPage, 10, selectedCategory || undefined, itemsSearchQuery || undefined)
  });
  const items = paginatedItems?.items || [];

  const { data: dashboardLowStockPaginated } = useQuery({
    queryKey: ['inventory', 'items', 'low-stock'],
    queryFn: () => inventoryApi.getItems(1, 50, undefined, undefined, true)
  });
  const lowStockItems = dashboardLowStockPaginated?.items || [];

  const { data: categories, isLoading: categoriesLoading } = useQuery({
    queryKey: ['inventory', 'categories'],
    queryFn: inventoryApi.getCategories
  });

  const { data: paginatedTransactions, isLoading: transactionsLoading } = useQuery({
    queryKey: ['inventory', 'transactions', transactionsPage, transactionsSearchQuery],
    queryFn: () => inventoryApi.getTransactions(transactionsPage, 10, undefined, transactionsSearchQuery || undefined)
  });
  const transactions = paginatedTransactions?.items || [];

  // Mutations
  const addItemMutation = useMutation({
    mutationFn: inventoryApi.addItem,
    onSuccess: () => {
      toast.success('Inventory item added!');
      queryClient.invalidateQueries({ queryKey: ['inventory'] });
      setItemDialogOpen(false);
    }
  });

  const addCategoryMutation = useMutation({
    mutationFn: inventoryApi.createCategory,
    onSuccess: () => {
      toast.success('Category created!');
      queryClient.invalidateQueries({ queryKey: ['inventory', 'categories'] });
      setCategoryDialogOpen(false);
    }
  });

  const updateCategoryMutation = useMutation({
    mutationFn: ({ id, data }: { id: string, data: any }) => inventoryApi.updateCategory(id, data),
    onSuccess: () => {
      toast.success('Category updated!');
      queryClient.invalidateQueries({ queryKey: ['inventory', 'categories'] });
      setCategoryDialogOpen(false);
      setSelectedCategoryData(null);
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.message || 'Failed to update category');
    }
  });

  const transactionMutation = useMutation({
    mutationFn: inventoryApi.processTransaction,
    onSuccess: () => {
      toast.success(`${transactionType === 'StockIn' ? 'Stock Added' : 'Item Issued'} successfully!`);
      queryClient.invalidateQueries({ queryKey: ['inventory'] });
      setTransactionDialogOpen(false);
      setSelectedItem(null);
    },
    onError: (error: any) => {
        toast.error(error.response?.data?.message || 'Transaction failed');
    }
  });

  // Handle Search Changes
  const handleItemSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setItemsSearchQuery(e.target.value);
    setItemsPage(1);
  };
  
  const handleCategoryFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setSelectedCategory(e.target.value);
    setItemsPage(1);
  };

  const handleTransactionSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setTransactionsSearchQuery(e.target.value);
    setTransactionsPage(1);
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-indigo-600 to-purple-600 bg-clip-text text-transparent flex items-center gap-3">
                <span>📦</span> Inventory Management
              </h1>
              <p className="text-gray-600 mt-2">Track stationary, uniforms, and sports kits with precision</p>
            </div>
            <div className="flex flex-wrap gap-3">
              <button 
                onClick={() => { setSelectedCategoryData(null); setCategoryDialogOpen(true); }}
                className="flex items-center gap-2 px-4 py-2 border border-gray-300 text-gray-700 bg-white rounded-xl hover:bg-gray-50 transition-all font-medium whitespace-nowrap"
              >
                + New Category
              </button>
              <button 
                onClick={() => setItemDialogOpen(true)}
                className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-indigo-600 to-purple-600 text-white rounded-xl hover:shadow-lg hover:scale-105 transition-all duration-300 font-medium whitespace-nowrap"
              >
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
                Add Item
              </button>
            </div>
          </div>

          {/* Navigation Tabs */}
          <div className="mb-6">
            <div className="border-b border-gray-200">
              <nav className="-mb-px flex space-x-8">
                {[
                  { id: 'dashboard', label: 'Dashboard', icon: '📊' },
                  { id: 'items', label: 'Stock Items', icon: '📋' },
                  { id: 'history', label: 'Movement Logs', icon: '🔄' },
                  { id: 'categories', label: 'Categories', icon: '📁' }
                ].map(tab => (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id as any)}
                    className={`whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors flex items-center gap-2 ${
                      activeTab === tab.id 
                        ? 'border-indigo-500 text-indigo-600' 
                        : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    }`}
                  >
                    <span>{tab.icon}</span> {tab.label}
                  </button>
                ))}
              </nav>
            </div>
          </div>

          {/* Tab Content */}
          {activeTab === 'dashboard' && (
            <div className="space-y-8">
              {/* Stats Grid */}
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <StatCard 
                  title="Total Items" 
                  value={summary?.totalItems || 0} 
                  icon="📦" 
                  color="blue"
                  isLoading={summaryLoading}
                />
                <StatCard 
                  title="Categories" 
                  value={summary?.totalCategories || 0} 
                  icon="📁" 
                  color="purple"
                  isLoading={summaryLoading}
                />
                <StatCard 
                  title="Low Stock Warning" 
                  value={summary?.lowStockItemsCount || 0} 
                  icon="⚠️" 
                  color="amber"
                  isLoading={summaryLoading}
                  isAlert={ (summary?.lowStockItemsCount || 0) > 0 }
                />
                <StatCard 
                  title="Inventory Value" 
                  value={summary?.totalInventoryValue || 0} 
                  icon="💰" 
                  color="emerald" 
                  isCurrency 
                  isLoading={summaryLoading}
                />
              </div>

              {/* Low Stock Watchlist */}
              <div className="bg-white rounded-2xl shadow-lg border border-gray-100 overflow-hidden">
                <div className="px-8 py-6 border-b border-gray-100 flex items-center justify-between bg-amber-50/50">
                    <h2 className="text-xl font-bold text-amber-900 flex items-center gap-2">
                      <span className="text-amber-500 text-2xl">⚠️</span> Low Stock Watchlist
                    </h2>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full">
                      <thead className="bg-gray-50 text-gray-700 text-xs font-bold uppercase tracking-widest border-b border-gray-100">
                        <tr>
                          <th className="px-8 py-4 text-left">Item Name</th>
                          <th className="px-8 py-4 text-left">SKU</th>
                          <th className="px-8 py-4 text-center">Remaining</th>
                          <th className="px-8 py-4 text-center">Reorder At</th>
                          <th className="px-8 py-4 text-center">Action</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-100">
                        {lowStockItems.map(item => (
                          <tr key={item.id} className="hover:bg-amber-50/20 transition-colors">
                            <td className="px-8 py-5 font-bold text-gray-900">{item.name}</td>
                            <td className="px-8 py-5 text-gray-500 font-medium">{item.sku}</td>
                            <td className="px-8 py-5 text-center">
                              <span className="px-3 py-1 bg-red-100 text-red-700 rounded-full font-black text-sm">
                                {item.totalQuantity}
                              </span>
                            </td>
                            <td className="px-8 py-5 text-center font-bold text-gray-400">{item.reorderLevel}</td>
                            <td className="px-8 py-5 text-center">
                              <button 
                                onClick={() => { setSelectedItem(item); setTransactionType('StockIn'); setTransactionDialogOpen(true); }}
                                className="text-indigo-600 font-bold text-sm hover:underline"
                              >
                                Add Stock
                              </button>
                            </td>
                          </tr>
                        ))}
                        {(!lowStockItems || lowStockItems.length === 0) && (
                          <tr>
                            <td colSpan={5} className="px-8 py-12 text-center text-gray-400 font-medium italic">
                              All stock levels are currently healthy! ✨
                            </td>
                          </tr>
                        )}
                      </tbody>
                  </table>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'items' && (
            <div className="space-y-6">
              <div className="flex flex-col md:flex-row gap-4 justify-between items-center bg-white p-4 rounded-2xl shadow-lg border border-gray-100">
                <div className="relative flex-1 w-full">
                    <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                    <input 
                      type="text" 
                      placeholder="Search items by name or SKU..." 
                      className="w-full pl-12 pr-4 py-3 bg-gray-50 border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 font-medium border transition-colors outline-none"
                      value={itemsSearchQuery}
                      onChange={handleItemSearch}
                    />
                </div>
                <select 
                  className="px-6 py-3 bg-gray-50 border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 font-bold text-gray-600 border outline-none"
                  value={selectedCategory}
                  onChange={handleCategoryFilter}
                >
                  <option value="">All Categories</option>
                  {categories?.map(c => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
              </div>

              <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden flex flex-col">
                <div className="overflow-x-auto">
                    <table className="w-full">
                        <thead className="bg-indigo-600 text-white text-xs font-bold uppercase tracking-widest">
                          <tr>
                            <th className="px-8 py-5 text-left font-semibold">Item Details</th>
                            <th className="px-8 py-5 text-left font-semibold">Category</th>
                            <th className="px-8 py-5 text-center font-semibold">Stock Level</th>
                            <th className="px-8 py-5 text-right font-semibold">Unit Price</th>
                            <th className="px-8 py-5 text-center font-semibold">Quick Actions</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                          {items.map(item => (
                            <tr key={item.id} className="group hover:bg-indigo-50/30 transition-all">
                              <td className="px-8 py-5">
                                <div className="font-bold text-gray-900 text-lg leading-none mb-1">{item.name}</div>
                                <div className="text-xs font-bold text-indigo-400 bg-indigo-50 w-fit px-2 py-0.5 rounded uppercase tracking-wider border border-indigo-100">{item.sku}</div>
                              </td>
                              <td className="px-8 py-5 font-bold text-gray-500">{item.categoryName}</td>
                              <td className="px-8 py-5 text-center">
                                <div className="flex flex-col items-center">
                                  <span className={`px-4 py-1 rounded-full text-sm font-black mb-1 ${
                                    item.totalQuantity <= item.reorderLevel ? 'bg-red-100 text-red-600' : 'bg-emerald-100 text-emerald-600'
                                  }`}>
                                    {item.totalQuantity} in stock
                                  </span>
                                  <div className="text-[10px] font-bold text-gray-400 uppercase">Min Level: {item.reorderLevel}</div>
                                </div>
                              </td>
                              <td className="px-8 py-5 text-right font-black text-gray-900">
                                {new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR' }).format(item.unitPrice)}
                              </td>
                              <td className="px-8 py-5">
                                <div className="flex justify-center gap-2">
                                    <button 
                                      onClick={() => { setSelectedItem(item); setTransactionType('StockIn'); setTransactionDialogOpen(true); }}
                                      className="p-2 bg-indigo-100 text-indigo-600 rounded-xl hover:bg-indigo-600 hover:text-white transition-all transform hover:scale-110"
                                      title="Add Stock"
                                    >
                                      📥
                                    </button>
                                    <button 
                                      onClick={() => { setSelectedItem(item); setTransactionType('StockOut'); setTransactionDialogOpen(true); }}
                                      className="p-2 bg-pink-100 text-pink-600 rounded-xl hover:bg-pink-600 hover:text-white transition-all transform hover:scale-110"
                                      title="Issue Item"
                                    >
                                      📤
                                    </button>
                                </div>
                              </td>
                            </tr>
                          ))}
                          {items.length === 0 && !itemsLoading && (
                            <tr>
                              <td colSpan={5} className="px-8 py-12 text-center text-gray-500 font-medium bg-gray-50/50">
                                {itemsSearchQuery ? `No items matched "${itemsSearchQuery}"` : "No items found in this category"}
                              </td>
                            </tr>
                          )}
                        </tbody>
                    </table>
                </div>

                {paginatedItems && paginatedItems.totalCount > 0 && (
                   <div className="border-t border-gray-100 px-8 py-4 bg-gray-50/50 flex items-center justify-between">
                     <span className="text-sm font-medium text-gray-500">
                       Showing page <span className="font-bold text-gray-900">{paginatedItems.pageNumber}</span> of <span className="font-bold text-gray-900">{paginatedItems.totalPages}</span> ({paginatedItems.totalCount} total items)
                     </span>
                     <div className="flex gap-2">
                       <button
                         onClick={() => setItemsPage(prev => Math.max(1, prev - 1))}
                         disabled={itemsPage === 1}
                         className="px-4 py-2 border border-gray-300 rounded-xl font-bold text-sm bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                       >
                         Previous
                       </button>
                       <button
                         onClick={() => setItemsPage(prev => Math.min(paginatedItems.totalPages, prev + 1))}
                         disabled={itemsPage >= paginatedItems.totalPages}
                         className="px-4 py-2 border border-gray-300 rounded-xl font-bold text-sm bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                       >
                         Next
                       </button>
                     </div>
                   </div>
                )}
              </div>
            </div>
          )}

          {activeTab === 'history' && (
            <div className="space-y-6">
              <div className="flex flex-col md:flex-row gap-4 justify-between items-center bg-white p-4 rounded-2xl shadow-lg border border-gray-100">
                <div className="relative flex-1 w-full">
                    <span className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-400">🔍</span>
                    <input 
                      type="text" 
                      placeholder="Search transactions by Item Name or SKU..." 
                      className="w-full pl-12 pr-4 py-3 bg-gray-50 border-gray-200 rounded-xl focus:ring-2 focus:ring-indigo-500 font-medium border transition-colors outline-none"
                      value={transactionsSearchQuery}
                      onChange={handleTransactionSearch}
                    />
                </div>
              </div>

              <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden flex flex-col">
                <div className="px-8 py-6 border-b border-gray-100 bg-gray-50/50">
                    <h2 className="text-xl font-bold text-gray-900">Transaction History</h2>
                    <p className="text-sm text-gray-500">Chronological list of all stock movements</p>
                </div>
                <div className="overflow-x-auto">
                    <table className="w-full">
                        <thead className="bg-gray-50 text-gray-700 text-xs font-bold uppercase tracking-widest border-b border-gray-100">
                          <tr>
                            <th className="px-8 py-4 text-left">Date & Time</th>
                            <th className="px-8 py-4 text-left">Item Name</th>
                            <th className="px-8 py-4 text-center">Action</th>
                            <th className="px-8 py-4 text-center">Qty</th>
                            <th className="px-8 py-4 text-left">Recipient/Remarks</th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-100">
                          {transactions.map(log => (
                            <tr key={log.id} className="hover:bg-gray-50/50 transition-colors">
                              <td className="px-8 py-5">
                                  <div className="font-bold text-gray-900 text-sm">
                                    {new Date(log.transactionDate).toLocaleDateString()}
                                  </div>
                                  <div className="text-[10px] text-gray-400 uppercase font-bold">
                                    {new Date(log.transactionDate).toLocaleTimeString()}
                                  </div>
                              </td>
                              <td className="px-8 py-5 font-black text-indigo-700">{log.itemName}</td>
                              <td className="px-8 py-5 text-center">
                                  <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-tighter ${
                                    log.transactionType === 'StockIn' ? 'bg-emerald-100 text-emerald-700' : 'bg-pink-100 text-pink-700'
                                  }`}>
                                    {log.transactionType === 'StockIn' ? 'IN' : 'OUT'}
                                  </span>
                              </td>
                              <td className="px-8 py-5 text-center font-black text-gray-900">{log.quantity}</td>
                              <td className="px-8 py-5">
                                  <div className="font-bold text-gray-700 text-sm">{log.receivedBy || '-'}</div>
                                  <div className="text-xs text-gray-400 italic font-medium">{log.remarks}</div>
                              </td>
                            </tr>
                          ))}
                          {transactions.length === 0 && !transactionsLoading && (
                            <tr>
                              <td colSpan={5} className="px-8 py-12 text-center text-gray-500 font-medium bg-gray-50/50">
                                {transactionsSearchQuery ? `No transactions found for "${transactionsSearchQuery}"` : "No movement logs available"}
                              </td>
                            </tr>
                          )}
                        </tbody>
                    </table>
                </div>

                {paginatedTransactions && paginatedTransactions.totalCount > 0 && (
                   <div className="border-t border-gray-100 px-8 py-4 bg-gray-50/50 flex items-center justify-between">
                     <span className="text-sm font-medium text-gray-500">
                       Showing page <span className="font-bold text-gray-900">{paginatedTransactions.pageNumber}</span> of <span className="font-bold text-gray-900">{paginatedTransactions.totalPages}</span> ({paginatedTransactions.totalCount} total logs)
                     </span>
                     <div className="flex gap-2">
                       <button
                         onClick={() => setTransactionsPage(prev => Math.max(1, prev - 1))}
                         disabled={transactionsPage === 1}
                         className="px-4 py-2 border border-gray-300 rounded-xl font-bold text-sm bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                       >
                         Previous
                       </button>
                       <button
                         onClick={() => setTransactionsPage(prev => Math.min(paginatedTransactions.totalPages, prev + 1))}
                         disabled={transactionsPage >= paginatedTransactions.totalPages}
                         className="px-4 py-2 border border-gray-300 rounded-xl font-bold text-sm bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                       >
                         Next
                       </button>
                     </div>
                   </div>
                )}
              </div>
            </div>
          )}

          {activeTab === 'categories' && (
            <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden">
               <div className="px-8 py-6 border-b border-gray-100 bg-gray-50/50">
                  <h2 className="text-xl font-bold text-gray-900">Category Management</h2>
                  <p className="text-sm text-gray-500">List of all inventory categories and their descriptions</p>
               </div>
               <table className="w-full">
                  <thead className="bg-gray-50 text-gray-700 text-xs font-bold uppercase tracking-widest border-b border-gray-100">
                    <tr>
                      <th className="px-8 py-4 text-left">Category Name</th>
                      <th className="px-8 py-4 text-left">Description</th>
                      <th className="px-8 py-4 text-center">Items</th>
                      <th className="px-8 py-4 text-right">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {categories?.map(cat => (
                      <tr key={cat.id} className="hover:bg-gray-50/50 transition-colors">
                        <td className="px-8 py-5 font-bold text-gray-900">{cat.name}</td>
                        <td className="px-8 py-5 text-gray-500 text-sm">{cat.description || '-'}</td>
                        <td className="px-8 py-5 text-center">
                          <span className="px-3 py-1 bg-indigo-50 text-indigo-600 rounded-full font-bold text-xs border border-indigo-100">
                            {cat.itemCount || 0} items
                          </span>
                        </td>
                        <td className="px-8 py-5 text-right">
                          <button 
                            onClick={() => { setSelectedCategoryData(cat); setCategoryDialogOpen(true); }}
                            className="text-indigo-600 font-bold text-sm hover:underline"
                          >
                            Edit
                          </button>
                        </td>
                      </tr>
                    ))}
                    {(!categories || categories.length === 0) && !categoriesLoading && (
                      <tr>
                         <td colSpan={4} className="px-8 py-12 text-center text-gray-500 font-medium bg-gray-50/50">
                           No categories configured. Click "+ New Category" to create one.
                         </td>
                      </tr>
                    )}
                  </tbody>
               </table>
            </div>
          )}
        </div>
      </div>

      {/* Item Dialog */}
      {itemDialogOpen && (
        <Modal 
          title="Add New Inventory Item" 
          onClose={() => setItemDialogOpen(false)}
        >
          <ItemForm 
            categories={categories || []} 
            activeYearId={activeYear?.id || ''}
            onSubmit={(data) => addItemMutation.mutate(data)} 
            onCancel={() => setItemDialogOpen(false)}
          />
        </Modal>
      )}

      {/* Category Dialog */}
      {categoryDialogOpen && (
        <Modal 
          title={selectedCategoryData ? "Edit Inventory Category" : "Create Inventory Category"} 
          onClose={() => { setCategoryDialogOpen(false); setSelectedCategoryData(null); }}
        >
          <CategoryForm 
            category={selectedCategoryData}
            onSubmit={(data) => {
              if (selectedCategoryData) {
                updateCategoryMutation.mutate({ id: selectedCategoryData.id, data: { ...data, id: selectedCategoryData.id } });
              } else {
                addCategoryMutation.mutate(data);
              }
            }}
            onCancel={() => { setCategoryDialogOpen(false); setSelectedCategoryData(null); }}
          />
        </Modal>
      )}

      {/* Transaction Dialog */}
      {transactionDialogOpen && selectedItem && (
        <Modal 
          title={transactionType === 'StockIn' ? `Add Stock: ${selectedItem.name}` : `Issue Item: ${selectedItem.name}`} 
          onClose={() => setTransactionDialogOpen(false)}
        >
          <TransactionForm 
            type={transactionType}
            item={selectedItem}
            activeYearId={activeYear?.id || ''}
            onSubmit={(data) => transactionMutation.mutate(data)}
            onCancel={() => setTransactionDialogOpen(false)}
          />
        </Modal>
      )}

    </div>
  );
}

// Helper Components
function StatCard({ title, value, icon, color, isCurrency = false, isLoading = false, isAlert = false }: any) {
  const colors: any = {
    blue: 'from-blue-600 to-indigo-800 shadow-blue-200',
    purple: 'from-purple-600 to-indigo-900 shadow-purple-200',
    amber: 'from-orange-500 to-red-600 shadow-orange-200 text-white',
    emerald: 'from-emerald-600 to-teal-800 shadow-emerald-200'
  };

  return (
    <div className={`relative overflow-hidden rounded-3xl p-6 shadow-xl text-white bg-gradient-to-br ${colors[color]} ${isAlert ? 'animate-pulse ring-4 ring-amber-200' : ''}`}>
       <div className="absolute -right-2 -top-2 text-7xl opacity-10 pointer-events-none">{icon}</div>
       <div className="relative">
          <div className="text-xs font-black uppercase tracking-[0.2em] text-white/90 mb-2">{title}</div>
          {isLoading ? (
            <div className="h-10 w-24 bg-white/20 rounded-lg animate-pulse mt-2"></div>
          ) : (
            <div className="text-3xl font-black mb-1">
               {isCurrency ? new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(value) : value}
            </div>
          )}
       </div>
    </div>
  );
}

function Modal({ title, children, onClose }: any) {
  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black bg-opacity-50 backdrop-blur-sm animate-fadeIn">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg max-h-[90vh] overflow-y-auto animate-slideUp">
        <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between z-20">
          <h2 className="text-2xl font-bold text-gray-900">{title}</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 transition-colors">
            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
        <div className="p-6">
          {children}
        </div>
      </div>
    </div>
  );
}

function ItemForm({ categories, activeYearId, onSubmit, onCancel }: any) {
  const [formData, setFormData] = useState<CreateInventoryItemDto>({
    name: '',
    sku: '',
    description: '',
    categoryId: '',
    initialQuantity: 0,
    reorderLevel: 5,
    unitPrice: 0,
    academicYearId: activeYearId
  });

  return (
    <form className="space-y-6" onSubmit={(e) => { e.preventDefault(); onSubmit(formData); }}>
      <div className="grid grid-cols-2 gap-4">
        <div className="col-span-2">
           <label className="block text-sm font-medium text-gray-700 mb-1">Item Name <span className="text-red-500">*</span></label>
           <input type="text" required value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium" placeholder="e.g. Science Grade 10 Book"/>
        </div>
        <div>
           <label className="block text-sm font-medium text-gray-700 mb-1">SKU / Code <span className="text-red-500">*</span></label>
           <input type="text" required value={formData.sku} onChange={e => setFormData({...formData, sku: e.target.value})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium uppercase" placeholder="BK-10-SCI"/>
        </div>
        <div>
           <label className="block text-sm font-medium text-gray-700 mb-1">Category <span className="text-red-500">*</span></label>
           <select required value={formData.categoryId} onChange={e => setFormData({...formData, categoryId: e.target.value})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium">
              <option value="">Select Category</option>
              {categories.map((c: any) => (<option key={c.id} value={c.id}>{c.name}</option>))}
           </select>
        </div>
        <div>
           <label className="block text-sm font-medium text-gray-700 mb-1">Opening Stock</label>
           <input type="number" required value={formData.initialQuantity} onChange={e => setFormData({...formData, initialQuantity: parseInt(e.target.value)})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium"/>
        </div>
        <div>
           <label className="block text-sm font-medium text-gray-700 mb-1">Min Alert Level</label>
           <input type="number" required value={formData.reorderLevel} onChange={e => setFormData({...formData, reorderLevel: parseInt(e.target.value)})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium"/>
        </div>
        <div>
           <label className="block text-sm font-medium text-gray-700 mb-1">Unit Price (₹)</label>
           <input type="number" step="0.01" required value={formData.unitPrice} onChange={e => setFormData({...formData, unitPrice: parseFloat(e.target.value)})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium"/>
        </div>
      </div>
      <div className="flex gap-4 mt-8">
        <button type="button" onClick={onCancel} className="flex-1 py-3 bg-gray-100 text-gray-800 rounded-xl font-medium hover:bg-gray-200 transition-all">Cancel</button>
        <button type="submit" className="flex-1 py-3 bg-indigo-600 text-white rounded-xl font-medium shadow-lg hover:bg-indigo-700 transition-all">Create Item</button>
      </div>
    </form>
  );
}

function CategoryForm({ category, onSubmit, onCancel }: any) {
  const [formData, setFormData] = useState<CreateInventoryCategoryDto>({ 
    name: category?.name || '', 
    description: category?.description || '' 
  });
  return (
    <form className="space-y-6" onSubmit={(e) => { e.preventDefault(); onSubmit(formData); }}>
       <div>
           <label className="block text-sm font-medium text-gray-700 mb-1">Category Name <span className="text-red-500">*</span></label>
           <input type="text" required value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium" placeholder="e.g. Uniforms, Stationary"/>
       </div>
       <div>
           <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
           <textarea rows={3} value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium resize-none" placeholder="Brief details about this category..."/>
       </div>
       <div className="flex gap-4 mt-8">
        <button type="button" onClick={onCancel} className="flex-1 py-3 bg-gray-100 text-gray-800 rounded-xl font-medium">Cancel</button>
        <button type="submit" className="flex-1 py-3 bg-indigo-600 text-white rounded-xl font-medium shadow-lg hover:bg-indigo-700 transition-all">
          {category ? 'Update Category' : 'Save Category'}
        </button>
      </div>
    </form>
  );
}

function TransactionForm({ type, item, activeYearId, onSubmit, onCancel }: any) {
  const [formData, setFormData] = useState<StockTransactionDto>({
    itemId: item.id,
    transactionType: type,
    quantity: 1,
    receivedBy: '',
    remarks: '',
    academicYearId: activeYearId
  });

  return (
    <form className="space-y-6" onSubmit={(e) => { e.preventDefault(); onSubmit(formData); }}>
       <div className="bg-indigo-50 p-6 rounded-xl border border-indigo-100 mb-6">
          <div className="text-sm font-medium text-indigo-700 mb-1">Current Stock</div>
          <div className="text-3xl font-bold text-indigo-900">{item.totalQuantity} Units</div>
       </div>

       <div className="space-y-4">
          <div>
             <label className="block text-sm font-medium text-gray-700 mb-1">
               {type === 'StockIn' ? 'Quantity to Add' : 'Quantity to Issue'} <span className="text-red-500">*</span>
             </label>
             <input type="number" required min="1" max={type === 'StockOut' ? item.totalQuantity : 9999} value={formData.quantity} onChange={e => setFormData({...formData, quantity: parseInt(e.target.value)})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium"/>
             {type === 'StockOut' && item.totalQuantity < formData.quantity && (
                 <p className="text-red-500 text-xs mt-2 font-bold">⚠️ Cannot issue more than available stock!</p>
             )}
          </div>
          <div>
             <label className="block text-sm font-medium text-gray-700 mb-1">
               {type === 'StockIn' ? 'Purchase Ref / Source' : 'Recipient (Staff/Class)'}
             </label>
             <input type="text" value={formData.receivedBy || ''} onChange={e => setFormData({...formData, receivedBy: e.target.value})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium" placeholder={type === 'StockIn' ? 'e.g. Global Stationary Mart' : 'e.g. Science Dept / Staff John'}/>
          </div>
          <div>
             <label className="block text-sm font-medium text-gray-700 mb-1">Additional Remarks</label>
             <textarea rows={2} value={formData.remarks || ''} onChange={e => setFormData({...formData, remarks: e.target.value})} className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-medium resize-none" placeholder="Optional notes..."/>
          </div>
       </div>

       <div className="flex gap-4 mt-8">
        <button type="button" onClick={onCancel} className="flex-1 py-3 bg-gray-100 text-gray-800 rounded-xl font-medium">Cancel</button>
        <button 
            type="submit" 
            disabled={type === 'StockOut' && item.totalQuantity < formData.quantity}
            className={`flex-1 py-3 text-white rounded-xl font-medium shadow-lg transition-all ${
                type === 'StockIn' 
                    ? 'bg-emerald-600 hover:bg-emerald-700' 
                    : 'bg-pink-600 hover:bg-pink-700'
            } disabled:bg-gray-300 disabled:shadow-none`}
        >
          {type === 'StockIn' ? 'Confirm Stock In' : 'Confirm Issuance'}
        </button>
      </div>
    </form>
  );
}

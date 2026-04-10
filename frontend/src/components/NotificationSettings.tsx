import { useState, useEffect } from 'react';
import { notificationApi, type NotificationTemplate, type CreateNotificationTemplateDto, type UpdateNotificationTemplateDto } from '../services/api';
import toast from 'react-hot-toast';
import { WhatsAppIcon } from './WhatsAppIcon';

export function NotificationSettings() {
  const [templates, setTemplates] = useState<NotificationTemplate[]>([]);
  const [loading, setLoading] = useState(true);
  const [editingTemplate, setEditingTemplate] = useState<NotificationTemplate | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  useEffect(() => {
    fetchTemplates();
  }, []);

  const fetchTemplates = async () => {
    try {
      setLoading(true);
      const data = await notificationApi.getTemplates();
      setTemplates(data);
    } catch (error) {
      toast.error('Failed to load notification templates');
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    e.stopPropagation(); // Prevent bubbling to parent form
    if (!editingTemplate) return;

    try {
      if ('id' in editingTemplate && editingTemplate.id) {
        await notificationApi.updateTemplate(editingTemplate.id, editingTemplate as UpdateNotificationTemplateDto);
        toast.success('Template updated successfully');
      } else {
        // Omit id field for creation to avoid DTO mapping issues
        const { id, ...createDto } = editingTemplate as any;
        await notificationApi.createTemplate(createDto as CreateNotificationTemplateDto);
        toast.success('Template created successfully');
      }
      setIsModalOpen(false);
      fetchTemplates();
    } catch (error) {
      toast.error('Failed to save template');
    }
  };

  const openEditModal = (template?: NotificationTemplate) => {
    if (template) {
      setEditingTemplate({ ...template });
    } else {
      setEditingTemplate({
        id: '',
        name: '',
        description: '',
        content: '',
        channel: 'SMS',
        category: 'General',
        isActive: true,
      } as any);
    }
    setIsModalOpen(true);
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h3 className="text-xl font-bold text-gray-900">Message Templates</h3>
        <button
          onClick={() => openEditModal()}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
        >
          <span>➕</span> Add Template
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {templates.map((template) => (
          <div key={template.id} className="bg-white border border-gray-200 rounded-xl p-5 shadow-sm hover:shadow-md transition-shadow">
            <div className="flex justify-between items-start mb-3">
              <div>
                <span className={`px-3 py-1 rounded-full text-xs font-bold uppercase flex items-center gap-1.5 ${template.channel === 'SMS' ? 'bg-orange-100 text-orange-700' : 'bg-green-100 text-green-700'}`}>
                  {template.channel === 'SMS' ? (
                    <>📱 SMS</>
                  ) : (
                    <><WhatsAppIcon size={14} className="text-[#25D366]" /> WhatsApp</>
                  )}
                </span>
                <h4 className="text-lg font-bold text-gray-900 mt-2">{template.name}</h4>
                <p className="text-sm text-gray-500">{template.category}</p>
              </div>
              <div className="flex gap-2">
                <button
                  onClick={() => openEditModal(template)}
                  className="p-2 text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
                  title="Edit Template"
                >
                  ✏️
                </button>
              </div>
            </div>
            <p className="text-sm text-gray-600 mb-4 line-clamp-2 italic">"{template.content}"</p>
            <div className="flex items-center text-xs text-blue-600 font-medium">
              <span>View placeholders & details</span>
              <span className="ml-1">→</span>
            </div>
          </div>
        ))}
      </div>

      {isModalOpen && editingTemplate && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl max-w-2xl w-full max-h-[90vh] overflow-y-auto shadow-2xl">
            <div className="p-6 border-b border-gray-100 flex justify-between items-center">
              <h3 className="text-2xl font-bold text-gray-900">
                {editingTemplate.id ? 'Edit Template' : 'New Template'}
              </h3>
              <button 
                onClick={() => setIsModalOpen(false)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                ✕
              </button>
            </div>
            <form onSubmit={handleSave} className="p-6 space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Internal Name</label>
                  <input
                    type="text"
                    value={editingTemplate.name}
                    onChange={(e) => setEditingTemplate({ ...editingTemplate, name: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
                    placeholder="e.g., FEE_RECEIPT_SMS"
                    required
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Channel</label>
                  <select
                    value={editingTemplate.channel}
                    onChange={(e) => setEditingTemplate({ ...editingTemplate, channel: e.target.value as any })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
                  >
                    <option value="SMS">SMS</option>
                    <option value="WhatsApp">WhatsApp</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Category</label>
                <select
                  value={editingTemplate.category}
                  onChange={(e) => setEditingTemplate({ ...editingTemplate, category: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
                >
                  <option value="Fees">Fees</option>
                  <option value="Attendance">Attendance</option>
                  <option value="Transport">Transport</option>
                  <option value="General">General</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Message Content</label>
                <textarea
                  value={editingTemplate.content}
                  onChange={(e) => setEditingTemplate({ ...editingTemplate, content: e.target.value })}
                  className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none min-h-[120px]"
                  placeholder="Type your message here... Use {{StudentName}}, {{Amount}}, etc."
                  required
                />
                <div className="mt-2 p-3 bg-blue-50 rounded-lg">
                  <p className="text-xs text-blue-700 leading-relaxed font-medium">
                    <strong>💡 Tips:</strong> Use double curly braces for dynamic data: <br/>
                    <code className="bg-white px-1 rounded shadow-sm border border-blue-100 text-blue-900">{"{{StudentName}}"}</code>, 
                    <code className="bg-white px-1 rounded shadow-sm border border-blue-100 text-blue-900 ml-1">{"{{Amount}}"}</code>, 
                    <code className="bg-white px-1 rounded shadow-sm border border-blue-100 text-blue-900 ml-1">{"{{Date}}"}</code>,
                    <code className="bg-white px-1 rounded shadow-sm border border-blue-100 text-blue-900 ml-1">{"{{RouteName}}"}</code>
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="isActive"
                  checked={editingTemplate.isActive}
                  onChange={(e) => setEditingTemplate({ ...editingTemplate, isActive: e.target.checked })}
                  className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
                />
                <label htmlFor="isActive" className="text-sm font-medium text-gray-700">Active (Allows using this template in modules)</label>
              </div>

              <div className="flex justify-end gap-3 pt-4">
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="px-6 py-2 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 font-medium"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 font-medium shadow-md transition-all active:scale-95"
                >
                  Save Template
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

import React, { useState, useEffect } from 'react';
import toast from 'react-hot-toast';
import { useSchool } from '../hooks/useSchool';
import { type SchoolDto } from '../services/api';
import { NotificationSettings } from '../components/NotificationSettings';

type TabType = 'basic' | 'branding' | 'preferences' | 'notifications';

export function SettingsPage() {
  const { school, isLoading, updateSchool, uploadLogo } = useSchool();
  const [activeTab, setActiveTab] = useState<TabType>('basic');
  const [formData, setFormData] = useState<Partial<SchoolDto> | null>(null);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);

  // Initialize form data when school data loads
  useEffect(() => {
    if (!formData && school) {
      setFormData(school);
    }
  }, [school, formData]);

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-flex items-center justify-center w-12 h-12 bg-blue-100 rounded-full mb-4">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
          </div>
          <p className="text-gray-600">Loading settings...</p>
        </div>
      </div>
    );
  }

  if (!school) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="bg-red-100 rounded-full p-3 w-14 h-14 mx-auto mb-4">
            <svg className="w-8 h-8 text-red-600 mx-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4v2m0 4v2m0-14V5m0 14v2M6.406 6.406L4.93 4.93M19.07 19.07l-1.476-1.476m0-2.828l1.476-1.476M4.93 19.07l1.476-1.476m0-2.828L4.93 13.29" />
            </svg>
          </div>
          <p className="text-red-600 font-medium">Failed to load settings</p>
          <p className="text-gray-500 text-sm mt-1">Please try refreshing the page</p>
        </div>
      </div>
    );
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData) return;

    try {
      await updateSchool.mutateAsync(formData);
      toast.success('Settings saved successfully');
    } catch {
      toast.error('Failed to save settings');
    }
  };

  const handleLogoChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file size and type
    if (file.size > 5 * 1024 * 1024) {
      toast.error('File size exceeds 5MB limit');
      return;
    }

    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type)) {
      toast.error('Only image files are allowed (JPG, PNG, GIF)');
      return;
    }

    try {
      // Show preview
      const reader = new FileReader();
      reader.onloadend = () => {
        setLogoPreview(reader.result as string);
      };
      reader.readAsDataURL(file);

      // Upload logo
      await uploadLogo.mutateAsync(file);
      toast.success('Logo uploaded successfully');
    } catch {
      toast.error('Failed to upload logo');
    }
  };

  const displayLogo = logoPreview || (school.logoBase64 ? `data:image/png;base64,${school.logoBase64}` : null);

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="space-y-6">
          {/* Header */}
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-blue-800 bg-clip-text text-transparent flex items-center gap-3">
                <span>⚙️</span> School Settings
              </h1>
              <p className="text-gray-600 mt-2">Configure your school information, branding, and system preferences</p>
            </div>
          </div>

          {/* Tabs */}
          <div className="border-b border-gray-200">
            <nav className="flex space-x-8">
              <button
                onClick={() => setActiveTab('basic')}
                className={`${
                  activeTab === 'basic'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
              >
                📋 Basic Information
              </button>
              <button
                onClick={() => setActiveTab('branding')}
                className={`${
                  activeTab === 'branding'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
              >
                🎨 Branding
              </button>
              <button
                onClick={() => setActiveTab('preferences')}
                className={`${
                  activeTab === 'preferences'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
              >
                ⚡ Preferences
              </button>
              <button
                onClick={() => setActiveTab('notifications')}
                className={`${
                  activeTab === 'notifications'
                    ? 'border-blue-500 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
              >
                🔔 Notifications
              </button>
            </nav>
          </div>

          {/* Content Card */}
          <div className="bg-white rounded-xl shadow-lg border border-gray-100 overflow-hidden">
            {activeTab === 'notifications' ? (
              <div className="p-8">
                <NotificationSettings />
              </div>
            ) : (
              <form onSubmit={handleSubmit}>
                <div className="p-8 space-y-8">
                  {activeTab === 'basic' && (
                    <div className="space-y-6">
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            School Name <span className="text-red-500">*</span>
                          </label>
                          <input
                            type="text"
                            value={formData?.name || ''}
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                            required
                          />
                        </div>
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">
                            School Code <span className="text-red-500">*</span>
                          </label>
                          <input
                            type="text"
                            value={formData?.code || ''}
                            onChange={(e) => setFormData({ ...formData, code: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                            required
                          />
                        </div>
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">Address</label>
                        <input
                          type="text"
                          value={formData?.address || ''}
                          onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                          className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                        />
                      </div>

                      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">City</label>
                          <input
                            type="text"
                            value={formData?.city || ''}
                            onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          />
                        </div>
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">State</label>
                          <input
                            type="text"
                            value={formData?.state || ''}
                            onChange={(e) => setFormData({ ...formData, state: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          />
                        </div>
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Postal Code</label>
                          <input
                            type="text"
                            value={formData?.postalCode || ''}
                            onChange={(e) => setFormData({ ...formData, postalCode: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          />
                        </div>
                      </div>

                      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Phone Number</label>
                          <input
                            type="tel"
                            value={formData?.phoneNumber || ''}
                            onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          />
                        </div>
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Email Address</label>
                          <input
                            type="email"
                            value={formData?.emailAddress || ''}
                            onChange={(e) => setFormData({ ...formData, emailAddress: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          />
                        </div>
                      </div>

                      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Website</label>
                          <input
                            type="url"
                            value={formData?.website || ''}
                            onChange={(e) => setFormData({ ...formData, website: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          />
                        </div>
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Established Date</label>
                          <input
                            type="date"
                            value={formData?.establishedDate ? new Date(formData.establishedDate).toISOString().split('T')[0] : ''}
                            onChange={(e) => setFormData({ ...formData, establishedDate: new Date(e.target.value) })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          />
                        </div>
                      </div>
                    </div>
                  )}

                  {activeTab === 'branding' && (
                    <div className="space-y-6">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-4">School Logo</label>
                        <div className="flex gap-8">
                          <div className="flex-1">
                            <div className="border-2 border-dashed border-gray-300 rounded-xl p-8 text-center hover:border-blue-400 transition-colors cursor-pointer relative overflow-hidden bg-gray-50">
                              <input
                                type="file"
                                accept="image/*"
                                onChange={handleLogoChange}
                                className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
                              />
                              {displayLogo ? (
                                <div className="space-y-2">
                                  <img src={displayLogo} alt="Logo preview" className="inline-block h-40 object-contain" />
                                  <p className="text-xs text-gray-500 mt-2">Click to change logo</p>
                                </div>
                              ) : (
                                <div className="space-y-2">
                                  <svg className="mx-auto h-12 w-12 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48">
                                    <path d="M28 8H12a4 4 0 00-4 4v20a4 4 0 004 4h24a4 4 0 004-4V20" />
                                    <circle cx="20" cy="24" r="3" />
                                    <path d="M28 20l8-8m0 0v6m0-6h-6" />
                                  </svg>
                                  <p className="text-sm text-gray-600 font-medium">Click to upload logo</p>
                                  <p className="text-xs text-gray-500">PNG, JPG, GIF up to 5MB</p>
                                </div>
                              )}
                            </div>
                          </div>
                        </div>
                      </div>

                      <div className="border-t pt-6">
                        <h3 className="text-lg font-semibold text-gray-900 mb-6">Color Scheme</h3>
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-3">Primary Color</label>
                            <div className="flex gap-3 items-center">
                              <input
                                type="color"
                                value={formData?.primaryColor || '#1976D2'}
                                onChange={(e) => setFormData({ ...formData, primaryColor: e.target.value })}
                                className="w-16 h-10 rounded cursor-pointer border border-gray-300"
                              />
                              <input
                                type="text"
                                value={formData?.primaryColor || '#1976D2'}
                                onChange={(e) => setFormData({ ...formData, primaryColor: e.target.value })}
                                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                              />
                            </div>
                          </div>
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-3">Secondary Color</label>
                            <div className="flex gap-3 items-center">
                              <input
                                type="color"
                                value={formData?.secondaryColor || '#DC004E'}
                                onChange={(e) => setFormData({ ...formData, secondaryColor: e.target.value })}
                                className="w-16 h-10 rounded cursor-pointer border border-gray-300"
                              />
                              <input
                                type="text"
                                value={formData?.secondaryColor || '#DC004E'}
                                onChange={(e) => setFormData({ ...formData, secondaryColor: e.target.value })}
                                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                              />
                            </div>
                          </div>
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-3">Accent Color</label>
                            <div className="flex gap-3 items-center">
                              <input
                                type="color"
                                value={formData?.accentColor || '#FF6F00'}
                                onChange={(e) => setFormData({ ...formData, accentColor: e.target.value })}
                                className="w-16 h-10 rounded cursor-pointer border border-gray-300"
                              />
                              <input
                                type="text"
                                value={formData?.accentColor || '#FF6F00'}
                                onChange={(e) => setFormData({ ...formData, accentColor: e.target.value })}
                                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent text-sm"
                              />
                            </div>
                          </div>
                        </div>
                      </div>

                      <div className="border-t pt-6 space-y-6">
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Header Text</label>
                          <textarea
                            value={formData?.headerText || ''}
                            onChange={(e) => setFormData({ ...formData, headerText: e.target.value })}
                            rows={2}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                            placeholder="Text displayed in page header"
                          />
                        </div>

                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Footer Text</label>
                          <textarea
                            value={formData?.footerText || ''}
                            onChange={(e) => setFormData({ ...formData, footerText: e.target.value })}
                            rows={2}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                            placeholder="Text displayed in page footer"
                          />
                        </div>
                      </div>
                    </div>
                  )}

                  {activeTab === 'preferences' && (
                    <div className="space-y-6">
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Date Format</label>
                          <select
                            value={formData?.dateFormat || 'dd/MM/yyyy'}
                            onChange={(e) => setFormData({ ...formData, dateFormat: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          >
                            <option value="dd/MM/yyyy">dd/MM/yyyy</option>
                            <option value="MM/dd/yyyy">MM/dd/yyyy</option>
                            <option value="yyyy-MM-dd">yyyy-MM-dd</option>
                          </select>
                        </div>
                        <div>
                          <label className="block text-sm font-medium text-gray-700 mb-2">Currency Code</label>
                          <input
                            type="text"
                            value={formData?.currencyCode || 'INR'}
                            onChange={(e) => setFormData({ ...formData, currencyCode: e.target.value })}
                            className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                            maxLength={3}
                          />
                        </div>
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">Currency Symbol</label>
                        <input
                          type="text"
                          value={formData?.currencySymbol || '₹'}
                          onChange={(e) => setFormData({ ...formData, currencySymbol: e.target.value })}
                          className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-colors"
                          maxLength={5}
                        />
                      </div>
                    </div>
                  )}
                </div>

                {/* Form Actions */}
                <div className="border-t border-gray-200 bg-gray-50 px-8 py-6 flex justify-end gap-3 rounded-b-xl">
                  <button
                    type="button"
                    onClick={() => setFormData(school)}
                    className="px-6 py-2.5 border border-gray-300 rounded-lg text-gray-700 font-medium hover:bg-gray-100 transition-colors"
                    disabled={updateSchool.isPending}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="px-6 py-2.5 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-lg font-medium hover:shadow-lg hover:scale-105 transition-all duration-300 disabled:opacity-50 flex items-center gap-2"
                    disabled={updateSchool.isPending}
                  >
                    {updateSchool.isPending ? (
                      <>
                        <span className="inline-block animate-spin">⏳</span>
                        Saving...
                      </>
                    ) : (
                      <>
                        <span>💾</span>
                        Save Changes
                      </>
                    )}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

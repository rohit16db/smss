import { Toaster } from 'react-hot-toast';

export const ToastProvider = () => {
  return (
    <Toaster
      position="top-right"
      reverseOrder={false}
      gutter={8}
      toastOptions={{
        duration: 4000,
        style: {
          background: '#363636',
          color: '#fff',
          padding: '16px',
          borderRadius: '8px',
          fontSize: '14px',
          fontWeight: '500',
        },
        success: {
          duration: 3000,
          style: {
            background: '#10b981',
            color: '#fff',
          },
          icon: '✓',
        },
        error: {
          duration: 4000,
          style: {
            background: '#ef4444',
            color: '#fff',
          },
          icon: '✕',
        },
      }}
    />
  );
};

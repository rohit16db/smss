import toast from 'react-hot-toast';

export const showSuccessToast = (message: string) => {
  toast.success(message);
};

export const showErrorToast = (error: any, defaultMessage: string) => {
  const message = error?.response?.data?.message || error?.message || defaultMessage;
  toast.error(message);
};

export const showLoadingToast = (message: string) => {
  return toast.loading(message);
};

export const dismissToast = (toastId: string) => {
  toast.dismiss(toastId);
};

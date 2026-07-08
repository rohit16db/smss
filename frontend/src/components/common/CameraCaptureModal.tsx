import { useState, useEffect, useRef } from 'react';
import toast from 'react-hot-toast';

interface CameraCaptureModalProps {
  isOpen: boolean;
  onCapture: (file: File) => void;
  onCancel: () => void;
}

export function CameraCaptureModal({
  isOpen,
  onCapture,
  onCancel,
}: Readonly<CameraCaptureModalProps>) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);

  useEffect(() => {
    if (!isOpen) return;

    let activeStream: MediaStream | null = null;

    async function startCamera() {
      try {
        setError(null);
        setLoading(true);
        const stream = await navigator.mediaDevices.getUserMedia({
          video: { width: 640, height: 480, facingMode: 'user' },
          audio: false,
        });
        activeStream = stream;
        if (videoRef.current) {
          videoRef.current.srcObject = stream;
          // Play stream
          videoRef.current.onloadedmetadata = () => {
            videoRef.current?.play().catch(err => {
              console.error('Error starting video playback:', err);
            });
          };
        }
        setLoading(false);
      } catch (err: any) {
        console.error('Camera access error:', err);
        setError('Could not access camera. Please verify camera permissions are granted and no other application is using the camera.');
        setLoading(false);
      }
    }

    startCamera();

    return () => {
      if (activeStream) {
        activeStream.getTracks().forEach(track => track.stop());
      }
    };
  }, [isOpen]);

  if (!isOpen) return null;

  const handleCapture = () => {
    const video = videoRef.current;
    const canvas = canvasRef.current;
    if (video && canvas) {
      const context = canvas.getContext('2d');
      if (context) {
        // Match canvas dimensions to the video resolution
        const width = video.videoWidth || 640;
        const height = video.videoHeight || 480;
        canvas.width = width;
        canvas.height = height;

        // Draw current frame from video onto canvas
        context.drawImage(video, 0, 0, width, height);

        // Convert canvas image to Blob
        canvas.toBlob((blob) => {
          if (blob) {
            const file = new File([blob], `captured-snapshot-${Date.now()}.jpg`, { type: 'image/jpeg' });
            onCapture(file);
          } else {
            toast.error('Failed to capture snapshot. Please try again.');
          }
        }, 'image/jpeg', 0.95);
      }
    }
  };

  return (
    <div className="fixed inset-0 bg-black/75 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div className="bg-white rounded-2xl shadow-2xl max-w-lg w-full overflow-hidden border border-gray-100 animate-slide-up">
        {/* Header */}
        <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4 flex items-center justify-between text-white">
          <h2 className="text-xl font-bold flex items-center gap-2">
            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            Take Live Photo
          </h2>
          <button
            type="button"
            onClick={onCancel}
            className="text-white/80 hover:text-white transition-colors"
          >
            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Content */}
        <div className="p-6 bg-slate-50 flex flex-col items-center justify-center">
          <div className="w-full aspect-video bg-slate-900 rounded-xl overflow-hidden relative border-2 border-slate-200 shadow-inner flex items-center justify-center">
            {loading && (
              <div className="absolute inset-0 flex flex-col items-center justify-center text-white bg-slate-900/90 z-10">
                <svg className="animate-spin h-10 w-10 text-blue-500 mb-3" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <p className="text-sm font-semibold">Accessing camera stream...</p>
              </div>
            )}

            {error && (
              <div className="absolute inset-0 flex flex-col items-center justify-center text-center p-6 text-white bg-slate-900/95 z-10">
                <svg className="w-12 h-12 text-red-500 mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
                <p className="text-base font-bold text-red-400">Camera Access Error</p>
                <p className="text-xs text-slate-300 mt-2 max-w-sm">{error}</p>
              </div>
            )}

            <video
              ref={videoRef}
              autoPlay
              playsInline
              muted
              className="w-full h-full object-cover"
            />
            {/* Hidden canvas to draw frame */}
            <canvas ref={canvasRef} className="hidden" />
          </div>
          <p className="text-xs text-slate-500 mt-3 text-center">
            Position the student in the center of the frame before capturing.
          </p>
        </div>

        {/* Footer */}
        <div className="bg-white px-6 py-4 border-t border-gray-100 flex gap-4">
          <button
            type="button"
            onClick={onCancel}
            className="flex-1 px-4 py-2.5 border-2 border-gray-300 text-gray-700 font-semibold rounded-xl hover:bg-gray-50 transition-all flex items-center justify-center gap-2"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handleCapture}
            disabled={loading || !!error}
            className="flex-1 px-4 py-2.5 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white font-semibold rounded-xl shadow-md transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          >
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
            Capture Snapshot
          </button>
        </div>
      </div>
    </div>
  );
}

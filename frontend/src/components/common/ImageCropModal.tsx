import { useState, useRef, useEffect, useMemo } from 'react';
import toast from 'react-hot-toast';

interface ImageCropModalProps {
  isOpen: boolean;
  imageFile: File | null;
  onCropDone: (croppedFile: File) => void;
  onCancel: () => void;
  cropSize?: number; // Initial size of the crop box in pixels
}

interface CropBoxState {
  x: number;
  y: number;
  width: number;
  height: number;
}

// Helper function to crop the image using canvas
const cropImageFromCoordinates = async (
  imageSrc: string,
  cropX: number,
  cropY: number,
  cropWidth: number,
  cropHeight: number
): Promise<Blob> => {
  const image = new Image();
  image.crossOrigin = 'anonymous';
  image.src = imageSrc;

  return new Promise((resolve, reject) => {
    image.onload = () => {
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');

      if (!ctx) {
        reject(new Error('No 2d context'));
        return;
      }

      canvas.width = cropWidth;
      canvas.height = cropHeight;

      ctx.drawImage(
        image,
        cropX,
        cropY,
        cropWidth,
        cropHeight,
        0,
        0,
        cropWidth,
        cropHeight
      );

      canvas.toBlob((blob) => {
        if (blob) resolve(blob);
        else reject(new Error('Failed to create blob'));
      }, 'image/jpeg', 0.95);
    };
    image.onerror = () => reject(new Error('Failed to load image'));
  });
};

export function ImageCropModal({
  isOpen,
  imageFile,
  onCropDone,
  onCancel,
  cropSize = 300,
}: Readonly<ImageCropModalProps>) {
  const [relativeZoom, setRelativeZoom] = useState(1);
  const [offsetX, setOffsetX] = useState(0);
  const [offsetY, setOffsetY] = useState(0);
  const [isDraggingImage, setIsDraggingImage] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [imageSize, setImageSize] = useState({ width: 0, height: 0 });
  const [isProcessing, setIsProcessing] = useState(false);
  const [cropBox] = useState<CropBoxState>(() => {
    const containerSize = 400;
    const initialSize = cropSize;
    return {
      x: (containerSize - initialSize) / 2,
      y: (containerSize - initialSize) / 2,
      width: initialSize,
      height: initialSize,
    };
  });

  const containerRef = useRef<HTMLDivElement>(null);
  const imageRef = useRef<HTMLImageElement>(null);
  const cropBoxRef = useRef<HTMLDivElement>(null);

  const imageSrc = useMemo(() => {
    if (!imageFile) return '';
    return URL.createObjectURL(imageFile);
  }, [imageFile]);

  // Clean up blob URL on unmount or when imageFile changes
  useEffect(() => {
    return () => {
      if (imageSrc) {
        URL.revokeObjectURL(imageSrc);
      }
    };
  }, [imageSrc]);

  const containerSize = 400;

  // Calculate absolute zoom based on minScale and relativeZoom
  const minScale = imageSize.width ? Math.max(cropBox.width / imageSize.width, cropBox.height / imageSize.height) : 1;
  const currentZoom = minScale * relativeZoom;

  // Handle image load to get dimensions and initialize offsets/scale
  useEffect(() => {
    if (!imageSrc) return;

    const img = new Image();
    img.onload = () => {
      const width = img.width;
      const height = img.height;
      setImageSize({ width, height });
      setRelativeZoom(1);

      // Center the image in the container
      const initialMinScale = Math.max(cropBox.width / width, cropBox.height / height);
      const initialOffsetX = (containerSize - width * initialMinScale) / 2;
      const initialOffsetY = (containerSize - height * initialMinScale) / 2;
      setOffsetX(initialOffsetX);
      setOffsetY(initialOffsetY);
    };
    img.src = imageSrc;
  }, [imageSrc, cropBox.width, cropBox.height]);

  // Mouse down handler for image dragging (click anywhere in container to drag)
  const handleImageMouseDown = (e: React.MouseEvent<HTMLDivElement>) => {
    setIsDraggingImage(true);
    setDragStart({ x: e.clientX - offsetX, y: e.clientY - offsetY });
  };

  // Mouse move handler for image dragging
  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (isDraggingImage) {
      const newX = e.clientX - dragStart.x;
      const newY = e.clientY - dragStart.y;

      // Constrain offsets to keep image covering the crop box
      const maxX = cropBox.x;
      const minX = cropBox.x + cropBox.width - imageSize.width * currentZoom;
      const maxY = cropBox.y;
      const minY = cropBox.y + cropBox.height - imageSize.height * currentZoom;

      setOffsetX(Math.min(maxX, Math.max(minX, newX)));
      setOffsetY(Math.min(maxY, Math.max(minY, newY)));
    }
  };

  // Mouse up handler
  const handleMouseUp = () => {
    setIsDraggingImage(false);
  };

  // Handle zoom changes and adjust offsets to zoom relative to container center
  const handleZoomChange = (newRelativeZoom: number) => {
    if (!imageSize.width || !imageSize.height) return;

    const oldAbsoluteZoom = currentZoom;
    const newAbsoluteZoom = minScale * newRelativeZoom;

    const centerX = containerSize / 2;
    const centerY = containerSize / 2;

    // Shift offsets relative to container center
    const newOffsetX = centerX - ((centerX - offsetX) * newAbsoluteZoom) / oldAbsoluteZoom;
    const newOffsetY = centerY - ((centerY - offsetY) * newAbsoluteZoom) / oldAbsoluteZoom;

    // Constrain new offsets to cover the cropBox
    const maxX = cropBox.x;
    const minX = cropBox.x + cropBox.width - imageSize.width * newAbsoluteZoom;
    const maxY = cropBox.y;
    const minY = cropBox.y + cropBox.height - imageSize.height * newAbsoluteZoom;

    setOffsetX(Math.min(maxX, Math.max(minX, newOffsetX)));
    setOffsetY(Math.min(maxY, Math.max(minY, newOffsetY)));
    setRelativeZoom(newRelativeZoom);
  };

  // Handle crop and save
  const handleCropAndSave = async () => {
    if (!imageFile || !imageSize.width || !imageSize.height) return;

    try {
      setIsProcessing(true);

      const renderedImgLeft = offsetX;
      const renderedImgTop = offsetY;
      const renderedImgRight = offsetX + imageSize.width * currentZoom;
      const renderedImgBottom = offsetY + imageSize.height * currentZoom;

      const cropBoxLeft = cropBox.x;
      const cropBoxTop = cropBox.y;
      const cropBoxRight = cropBox.x + cropBox.width;
      const cropBoxBottom = cropBox.y + cropBox.height;

      // Find intersection of crop box and rendered image in container space
      const intersectionLeft = Math.max(cropBoxLeft, renderedImgLeft);
      const intersectionTop = Math.max(cropBoxTop, renderedImgTop);
      const intersectionRight = Math.min(cropBoxRight, renderedImgRight);
      const intersectionBottom = Math.min(cropBoxBottom, renderedImgBottom);

      // If no intersection, bail out
      if (intersectionLeft >= intersectionRight || intersectionTop >= intersectionBottom) {
        console.error('No intersection between crop box and image');
        setIsProcessing(false);
        return;
      }

      // Calculate intersection rectangle in rendered image space
      const intersectionLeftInRenderedImg = intersectionLeft - renderedImgLeft;
      const intersectionTopInRenderedImg = intersectionTop - renderedImgTop;
      const intersectionRightInRenderedImg = intersectionRight - renderedImgLeft;
      const intersectionBottomInRenderedImg = intersectionBottom - renderedImgTop;

      // Convert from rendered image space to original image space
      const cropX = Math.round(intersectionLeftInRenderedImg / currentZoom);
      const cropY = Math.round(intersectionTopInRenderedImg / currentZoom);
      const cropX2 = Math.round(intersectionRightInRenderedImg / currentZoom);
      const cropY2 = Math.round(intersectionBottomInRenderedImg / currentZoom);

      const cropWidth = Math.max(1, cropX2 - cropX);
      const cropHeight = Math.max(1, cropY2 - cropY);

      console.log('=== CROP CALCULATION DEBUG ===');
      console.log('Original image size:', imageSize);
      console.log('Zoom:', currentZoom, 'OffsetX:', offsetX, 'OffsetY:', offsetY);
      console.log('Crop box rect:', { left: cropBoxLeft, top: cropBoxTop, width: cropBox.width, height: cropBox.height });
      console.log('Final crop rect in original image:', { x: cropX, y: cropY, width: cropWidth, height: cropHeight });
      console.log('=== END DEBUG ===');

      const croppedBlob = await cropImageFromCoordinates(
        imageSrc,
        cropX,
        cropY,
        cropWidth,
        cropHeight
      );

      const croppedFile = new File(
        [croppedBlob],
        imageFile.name,
        { type: 'image/jpeg', lastModified: Date.now() }
      );

      onCropDone(croppedFile);
    } catch (error) {
      console.error('Error during image crop:', error);
      toast.error('Failed to crop image');
    } finally {
      setIsProcessing(false);
    }
  };

  if (!isOpen || !imageFile) return null;

  return (
    <div className="fixed inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg flex flex-col overflow-hidden animate-scale-up">
        {/* Header */}
        <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4 flex items-center justify-between text-white">
          <div>
            <h2 className="text-xl font-bold flex items-center gap-2">
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              Crop Profile Image
            </h2>
            <p className="text-xs text-blue-100 mt-1">Drag the photo to position it, or adjust zoom below.</p>
          </div>
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

        {/* Crop Container */}
        <div className="bg-slate-900 p-6 flex items-center justify-center overflow-hidden">
          <div
            ref={containerRef}
            className="relative bg-black rounded-xl overflow-hidden shadow-inner select-none cursor-grab"
            style={{
              width: containerSize,
              height: containerSize,
              border: '2px solid rgba(255, 255, 255, 0.1)',
            }}
            onMouseDown={handleImageMouseDown}
            onMouseMove={handleMouseMove}
            onMouseUp={handleMouseUp}
            onMouseLeave={handleMouseUp}
          >
            {/* Image */}
            {imageSize.width > 0 && (
              <img
                ref={imageRef}
                src={imageSrc}
                alt="Crop preview"
                style={{
                  position: 'absolute',
                  left: `${offsetX}px`,
                  top: `${offsetY}px`,
                  width: `${imageSize.width * currentZoom}px`,
                  height: `${imageSize.height * currentZoom}px`,
                  maxWidth: 'none',
                  maxHeight: 'none',
                  objectFit: 'cover',
                  pointerEvents: 'none',
                  userSelect: 'none',
                }}
              />
            )}

            {/* Darkened overlay outside crop area */}
            <div className="absolute inset-0 pointer-events-none">
              {/* Top overlay */}
              <div
                className="absolute left-0 right-0 bg-black/60"
                style={{
                  top: 0,
                  height: cropBox.y,
                }}
              />
              {/* Bottom overlay */}
              <div
                className="absolute left-0 right-0 bg-black/60"
                style={{
                  top: cropBox.y + cropBox.height,
                  height: containerSize - (cropBox.y + cropBox.height),
                }}
              />
              {/* Left overlay */}
              <div
                className="absolute left-0 bg-black/60"
                style={{
                  top: cropBox.y,
                  width: cropBox.x,
                  height: cropBox.height,
                }}
              />
              {/* Right overlay */}
              <div
                className="absolute bg-black/60"
                style={{
                  left: cropBox.x + cropBox.width,
                  top: cropBox.y,
                  width: containerSize - (cropBox.x + cropBox.width),
                  height: cropBox.height,
                }}
              />
            </div>

            {/* Crop Box */}
            <div
              ref={cropBoxRef}
              className="absolute border-2 border-blue-500 shadow-[0_0_0_1px_rgba(255,255,255,0.5)] pointer-events-none"
              style={{
                left: `${cropBox.x}px`,
                top: `${cropBox.y}px`,
                width: `${cropBox.width}px`,
                height: `${cropBox.height}px`,
                boxShadow: 'inset 0 0 0 1px rgba(255, 255, 255, 0.1)',
              }}
            >
              {/* Grid lines */}
              <div
                style={{
                  position: 'absolute',
                  left: '33.33%',
                  top: 0,
                  bottom: 0,
                  width: '1px',
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                }}
              />
              <div
                style={{
                  position: 'absolute',
                  left: '66.66%',
                  top: 0,
                  bottom: 0,
                  width: '1px',
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                }}
              />
              <div
                style={{
                  position: 'absolute',
                  top: '33.33%',
                  left: 0,
                  right: 0,
                  height: '1px',
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                }}
              />
              <div
                style={{
                  position: 'absolute',
                  top: '66.66%',
                  left: 0,
                  right: 0,
                  height: '1px',
                  backgroundColor: 'rgba(255, 255, 255, 0.15)',
                }}
              />
            </div>
          </div>
        </div>

        {/* Controls */}
        <div className="p-6 bg-white border-t border-gray-100">
          <div className="space-y-5">
            {/* Zoom Slider */}
            <div>
              <div className="flex justify-between text-sm font-semibold text-gray-700 mb-2">
                <span>Adjust scale</span>
                <span className="text-blue-600 font-bold">{relativeZoom.toFixed(1)}x</span>
              </div>
              <input
                type="range"
                min="1"
                max="5"
                step="0.05"
                value={relativeZoom}
                onChange={(e) => handleZoomChange(Number.parseFloat(e.target.value))}
                className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-blue-600"
              />
              <div className="flex justify-between text-[10px] text-gray-400 mt-1.5">
                <span>Fit Scale</span>
                <span>Zoom In</span>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-4 pt-3">
              <button
                type="button"
                onClick={onCancel}
                className="flex-1 px-4 py-2.5 border-2 border-gray-300 text-gray-700 font-semibold rounded-xl hover:bg-gray-50 transition-all flex items-center justify-center gap-2"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleCropAndSave}
                disabled={isProcessing || imageSize.width === 0}
                className="flex-1 px-4 py-2.5 bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white font-semibold rounded-xl shadow-md transition-all disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
              >
                {isProcessing ? 'Cropping...' : 'Apply Crop'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

import { useState, useRef, useEffect } from 'react';

interface ImageCropModalProps {
  isOpen: boolean;
  imageFile: File | null;
  onCropDone: (croppedFile: File) => void;
  onCancel: () => void;
  cropSize?: number; // Size of the crop box in pixels
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
      }, 'image/jpeg', 0.9);
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
}: ImageCropModalProps) {
  const [zoom, setZoom] = useState(1);
  const [offsetX, setOffsetX] = useState(0);
  const [offsetY, setOffsetY] = useState(0);
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [imageSize, setImageSize] = useState({ width: 0, height: 0 });
  const [isProcessing, setIsProcessing] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const imageRef = useRef<HTMLImageElement>(null);

  const imageSrc = imageFile ? URL.createObjectURL(imageFile) : '';

  // Calculate the position of the crop box in the center
  const containerSize = 400;
  const cropBoxX = (containerSize - cropSize) / 2;
  const cropBoxY = (containerSize - cropSize) / 2;

  // Handle image load to get dimensions
  useEffect(() => {
    if (!imageSrc) return;

    const img = new Image();
    img.onload = () => {
      setImageSize({ width: img.width, height: img.height });
    };
    img.src = imageSrc;
  }, [imageSrc]);

  // Mouse down handler
  const handleMouseDown = (e: React.MouseEvent<HTMLDivElement>) => {
    setIsDragging(true);
    setDragStart({ x: e.clientX - offsetX, y: e.clientY - offsetY });
  };

  // Mouse move handler
  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!isDragging) return;

    const newX = e.clientX - dragStart.x;
    const newY = e.clientY - dragStart.y;

    // Calculate bounds to keep image within the container while showing the crop area
    const maxX = 0;
    const minX = containerSize - imageSize.width * zoom;
    const maxY = 0;
    const minY = containerSize - imageSize.height * zoom;

    setOffsetX(Math.min(maxX, Math.max(minX, newX)));
    setOffsetY(Math.min(maxY, Math.max(minY, newY)));
  };

  // Mouse up handler
  const handleMouseUp = () => {
    setIsDragging(false);
  };

  // Handle crop and save
  const handleCropAndSave = async () => {
    if (!imageFile) return;

    try {
      setIsProcessing(true);

      // Calculate the crop coordinates in the original image
      // The crop box is positioned at (cropBoxX, cropBoxY) in the container
      // The image is positioned at (offsetX, offsetY) with zoom
      const cropX = (cropBoxX - offsetX) / zoom;
      const cropY = (cropBoxY - offsetY) / zoom;
      const cropWidth = cropSize / zoom;
      const cropHeight = cropSize / zoom;

      const croppedBlob = await cropImageFromCoordinates(
        imageSrc,
        Math.max(0, Math.round(cropX)),
        Math.max(0, Math.round(cropY)),
        Math.round(cropWidth),
        Math.round(cropHeight)
      );

      // Create a new File from the blob
      const croppedFile = new File(
        [croppedBlob],
        imageFile.name.replace(/\.[^/.]+$/, '_cropped.jpg'),
        { type: 'image/jpeg' }
      );

      onCropDone(croppedFile);
      setIsProcessing(false);
    } catch (error) {
      console.error('Error cropping image:', error);
      setIsProcessing(false);
    }
  };

  if (!isOpen || !imageFile) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-lg w-full max-w-2xl max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="p-6 border-b border-gray-200">
          <h2 className="text-2xl font-bold text-gray-900">Crop Image</h2>
          <p className="text-sm text-gray-600 mt-1">Move the image to position it in the crop area. The area within the box will be used as your profile picture.</p>
        </div>

        {/* Crop Container */}
        <div className="flex-1 bg-gray-900 p-6 flex items-center justify-center overflow-hidden">
          <div
            ref={containerRef}
            className="relative bg-black rounded-lg overflow-hidden cursor-move select-none"
            style={{
              width: containerSize,
              height: containerSize,
              border: '2px solid rgba(255, 255, 255, 0.2)',
            }}
            onMouseDown={handleMouseDown}
            onMouseMove={handleMouseMove}
            onMouseUp={handleMouseUp}
            onMouseLeave={handleMouseUp}
          >
            {/* Image */}
            <img
              ref={imageRef}
              src={imageSrc}
              alt="Crop preview"
              style={{
                position: 'absolute',
                left: `${offsetX}px`,
                top: `${offsetY}px`,
                width: `${imageSize.width * zoom}px`,
                height: `${imageSize.height * zoom}px`,
                objectFit: 'contain',
                pointerEvents: 'none',
                userSelect: 'none',
              }}
            />

            {/* Darkened overlay outside crop area */}
            <div className="absolute inset-0 pointer-events-none">
              {/* Top overlay */}
              <div
                className="absolute left-0 right-0 bg-black bg-opacity-70"
                style={{
                  top: 0,
                  height: cropBoxY,
                  width: '100%',
                }}
              />
              {/* Bottom overlay */}
              <div
                className="absolute left-0 right-0 bg-black bg-opacity-70"
                style={{
                  top: cropBoxY + cropSize,
                  height: containerSize - (cropBoxY + cropSize),
                  width: '100%',
                }}
              />
              {/* Left overlay */}
              <div
                className="absolute top-0 bottom-0 bg-black bg-opacity-70"
                style={{
                  left: 0,
                  width: cropBoxX,
                  height: '100%',
                }}
              />
              {/* Right overlay */}
              <div
                className="absolute top-0 bottom-0 bg-black bg-opacity-70"
                style={{
                  left: cropBoxX + cropSize,
                  width: containerSize - (cropBoxX + cropSize),
                  height: '100%',
                }}
              />

              {/* Crop box border and guides */}
              <div
                className="absolute border-2 border-yellow-400"
                style={{
                  left: cropBoxX,
                  top: cropBoxY,
                  width: cropSize,
                  height: cropSize,
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
                    backgroundColor: 'rgba(255, 255, 255, 0.2)',
                  }}
                />
                <div
                  style={{
                    position: 'absolute',
                    left: '66.66%',
                    top: 0,
                    bottom: 0,
                    width: '1px',
                    backgroundColor: 'rgba(255, 255, 255, 0.2)',
                  }}
                />
                <div
                  style={{
                    position: 'absolute',
                    top: '33.33%',
                    left: 0,
                    right: 0,
                    height: '1px',
                    backgroundColor: 'rgba(255, 255, 255, 0.2)',
                  }}
                />
                <div
                  style={{
                    position: 'absolute',
                    top: '66.66%',
                    left: 0,
                    right: 0,
                    height: '1px',
                    backgroundColor: 'rgba(255, 255, 255, 0.2)',
                  }}
                />

                {/* Corner markers */}
                {[
                  { top: -6, left: -6 },
                  { top: -6, right: -6 },
                  { bottom: -6, left: -6 },
                  { bottom: -6, right: -6 },
                ].map((pos, idx) => (
                  <div
                    key={idx}
                    style={{
                      position: 'absolute',
                      ...pos,
                      width: '12px',
                      height: '12px',
                      backgroundColor: '#FBBF24',
                      borderRadius: '2px',
                    }}
                  />
                ))}
              </div>
            </div>

            {/* Hint text */}
            <div className="absolute bottom-3 left-3 right-3 text-center">
              <p className="text-xs text-gray-400">Drag to reposition • Size: {cropSize}x{cropSize}px</p>
            </div>
          </div>
        </div>

        {/* Controls */}
        <div className="p-6 border-t border-gray-200">
          <div className="space-y-4">
            {/* Zoom Slider */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Zoom: {zoom.toFixed(2)}x
              </label>
              <input
                type="range"
                min="0.5"
                max="5"
                step="0.1"
                value={zoom}
                onChange={(e) => setZoom(parseFloat(e.target.value))}
                className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer accent-blue-600"
              />
              <div className="flex justify-between text-xs text-gray-500 mt-1">
                <span>0.5x (Zoom Out)</span>
                <span>5x (Zoom In)</span>
              </div>
            </div>

            {/* Buttons */}
            <div className="flex gap-3 pt-4">
              <button
                type="button"
                onClick={onCancel}
                className="flex-1 px-4 py-2 border border-gray-300 text-gray-700 font-medium rounded-lg hover:bg-gray-50 transition"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleCropAndSave}
                disabled={isProcessing}
                className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {isProcessing ? 'Processing...' : 'Crop & Use'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

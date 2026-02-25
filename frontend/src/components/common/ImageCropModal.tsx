import { useState, useRef, useEffect } from 'react';

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

type ResizeHandle = 'nw' | 'ne' | 'sw' | 'se' | 'n' | 's' | 'e' | 'w' | null;

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
}: Readonly<ImageCropModalProps>) {
  const [zoom, setZoom] = useState(1);
  const [offsetX, setOffsetX] = useState(0);
  const [offsetY, setOffsetY] = useState(0);
  const [isDraggingImage, setIsDraggingImage] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });
  const [imageSize, setImageSize] = useState({ width: 0, height: 0 });
  const [isProcessing, setIsProcessing] = useState(false);
  const [resizingHandle, setResizingHandle] = useState<ResizeHandle>(null);
  const [isDraggingBox, setIsDraggingBox] = useState(false);
  const [cropBox, setCropBox] = useState<CropBoxState>(() => {
    const containerSize = 400;
    const initialSize = cropSize;
    return {
      x: (containerSize - initialSize) / 2,
      y: (containerSize - initialSize) / 2,
      width: initialSize,
      height: initialSize,
    };
  });
  const [dragBoxStart, setDragBoxStart] = useState({ x: 0, y: 0 });

  const containerRef = useRef<HTMLDivElement>(null);
  const imageRef = useRef<HTMLImageElement>(null);
  const cropBoxRef = useRef<HTMLDivElement>(null);

  const imageSrc = imageFile ? URL.createObjectURL(imageFile) : '';
  const containerSize = 400;
  const minCropSize = 50;

  // Handle image load to get dimensions
  useEffect(() => {
    if (!imageSrc) return;

    const img = new Image();
    img.onload = () => {
      setImageSize({ width: img.width, height: img.height });
    };
    img.src = imageSrc;
  }, [imageSrc]);

  // Mouse down handler for image dragging
  const handleImageMouseDown = (e: React.MouseEvent<HTMLDivElement>) => {
    // Don't start image drag if clicking on crop box
    const rect = cropBoxRef.current?.getBoundingClientRect();
    const containerRect = containerRef.current?.getBoundingClientRect();
    if (rect && containerRect) {
      const clickX = e.clientX - containerRect.left;
      const clickY = e.clientY - containerRect.top;
      if (
        clickX >= cropBox.x &&
        clickX <= cropBox.x + cropBox.width &&
        clickY >= cropBox.y &&
        clickY <= cropBox.y + cropBox.height
      ) {
        return;
      }
    }

    setIsDraggingImage(true);
    setDragStart({ x: e.clientX - offsetX, y: e.clientY - offsetY });
  };

  // Mouse move handler for image dragging
  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (isDraggingImage) {
      const newX = e.clientX - dragStart.x;
      const newY = e.clientY - dragStart.y;

      // Calculate bounds to keep image within the container
      const maxX = 0;
      const minX = containerSize - imageSize.width * zoom;
      const maxY = 0;
      const minY = containerSize - imageSize.height * zoom;

      setOffsetX(Math.min(maxX, Math.max(minX, newX)));
      setOffsetY(Math.min(maxY, Math.max(minY, newY)));
    }

    if (resizingHandle) {
      handleResizeMove(e);
    }

    if (isDraggingBox) {
      handleCropBoxMove(e);
    }
  };

  // Mouse up handler
  const handleMouseUp = () => {
    setIsDraggingImage(false);
    setResizingHandle(null);
    setIsDraggingBox(false);
  };

  // Handle crop box dragging
  const handleCropBoxMouseDown = (e: React.MouseEvent<HTMLDivElement>) => {
    e.stopPropagation();
    setIsDraggingBox(true);
    setDragBoxStart({
      x: e.clientX - cropBox.x,
      y: e.clientY - cropBox.y,
    });
  };

  const handleCropBoxMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const newX = e.clientX - dragBoxStart.x;
    const newY = e.clientY - dragBoxStart.y;

    // Constrain crop box within container
    const constrainedX = Math.max(0, Math.min(newX, containerSize - cropBox.width));
    const constrainedY = Math.max(0, Math.min(newY, containerSize - cropBox.height));

    setCropBox((prev) => ({
      ...prev,
      x: constrainedX,
      y: constrainedY,
    }));
  };

  // Handle resize handle mouse down
  const handleResizeMouseDown = (e: React.MouseEvent<HTMLDivElement>, handle: ResizeHandle) => {
    e.stopPropagation();
    setResizingHandle(handle);
  };

  const handleResizeMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!resizingHandle) return;

    const containerRect = containerRef.current?.getBoundingClientRect();
    if (!containerRect) return;

    const currentX = e.clientX - containerRect.left;
    const currentY = e.clientY - containerRect.top;

    const newCropBox = { ...cropBox };

    // Handle corner and edge resizing
    switch (resizingHandle) {
      case 'nw':
        newCropBox.x = Math.max(0, currentX);
        newCropBox.y = Math.max(0, currentY);
        newCropBox.width = Math.max(minCropSize, cropBox.x + cropBox.width - newCropBox.x);
        newCropBox.height = Math.max(minCropSize, cropBox.y + cropBox.height - newCropBox.y);
        break;
      case 'ne':
        newCropBox.y = Math.max(0, currentY);
        newCropBox.width = Math.max(minCropSize, currentX - cropBox.x);
        newCropBox.height = Math.max(minCropSize, cropBox.y + cropBox.height - newCropBox.y);
        break;
      case 'sw':
        newCropBox.x = Math.max(0, currentX);
        newCropBox.width = Math.max(minCropSize, cropBox.x + cropBox.width - newCropBox.x);
        newCropBox.height = Math.max(minCropSize, currentY - cropBox.y);
        break;
      case 'se':
        newCropBox.width = Math.max(minCropSize, currentX - cropBox.x);
        newCropBox.height = Math.max(minCropSize, currentY - cropBox.y);
        break;
      case 'n':
        newCropBox.y = Math.max(0, currentY);
        newCropBox.height = Math.max(minCropSize, cropBox.y + cropBox.height - newCropBox.y);
        break;
      case 's':
        newCropBox.height = Math.max(minCropSize, currentY - cropBox.y);
        break;
      case 'e':
        newCropBox.width = Math.max(minCropSize, currentX - cropBox.x);
        break;
      case 'w':
        newCropBox.x = Math.max(0, currentX);
        newCropBox.width = Math.max(minCropSize, cropBox.x + cropBox.width - newCropBox.x);
        break;
    }

    // Constrain within container
    if (newCropBox.x + newCropBox.width > containerSize) {
      newCropBox.width = containerSize - newCropBox.x;
    }
    if (newCropBox.y + newCropBox.height > containerSize) {
      newCropBox.height = containerSize - newCropBox.y;
    }

    setCropBox(newCropBox);
  };

  // Handle crop and save
  const handleCropAndSave = async () => {
    if (!imageFile || !imageSize.width || !imageSize.height) return;

    try {
      setIsProcessing(true);

      // The image is rendered at position (offsetX, offsetY) in the container with size (imageSize.width * zoom, imageSize.height * zoom)
      // The crop box is positioned at (cropBox.x, cropBox.y) with size (cropBox.width, cropBox.height) in the container
      // We need to find the intersection of the crop box and image, then convert back to original image coordinates

      const renderedImgLeft = offsetX;
      const renderedImgTop = offsetY;
      const renderedImgRight = offsetX + imageSize.width * zoom;
      const renderedImgBottom = offsetY + imageSize.height * zoom;

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

      // Calculate the intersection rectangle in rendered image space (relative to rendered image start)
      const intersectionLeftInRenderedImg = intersectionLeft - renderedImgLeft;
      const intersectionTopInRenderedImg = intersectionTop - renderedImgTop;
      const intersectionRightInRenderedImg = intersectionRight - renderedImgLeft;
      const intersectionBottomInRenderedImg = intersectionBottom - renderedImgTop;

      // Convert from rendered image space to original image space
      const cropX = Math.round(intersectionLeftInRenderedImg / zoom);
      const cropY = Math.round(intersectionTopInRenderedImg / zoom);
      const cropX2 = Math.round(intersectionRightInRenderedImg / zoom);
      const cropY2 = Math.round(intersectionBottomInRenderedImg / zoom);

      const cropWidth = Math.max(1, cropX2 - cropX);
      const cropHeight = Math.max(1, cropY2 - cropY);

      console.log('=== CROP CALCULATION DEBUG ===');
      console.log('Original image size:', imageSize);
      console.log('Zoom:', zoom, 'OffsetX:', offsetX, 'OffsetY:', offsetY);
      console.log('Rendered image rect:', { left: renderedImgLeft, top: renderedImgTop, right: renderedImgRight, bottom: renderedImgBottom });
      console.log('Crop box rect:', { left: cropBoxLeft, top: cropBoxTop, right: cropBoxRight, bottom: cropBoxBottom });
      console.log('Intersection rect (container space):', { left: intersectionLeft, top: intersectionTop, right: intersectionRight, bottom: intersectionBottom });
      console.log('Intersection in rendered img space:', { left: intersectionLeftInRenderedImg, top: intersectionTopInRenderedImg, right: intersectionRightInRenderedImg, bottom: intersectionBottomInRenderedImg });
      console.log('Final crop rect in original image:', { x: cropX, y: cropY, width: cropWidth, height: cropHeight });
      console.log('=== END DEBUG ===');

      const croppedBlob = await cropImageFromCoordinates(
        imageSrc,
        cropX,
        cropY,
        cropWidth,
        cropHeight
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

  const getCursorStyle = (handle: ResizeHandle): string => {
    if (handle === null) return 'grab';
    
    const cursorMap: Record<Exclude<ResizeHandle, null>, string> = {
      nw: 'nwse-resize',
      ne: 'nesw-resize',
      sw: 'nesw-resize',
      se: 'nwse-resize',
      n: 'ns-resize',
      s: 'ns-resize',
      e: 'ew-resize',
      w: 'ew-resize',
    };
    return cursorMap[handle] || 'default';
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-lg w-full max-w-2xl max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="p-6 border-b border-gray-200">
          <h2 className="text-2xl font-bold text-gray-900">Crop Image</h2>
          <p className="text-sm text-gray-600 mt-1">Drag the crop box to position it, drag edges/corners to resize. The area within the box will be used as your profile picture.</p>
        </div>

        {/* Crop Container */}
        <div className="flex-1 bg-gray-900 p-6 flex items-center justify-center overflow-hidden">
          <div
            ref={containerRef}
            className="relative bg-black rounded-lg overflow-hidden"
            style={{
              width: containerSize,
              height: containerSize,
              border: '2px solid rgba(255, 255, 255, 0.2)',
              cursor: isDraggingImage ? 'grabbing' : 'grab',
            }}
            onMouseDown={handleImageMouseDown}
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
                  left: 0,
                  right: 0,
                  height: cropBox.y,
                  width: '100%',
                }}
              />
              {/* Bottom overlay */}
              <div
                className="absolute left-0 right-0 bg-black bg-opacity-70"
                style={{
                  top: cropBox.y + cropBox.height,
                  left: 0,
                  right: 0,
                  height: containerSize - (cropBox.y + cropBox.height),
                  width: '100%',
                }}
              />
              {/* Left overlay */}
              <div
                className="absolute top-0 bottom-0 bg-black bg-opacity-70"
                style={{
                  left: 0,
                  top: cropBox.y,
                  width: cropBox.x,
                  height: cropBox.height,
                }}
              />
              {/* Right overlay */}
              <div
                className="absolute top-0 bottom-0 bg-black bg-opacity-70"
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
              className="absolute border-2 border-yellow-400 group"
              style={{
                left: `${cropBox.x}px`,
                top: `${cropBox.y}px`,
                width: `${cropBox.width}px`,
                height: `${cropBox.height}px`,
                boxShadow: 'inset 0 0 0 1px rgba(255, 255, 255, 0.1)',
                cursor: isDraggingBox ? 'grabbing' : 'grab',
              }}
              onMouseDown={handleCropBoxMouseDown}
              onMouseMove={handleMouseMove}
              onMouseUp={handleMouseUp}
              onMouseLeave={handleMouseUp}
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
                  pointerEvents: 'none',
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
                  pointerEvents: 'none',
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
                  pointerEvents: 'none',
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
                  pointerEvents: 'none',
                }}
              />

              {/* Corner and Edge Resize Handles */}
              {/* Top-Left Corner */}
              <div
                style={{
                  position: 'absolute',
                  top: -8,
                  left: -8,
                  width: '16px',
                  height: '16px',
                  backgroundColor: '#FBBF24',
                  borderRadius: '50%',
                  cursor: getCursorStyle('nw'),
                }}
                onMouseDown={(e) => handleResizeMouseDown(e, 'nw')}
                title="Drag to resize"
              />
              {/* Top-Right Corner */}
              <div
                style={{
                  position: 'absolute',
                  top: -8,
                  right: -8,
                  width: '16px',
                  height: '16px',
                  backgroundColor: '#FBBF24',
                  borderRadius: '50%',
                  cursor: getCursorStyle('ne'),
                }}
                onMouseDown={(e) => handleResizeMouseDown(e, 'ne')}
                title="Drag to resize"
              />
              {/* Bottom-Left Corner */}
              <div
                style={{
                  position: 'absolute',
                  bottom: -8,
                  left: -8,
                  width: '16px',
                  height: '16px',
                  backgroundColor: '#FBBF24',
                  borderRadius: '50%',
                  cursor: getCursorStyle('sw'),
                }}
                onMouseDown={(e) => handleResizeMouseDown(e, 'sw')}
                title="Drag to resize"
              />
              {/* Bottom-Right Corner */}
              <div
                style={{
                  position: 'absolute',
                  bottom: -8,
                  right: -8,
                  width: '16px',
                  height: '16px',
                  backgroundColor: '#FBBF24',
                  borderRadius: '50%',
                  cursor: getCursorStyle('se'),
                }}
                onMouseDown={(e) => handleResizeMouseDown(e, 'se')}
                title="Drag to resize"
              />
              {/* Top Edge */}
              <div
                style={{
                  position: 'absolute',
                  top: -6,
                  left: '50%',
                  transform: 'translateX(-50%)',
                  width: '40px',
                  height: '12px',
                  backgroundColor: 'rgba(251, 191, 36, 0.6)',
                  borderRadius: '2px',
                  cursor: getCursorStyle('n'),
                }}
                onMouseDown={(e) => handleResizeMouseDown(e, 'n')}
                title="Drag to resize"
              />
              {/* Bottom Edge */}
              <div
                style={{
                  position: 'absolute',
                  bottom: -6,
                  left: '50%',
                  transform: 'translateX(-50%)',
                  width: '40px',
                  height: '12px',
                  backgroundColor: 'rgba(251, 191, 36, 0.6)',
                  borderRadius: '2px',
                  cursor: getCursorStyle('s'),
                }}
                onMouseDown={(e) => handleResizeMouseDown(e, 's')}
                title="Drag to resize"
              />
              {/* Left Edge */}
              <div
                style={{
                  position: 'absolute',
                  left: -6,
                  top: '50%',
                  transform: 'translateY(-50%)',
                  width: '12px',
                  height: '40px',
                  backgroundColor: 'rgba(251, 191, 36, 0.6)',
                  borderRadius: '2px',
                  cursor: getCursorStyle('w'),
                }}
                onMouseDown={(e) => handleResizeMouseDown(e, 'w')}
                title="Drag to resize"
              />
              {/* Right Edge */}
              <div
                style={{
                  position: 'absolute',
                  right: -6,
                  top: '50%',
                  transform: 'translateY(-50%)',
                  width: '12px',
                  height: '40px',
                  backgroundColor: 'rgba(251, 191, 36, 0.6)',
                  borderRadius: '2px',
                  cursor: getCursorStyle('e'),
                }}
                onMouseDown={(e) => handleResizeMouseDown(e, 'e')}
                title="Drag to resize"
              />
            </div>

            {/* Hint text */}
            <div className="absolute bottom-3 left-3 right-3 text-center pointer-events-none">
              <p className="text-xs text-gray-400">Size: {Math.round(cropBox.width)}x{Math.round(cropBox.height)}px</p>
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
                onChange={(e) => setZoom(Number.parseFloat(e.target.value))}
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

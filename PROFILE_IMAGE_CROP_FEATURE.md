# Profile Image Crop Feature - Student Section

## Overview

The Profile Image Crop feature for the Student section has been enhanced with **resizable** and **draggable** crop box functionality. This allows users to:

1. **Drag the crop box** anywhere over the image
2. **Resize the crop box** from all corners and edges
3. **Zoom in/out** on the image for precise positioning
4. **Pan the image** by dragging outside the crop box

## Features

### 1. Resizable Crop Box
- **8 Resize Handles**: 4 corners + 4 edges
- **Corner handles** (circular, yellow):
  - Top-Left (↖)
  - Top-Right (↗)
  - Bottom-Left (↙)
  - Bottom-Right (↘)
- **Edge handles** (rectangular, semi-transparent yellow):
  - Top (↑)
  - Bottom (↓)
  - Left (←)
  - Right (→)

### 2. Draggable Crop Box
- Click and drag the crop box border to move it anywhere on the image
- The entire crop box moves together while maintaining its size
- Constrained within the container boundaries

### 3. Image Panning
- Drag outside the crop box to pan the image
- Useful for positioning specific areas when zoomed in

### 4. Zoom Control
- Zoom slider from 0.5x (zoom out) to 5x (zoom in)
- Precise control for detailed cropping
- Real-time zoom feedback

### 5. Visual Guides
- **Grid lines** inside the crop box for rule-of-thirds composition
- **Darkened overlay** outside the crop area for focus
- **Real-time dimension display** showing current crop size
- **Rule-of-thirds guides** to help with composition

## User Interface

### Crop Modal Layout
```
┌─────────────────────────────────────┐
│ Crop Image                          │
│ [Instructions]                      │
├─────────────────────────────────────┤
│                                     │
│    ┌─────────────────────────────┐ │
│    │    [Darkened Area]          │ │
│    │  ┌────────────────────────┐ │ │
│    │  │ ● Cropped Image ●      │ │ │
│    │  │ │   Grid Lines   │     │ │ │
│    │  │ │      [●]       │     │ │ │
│    │  │ ●   [●]   [●]   ●     │ │ │
│    │  │     Grid Lines        │ │ │
│    │  │        [●]       │    │ │ │
│    │  │ ●                 ●   │ │ │
│    │  └────────────────────────┘ │ │
│    │       Size: 300x300px        │ │
│    └─────────────────────────────┘ │
│                                     │
├─────────────────────────────────────┤
│ Zoom: 1.00x                         │
│ [========●=========================] │
│                                     │
│ [Cancel]  [Crop & Use]              │
└─────────────────────────────────────┘
```

### Cursor Feedback
- **Grab cursor**: When hovering over crop box (indicates draggable)
- **Grabbing cursor**: While dragging the crop box
- **Resize cursors**: When hovering over handles
  - ↖ NW-SE diagonal: Corner handles
  - ↙ NE-SW diagonal: Opposite corners
  - ↕ Vertical: Top/Bottom edges
  - ↔ Horizontal: Left/Right edges

## How to Use

### Step 1: Upload Image
1. Go to the Student section (Add or Edit Student)
2. Scroll to **Profile Image** section
3. Click **"Click to upload"** or drag an image into the upload area
4. Accepted formats: PNG, JPG, GIF, WebP (Max 5MB)

### Step 2: Position Image
The crop modal opens automatically after image selection.

**To move the image:**
- Click and drag outside the crop box to reposition the image
- Use the **Zoom slider** to zoom in/out for precise positioning

### Step 3: Adjust Crop Box
There are two ways to adjust the crop box:

**Option A: Resize from Corners**
- Click on any corner handle (● symbols)
- Drag to resize maintaining aspect ratio and position

**Option B: Resize from Edges**
- Click on any edge handle
- Drag to resize in that direction

**Option C: Move Entire Crop Box**
- Click on the yellow border of the crop box
- Drag to move the entire box to a different position

### Step 4: Fine-tune with Zoom
- Use the zoom slider at the bottom
- Zoom in (5x) for detailed cropping
- Zoom out (0.5x) for full image view

### Step 5: Apply Crop
- Click **"Crop & Use"** to apply the crop
- Click **"Cancel"** to discard and reselect

## Technical Details

### Crop Box Constraints
- **Minimum size**: 50x50 pixels
- **Constrained within**: 400x400px container
- **Aspect ratio**: Can be any ratio (not locked)

### Image Panning Constraints
- Image cannot extend beyond container on zoomed out view
- Image can extend beyond container when zoomed in
- Image stays positioned within bounds

### Cropped Image Output
- **Format**: JPEG (.jpg)
- **Quality**: 90% compression
- **Dimensions**: Exact size of the crop box selected
- **Named as**: `{original_name}_cropped.jpg`

## Code Implementation

### Component: ImageCropModal.tsx
Location: `frontend/src/components/common/ImageCropModal.tsx`

**Key Functions:**
- `handleCropBoxMouseDown`: Initiates crop box dragging
- `handleCropBoxMove`: Moves entire crop box
- `handleResizeMouseDown`: Initiates resize operation
- `handleResizeMove`: Handles resizing from all 8 handles
- `handleImageMouseDown`: Initiates image panning
- `handleMouseMove`: Unified mouse movement handler
- `handleCropAndSave`: Crops and exports final image

**State Management:**
```typescript
interface CropBoxState {
  x: number;      // X position in container
  y: number;      // Y position in container
  width: number;  // Width in pixels
  height: number; // Height in pixels
}
```

### Integration: StudentsPage.tsx
Location: `frontend/src/pages/StudentsPage.tsx`

**Flow:**
1. User selects image → `handleImageSelect(file)`
2. Crop modal opens with `tempImageFile`
3. User completes crop → `handleImageCropDone(croppedFile)`
4. Cropped file stored in `imageFile` state
5. On form submit → API uploads cropped image

## Browser Compatibility

✅ Modern browsers with:
- HTML5 Canvas API
- File API
- Mouse Events API
- CSS Grid Layout

Tested on:
- Chrome 90+
- Firefox 88+
- Edge 90+
- Safari 14+

## Performance Notes

- **Canvas cropping**: Done on-demand (only when "Crop & Use" clicked)
- **No real-time processing**: DOM updates only on mouse move
- **Memory efficient**: Uses blob conversion for file handling
- **Smooth interactions**: Optimized React renders with proper state isolation

## Troubleshooting

### Image not showing in crop modal
- Ensure file is valid image format (PNG, JPG, GIF, WebP)
- Check file size is under 5MB
- Verify browser supports File API

### Crop box stuck or unresponsive
- Try refreshing the page
- Ensure mouse events are properly registered
- Check browser console for errors

### Cropped image too small/large
- Use zoom slider to get precise view
- Resize crop box to desired dimensions
- Check the size display (Size: XXxYYpx)

### Image quality degraded
- The system uses 90% JPEG compression
- Original image should be high quality
- Larger source image = better crop quality

## Future Enhancements

Potential improvements:
1. **Aspect ratio locking** (1:1, 16:9, custom)
2. **Rotation control** (0°, 90°, 180°, 270°, custom)
3. **Undo/Reset** functionality
4. **Crop presets** (profile pic, thumbnail, etc.)
5. **Touch/Mobile support** enhancements
6. **Keyboard shortcuts** for power users
7. **Crop history** tracking

## Related Files

- `frontend/src/components/common/ImageCropModal.tsx` - Main component
- `frontend/src/pages/StudentsPage.tsx` - Integration point
- `backend/src/SMS.Application/DTOs/` - API DTOs
- `backend/src/SMS.Infrastructure/` - File upload handling

## FAQ

**Q: Can I crop without changing the image aspect ratio?**
A: Not currently, but you can manually resize maintaining proportions using corner handles.

**Q: What happens if I move the crop box outside the image?**
A: The crop box stays within the container. Ensure image covers the desired crop area.

**Q: Can I upload multiple images for one student?**
A: The system replaces the previous image. Only one profile image per student is supported.

**Q: Is my crop reversible?**
A: The original image is not stored server-side after cropping. Download the original separately if needed for future reference.

**Q: Can I crop to exact dimensions?**
A: Not through UI, but the dimensions display helps you estimate. API could support exact dimensions in future.

# Image Crop Feature - Fixed-Size Box

## Overview
The image crop feature now uses a **fixed-size crop box** (300x300 pixels) that users can position over their image. Only the content within the crop box will be used as the profile picture.

## How It Works

### 1. **Image Upload**
- Users click the image upload area in the Student or Teacher form
- A file picker opens to select an image (JPEG, PNG, GIF, WebP - Max 5MB)

### 2. **Crop Modal Opens**
When an image is selected, the crop modal displays:
- **Fixed Crop Box (300x300px)** - Yellow border with corner markers
- **Darkened Overlay** - Area outside the crop box appears darker
- **Grid Lines** - 3x3 grid inside the crop box for reference
- **Zoom Slider** - Ranges from 0.5x to 5x zoom

### 3. **Positioning the Image**
Users can:
- **Drag the image** - Click and drag to move the image around
- **Zoom** - Use the slider to zoom in/out
- **Position precisely** - Move until the desired portion is within the yellow box

### 4. **Crop & Use**
- Click "Crop & Use" button
- The system extracts exactly what's in the yellow box
- A preview is shown in the form
- The cropped image is saved when the form is submitted

## Key Features

✅ **Fixed-Size Box** - Always 300x300 pixels (perfect square)
✅ **Clear Visual Indicator** - Yellow border shows exact crop area
✅ **Grid Lines** - 3x3 grid helps with positioning
✅ **Smooth Dragging** - Drag to reposition image easily
✅ **Flexible Zoom** - 0.5x to 5x zoom range for precision
✅ **Dark Overlay** - Shows what will be cropped out
✅ **Preview Before Save** - See the cropped result in the form
✅ **File Size Control** - Max 5MB with validation

## Technical Details

- **Crop Algorithm**: Uses HTML5 Canvas for precise image extraction
- **Format**: Output is always JPEG with 90% quality
- **Size**: Fixed 300x300 pixels for consistency
- **Custom Implementation**: No external heavy library needed (removed react-easy-crop)

## User Experience

1. Student/Teacher clicks **"Click to upload"** in the Profile Image section
2. Modal opens with image and crop box visible
3. User drags image to position it within the yellow box
4. User zooms if needed for fine-tuning
5. User clicks **"Crop & Use"**
6. Preview updates showing the final cropped image
7. Form can be submitted with the cropped image

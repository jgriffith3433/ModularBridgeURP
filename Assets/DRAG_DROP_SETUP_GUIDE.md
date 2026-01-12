# Bridge Drag and Drop UI System - Setup Guide

## Overview
This system allows players to drag bridge segment UI elements from an inventory panel and place them in the 3D world. The BridgePlacementController now only activates when a 3D object is instantiated from a UI drag operation.

## Architecture

### Components

1. **DraggableBridgeItem** - Attached to UI elements, handles UI drag events
2. **BridgeDragDropManager** - Singleton manager that coordinates UI drag to 3D placement
3. **BridgePlacementController** - Refactored to work with instantiated 3D objects
4. **BridgeInventoryPanel** - Manages the UI panel containing draggable bridge items

### Flow

```
Player drags UI element
    ↓
DraggableBridgeItem.OnBeginDrag()
    ↓
BridgeDragDropManager.OnBeginDragFromUI()
    ↓
Instantiate 3D BridgeSegment
    ↓
BridgePlacementController.BeginPlacement()
    ↓
Update position during drag
    ↓
OnEndDrag → CompletePlacement() or CancelPlacement()
```

## Setup Instructions

### 1. Scene Setup

#### Create the BridgeDragDropManager
1. Create an empty GameObject in your scene named "BridgeDragDropManager"
2. Add the `BridgeDragDropManager` component
3. Assign reference to your `BridgePlacementController` (or it will auto-find it)

#### Update BridgePlacementController
- Your existing BridgePlacementController will continue to work
- It no longer needs Input System setup (removed input actions)
- Now responds to calls from BridgeDragDropManager

### 2. UI Setup

#### Create Canvas (if not already present)
1. Right-click in Hierarchy → UI → Canvas
2. Set Canvas Scaler to "Scale With Screen Size" (recommended)
3. Reference Resolution: 1920x1080

#### Create Inventory Panel
1. Right-click Canvas → UI → Panel (name it "BridgeInventoryPanel")
2. Position it where you want (bottom of screen, side panel, etc.)
3. Add the `BridgeInventoryPanel` component
4. Inside the panel, create an empty GameObject as "ItemContainer"
5. Add a Horizontal Layout Group or Grid Layout Group to ItemContainer:
   - Horizontal Layout Group settings:
     - Spacing: 10
     - Child Force Expand: Width & Height unchecked
   - OR Grid Layout Group settings:
     - Cell Size: 80x80
     - Spacing: 10x10
     - Constraint: Fixed Column Count (4-6 recommended)

#### Configure BridgeInventoryPanel Component
1. Select your BridgeInventoryPanel GameObject
2. In Inspector, set:
   - **Item Container**: Drag the ItemContainer GameObject
   - **Draggable Item Prefab**: (Optional) Create custom prefab or leave empty for default
   - **Available Segments**: Set size to number of bridge types (typically 4: Start, Middle, Filler, End)

3. For each segment in Available Segments:
   - **Segment Prefab**: Drag your Bridge_Start, Bridge_End, etc. prefabs
   - **Icon**: (Optional) Assign a sprite icon
   - **Display Name**: "Bridge Start", "Bridge End", etc.
   - **Description**: (Optional) Brief description

### 3. Creating Custom Draggable Item Prefab (Optional)

If you want custom styled UI items instead of default:

1. Create a UI → Button in your Canvas (temporary)
2. Customize appearance:
   - Resize to 80x80 (or desired size)
   - Add/customize Image component
   - Add Text component for label
   - Style colors, fonts, etc.
3. Add required components:
   - `DraggableBridgeItem` component
   - `CanvasGroup` component (if not auto-added)
4. **Important**: Remove the Button component (we use drag, not click)
5. Drag to Project window to create prefab
6. Delete from Canvas
7. Assign this prefab to BridgeInventoryPanel's "Draggable Item Prefab" field

### 4. Bridge Segment Prefab Setup

Ensure your bridge segment prefabs have:
- `BridgeSegment` component configured
- Segment Type set correctly (Start, Middle, Filler, End)
- Connection points assigned
- Grid size configured
- Materials setup for valid/invalid placement

### 5. Input Requirements

The system uses Unity's Event System:
- EventSystem should exist in scene (auto-created with first Canvas)
- Ensure "Standalone Input Module" or "Input System UI Input Module" is present
- If using new Input System, ensure UI input actions are configured

## Usage

### At Runtime
1. The inventory panel will automatically populate with draggable items
2. Player clicks and drags a bridge item from the inventory
3. A 3D preview appears following the mouse
4. Preview shows green (valid) or red (invalid) placement
5. Release mouse to place or cancel

### In Code
```csharp
// Access the manager
BridgeDragDropManager.Instance.CancelDrag();

// Check if currently dragging
bool isDragging = BridgeDragDropManager.Instance.IsDragging;

// Add segment to inventory at runtime
BridgeInventoryPanel panel = FindObjectOfType<BridgeInventoryPanel>();
panel.AddSegment(new BridgeInventoryPanel.BridgeSegmentData {
    segmentPrefab = myBridgePrefab,
    icon = myIcon,
    displayName = "Custom Bridge"
});

// Refresh inventory
panel.RefreshInventory();
```

## Key Changes from Old System

### BridgePlacementController
**Removed:**
- `BeginDrag(BridgeSegment prefab)` - No longer instantiates from prefab
- Input System integration (GameInputActions)
- Mouse click handling

**Added:**
- `BeginPlacement(BridgeSegment instance, PointerEventData)` - Receives instantiated object
- `UpdatePlacement(PointerEventData)` - Updates during drag
- `CompletePlacement(PointerEventData)` - Returns bool for success/failure

**Changed:**
- Now works with `activeSegmentInstance` instead of `draggedSegmentPrefab`
- Placement is handled by already-instantiated objects
- Manager controls lifecycle, not the controller

### Workflow
**Old:** UI calls controller → Controller instantiates → Controller places
**New:** UI drag → Manager instantiates → Controller places → Manager handles result

## Troubleshooting

### Items won't drag
- Check EventSystem exists in scene
- Verify CanvasGroup on draggable items
- Ensure Canvas is set to Screen Space - Overlay or has proper camera reference
- Check that BridgeDragDropManager is in scene and Instance is not null

### 3D object doesn't spawn
- Verify bridge segment prefabs are assigned in BridgeInventoryPanel
- Check Console for errors
- Ensure BridgePlacementController reference is set in BridgeDragDropManager

### Placement doesn't work
- Verify Game.Instance.Grid exists and is initialized
- Check ground layer mask in BridgePlacementController
- Ensure your ground has correct layer assigned
- Verify camera reference in BridgePlacementController

### Preview doesn't show
- Check previewContainer in BridgePlacementController
- Ensure bridge segment materials are assigned (valid/invalid placement)
- Verify ShowPlacementPreview() method in BridgeSegment works

## Extending the System

### Adding Custom Validation
Modify `BridgePlacementController.CompletePlacement()`:
```csharp
public bool CompletePlacement(PointerEventData eventData)
{
    // ... existing code ...
    
    // Add custom validation
    if (!CustomValidation())
    {
        Debug.LogWarning("Custom validation failed");
        CancelPlacement();
        return false;
    }
    
    // ... rest of code ...
}
```

### Adding Cost System
In `BridgeDragDropManager.OnBeginDragFromUI()`:
```csharp
public void OnBeginDragFromUI(DraggableBridgeItem dragItem, PointerEventData eventData)
{
    // Check if player can afford it
    if (!GameManager.Instance.CanAfford(dragItem.BridgeSegmentPrefab))
    {
        Debug.Log("Cannot afford this bridge segment");
        return;
    }
    
    // ... rest of code ...
}
```

And in `CompletePlacement()`, deduct cost only on successful placement.

### Adding Tooltips
Extend `DraggableBridgeItem`:
```csharp
public void OnPointerEnter(PointerEventData eventData)
{
    // Show tooltip with segment info
    TooltipManager.Instance.Show(segmentInfo);
}

public void OnPointerExit(PointerEventData eventData)
{
    TooltipManager.Instance.Hide();
}
```

## Best Practices

1. **Testing**: Test with different screen resolutions and canvas scalers
2. **Performance**: Use object pooling if placing/removing many segments rapidly
3. **Feedback**: Add audio feedback for drag start, placement success/failure
4. **Visual Polish**: Add particle effects on successful placement
5. **Mobile**: Consider touch input - system already uses PointerEventData (works with touch)

## Mobile Considerations

The system uses `PointerEventData` which works with both mouse and touch:
- Single touch drag works automatically
- Consider larger UI elements for touch (100x100 instead of 80x80)
- Add haptic feedback on placement
- Test on various device sizes

## Summary

Your new drag and drop system is now set up! The key flow is:

1. UI element (`DraggableBridgeItem`) → 
2. Manager (`BridgeDragDropManager`) → 
3. 3D Placement (`BridgePlacementController`)

The BridgePlacementController is now focused solely on placing already-instantiated objects, making the architecture cleaner and more maintainable.

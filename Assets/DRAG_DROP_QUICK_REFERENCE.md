# Bridge Drag & Drop System - Quick Reference

## Component Hierarchy

```
Scene Root
├── BridgeDragDropManager (Singleton)
│   └── References BridgePlacementController
│
├── BridgePlacementController
│   └── PreviewContainer (auto-created)
│
└── Canvas
    └── BridgeInventoryPanel
        └── ItemContainer (Layout Group)
            ├── DraggableBridgeItem (Bridge Start)
            ├── DraggableBridgeItem (Bridge Middle)
            ├── DraggableBridgeItem (Bridge Filler)
            └── DraggableBridgeItem (Bridge End)
```

## Event Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│ 1. USER STARTS DRAG                                         │
│    Player clicks and drags UI bridge item                   │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. DraggableBridgeItem.OnBeginDrag()                        │
│    • Stores original position                               │
│    • Makes item semi-transparent                            │
│    • Calls Manager                                          │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. BridgeDragDropManager.OnBeginDragFromUI()                │
│    • Instantiates 3D bridge segment from prefab             │
│    • Stores reference to instantiated object                │
│    • Calls BridgePlacementController.BeginPlacement()       │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. BridgePlacementController.BeginPlacement()               │
│    • Receives the INSTANTIATED segment (not prefab)         │
│    • Moves it to preview container                          │
│    • Sets isPlacing = true                                  │
│    • Ready to track mouse position                          │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 5. DRAGGING - Update Loop                                   │
│    DraggableBridgeItem.OnDrag()                             │
│          ↓                                                   │
│    Manager.OnDragUpdate()                                   │
│          ↓                                                   │
│    Controller.UpdatePlacement()                             │
│          ↓                                                   │
│    Controller.UpdateDragPosition()                          │
│          ↓                                                   │
│    • Raycast to ground                                      │
│    • Convert to grid position                               │
│    • Update preview (green/red)                             │
│    • Show bridge preview if connecting                      │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 6. USER RELEASES DRAG                                       │
│    Player releases mouse button                             │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 7. DraggableBridgeItem.OnEndDrag()                          │
│    • Restores item appearance                               │
│    • Returns to original position                           │
│    • Calls Manager                                          │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 8. BridgeDragDropManager.OnEndDrag()                        │
│    • Calls Controller.CompletePlacement()                   │
│    • If placement fails → Destroy instantiated object       │
│    • Reset drag state                                       │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│ 9. BridgePlacementController.CompletePlacement()            │
│    • Final position check                                   │
│    • Validate grid placement                                │
│    • Call segment.TryPlace(gridPosition)                    │
│    • Return true/false for success                          │
│                                                              │
│    SUCCESS PATH:                                            │
│    • Segment placed in world                                │
│    • Registered in grid                                     │
│    • Added to bridge system                                 │
│    • Returns true                                           │
│                                                              │
│    FAILURE PATH:                                            │
│    • CancelPlacement()                                      │
│    • Returns false → Manager destroys object                │
└─────────────────────────────────────────────────────────────┘
```

## Key Differences from Old System

| Aspect | OLD System | NEW System |
|--------|-----------|------------|
| **Trigger** | Direct call to Controller | UI drag event |
| **Instantiation** | Controller instantiates | Manager instantiates |
| **Input** | Input System (GameInputActions) | PointerEventData from UI |
| **Object Type** | Receives prefab reference | Receives instantiated object |
| **Lifecycle** | Controller owns lifecycle | Manager owns lifecycle |
| **Activation** | Always listening for input | Only active during drag |

## Quick Setup Checklist

### Scene Setup
- [ ] Add BridgeDragDropManager GameObject to scene
- [ ] Assign BridgePlacementController reference
- [ ] Verify EventSystem exists (auto-created with Canvas)

### UI Setup
- [ ] Create Canvas (if needed)
- [ ] Add Panel for inventory
- [ ] Add ItemContainer with Layout Group
- [ ] Add BridgeInventoryPanel component
- [ ] Configure Available Segments array

### Prefab Setup
- [ ] Ensure bridge segment prefabs have BridgeSegment component
- [ ] Set correct SegmentType
- [ ] Assign connection points
- [ ] Configure materials for placement preview

### Testing
- [ ] Play scene
- [ ] Verify inventory items appear
- [ ] Test dragging from inventory
- [ ] Test 3D preview appears
- [ ] Test placement (valid/invalid)
- [ ] Test cancellation (drag off-screen)

## Common Scenarios

### Scenario 1: Simple Placement
1. Drag "Bridge Start" from UI
2. Move over valid ground location (green preview)
3. Release → Bridge placed successfully

### Scenario 2: Connection Preview
1. Place "Bridge Start" at position A
2. Drag "Bridge End" from UI
3. Hover on same axis as start → Full bridge preview shows
4. Release → Complete bridge built

### Scenario 3: Invalid Placement
1. Drag bridge segment from UI
2. Move over occupied grid cell (red preview)
3. Release → Placement cancelled, object destroyed

### Scenario 4: Manual Cancel
1. Drag bridge segment from UI
2. Press ESC or drag off-screen
3. BridgeDragDropManager.CancelDrag() called
4. Object destroyed, state reset

## File Locations

```
Assets/
├── Scripts/
│   ├── Bridge/
│   │   ├── BridgePlacementController.cs  ← REFACTORED
│   │   ├── BridgeSegment.cs
│   │   ├── Bridge.cs
│   │   └── BridgeBuilder.cs
│   │
│   └── Input/
│       ├── DraggableBridgeItem.cs        ← NEW
│       ├── BridgeDragDropManager.cs      ← NEW
│       └── BridgeInventoryPanel.cs       ← NEW
│
└── DRAG_DROP_SETUP_GUIDE.md              ← FULL GUIDE
```

## API Reference

### DraggableBridgeItem
```csharp
[SerializeField] private BridgeSegment bridgeSegmentPrefab;  // The prefab to spawn
[SerializeField] private Sprite dragIcon;                     // Optional icon
[SerializeField] private float dragAlpha = 0.6f;             // Transparency while dragging

public BridgeSegment BridgeSegmentPrefab { get; }            // Get assigned prefab
```

### BridgeDragDropManager
```csharp
public static BridgeDragDropManager Instance { get; }        // Singleton access

public void OnBeginDragFromUI(DraggableBridgeItem, PointerEventData)  // Start drag
public void OnDragUpdate(PointerEventData)                             // Update position
public void OnEndDrag(PointerEventData)                                // Finish drag
public void CancelDrag()                                               // Cancel operation
public bool IsDragging { get; }                                        // Check if dragging
```

### BridgePlacementController
```csharp
public void BeginPlacement(BridgeSegment instance, PointerEventData)  // Start placing
public void UpdatePlacement(PointerEventData)                         // Update position
public bool CompletePlacement(PointerEventData)                       // Try to place (returns success)
public void CancelPlacement()                                         // Cancel and destroy
```

### BridgeInventoryPanel
```csharp
[SerializeField] private List<BridgeSegmentData> availableSegments;   // Segments to show

public void AddSegment(BridgeSegmentData)                             // Add segment at runtime
public void RefreshInventory()                                        // Rebuild UI
```

## Tips & Tricks

### Performance
- Preview segments are destroyed/recreated on position change
- Consider object pooling for very frequent placement
- Use LayerMask efficiently for raycasting

### Visual Feedback
- Green = Valid placement
- Red = Invalid placement
- Semi-transparent UI item = Currently dragging
- Full bridge preview = Connection detected

### Mobile Support
- System uses PointerEventData (works with touch)
- Increase UI button size for touch (100x100 recommended)
- Test on various screen sizes
- Consider adding touch hold delay before drag

### Debugging
- Check Console for detailed logs
- Each component logs its actions
- Use Unity Profiler to check for performance issues
- Verify references in Inspector (no null refs)

## Next Steps

1. **Test the system** in Unity Editor
2. **Customize UI appearance** with your art style
3. **Add sound effects** for drag/place/cancel
4. **Implement additional features**:
   - Cost system
   - Unlock system
   - Multiple pages/categories
   - Drag-to-delete functionality
5. **Build for target platform** and test thoroughly

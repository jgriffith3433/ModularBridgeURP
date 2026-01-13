using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Handles runtime placement of bridge segments with live preview and grid snapping.
/// Now works with the drag and drop UI system - only activates when a 3D object
/// is instantiated from a UI drag operation.
/// </summary>
public class BridgePlacementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private GameSettings gameSettings;
    
    [Header("Preview Settings")]
    [SerializeField] private Transform previewContainer;
    
    // Current placement state - now receives the instantiated object
    private BridgeSegment activeSegmentInstance;
    private bool isPlacing = false;
    
    // Move mode state - for dragging already-placed segments
    private bool isDraggingExisting = false;
    private Vector3Int originalGridPosition;
    private Bridge originalBridge;
    
    // Preview bridge segments
    private List<GameObject> previewSegments = new List<GameObject>();
    private BridgeSegment potentialConnectionTarget;
    
    // Grid position tracking
    private Vector3Int currentGridPosition;
    private Vector3Int lastGridPosition;
    
    // Input - mouse position from pointer events
    private Vector2 mousePosition;
    
    public bool IsPlacing => isPlacing;
    
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (gridSystem == null)
            throw new System.Exception("[BridgePlacementController] GridSystem not assigned!");
        
        if (gameSettings == null)
            throw new System.Exception("[BridgePlacementController] GameSettings not assigned!");
        
        if (previewContainer == null)
            throw new System.Exception("[BridgePlacementController] PreviewContainer not assigned!");
    }
    
    private void Update()
    {
        if (!isPlacing || activeSegmentInstance == null)
            return;
        
        UpdateDragPosition();
    }
    
    #region Public API - Called by BridgeDragDropManager
    
    /// <summary>
    /// Begin placement with an already instantiated bridge segment from UI drag.
    /// </summary>
    public void BeginPlacement(BridgeSegment instantiatedSegment, PointerEventData eventData)
    {
        if (isPlacing)
        {
            CancelPlacement();
        }
        
        activeSegmentInstance = instantiatedSegment;
        isPlacing = true;
        mousePosition = eventData.position;
        
        // Move segment to preview container
        activeSegmentInstance.transform.SetParent(previewContainer);
        
        // Force first update
        lastGridPosition = Vector3Int.one * int.MinValue;
        
        Debug.Log($"[BridgePlacementController] Started placement of {activeSegmentInstance.Type}");
    }
    
    /// <summary>
    /// Begin moving an already-placed bridge segment.
    /// </summary>
    public void BeginMove(BridgeSegment existingSegment, PointerEventData eventData)
    {
        if (isPlacing)
        {
            CancelPlacement();
        }
        
        if (!existingSegment.IsPlaced)
        {
            Debug.LogWarning("[BridgePlacementController] Cannot move segment that isn't placed");
            return;
        }
        
        // Store original state
        originalGridPosition = existingSegment.GridPosition;
        originalBridge = existingSegment.ParentBridge;
        
        // If part of a bridge, break it
        if (originalBridge != null)
        {
            Debug.Log($"[BridgePlacementController] Breaking bridge to move segment");
            Game.Instance.Bridges.BreakBridge(originalBridge, keepStartEnd: true);
        }
        
        // Remove from grid (this fires OnRemoved)
        existingSegment.Remove();
        
        // Set up move state
        activeSegmentInstance = existingSegment;
        isPlacing = true;
        isDraggingExisting = true;
        mousePosition = eventData.position;
        
        // Move segment to preview container
        activeSegmentInstance.transform.SetParent(previewContainer);
        
        // Force first update
        lastGridPosition = Vector3Int.one * int.MinValue;
        
        Debug.Log($"[BridgePlacementController] Started moving {activeSegmentInstance.Type} segment from {originalGridPosition}");
    }
    
    /// <summary>
    /// Update placement position during drag.
    /// </summary>
    public void UpdatePlacement(PointerEventData eventData)
    {
        if (!isPlacing)
            return;
        
        mousePosition = eventData.position;
    }
    
    /// <summary>
    /// Try to complete the placement. Returns true if successful.
    /// </summary>
    public bool CompletePlacement(PointerEventData eventData)
    {
        if (!isPlacing || activeSegmentInstance == null)
            return false;
        
        mousePosition = eventData.position;
        UpdateDragPosition(); // Final position update
        
        // Check if we can place
        bool canPlace = gridSystem.CanPlaceObject(currentGridPosition, activeSegmentInstance);
        
        if (!canPlace)
        {
            // For UI drag: destroy the segment
            // For move drag: handled by CompleteMove which will snap back
            if (!isDraggingExisting)
            {
                Debug.LogWarning("[BridgePlacementController] Cannot place - grid cells occupied");
                CancelPlacement();
            }
            return false;
        }
        
        // Move out of preview container
        activeSegmentInstance.transform.SetParent(null);
        
        // Try to place the segment
        if (activeSegmentInstance.TryPlace(currentGridPosition))
        {
            Debug.Log($"[BridgePlacementController] Placed {activeSegmentInstance.Type} segment at {currentGridPosition}");
            
            // Clear preview and reset state (but don't destroy the placed segment)
            ClearPreviewSegments();
            potentialConnectionTarget = null;
            activeSegmentInstance = null;
            isPlacing = false;
            isDraggingExisting = false;
            
            return true;
        }
        else
        {
            Debug.LogWarning("[BridgePlacementController] Placement failed");
            if (!isDraggingExisting)
            {
                CancelPlacement();
            }
            return false;
        }
    }
    
    /// <summary>
    /// Cancel current placement and destroy the active segment.
    /// </summary>
    public void CancelPlacement()
    {
        if (!isPlacing)
            return;
        
        isPlacing = false;
        
        if (activeSegmentInstance != null)
        {
            Destroy(activeSegmentInstance.gameObject);
            activeSegmentInstance = null;
        }
        
        ClearPreviewSegments();
        potentialConnectionTarget = null;
        
        Debug.Log("[BridgePlacementController] Placement cancelled");
    }
    
    /// <summary>
    /// Complete move operation. If invalid position, snap back to original.
    /// </summary>
    public bool CompleteMove(PointerEventData eventData)
    {
        if (!isPlacing || !isDraggingExisting || activeSegmentInstance == null)
            return false;
        
        mousePosition = eventData.position;
        UpdateDragPosition(); // Final position update
        
        // Check if we can place at the new position
        bool canPlace = gridSystem.CanPlaceObject(currentGridPosition, activeSegmentInstance);
        
        if (canPlace && currentGridPosition != originalGridPosition)
        {
            // Valid new position - place there
            activeSegmentInstance.transform.SetParent(null);
            
            if (activeSegmentInstance.TryPlace(currentGridPosition))
            {
                Debug.Log($"[BridgePlacementController] Moved {activeSegmentInstance.Type} segment to {currentGridPosition}");
                
                // Clear state
                ClearPreviewSegments();
                potentialConnectionTarget = null;
                activeSegmentInstance = null;
                isPlacing = false;
                isDraggingExisting = false;
                originalBridge = null;
                
                return true;
            }
        }
        
        // Invalid position or placement failed - snap back to original position
        Debug.LogWarning($"[BridgePlacementController] Invalid position, snapping back to {originalGridPosition}");
        
        activeSegmentInstance.transform.SetParent(null);
        
        if (activeSegmentInstance.TryPlace(originalGridPosition))
        {
            Debug.Log($"[BridgePlacementController] Snapped {activeSegmentInstance.Type} segment back to original position");
        }
        
        // Clear state
        ClearPreviewSegments();
        potentialConnectionTarget = null;
        activeSegmentInstance = null;
        isPlacing = false;
        isDraggingExisting = false;
        originalBridge = null;
        
        return false;
    }
    
    /// <summary>
    /// Cancel move operation and snap back to original position.
    /// </summary>
    public void CancelMove()
    {
        if (!isPlacing || !isDraggingExisting)
            return;
        
        Debug.Log($"[BridgePlacementController] Cancelling move, snapping back to {originalGridPosition}");
        
        if (activeSegmentInstance != null)
        {
            activeSegmentInstance.transform.SetParent(null);
            activeSegmentInstance.TryPlace(originalGridPosition);
        }
        
        ClearPreviewSegments();
        potentialConnectionTarget = null;
        activeSegmentInstance = null;
        isPlacing = false;
        isDraggingExisting = false;
        originalBridge = null;
        
        Debug.Log("[BridgePlacementController] Move cancelled");
    }
    
    #endregion
    
    #region Placement Logic
    
    private void UpdateDragPosition()
    {
        // Raycast from mouse to ground
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            return;
        
        // Convert to grid position
        Vector3Int newGridPos = gridSystem.WorldToGrid(hit.point);
        
        // Only update if position changed (performance optimization)
        if (newGridPos == lastGridPosition)
            return;
        
        currentGridPosition = newGridPos;
        lastGridPosition = newGridPos;
        
        UpdatePreview();
    }
    
    private void UpdatePreview()
    {
        // Check if we can place at this position
        bool canPlace = gridSystem.CanPlaceObject(currentGridPosition, activeSegmentInstance);
        
        // Update dragged segment position and material
        activeSegmentInstance.ShowPlacementPreview(currentGridPosition, canPlace);
        
        // Clear old preview segments
        ClearPreviewSegments();
        
        // Check for potential connections
        potentialConnectionTarget = FindPotentialConnection();
        
        if (potentialConnectionTarget != null && canPlace)
        {
            ShowBridgePreview();
        }
    }
    
    private BridgeSegment FindPotentialConnection()
    {
        // Only Start and End pieces can initiate connections
        if (activeSegmentInstance.Type != BridgeSegment.SegmentType.Start &&
            activeSegmentInstance.Type != BridgeSegment.SegmentType.End)
        {
            return null;
        }
        
        // Determine what we're looking for
        BridgeSegment.SegmentType lookingFor = BridgeSegment.SegmentType.End;
        if (activeSegmentInstance.Type == BridgeSegment.SegmentType.End)
        {
            lookingFor = BridgeSegment.SegmentType.Start;
        }
        
        // Get all placed bridge segments of the target type
        var candidates = gridSystem.Registry.GetObjectsOfType<BridgeSegment>();
        
        foreach (var candidate in candidates)
        {
            if (candidate.Type != lookingFor)
                continue;
            
            if (!candidate.IsPlaced)
                continue;
            
            Vector3Int candidatePos = candidate.GridPosition;
            
            // Check if on same axis (X or Z)
            bool sameX = candidatePos.x == currentGridPosition.x;
            bool sameZ = candidatePos.z == currentGridPosition.z;
            
            if (sameX || sameZ)
            {
                return candidate;
            }
        }
        
        return null;
    }
    
    private void ShowBridgePreview()
    {
        // Determine start and end positions
        Vector3Int startPos = activeSegmentInstance.Type == BridgeSegment.SegmentType.Start
            ? currentGridPosition
            : potentialConnectionTarget.GridPosition;
        
        Vector3Int endPos = activeSegmentInstance.Type == BridgeSegment.SegmentType.End
            ? currentGridPosition
            : potentialConnectionTarget.GridPosition;
        
        // Calculate bridge plan
        BridgeBuilder.BridgePlan plan = BridgeBuilder.CalculateBridge(startPos, endPos, gameSettings);
        
        if (!plan.IsValid)
            return;
        
        // Create preview segments (skip start and end as they're already visible)
        BridgeSettings settings = gameSettings.BridgeSettings;
        
        for (int i = 1; i < plan.Placements.Count - 1; i++)
        {
            var placement = plan.Placements[i];
            
            BridgeSegment prefab = settings.GetPrefabForType(placement.Type);
            if (prefab == null)
                continue;
            
            GameObject previewObj = Instantiate(prefab.gameObject, previewContainer);
            previewObj.transform.position = gridSystem.GridToWorld(placement.GridPosition);
            previewObj.transform.rotation = placement.Rotation;
            
            // Apply preview material
            BridgeSegment previewSegment = previewObj.GetComponent<BridgeSegment>();
            if (previewSegment != null)
            {
                previewSegment.ShowPlacementPreview(placement.GridPosition, true);
            }
            
            previewSegments.Add(previewObj);
        }
    }
    
    private void ClearPreviewSegments()
    {
        foreach (var obj in previewSegments)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        previewSegments.Clear();
    }
    
    #endregion
}
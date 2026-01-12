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
    
    [Header("Preview Settings")]
    [SerializeField] private Transform previewContainer;
    
    // Current placement state - now receives the instantiated object
    private BridgeSegment activeSegmentInstance;
    private bool isPlacing = false;
    
    // Preview bridge segments
    private List<GameObject> previewSegments = new List<GameObject>();
    private BridgeSegment potentialConnectionTarget;
    
    // Grid position tracking
    private Vector3Int currentGridPosition;
    private Vector3Int lastGridPosition;
    
    // Input - mouse position from pointer events
    private Vector2 mousePosition;
    
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (previewContainer == null)
        {
            GameObject container = new GameObject("BridgePreviews");
            previewContainer = container.transform;
            previewContainer.SetParent(transform);
        }
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
        bool canPlace = Game.Instance.Grid.CanPlaceObject(currentGridPosition, activeSegmentInstance.GridSize);
        
        if (!canPlace)
        {
            Debug.LogWarning("[BridgePlacementController] Cannot place - grid cells occupied");
            CancelPlacement();
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
            
            return true;
        }
        else
        {
            Debug.LogWarning("[BridgePlacementController] Placement failed");
            CancelPlacement();
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
    
    #endregion
    
    #region Placement Logic
    
    private void UpdateDragPosition()
    {
        // Raycast from mouse to ground
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            return;
        
        // Convert to grid position
        Vector3Int newGridPos = Game.Instance.Grid.WorldToGrid(hit.point);
        
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
        bool canPlace = Game.Instance.Grid.CanPlaceObject(currentGridPosition, activeSegmentInstance.GridSize);
        
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
        var candidates = Game.Instance.Grid.Registry.GetObjectsOfType<BridgeSegment>();
        
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
        BridgeBuilder.BridgePlan plan = BridgeBuilder.CalculateBridge(startPos, endPos);
        
        if (!plan.IsValid)
            return;
        
        // Create preview segments (skip start and end as they're already visible)
        BridgeSettings settings = Game.Instance.Settings.BridgeSettings;
        
        for (int i = 1; i < plan.Placements.Count - 1; i++)
        {
            var placement = plan.Placements[i];
            
            BridgeSegment prefab = GetPrefabForType(placement.Type);
            if (prefab == null)
                continue;
            
            GameObject previewObj = Instantiate(prefab.gameObject, previewContainer);
            previewObj.transform.position = Game.Instance.Grid.GridToWorld(placement.GridPosition);
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
    
    private BridgeSegment GetPrefabForType(BridgeSegment.SegmentType type)
    {
        BridgeSettings settings = Game.Instance.Settings.BridgeSettings;
        
        switch (type)
        {
            case BridgeSegment.SegmentType.Start:
                return settings.StartPrefab;
            case BridgeSegment.SegmentType.Middle:
                return settings.MiddlePrefab;
            case BridgeSegment.SegmentType.Filler:
                return settings.FillerPrefab;
            case BridgeSegment.SegmentType.End:
                return settings.EndPrefab;
            default:
                return null;
        }
    }
    
    #endregion
}
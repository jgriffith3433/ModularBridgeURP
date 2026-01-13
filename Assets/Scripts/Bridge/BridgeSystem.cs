using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all bridges in the game: creation, tracking, and lifecycle.
/// </summary>
public class BridgeSystem : MonoBehaviour
{
    [Header("Bridge Container")]
    [SerializeField] private Transform bridgeContainer;
    
    [Header("Dependencies")]
    [SerializeField] private GameSettings gameSettings;
    
    // Active bridges
    private List<Bridge> activeBridges = new List<Bridge>();
    
    // Standalone segments (not part of a bridge yet)
    private HashSet<BridgeSegment> standaloneSegments = new HashSet<BridgeSegment>();
    
    private void Awake()
    {
        if (gameSettings == null)
            throw new System.Exception("[BridgeSystem] GameSettings not assigned!");
        
        if (bridgeContainer == null)
            throw new System.Exception("[BridgeSystem] BridgeContainer not assigned!");
    }
    
    /// <summary>
    /// Called when a bridge segment is placed on the grid.
    /// </summary>
    public void OnSegmentPlaced(BridgeSegment segment)
    {
        // If it's a start or end piece, check for potential connections
        if (segment.Type == BridgeSegment.SegmentType.Start ||
            segment.Type == BridgeSegment.SegmentType.End)
        {
            standaloneSegments.Add(segment);
            TryAutoConnect(segment);
        }
    }
    
    /// <summary>
    /// Called when a bridge segment is removed from the grid.
    /// </summary>
    public void OnSegmentRemoved(BridgeSegment segment)
    {
        standaloneSegments.Remove(segment);
        
        // If this segment was part of a bridge, destroy the entire bridge
        if (segment.ParentBridge != null)
        {
            DestroyBridge(segment.ParentBridge);
        }
    }
    
    /// <summary>
    /// Try to automatically connect a newly placed segment to a compatible segment.
    /// </summary>
    private void TryAutoConnect(BridgeSegment newSegment)
    {
        // Find a compatible segment on the same axis
        BridgeSegment targetSegment = FindConnectableSegment(newSegment);
        
        if (targetSegment != null)
        {
            CreateBridge(newSegment, targetSegment);
        }
    }
    
    /// <summary>
    /// Find a segment that can connect to the given segment.
    /// </summary>
    private BridgeSegment FindConnectableSegment(BridgeSegment segment)
    {
        Vector3Int segmentPos = segment.GridPosition;
        BridgeSegment.SegmentType lookingFor = BridgeSegment.SegmentType.End;
        
        if (segment.Type == BridgeSegment.SegmentType.End)
        {
            lookingFor = BridgeSegment.SegmentType.Start;
        }
        else if (segment.Type != BridgeSegment.SegmentType.Start)
        {
            return null; // Only Start and End can initiate connections
        }
        
        // Search through standalone segments
        foreach (var candidate in standaloneSegments)
        {
            if (candidate == segment)
                continue;
            
            if (candidate.Type != lookingFor)
                continue;
            
            Vector3Int candidatePos = candidate.GridPosition;
            
            // Check if on same axis
            bool sameX = candidatePos.x == segmentPos.x;
            bool sameZ = candidatePos.z == segmentPos.z;
            
            if (sameX || sameZ)
            {
                return candidate;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Create a complete bridge between two segments.
    /// </summary>
    public Bridge CreateBridge(BridgeSegment start, BridgeSegment end)
    {
        // Ensure proper start/end order
        if (start.Type == BridgeSegment.SegmentType.End)
        {
            var temp = start;
            start = end;
            end = temp;
        }
        
        // Calculate bridge plan
        BridgeBuilder.BridgePlan plan = BridgeBuilder.CalculateBridge(
            start.GridPosition,
            end.GridPosition,
            gameSettings
        );
        
        if (!plan.IsValid)
        {
            Debug.LogWarning("[BridgeSystem] Cannot create bridge - invalid plan");
            return null;
        }
        
        // Create bridge object
        Bridge bridge = new Bridge(start, end);
        
        // Instantiate intermediate segments
        BridgeSettings settings = gameSettings.BridgeSettings;
        
        for (int i = 1; i < plan.Placements.Count - 1; i++) // Skip first (start) and last (end)
        {
            var placement = plan.Placements[i];
            
            BridgeSegment prefab = settings.GetPrefabForType(placement.Type);
            if (prefab == null)
                continue;
            
            BridgeSegment segment = Instantiate(prefab, bridgeContainer);
            segment.transform.rotation = placement.Rotation;
            
            if (segment.TryPlace(placement.GridPosition))
            {
                if (placement.Type == BridgeSegment.SegmentType.Middle)
                {
                    bridge.AddMiddleSegment(segment);
                }
                else if (placement.Type == BridgeSegment.SegmentType.Filler)
                {
                    bridge.AddFillerSegment(segment);
                }
            }
            else
            {
                // Placement failed - cleanup and abort
                Destroy(segment.gameObject);
                bridge.Destroy();
                return null;
            }
        }
        
        // Remove start/end from standalone list
        standaloneSegments.Remove(start);
        standaloneSegments.Remove(end);
        
        // Add to active bridges
        activeBridges.Add(bridge);
        
        Debug.Log($"[BridgeSystem] Created bridge with {bridge.MiddleSegments.Count} middle and {bridge.FillerSegments.Count} filler segments");
        
        return bridge;
    }
    
    /// <summary>
    /// Break a bridge apart, destroying intermediate segments.
    /// </summary>
    /// <param name="bridge">The bridge to break</param>
    /// <param name="keepStartEnd">If true, Start/End segments are kept as standalone. If false, all segments are destroyed.</param>
    public void BreakBridge(Bridge bridge, bool keepStartEnd = true)
    {
        if (!activeBridges.Contains(bridge))
            return;
        
        activeBridges.Remove(bridge);
        
        // Handle Start/End segments
        if (keepStartEnd)
        {
            // Return start/end to standalone if they still exist
            if (bridge.StartSegment != null)
            {
                standaloneSegments.Add(bridge.StartSegment);
                bridge.StartSegment.ParentBridge = null;
            }
            
            if (bridge.EndSegment != null)
            {
                standaloneSegments.Add(bridge.EndSegment);
                bridge.EndSegment.ParentBridge = null;
            }
        }
        else
        {
            // Destroy start/end segments as well
            if (bridge.StartSegment != null)
            {
                bridge.StartSegment.Remove();
                Destroy(bridge.StartSegment.gameObject);
            }
            
            if (bridge.EndSegment != null)
            {
                bridge.EndSegment.Remove();
                Destroy(bridge.EndSegment.gameObject);
            }
        }
        
        // Destroy intermediate segments
        foreach (var segment in bridge.MiddleSegments)
        {
            if (segment != null)
            {
                segment.Remove();
                Destroy(segment.gameObject);
            }
        }
        
        foreach (var segment in bridge.FillerSegments)
        {
            if (segment != null)
            {
                segment.Remove();
                Destroy(segment.gameObject);
            }
        }
        
        Debug.Log($"[BridgeSystem] Broke bridge (keepStartEnd: {keepStartEnd})");
    }
    
    /// <summary>
    /// Get the bridge that contains the specified segment.
    /// </summary>
    public Bridge GetBridgeForSegment(BridgeSegment segment)
    {
        return segment?.ParentBridge;
    }
    
    /// <summary>
    /// Destroy a bridge and all its segments.
    /// </summary>
    public void DestroyBridge(Bridge bridge)
    {
        // Use BreakBridge with keepStartEnd = true, so Start/End become standalone
        BreakBridge(bridge, keepStartEnd: true);
    }
    
    public IReadOnlyList<Bridge> GetActiveBridges()
    {
        return activeBridges.AsReadOnly();
    }
}
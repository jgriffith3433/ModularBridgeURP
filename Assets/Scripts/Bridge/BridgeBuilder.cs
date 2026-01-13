using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calculates how to build a bridge between two points using available segments.
/// </summary>
public static class BridgeBuilder
{
    public struct BridgePlan
    {
        public Vector3Int StartPosition;
        public Vector3Int EndPosition;
        public Vector3 Direction;
        public List<SegmentPlacement> Placements;
        public bool IsValid;
    }
    
    public struct SegmentPlacement
    {
        public BridgeSegment.SegmentType Type;
        public Vector3Int GridPosition;
        public Quaternion Rotation;
    }
    
    /// <summary>
    /// Calculate a bridge plan between start and end positions.
    /// </summary>
    public static BridgePlan CalculateBridge(Vector3Int startPos, Vector3Int endPos, GameSettings gameSettings)
    {
        BridgePlan plan = new BridgePlan
        {
            StartPosition = startPos,
            EndPosition = endPos,
            Placements = new List<SegmentPlacement>(),
            IsValid = false
        };
        
        // Calculate direction and distance
        Vector3Int delta = endPos - startPos;
        
        // Must be along one axis only (straight line)
        if (!IsStraightLine(delta))
        {
            Debug.LogWarning("[BridgeBuilder] Bridge must be built in a straight line");
            return plan;
        }
        
        // Get settings
        BridgeSettings settings = gameSettings.BridgeSettings;
        int middleLength = settings.GetSegmentLength(BridgeSegment.SegmentType.Middle);
        int fillerLength = settings.GetSegmentLength(BridgeSegment.SegmentType.Filler);
        
        // Calculate total distance
        int totalDistance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y) + Mathf.Abs(delta.z);
        
        // Get actual segment lengths
        int startLength = settings.GetSegmentLength(BridgeSegment.SegmentType.Start);
        int endLength = settings.GetSegmentLength(BridgeSegment.SegmentType.End);
        
        Debug.Log($"[BridgeBuilder] Building bridge: distance={totalDistance}, start={startLength}, middle={middleLength}, filler={fillerLength}, end={endLength}");
        
        // Calculate direction vector
        Vector3 direction = new Vector3(
            delta.x != 0 ? Mathf.Sign(delta.x) : 0,
            delta.y != 0 ? Mathf.Sign(delta.y) : 0,
            delta.z != 0 ? Mathf.Sign(delta.z) : 0
        );
        plan.Direction = direction;
        
        // Calculate rotation - bridge models face along X-axis, so rotate 90 degrees
        Quaternion baseRotation = Quaternion.LookRotation(direction);
        Quaternion correctionRotation = Quaternion.Euler(0, -90, 0);
        Quaternion finalRotation = baseRotation * correctionRotation;
        
        // Start segment
        plan.Placements.Add(new SegmentPlacement
        {
            Type = BridgeSegment.SegmentType.Start,
            GridPosition = startPos,
            Rotation = finalRotation
        });
        
        // Get spacing intervals (accounts for centered bounds)
        int startSpacing = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Start, direction);
        int middleSpacing = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Middle, direction);
        int fillerSpacing = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Filler, direction);
        int endSpacing = settings.GetSegmentSpacing(BridgeSegment.SegmentType.End, direction);
        
        int currentDistance = startSpacing; // Start after the Start segment's extent
        int remainingDistance = totalDistance - startSpacing; // Space left after start segment
        
        // Fill with middle segments
        while (remainingDistance >= middleSpacing + endSpacing) // Need room for middle + end spacing
        {
            Vector3Int pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
            
            plan.Placements.Add(new SegmentPlacement
            {
                Type = BridgeSegment.SegmentType.Middle,
                GridPosition = pos,
                Rotation = finalRotation
            });
            
            currentDistance += middleSpacing;
            remainingDistance -= middleSpacing;
        }
        
        // Fill remaining gap with fillers
        while (remainingDistance >= fillerSpacing + endSpacing) // Need room for filler + end spacing
        {
            Vector3Int pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
            
            plan.Placements.Add(new SegmentPlacement
            {
                Type = BridgeSegment.SegmentType.Filler,
                GridPosition = pos,
                Rotation = finalRotation
            });
            
            currentDistance += fillerSpacing;
            remainingDistance -= fillerSpacing;
        }
        
        // End segment - place at exact end position
        plan.Placements.Add(new SegmentPlacement
        {
            Type = BridgeSegment.SegmentType.End,
            GridPosition = endPos,
            Rotation = finalRotation
        });
        
        // Warn if there's a gap
        int gapSize = remainingDistance - endLength;
        if (gapSize > 0)
        {
            Debug.LogWarning($"[BridgeBuilder] Gap of {gapSize} cells detected. Consider adjusting segment sizes.");
        }
        
        plan.IsValid = true;
        return plan;
    }
    
    private static bool IsStraightLine(Vector3Int delta)
    {
        int nonZeroAxes = 0;
        if (delta.x != 0) nonZeroAxes++;
        if (delta.y != 0) nonZeroAxes++;
        if (delta.z != 0) nonZeroAxes++;
        
        return nonZeroAxes == 1;
    }
}
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
    public static BridgePlan CalculateBridge(Vector3Int startPos, Vector3Int endPos)
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
        BridgeSettings settings = Game.Instance.Settings.BridgeSettings;
        int middleLength = settings.MiddleSegmentLength;
        int fillerLength = settings.FillerSegmentLength;
        
        // Calculate total distance
        int totalDistance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y) + Mathf.Abs(delta.z);
        
        // Calculate direction vector
        Vector3 direction = new Vector3(
            delta.x != 0 ? Mathf.Sign(delta.x) : 0,
            delta.y != 0 ? Mathf.Sign(delta.y) : 0,
            delta.z != 0 ? Mathf.Sign(delta.z) : 0
        );
        plan.Direction = direction;
        
        // Start segment (length 1)
        plan.Placements.Add(new SegmentPlacement
        {
            Type = BridgeSegment.SegmentType.Start,
            GridPosition = startPos,
            Rotation = Quaternion.LookRotation(direction)
        });
        
        int currentDistance = 1; // Start segment takes 1 unit
        
        // Fill with middle segments
        while (currentDistance + middleLength + 1 <= totalDistance) // +1 for end segment
        {
            Vector3Int pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
            
            plan.Placements.Add(new SegmentPlacement
            {
                Type = BridgeSegment.SegmentType.Middle,
                GridPosition = pos,
                Rotation = Quaternion.LookRotation(direction)
            });
            
            currentDistance += middleLength;
        }
        
        // Fill remaining gap with fillers
        while (currentDistance + fillerLength + 1 <= totalDistance)
        {
            Vector3Int pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
            
            plan.Placements.Add(new SegmentPlacement
            {
                Type = BridgeSegment.SegmentType.Filler,
                GridPosition = pos,
                Rotation = Quaternion.LookRotation(direction)
            });
            
            currentDistance += fillerLength;
        }
        
        // End segment
        plan.Placements.Add(new SegmentPlacement
        {
            Type = BridgeSegment.SegmentType.End,
            GridPosition = endPos,
            Rotation = Quaternion.LookRotation(direction)
        });
        
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
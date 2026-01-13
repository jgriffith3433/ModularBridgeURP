using System.Collections.Generic;
using UnityEngine;
using ModularBridge.Core;

namespace ModularBridge.Bridge
{
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
            
            Vector3Int delta = endPos - startPos;
            if (!IsStraightLine(delta))
                return plan;
            
            BridgeSettings settings = gameSettings.BridgeSettings;
            
            // Direction vector
            Vector3 direction = new Vector3(
                delta.x != 0 ? Mathf.Sign(delta.x) : 0,
                delta.y != 0 ? Mathf.Sign(delta.y) : 0,
                delta.z != 0 ? Mathf.Sign(delta.z) : 0
            );
            plan.Direction = direction;
            
            // Rotation
            Quaternion baseRotation = Quaternion.LookRotation(direction);
            Quaternion correctionRotation = Quaternion.Euler(0, -90, 0);
            Quaternion finalRotation = baseRotation * correctionRotation;
            
            // Get segment widths
            int startWidth = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Start, direction);
            int middleWidth = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Middle, direction);
            int fillerWidth = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Filler, direction);
            int endWidth = settings.GetSegmentSpacing(BridgeSegment.SegmentType.End, direction);
            
            // Total distance between centers
            int totalDistance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y) + Mathf.Abs(delta.z);
            
            // Calculate gap: distance from cell after Start's extent to cell before End's extent
            // Start at position S extends to S+3 in direction, next cell is S+4
            // End at position E extends back to E-3 from direction, previous cell is E-4
            // Gap = cells from (S+4) to (E-4) inclusive
            int gapCells = totalDistance - (startWidth / 2) - (endWidth / 2) - 1;
            
            // Fill gap with Middle and Filler segments
            int middleCount = gapCells / middleWidth;
            int remainingCells = gapCells - (middleCount * middleWidth);
            int fillerCount = remainingCells;
            
            // Distribute fillers symmetrically
            int fillersBeforeMiddles = fillerCount / 2;
            int fillersAfterMiddles = fillerCount - fillersBeforeMiddles;
            
            // Place Start
            plan.Placements.Add(new SegmentPlacement
            {
                Type = BridgeSegment.SegmentType.Start,
                GridPosition = startPos,
                Rotation = finalRotation
            });
            
            // Track current position along bridge (distance from Start center)
            int currentDistance = startWidth / 2 + 1; // Start from edge of Start segment
            
            // Place fillers before middles
            for (int i = 0; i < fillersBeforeMiddles; i++)
            {
                Vector3Int pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
                plan.Placements.Add(new SegmentPlacement
                {
                    Type = BridgeSegment.SegmentType.Filler,
                    GridPosition = pos,
                    Rotation = finalRotation
                });
                currentDistance += fillerWidth;
            }
            
            // Adjust position for middle segment center (middle extends back from its center)
            if (fillersBeforeMiddles > 0 && middleCount > 0)
            {
                currentDistance += (middleWidth / 2);
            }
            else if (middleCount > 0)
            {
                currentDistance = startWidth / 2 + 1 + middleWidth / 2;
            }
            
            // Place middle segments
            for (int i = 0; i < middleCount; i++)
            {
                Vector3Int pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
                plan.Placements.Add(new SegmentPlacement
                {
                    Type = BridgeSegment.SegmentType.Middle,
                    GridPosition = pos,
                    Rotation = finalRotation
                });
                currentDistance += middleWidth;
            }
            
            // Adjust position for fillers after middles
            // Last middle at position M extends to M+3, so first filler should be at M+4
            // currentDistance is now at M+7 (ready for next middle), so adjust back
            if (middleCount > 0 && fillersAfterMiddles > 0)
            {
                currentDistance = currentDistance - middleWidth + (middleWidth / 2) + 1;
            }
            
            // Place fillers after middles
            for (int i = 0; i < fillersAfterMiddles; i++)
            {
                Vector3Int pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
                plan.Placements.Add(new SegmentPlacement
                {
                    Type = BridgeSegment.SegmentType.Filler,
                    GridPosition = pos,
                    Rotation = finalRotation
                });
                currentDistance += fillerWidth;
            }
            
            // Place End
            plan.Placements.Add(new SegmentPlacement
            {
                Type = BridgeSegment.SegmentType.End,
                GridPosition = endPos,
                Rotation = finalRotation
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
}
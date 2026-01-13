using System.Collections.Generic;
using UnityEngine;
using ModularBridge.Core;

namespace ModularBridge.Bridge
{
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
        
        public static BridgePlan CalculateBridge(Vector3Int startPos, Vector3Int endPos, GameSettings gameSettings)
        {
            var plan = new BridgePlan
            {
                StartPosition = startPos,
                EndPosition = endPos,
                Placements = new List<SegmentPlacement>(),
                IsValid = false
            };
            
            var delta = endPos - startPos;
            if (!IsStraightLine(delta))
                return plan;
            
            var settings = gameSettings.BridgeSettings;
            
            var direction = new Vector3(
                delta.x != 0 ? Mathf.Sign(delta.x) : 0,
                delta.y != 0 ? Mathf.Sign(delta.y) : 0,
                delta.z != 0 ? Mathf.Sign(delta.z) : 0
            );
            plan.Direction = direction;
            
            var baseRotation = Quaternion.LookRotation(direction);
            var correctionRotation = Quaternion.Euler(0, -90, 0);
            var finalRotation = baseRotation * correctionRotation;
            
            var startWidth = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Start, direction);
            var middleWidth = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Middle, direction);
            var fillerWidth = settings.GetSegmentSpacing(BridgeSegment.SegmentType.Filler, direction);
            var endWidth = settings.GetSegmentSpacing(BridgeSegment.SegmentType.End, direction);
            
            var totalDistance = Mathf.Abs(delta.x) + Mathf.Abs(delta.y) + Mathf.Abs(delta.z);
            
            var gapCells = totalDistance - (startWidth / 2) - (endWidth / 2) - 1;
            
            var middleCount = gapCells / middleWidth;
            var remainingCells = gapCells - (middleCount * middleWidth);
            var fillerCount = remainingCells;
            
            var fillersBeforeMiddles = fillerCount / 2;
            var fillersAfterMiddles = fillerCount - fillersBeforeMiddles;
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
            
            if (fillersBeforeMiddles > 0 && middleCount > 0)
            {
                currentDistance += (middleWidth / 2);
            }
            else if (middleCount > 0)
            {
                currentDistance = startWidth / 2 + 1 + middleWidth / 2;
            }
            
            for (int i = 0; i < middleCount; i++)
            {
                var pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
                plan.Placements.Add(new SegmentPlacement
                {
                    Type = BridgeSegment.SegmentType.Middle,
                    GridPosition = pos,
                    Rotation = finalRotation
                });
                currentDistance += middleWidth;
            }
            
            if (middleCount > 0 && fillersAfterMiddles > 0)
            {
                currentDistance = currentDistance - middleWidth + (middleWidth / 2) + 1;
            }
            
            for (int i = 0; i < fillersAfterMiddles; i++)
            {
                var pos = startPos + Vector3Int.RoundToInt(direction * currentDistance);
                plan.Placements.Add(new SegmentPlacement
                {
                    Type = BridgeSegment.SegmentType.Filler,
                    GridPosition = pos,
                    Rotation = finalRotation
                });
                currentDistance += fillerWidth;
            }
            
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
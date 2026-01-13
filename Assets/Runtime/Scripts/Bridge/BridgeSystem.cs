using System.Collections.Generic;
using UnityEngine;
using ModularBridge.Core;

namespace ModularBridge.Bridge
{
    public class BridgeSystem : MonoBehaviour
    {
        [Header("Bridge Container")]
        [SerializeField] private Transform bridgeContainer;
        
        [Header("Dependencies")]
        [SerializeField] private GameSettings gameSettings;
        
        private List<Bridge> activeBridges = new List<Bridge>();
        private HashSet<BridgeSegment> standaloneSegments = new HashSet<BridgeSegment>();
        
        private void Awake()
        {
            if (gameSettings == null)
                throw new System.Exception("[BridgeSystem] GameSettings not assigned!");
            
            if (bridgeContainer == null)
                throw new System.Exception("[BridgeSystem] BridgeContainer not assigned!");
        }
        
        public void OnSegmentPlaced(BridgeSegment segment)
        {
            if (segment.Type == BridgeSegment.SegmentType.Start ||
                segment.Type == BridgeSegment.SegmentType.End)
            {
                standaloneSegments.Add(segment);
                TryAutoConnect(segment);
            }
        }
        
        public void OnSegmentRemoved(BridgeSegment segment)
        {
            standaloneSegments.Remove(segment);
            
            if (segment.ParentBridge != null)
            {
                DestroyBridge(segment.ParentBridge);
            }
        }
        
        private void TryAutoConnect(BridgeSegment newSegment)
        {
            var targetSegment = FindConnectableSegment(newSegment);
            
            if (targetSegment != null)
            {
                CreateBridge(newSegment, targetSegment);
            }
        }
        
        private BridgeSegment FindConnectableSegment(BridgeSegment segment)
        {
            var segmentPos = segment.GridPosition;
            var lookingFor = BridgeSegment.SegmentType.End;
            
            if (segment.Type == BridgeSegment.SegmentType.End)
            {
                lookingFor = BridgeSegment.SegmentType.Start;
            }
            else if (segment.Type != BridgeSegment.SegmentType.Start)
            {
                return null;
            }
            
            foreach (var candidate in standaloneSegments)
            {
                if (candidate == segment)
                    continue;
                
                if (candidate.Type != lookingFor)
                    continue;
                
                var candidatePos = candidate.GridPosition;
                
                var sameX = candidatePos.x == segmentPos.x;
                var sameZ = candidatePos.z == segmentPos.z;
                
                if (sameX || sameZ)
                {
                    return candidate;
                }
            }
            
            return null;
        }
        
        public Bridge CreateBridge(BridgeSegment start, BridgeSegment end)
        {
            if (start.Type == BridgeSegment.SegmentType.End)
            {
                var temp = start;
                start = end;
                end = temp;
            }
            
            standaloneSegments.Remove(start);
            standaloneSegments.Remove(end);
            
            var plan = BridgeBuilder.CalculateBridge(
                start.GridPosition,
                end.GridPosition,
                gameSettings
            );
            
            if (!plan.IsValid)
            {
                standaloneSegments.Add(start);
                standaloneSegments.Add(end);
                return null;
            }
            
            var bridge = new Bridge(start, end);
            
            var settings = gameSettings.BridgeSettings;
            
            for (int i = 1; i < plan.Placements.Count - 1; i++)
            {
                var placement = plan.Placements[i];
                
                var prefab = settings.GetPrefabForType(placement.Type);
                if (prefab == null)
                    continue;
                
                var segment = Instantiate(prefab, bridgeContainer);
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
                    Destroy(segment.gameObject);
                    bridge.Destroy();
                    
                    standaloneSegments.Add(start);
                    standaloneSegments.Add(end);
                    return null;
                }
            }
            
            activeBridges.Add(bridge);
            
            return bridge;
        }
        
        public void BreakBridge(Bridge bridge, bool keepStartEnd = true)
        {
            if (!activeBridges.Contains(bridge))
                return;
            
            activeBridges.Remove(bridge);
            
            if (keepStartEnd)
            {
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
        }
        
        public Bridge GetBridgeForSegment(BridgeSegment segment)
        {
            return segment?.ParentBridge;
        }
        
        public void DestroyBridge(Bridge bridge)
        {
            BreakBridge(bridge, keepStartEnd: true);
        }
        
        public IReadOnlyList<Bridge> GetActiveBridges()
        {
            return activeBridges.AsReadOnly();
        }
    }
}
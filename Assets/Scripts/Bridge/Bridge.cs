using System.Collections.Generic;
using UnityEngine;

namespace ModularBridge.Bridge
{
    /// <summary>
    /// Represents a complete bridge made up of multiple segments.
    /// </summary>
    public class Bridge
    {
        public BridgeSegment StartSegment { get; private set; }
        public BridgeSegment EndSegment { get; private set; }
        public List<BridgeSegment> MiddleSegments { get; private set; }
        public List<BridgeSegment> FillerSegments { get; private set; }
        
        public Vector3Int StartPosition => StartSegment.GridPosition;
        public Vector3Int EndPosition => EndSegment.GridPosition;
        
        public Bridge(BridgeSegment start, BridgeSegment end)
        {
            StartSegment = start;
            EndSegment = end;
            MiddleSegments = new List<BridgeSegment>();
            FillerSegments = new List<BridgeSegment>();
            
            // Link segments to this bridge
            StartSegment.ParentBridge = this;
            EndSegment.ParentBridge = this;
        }
        
        public void AddMiddleSegment(BridgeSegment segment)
        {
            MiddleSegments.Add(segment);
            segment.ParentBridge = this;
        }
        
        public void AddFillerSegment(BridgeSegment segment)
        {
            FillerSegments.Add(segment);
            segment.ParentBridge = this;
        }
        
        public List<BridgeSegment> GetAllSegments()
        {
            List<BridgeSegment> all = new List<BridgeSegment>();
            all.Add(StartSegment);
            all.AddRange(MiddleSegments);
            all.AddRange(FillerSegments);
            all.Add(EndSegment);
            return all;
        }
        
        public void Destroy()
        {
            // Remove all segments
            foreach (var segment in GetAllSegments())
            {
                segment.Remove();
                Object.Destroy(segment.gameObject);
            }
        }
    }
}
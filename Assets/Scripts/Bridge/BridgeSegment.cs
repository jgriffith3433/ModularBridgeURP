using UnityEngine;
using ModularBridge.Core;
using ModularBridge.Grid;

namespace ModularBridge.Bridge
{
    /// <summary>
    /// Represents a single segment of a bridge (Start, Middle, Filler, or End).
    /// </summary>
    public class BridgeSegment : GridObject
    {
        public enum SegmentType
        {
            Start,
            Middle,
            Filler,
            End
        }
        
        [Header("Bridge Properties")]
        [SerializeField] private SegmentType segmentType;
        
        // Parent bridge (if part of a connected bridge system)
        private Bridge parentBridge;
        
        public SegmentType Type => segmentType;
        public Bridge ParentBridge
        {
            get => parentBridge;
            set => parentBridge = value;
        }
        
        protected override void OnPlaced()
        {
            base.OnPlaced();
            
            // Notify bridge system that a segment was placed
            Game.Instance.Bridges.OnSegmentPlaced(this);
        }
        
        protected override void OnRemoved()
        {
            base.OnRemoved();
            
            // Notify bridge system that a segment was removed
            Game.Instance.Bridges.OnSegmentRemoved(this);
        }
    }
}
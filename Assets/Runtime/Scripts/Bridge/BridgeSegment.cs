using UnityEngine;
using ModularBridge.Core;
using ModularBridge.Grid;

namespace ModularBridge.Bridge
{
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
            
            if (Game.Instance != null && Game.Instance.Bridges != null)
            {
                Game.Instance.Bridges.OnSegmentPlaced(this);
            }
        }
        
        protected override void OnRemoved()
        {
            base.OnRemoved();
            
            if (Game.Instance != null && Game.Instance.Bridges != null)
            {
                Game.Instance.Bridges.OnSegmentRemoved(this);
            }
        }
    }
}
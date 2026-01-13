using UnityEngine;

namespace ModularBridge.Bridge
{
    [CreateAssetMenu(fileName = "BridgeSettings", menuName = "Game/Bridge Settings")]
    public class BridgeSettings : ScriptableObject
    {
        [Header("Prefabs")]
        [SerializeField] private BridgeSegment startPrefab;
        [SerializeField] private BridgeSegment endPrefab;
        [SerializeField] private BridgeSegment middlePrefab;
        [SerializeField] private BridgeSegment fillerPrefab;
        
        public BridgeSegment StartPrefab => startPrefab;
        public BridgeSegment EndPrefab => endPrefab;
        public BridgeSegment MiddlePrefab => middlePrefab;
        public BridgeSegment FillerPrefab => fillerPrefab;
        
        public int GetSegmentLength(BridgeSegment.SegmentType type)
        {
            var prefab = GetPrefabForType(type);
            if (prefab == null) return 1;
            
            var size = prefab.GridSize;
            return Mathf.Max(size.x, Mathf.Max(size.y, size.z));
        }
        
        public int GetSegmentSpacing(BridgeSegment.SegmentType type, Vector3 direction)
        {
            var prefab = GetPrefabForType(type);
            if (prefab == null) return 1;
            
            var size = prefab.GridSize;
            
            if (Mathf.Abs(direction.x) > 0.5f)
                return size.x;
            if (Mathf.Abs(direction.y) > 0.5f)
                return size.y;
            if (Mathf.Abs(direction.z) > 0.5f)
                return size.z;
            
            return 1;
        }
        
        public BridgeSegment GetPrefabForType(BridgeSegment.SegmentType type)
        {
            switch (type)
            {
                case BridgeSegment.SegmentType.Start:
                    return startPrefab;
                case BridgeSegment.SegmentType.Middle:
                    return middlePrefab;
                case BridgeSegment.SegmentType.Filler:
                    return fillerPrefab;
                case BridgeSegment.SegmentType.End:
                    return endPrefab;
                default:
                    return null;
            }
        }
    }
}
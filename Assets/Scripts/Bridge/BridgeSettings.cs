using UnityEngine;

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
    
    /// <summary>
    /// Get the length of a segment type along the bridge direction (uses max dimension of GridSize).
    /// </summary>
    public int GetSegmentLength(BridgeSegment.SegmentType type)
    {
        BridgeSegment prefab = GetPrefabForType(type);
        if (prefab == null) return 1;
        
        Vector3Int size = prefab.GridSize;
        // Return the maximum dimension (the length along the bridge)
        return Mathf.Max(size.x, Mathf.Max(size.y, size.z));
    }
    
    /// <summary>
    /// Get the spacing interval for a segment type along a specific direction.
    /// This is the distance to place the next segment to make them adjacent.
    /// </summary>
    public int GetSegmentSpacing(BridgeSegment.SegmentType type, Vector3 direction)
    {
        BridgeSegment prefab = GetPrefabForType(type);
        if (prefab == null) return 1;
        
        Vector3Int size = prefab.GridSize;
        
        // Get the size in the bridge direction
        if (Mathf.Abs(direction.x) > 0.5f)
            return size.x;
        if (Mathf.Abs(direction.y) > 0.5f)
            return size.y;
        if (Mathf.Abs(direction.z) > 0.5f)
            return size.z;
        
        return 1;
    }
    
    /// <summary>
    /// Get the prefab for a given segment type.
    /// </summary>
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
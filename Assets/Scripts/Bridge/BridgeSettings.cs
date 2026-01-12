using UnityEngine;

[CreateAssetMenu(fileName = "BridgeSettings", menuName = "Game/Bridge Settings")]
public class BridgeSettings : ScriptableObject
{
    [Header("Prefabs")]
    [SerializeField] private BridgeSegment startPrefab;
    [SerializeField] private BridgeSegment endPrefab;
    [SerializeField] private BridgeSegment middlePrefab;
    [SerializeField] private BridgeSegment fillerPrefab;
    
    [Header("Segment Lengths (in grid units)")]
    [SerializeField] private int middleSegmentLength = 5;
    [SerializeField] private int fillerSegmentLength = 1;
    
    public BridgeSegment StartPrefab => startPrefab;
    public BridgeSegment EndPrefab => endPrefab;
    public BridgeSegment MiddlePrefab => middlePrefab;
    public BridgeSegment FillerPrefab => fillerPrefab;
    public int MiddleSegmentLength => middleSegmentLength;
    public int FillerSegmentLength => fillerSegmentLength;
}
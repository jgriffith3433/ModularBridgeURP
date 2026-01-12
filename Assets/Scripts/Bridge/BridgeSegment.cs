using UnityEngine;
using System.Collections.Generic;

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
    [SerializeField] private int segmentLength = 1; // Length in grid units
    
    [Header("Connection Rules")]
    [SerializeField] private List<SegmentType> canConnectTo = new List<SegmentType>();
    
    [Header("Connection Points")]
    [SerializeField] private Transform connectionStart;
    [SerializeField] private Transform connectionEnd;
    
    // Parent bridge (if part of a connected bridge system)
    private Bridge parentBridge;
    
    public SegmentType Type => segmentType;
    public int SegmentLength => segmentLength;
    public Transform ConnectionStart => connectionStart;
    public Transform ConnectionEnd => connectionEnd;
    public Bridge ParentBridge
    {
        get => parentBridge;
        set => parentBridge = value;
    }
    
    public bool CanConnectTo(SegmentType otherType)
    {
        return canConnectTo.Contains(otherType);
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
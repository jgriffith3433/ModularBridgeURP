using UnityEngine;

/// <summary>
/// Manages the game's grid system: coordinate conversion, occupancy tracking, and visualization.
/// </summary>
public class GridSystem : MonoBehaviour
{
    [SerializeField] private GameGridSettings settings = null;

    private GridObjectRegistry registry;
    
    public GridObjectRegistry Registry => registry;
    
    private void Awake()
    {
        registry = new GridObjectRegistry();
    }
    
    #region Coordinate Conversion
    
    /// <summary>
    /// Convert world position to grid coordinates.
    /// </summary>
    public Vector3Int WorldToGrid(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - settings.GridOrigin;
        
        return new Vector3Int(
            Mathf.RoundToInt(localPos.x / settings.CellSize),
            Mathf.RoundToInt(localPos.y / settings.CellSize),
            Mathf.RoundToInt(localPos.z / settings.CellSize)
        );
    }
    
    /// <summary>
    /// Convert grid coordinates to world position (center of cell).
    /// </summary>
    public Vector3 GridToWorld(Vector3Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * settings.CellSize,
            gridPosition.y * settings.CellSize,
            gridPosition.z * settings.CellSize
        ) + settings.GridOrigin;
    }
    
    /// <summary>
    /// Snap world position to nearest grid cell center.
    /// </summary>
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        return GridToWorld(WorldToGrid(worldPosition));
    }
    
    #endregion
    
    #region Occupancy Queries
    
    public bool IsCellOccupied(Vector3Int gridPosition)
    {
        return registry.IsCellOccupied(gridPosition);
    }
    
    public GridObject GetObjectAt(Vector3Int gridPosition)
    {
        return registry.GetObjectAt(gridPosition);
    }
    
    public bool CanPlaceObject(Vector3Int gridPosition, Vector3Int size, GridObject ignoreObject = null)
    {
        return registry.CanPlaceObject(gridPosition, size, ignoreObject);
    }
    
    #endregion
    
    #region Visualization
    
    private void OnDrawGizmos()
    {
        if (settings == null || !settings.ShowGridInEditor)
            return;
        
        DrawGrid();
    }
    
    private void DrawGrid()
    {
        Gizmos.color = settings.GridColor;
        
        int range = settings.GridDrawDistance;
        float cellSize = settings.CellSize;
        Vector3 origin = settings.GridOrigin;
        
        // Draw vertical lines (along Z axis)
        for (int x = -range; x <= range; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0, -range * cellSize);
            Vector3 end = origin + new Vector3(x * cellSize, 0, range * cellSize);
            Gizmos.DrawLine(start, end);
        }
        
        // Draw horizontal lines (along X axis)
        for (int z = -range; z <= range; z++)
        {
            Vector3 start = origin + new Vector3(-range * cellSize, 0, z * cellSize);
            Vector3 end = origin + new Vector3(range * cellSize, 0, z * cellSize);
            Gizmos.DrawLine(start, end);
        }
    }
    
    #endregion
}
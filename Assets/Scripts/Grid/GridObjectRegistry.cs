using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// High-performance spatial data structure for looking up GridObjects.
/// Uses a Dictionary for O(1) lookups by grid position.
/// </summary>
public class GridObjectRegistry
{
    // Fast lookup: grid position -> GridObject
    private readonly Dictionary<Vector3Int, GridObject> objectsByPosition = new Dictionary<Vector3Int, GridObject>();
    
    // Fast lookup by type (for finding specific objects like bridge segments)
    private readonly Dictionary<System.Type, HashSet<GridObject>> objectsByType = new Dictionary<System.Type, HashSet<GridObject>>();
    
    // All registered objects
    private readonly HashSet<GridObject> allObjects = new HashSet<GridObject>();
    
    /// <summary>
    /// Register an object and all cells it occupies.
    /// </summary>
    public void Register(GridObject gridObject)
    {
        if (gridObject == null)
        {
            Debug.LogError("[GridObjectRegistry] Attempted to register null GridObject");
            return;
        }
        
        if (allObjects.Contains(gridObject))
        {
            Debug.LogWarning($"[GridObjectRegistry] Object {gridObject.name} is already registered");
            return;
        }
        
        // Register all cells this object occupies
        Vector3Int basePos = gridObject.GridPosition;
        Vector3Int size = gridObject.GridSize;
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    Vector3Int cellPos = basePos + new Vector3Int(x, y, z);
                    
                    if (objectsByPosition.ContainsKey(cellPos))
                    {
                        Debug.LogWarning($"[GridObjectRegistry] Cell {cellPos} is already occupied!");
                        continue;
                    }
                    
                    objectsByPosition[cellPos] = gridObject;
                }
            }
        }
        
        // Add to type lookup
        System.Type objectType = gridObject.GetType();
        if (!objectsByType.ContainsKey(objectType))
        {
            objectsByType[objectType] = new HashSet<GridObject>();
        }
        objectsByType[objectType].Add(gridObject);
        
        // Add to all objects
        allObjects.Add(gridObject);
    }
    
    /// <summary>
    /// Unregister an object and free all its cells.
    /// </summary>
    public void Unregister(GridObject gridObject)
    {
        if (gridObject == null || !allObjects.Contains(gridObject))
            return;
        
        // Unregister all cells
        Vector3Int basePos = gridObject.GridPosition;
        Vector3Int size = gridObject.GridSize;
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    Vector3Int cellPos = basePos + new Vector3Int(x, y, z);
                    objectsByPosition.Remove(cellPos);
                }
            }
        }
        
        // Remove from type lookup
        System.Type objectType = gridObject.GetType();
        if (objectsByType.ContainsKey(objectType))
        {
            objectsByType[objectType].Remove(gridObject);
        }
        
        // Remove from all objects
        allObjects.Remove(gridObject);
    }
    
    /// <summary>
    /// Check if a cell is occupied. O(1) lookup.
    /// </summary>
    public bool IsCellOccupied(Vector3Int gridPosition)
    {
        return objectsByPosition.ContainsKey(gridPosition);
    }
    
    /// <summary>
    /// Get the object at a specific cell. O(1) lookup.
    /// </summary>
    public GridObject GetObjectAt(Vector3Int gridPosition)
    {
        return objectsByPosition.TryGetValue(gridPosition, out GridObject obj) ? obj : null;
    }
    
    /// <summary>
    /// Check if an object can be placed at a position with a given size.
    /// </summary>
    public bool CanPlaceObject(Vector3Int gridPosition, Vector3Int size, GridObject ignoreObject = null)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    Vector3Int cellPos = gridPosition + new Vector3Int(x, y, z);
                    
                    if (objectsByPosition.TryGetValue(cellPos, out GridObject existingObject))
                    {
                        if (existingObject != ignoreObject)
                        {
                            return false;
                        }
                    }
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get all objects of a specific type. O(1) lookup.
    /// </summary>
    public IEnumerable<T> GetObjectsOfType<T>() where T : GridObject
    {
        System.Type type = typeof(T);
        
        if (objectsByType.TryGetValue(type, out HashSet<GridObject> objects))
        {
            foreach (var obj in objects)
            {
                yield return obj as T;
            }
        }
    }
    
    /// <summary>
    /// Get all objects within a radius of a point.
    /// </summary>
    public List<GridObject> GetObjectsInRadius(Vector3Int center, int radius)
    {
        List<GridObject> results = new List<GridObject>();
        HashSet<GridObject> added = new HashSet<GridObject>();
        
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3Int checkPos = center + new Vector3Int(x, y, z);
                    
                    if (objectsByPosition.TryGetValue(checkPos, out GridObject obj))
                    {
                        if (!added.Contains(obj))
                        {
                            results.Add(obj);
                            added.Add(obj);
                        }
                    }
                }
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// Clear all registered objects.
    /// </summary>
    public void Clear()
    {
        objectsByPosition.Clear();
        objectsByType.Clear();
        allObjects.Clear();
    }
}
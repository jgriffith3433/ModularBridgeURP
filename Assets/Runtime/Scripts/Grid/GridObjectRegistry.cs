using System.Collections.Generic;
using UnityEngine;

namespace ModularBridge.Grid
{
    public class GridObjectRegistry
    {
        private readonly Dictionary<Vector3Int, GridObject> objectsByPosition = new Dictionary<Vector3Int, GridObject>();
        private readonly Dictionary<System.Type, HashSet<GridObject>> objectsByType = new Dictionary<System.Type, HashSet<GridObject>>();
        private readonly HashSet<GridObject> allObjects = new HashSet<GridObject>();
        
        public void Register(GridObject gridObject)
        {
            if (gridObject == null)
                return;
            
            if (allObjects.Contains(gridObject))
                return;
            
            var basePos = gridObject.GridPosition;
            var min = gridObject.GridMin;
            var max = gridObject.GridMax;
            
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        var cellPos = basePos + new Vector3Int(x, y, z);
                        
                        if (objectsByPosition.ContainsKey(cellPos))
                            continue;
                        
                        objectsByPosition[cellPos] = gridObject;
                    }
                }
            }
            
            var objectType = gridObject.GetType();
            if (!objectsByType.ContainsKey(objectType))
            {
                objectsByType[objectType] = new HashSet<GridObject>();
            }
            objectsByType[objectType].Add(gridObject);
            
            allObjects.Add(gridObject);
        }
        
        public void Unregister(GridObject gridObject)
        {
            if (gridObject == null || !allObjects.Contains(gridObject))
                return;
            
            var basePos = gridObject.GridPosition;
            var min = gridObject.GridMin;
            var max = gridObject.GridMax;
            
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        var cellPos = basePos + new Vector3Int(x, y, z);
                        objectsByPosition.Remove(cellPos);
                    }
                }
            }
            
            var objectType = gridObject.GetType();
            if (objectsByType.ContainsKey(objectType))
            {
                objectsByType[objectType].Remove(gridObject);
            }
            
            allObjects.Remove(gridObject);
        }
        
        public bool IsCellOccupied(Vector3Int gridPosition)
        {
            return objectsByPosition.ContainsKey(gridPosition);
        }
        
        public GridObject GetObjectAt(Vector3Int gridPosition)
        {
            return objectsByPosition.TryGetValue(gridPosition, out var obj) ? obj : null;
        }
        
        public bool CanPlaceObject(Vector3Int gridPosition, Vector3Int size, GridObject ignoreObject = null)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var cellPos = gridPosition + new Vector3Int(x, y, z);
                        
                        if (objectsByPosition.TryGetValue(cellPos, out var existingObject))
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
        
        public bool CanPlaceObject(Vector3Int gridPosition, GridObject checkObject, GridObject ignoreObject = null)
        {
            var min = checkObject.GridMin;
            var max = checkObject.GridMax;
            
            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        var cellPos = gridPosition + new Vector3Int(x, y, z);
                        
                        if (objectsByPosition.TryGetValue(cellPos, out var existingObject))
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
        
        public IEnumerable<T> GetObjectsOfType<T>() where T : GridObject
        {
            var type = typeof(T);
            
            if (objectsByType.TryGetValue(type, out var objects))
            {
                foreach (var obj in objects)
                {
                    yield return obj as T;
                }
            }
        }
        
        public List<GridObject> GetObjectsInRadius(Vector3Int center, int radius)
        {
            var results = new List<GridObject>();
            var added = new HashSet<GridObject>();
            
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    for (int z = -radius; z <= radius; z++)
                    {
                        var checkPos = center + new Vector3Int(x, y, z);
                        
                        if (objectsByPosition.TryGetValue(checkPos, out var obj))
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
        
        public void Clear()
        {
            objectsByPosition.Clear();
            objectsByType.Clear();
            allObjects.Clear();
        }
    }
}
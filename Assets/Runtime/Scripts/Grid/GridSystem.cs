using UnityEngine;

namespace ModularBridge.Grid
{
    public class GridSystem : MonoBehaviour
    {
        [SerializeField] private GameGridSettings settings = null;

        private GridObjectRegistry registry;
        
        public GridObjectRegistry Registry => registry;
        
        private void Awake()
        {
            registry = new GridObjectRegistry();
        }
        
        public Vector3Int WorldToGrid(Vector3 worldPosition)
        {
            var localPos = worldPosition - settings.GridOrigin;
            
            return new Vector3Int(
                Mathf.RoundToInt(localPos.x / settings.CellSize),
                Mathf.RoundToInt(localPos.y / settings.CellSize),
                Mathf.RoundToInt(localPos.z / settings.CellSize)
            );
        }
        
        public Vector3 GridToWorld(Vector3Int gridPosition)
        {
            return new Vector3(
                gridPosition.x * settings.CellSize,
                gridPosition.y * settings.CellSize,
                gridPosition.z * settings.CellSize
            ) + settings.GridOrigin;
        }
        
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            return GridToWorld(WorldToGrid(worldPosition));
        }
        
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
        
        public bool CanPlaceObject(Vector3Int gridPosition, GridObject checkObject, GridObject ignoreObject = null)
        {
            return registry.CanPlaceObject(gridPosition, checkObject, ignoreObject);
        }
    }
}
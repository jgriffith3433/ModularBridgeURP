using UnityEngine;
using ModularBridge.Core;

namespace ModularBridge.Grid
{
    public class GridObject : MonoBehaviour
    {
        [Header("Inventory")]
        [SerializeField] private InventoryItemDefinition inventoryItemDefinition;
        
        [Header("Grid Properties")]
        [Tooltip("Minimum bounds of the grid footprint (inclusive). For 2D grids, use Y=0.")]
        [SerializeField] private Vector3Int gridMin = Vector3Int.zero;
        [Tooltip("Maximum bounds of the grid footprint (inclusive). For 2D grids, use Y=0.")]
        [SerializeField] private Vector3Int gridMax = Vector3Int.zero;
        
        [Header("Dependencies")]
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private GameSettings gameSettings;
        
        [Header("Placement Materials")]
        [SerializeField] private Material validPlacementMaterial;
        [SerializeField] private Material invalidPlacementMaterial;
        
        private Vector3Int currentGridPosition;
        private bool isPlaced = false;
        private bool isShowingPreview = false;
        
        private Renderer[] renderers;
        private Material[][] originalMaterials;
        
        public Vector3Int GridMin => gridMin;
        public Vector3Int GridMax => gridMax;
        public Vector3Int GridSize => gridMax - gridMin + Vector3Int.one;
        public Vector3Int GridPosition => currentGridPosition;
        public bool IsPlaced => isPlaced;
        public InventoryItemDefinition InventoryItem => inventoryItemDefinition;
        
        protected virtual void Awake()
        {
            if (gridSystem == null)
                gridSystem = Game.Instance.Grid;
            
            if (gameSettings == null)
                gameSettings = Game.Instance.Settings;
            
            if (gameSettings == null)
                throw new System.Exception($"[GridObject] GameSettings not assigned on {name}!");
            
            if (gridSystem == null)
                throw new System.Exception($"[GridObject] GridSystem not assigned on {name}!");
            
            CacheRenderers();
        }
        
        private void CacheRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalMaterials = new Material[renderers.Length][];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].sharedMaterials;
            }
        }
        
        public void SetGridPosition(Vector3Int gridPosition)
        {
            currentGridPosition = gridPosition;
            transform.position = gridSystem.GridToWorld(gridPosition);
        }
        
        public void ShowPlacementPreview(Vector3Int gridPosition, bool isValid)
        {
            SetGridPosition(gridPosition);
            isShowingPreview = true;
            
            var previewMaterial = isValid ? validPlacementMaterial : invalidPlacementMaterial;
            
            if (previewMaterial != null)
            {
                ApplyMaterialToAll(previewMaterial);
            }
        }
        
        public void HidePlacementPreview()
        {
            if (!isShowingPreview)
                return;
            
            isShowingPreview = false;
            RestoreOriginalMaterials();
        }
        
        public bool TryPlace(Vector3Int gridPosition)
        {
            if (!gridSystem.CanPlaceObject(gridPosition, this))
            {
                return false;
            }
            
            SetGridPosition(gridPosition);
            gridSystem.Registry.Register(this);
            isPlaced = true;
            isShowingPreview = false;
            
            RestoreOriginalMaterials();
            OnPlaced();
            
            return true;
        }
        
        public void Remove()
        {
            if (!isPlaced)
                return;
            
            gridSystem.Registry.Unregister(this);
            isPlaced = false;
            
            OnRemoved();
        }
        
        private void ApplyMaterialToAll(Material material)
        {
            foreach (var renderer in renderers)
            {
                var materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
                renderer.materials = materials;
            }
        }
        
        private void RestoreOriginalMaterials()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].materials = originalMaterials[i];
            }
        }
        
        protected virtual void OnPlaced()
        {
        }
        
        protected virtual void OnRemoved()
        {
        }
        
        private void OnDestroy()
        {
            Remove();
        }
    }
}
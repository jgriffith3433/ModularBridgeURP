using UnityEngine;

/// <summary>
/// Base component for any object that exists on the game grid.
/// Handles grid positioning, placement validation, and visual feedback.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class GridObject : MonoBehaviour
{
    [Header("Grid Properties")]
    [SerializeField] private Vector3Int gridSize = Vector3Int.one;
    [SerializeField] private Vector3Int gridOffset = Vector3Int.zero;
    
    [Header("Placement Materials")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    
    private Vector3Int currentGridPosition;
    private bool isPlaced = false;
    private bool isShowingPreview = false;
    
    private Renderer[] renderers;
    private Material[][] originalMaterials;
    
    // Public properties
    public Vector3Int GridSize => gridSize;
    public Vector3Int GridOffset => gridOffset;
    public Vector3Int GridPosition => currentGridPosition;
    public bool IsPlaced => isPlaced;
    
    protected virtual void Awake()
    {
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
    
    #region Placement
    
    /// <summary>
    /// Set grid position without placing (for preview).
    /// </summary>
    public void SetGridPosition(Vector3Int gridPosition)
    {
        currentGridPosition = gridPosition;
        transform.position = Game.Instance.Grid.GridToWorld(gridPosition);
    }
    
    /// <summary>
    /// Show placement preview with validity feedback.
    /// </summary>
    public void ShowPlacementPreview(Vector3Int gridPosition, bool isValid)
    {
        SetGridPosition(gridPosition);
        isShowingPreview = true;
        
        Material previewMaterial = isValid ? validPlacementMaterial : invalidPlacementMaterial;
        
        if (previewMaterial != null)
        {
            ApplyMaterialToAll(previewMaterial);
        }
    }
    
    /// <summary>
    /// Hide placement preview and restore original materials.
    /// </summary>
    public void HidePlacementPreview()
    {
        if (!isShowingPreview)
            return;
        
        isShowingPreview = false;
        RestoreOriginalMaterials();
    }
    
    /// <summary>
    /// Attempt to place this object at the specified grid position.
    /// </summary>
    public bool TryPlace(Vector3Int gridPosition)
    {
        if (!Game.Instance.Grid.CanPlaceObject(gridPosition, gridSize))
        {
            Debug.LogWarning($"[GridObject] Cannot place {name} at {gridPosition} - cells occupied");
            return false;
        }
        
        SetGridPosition(gridPosition);
        Game.Instance.Grid.Registry.Register(this);
        isPlaced = true;
        isShowingPreview = false;
        
        RestoreOriginalMaterials();
        OnPlaced();
        
        return true;
    }
    
    /// <summary>
    /// Remove this object from the grid.
    /// </summary>
    public void Remove()
    {
        if (!isPlaced)
            return;
        
        Game.Instance.Grid.Registry.Unregister(this);
        isPlaced = false;
        
        OnRemoved();
    }
    
    #endregion
    
    #region Material Management
    
    private void ApplyMaterialToAll(Material material)
    {
        foreach (var renderer in renderers)
        {
            Material[] materials = new Material[renderer.sharedMaterials.Length];
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
    
    #endregion
    
    #region Virtual Methods
    
    protected virtual void OnPlaced()
    {
        // Override in derived classes for placement logic
    }
    
    protected virtual void OnRemoved()
    {
        // Override in derived classes for removal logic
    }
    
    #endregion
    
    private void OnDestroy()
    {
        Remove();
    }
    
    #region Editor Helpers
    
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            // Show grid footprint in editor
            Vector3 cellSize = Vector3.one * (Game.Instance?.Settings?.GridSettings?.CellSize ?? 1f);
            
            Gizmos.color = Color.cyan;
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        Vector3 offset = new Vector3(x, y, z);
                        Vector3 center = transform.position + Vector3.Scale(offset, cellSize);
                        Gizmos.DrawWireCube(center, cellSize * 0.9f);
                    }
                }
            }
        }
    }
    
    #endregion
}
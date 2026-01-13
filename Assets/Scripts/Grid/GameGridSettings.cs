using UnityEngine;

namespace ModularBridge.Grid
{
    [CreateAssetMenu(fileName = "GameGridSettings", menuName = "Game/Grid Settings")]
    public class GameGridSettings : ScriptableObject
    {
        [Header("Grid Dimensions")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3Int gridDimensions = new Vector3Int(100, 1, 100);
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        
        [Header("Visualization")]
        [SerializeField] private bool showGridInEditor = true;
        [SerializeField] private bool showGridInGame = false;
        [SerializeField] private Color gridColor = new Color(1, 1, 1, 0.2f);
        [SerializeField] private int gridDrawDistance = 50;
        
        public float CellSize => cellSize;
        public Vector3Int GridDimensions => gridDimensions;
        public Vector3 GridOrigin => gridOrigin;
        public bool ShowGridInEditor => showGridInEditor;
        public bool ShowGridInGame => showGridInGame;
        public Color GridColor => gridColor;
        public int GridDrawDistance => gridDrawDistance;
    }
}
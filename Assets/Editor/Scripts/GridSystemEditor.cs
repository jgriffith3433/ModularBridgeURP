using UnityEngine;
using UnityEditor;
using ModularBridge.Grid;

namespace ModularBridgeEditor
{
    [CustomEditor(typeof(GridSystem))]
    public class GridSystemEditor : Editor
    {
        private GridSystem gridSystem;
        
        private void OnEnable()
        {
            gridSystem = (GridSystem)target;
        }
        
        private void OnSceneGUI()
        {
            if (gridSystem == null)
                return;
            
            var settings = gridSystem.GetType()
                .GetField("settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(gridSystem) as GameGridSettings;
            
            if (settings == null || !settings.ShowGridInEditor)
                return;
            
            DrawGrid(settings);
        }
        
        private void DrawGrid(GameGridSettings settings)
        {
            Handles.color = settings.GridColor;
            
            var range = settings.GridDrawDistance;
            var cellSize = settings.CellSize;
            var origin = settings.GridOrigin;
            
            for (int x = -range; x <= range; x++)
            {
                var start = origin + new Vector3(x * cellSize, 0, -range * cellSize);
                var end = origin + new Vector3(x * cellSize, 0, range * cellSize);
                Handles.DrawLine(start, end);
            }
            
            for (int z = -range; z <= range; z++)
            {
                var start = origin + new Vector3(-range * cellSize, 0, z * cellSize);
                var end = origin + new Vector3(range * cellSize, 0, z * cellSize);
                Handles.DrawLine(start, end);
            }
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (gridSystem == null)
                return;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid Visualization", EditorStyles.boldLabel);
            
            var settings = gridSystem.GetType()
                .GetField("settings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(gridSystem) as GameGridSettings;
            
            if (settings != null)
            {
                EditorGUILayout.HelpBox(
                    $"Grid Draw Distance: {settings.GridDrawDistance}\n" +
                    $"Cell Size: {settings.CellSize}\n" +
                    $"Show in Editor: {settings.ShowGridInEditor}", 
                    MessageType.Info);
            }
        }
    }
}

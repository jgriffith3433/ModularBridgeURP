using UnityEngine;
using UnityEditor;
using ModularBridge.Grid;

namespace ModularBridgeEditor
{
    [CustomEditor(typeof(GridObject), true)]
    public class GridObjectEditor : Editor
    {
        private GridObject gridObject;
        
        private void OnEnable()
        {
            gridObject = (GridObject)target;
        }
        
        private void OnSceneGUI()
        {
            if (gridObject == null)
                return;
            
            var gameSettings = gridObject.GetType()
                .GetField("gameSettings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(gridObject) as ModularBridge.Core.GameSettings;
            
            if (gameSettings == null || gameSettings.GridSettings == null)
                return;
            
            var cellSize = gameSettings.GridSettings.CellSize;
            var gridMin = gridObject.GridMin;
            var gridMax = gridObject.GridMax;
            
            var isSelected = Selection.activeGameObject == gridObject.gameObject;
            Handles.color = isSelected ? Color.cyan : new Color(1f, 1f, 0f, 0.3f);
            
            for (int x = gridMin.x; x <= gridMax.x; x++)
            {
                for (int y = gridMin.y; y <= gridMax.y; y++)
                {
                    for (int z = gridMin.z; z <= gridMax.z; z++)
                    {
                        var offset = new Vector3(x, y, z) * cellSize;
                        var center = gridObject.transform.position + offset;
                        var size = Vector3.one * (cellSize * 0.9f);
                        
                        DrawWireCube(center, size);
                    }
                }
            }
        }
        
        private void DrawWireCube(Vector3 center, Vector3 size)
        {
            var halfSize = size * 0.5f;
            
            var v000 = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            var v001 = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            var v010 = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            var v011 = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            var v100 = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            var v101 = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            var v110 = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            var v111 = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            
            Handles.DrawLine(v000, v001);
            Handles.DrawLine(v000, v010);
            Handles.DrawLine(v000, v100);
            Handles.DrawLine(v001, v011);
            Handles.DrawLine(v001, v101);
            Handles.DrawLine(v010, v011);
            Handles.DrawLine(v010, v110);
            Handles.DrawLine(v011, v111);
            Handles.DrawLine(v100, v101);
            Handles.DrawLine(v100, v110);
            Handles.DrawLine(v101, v111);
            Handles.DrawLine(v110, v111);
        }
    }
}

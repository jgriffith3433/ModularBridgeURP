using UnityEngine;
using ModularBridge.Grid;

namespace ModularBridge.Core
{
    [CreateAssetMenu(fileName = "InventoryItemDefinition", menuName = "Game/Inventory Item Definition")]
    public class InventoryItemDefinition : ScriptableObject
    {
        [SerializeField] private GridObject prefab;
        [SerializeField] private int startingCount = 3;
        
        public GridObject Prefab => prefab;
        public int StartingCount => startingCount;
    }
}

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using ModularBridge.Grid;

namespace ModularBridge.Core
{
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] private List<InventoryItemDefinition> inventoryItems = new List<InventoryItemDefinition>();
        
        public UnityEvent<InventoryItemDefinition, int> OnInventoryChanged;
        
        private Dictionary<InventoryItemDefinition, int> currentCounts = new Dictionary<InventoryItemDefinition, int>();
        
        public IEnumerable<InventoryItemDefinition> GetAllDefinitions() => inventoryItems;
        
        private void Awake()
        {
            foreach (var item in inventoryItems)
            {
                if (item != null)
                {
                    currentCounts[item] = item.StartingCount;
                }
            }
        }
        
        public int GetCount(InventoryItemDefinition item)
        {
            return currentCounts.ContainsKey(item) ? currentCounts[item] : 0;
        }
        
        public bool HasItem(InventoryItemDefinition item)
        {
            return GetCount(item) > 0;
        }
        
        public bool TryTakeItem(InventoryItemDefinition item)
        {
            if (!currentCounts.ContainsKey(item) || currentCounts[item] <= 0)
                return false;
            
            currentCounts[item]--;
            OnInventoryChanged?.Invoke(item, currentCounts[item]);
            return true;
        }
        
        public void ReturnItem(InventoryItemDefinition item)
        {
            if (!currentCounts.ContainsKey(item))
            {
                currentCounts[item] = 0;
            }
            
            currentCounts[item]++;
            OnInventoryChanged?.Invoke(item, currentCounts[item]);
        }
    }
}

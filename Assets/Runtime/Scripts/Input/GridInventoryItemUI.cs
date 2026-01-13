using UnityEngine;
using TMPro;
using ModularBridge.Core;

namespace ModularBridge.Input
{
    public class GridInventoryItemUI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private InventoryItemDefinition inventoryItem;
        
        [Header("References")]
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private DraggableGridItem draggableItem;
        [SerializeField] private GridObjectDragDropManager dragDropManager;
        
        private void Awake()
        {
            if (inventoryItem == null)
            {
                throw new System.Exception("[GridInventoryItemUI] InventoryItemDefinition not assigned!");
            }
            
            if (inventorySystem == null)
            {
                throw new System.Exception("[GridInventoryItemUI] InventorySystem not assigned!");
            }
            
            if (draggableItem == null)
            {
                throw new System.Exception("[GridInventoryItemUI] DraggableGridItem not assigned!");
            }
            
            if (dragDropManager == null)
            {
                throw new System.Exception("[GridInventoryItemUI] GridObjectDragDropManager not assigned!");
            }
        }
        
        private void OnEnable()
        {
            if (dragDropManager != null)
            {
                dragDropManager.CanSpawn += CanSpawnItem;
                dragDropManager.OnCancelled += OnItemCancelled;
            }
            
            if (inventorySystem != null)
            {
                inventorySystem.OnInventoryChanged.AddListener(OnInventoryChanged);
            }
            
            UpdateDisplay();
        }
        
        private void OnDisable()
        {
            if (dragDropManager != null)
            {
                dragDropManager.CanSpawn -= CanSpawnItem;
                dragDropManager.OnCancelled -= OnItemCancelled;
            }
            
            if (inventorySystem != null)
            {
                inventorySystem.OnInventoryChanged.RemoveListener(OnInventoryChanged);
            }
        }
        
        private void Start()
        {
            UpdateDisplay();
        }
        
        private void OnInventoryChanged(InventoryItemDefinition item, int count)
        {
            if (item == inventoryItem)
            {
                UpdateDisplay();
            }
        }
        
        private bool CanSpawnItem(Grid.GridObject gridObjectPrefab)
        {
            if (gridObjectPrefab != inventoryItem.Prefab)
                return true;
            
            if (!inventorySystem.HasItem(inventoryItem))
                return false;
            
            inventorySystem.TryTakeItem(inventoryItem);
            UpdateDisplay();
            return true;
        }
        
        private void OnItemCancelled(Grid.GridObject gridObjectPrefab)
        {
            if (gridObjectPrefab != inventoryItem.Prefab)
                return;
            
            inventorySystem.ReturnItem(inventoryItem);
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            var count = inventorySystem.GetCount(inventoryItem);
            
            if (countText != null)
            {
                countText.text = count.ToString();
            }
            
            if (draggableItem != null)
            {
                draggableItem.SetEnabled(count > 0);
            }
        }
    }
}

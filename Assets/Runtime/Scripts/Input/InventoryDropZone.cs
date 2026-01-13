using UnityEngine;
using UnityEngine.EventSystems;
using ModularBridge.Core;
using ModularBridge.Grid;

namespace ModularBridge.Input
{
    public class InventoryDropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private InventorySystem inventorySystem;
        [SerializeField] private GameObject dropIndicator;
        
        private static InventoryDropZone hoveredDropZone;
        
        public static bool IsOverDropZone => hoveredDropZone != null;
        public static InventoryDropZone Current => hoveredDropZone;
        
        private void Awake()
        {
            if (inventorySystem == null)
            {
                throw new System.Exception("[InventoryDropZone] InventorySystem not assigned!");
            }
            
            if (dropIndicator != null)
            {
                dropIndicator.SetActive(false);
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            hoveredDropZone = this;
            
            if (dropIndicator != null)
            {
                dropIndicator.SetActive(true);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoveredDropZone == this)
            {
                hoveredDropZone = null;
            }
            
            if (dropIndicator != null)
            {
                dropIndicator.SetActive(false);
            }
        }
        
        public bool TryReturnItem(GridObject gridObject)
        {
            if (inventorySystem == null || gridObject == null)
                return false;
            
            if (gridObject.InventoryItem != null)
            {
                inventorySystem.ReturnItem(gridObject.InventoryItem);
                return true;
            }
            
            return false;
        }
        
        private void OnDisable()
        {
            if (hoveredDropZone == this)
            {
                hoveredDropZone = null;
            }
            
            if (dropIndicator != null)
            {
                dropIndicator.SetActive(false);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using ModularBridge.Bridge;

namespace ModularBridge.Input
{
    public class BridgeDragDropManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BridgePlacementController placementController;
        
        private DraggableBridgeItem currentDragItem;
        private BridgeSegment instantiated3DObject;
        private bool isDraggingFrom3D = false;
        
        private void Awake()
        {
            if (placementController == null)
            {
                throw new System.Exception("[BridgeDragDropManager] BridgePlacementController not assigned!");
            }
        }
        
        public void OnBeginDragFromUI(DraggableBridgeItem dragItem, PointerEventData eventData)
        {
            if (dragItem == null || dragItem.BridgeSegmentPrefab == null)
                return;
            
            if (placementController.IsPlacing)
                return;
            
            currentDragItem = dragItem;
            instantiated3DObject = Instantiate(dragItem.BridgeSegmentPrefab);
            instantiated3DObject.gameObject.name = $"{dragItem.BridgeSegmentPrefab.name}_Dragging";
            
            if (placementController != null)
            {
                isDraggingFrom3D = true;
                placementController.BeginPlacement(instantiated3DObject, eventData);
            }
        }
        
        public void OnDragUpdate(PointerEventData eventData)
        {
            if (!isDraggingFrom3D || placementController == null)
                return;
            
            placementController.UpdatePlacement(eventData);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggingFrom3D)
                return;
            
            if (placementController != null)
            {
                var placed = placementController.CompletePlacement(eventData);
                
                if (!placed && instantiated3DObject != null)
                {
                    Destroy(instantiated3DObject.gameObject);
                }
            }
            else if (instantiated3DObject != null)
            {
                Destroy(instantiated3DObject.gameObject);
            }
            
            currentDragItem = null;
            instantiated3DObject = null;
            isDraggingFrom3D = false;
        }
        
        public void CancelDrag()
        {
            if (!isDraggingFrom3D)
                return;
            
            if (placementController != null)
            {
                placementController.CancelPlacement();
            }
            
            if (instantiated3DObject != null)
            {
                Destroy(instantiated3DObject.gameObject);
            }
            
            currentDragItem = null;
            instantiated3DObject = null;
            isDraggingFrom3D = false;
        }
        
        public bool IsDragging => isDraggingFrom3D;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using ModularBridge.Bridge;

namespace ModularBridge.Input
{
    /// <summary>
    /// Manages the drag and drop flow from UI to 3D world.
    /// Acts as a bridge between UI draggable items and the BridgePlacementController.
    /// </summary>
    public class BridgeDragDropManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BridgePlacementController placementController;
        
        // Current drag state
        private DraggableBridgeItem currentDragItem;
        private BridgeSegment instantiated3DObject;
        private bool isDraggingFrom3D = false;
        
        private void Awake()
        {
            // Validate placement controller is assigned
            if (placementController == null)
            {
                throw new System.Exception("[BridgeDragDropManager] BridgePlacementController not assigned!");
            }
        }
        
        /// <summary>
        /// Called when user starts dragging a UI bridge item.
        /// </summary>
        public void OnBeginDragFromUI(DraggableBridgeItem dragItem, PointerEventData eventData)
        {
            if (dragItem == null || dragItem.BridgeSegmentPrefab == null)
                return;
            
            // Don't interfere if placement controller is already being used (e.g., 3D drag)
            if (placementController.IsPlacing)
                return;
            
            currentDragItem = dragItem;
            
            // Instantiate the 3D bridge segment
            instantiated3DObject = Instantiate(dragItem.BridgeSegmentPrefab);
            instantiated3DObject.gameObject.name = $"{dragItem.BridgeSegmentPrefab.name}_Dragging";
            
            // Activate the placement controller with the instantiated object
            if (placementController != null)
            {
                isDraggingFrom3D = true;
                placementController.BeginPlacement(instantiated3DObject, eventData);
            }
        }
        
        /// <summary>
        /// Called during drag to update position.
        /// </summary>
        public void OnDragUpdate(PointerEventData eventData)
        {
            if (!isDraggingFrom3D || placementController == null)
                return;
            
            placementController.UpdatePlacement(eventData);
        }
        
        /// <summary>
        /// Called when user releases the drag.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggingFrom3D)
                return;
            
            // Try to complete placement
            if (placementController != null)
            {
                bool placed = placementController.CompletePlacement(eventData);
                
                if (!placed && instantiated3DObject != null)
                {
                    // Placement failed, destroy the instantiated object
                    Destroy(instantiated3DObject.gameObject);
                }
            }
            else if (instantiated3DObject != null)
            {
                // No placement controller, cleanup
                Destroy(instantiated3DObject.gameObject);
            }
            
            // Reset state
            currentDragItem = null;
            instantiated3DObject = null;
            isDraggingFrom3D = false;
        }
        
        /// <summary>
        /// Cancel the current drag operation.
        /// </summary>
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

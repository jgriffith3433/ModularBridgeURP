using UnityEngine;
using UnityEngine.EventSystems;
using ModularBridge.Bridge;
using ModularBridge.Grid;

namespace ModularBridge.Input
{
    public class GridObjectDragDropManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BridgePlacementController placementController;
        
        private DraggableGridItem currentDragItem;
        private GridObject instantiatedGridObject;
        private bool isDraggingFromUI = false;
        
        public delegate bool CanSpawnDelegate(GridObject gridObjectPrefab);
        public delegate void OnSpawnedDelegate(GridObject gridObjectPrefab);
        public delegate void OnCancelledDelegate(GridObject gridObjectPrefab);
        
        public CanSpawnDelegate CanSpawn;
        public OnSpawnedDelegate OnSpawned;
        public OnCancelledDelegate OnCancelled;
        
        private void Awake()
        {
            if (placementController == null)
            {
                throw new System.Exception("[GridObjectDragDropManager] BridgePlacementController not assigned!");
            }
        }
        
        public void OnBeginDragFromUI(DraggableGridItem dragItem, PointerEventData eventData)
        {
            if (dragItem == null || dragItem.GridObjectPrefab == null)
                return;
            
            if (placementController.IsPlacing)
                return;
            
            if (CanSpawn != null && !CanSpawn(dragItem.GridObjectPrefab))
                return;
            
            currentDragItem = dragItem;
            instantiatedGridObject = Instantiate(dragItem.GridObjectPrefab);
            instantiatedGridObject.gameObject.name = $"{dragItem.GridObjectPrefab.name}_Dragging";
            
            if (placementController != null)
            {
                isDraggingFromUI = true;
                
                if (instantiatedGridObject is BridgeSegment bridgeSegment)
                {
                    placementController.BeginPlacement(bridgeSegment, eventData);
                }
            }
            
            OnSpawned?.Invoke(dragItem.GridObjectPrefab);
        }
        
        public void OnDragUpdate(PointerEventData eventData)
        {
            if (!isDraggingFromUI || placementController == null)
                return;
            
            placementController.UpdatePlacement(eventData);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggingFromUI)
                return;
            
            if (InventoryDropZone.IsOverDropZone && instantiatedGridObject != null)
            {
                if (placementController != null)
                {
                    placementController.CancelPlacement();
                }
                
                OnCancelled?.Invoke(currentDragItem.GridObjectPrefab);
                Destroy(instantiatedGridObject.gameObject);
                currentDragItem = null;
                instantiatedGridObject = null;
                isDraggingFromUI = false;
                return;
            }
            
            var placed = false;
            
            if (placementController != null)
            {
                placed = placementController.CompletePlacement(eventData);
                
                if (!placed && instantiatedGridObject != null)
                {
                    OnCancelled?.Invoke(currentDragItem.GridObjectPrefab);
                    Destroy(instantiatedGridObject.gameObject);
                }
            }
            else if (instantiatedGridObject != null)
            {
                OnCancelled?.Invoke(currentDragItem.GridObjectPrefab);
                Destroy(instantiatedGridObject.gameObject);
            }
            
            currentDragItem = null;
            instantiatedGridObject = null;
            isDraggingFromUI = false;
        }
        
        public void CancelDrag()
        {
            if (!isDraggingFromUI)
                return;
            
            if (placementController != null)
            {
                placementController.CancelPlacement();
            }
            
            if (instantiatedGridObject != null)
            {
                if (currentDragItem != null)
                {
                    OnCancelled?.Invoke(currentDragItem.GridObjectPrefab);
                }
                Destroy(instantiatedGridObject.gameObject);
            }
            
            currentDragItem = null;
            instantiatedGridObject = null;
            isDraggingFromUI = false;
        }
        
        public bool IsDragging => isDraggingFromUI;
    }
}

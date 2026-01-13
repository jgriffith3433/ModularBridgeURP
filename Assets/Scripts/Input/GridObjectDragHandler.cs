using UnityEngine;
using UnityEngine.EventSystems;
using ModularBridge.Bridge;
using ModularBridge.Core;

namespace ModularBridge.Input
{
    /// <summary>
    /// Detects and handles dragging of already-placed GridObjects in 3D space.
    /// Allows moving bridge segments that have been placed on the grid.
    /// </summary>
    public class GridObjectDragHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask gridObjectLayer;
        [SerializeField] private BridgePlacementController placementController;
        [SerializeField] private InputManager inputManager;
        
        private BridgeSegment selectedSegment;
        private bool isDragging = false;
        private Vector2 currentMousePosition;
        
        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            if (placementController == null)
                throw new System.Exception("[GridObjectDragHandler] BridgePlacementController not assigned!");
            
            if (inputManager == null)
                throw new System.Exception("[GridObjectDragHandler] InputManager not assigned!");
        }
        
        private void OnEnable()
        {
            if (inputManager.InputActions != null)
            {
                inputManager.InputActions.Gameplay.Click.performed += OnClickPerformed;
                inputManager.InputActions.Gameplay.Click.canceled += OnClickReleased;
            }
        }
        
        private void OnDisable()
        {
            if (inputManager.InputActions != null)
            {
                inputManager.InputActions.Gameplay.Click.performed -= OnClickPerformed;
                inputManager.InputActions.Gameplay.Click.canceled -= OnClickReleased;
            }
        }
        
        private void Update()
        {
            if (isDragging && inputManager.InputActions != null)
            {
                currentMousePosition = inputManager.InputActions.Gameplay.Point.ReadValue<Vector2>();
                
                // Update the placement controller with the current mouse position
                PointerEventData eventData = new PointerEventData(EventSystem.current)
                {
                    position = currentMousePosition
                };
                placementController.UpdatePlacement(eventData);
            }
        }
        
        private void OnClickPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            // Don't interfere if already placing something
            if (placementController.IsPlacing)
                return;
            
            currentMousePosition = inputManager.InputActions.Gameplay.Point.ReadValue<Vector2>();
            
            // Raycast to see if we clicked on a GridObject
            Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridObjectLayer))
            {
                BridgeSegment segment = hit.collider.GetComponentInParent<BridgeSegment>();
                
                if (segment != null && segment.IsPlaced)
                {
                    // Only allow dragging Start and End segments
                    if (segment.Type == BridgeSegment.SegmentType.Start || 
                        segment.Type == BridgeSegment.SegmentType.End)
                    {
                        BeginDrag(segment);
                    }
                }
            }
        }
        
        private void OnClickReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (isDragging)
            {
                EndDrag();
            }
        }
        
        private void BeginDrag(BridgeSegment segment)
        {
            selectedSegment = segment;
            isDragging = true;
            
            // Create a fake PointerEventData for the placement controller
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = currentMousePosition
            };
            
            // Start the move operation
            placementController.BeginMove(selectedSegment, eventData);
        }
        
        private void EndDrag()
        {
            if (selectedSegment == null || !isDragging)
                return;
            
            // Create a fake PointerEventData for the placement controller
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = currentMousePosition
            };
            
            // Complete the move
            bool success = placementController.CompleteMove(eventData);
            
            selectedSegment = null;
            isDragging = false;
        }
    }
}

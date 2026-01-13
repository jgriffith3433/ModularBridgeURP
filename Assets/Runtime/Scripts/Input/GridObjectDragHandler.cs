using UnityEngine;
using UnityEngine.EventSystems;
using ModularBridge.Bridge;
using ModularBridge.Core;

namespace ModularBridge.Input
{
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
                inputManager.InputActions.Gameplay.Point.performed += OnPointerMove;
            }
        }
        
        private void OnDisable()
        {
            if (inputManager.InputActions != null)
            {
                inputManager.InputActions.Gameplay.Click.performed -= OnClickPerformed;
                inputManager.InputActions.Gameplay.Click.canceled -= OnClickReleased;
                inputManager.InputActions.Gameplay.Point.performed -= OnPointerMove;
            }
        }
        
        private void OnPointerMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!isDragging)
                return;
            
            currentMousePosition = context.ReadValue<Vector2>();
            
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = currentMousePosition
            };
            placementController.UpdatePlacement(eventData);
        }
        
        private void OnClickPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (placementController.IsPlacing)
                return;
            
            currentMousePosition = inputManager.InputActions.Gameplay.Point.ReadValue<Vector2>();
            
            var ray = mainCamera.ScreenPointToRay(currentMousePosition);
            
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, gridObjectLayer))
            {
                var segment = hit.collider.GetComponentInParent<BridgeSegment>();
                
                if (segment != null && segment.IsPlaced)
                {
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
            
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = currentMousePosition
            };
            
            placementController.BeginMove(selectedSegment, eventData);
        }
        
        private void EndDrag()
        {
            if (selectedSegment == null || !isDragging)
                return;
            
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = currentMousePosition
            };
            
            var success = placementController.CompleteMove(eventData);
            
            selectedSegment = null;
            isDragging = false;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using ModularBridge.Core;
using ModularBridge.Grid;

namespace ModularBridge.Bridge
{
    public class BridgePlacementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private GridSystem gridSystem;
        [SerializeField] private GameSettings gameSettings;
        [SerializeField] private Input.InputManager inputManager;
        
        [Header("Preview Settings")]
        [SerializeField] private Transform previewContainer;
        
        private BridgeSegment activeSegmentInstance;
        private bool isPlacing = false;
        
        private bool isDraggingExisting = false;
        private Vector3Int originalGridPosition;
        private Bridge originalBridge;
        
        private List<GameObject> previewSegments = new List<GameObject>();
        private BridgeSegment potentialConnectionTarget;
        
        private Vector3Int currentGridPosition;
        private Vector3Int lastGridPosition;
        
        private Vector2 mousePosition;
        
        public bool IsPlacing => isPlacing;
        
        private void Awake()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            if (gridSystem == null)
                throw new System.Exception("[BridgePlacementController] GridSystem not assigned!");
            
            if (gameSettings == null)
                throw new System.Exception("[BridgePlacementController] GameSettings not assigned!");
            
            if (previewContainer == null)
                throw new System.Exception("[BridgePlacementController] PreviewContainer not assigned!");
            
            if (inputManager == null)
                throw new System.Exception("[BridgePlacementController] InputManager not assigned!");
        }
        
        private void OnEnable()
        {
            if (inputManager != null && inputManager.InputActions != null)
            {
                inputManager.InputActions.Gameplay.Point.performed += OnPointerMove;
            }
        }
        
        private void OnDisable()
        {
            if (inputManager != null && inputManager.InputActions != null)
            {
                inputManager.InputActions.Gameplay.Point.performed -= OnPointerMove;
            }
        }
        
        private void OnPointerMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!isPlacing || activeSegmentInstance == null)
                return;
            
            mousePosition = context.ReadValue<Vector2>();
            UpdateDragPosition();
        }
        
        public void BeginPlacement(BridgeSegment instantiatedSegment, PointerEventData eventData)
        {
            if (isPlacing)
            {
                CancelPlacement();
            }
            
            activeSegmentInstance = instantiatedSegment;
            isPlacing = true;
            mousePosition = eventData.position;
            
            activeSegmentInstance.transform.SetParent(previewContainer);
            
            lastGridPosition = Vector3Int.one * int.MinValue;
        }
        
        public void BeginMove(BridgeSegment existingSegment, PointerEventData eventData)
        {
            if (isPlacing)
            {
                CancelPlacement();
            }
            
            if (!existingSegment.IsPlaced)
                return;
            
            originalGridPosition = existingSegment.GridPosition;
            originalBridge = existingSegment.ParentBridge;
            
            if (originalBridge != null)
            {
                Game.Instance.Bridges.BreakBridge(originalBridge, keepStartEnd: true);
            }
            
            existingSegment.Remove();
            
            activeSegmentInstance = existingSegment;
            isPlacing = true;
            isDraggingExisting = true;
            mousePosition = eventData.position;
            
            activeSegmentInstance.transform.SetParent(previewContainer);
            
            lastGridPosition = Vector3Int.one * int.MinValue;
        }
        
        public void UpdatePlacement(PointerEventData eventData)
        {
            if (!isPlacing)
                return;
            
            mousePosition = eventData.position;
        }
        
        public bool CompletePlacement(PointerEventData eventData)
        {
            if (!isPlacing || activeSegmentInstance == null)
                return false;
            
            mousePosition = eventData.position;
            UpdateDragPosition();
            
            var canPlace = gridSystem.CanPlaceObject(currentGridPosition, activeSegmentInstance);
            
            if (!canPlace)
            {
                if (!isDraggingExisting)
                {
                    CancelPlacement();
                }
                return false;
            }
            
            activeSegmentInstance.transform.SetParent(null);
            
            if (activeSegmentInstance.TryPlace(currentGridPosition))
            {
                ClearPreviewSegments();
                potentialConnectionTarget = null;
                activeSegmentInstance = null;
                isPlacing = false;
                isDraggingExisting = false;
                
                return true;
            }
            else
            {
                if (!isDraggingExisting)
                {
                    CancelPlacement();
                }
                return false;
            }
        }
        
        public void CancelPlacement()
        {
            if (!isPlacing)
                return;
            
            isPlacing = false;
            
            if (activeSegmentInstance != null)
            {
                Destroy(activeSegmentInstance.gameObject);
                activeSegmentInstance = null;
            }
            
            ClearPreviewSegments();
            potentialConnectionTarget = null;
        }
        
        public bool CompleteMove(PointerEventData eventData)
        {
            if (!isPlacing || !isDraggingExisting || activeSegmentInstance == null)
                return false;
            
            mousePosition = eventData.position;
            UpdateDragPosition();
            
            var canPlace = gridSystem.CanPlaceObject(currentGridPosition, activeSegmentInstance);
            
            if (canPlace && currentGridPosition != originalGridPosition)
            {
                activeSegmentInstance.transform.SetParent(null);
                
                if (activeSegmentInstance.TryPlace(currentGridPosition))
                {
                    ClearPreviewSegments();
                    potentialConnectionTarget = null;
                    activeSegmentInstance = null;
                    isPlacing = false;
                    isDraggingExisting = false;
                    originalBridge = null;
                    
                    return true;
                }
            }
            activeSegmentInstance.transform.SetParent(null);
            activeSegmentInstance.TryPlace(originalGridPosition);
            
            // Clear state
            ClearPreviewSegments();
            potentialConnectionTarget = null;
            activeSegmentInstance = null;
            isPlacing = false;
            isDraggingExisting = false;
            originalBridge = null;
            
            return false;
        }
        
        public void CancelMove()
        {
            if (!isPlacing || !isDraggingExisting)
                return;
            
            if (activeSegmentInstance != null)
            {
                activeSegmentInstance.transform.SetParent(null);
                activeSegmentInstance.TryPlace(originalGridPosition);
            }
            
            ClearPreviewSegments();
            potentialConnectionTarget = null;
            activeSegmentInstance = null;
            isPlacing = false;
            isDraggingExisting = false;
            originalBridge = null;
        }
        
        private void UpdateDragPosition()
        {
            var ray = mainCamera.ScreenPointToRay(mousePosition);
            
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer))
                return;
            
            var newGridPos = gridSystem.WorldToGrid(hit.point);
            
            if (newGridPos == lastGridPosition)
                return;
            
            currentGridPosition = newGridPos;
            lastGridPosition = newGridPos;
            
            UpdatePreview();
        }
        
        private void UpdatePreview()
        {
            var canPlace = gridSystem.CanPlaceObject(currentGridPosition, activeSegmentInstance);
            
            activeSegmentInstance.ShowPlacementPreview(currentGridPosition, canPlace);
            
            ClearPreviewSegments();
            
            potentialConnectionTarget = FindPotentialConnection();
            
            if (potentialConnectionTarget != null && canPlace)
            {
                ShowBridgePreview();
            }
        }
        
        private BridgeSegment FindPotentialConnection()
        {
            if (activeSegmentInstance.Type != BridgeSegment.SegmentType.Start &&
                activeSegmentInstance.Type != BridgeSegment.SegmentType.End)
            {
                return null;
            }
            
            var lookingFor = BridgeSegment.SegmentType.End;
            if (activeSegmentInstance.Type == BridgeSegment.SegmentType.End)
            {
                lookingFor = BridgeSegment.SegmentType.Start;
            }
            var candidates = gridSystem.Registry.GetObjectsOfType<BridgeSegment>();
            
            foreach (var candidate in candidates)
            {
                if (candidate.Type != lookingFor)
                    continue;
                
                if (!candidate.IsPlaced)
                    continue;
                
                var candidatePos = candidate.GridPosition;
                
                var sameX = candidatePos.x == currentGridPosition.x;
                var sameZ = candidatePos.z == currentGridPosition.z;
                
                if (sameX || sameZ)
                {
                    return candidate;
                }
            }
            
            return null;
        }
        
        private void ShowBridgePreview()
        {
            var startPos = activeSegmentInstance.Type == BridgeSegment.SegmentType.Start
                ? currentGridPosition
                : potentialConnectionTarget.GridPosition;
            
            var endPos = activeSegmentInstance.Type == BridgeSegment.SegmentType.End
                ? currentGridPosition
                : potentialConnectionTarget.GridPosition;
            
            var plan = BridgeBuilder.CalculateBridge(startPos, endPos, gameSettings);
            
            if (!plan.IsValid)
                return;
            
            var settings = gameSettings.BridgeSettings;
            
            for (int i = 1; i < plan.Placements.Count - 1; i++)
            {
                var placement = plan.Placements[i];
                
                var prefab = settings.GetPrefabForType(placement.Type);
                if (prefab == null)
                    continue;
                
                var previewObj = Instantiate(prefab.gameObject, previewContainer);
                previewObj.transform.position = gridSystem.GridToWorld(placement.GridPosition);
                previewObj.transform.rotation = placement.Rotation;
                
                var previewSegment = previewObj.GetComponent<BridgeSegment>();
                if (previewSegment != null)
                {
                    previewSegment.ShowPlacementPreview(placement.GridPosition, true);
                }
                
                previewSegments.Add(previewObj);
            }
        }
        
        private void ClearPreviewSegments()
        {
            foreach (var obj in previewSegments)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            previewSegments.Clear();
        }
    }
}
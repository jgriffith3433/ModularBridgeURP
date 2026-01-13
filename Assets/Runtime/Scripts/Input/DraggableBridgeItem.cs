using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ModularBridge.Bridge;

namespace ModularBridge.Input
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableBridgeItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Bridge Segment Configuration")]
        [Tooltip("The bridge segment prefab to spawn when this UI item is dragged")]
        [SerializeField] private BridgeSegment bridgeSegmentPrefab;
        
        [Header("Drag Drop Manager")]
        [SerializeField] private BridgeDragDropManager dragDropManager;
        
        [Header("Visual Feedback")]

        [SerializeField] private float dragAlpha = 0.6f;
        
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Canvas canvas;
        private Vector2 originalPosition;
        private Transform originalParent;
        
        public BridgeSegment BridgeSegmentPrefab => bridgeSegmentPrefab;
        
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            
            if (bridgeSegmentPrefab == null)
            {
            }
            
            if (dragDropManager == null)
            {
                throw new System.Exception($"[DraggableBridgeItem] BridgeDragDropManager not assigned to {gameObject.name}!");
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (bridgeSegmentPrefab == null)
                return;
            
            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;
            
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
            
            dragDropManager.OnBeginDragFromUI(this, eventData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (bridgeSegmentPrefab == null)
                return;
            
            if (canvas != null)
            {
                rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            }
            
            dragDropManager.OnDragUpdate(eventData);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (bridgeSegmentPrefab == null)
                return;
            
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            rectTransform.anchoredPosition = originalPosition;
            
            dragDropManager.OnEndDrag(eventData);
        }
    }
}

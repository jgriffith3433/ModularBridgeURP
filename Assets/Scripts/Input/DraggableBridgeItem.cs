using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Makes a UI element draggable and allows it to spawn a 3D bridge segment when dragged.
/// Attach this to UI buttons/images in your bridge inventory panel.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class DraggableBridgeItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Bridge Segment Configuration")]
    [Tooltip("The bridge segment prefab to spawn when this UI item is dragged")]
    [SerializeField] private BridgeSegment bridgeSegmentPrefab;
    
    [Header("Drag Drop Manager")]
    [SerializeField] private BridgeDragDropManager dragDropManager;
    
    [Header("Visual Feedback")]
    [Tooltip("Optional icon to display while dragging")]
    [SerializeField] private Sprite dragIcon;
    [Tooltip("Alpha value while dragging")]
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
            Debug.LogError($"[DraggableBridgeItem] No bridge segment prefab assigned to {gameObject.name}");
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
        
        // Store original position and parent
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Make semi-transparent during drag
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
        
        // Notify the drag and drop manager
        dragDropManager.OnBeginDragFromUI(this, eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (bridgeSegmentPrefab == null)
            return;
        
        // Move the UI element to follow cursor
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
        
        // Notify the drag and drop manager
        dragDropManager.OnDragUpdate(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (bridgeSegmentPrefab == null)
            return;
        
        // Restore UI element
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        rectTransform.anchoredPosition = originalPosition;
        
        // Notify the drag and drop manager
        dragDropManager.OnEndDrag(eventData);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ModularBridge.Grid;

namespace ModularBridge.Input
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DraggableGridItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Grid Object Configuration")]
        [Tooltip("The grid object prefab to spawn when this UI item is dragged")]
        [SerializeField] private GridObject gridObjectPrefab;
        
        [Header("Drag Drop Manager")]
        [SerializeField] private GridObjectDragDropManager dragDropManager;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color draggingColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        
        private CanvasGroup canvasGroup;
        private bool isEnabled = true;
        
        public GridObject GridObjectPrefab => gridObjectPrefab;
        public bool IsEnabled => isEnabled;
        
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }
            
            if (gridObjectPrefab == null)
            {
            }
            
            if (dragDropManager == null)
            {
                throw new System.Exception($"[DraggableGridItem] GridObjectDragDropManager not assigned to {gameObject.name}!");
            }
            
            UpdateVisuals();
        }
        
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (iconImage == null) return;
            
            if (!isEnabled)
            {
                iconImage.color = disabledColor;
                canvasGroup.interactable = false;
            }
            else
            {
                iconImage.color = normalColor;
                canvasGroup.interactable = true;
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (gridObjectPrefab == null || !isEnabled)
                return;
            
            if (iconImage != null)
            {
                iconImage.color = draggingColor;
            }
            
            dragDropManager.OnBeginDragFromUI(this, eventData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (gridObjectPrefab == null || !isEnabled)
                return;
            
            dragDropManager.OnDragUpdate(eventData);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (gridObjectPrefab == null)
                return;
            
            if (iconImage != null)
            {
                iconImage.color = isEnabled ? normalColor : disabledColor;
            }
            
            dragDropManager.OnEndDrag(eventData);
        }
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A simple helper class you can use to trigger methods on Unity's pointer events
    /// Typically used on a UI Image
    /// </summary>
    public class MMOnPointer : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Tooltip("an event to trigger when the pointer enters the associated game object")]
        public UnityEvent PointerEnter;

        [Tooltip("an event to trigger when the pointer exits the associated game object")]
        public UnityEvent PointerExit;

        [Tooltip("an event to trigger when the pointer is pressed down on the associated game object")]
        public UnityEvent PointerDown;

        [Tooltip("an event to trigger when the pointer is pressed up on the associated game object")]
        public UnityEvent PointerUp;

        [Tooltip("an event to trigger when the pointer is clicked on the associated game object")]
        public UnityEvent PointerClick;

        public void OnPointerEnter(PointerEventData eventData) => PointerEnter?.Invoke();
        public void OnPointerExit(PointerEventData eventData) => PointerExit?.Invoke();
        public void OnPointerDown(PointerEventData eventData) => PointerDown?.Invoke();
        public void OnPointerUp(PointerEventData eventData) => PointerUp?.Invoke();
        public void OnPointerClick(PointerEventData eventData) => PointerClick?.Invoke();
    }
}
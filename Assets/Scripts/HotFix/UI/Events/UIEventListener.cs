using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains
{
    [Serializable]
    public sealed class PointerEventUnityEvent : UnityEvent<PointerEventData>
    {
    }

    [Serializable]
    public sealed class DragReleaseUnityEvent : UnityEvent<UIDragReleaseEventData>
    {
    }

    /// <summary>
    /// Snapshot of all UI raycast targets under the pointer when a drag finishes.
    /// Results are ordered in the same way as EventSystem.RaycastAll (frontmost first).
    /// </summary>
    public sealed class UIDragReleaseEventData : ClassObject, IArgs<PointerEventData, List<RaycastResult>>
    {
        List<RaycastResult> _raycastResults = new();
        List<GameObject> _gameObjects = new();

        public PointerEventData PointerEventData;
        public IReadOnlyList<RaycastResult> RaycastResults => _raycastResults;
        public IReadOnlyList<GameObject> GameObjects => _gameObjects;
        public GameObject TopmostGameObject => _gameObjects.Count > 0 ? _gameObjects[0] : null;

        public override void resetProperty()
        {
            base.resetProperty();
            PointerEventData = null;
            //_raycastResults = null;
            //_gameObjects = null;
            _raycastResults.Clear();
            _gameObjects.Clear();
        }

        public void onCreate(PointerEventData pointerEventData, List<RaycastResult> raycastResults)
        {
            PointerEventData = pointerEventData;
            _raycastResults.Clear();
            _raycastResults.AddRange(raycastResults);

            _gameObjects.Clear();
            using var _ = new HashSetScope<GameObject>(out var visitedObjects);
            for (var i = 0; i < _raycastResults.Count; i++)
            {
                var target = _raycastResults[i].gameObject;
                if (target && visitedObjects.Add(target))
                {
                    _gameObjects.Add(target);
                }
            }
        }
    }

    /// <summary>
    /// General-purpose callback component for Unity UI pointer, hover and drag events.
    /// The target needs a Raycast Graphic (or another active BaseRaycaster target),
    /// and the scene needs an EventSystem with a compatible input module.
    /// </summary>
    [DisallowMultipleComponent]
    public class UIEventListener : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IPointerMoveHandler,
        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IDropHandler,
        IScrollHandler,
        ISelectHandler,
        IDeselectHandler,
        ISubmitHandler,
        ICancelHandler,
        IMoveHandler
    {
        /*[Header("Pointer")]
        [SerializeField] PointerEventUnityEvent _onPointerEnter = new();
        [SerializeField] PointerEventUnityEvent _onPointerExit = new();
        [SerializeField] PointerEventUnityEvent _onPointerDown = new();
        [SerializeField] PointerEventUnityEvent _onPointerUp = new();
        [SerializeField] PointerEventUnityEvent _onPointerClick = new();
        [SerializeField] PointerEventUnityEvent _onPointerMove = new();
        [SerializeField] PointerEventUnityEvent _onPointerHover = new();

        [Header("Drag And Drop")]
        [SerializeField] PointerEventUnityEvent _onInitializePotentialDrag = new();
        [SerializeField] PointerEventUnityEvent _onBeginDrag = new();
        [SerializeField] PointerEventUnityEvent _onDrag = new();
        [SerializeField] PointerEventUnityEvent _onEndDrag = new();
        [SerializeField] PointerEventUnityEvent _onDrop = new();
        [SerializeField] DragReleaseUnityEvent _onDragReleasedOverUI = new();

        [Header("Other EventSystem Events")]
        [SerializeField] PointerEventUnityEvent _onScroll = new();
        [SerializeField] UnityEvent<BaseEventData> _onSelect = new();
        [SerializeField] UnityEvent<BaseEventData> _onDeselect = new();
        [SerializeField] UnityEvent<BaseEventData> _onSubmit = new();
        [SerializeField] UnityEvent<BaseEventData> _onCancel = new();
        [SerializeField] UnityEvent<AxisEventData> _onMove = new();*/

        Dictionary<int, PointerEventData> _hoveringPointers = new();
        List<PointerEventData> _hoverSnapshot = new();
        List<RaycastResult> _raycastResults = new();

        /*public PointerEventUnityEvent OnPointerEnterEvent => _onPointerEnter;
        public PointerEventUnityEvent OnPointerExitEvent => _onPointerExit;
        public PointerEventUnityEvent OnPointerDownEvent => _onPointerDown;
        public PointerEventUnityEvent OnPointerUpEvent => _onPointerUp;
        public PointerEventUnityEvent OnPointerClickEvent => _onPointerClick;
        public PointerEventUnityEvent OnPointerMoveEvent => _onPointerMove;
        public PointerEventUnityEvent OnPointerHoverEvent => _onPointerHover;
        public PointerEventUnityEvent OnInitializePotentialDragEvent => _onInitializePotentialDrag;
        public PointerEventUnityEvent OnBeginDragEvent => _onBeginDrag;
        public PointerEventUnityEvent OnDragEvent => _onDrag;
        public PointerEventUnityEvent OnEndDragEvent => _onEndDrag;
        public PointerEventUnityEvent OnDropEvent => _onDrop;
        public DragReleaseUnityEvent OnDragReleasedOverUIEvent => _onDragReleasedOverUI;
        public PointerEventUnityEvent OnScrollEvent => _onScroll;*/

        public Action<PointerEventData> PointerEntered;
        public Action<PointerEventData> PointerExited;
        public Action<PointerEventData> PointerPressed;
        public Action<PointerEventData> PointerReleased;
        public Action<PointerEventData> PointerClicked;
        public Action<PointerEventData> PointerMoved;
        public Action<PointerEventData> PointerHovered;
        public Action<PointerEventData> PotentialDragInitialized;
        public Action<PointerEventData> DragStarted;
        public Action<PointerEventData> Dragging;
        public Action<PointerEventData> DragEnded;
        public Action<PointerEventData> Dropped;
        public Action<UIDragReleaseEventData> DragReleasedOverUI;
        public Action<PointerEventData> Scrolled;
        public Action<BaseEventData> Selected;
        public Action<BaseEventData> Deselected;
        public Action<BaseEventData> Submitted;
        public Action<BaseEventData> Cancelled;
        public Action<AxisEventData> Moved;

        protected virtual void Update()
        {
            if (_hoveringPointers.Count == 0)
            {
                return;
            }

            _hoverSnapshot.Clear();
            _hoverSnapshot.AddRange(_hoveringPointers.Values);
            for (var i = 0; i < _hoverSnapshot.Count; i++)
            {
                var pointerEventData = _hoverSnapshot[i];
                // _onPointerHover.Invoke(pointerEventData);
                PointerHovered?.Invoke(pointerEventData);
            }
            _hoverSnapshot.Clear();
        }

        protected virtual void OnDisable()
        {
            _hoveringPointers.Clear();
            _hoverSnapshot.Clear();
            _raycastResults.Clear();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _hoveringPointers[eventData.pointerId] = eventData;
            //_onPointerEnter.Invoke(eventData);
            PointerEntered?.Invoke(eventData);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            _hoveringPointers.Remove(eventData.pointerId);
            //_onPointerExit.Invoke(eventData);
            PointerExited?.Invoke(eventData);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            //_onPointerDown.Invoke(eventData);
            PointerPressed?.Invoke(eventData);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            //_onPointerUp.Invoke(eventData);
            PointerReleased?.Invoke(eventData);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            //_onPointerClick.Invoke(eventData);
            PointerClicked?.Invoke(eventData);
        }

        public virtual void OnPointerMove(PointerEventData eventData)
        {
            _hoveringPointers[eventData.pointerId] = eventData;
            //_onPointerMove.Invoke(eventData);
            PointerMoved?.Invoke(eventData);
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            //_onInitializePotentialDrag.Invoke(eventData);
            PotentialDragInitialized?.Invoke(eventData);
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            //_onBeginDrag.Invoke(eventData);
            DragStarted?.Invoke(eventData);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            //_onDrag.Invoke(eventData);
            Dragging?.Invoke(eventData);
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            //_onEndDrag.Invoke(eventData);
            DragEnded?.Invoke(eventData);

            using var _ = new ClassScope<UIDragReleaseEventData>(out var releaseEventData);
            CreateDragReleaseEventData(eventData, ref releaseEventData);
            //_onDragReleasedOverUI.Invoke(releaseEventData);
            DragReleasedOverUI?.Invoke(releaseEventData);
        }

        public virtual void OnDrop(PointerEventData eventData)
        {
            //_onDrop.Invoke(eventData);
            Dropped?.Invoke(eventData);
        }

        public virtual void OnScroll(PointerEventData eventData)
        {
            //_onScroll.Invoke(eventData);
            Scrolled?.Invoke(eventData);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            //_onSelect.Invoke(eventData);
            Selected?.Invoke(eventData);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            //_onDeselect.Invoke(eventData);
            Deselected?.Invoke(eventData);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            //_onSubmit.Invoke(eventData);
            Submitted?.Invoke(eventData);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            //_onCancel.Invoke(eventData);
            Cancelled?.Invoke(eventData);
        }

        public virtual void OnMove(AxisEventData eventData)
        {
            //_onMove.Invoke(eventData);
            Moved?.Invoke(eventData);
        }

        void CreateDragReleaseEventData(PointerEventData eventData,ref UIDragReleaseEventData releaseEventData)
        {
            _raycastResults.Clear();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, _raycastResults);
            for (var i = _raycastResults.Count - 1; i >= 0; i--)
            {
                if (_raycastResults[i].module is not GraphicRaycaster)
                {
                    _raycastResults.RemoveAt(i);
                }
            }

            releaseEventData.with(eventData, _raycastResults);
            _raycastResults.Clear();
        }


        public void SetOnPointerEntered(Action<PointerEventData> action) => PointerEntered = action;
        public void SetOnPointerExited(Action<PointerEventData> action) => PointerExited = action;
        public void SetOnPointerPressed(Action<PointerEventData> action) => PointerPressed = action;
        public void SetOnPointerReleased(Action<PointerEventData> action) => PointerReleased = action;
        public void SetOnPointerClicked(Action<PointerEventData> action) => PointerClicked = action;
        public void SetOnPointerMoved(Action<PointerEventData> action) => PointerMoved = action;
        public void SetOnPointerHovered(Action<PointerEventData> action) => PointerHovered = action;
        public void SetOnPotentialDragInitialized(Action<PointerEventData> action) => PotentialDragInitialized = action;
        public void SetOnDragStarted(Action<PointerEventData> action) => DragStarted = action;
        public void SetOnDragging(Action<PointerEventData> action) => Dragging = action;
        public void SetOnDragEnded(Action<PointerEventData> action) => DragEnded = action;
        public void SetOnDropped(Action<PointerEventData> action) => Dropped = action;
        public void SetOnDragReleasedOverUI(Action<UIDragReleaseEventData> action) => DragReleasedOverUI = action;
        public void SetOnScrolled(Action<PointerEventData> action) => Scrolled = action;
        public void SetOnSelected(Action<BaseEventData> action) => Selected = action;
        public void SetOnDeselected(Action<BaseEventData> action) => Deselected = action;
        public void SetOnSubmitted(Action<BaseEventData> action) => Submitted = action;
        public void SetOnCancelled(Action<BaseEventData> action) => Cancelled = action;
        public void SetOnMoved(Action<AxisEventData> action) => Moved = action;
    }
}

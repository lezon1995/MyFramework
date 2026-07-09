using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
    [Serializable]
    public class AxisEvent : UnityEvent<float>
    {
    }

    /// <summary>
    /// Add this component to a GUI Image to have it act as an axis. 
    /// Bind pressed down, pressed continually and released actions to it from the inspector
    /// Handles mouse and multi touch
    /// </summary>
    [RequireComponent(typeof(Rect))]
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("More Mountains/Tools/Controls/MMTouchAxis")]
    public class MMTouchAxis : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerExitHandler,
        IPointerEnterHandler
    {
        public enum States
        {
            Off,
            Down,
            Pressed,
            Up
        }

        [Header("Binding")]
        [Tooltip("The method(s) to call when the axis gets pressed down")]
        public UnityEvent AxisPressedFirstTime;

        [Tooltip("The method(s) to call when the axis gets released")]
        public UnityEvent AxisReleased;

        [Tooltip("The method(s) to call while the axis is being pressed")]
        public AxisEvent AxisPressed;

        [Header("Pressed Behaviour")]
        [MMInformation("Here you can set the opacity of the button when it's pressed. Useful for visual feedback.")]
        [Tooltip("the new opacity to apply to the canvas group when the axis is pressed")]
        public float PressedOpacity = 0.5f;

        [Tooltip("the value to send the bound method when the axis is pressed")]
        public float AxisValue;

        [Header("Mouse Mode")]
        [MMInformation("If you set this to true, you'll need to actually press the axis for it to be triggered, otherwise a simple hover will trigger it (better for touch input).")]
        [Tooltip("If you set this to true, you'll need to actually press the axis for it to be triggered, otherwise a simple hover will trigger it (better for touch input).")]
        public bool MouseMode;

        public virtual States CurrentState { get; protected set; }

        protected CanvasGroup _canvasGroup;
        protected float _initialOpacity;

        /// <summary>
        /// On Start, we get our canvas group and set our initial alpha
        /// </summary>
        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup)
            {
                _initialOpacity = _canvasGroup.alpha;
            }

            ResetButton();
        }

        /// <summary>
        /// Every frame, if the touch zone is pressed, we trigger the bound method if it exists
        /// </summary>
        protected virtual void Update()
        {
            if (AxisPressed != null)
            {
                if (CurrentState == States.Pressed)
                {
                    AxisPressed.Invoke(AxisValue);
                }
            }
        }

        /// <summary>
        /// At the end of every frame, we change our button's state if needed
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (CurrentState == States.Up)
                CurrentState = States.Off;

            if (CurrentState == States.Down)
                CurrentState = States.Pressed;
        }

        /// <summary>
        /// Triggers the bound pointer down action
        /// </summary>
        public virtual void OnPointerDown(PointerEventData data)
        {
            if (CurrentState != States.Off)
                return;

            CurrentState = States.Down;
            if (_canvasGroup)
                _canvasGroup.alpha = PressedOpacity;

            AxisPressedFirstTime?.Invoke();
        }

        /// <summary>
        /// Triggers the bound pointer up action
        /// </summary>
        public virtual void OnPointerUp(PointerEventData data)
        {
            if (CurrentState != States.Pressed && CurrentState != States.Down)
                return;

            CurrentState = States.Up;
            if (_canvasGroup)
            {
                _canvasGroup.alpha = _initialOpacity;
            }

            AxisReleased?.Invoke();
            AxisPressed?.Invoke(0);
        }

        /// <summary>
        /// OnEnable, we reset our button state
        /// </summary>
        protected virtual void OnEnable()
        {
            ResetButton();
        }

        /// <summary>
        /// Resets the button's state and opacity
        /// </summary>
        protected virtual void ResetButton()
        {
            CurrentState = States.Off;
            _canvasGroup.alpha = _initialOpacity;
            CurrentState = States.Off;
        }

        /// <summary>
        /// Triggers the bound pointer enter action when touch enters zone
        /// </summary>
        public virtual void OnPointerEnter(PointerEventData data)
        {
            if (!MouseMode)
                OnPointerDown(data);
        }

        /// <summary>
        /// Triggers the bound pointer exit action when touch is out of zone
        /// </summary>
        public virtual void OnPointerExit(PointerEventData data)
        {
            if (!MouseMode)
                OnPointerUp(data);
        }
    }
}
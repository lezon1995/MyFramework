using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this component to a GUI Image to have it act as a button. 
    /// Bind pressed down, pressed continually and released actions to it from the inspector
    /// Handles mouse and multi touch
    /// </summary>
    [RequireComponent(typeof(Rect))]
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("More Mountains/Tools/Controls/MMTouchButton")]
    public class MMTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, ISubmitHandler
    {
        // whether or not this button can be interacted with
        [Header("Interaction")]
        public bool Interactable = true;

        /// The different possible states for the button : 
        /// Off (default idle state), ButtonDown (button pressed for the first time), ButtonPressed (button being pressed), ButtonUp (button being released), Disabled (unclickable but still present on screen)
        /// ButtonDown and ButtonUp will only last one frame, the others will last however long you press them / disable them / do nothing
        public enum States
        {
            Off,
            Down,
            Pressed,
            Up,
            Disabled
        }

        [Header("Binding")]
        [Tooltip("The method(s) to call when the button gets pressed down")]
        public UnityEvent ButtonPressedFirstTime;

        [Tooltip("The method(s) to call when the button gets released")]
        public UnityEvent ButtonReleased;

        [Tooltip("The method(s) to call while the button is being pressed")]
        public UnityEvent ButtonPressed;

        [Header("Sprite Swap")]
        [MMInformation("Here you can define, for disabled and pressed states, if you want a different sprite, and a different color.")]
        [Tooltip("the sprite to use on the button when it's in the disabled state")]
        public Sprite DisabledSprite;

        [Tooltip("whether or not to change color when the button is disabled")]
        public bool DisabledChangeColor;

        [Tooltip("the color to use when the button is disabled")]
        [MMCondition("DisabledChangeColor", true)]
        public Color DisabledColor = Color.white;

        [Tooltip("the sprite to use on the button when it's in the pressed state")]
        public Sprite PressedSprite;

        [Tooltip("whether or not to change the button color on press")]
        public bool PressedChangeColor;

        [Tooltip("the color to use when the button is pressed")]
        [MMCondition("PressedChangeColor", true)]
        public Color PressedColor = Color.white;

        [Tooltip("the sprite to use on the button when it's in the highlighted state")]
        public Sprite HighlightedSprite;

        [Tooltip("whether or not to change color when highlighting the button")]
        public bool HighlightedChangeColor;

        [Tooltip("the color to use when the button is highlighted")]
        [MMCondition("HighlightedChangeColor", true)]
        public Color HighlightedColor = Color.white;

        [Header("Opacity")]
        [MMInformation("Here you can set different opacities for the button when it's pressed, idle, or disabled. Useful for visual feedback.")]
        [Tooltip("the opacity to apply to the canvas group when the button is pressed")]
        public float PressedOpacity = 1f;

        [Tooltip("the new opacity to apply to the canvas group when the button is idle")]
        public float IdleOpacity = 1f;

        [Tooltip("the new opacity to apply to the canvas group when the button is disabled")]
        public float DisabledOpacity = 1f;

        [Header("Delays")]
        [MMInformation("Specify here the delays to apply when the button is pressed initially, and when it gets released. Usually you'll keep them at 0.")]
        [Tooltip("the delay to apply to events when the button gets pressed for the first time")]
        public float PressedFirstTimeDelay;

        [Tooltip("the delay to apply to events when the button gets released")]
        public float ReleasedDelay;

        [Header("Buffer")]
        [Tooltip("the duration (in seconds) after a press during which the button can't be pressed again")]
        public float BufferDuration;

        [Header("Animation")]
        [MMInformation("Here you can bind an animator, and specify animation parameter names for the various states.")]
        [Tooltip("an animator you can bind to this button to have its states updated to reflect the button's states")]
        public Animator Animator;

        [Tooltip("the name of the animation parameter to turn true when the button is idle")]
        public string IdleAnimationParameterName = "Idle";

        [Tooltip("the name of the animation parameter to turn true when the button is disabled")]
        public string DisabledAnimationParameterName = "Disabled";

        [Tooltip("the name of the animation parameter to turn true when the button is pressed")]
        public string PressedAnimationParameterName = "Pressed";

        [Header("Mouse Mode")]
        [MMInformation("If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better to leave it unchecked if you're going for touch input).")]
        [Tooltip("If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).")]
        public bool MouseMode;

        public bool ReturnToInitialSpriteAutomatically { get; set; }

        /// the current state of the button (off, down, pressed or up)
        public States State { get; protected set; }

        public event Action<PointerEventData.FramePressState, PointerEventData> ButtonStateChange;

        protected bool _zonePressed = false;
        protected CanvasGroup _canvasGroup;
        protected float _initialOpacity;
        protected Animator _animator;
        protected Image _image;
        protected Sprite _initialSprite;
        protected Color _initialColor;
        protected float _lastClickTimestamp;
        protected Selectable _selectable;

        /// <summary>
        /// On Start, we get our canvas group and set our initial alpha
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
        }

        /// <summary>
        /// On init we grab our Image, Animator and CanvasGroup and set them up
        /// </summary>
        protected virtual void Initialization()
        {
            ReturnToInitialSpriteAutomatically = true;

            _selectable = GetComponent<Selectable>();

            _image = GetComponent<Image>();
            if (_image)
            {
                _initialColor = _image.color;
                _initialSprite = _image.sprite;
            }

            _animator = GetComponent<Animator>();
            if (Animator)
                _animator = Animator;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup)
            {
                _initialOpacity = IdleOpacity;
                _canvasGroup.alpha = _initialOpacity;
                _initialOpacity = _canvasGroup.alpha;
            }

            ResetButton();
        }

        /// <summary>
        /// Every frame, if the touch zone is pressed, we trigger the OnPointerPressed method, to detect continuous press
        /// </summary>
        protected virtual void Update()
        {
            switch (State)
            {
                case States.Off:
                    SetOpacity(IdleOpacity);
                    if (_image && ReturnToInitialSpriteAutomatically)
                    {
                        _image.color = _initialColor;
                        _image.sprite = _initialSprite;
                    }

                    if (_selectable)
                    {
                        _selectable.interactable = true;
                        if (EventSystem.current.currentSelectedGameObject == gameObject)
                        {
                            if (_image && HighlightedChangeColor)
                                _image.color = HighlightedColor;

                            if (HighlightedSprite)
                                _image.sprite = HighlightedSprite;
                        }
                    }

                    break;

                case States.Disabled:
                    SetOpacity(DisabledOpacity);
                    if (_image)
                    {
                        if (DisabledSprite)
                            _image.sprite = DisabledSprite;

                        if (DisabledChangeColor)
                            _image.color = DisabledColor;
                    }

                    if (_selectable)
                        _selectable.interactable = false;

                    break;

                case States.Down:
                    break;
                case States.Pressed:
                    SetOpacity(PressedOpacity);
                    OnPointerPressed();
                    if (_image)
                    {
                        if (PressedSprite)
                            _image.sprite = PressedSprite;

                        if (PressedChangeColor)
                            _image.color = PressedColor;
                    }

                    break;
                case States.Up:
                    break;
            }

            UpdateAnimatorStates();
        }

        /// <summary>
        /// At the end of every frame, we change our button's state if needed
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (State == States.Up)
                State = States.Off;

            if (State == States.Down)
                State = States.Pressed;
        }

        /// <summary>
        /// Triggers the ButtonStateChange event for the specified state
        /// </summary>
        /// <param name="newState"></param>
        /// <param name="data"></param>
        public virtual void InvokeButtonStateChange(PointerEventData.FramePressState newState, PointerEventData data)
        {
            ButtonStateChange?.Invoke(newState, data);
        }

        /// <summary>
        /// Triggers the bound pointer down action
        /// </summary>
        public virtual void OnPointerDown(PointerEventData data)
        {
            if (!Interactable)
                return;

            if (Time.time - _lastClickTimestamp < BufferDuration)
                return;

            if (State != States.Off)
                return;

            State = States.Down;
            _lastClickTimestamp = Time.time;
            InvokeButtonStateChange(PointerEventData.FramePressState.Pressed, data);
            if (Time.timeScale != 0 && PressedFirstTimeDelay > 0)
            {
                Invoke(nameof(InvokePressedFirstTime), PressedFirstTimeDelay);
            }
            else
            {
                ButtonPressedFirstTime.Invoke();
            }
        }

        /// <summary>
        /// Raises the ButtonPressedFirstTime event
        /// </summary>
        protected virtual void InvokePressedFirstTime()
        {
            ButtonPressedFirstTime?.Invoke();
        }

        /// <summary>
        /// Triggers the bound pointer up action
        /// </summary>
        public virtual void OnPointerUp(PointerEventData data)
        {
            if (!Interactable)
                return;

            if (State != States.Pressed && State != States.Down)
                return;

            State = States.Up;
            InvokeButtonStateChange(PointerEventData.FramePressState.Released, data);
            if (Time.timeScale != 0 && ReleasedDelay > 0)
            {
                Invoke(nameof(InvokeReleased), ReleasedDelay);
            }
            else
            {
                ButtonReleased.Invoke();
            }
        }

        /// <summary>
        /// Invokes the ButtonReleased event
        /// </summary>
        protected virtual void InvokeReleased()
        {
            ButtonReleased?.Invoke();
        }

        /// <summary>
        /// Triggers the bound pointer pressed action
        /// </summary>
        public virtual void OnPointerPressed()
        {
            if (!Interactable)
                return;

            State = States.Pressed;
            ButtonPressed?.Invoke();
        }

        /// <summary>
        /// Resets the button's state and opacity
        /// </summary>
        protected virtual void ResetButton()
        {
            SetOpacity(_initialOpacity);
            State = States.Off;
        }

        /// <summary>
        /// Triggers the bound pointer enter action when touch enters zone
        /// </summary>
        public virtual void OnPointerEnter(PointerEventData data)
        {
            if (!Interactable)
                return;

            if (!MouseMode)
                OnPointerDown(data);
        }

        /// <summary>
        /// Triggers the bound pointer exit action when touch is out of zone
        /// </summary>
        public virtual void OnPointerExit(PointerEventData data)
        {
            if (!Interactable)
                return;

            if (!MouseMode)
                OnPointerUp(data);
        }

        /// <summary>
        /// OnEnable, we reset our button state
        /// </summary>
        protected virtual void OnEnable()
        {
            ResetButton();
        }

        /// <summary>
        /// On disable we reset our flags and disable the button
        /// </summary>
        private void OnDisable()
        {
            bool wasActive = State != States.Off && State != States.Disabled && State != States.Up;
            DisableButton();
            State = States.Off;
            if (wasActive)
            {
                InvokeButtonStateChange(PointerEventData.FramePressState.Released, null);
                ButtonReleased?.Invoke();
            }
        }

        /// <summary>
        /// Prevents the button from receiving touches
        /// </summary>
        public virtual void DisableButton()
        {
            State = States.Disabled;
        }

        /// <summary>
        /// Allows the button to receive touches
        /// </summary>
        public virtual void EnableButton()
        {
            if (State == States.Disabled)
                State = States.Off;
        }

        /// <summary>
        /// Sets the canvas group's opacity to the requested value
        /// </summary>
        /// <param name="newOpacity"></param>
        protected virtual void SetOpacity(float newOpacity)
        {
            if (_canvasGroup)
                _canvasGroup.alpha = newOpacity;
        }

        /// <summary>
        /// Updates animator states based on the current state of the button
        /// </summary>
        protected virtual void UpdateAnimatorStates()
        {
            if (_animator == null)
                return;

            if (DisabledAnimationParameterName != null)
            {
                _animator.SetBool(DisabledAnimationParameterName, State == States.Disabled);
            }

            if (PressedAnimationParameterName != null)
            {
                _animator.SetBool(PressedAnimationParameterName, State == States.Pressed);
            }

            if (IdleAnimationParameterName != null)
            {
                _animator.SetBool(IdleAnimationParameterName, State == States.Off);
            }
        }

        /// <summary>
        /// On submit, raises the appropriate events
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnSubmit(BaseEventData eventData)
        {
            ButtonPressedFirstTime?.Invoke();
            ButtonReleased?.Invoke();
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains
{
    /// <summary>
    /// Tooltip触发器组件
    /// 挂载到需要显示Tooltip的UI元素上
    /// 支持IPointerEnterHandler、IPointerExitHandler、ISelectHandler、IDeselectHandler接口
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
    {
        #region Serialized Fields

        [Header("Tooltip Content")]
        [Tooltip("Tooltip标题")]
        public string TooltipTitle;

        [Tooltip("Tooltip描述内容")]
        [TextArea(2, 5)]
        public string TooltipDescription;

        [Tooltip("自定义图标")]
        public Sprite Icon;

        [Header("Timing Settings")]
        [Tooltip("显示延迟（秒），0表示立即显示")]
        public float ShowDelay = 0.5f;

        [Tooltip("显示时长（秒），0表示永久显示")]
        public float DisplayDuration;

        [Header("Position Settings")]
        [Tooltip("显示位置模式")]
        public TooltipPositionMode PositionMode = TooltipPositionMode.PivotAnchored;

        [Tooltip("锚点方向（用于PivotAnchored模式）")]
        public TooltipAnchorDirection AnchorDirection = TooltipAnchorDirection.Top;

        [Tooltip("鼠标位置偏移（用于MousePosition模式）")]
        public Vector2 MouseOffset;

        [Tooltip("锚点偏移（用于PivotAnchored模式）")]
        public Vector2 AnchorOffset;

        [Header("Meta Tooltip Settings")]
        [Tooltip("是否启用MetaTooltip")]
        public bool EnableMetaTooltip = true;

        [Header("Advanced Settings")]
        [Tooltip("自定义Tooltip内容生成器")]
        public TooltipContentGenerator CustomContentGenerator;

        [Tooltip("是否使用全局设置")]
        public bool UseGlobalSettings = true;

        #endregion

        #region Private Fields

        protected bool _isMouseOver;
        protected bool _isSelected;
        protected bool _isTooltipShown;
        protected float _hoverTimer;
        protected bool _isHoverTimerRunning;
        protected RectTransform _rectTransform;
        protected TooltipRequest _currentRequest;

        #endregion

        #region Properties

        /// <summary>
        /// 获取当前关联的RectTransform
        /// </summary>
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    TryGetComponent(out _rectTransform);

                return _rectTransform;
            }
        }

        /// <summary>
        /// 是否正在显示Tooltip
        /// </summary>
        public bool isTooltipShown => _isTooltipShown;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            TryGetComponent(out _rectTransform);
        }

        protected virtual void Start()
        {
            // 确保TooltipManager存在
            EnsureTooltipManager();
        }

        protected virtual void Update()
        {
            if (!_isHoverTimerRunning || !_isMouseOver) 
                return;

            float effectiveDelay = UseGlobalSettings && TooltipManager.Instance
                ? TooltipManager.Instance.settings.defaultShowDelay
                : ShowDelay;

            if (effectiveDelay <= 0)
            {
                ShowTooltipInternal();
            }
            else
            {
                _hoverTimer += Time.unscaledDeltaTime;
                if (_hoverTimer >= effectiveDelay)
                {
                    ShowTooltipInternal();
                }
            }
        }

        protected virtual void OnDisable()
        {
            HideTooltipInternal();
        }

        protected virtual void OnDestroy()
        {
            HideTooltipInternal();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 设置Tooltip内容
        /// </summary>
        public virtual void SetContent(string description)
        {
            TooltipDescription = description;
        }

        /// <summary>
        /// 设置Tooltip内容
        /// </summary>
        public virtual void SetContent(string title, string description)
        {
            TooltipTitle = title;
            TooltipDescription = description;
        }

        /// <summary>
        /// 设置Tooltip内容
        /// </summary>
        public virtual void SetContent(TooltipContent content)
        {
            if (content != null)
            {
                TooltipTitle = content.title;
                TooltipDescription = content.description;
                Icon = content.icon;
            }
        }

        /// <summary>
        /// 设置显示延迟
        /// </summary>
        public virtual void SetShowDelay(float delay)
        {
            ShowDelay = delay;
        }

        /// <summary>
        /// 设置显示时长
        /// </summary>
        public virtual void SetDisplayDuration(float duration)
        {
            DisplayDuration = duration;
        }

        /// <summary>
        /// 设置位置模式
        /// </summary>
        public virtual void SetPositionMode(TooltipPositionMode mode)
        {
            PositionMode = mode;
        }

        /// <summary>
        /// 设置锚点方向
        /// </summary>
        public virtual void SetAnchorDirection(TooltipAnchorDirection direction)
        {
            AnchorDirection = direction;
        }

        /// <summary>
        /// 手动显示Tooltip
        /// </summary>
        public virtual void ShowTooltip()
        {
            _isMouseOver = true;
            ShowTooltipInternal();
        }

        /// <summary>
        /// 手动隐藏Tooltip
        /// </summary>
        public virtual void HideTooltip()
        {
            _isMouseOver = false;
            HideTooltipInternal();
        }

        /// <summary>
        /// 刷新Tooltip内容（如果正在显示）
        /// </summary>
        public virtual void RefreshTooltip()
        {
            if (_isTooltipShown)
            {
                HideTooltipInternal();
                ShowTooltipInternal();
            }
        }

        /// <summary>
        /// 设置自定义内容生成器
        /// </summary>
        public virtual void SetCustomContentGenerator(TooltipContentGenerator generator)
        {
            CustomContentGenerator = generator;
        }

        /// <summary>
        /// 获取当前Tooltip内容
        /// </summary>
        public virtual TooltipContent GetContent()
        {
            if (CustomContentGenerator != null)
            {
                return CustomContentGenerator.GenerateContent(this);
            }

            return new(TooltipTitle, TooltipDescription, Icon);
        }

        #endregion

        #region Interface Implementations

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
            _hoverTimer = 0f;
            _isHoverTimerRunning = true;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
            _isHoverTimerRunning = false;
            _hoverTimer = 0f;
            HideTooltipInternal();
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            ShowTooltipInternal();
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            if (!_isMouseOver)
            {
                HideTooltipInternal();
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // 可选：点击时隐藏Tooltip
            // HideTooltipInternal();
        }

        #endregion

        #region Private Methods

        void EnsureTooltipManager()
        {
            if (TooltipManager.Instance == null)
            {
                GameObject go = new GameObject("TooltipManager");
                go.AddComponent<TooltipManager>();
                DontDestroyOnLoad(go);
            }
        }

        protected virtual void ShowTooltipInternal()
        {
            if (!CanShowTooltip()) 
                return;

            TooltipContent content = GetContent();
            if (content == null || string.IsNullOrEmpty(content.description)) 
                return;

            float effectiveDelay = UseGlobalSettings && TooltipManager.Instance
                ? TooltipManager.Instance.settings.defaultShowDelay
                : ShowDelay;

            float effectiveDuration = UseGlobalSettings && TooltipManager.Instance
                ? (DisplayDuration > 0 ? DisplayDuration : (TooltipManager.Instance.settings.defaultDisplayDuration))
                : DisplayDuration;

            TooltipDurationMode durationMode = effectiveDuration > 0
                ? TooltipDurationMode.Timed
                : TooltipDurationMode.Permanent;

            _currentRequest = new TooltipRequest
            {
                content = content,
                positionMode = PositionMode,
                anchorDirection = AnchorDirection,
                durationMode = durationMode,
                displayDuration = effectiveDuration,
                mouseOffset = MouseOffset,
                anchorOffset = AnchorOffset,
                targetRect = rectTransform,
                trigger = this,
                isMetaEnabled = EnableMetaTooltip,
                onShow = OnTooltipShow,
                onHide = OnTooltipHide
            };

            if (TooltipManager.Instance)
            {
                TooltipManager.Instance.ShowTooltip(_currentRequest);
            }

            _isTooltipShown = true;
            _isHoverTimerRunning = false;
        }

        protected virtual void HideTooltipInternal()
        {
            if (!_isTooltipShown) 
                return;

            if (TooltipManager.Instance)
            {
                TooltipManager.Instance.HideTooltip();
            }

            _isTooltipShown = false;
            _currentRequest = null;
        }

        protected virtual bool CanShowTooltip()
        {
            if (TooltipManager.Instance && !TooltipManager.Instance.settings.enableTooltip)
                return false;
            
            if (_isTooltipShown)
                return false;

            return true;
        }

        void OnTooltipShow()
        {
            _isTooltipShown = true;
        }

        void OnTooltipHide()
        {
            _isTooltipShown = false;
            _currentRequest = null;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (DisplayDuration < 0)
            {
                DisplayDuration = 0;
            }
        }
#endif

        #endregion
    }
}
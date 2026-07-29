using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains
{
    /// <summary>
    /// Tooltip内容数据结构
    /// </summary>
    [Serializable]
    public class TooltipContent
    {
        /// <summary>
        /// Tooltip标题
        /// </summary>
        public string title;

        /// <summary>
        /// Tooltip描述内容
        /// </summary>
        public string description;

        /// <summary>
        /// 自定义图标（可选）
        /// </summary>
        public Sprite icon;

        /// <summary>
        /// 额外自定义数据
        /// </summary>
        public object customData;

        /// <summary>
        /// 使用便捷构造函数
        /// </summary>
        public TooltipContent(string description)
        {
            this.description = description;
        }

        public TooltipContent(string title, string description)
        {
            this.title = title;
            this.description = description;
        }

        public TooltipContent(string title, string description, Sprite icon)
        {
            this.title = title;
            this.description = description;
            this.icon = icon;
        }
    }

    /// <summary>
    /// MetaTooltip内容数据结构
    /// </summary>
    [Serializable]
    public class MetaTooltipContent
    {
        /// <summary>
        /// Meta关键字类型
        /// </summary>
        public MetaKeywordType keywordType;

        /// <summary>
        /// 关键字内容
        /// </summary>
        public string keyword;

        /// <summary>
        /// 显示名称
        /// </summary>
        public string displayName;

        /// <summary>
        /// 描述内容
        /// </summary>
        public string description;

        /// <summary>
        /// 额外数据
        /// </summary>
        public object customData;

        public MetaTooltipContent(MetaKeywordType type, string keyword, string displayName, string description)
        {
            this.keywordType = type;
            this.keyword = keyword;
            this.displayName = displayName;
            this.description = description;
        }
    }

    /// <summary>
    /// Tooltip显示请求数据结构
    /// </summary>
    public class TooltipRequest
    {
        public TooltipContent content;
        public TooltipPositionMode positionMode;
        public TooltipAnchorDirection anchorDirection;
        public TooltipDurationMode durationMode;
        public float displayDuration;
        public Vector2 fixedPosition;
        public Vector2 mouseOffset;
        public Vector2 anchorOffset;
        public RectTransform targetRect;
        public MonoBehaviour trigger;
        public Action onShow;
        public Action onHide;
        public bool isMetaEnabled;

        public TooltipRequest()
        {
            positionMode = TooltipPositionMode.PivotAnchored;
            anchorDirection = TooltipAnchorDirection.Top;
            durationMode = TooltipDurationMode.Permanent;
            displayDuration = 5f;
            mouseOffset = new(20f, -20f);
            anchorOffset = new(0f, -10f);
            isMetaEnabled = true;
        }
    }

    /// <summary>
    /// Tooltip全局管理器
    /// 需要挂载到一个持久化的GameObject上
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        #region Singleton

        static TooltipManager _instance;

        public static TooltipManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TooltipManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TooltipManager");
                        _instance = go.AddComponent<TooltipManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Properties

        public TooltipSettings _settings = new();
        public TooltipSettings settings => _settings;

        [Header("UI References")]
        public GameObject _tooltipBoxPrefab;

        public GameObject _metaTooltipBoxPrefab;

        TooltipBox _currentTooltipBox;
        List<MetaTooltipBox> _currentMetaBoxes = new();
        Canvas _mainCanvas;
        Camera _uiCamera;

        TooltipRequest _currentRequest;
        float _showTimer;
        float _fadeTimer;
        bool _isShowing;
        bool _isFading;
        float _currentFadeDuration;
        float _fadeStartAlpha;
        float _fadeEndAlpha;

        Dictionary<string, MetaTooltipContent> _keywordCache = new();
        Regex _keywordRegex;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeCanvas();
            InitializeKeywordRegex();
        }

        void Update()
        {
            if (!_isShowing && !_isFading) 
                return;

            if (_isShowing && _currentRequest != null)
            {
                if (_currentRequest.durationMode == TooltipDurationMode.Timed)
                {
                    _showTimer -= Time.unscaledDeltaTime;
                    if (_showTimer <= 0)
                    {
                        HideTooltip();
                        return;
                    }
                }

                if (_currentRequest.positionMode == TooltipPositionMode.MousePosition)
                {
                    UpdateMousePosition();
                }
            }

            if (_isFading)
            {
                _fadeTimer -= Time.unscaledDeltaTime;
                float t = 1f - Mathf.Clamp01(_fadeTimer / _currentFadeDuration);
                float alpha = Mathf.Lerp(_fadeStartAlpha, _fadeEndAlpha, t);

                if (_currentTooltipBox)
                {
                    _currentTooltipBox.SetAlpha(alpha);
                }

                for (int i = _currentMetaBoxes.Count - 1; i >= 0; i--)
                {
                    _currentMetaBoxes[i].SetAlpha(alpha);
                }

                if (_fadeTimer <= 0)
                {
                    CompleteHide();
                }
            }
        }

        #endregion

        #region Initialization

        void InitializeCanvas()
        {
            _mainCanvas = GetComponentInParent<Canvas>();
            if (_mainCanvas == null)
            {
                _mainCanvas = FindFirstObjectByType<Canvas>();
            }

            if (_mainCanvas)
            {
                _uiCamera = _mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : _mainCanvas.worldCamera;
            }
        }

        void InitializeKeywordRegex()
        {
            if (!string.IsNullOrEmpty(_settings.keywordPattern))
            {
                _keywordRegex = new(_settings.keywordPattern);
            }

            foreach (var preset in _settings.keywordPresets)
            {
                if (!string.IsNullOrEmpty(preset.keyword))
                {
                    _keywordCache[preset.keyword] = new(
                        preset.type,
                        preset.keyword,
                        preset.displayName,
                        string.Empty
                    );
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// 显示Tooltip
        /// </summary>
        public void ShowTooltip(TooltipRequest request)
        {
            if (!_settings.enableTooltip || request?.content == null) 
                return;

            _currentRequest = request;

            if (_currentRequest.durationMode == TooltipDurationMode.Timed)
            {
                _showTimer = _currentRequest.displayDuration > 0 ? _currentRequest.displayDuration : _settings.defaultDisplayDuration;
            }

            CreateTooltipBox();
            CreateMetaTooltipBoxes();

            PositionTooltipBox();
            ShowWithFade();

            _isShowing = true;
            _isFading = false;

            _currentRequest.onShow?.Invoke();
        }

        /// <summary>
        /// 隐藏Tooltip
        /// </summary>
        public void HideTooltip()
        {
            if (!_isShowing) 
                return;

            _isShowing = false;

            var fadeOutDuration = _settings.fadeOutDuration;
            if (fadeOutDuration > 0)
            {
                _isFading = true;
                _fadeTimer = fadeOutDuration;
                _currentFadeDuration = fadeOutDuration;
                _fadeStartAlpha = _currentTooltipBox ? _currentTooltipBox.GetAlpha() : 1f;
                _fadeEndAlpha = 0F;
            }
            else
            {
                CompleteHide();
            }

            _currentRequest?.onHide?.Invoke();
        }

        /// <summary>
        /// 立即隐藏Tooltip（无动画）
        /// </summary>
        public void HideTooltipImmediate()
        {
            _isShowing = false;
            _isFading = false;
            CompleteHide();
            _currentRequest?.onHide?.Invoke();
        }

        /// <summary>
        /// 更新Tooltip内容
        /// </summary>
        public void UpdateTooltipContent(TooltipContent content)
        {
            if (_currentTooltipBox && content != null)
            {
                _currentTooltipBox.SetContent(content);
                CreateMetaTooltipBoxes();
                PositionTooltipBox();
            }
        }

        /// <summary>
        /// 更新设置
        /// </summary>
        public void UpdateSettings(TooltipSettings newSettings)
        {
            if (newSettings != null)
            {
                _settings = newSettings;
                InitializeKeywordRegex();
            }
        }

        /// <summary>
        /// 注册自定义Meta关键字
        /// </summary>
        public void RegisterMetaKeyword(string keyword, MetaTooltipContent content)
        {
            if (!string.IsNullOrEmpty(keyword) && content != null)
            {
                _keywordCache[keyword] = content;
            }
        }

        /// <summary>
        /// 获取当前是否正在显示Tooltip
        /// </summary>
        public bool IsShowing() => _isShowing;

        #endregion

        #region Private Methods

        void CreateTooltipBox()
        {
            if (_currentTooltipBox)
            {
                DestroyTooltipBox();
            }

            GameObject prefab = _tooltipBoxPrefab;
            if (prefab == null)
            {
                prefab = CreateDefaultTooltipBoxPrefab();
            }

            GameObject go = Instantiate(prefab, transform);
            _currentTooltipBox = go.GetComponent<TooltipBox>();

            if (_currentTooltipBox == null)
            {
                _currentTooltipBox = go.AddComponent<TooltipBox>();
            }

            _currentTooltipBox.Initialize(_settings);
            _currentTooltipBox.SetContent(_currentRequest.content);
            _currentTooltipBox.SetAlpha(0f);
            _currentTooltipBox.gameObject.SetActive(true);
        }

        void DestroyTooltipBox()
        {
            if (_currentTooltipBox)
            {
                Destroy(_currentTooltipBox.gameObject);
                _currentTooltipBox = null;
            }
        }

        void CreateMetaTooltipBoxes()
        {
            if (!_settings.enableMetaTooltip || !_currentRequest.isMetaEnabled || _currentRequest.content == null)
            {
                ClearMetaBoxes();
                return;
            }

            var metaKeywords = ExtractMetaKeywords(_currentRequest.content.description);

            ClearMetaBoxes();

            foreach (var keyword in metaKeywords)
            {
                if (_keywordCache.TryGetValue(keyword, out var metaContent))
                {
                    CreateMetaTooltipBox(metaContent);
                }
            }
        }

        List<string> ExtractMetaKeywords(string text)
        {
            var keywords = new List<string>();

            if (_keywordRegex != null && !string.IsNullOrEmpty(text))
            {
                var matches = _keywordRegex.Matches(text);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        string keyword = match.Groups[1].Value;
                        if (!string.IsNullOrEmpty(keyword) && !keywords.Contains(keyword))
                        {
                            keywords.Add(keyword);
                        }
                    }
                }
            }

            return keywords;
        }

        void CreateMetaTooltipBox(MetaTooltipContent content)
        {
            GameObject prefab = _metaTooltipBoxPrefab;
            if (prefab == null)
            {
                prefab = CreateDefaultMetaTooltipBoxPrefab();
            }

            GameObject go = Instantiate(prefab, transform);
            var metaBox = go.GetComponent<MetaTooltipBox>();

            if (metaBox == null)
            {
                metaBox = go.AddComponent<MetaTooltipBox>();
            }

            metaBox.Initialize(_settings);
            metaBox.SetContent(content);
            metaBox.SetAlpha(_isFading ? 0f : (_isShowing ? 1f : 0f));
            metaBox.gameObject.SetActive(true);

            _currentMetaBoxes.Add(metaBox);
        }

        void ClearMetaBoxes()
        {
            foreach (var box in _currentMetaBoxes)
            {
                if (box)
                {
                    Destroy(box.gameObject);
                }
            }

            _currentMetaBoxes.Clear();
        }

        void PositionTooltipBox()
        {
            if (_currentTooltipBox == null || _currentRequest == null) 
                return;

            Vector2 targetPosition = CalculateTargetPosition();

            if (_settings.autoAdjustPosition)
            {
                targetPosition = AdjustPositionToScreen(targetPosition, _currentTooltipBox.GetRectTransform());
            }

            _currentTooltipBox.SetPosition(targetPosition);

            PositionMetaBoxes();
        }

        Vector2 CalculateTargetPosition()
        {
            switch (_currentRequest.positionMode)
            {
                case TooltipPositionMode.Fixed:
                    return _currentRequest.fixedPosition != Vector2.zero
                        ? _currentRequest.fixedPosition
                        : _settings.fixedPosition;

                case TooltipPositionMode.MousePosition:
                    Vector2 mousePos = Input.mousePosition;
                    return mousePos + _currentRequest.mouseOffset;

                case TooltipPositionMode.PivotAnchored:
                    return CalculatePivotAnchoredPosition();

                default:
                    return _settings.fixedPosition;
            }
        }

        Vector2 CalculatePivotAnchoredPosition()
        {
            if (_currentRequest.targetRect == null) 
                return _settings.fixedPosition;

            Vector2 pivot = _currentRequest.targetRect.pivot;
            using var _ = new ArrayScope<Vector3>(out var corners, 4);
            _currentRequest.targetRect.GetWorldCorners(corners);

            float width = Vector3.Distance(corners[0], corners[3]);
            float height = Vector3.Distance(corners[0], corners[1]);

            Vector2 offset = _currentRequest.anchorOffset;

            switch (_currentRequest.anchorDirection)
            {
                case TooltipAnchorDirection.Bottom:
                    return (Vector2)corners[0] + new Vector2(width * pivot.x, -height * (1f - pivot.y)) + offset;

                case TooltipAnchorDirection.Top:
                    return (Vector2)corners[1] + new Vector2(width * pivot.x, height * pivot.y) + offset;

                case TooltipAnchorDirection.Left:
                    return (Vector2)corners[0] + new Vector2(-width * (1f - pivot.x), height * pivot.y) + offset;

                case TooltipAnchorDirection.Right:
                    return (Vector2)corners[3] + new Vector2(width * pivot.x, height * pivot.y) + offset;

                case TooltipAnchorDirection.BottomLeft:
                    return (Vector2)corners[0] + new Vector2(-width * (1f - pivot.x), -height * (1f - pivot.y)) + offset;

                case TooltipAnchorDirection.BottomRight:
                    return (Vector2)corners[3] + new Vector2(width * pivot.x, -height * (1f - pivot.y)) + offset;

                case TooltipAnchorDirection.TopLeft:
                    return (Vector2)corners[1] + new Vector2(-width * (1f - pivot.x), height * pivot.y) + offset;

                case TooltipAnchorDirection.TopRight:
                    return (Vector2)corners[2] + new Vector2(width * pivot.x, height * pivot.y) + offset;

                case TooltipAnchorDirection.Center:
                    Vector2 center = (corners[0] + corners[2]) / 2f;
                    return center + offset;

                default:
                    return (Vector2)corners[1] + offset;
            }
        }

        Vector2 AdjustPositionToScreen(Vector2 position, RectTransform tooltipRect)
        {
            if (tooltipRect == null) 
                return position;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) 
                return position;

            float padding = _settings.screenEdgePadding;

            float canvasWidth = Screen.width;
            float canvasHeight = Screen.height;

            float tooltipWidth = tooltipRect.rect.width * tooltipRect.lossyScale.x;
            float tooltipHeight = tooltipRect.rect.height * tooltipRect.lossyScale.y;

            float adjustedX = position.x;
            float adjustedY = position.y;

            if (adjustedX + tooltipWidth > canvasWidth - padding)
            {
                adjustedX = canvasWidth - tooltipWidth - padding;
            }

            if (adjustedX < padding)
            {
                adjustedX = padding;
            }

            if (adjustedY - tooltipHeight < padding)
            {
                adjustedY = tooltipHeight + padding;
            }

            if (adjustedY > canvasHeight - padding)
            {
                adjustedY = canvasHeight - padding;
            }

            return new(adjustedX, adjustedY);
        }

        void PositionMetaBoxes()
        {
            if (_currentTooltipBox == null || _currentMetaBoxes.Count == 0) 
                return;

            RectTransform mainRect = _currentTooltipBox.GetRectTransform();
            Vector2 mainPos = mainRect.position;

            float spacing = _settings.metaTooltipSpacing;
            float currentX = mainPos.x + mainRect.rect.width * 0.5f + spacing;

            foreach (var metaBox in _currentMetaBoxes)
            {
                RectTransform metaRect = metaBox.GetRectTransform();
                float metaWidth = metaRect.rect.width * metaRect.lossyScale.x;

                Vector2 metaPos = new Vector2(currentX + metaWidth * 0.5f, mainPos.y);

                if (_settings.autoAdjustPosition)
                {
                    metaPos = AdjustPositionToScreen(metaPos, metaRect);
                }

                metaBox.SetPosition(metaPos);

                currentX += metaWidth + spacing;
            }
        }

        void UpdateMousePosition()
        {
            if (_currentTooltipBox == null) 
                return;

            Vector2 targetPosition = Input.mousePosition + (Vector3)_currentRequest.mouseOffset;

            if (_settings.autoAdjustPosition)
            {
                targetPosition = AdjustPositionToScreen(targetPosition, _currentTooltipBox.GetRectTransform());
            }

            _currentTooltipBox.SetPosition(targetPosition);
            PositionMetaBoxes();
        }

        void ShowWithFade()
        {
            var fadeInDuration = _settings.fadeInDuration;
            if (fadeInDuration > 0)
            {
                _fadeTimer = fadeInDuration;
                _currentFadeDuration = fadeInDuration;
                _fadeStartAlpha = 0f;
                _fadeEndAlpha = 1f;
                _isFading = true;
            }
            else
            {
                _currentTooltipBox.SetAlpha(1f);
                foreach (var metaBox in _currentMetaBoxes)
                {
                    metaBox.SetAlpha(1f);
                }

                _isFading = false;
            }
        }

        void CompleteHide()
        {
            _isFading = false;
            DestroyTooltipBox();
            ClearMetaBoxes();
            _currentRequest = null;
        }

        GameObject CreateDefaultTooltipBoxPrefab()
        {
            GameObject go = new GameObject("DefaultTooltipBox");

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 100);
            rect.anchoredPosition = Vector2.zero;

            Image bg = go.AddComponent<Image>();
            bg.color = new(0.1f, 0.1f, 0.1f, 0.95f);

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new(0, 0, 0, 0.5f);
            outline.effectDistance = new(2, 2);

            go.AddComponent<TooltipBox>();

            return go;
        }

        GameObject CreateDefaultMetaTooltipBoxPrefab()
        {
            GameObject go = new GameObject("DefaultMetaTooltipBox");

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 80);
            rect.anchoredPosition = Vector2.zero;

            Image bg = go.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.25f, 0.95f);

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.5f, 0.5f, 1f, 0.5f);
            outline.effectDistance = new Vector2(2, 2);

            go.AddComponent<MetaTooltipBox>();

            return go;
        }

        #endregion
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoreMountains
{
    /// <summary>
    /// MetaTooltipBox元信息显示组件
    /// 用于显示关键字的详细信息，如Buff、Skill等的具体描述
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MetaTooltipBox : MonoBehaviour
    {
        #region UI Components

        [Header("UI Components")]
        [SerializeField]
        protected Image mBackground;

        [SerializeField]
        protected Image mTypeIcon;

        [SerializeField]
        protected TextMeshProUGUI mKeywordText;

        [SerializeField]
        protected TextMeshProUGUI mDescriptionText;

        [SerializeField]
        protected RectTransform mContentContainer;

        #endregion

        #region Visual Settings

        [Header("Type Colors")]
        [SerializeField]
        protected Color mBuffColor = new(0.3f, 0.6f, 0.9f, 0.95f);

        [SerializeField]
        protected Color mSkillColor = new(0.9f, 0.6f, 0.3f, 0.95f);

        [SerializeField]
        protected Color mItemColor = new(0.6f, 0.9f, 0.3f, 0.95f);

        [SerializeField]
        protected Color mStatusColor = new(0.9f, 0.3f, 0.6f, 0.95f);

        [SerializeField]
        protected Color mCustomColor = new(0.7f, 0.7f, 0.9f, 0.95f);

        [SerializeField]
        protected Color mDefaultColor = new(0.15f, 0.15f, 0.25f, 0.95f);

        #endregion

        #region Properties

        TooltipSettings _settings;
        MetaTooltipContent _content;
        CanvasGroup _canvasGroup;
        float _currentAlpha = 1f;
        RectTransform _rectTransform;

        public RectTransform GetRectTransform()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            return _rectTransform;
        }

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            CacheComponents();
        }

        protected virtual void Start()
        {
        }

        protected virtual void Update()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化MetaTooltipBox
        /// </summary>
        public virtual void Initialize(TooltipSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// 设置MetaTooltip内容
        /// </summary>
        public virtual void SetContent(MetaTooltipContent content)
        {
            _content = content;

            if (content == null) 
                return;

            // 设置关键字文本
            if (mKeywordText)
            {
                if (!string.IsNullOrEmpty(content.displayName))
                {
                    mKeywordText.text = $"[{content.displayName}]";
                }
                else if (!string.IsNullOrEmpty(content.keyword))
                {
                    mKeywordText.text = $"[{content.keyword}]";
                }
                else
                {
                    mKeywordText.text = string.Empty;
                }
            }

            // 设置描述文本
            if (mDescriptionText)
            {
                if (!string.IsNullOrEmpty(content.description))
                {
                    mDescriptionText.text = content.description;
                    mDescriptionText.gameObject.SetActive(true);
                }
                else
                {
                    mDescriptionText.text = GetDefaultDescription(content.keywordType);
                    mDescriptionText.gameObject.SetActive(!string.IsNullOrEmpty(mDescriptionText.text));
                }
            }

            // 设置类型图标
            if (mTypeIcon)
            {
                Sprite typeSprite = GetTypeIcon(content.keywordType);
                if (typeSprite)
                {
                    mTypeIcon.sprite = typeSprite;
                    mTypeIcon.gameObject.SetActive(true);
                }
                else
                {
                    mTypeIcon.gameObject.SetActive(false);
                }
            }

            // 设置背景颜色
            if (mBackground)
            {
                mBackground.color = GetTypeColor(content.keywordType);
            }

            RefreshLayout();
        }

        /// <summary>
        /// 设置透明度
        /// </summary>
        public virtual void SetAlpha(float alpha)
        {
            _currentAlpha = Mathf.Clamp01(alpha);

            if (_canvasGroup)
            {
                _canvasGroup.alpha = _currentAlpha;
            }

            if (mBackground)
            {
                Color color = mBackground.color;
                color.a = _currentAlpha;
                mBackground.color = color;
            }
        }

        /// <summary>
        /// 获取当前透明度
        /// </summary>
        public virtual float GetAlpha()
        {
            return _currentAlpha;
        }

        /// <summary>
        /// 设置位置
        /// </summary>
        public virtual void SetPosition(Vector2 position)
        {
            GetRectTransform().position = position;
        }

        /// <summary>
        /// 获取当前内容
        /// </summary>
        public MetaTooltipContent GetContent()
        {
            return _content;
        }

        /// <summary>
        /// 获取关键字类型
        /// </summary>
        public MetaKeywordType GetKeywordType()
        {
            return _content?.keywordType ?? MetaKeywordType.Custom;
        }

        #endregion

        #region Protected Methods

        protected virtual void CacheComponents()
        {
            if (mBackground == null)
            {
                mBackground = GetComponent<Image>();
            }

            if (mContentContainer == null)
            {
                mContentContainer = transform.Find("ContentContainer")?.GetComponent<RectTransform>();
            }

            if (mKeywordText == null)
            {
                Transform keywordTrans = transform.Find("ContentContainer/Keyword");
                if (keywordTrans)
                {
                    mKeywordText = keywordTrans.GetComponent<TextMeshProUGUI>();
                }
            }

            if (mDescriptionText == null)
            {
                Transform descTrans = transform.Find("ContentContainer/Description");
                if (descTrans)
                {
                    mDescriptionText = descTrans.GetComponent<TextMeshProUGUI>();
                }
            }

            if (mTypeIcon == null)
            {
                Transform iconTrans = transform.Find("ContentContainer/TypeIcon");
                if (iconTrans)
                {
                    mTypeIcon = iconTrans.GetComponent<Image>();
                }
            }
        }

        protected virtual void RefreshLayout()
        {
            LayoutRebuilder.MarkLayoutForRebuild(GetRectTransform());
        }

        protected virtual Color GetTypeColor(MetaKeywordType type)
        {
            switch (type)
            {
                case MetaKeywordType.Buff:
                    return mBuffColor;
                case MetaKeywordType.Skill:
                    return mSkillColor;
                case MetaKeywordType.Item:
                    return mItemColor;
                case MetaKeywordType.Status:
                    return mStatusColor;
                case MetaKeywordType.Custom:
                    return mCustomColor;
                default:
                    return mDefaultColor;
            }
        }

        protected virtual Sprite GetTypeIcon(MetaKeywordType type)
        {
            // 这里可以从资源加载对应的图标
            // 目前返回null，实际使用时可以从Settings或资源系统加载
            return null;
        }

        protected virtual string GetDefaultDescription(MetaKeywordType type)
        {
            switch (type)
            {
                case MetaKeywordType.Buff:
                    return "增益效果";
                case MetaKeywordType.Skill:
                    return "技能";
                case MetaKeywordType.Item:
                    return "道具";
                case MetaKeywordType.Status:
                    return "状态";
                case MetaKeywordType.Custom:
                    return "自定义";
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Layout Helpers

        /// <summary>
        /// 自动创建默认UI结构
        /// </summary>
        public void CreateDefaultStructure()
        {
            // Background
            if (mBackground == null)
            {
                mBackground = GetComponent<Image>();
                if (mBackground == null)
                {
                    mBackground = gameObject.AddComponent<Image>();
                }
            }

            mBackground.color = mDefaultColor;

            // Outline effect
            Outline outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
            }

            outline.effectColor = new(0.5f, 0.5f, 1f, 0.5f);
            outline.effectDistance = new Vector2(2, 2);

            // Content Container
            GameObject contentObj = new GameObject("ContentContainer");
            contentObj.transform.SetParent(transform);
            mContentContainer = contentObj.AddComponent<RectTransform>();
            mContentContainer.anchorMin = Vector2.zero;
            mContentContainer.anchorMax = Vector2.one;
            mContentContainer.sizeDelta = new Vector2(-16, -16);
            mContentContainer.localPosition = Vector3.zero;
            mContentContainer.localScale = Vector3.one;

            HorizontalLayoutGroup hLayout = contentObj.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 8;
            hLayout.padding = new RectOffset(8, 8, 8, 8);
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Type Icon
            GameObject iconObj = new GameObject("TypeIcon");
            iconObj.transform.SetParent(contentObj.transform);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(24, 24);
            mTypeIcon = iconObj.AddComponent<Image>();
            mTypeIcon.raycastTarget = false;
            iconObj.SetActive(false);

            // Keyword Text
            GameObject keywordObj = new GameObject("Keyword");
            keywordObj.transform.SetParent(contentObj.transform);
            RectTransform keywordRect = keywordObj.AddComponent<RectTransform>();
            keywordRect.sizeDelta = new Vector2(0, 24);
            mKeywordText = keywordObj.AddComponent<TextMeshProUGUI>();
            mKeywordText.text = "[Keyword]";
            mKeywordText.fontSize = 16;
            mKeywordText.fontStyle = FontStyles.Bold;
            mKeywordText.color = Color.white;
            mKeywordText.alignment = TextAlignmentOptions.Left;

            // Description Text
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(contentObj.transform);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(0, 20);
            mDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
            mDescriptionText.text = "Description";
            mDescriptionText.fontSize = 12;
            mDescriptionText.color = new(0.8f, 0.8f, 0.8f);
            mDescriptionText.alignment = TextAlignmentOptions.Left;
            mDescriptionText.raycastTarget = false;
        }

        #endregion
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoreMountains
{
    /// <summary>
    /// TooltipBox显示组件
    /// 需要挂载到Tooltip的根GameObject上
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TooltipBox : MonoBehaviour
    {
        #region UI Components

        [Header("UI Components")]
        [SerializeField]
        protected Image mBackground;

        [SerializeField]
        protected TextMeshProUGUI mTitleText;

        [SerializeField]
        protected TextMeshProUGUI mDescriptionText;

        [SerializeField]
        protected Image mIconImage;

        [SerializeField]
        protected RectTransform mContentContainer;

        #endregion

        #region Properties

        TooltipSettings _settings;
        TooltipContent _content;
        CanvasGroup _canvasGroup;
        float _currentAlpha = 1f;
        RectTransform _rectTransform;

        public RectTransform GetRectTransform()
        {
            if (_rectTransform == null)
                TryGetComponent(out _rectTransform);

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
        /// 初始化TooltipBox
        /// </summary>
        public virtual void Initialize(TooltipSettings settings)
        {
            _settings = settings;

            if (_settings != null)
            {
                ApplySettings();
            }
        }

        /// <summary>
        /// 设置Tooltip内容
        /// </summary>
        public virtual void SetContent(TooltipContent content)
        {
            _content = content;

            if (content == null)
                return;

            if (mTitleText && !string.IsNullOrEmpty(content.title))
            {
                mTitleText.text = content.title;
                mTitleText.gameObject.SetActive(true);
            }
            else if (mTitleText)
            {
                mTitleText.gameObject.SetActive(false);
            }

            if (mDescriptionText && !string.IsNullOrEmpty(content.description))
            {
                mDescriptionText.text = content.description;
                mDescriptionText.gameObject.SetActive(true);
            }
            else if (mDescriptionText)
            {
                mDescriptionText.gameObject.SetActive(false);
            }

            if (mIconImage)
            {
                if (content.icon)
                {
                    mIconImage.sprite = content.icon;
                    mIconImage.gameObject.SetActive(true);
                }
                else
                {
                    mIconImage.gameObject.SetActive(false);
                }
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
        /// 设置本地位置
        /// </summary>
        public virtual void SetLocalPosition(Vector3 position)
        {
            GetRectTransform().localPosition = position;
        }

        /// <summary>
        /// 获取当前内容
        /// </summary>
        public TooltipContent GetContent()
        {
            return _content;
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

            if (mTitleText == null)
            {
                Transform titleTrans = transform.Find("ContentContainer/Title");
                if (titleTrans)
                {
                    mTitleText = titleTrans.GetComponent<TextMeshProUGUI>();
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

            if (mIconImage == null)
            {
                Transform iconTrans = transform.Find("ContentContainer/Icon");
                if (iconTrans)
                {
                    mIconImage = iconTrans.GetComponent<Image>();
                }
            }
        }

        protected virtual void ApplySettings()
        {
            // 可以根据settings调整样式
        }

        protected virtual void RefreshLayout()
        {
            // 触发布局重建
            LayoutRebuilder.MarkLayoutForRebuild(GetRectTransform());
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

            mBackground.color = new(0.1f, 0.1f, 0.1f, 0.95f);

            // Content Container
            GameObject contentObj = new GameObject("ContentContainer");
            contentObj.transform.SetParent(transform);
            mContentContainer = contentObj.AddComponent<RectTransform>();
            mContentContainer.anchorMin = Vector2.zero;
            mContentContainer.anchorMax = Vector2.one;
            mContentContainer.sizeDelta = new Vector2(-20, -20);
            mContentContainer.localPosition = Vector3.zero;
            mContentContainer.localScale = Vector3.one;

            VerticalLayoutGroup vLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            vLayout.spacing = 8;
            vLayout.padding = new RectOffset(10, 10, 10, 10);
            vLayout.childAlignment = TextAnchor.UpperLeft;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = contentObj.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(contentObj.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 30);
            mTitleText = titleObj.AddComponent<TextMeshProUGUI>();
            mTitleText.text = "Title";
            mTitleText.fontSize = 18;
            mTitleText.fontStyle = FontStyles.Bold;
            mTitleText.color = Color.white;
            mTitleText.alignment = TextAlignmentOptions.Left;

            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(contentObj.transform);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(0, 50);
            mDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
            mDescriptionText.text = "Description";
            mDescriptionText.fontSize = 14;
            mDescriptionText.color = new Color(0.9f, 0.9f, 0.9f);
            mDescriptionText.alignment = TextAlignmentOptions.Left;

            // Icon
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(contentObj.transform);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.sizeDelta = new Vector2(40, 40);
            mIconImage = iconObj.AddComponent<Image>();
            mIconImage.raycastTarget = false;
            iconObj.SetActive(false);
        }

        #endregion
    }
}
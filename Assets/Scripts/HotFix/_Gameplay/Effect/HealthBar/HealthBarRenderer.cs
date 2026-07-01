using UnityEngine;

namespace MarbleHero
{
    /// <summary>
    /// 血条着色器属性枚举，对应 Shader 中的 Property 名称
    /// </summary>
    public enum FillOrigin
    {
        Left = 0,
        Right = 1,
        Bottom = 2,
        Top = 3,
    }

    /// <summary>
    /// 血条渲染辅助类：封装 Shader 的 MaterialPropertyBlock，
    /// 通过 SpriteRenderer 组件渲染，无需改变 size/坐标/transform
    /// </summary>
    public class HealthBarRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        SpriteRenderer _spriteRenderer;

        [Header("Colors")]
        [SerializeField]
        Color _foregroundColor = new Color(0.9f, 0.15f, 0.15f, 1f);

        [SerializeField]
        Color _bufferColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        [Header("Progress [0-1]")]
        [Range(0f, 1f)]
        [SerializeField]
        float _foregroundProgress = 1f;

        [Range(0f, 1f)]
        [SerializeField]
        float _bufferProgress = 1f;

        [Header("Buffer Settings")]
        [Range(0f, 0.5f)]
        [SerializeField]
        float _bufferOffset = 0.03f;

        [Range(0f, 0.05f)]
        [SerializeField]
        float _bufferInnerFade = 0.01f;

        [Range(0f, 0.05f)]
        [SerializeField]
        float _bufferOuterFade = 0.005f;

        [Header("Fill Direction")]
        [SerializeField]
        FillOrigin _fillOrigin = FillOrigin.Left;

        [SerializeField]
        bool _flipHorizontal;

        [SerializeField]
        bool _flipVertical;

        [Header("Style")]
        [SerializeField]
        bool _useChamfer;

        [Range(0f, 0.15f)]
        [SerializeField]
        float _chamferSize = 0.08f;

        [Header("Border")]
        [SerializeField]
        bool _useBorder;

        [SerializeField]
        Color _borderColor = Color.black;

        [Range(0f, 0.5f)]
        [SerializeField]
        float _borderWidth = 0.05f;

        [Range(0f, 0.5f)]
        [SerializeField]
        float _borderSqueeze;

        [Header("Advanced")]
        [SerializeField]
        bool _antiFlicker = true;

        // ——— Runtime Buffer Animation ———
        float _targetBufferProgress;
        float _currentBufferVelocity;

        static readonly int kForegroundColor = Shader.PropertyToID("_ForegroundColor");
        static readonly int kForegroundProgress = Shader.PropertyToID("_ForegroundProgress");
        static readonly int kBufferColor = Shader.PropertyToID("_BufferColor");
        static readonly int kBufferProgress = Shader.PropertyToID("_BufferProgress");
        static readonly int kBufferOffset = Shader.PropertyToID("_BufferOffset");
        static readonly int kBufferInnerFade = Shader.PropertyToID("_BufferInnerFade");
        static readonly int kBufferOuterFade = Shader.PropertyToID("_BufferOuterFade");
        static readonly int kFillOrigin = Shader.PropertyToID("_FillOrigin");
        static readonly int kFlipHorizontal = Shader.PropertyToID("_FlipHorizontal");
        static readonly int kFlipVertical = Shader.PropertyToID("_FlipVertical");
        static readonly int kUseChamfer = Shader.PropertyToID("_UseChamfer");
        static readonly int kChamferSize = Shader.PropertyToID("_ChamferSize");
        static readonly int kUseBorder = Shader.PropertyToID("_UseBorder");
        static readonly int kBorderColor = Shader.PropertyToID("_BorderColor");
        static readonly int kBorderWidth = Shader.PropertyToID("_BorderWidth");
        static readonly int kBorderSqueeze = Shader.PropertyToID("_BorderSqueeze");
        static readonly int kUseBuffer = Shader.PropertyToID("_UseBuffer");
        static readonly int kUseAntiFlicker = Shader.PropertyToID("_UseAntiFlicker");

        MaterialPropertyBlock _block;

        // ================================================================
        // Public API
        // ================================================================

        /// <summary>
        /// 设置当前实际血量 [0, 1]
        /// </summary>
        public float ForegroundProgress
        {
            get => _foregroundProgress;
            set => _foregroundProgress = Mathf.Clamp01(value);
        }

        /// <summary>
        /// 设置目标缓冲血量 [0, 1]，会平滑动画到目标值
        /// </summary>
        public float BufferProgress
        {
            get => _targetBufferProgress;
            set => _targetBufferProgress = Mathf.Clamp01(value);
        }

        /// <summary>
        /// 立即将缓冲层设置到指定值（无动画）
        /// </summary>
        public void SetBufferImmediate(float value)
        {
            _targetBufferProgress = Mathf.Clamp01(value);
            _bufferProgress = _targetBufferProgress;
        }

        /// <summary>
        /// 扣血：foreground 立即减少，buffer 层延迟追上来
        /// </summary>
        /// <param name="currentHP">当前血量比例 [0,1]</param>
        /// <param name="bufferDelay">buffer 追上来的速度（每秒变化量）</param>
        public void ApplyDamage(float currentHP, float bufferDelay = 0.5f)
        {
            ForegroundProgress = currentHP;
            _targetBufferProgress = currentHP;
        }

        /// <summary>
        /// 直接设置前景和缓冲值（前台立即扣，缓冲延迟）
        /// </summary>
        public void SetProgress(float foreground, float buffer, bool immediateBuffer = false)
        {
            _foregroundProgress = Mathf.Clamp01(foreground);
            _targetBufferProgress = Mathf.Clamp01(buffer);
            if (immediateBuffer)
                _bufferProgress = _targetBufferProgress;
        }

        public Color ForegroundColor
        {
            get => _foregroundColor;
            set => _foregroundColor = value;
        }

        public Color BufferColor
        {
            get => _bufferColor;
            set => _bufferColor = value;
        }

        public FillOrigin Direction
        {
            get => _fillOrigin;
            set => _fillOrigin = value;
        }

        // ================================================================
        // MonoBehaviour
        // ================================================================

        void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            _block = new MaterialPropertyBlock();
            _bufferProgress = _targetBufferProgress = _foregroundProgress;
        }

        void Update()
        {
            // Buffer 层平滑追赶
            if (!Mathf.Approximately(_bufferProgress, _targetBufferProgress))
            {
                _bufferProgress = Mathf.SmoothDamp(
                    _bufferProgress,
                    _targetBufferProgress,
                    ref _currentBufferVelocity,
                    0.15f,
                    Mathf.Infinity,
                    Time.deltaTime
                );

                // 防止抖动死循环
                if (Mathf.Abs(_bufferProgress - _targetBufferProgress) < 0.0005f)
                    _bufferProgress = _targetBufferProgress;
            }

            ApplyToMaterial();
        }

        // ================================================================
        // Internal
        // ================================================================

        void ApplyToMaterial()
        {
            if (_spriteRenderer == null) return;

            _spriteRenderer.GetPropertyBlock(_block);

            _block.SetColor(kForegroundColor, _foregroundColor);
            _block.SetFloat(kForegroundProgress, _foregroundProgress);

            _block.SetColor(kBufferColor, _bufferColor);
            _block.SetFloat(kBufferProgress, _bufferProgress);
            _block.SetFloat(kBufferOffset, _bufferOffset);
            _block.SetFloat(kBufferInnerFade, _bufferInnerFade);
            _block.SetFloat(kBufferOuterFade, _bufferOuterFade);

            _block.SetInt(kFillOrigin, (int)_fillOrigin);
            _block.SetInt(kFlipHorizontal, _flipHorizontal ? 1 : 0);
            _block.SetInt(kFlipVertical, _flipVertical ? 1 : 0);

            _block.SetInt(kUseChamfer, _useChamfer ? 1 : 0);
            _block.SetFloat(kChamferSize, _chamferSize);

            _block.SetInt(kUseBorder, _useBorder ? 1 : 0);
            _block.SetColor(kBorderColor, _borderColor);
            _block.SetFloat(kBorderWidth, _borderWidth);
            _block.SetFloat(kBorderSqueeze, _borderSqueeze);

            _block.SetInt(kUseBuffer, 1);
            _block.SetInt(kUseAntiFlicker, _antiFlicker ? 1 : 0);

            _spriteRenderer.SetPropertyBlock(_block);
        }

        // ================================================================
        // Editor Helpers
        // ================================================================

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Application.isPlaying) return;
            ApplyToMaterial();
        }
#endif
    }
}
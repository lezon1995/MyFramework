using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 单次受击产生的 DamageChunk
    /// </summary>
    public struct DamageChunk
    {
        public int index; //该 chunk 在数组中的索引（Shader 固定为 0..7）
        public float start; //chunk 起始进度
        public float end; //chunk 结束进度
        public float opacity; //当前透明度，1=完全不透明，0=完全消失
        public Color color; //该 chunk 的颜色
        public bool isActive; //该 chunk 是否被占用（opacity > 0 即视为占用）
    }

    /// <summary>
    /// 血条渲染辅助类：通过 SpriteRenderer + Shader 的 MaterialPropertyBlock 控制血条。
    /// 只需传入整数值（当前血量、最大血量、护盾值），位置和长度自动计算。
    /// </summary>
    public class DamageChunkHealthBarRenderer : MonoBehaviour, IHealthBarRenderer
    {
        // ================================================================
        // Constants
        // ================================================================
        const int MaxChunks = 8;

        public Health health;

        // ================================================================
        // Inspector
        // ================================================================
        [Header("References")]
        [SerializeField]
        SpriteRenderer _spriteRenderer;

        [Header("Foreground (Actual HP)")]
        [SerializeField]
        Color _foregroundColor = new(0.9f, 0.15f, 0.15f, 1f);

        [Range(0f, 1f)]
        [SerializeField]
        float _foregroundProgress = 1f;

        [Header("Fill Direction")]
        [SerializeField]
        FillOrigin _fillOrigin = FillOrigin.Left;

        [SerializeField]
        bool _flipHorizontal;

        [SerializeField]
        bool _flipVertical;

        [Header("Chunk Style")]
        [SerializeField]
        float _bufferOuterFade = 0.005f;

        [Header("Chunk Colors")]
        [SerializeField]
        Color defaultChunkColor = new(1f, 0.8196079F, 0F, 1f);

        [Header("Chunk Animation")]
        [Tooltip("Chunk 透明度从 1.0 衰减到 0 的总时长（秒）")]
        [SerializeField]
        float _chunkFadeDuration = 1.2f;

        [Header("Shield")]
        [SerializeField]
        bool _useShield = true;

        [SerializeField]
        Color shieldColor = new(0.7803922f, 0.7803922f, 0.7803922f, 1f);

        [SerializeField]
        int _shieldValue;

        [Range(0f, 1f)]
        [SerializeField]
        float _shieldLength;

        [Range(0f, 1f)]
        [SerializeField]
        float _shieldGlow = 0.3f;

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

        [Header("Style")]
        [SerializeField]
        bool _useChamfer;

        [Range(0f, 0.15f)]
        [SerializeField]
        float _chamferSize = 0.08f;

        // ================================================================
        // Runtime State
        // ================================================================
        List<DamageChunk> _chunks = new(MaxChunks);
        int[] _slotOwner = new int[MaxChunks];

        // 当前整数值（由 SetHealth 提供）
        int _currentHp => health.CurrentHealth;
        int _maxHp => health.maximumHealth;
        int _shield => health.Shield.CurrentShield;
        float shieldProgressStart => _foregroundProgress;
        float shieldProgressEnd => _foregroundProgress + _shieldLength;

        // Shader Property IDs
        static readonly int kForegroundColor = Shader.PropertyToID("_ForegroundColor");
        static readonly int kForegroundProgress = Shader.PropertyToID("_ForegroundProgress");
        static readonly int kFillOrigin = Shader.PropertyToID("_FillOrigin");
        static readonly int kFlipHorizontal = Shader.PropertyToID("_FlipHorizontal");
        static readonly int kFlipVertical = Shader.PropertyToID("_FlipVertical");
        static readonly int kBufferOuterFade = Shader.PropertyToID("_BufferOuterFade");
        static readonly int kUseChamfer = Shader.PropertyToID("_UseChamfer");
        static readonly int kChamferSize = Shader.PropertyToID("_ChamferSize");
        static readonly int kUseBorder = Shader.PropertyToID("_UseBorder");
        static readonly int kBorderColor = Shader.PropertyToID("_BorderColor");
        static readonly int kBorderWidth = Shader.PropertyToID("_BorderWidth");
        static readonly int kBorderSqueeze = Shader.PropertyToID("_BorderSqueeze");
        static readonly int kChunkCount = Shader.PropertyToID("_ChunkCount");
        static readonly int kUseShield = Shader.PropertyToID("_UseShield");
        static readonly int kShieldColor = Shader.PropertyToID("_ShieldColor");
        static readonly int kShieldLength = Shader.PropertyToID("_ShieldLength");
        static readonly int kShieldGlow = Shader.PropertyToID("_ShieldGlow");

        static readonly int[] kChunkVec =
        {
            Shader.PropertyToID("_Chunk0"), Shader.PropertyToID("_Chunk1"),
            Shader.PropertyToID("_Chunk2"), Shader.PropertyToID("_Chunk3"),
            Shader.PropertyToID("_Chunk4"), Shader.PropertyToID("_Chunk5"),
            Shader.PropertyToID("_Chunk6"), Shader.PropertyToID("_Chunk7"),
        };

        static readonly int[] kChunkColor =
        {
            Shader.PropertyToID("_DamageChunkColor0"), Shader.PropertyToID("_DamageChunkColor1"),
            Shader.PropertyToID("_DamageChunkColor2"), Shader.PropertyToID("_DamageChunkColor3"),
            Shader.PropertyToID("_DamageChunkColor4"), Shader.PropertyToID("_DamageChunkColor5"),
            Shader.PropertyToID("_DamageChunkColor6"), Shader.PropertyToID("_DamageChunkColor7"),
        };

        MaterialPropertyBlock _block;

        // ================================================================
        // Public API - 简洁接口
        // ================================================================

        /// <summary>
        /// 当前实际血量进度 [0, 1]
        /// </summary>
        public float ForegroundProgress
        {
            get => _foregroundProgress;
            set => _foregroundProgress = Mathf.Clamp01(value);
        }

        public void SetHealth(Health h)
        {
            health = h;
        }

        /// <summary>
        /// 一键设置血量和护盾（推荐使用）
        /// </summary>
        /// <param name="currentHp">当前生命值（整数）</param>
        /// <param name="maxHp">最大生命值（整数）</param>
        /// <param name="shield">护盾值（整数，可为0）</param>
        public void RefreshHealthBarAndShieldBar()
        {
            // 计算前景进度（护盾会推高护盾条的位置，所以前景按当前血量/最大血量计算）
            float hpProgress = (float)_currentHp / _maxHp;
            float shieldProgress = (float)_shield / _maxHp;

            // 总长度 = hpProgress + shieldProgress
            // 总长度不能超过 1.0（如果超过，按比例压缩）
            float total = hpProgress + shieldProgress;
            if (total > 1f)
            {
                float scale = 1f / total;
                _foregroundProgress = hpProgress * scale;
                _shieldLength = shieldProgress * scale;
            }
            else
            {
                _foregroundProgress = hpProgress;
                _shieldLength = shieldProgress;
            }
        }

        /// <summary>
        /// 直接设置前景和缓冲进度（兼容旧接口）
        /// </summary>
        public void SetProgress(float curPct)
        {
            ForegroundProgress = curPct;
            ClearAllChunks();
        }

        /// <summary>
        /// 扣血：前景立即减少，产生一个新的 DamageChunk。
        /// </summary>
        /// <param name="curHpPct">扣血后的实际血量 [0,1]</param>
        public void ApplyDamageToHealthBar(float curHpPct)
        {
            float prevForeground = _foregroundProgress;

            float chunkStart = curHpPct;
            float chunkEnd = prevForeground;

            ForegroundProgress = curHpPct;

            if (chunkStart < chunkEnd)
            {
                CreateChunk(
                    chunkStart, // start = 受击瞬间的前景值
                    chunkEnd, // end = 受击瞬间的缓冲值（当前缓冲，不是目标缓冲）
                    defaultChunkColor
                );
            }
        }

        /// <summary>
        /// 受到伤害：自动先扣护盾再扣血。护盾和血量都共用 DamageChunk 显示受损动画。
        /// </summary>
        public void ApplyDamageToShieldBar(float curProgress)
        {
            float chunkStart = curProgress; // 受击后血量位置（= 受击后护盾起点）
            float chunkEnd = shieldProgressEnd;

            int prevShield = _shield;
            int prevHp = _currentHp;

            // 2. 计算受击前的总长度（用于产生 chunk）
            float prevTotal = Mathf.Min(1f, (float)(prevHp + prevShield) / _maxHp);
            float prevHpPct = prevTotal > 0f ? (float)prevHp / (prevHp + prevShield) * prevTotal : 0f;

            // 4. 重新计算 progress（自动处理压缩）
            RefreshHealthBarAndShieldBar();

            // 5. 产生 chunk（统一用 DamageChunk）
            // chunk 起点 = 受击后的血量终点（= 护盾起点）
            // chunk 终点 = 受击前的血量终点
            // 即 chunk 覆盖"扣除的血量 + 扣除的护盾"区域

            // chunk 必须有宽度才创建
            if (chunkEnd > chunkStart && !chunkEnd.isEqual(chunkStart))
            {
                CreateChunk(
                    chunkStart,
                    chunkEnd,
                    defaultChunkColor
                );
            }
        }

        /// <summary>
        /// 恢复满状态
        /// </summary>
        public void RestoreFull()
        {
            RefreshHealthBarAndShieldBar();
            ClearAllChunks();
        }

        /// <summary>
        /// 清空所有 DamageChunk
        /// </summary>
        public void ClearAllChunks()
        {
            for (int i = 0; i < MaxChunks; i++)
                _slotOwner[i] = -1;

            _chunks.Clear();
        }

        // ================================================================
        // Chunk Management
        // ================================================================

        /// <summary>
        /// 创建一个 DamageChunk 并放入 Shader 槽位
        /// </summary>
        public void CreateChunk(float start, float end, Color color)
        {
            int slot = -1;
            for (int i = 0; i < MaxChunks; i++)
            {
                if (_slotOwner[i] < 0)
                {
                    slot = i;
                    break;
                }
            }

            if (slot < 0)
            {
                float minOpacity = float.MaxValue;
                int oldestIdx = 0;
                for (int i = 0; i < _chunks.Count; i++)
                {
                    if (_chunks[i].opacity < minOpacity)
                    {
                        minOpacity = _chunks[i].opacity;
                        oldestIdx = i;
                    }
                }

                DamageChunk old = _chunks[oldestIdx];
                slot = old.index;
                _chunks.RemoveAt(oldestIdx);
            }

            DamageChunk chunk = new DamageChunk
            {
                index = slot,
                start = Mathf.Clamp01(start),
                end = Mathf.Clamp01(end),
                opacity = 1f,
                color = color,
                isActive = true,
            };

            _slotOwner[slot] = _chunks.Count;
            _chunks.Add(chunk);
        }

        // ================================================================
        // Inspector 属性
        // ================================================================

        public Color ForegroundColor
        {
            get => _foregroundColor;
            set => _foregroundColor = value;
        }

        public Color DefaultChunkColor
        {
            get => defaultChunkColor;
            set => defaultChunkColor = value;
        }

        public Color ShieldColor
        {
            get => shieldColor;
            set => shieldColor = value;
        }

        public FillOrigin Direction
        {
            get => _fillOrigin;
            set => _fillOrigin = value;
        }

        public bool UseShield
        {
            get => _useShield;
            set => _useShield = value;
        }

        public int CurrentHealth => _currentHp;
        public int MaxHealth => _maxHp;

        // ================================================================
        // MonoBehaviour
        // ================================================================

        void Awake()
        {
            if (_spriteRenderer == null)
                TryGetComponent(out _spriteRenderer);

            _block = new();
            for (int i = 0; i < MaxChunks; i++)
                _slotOwner[i] = -1;
        }

        void Update()
        {
            var dt = Time.deltaTime;
            UpdateChunks(dt);
            ApplyToMaterial();
        }

        void UpdateChunks(float dt)
        {
            float fadeSpeed = _chunkFadeDuration > 0 ? 1f / _chunkFadeDuration : 1f;

            for (int i = _chunks.Count - 1; i >= 0; i--)
            {
                var chunk = _chunks[i];
                chunk.opacity -= fadeSpeed * dt;

                if (chunk.opacity <= 0f)
                {
                    chunk.opacity = 0f;
                    _slotOwner[chunk.index] = -1;
                    _chunks.RemoveAt(i);
                }
                else
                {
                    _chunks[i] = chunk;
                }
            }
        }

        // ================================================================
        // MaterialPropertyBlock
        // ================================================================

        public void ApplyToMaterial()
        {
            if (_spriteRenderer == null)
                return;

            if (_block == null)
                return;

            _spriteRenderer.GetPropertyBlock(_block);

            // 前景
            _block.SetColor(kForegroundColor, _foregroundColor);
            _block.SetFloat(kForegroundProgress, _foregroundProgress);

            // 填充方向
            _block.SetInt(kFillOrigin, (int)_fillOrigin);
            _block.SetInt(kFlipHorizontal, _flipHorizontal ? 1 : 0);
            _block.SetInt(kFlipVertical, _flipVertical ? 1 : 0);

            // 边缘柔和
            _block.SetFloat(kBufferOuterFade, _bufferOuterFade);

            // 样式
            _block.SetInt(kUseChamfer, _useChamfer ? 1 : 0);
            _block.SetFloat(kChamferSize, _chamferSize);

            // 描边
            _block.SetInt(kUseBorder, _useBorder ? 1 : 0);
            _block.SetColor(kBorderColor, _borderColor);
            _block.SetFloat(kBorderWidth, _borderWidth);
            _block.SetFloat(kBorderSqueeze, _borderSqueeze);

            // 护盾
            _block.SetInt(kUseShield, _useShield ? 1 : 0);
            _block.SetColor(kShieldColor, shieldColor);
            _block.SetFloat(kShieldLength, _shieldLength);
            _block.SetFloat(kShieldGlow, _shieldGlow);

            // DamageChunks
            _block.SetInt(kChunkCount, _chunks.Count);

            for (int i = 0; i < MaxChunks; i++)
            {
                _block.SetVector(kChunkVec[i], Vector4.zero);
                _block.SetColor(kChunkColor[i], Color.clear);
            }

            for (int i = 0; i < _chunks.Count; i++)
            {
                DamageChunk chunk = _chunks[i];
                _block.SetVector(kChunkVec[chunk.index], new Vector3(chunk.start, chunk.end, chunk.opacity));
                _block.SetColor(kChunkColor[chunk.index], chunk.color);
            }

            _spriteRenderer.SetPropertyBlock(_block);
        }

        // ================================================================
        // Editor
        // ================================================================
#if UNITY_EDITOR
        void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            ApplyToMaterial();
        }
#endif
    }
}
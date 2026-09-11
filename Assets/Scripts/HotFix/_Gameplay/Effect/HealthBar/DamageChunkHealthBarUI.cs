using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains
{
    /// <summary>
    /// 血条渲染辅助类（UI版本）：通过 Image + Shader 的 Material 控制血条，
    /// 每次受击产生一个独立的 DamageChunk，支持 chunk 透明度和颜色动画。
    /// </summary>
    public class DamageChunkHealthBarUI : MonoBehaviour, IHealthBarRenderer
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
        Image _image;

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
        Color _defaultChunkColor = new(0.78f, 0.78f, 0.78f, 1f);

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
        /// <summary>当前活跃的 chunk 列表（有序：最老的在前面）</summary>
        List<DamageChunk> _chunks = new(MaxChunks);

        /// <summary>Shader 中的 _Chunk 数组固定 8 槽位，记录每个槽位当前存放哪个 chunk 的索引</summary>
        int[] _slotOwner = new int[MaxChunks]; // -1 = 空槽

        // 当前整数值（由 SetHealth 提供）
        int _currentHp => health.CurrentHealth;
        int _maxHp => health.maximumHealth;
        int _shield => health.Shield.CurrentShield;
        
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

        Material _material;

        // ================================================================
        // Public API
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
        /// 直接设置前景和缓冲进度
        /// </summary>
        /// <param name="curPct">实际血量 [0,1]</param>
        /// <param name="bufferPct">缓冲血量 [0,1]</param>
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
                    _defaultChunkColor
                );
            }
        }

        /// <summary>
        /// 扣血：前景立即减少，产生一个新的 DamageChunk。
        /// </summary>
        /// <param name="curProgress"></param>
        public void ApplyDamageToShieldBar(float curProgress)
        {
            int prevShield = _shield;
            int prevHp = _currentHp;

            // 2. 计算受击前的总长度（用于产生 chunk）
            float prevTotal = Mathf.Min(1f, (float)(prevHp + prevShield) / _maxHp);
            float prevHpPct = prevTotal > 0f ? (float)prevHp / (prevHp + prevShield) * prevTotal : 0f;

            // 4. 重新计算 progress（自动处理压缩）
            RefreshHealthBarAndShieldBar();

            // 5. 产生 chunk（统一用 DamageChunk）
            // chunk 位置 = 剩余血量位置到受击前血量位置
            Color color = _defaultChunkColor; // 默认用护盾色（更醒目）

            float chunkStart = _shieldLength; // 当前护盾起点（即当前血量终点）
            float chunkEnd = prevHpPct; // 受击前的血量位置（可能含护盾）

            // chunk 必须有宽度才创建
            if (chunkEnd > chunkStart)
            {
                CreateChunk(chunkStart, chunkEnd, color);
            }
        }

        /// <summary>
        /// 创建一个 DamageChunk 并放入 Shader 槽位。
        /// 如果所有 8 个槽位都正在使用，则复用最老的 chunk（透明度已接近 0 的优先）。
        /// </summary>
        void CreateChunk(float start, float end, Color color)
        {
            // 优先找一个空闲槽位
            int slot = -1;
            for (int i = 0; i < MaxChunks; i++)
            {
                if (_slotOwner[i] < 0)
                {
                    slot = i;
                    break;
                }
            }

            // 所有槽位都满了 → 找 opacity 最低的那个复用
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
                start = start,
                end = end,
                opacity = 1f,
                color = color,
            };

            _slotOwner[slot] = _chunks.Count;
            _chunks.Add(chunk);
        }

        /// <summary>
        /// 恢复血量：清空所有 chunk，前景和缓冲都设为满。
        /// </summary>
        public void RestoreFull()
        {
            _foregroundProgress = 1f;
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

        public Color ForegroundColor
        {
            get => _foregroundColor;
            set => _foregroundColor = value;
        }

        public Color DefaultChunkColor
        {
            get => _defaultChunkColor;
            set => _defaultChunkColor = value;
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
            if (_image == null)
                TryGetComponent(out _image);

            if (_image != null)
            {
                // UI Image 需要使用共享材质或实例材质
                _material = _image.material;
            }

            for (int i = 0; i < MaxChunks; i++)
                _slotOwner[i] = -1;
        }

        void Update()
        {
            var dt = Time.deltaTime;

            // 更新每个 chunk 的透明度
            UpdateChunks(dt);

            ApplyToMaterial();
        }

        // ================================================================
        // Chunk Lifecycle
        // ================================================================

        void UpdateChunks(float dt)
        {
            float fadeSpeed = _chunkFadeDuration > 0 ? 1f / _chunkFadeDuration : 1f;

            // 倒序遍历方便安全删除
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
        // Material Properties
        // ================================================================

        public void ApplyToMaterial()
        {
            if (_image == null)
                return;

            if (_material == null)
                return;

            // 前景
            _material.SetColor(kForegroundColor, _foregroundColor);
            _material.SetFloat(kForegroundProgress, _foregroundProgress);

            // 填充方向
            _material.SetInt(kFillOrigin, (int)_fillOrigin);
            _material.SetInt(kFlipHorizontal, _flipHorizontal ? 1 : 0);
            _material.SetInt(kFlipVertical, _flipVertical ? 1 : 0);

            // 边缘柔和
            _material.SetFloat(kBufferOuterFade, _bufferOuterFade);

            // 样式
            _material.SetInt(kUseChamfer, _useChamfer ? 1 : 0);
            _material.SetFloat(kChamferSize, _chamferSize);

            // 描边
            _material.SetInt(kUseBorder, _useBorder ? 1 : 0);
            _material.SetColor(kBorderColor, _borderColor);
            _material.SetFloat(kBorderWidth, _borderWidth);
            _material.SetFloat(kBorderSqueeze, _borderSqueeze);
            
            // 护盾
            _material.SetInt(kUseShield, _useShield ? 1 : 0);
            _material.SetColor(kShieldColor, shieldColor);
            _material.SetFloat(kShieldLength, _shieldLength);
            _material.SetFloat(kShieldGlow, _shieldGlow);

            // DamageChunks
            _material.SetInt(kChunkCount, _chunks.Count);

            // 先把所有 chunk vec/color 初始化为 (0,0,0,0)，保证空槽安全
            for (int i = 0; i < MaxChunks; i++)
            {
                _material.SetVector(kChunkVec[i], Vector4.zero);
                _material.SetColor(kChunkColor[i], Color.clear);
            }

            // 填入活跃 chunk
            for (int i = 0; i < _chunks.Count; i++)
            {
                DamageChunk chunk = _chunks[i];
                _material.SetVector(kChunkVec[chunk.index], new Vector3(chunk.start, chunk.end, chunk.opacity));
                _material.SetColor(kChunkColor[chunk.index], chunk.color);
            }
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
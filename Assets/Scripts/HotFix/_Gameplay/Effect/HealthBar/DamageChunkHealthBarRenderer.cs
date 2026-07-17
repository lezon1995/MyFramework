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
        public float start; //chunk 起始进度（= 受击瞬间的前景值）
        public float end; //chunk 结束进度（= 受击瞬间的缓冲值）
        public float opacity; //当前透明度，1=完全不透明，0=完全消失
        public Color color; //该 chunk 的颜色
        public bool isActive; //该 chunk 是否被占用（opacity > 0 即视为占用）
    }

    /// <summary>
    /// 血条渲染辅助类：通过 SpriteRenderer + Shader 的 MaterialPropertyBlock 控制血条，
    /// 每次受击产生一个独立的 DamageChunk，支持 chunk 透明度和颜色动画。
    /// </summary>
    public class DamageChunkHealthBarRenderer : MonoBehaviour
    {
        // ================================================================
        // Constants
        // ================================================================
        const int MaxChunks = 8;

        // ================================================================
        // Inspector
        // ================================================================
        [Header("References")]
        [SerializeField]
        SpriteRenderer _spriteRenderer;

        [Header("Foreground (Actual HP)")]
        [SerializeField]
        Color _foregroundColor = new Color(0.9f, 0.15f, 0.15f, 1f);

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
        Color _defaultChunkColor = new Color(0.78f, 0.78f, 0.78f, 1f);

        [Header("Chunk Animation")]
        [Tooltip("Chunk 透明度从 1.0 衰减到 0 的总时长（秒）")]
        [SerializeField]
        float _chunkFadeDuration = 1.2f;

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
        /// <param name="chunkColor">此次受击的 chunk 颜色（可传 null 使用默认灰色）</param>
        public void ApplyDamage(float curHpPct, Color? chunkColor = null)
        {
            float prevForeground = _foregroundProgress;
            ForegroundProgress = curHpPct;

            if (curHpPct < prevForeground)
            {
                CreateChunk(
                    curHpPct, // start = 受击瞬间的前景值
                    prevForeground, // end = 受击瞬间的缓冲值（当前缓冲，不是目标缓冲）
                    chunkColor ?? _defaultChunkColor
                );
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
                isActive = true,
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
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            _block = new();
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

            // DamageChunks
            _block.SetInt(kChunkCount, _chunks.Count);

            // 先把所有 chunk vec/color 初始化为 (0,0,0,0)，保证空槽安全
            for (int i = 0; i < MaxChunks; i++)
            {
                _block.SetVector(kChunkVec[i], Vector4.zero);
                _block.SetColor(kChunkColor[i], Color.clear);
            }

            // 填入活跃 chunk
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
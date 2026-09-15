using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 在指定 SpriteRenderer 身上按时间间隔"投胎"出残影实例。
    /// 残影的衰减由 C# 端每帧把当前 alpha 乘以 alphaMultiplier 实现，shader 自身无状态。
    ///
    /// 用法：
    ///   1) 准备一个 Material，Shader = Game/SpriteAfterImage
    ///   2) 挂到 Player 上 (或 SpriteRenderer 旁边)，指定 sourceRenderer 与 afterImageMaterial
    ///   3) 冲刺开始 -> emitter.Begin(); 冲刺结束 -> emitter.End();
    /// </summary>
    [DisallowMultipleComponent]
    public class SpriteAfterImageEmitter : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("要捕获的 SpriteRenderer (通常是玩家的 SpriteUnit)")]
        [SerializeField]
        SpriteRenderer sourceRenderer;

        [Header("Material")]
        [Tooltip("使用 Game/SpriteAfterImage 生成的 Material")]
        [SerializeField]
        Material afterImageMaterial;

        [Header("Spawn")]
        [Tooltip("每隔多少物理帧投一个残影")]
        [SerializeField, Min(1)]
        public int spawnFixedUpdateInterval = 1;

        [Tooltip("对象池大小 (超过后环形覆盖最老的)")]
        [SerializeField, Min(1)]
        int poolSize = 16;

        [Header("Fade")]
        [Tooltip("每帧残影 alpha 乘以这个系数 (1=不衰减, 0.92=约 1.3s 衰减到 0)")]
        [SerializeField, Range(0f, 1f)]
        float alphaMultiplier = 0.92f;

        [Tooltip("残影 alpha 低于此值时回收进池")]
        [SerializeField, Range(0f, 0.5f)]
        float recycleAlphaThreshold = 0.01f;

        [Header("Layering")]
        [Tooltip("残影排序相对源 sprite 的偏移 (-1 表示压在玩家下面)")]
        [SerializeField]
        int sortingOrderOffset = -1;

        [Tooltip("残影使用独立的 SortingLayer,留空则继承源 sprite 的")]
        [SerializeField]
        string overrideSortingLayer;

        ACreature _owner;
        SpriteRenderer _src;
        Transform _root; // 残影专用根节点,与 emitter 解耦,避免跟玩家移动
        SpriteRenderer[] _pool;
        Material[] _poolMats;
        float[] _alphas;
        int _cursor;
        public int FixedUpdateCounter { get; set; }
        bool _emitting;

        // Shader uniforms
        static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        void Awake()
        {
            if (sourceRenderer == null)
            {
                TryGetComponent(out sourceRenderer);
            }

            this.TryGetComponentInParent(out _owner);

            _src = sourceRenderer;
            BuildPool();
        }

        void BuildPool()
        {
            // 残影必须与玩家解耦:挂在独立根节点下,避免随父级移动/旋转
            var rootGo = new GameObject("[AfterImages]");
            _root = rootGo.transform;

            _pool = new SpriteRenderer[poolSize];
            _poolMats = new Material[poolSize];
            _alphas = new float[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject("AfterImage_" + i);
                go.hideFlags = HideFlags.DontSave;
                go.transform.SetParent(_root, worldPositionStays: false);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.material = new Material(afterImageMaterial);
                sr.enabled = false;

                _pool[i] = sr;
                _poolMats[i] = sr.material;
            }
        }

        /// <summary>冲刺开始时调用</summary>
        public void Begin()
        {
            _emitting = true;
            FixedUpdateCounter = 0;
        }

        /// <summary>冲刺结束时调用</summary>
        public void End()
        {
            _emitting = false;
        }

        void LateUpdate()
        {
            // 每帧对活着的残影做 alpha *= multiplier,推到材质
            for (int i = 0; i < poolSize; i++)
            {
                var sr = _pool[i];
                if (sr == null || !sr.enabled)
                    continue;

                float a = _alphas[i] * alphaMultiplier;
                _alphas[i] = a;

                var mat = _poolMats[i];
                mat.SetFloat(AlphaId, a);

                // alpha 太低就回收,不再占用 draw call
                if (a <= recycleAlphaThreshold)
                {
                    sr.enabled = false;
                }
            }
        }

        public void Spawn(Vector3 position)
        {
            int idx = _cursor;
            _cursor = (_cursor + 1) % poolSize;

            var sr = _pool[idx];
            sr.sprite = _src.sprite;
            sr.flipX = _src.flipX;
            sr.flipY = _src.flipY;
            // 残影与玩家解耦,直接写世界坐标 / 世界缩放,避免父链缩放叠加
            sr.transform.position = position;
            sr.transform.localScale = _src.transform.lossyScale;

            if (!string.IsNullOrEmpty(overrideSortingLayer))
            {
                sr.sortingLayerName = overrideSortingLayer;
            }
            else
            {
                sr.sortingLayerID = _src.sortingLayerID;
            }

            sr.sortingOrder = _src.sortingOrder + sortingOrderOffset;

            // 显式把 sprite 纹理喂给材质,避免依赖 Unity 的自动 _MainTex 绑定
            _poolMats[idx].SetTexture("_MainTex", _src.sprite.texture);

            // 新生残影从完全不透明起步,后续每帧衰减
            _alphas[idx] = 1f;
            _poolMats[idx].SetFloat(AlphaId, 1f);
            sr.enabled = true;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (sourceRenderer == null)
            {
                TryGetComponent(out sourceRenderer);
            }
        }
#endif

        void OnDestroy()
        {
            if (_poolMats != null)
            {
                for (int i = 0; i < _poolMats.Length; i++)
                {
                    if (_poolMats[i])
                    {
                        Destroy(_poolMats[i]);
                    }
                }
            }

            if (_root)
            {
                Destroy(_root.gameObject);
            }
        }
    }
}
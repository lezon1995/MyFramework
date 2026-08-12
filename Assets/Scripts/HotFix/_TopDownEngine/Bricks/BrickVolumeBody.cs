using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Brick的体积感组件
    /// 附加到Brick上以启用体积碰撞和链式击退
    /// </summary>
    [RequireComponent(typeof(Brick))]
    public class BrickVolumeBody : MonoBehaviour
    {
        [Header("体积参数")]
        [Tooltip("碰撞半径（为空则根据Brick的size自动计算）")]
        public float? OverrideRadius;

        [Tooltip("覆盖中心偏移（为空则使用 (0,0)）")]
        public Vector2? OverrideOffset;

        [Tooltip("强制使用矩形形状（默认圆形）")]
        public bool UseRectangle;

        [Tooltip("覆盖矩形尺寸（留空则按 Brick 的 size 计算）")]
        public Vector2? OverrideSize;

        [Tooltip("质量（越大越难被推开）")]
        [Range(0.1f, 10f)]
        public float Mass = 1f;

        [Tooltip("最大重叠比率（0表示完全不能重叠）")]
        [Range(0f, 0.5f)]
        public float MaxOverlapRatio = 0.2f;

        [Header("推力权重")]
        [Tooltip("推力权重（决定谁推谁动）")]
        [Range(0f, 10f)]
        public float PushForceWeight = 1f;

        [Header("击退参数")]
        [Tooltip("击退抗性（0表示完全受击退，1表示免疫击退）")]
        [Range(0f, 1f)]
        public float KnockbackResistance = 0f;

        [Tooltip("击退后速度衰减")]
        [Range(0f, 1f)]
        public float KnockbackDamping = 0.95f;

        [Header("链式击退")]
        [Tooltip("是否作为链式击退的源头")]
        public bool CanTriggerChainKnockback = true;

        [Tooltip("被击退时能否被链式传递")]
        public bool CanBeChained = true;

        [Header("调试")]
        [Tooltip("显示体积半径")]
        public bool ShowGizmos = true;

        public Color GizmosColor = new Color(0, 1, 0, 0.5f);

        // 运行时
        Brick _brick;
        TopDownController2D _body;
        Vector2 _lastPosition;
        Vector2 _knockbackVelocity;

        public TopDownController2D Body => _body;
        public Vector2 KnockbackVelocity => _knockbackVelocity;

        protected virtual void Awake()
        {
            _brick = GetComponent<Brick>();
            InitializeBody();
        }

        protected virtual void Start()
        {
            // 注册到体积管理器
            if (volumeManager != null)
            {
                volumeManager.Register(_body);
            }
        }

        protected virtual void OnEnable()
        {
            if (_body != null && volumeManager != null)
            {
                volumeManager.Register(_body);
            }
        }

        protected virtual void OnDisable()
        {
            if (_body != null && volumeManager != null)
            {
                volumeManager.Unregister(_body);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_body != null && volumeManager != null)
            {
                volumeManager.Unregister(_body);
            }
        }

        protected virtual void Update()
        {
            UpdatePosition();
        }

        protected virtual void LateUpdate()
        {
            ApplyKnockbackDamping();
        }

        /// <summary>
        /// 初始化体积数据
        /// </summary>
        protected virtual void InitializeBody()
        {
            _body = GetComponent<TopDownController2D>();
            if (_body == null)
            {
                _body = gameObject.AddComponent<TopDownController2D>();
            }

            // 设置参数
            if (_body.Volume == null)
                _body.Volume = new VolumeShape();

            _body.Volume.Shape = UseRectangle ? VolumeShapeType.Rectangle : VolumeShapeType.Circle;
            _body.Volume.Offset = OverrideOffset ?? Vector2.zero;

            if (UseRectangle)
            {
                Vector2 size = OverrideSize ?? (_brick != null ? _brick.size : Vector2.one);
                _body.Volume.Size = new Vector2(Mathf.Max(0.01f, size.x), Mathf.Max(0.01f, size.y));
            }
            else
            {
                float radius = OverrideRadius ?? CalculateRadius();
                _body.Volume.Radius = radius;
            }
            _body.Mass = Mass;
            _body.MaxOverlapRatio = MaxOverlapRatio;
            _body.PushForceWeight = PushForceWeight;
            _body.KnockbackResistance = KnockbackResistance;
            _body.SeparationForce = 10f;
            _body.VelocityDamping = KnockbackDamping;
            _body.GizmosColor = GizmosColor;

            // 同步初始位置
            _body.Position = transform.position;
            _lastPosition = _body.Position;
        }

        /// <summary>
        /// 根据Brick的size计算半径
        /// </summary>
        protected virtual float CalculateRadius()
        {
            if (_brick == null) 
                return 0.5f;

            return Mathf.Min(_brick.size.x, _brick.size.y) * 0.5f;
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        protected virtual void UpdatePosition()
        {
            if (_body == null) 
                return;

            _lastPosition = _body.Position;
            _body.Position = transform.position;
        }

        /// <summary>
        /// 应用击退速度衰减
        /// </summary>
        protected virtual void ApplyKnockbackDamping()
        {
            _knockbackVelocity *= KnockbackDamping;

            // 将击退速度同步到实体速度
            if (_body != null)
            {
                ((TopDownController)_body).IntentVelocity += (Vector3)_knockbackVelocity;
            }
        }

        /// <summary>
        /// 施打击退力
        /// </summary>
        public virtual void ApplyKnockback(Vector2 direction, float force)
        {
            if (_body == null) 
                return;

            if (!CanBeChained) 
                return;

            float actualForce = force * (1f - KnockbackResistance);
            if (actualForce < 0.01f) 
                return;

            // 添加到击退速度
            _knockbackVelocity += direction.normalized * actualForce;

            // 应用到实体
            _body.AddImpact(direction, actualForce);

            // 如果可以触发链式击退
            if (CanTriggerChainKnockback && volumeManager != null)
            {
                volumeManager.ApplyKnockback(_body, direction, force);
            }
        }

        /// <summary>
        /// 从球的攻击中受到击退
        /// </summary>
        public virtual void OnHitByBall(Vector2 hitNormal, float knockbackForce)
        {
            // 击退方向是碰撞法线的反方向
            Vector2 knockbackDir = -hitNormal;
            ApplyKnockback(knockbackDir, knockbackForce);

            // 触发链式击退
            if (CanTriggerChainKnockback && volumeManager != null)
            {
                volumeManager.ApplyKnockback(_body, knockbackDir, knockbackForce);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!ShowGizmos || _body == null)
                return;

            Vector2 center = _body.VolumeCenter;
            Vector2 size = _body.Volume != null ? _body.Volume.GetWorldSize() : Vector2.one;

            Gizmos.color = GizmosColor;
            if (_body.Volume != null && _body.Volume.Shape == VolumeShapeType.Rectangle)
                Gizmos.DrawWireCube(center, size);
            else
                Gizmos.DrawWireSphere(center, _body.Volume?.Radius ?? 0.5f);

            // 绘制有效半径
            Color effectiveColor = new Color(1, 0, 0, 0.3f);
            Gizmos.color = effectiveColor;
            float effective = _body.EffectiveRadius;
            if (_body.Volume != null && _body.Volume.Shape == VolumeShapeType.Rectangle)
                Gizmos.DrawWireCube(center, size * (1f - _body.MaxOverlapRatio));
            else
                Gizmos.DrawWireSphere(center, effective);
        }
    }
}

using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 标记场景中的固体碰撞体（边界、墙壁、障碍物等）
    /// 这些碰撞体会被 VolumeManager 检测并阻止实体（玩家/怪物）通过
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class VolumeCollider : MonoBehaviour
    {
        [Header("碰撞设置")]
        [Tooltip("是否启用此碰撞体")]
        public bool Enabled = true;

        [Tooltip("碰撞优先级，数值越高越优先被推开（用于实体↔实体碰撞）")]
        [Range(0f, 10f)]
        public float CollisionPriority = 1f;

        [Header("调试")]
        [Tooltip("Gizmos 颜色")]
        public Color GizmosColor = new(0.3f, 0.8f, 0.3f, 0.5f);

        protected Collider2D _collider;
        protected Vector2 _cachedPosition;
        protected bool _isCircle;
        protected bool _isBox;

        // 缓存的形状数据（世界尺度）
        protected float _radius;            // 圆形：真实半径；方形：包围圆半径
        protected Vector2 _halfExtents;     // 方形：半尺寸
        protected Vector2 _centerOffset;    // 相对 transform.position 的偏移

        public Collider2D Collider => _collider;
        public bool IsCircle => _isCircle;
        public bool IsBox => _isBox;

        /// <summary>
        /// 用于"圆 vs 圆"近似的包围半径（圆形 = 真实半径；方形 = 对角线一半）
        /// 注意：与 BoxCollider2D 的实际"中心到边距离"不同。
        /// </summary>
        public float BoundingRadius => _radius;

        /// <summary>
        /// 当前世界坐标下的位置（center）
        /// </summary>
        public Vector2 CurrentPosition => _cachedPosition;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider2D>();
            UpdateColliderCache();
        }

        protected virtual void Start()
        {
            UpdateColliderCache();
        }

        /// <summary>
        /// 更新碰撞体缓存（位置、大小变化时调用）
        /// </summary>
        public virtual void UpdateColliderCache()
        {
            _cachedPosition = _collider.bounds.center;

            // 确保不是 Trigger（避免被 Unity 物理系统吃掉碰撞）
            _collider.isTrigger = false;

            Vector3 lossy = _collider.transform.lossyScale;

            switch (_collider)
            {
                case CircleCollider2D cc:
                    _isCircle = true;
                    _isBox = false;
                    _radius = cc.radius * Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y));
                    _centerOffset = cc.offset;
                    break;
                case BoxCollider2D bc:
                    _isCircle = false;
                    _isBox = true;
                    // 注意 BoxCollider2D.bounds 已经包含 lossy scale，我们直接用 world extents
                    Bounds b = bc.bounds;
                    _halfExtents = b.extents;
                    _radius = b.extents.magnitude;
                    _centerOffset = bc.offset;
                    break;
                default:
                    _isCircle = true;
                    _isBox = false;
                    Bounds fb = _collider.bounds;
                    _radius = fb.extents.magnitude;
                    _centerOffset = Vector2.zero;
                    break;
            }
        }

        /// <summary>
        /// 当 transform 移动后让 VolumeCollider 重新同步缓存
        /// </summary>
        public virtual void RefreshAfterMove()
        {
            _cachedPosition = _collider.bounds.center;
        }

        /// <summary>
        /// 检测点和碰撞体的最近表面距离。
        /// 返回：
        ///   distance —— 实体中心到碰撞体表面的距离（穿透时为 0）
        ///   normal   —— 从表面指向实体的方向（即把实体推出碰撞体的方向）
        /// </summary>
        public bool TryGetDistanceAndNormal(Vector2 point, float pointRadius, out float distance, out Vector2 normal)
        {
            if (_isCircle)
            {
                Vector2 delta = point - _cachedPosition;
                float dist = delta.magnitude;
                if (dist < 0.0001f)
                {
                    // 实体在圆心，无法判断方向，给一个默认方向
                    normal = Vector2.up;
                    distance = 0f;
                    return true;
                }

                // 表面点 = center + dir * radius
                float surfaceDist = dist - _radius;
                normal = delta / dist;

                // 表面距离：实体半径 - 穿透
                // surfaceDist < 0 表示实体表面已经进入圆形
                // 实体半径需要 ≥ 表面距离才不碰撞
                distance = surfaceDist;
                return true;
            }

            if (_isBox)
            {
                // 使用 BoxCollider2D.ClosestPoint 获取最近的"表面点"
                Vector2 closest = _collider.ClosestPoint(point);

                Vector2 delta = point - closest;
                float dist = delta.magnitude;

                if (dist < 0.0001f)
                {
                    // 实体中心已经"在碰撞体内部"或贴着表面，
                    // 这时需要找一个真正的分离方向：从 BoxCollider 中心指向实体
                    Vector2 fromCenter = point - _cachedPosition;
                    if (fromCenter.sqrMagnitude < 0.0001f)
                    {
                        // 实体在 BoxCollider 正中心，强制选一个轴向
                        normal = Vector2.up;
                    }
                    else
                    {
                        normal = fromCenter.normalized;
                    }
                    distance = -pointRadius; // 完全穿透
                    return true;
                }

                normal = delta / dist;

                // distance 是实体中心到最近表面点的距离
                // 穿透时为 0
                distance = dist;
                return true;
            }

            // 回退
            Vector2 d = point - _cachedPosition;
            float dd = d.magnitude;
            normal = dd > 0.0001f ? d / dd : Vector2.up;
            distance = dd - _radius;
            return true;
        }

        protected virtual void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (_collider == null) return;

            Gizmos.color = GizmosColor;

            if (_isCircle)
            {
                Gizmos.DrawWireSphere(_cachedPosition, _radius);
            }
            else if (_isBox)
            {
                Gizmos.DrawWireCube(_cachedPosition, _halfExtents * 2f);
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (_collider == null) return;

            Gizmos.color = GizmosColor;
            Vector2 center = _collider.bounds.center;

            if (_collider is CircleCollider2D)
            {
                Bounds b = _collider.bounds;
                Gizmos.DrawWireSphere(center, b.extents.x);
            }
            else if (_collider is BoxCollider2D)
            {
                Gizmos.DrawWireCube(center, _collider.bounds.size);
            }
            else
            {
                Gizmos.DrawWireSphere(center, 0.5f);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreMountains
{
    /// <summary>
    /// 标记场景中的固体碰撞体（边界、墙壁、障碍物等）
    /// 这些碰撞体会被 VolumeManager 检测并阻止实体（玩家/怪物）通过
    /// 支持 Collider2D / BoxCollider2D / TilemapCollider2D
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class VolumeCollider : MonoBehaviour
    {
        [Header("碰撞设置")]
        [Tooltip("是否启用此碰撞体")]
        public bool Enabled = true;

        [Tooltip("是否自动注册此碰撞体")]
        public bool AutoRegister;

        [Tooltip("碰撞优先级，数值越高越优先被推开（用于实体↔实体碰撞）")]
        [Range(0f, 10f)]
        public float CollisionPriority = 1f;

        [Header("调试")]
        [Tooltip("Gizmos 颜色")]
        public Color GizmosColor = new(0.3f, 0.8f, 0.3f, 0.5f);

        [Tooltip("Tilemap 模式下逐个格子绘制 Gizmo")]
        public bool DrawTilemapCellsGizmo = true;

        public bool IsEnabled() => Enabled && _collider.enabled;

        protected Collider2D _collider;
        protected Vector2 _cachedPosition;
        protected bool _isCircle;
        protected bool _isBox;
        protected bool _isTilemap;

        // 缓存的形状数据（世界尺度）
        protected float _radius; // 圆形：真实半径；方形/tilemap：包围圆半径
        protected Vector2 _halfExtents; // 方形/tilemap AABB：半尺寸
        protected Vector2 _centerOffset; // 相对 transform.position 的偏移

        // Tilemap 相关（用于逐格绘制与精确缓存）
        protected Tilemap _tilemap;
        protected Vector3Int[] _cachedTilePositions = System.Array.Empty<Vector3Int>();

        public Collider2D Collider => _collider;
        public bool IsCircle => _isCircle;
        public bool IsBox => _isBox;
        public bool IsTilemap => _isTilemap;

        /// <summary>
        /// 当前世界坐标下的位置（center）
        /// </summary>
        public Vector2 CurrentPosition => _cachedPosition;

        protected virtual void Awake()
        {
            TryGetComponent(out _collider);
            TryGetComponent(out _tilemap);
            UpdateColliderCache();
        }

        protected virtual void Start()
        {
            UpdateColliderCache();
        }

        void OnDestroy()
        {
        }

        public void RegisterToVolumeManager()
        {
            volumeManager.RegisterSolidCollider(this);
        }

        public void UnregisterToVolumeManager()
        {
            volumeManager.UnregisterSolidCollider(this);
        }

        /// <summary>
        /// 更新碰撞体缓存（位置、大小变化时调用）
        /// </summary>
        public virtual void UpdateColliderCache()
        {
            _cachedPosition = _collider.bounds.center;

            // 确保不是 Trigger（避免被 Unity 物理系统吃掉碰撞）
            // _collider.isTrigger = false;

            Vector3 lossy = _collider.transform.lossyScale;

            switch (_collider)
            {
                case CircleCollider2D cc:
                    _isCircle = true;
                    _isBox = false;
                    _isTilemap = false;
                    _radius = cc.radius * Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y));
                    _centerOffset = cc.offset;
                    break;
                case BoxCollider2D bc:
                    _isCircle = false;
                    _isBox = true;
                    _isTilemap = false;
                    // 注意 BoxCollider2D.bounds 已经包含 lossy scale，我们直接用 world extents
                    Bounds b = bc.bounds;
                    _halfExtents = b.extents;
                    _radius = b.extents.magnitude;
                    _centerOffset = bc.offset;
                    break;
                case TilemapCollider2D tc:
                    _isCircle = false;
                    _isBox = false;
                    _isTilemap = true;
                    // TilemapCollider2D 由若干个与坐标轴平行的正方形格子组成
                    // bounds 是所有格子的世界空间 AABB，ClosestPoint 由 Unity 负责精确计算
                    Bounds tb = tc.bounds;
                    _halfExtents = tb.extents;
                    _radius = tb.extents.magnitude;
                    _centerOffset = Vector2.zero;
                    RefreshTilemapCellCache();
                    break;
                default:
                    _isCircle = true;
                    _isBox = false;
                    _isTilemap = false;
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
                    // 点在圆心，距离 = -radius（点在圆内部）
                    distance = -_radius;
                    normal = Vector2.up;
                    return true;
                }

                // distance = 实体中心到圆表面的距离
                // 如果为负，说明实体中心在圆内部
                distance = dist - _radius;
                normal = delta / dist;
                return true;
            }

            if (_isBox || _isTilemap)
            {
                // BoxCollider2D / TilemapCollider2D 都重写了 Collider2D.ClosestPoint，
                // 对于 TilemapCollider2D，Unity 会返回最近格子表面上的点（精确到格子边缘）。
                Vector2 closest = _collider.ClosestPoint(point);

                Vector2 delta = point - closest;
                float dist = delta.magnitude;

                // closest == point 表示点在碰撞体外部，ClosestPoint 返回了点本身
                // 这种情况下距离是 0（刚好在边界上，不算碰撞）
                if (dist < 0.0001f)
                {
                    distance = 0f;
                    Vector2 fromCenter = point - _cachedPosition;
                    normal = fromCenter.sqrMagnitude > 0.0001f ? fromCenter.normalized : Vector2.up;
                    return true;
                }

                // distance = 实体中心到 Box/Tilemap 表面的距离（不减去半径）
                distance = dist;
                normal = delta / dist;
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
            if (!Application.isPlaying)
                return;

            if (_collider == null)
                return;

            Gizmos.color = GizmosColor;

            if (_isCircle)
            {
                Gizmos.DrawWireSphere(_cachedPosition, _radius);
            }
            else if (_isBox)
            {
                Gizmos.DrawWireCube(_cachedPosition, _halfExtents * 2f);
            }
            else if (_isTilemap)
            {
                DrawTilemapGizmos();
            }
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (_collider == null)
                return;

            Gizmos.color = GizmosColor;
            Vector2 center = _collider.bounds.center;

            switch (_collider)
            {
                case CircleCollider2D:
                    Bounds b = _collider.bounds;
                    Gizmos.DrawWireSphere(center, b.extents.x);
                    break;
                case BoxCollider2D:
                    Gizmos.DrawWireCube(center, _collider.bounds.size);
                    break;
                case TilemapCollider2D:
                    DrawTilemapGizmos();
                    break;
                default:
                    Gizmos.DrawWireSphere(center, 0.5f);
                    break;
            }
        }

        /// <summary>
        /// 遍历 Tilemap 中实际存在瓦片的所有格子，按格子世界尺寸逐个绘制。
        /// 若找不到 Tilemap 组件或缓存为空，则退化为绘制整体 AABB。
        /// </summary>
        protected virtual void DrawTilemapGizmos()
        {
            if (_tilemap == null || !DrawTilemapCellsGizmo || _cachedTilePositions.Length == 0)
            {
                Gizmos.DrawWireCube(_cachedPosition, _halfExtents * 2f);
                return;
            }

            Vector3 cellSize = _tilemap.cellSize;
            Vector3 size = new Vector3(Mathf.Abs(cellSize.x), Mathf.Abs(cellSize.y), 0f);
            for (int i = 0; i < _cachedTilePositions.Length; i++)
            {
                Vector3Int cell = _cachedTilePositions[i];
                Vector3 world = _tilemap.CellToWorld(cell);
                Gizmos.DrawWireCube(new Vector3(world.x, world.y, 0f), size);
            }
        }

        /// <summary>
        /// 缓存所有存在瓦片的格子坐标。仅在 Awake / UpdateColliderCache / 外部主动调用时刷新。
        /// </summary>
        public virtual void RefreshTilemapCellCache()
        {
            if (_tilemap == null)
            {
                _cachedTilePositions = System.Array.Empty<Vector3Int>();
                return;
            }

            var tiles = new List<Vector3Int>();
            BoundsInt bounds = _tilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    if (_tilemap.HasTile(cell))
                        tiles.Add(cell);
                }
            }

            _cachedTilePositions = tiles.ToArray();
        }
    }
}
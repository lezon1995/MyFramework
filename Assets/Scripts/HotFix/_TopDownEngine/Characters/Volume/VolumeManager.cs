using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    #region Solid Collider Spatial Hash (embedded)

    /// <summary>
    /// 固体碰撞体专用空间分区（从 VolumeSpatialHash 独立出来）
    /// 负责管理场景中静态固体碰撞体的空间索引。
    /// </summary>
    public class VolumeColliderSpatialHash
    {
        public float CellSize => _cellSize;
        float _cellSize;
        float _invCellSize;
        Dictionary<(int, int), List<VolumeCollider>> _cells = new();
        Dictionary<VolumeCollider, int> _solidColliderCells = new();
        Dictionary<VolumeCollider, List<(int, int)>> _solidColliderKeys = new();

        public VolumeColliderSpatialHash(float cellSize)
        {
            _cellSize = cellSize;
            _invCellSize = 1f / cellSize;
        }

        public void Rebuild(List<VolumeCollider> solids)
        {
            Clear();
            int count = solids.Count;
            for (int i = 0; i < count; i++)
            {
                var solid = solids[i];
                if (solid != null)
                    Insert(solid);
            }
        }

        public void Clear()
        {
            foreach (var list in _cells.Values)
                ListPool<VolumeCollider>.Release(list);
            _cells.Clear();
            _solidColliderCells.Clear();
            foreach (var list in _solidColliderKeys.Values)
                ListPool<(int, int)>.Release(list);
            _solidColliderKeys.Clear();
        }

        public void Insert(VolumeCollider solid)
        {
            if (_solidColliderCells.ContainsKey(solid))
                return;

            solid.RefreshAfterMove();
            var bounds = solid.Collider.bounds;

            int minX = WorldToCell(bounds.min.x);
            int maxX = WorldToCell(bounds.max.x);
            int minY = WorldToCell(bounds.min.y);
            int maxY = WorldToCell(bounds.max.y);

            var keys = ListPool<(int, int)>.Get();
            for (int cx = minX; cx <= maxX; cx++)
            {
                for (int cy = minY; cy <= maxY; cy++)
                {
                    var key = CellToKey(cx, cy);
                    keys.Add(key);
                    if (!_cells.TryGetValue(key, out var list))
                    {
                        list = ListPool<VolumeCollider>.Get();
                        _cells[key] = list;
                    }

                    if (!list.Contains(solid))
                        list.Add(solid);
                }
            }

            _solidColliderCells[solid] = keys.Count;
            _solidColliderKeys[solid] = keys;
        }

        public void Remove(VolumeCollider solid)
        {
            if (!_solidColliderKeys.TryGetValue(solid, out var keys))
                return;

            int keyCount = keys.Count;
            for (int i = 0; i < keyCount; i++)
            {
                if (_cells.TryGetValue(keys[i], out var list))
                    list.Remove(solid);
            }

            ListPool<(int, int)>.Release(keys);
            _solidColliderKeys.Remove(solid);
            _solidColliderCells.Remove(solid);
        }

        /// <param name="results">结果追加到末尾，不清空。</param>
        public void GetPotentialSolids(TopDownController2D entity, List<VolumeCollider> results)
        {
            int cellX = WorldToCell(entity.CurPosition.x);
            int cellY = WorldToCell(entity.CurPosition.y);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var key = CellToKey(cellX + dx, cellY + dy);
                    if (_cells.TryGetValue(key, out var list))
                    {
                        int listCount = list.Count;
                        for (int i = 0; i < listCount; i++)
                        {
                            var item = list[i];
                            if (!item.IsEnabled())
                                continue;
                            if (!results.Contains(item))
                                results.Add(item);
                        }
                    }
                }
            }
        }

        int WorldToCell(float worldPos) => Mathf.FloorToInt(worldPos * _invCellSize);
        static (int, int) CellToKey(int x, int y) => (x, y);
    }

    #endregion

    public enum VolumeType
    {
        None,
        Entity,
        Collider,
    }

    /// <summary>
    /// 2D体积碰撞系统管理器
    /// 处理怪物/玩家之间的体积感、挤压感、链式击退等逻辑
    /// 不使用Unity内置物理系统，纯靠速度、质量和碰撞体大小来计算
    /// 性能优化：位运算配对去重 + 增量空间哈希 + 旁路式碰撞处理 + 预分配缓冲区
    /// </summary>
    public class VolumeManager : MainManagerBehaviour
    {
        [Header("系统设置")]
        [Tooltip("是否启用体积碰撞系统")]
        public bool Enabled = true;

        [Tooltip("系统更新频率（秒），设为0表示每帧更新")]
        public float UpdateInterval;

        [Header("调试")]
        [Tooltip("显示所有实体的碰撞范围")]
        public bool ShowAllGizmos;

        [Tooltip("显示空间分区网格")]
        public bool ShowSpatialHashGrid = true;

        public VolumeEntityManager entityManager;
        public VolumeColliderManager colliderManager;

        float _updateTimer;

        // 事件
        public event Action<VolumeCollisionEvent> OnCollisionDetected;
        public event Action<KnockbackEvent> OnKnockbackApplied;

        protected override void OnAwake()
        {
            base.OnAwake();
            InitializeSpatialHash();
        }

        void InitializeSpatialHash()
        {
            entityManager.InitializeSpatialHash();
            colliderManager.InitializeSpatialHash();
        }

        protected virtual void Start()
        {
            colliderManager.TryAutoDetectSolidColliders();
        }

        public override void OnFixedUpdate(float dt)
        {
            if (!Enabled)
                return;

            _updateTimer += dt;
            if (UpdateInterval > 0 && _updateTimer < UpdateInterval)
                return;

            _updateTimer = 0f;

            // 更新空间分区
            UpdateSpatialHash();

            // 实体↔实体碰撞（互相挤）
            ProcessAllCollisions(dt);
        }

        protected virtual void LateUpdate()
        {
            // 实体↔固体碰撞体碰撞（必须最后做，否则怪物下一次 LateUpdate 又会穿回去）
            // 这是最后一道保险：把实体推回表面外，并清理朝向墙的速度
            // var dt = Time.deltaTime;
            // colliderManager.ProcessSolidColliderCollisions(entityManager.Entities, dt);
        }

        #region Spatial Hash

        /// <summary>
        /// 更新空间分区（增量版：仅移动的实体才更新网格）
        /// </summary>
        void UpdateSpatialHash()
        {
            entityManager.UpdateSpatialHash();
            colliderManager.UpdateSpatialHash();
        }

        #endregion

        #region Solid Collider Registration

        /// <summary>
        /// 注册固体碰撞体
        /// </summary>
        public void RegisterSolidCollider(VolumeCollider collider) => colliderManager.Register(collider);

        /// <summary>
        /// 注销固体碰撞体
        /// </summary>
        public void UnregisterSolidCollider(VolumeCollider collider) => colliderManager.Unregister(collider);

        #endregion

        #region Entity Registration

        /// <summary>
        /// 注册实体到碰撞系统
        /// </summary>
        public void Register(TopDownController2D entity) => entityManager.Register(entity);

        /// <summary>
        /// 注销实体
        /// </summary>
        public void Unregister(TopDownController2D entity) => entityManager.Unregister(entity);

        /// <summary>
        /// 批量注册实体
        /// </summary>
        public void RegisterAll(List<TopDownController2D> entities)
        {
            foreach (var entity in entities)
            {
                Register(entity);
            }
        }

        /// <summary>
        /// 清空所有注册的实体
        /// </summary>
        public void ClearAll()
        {
            entityManager.ClearAll();
        }

        #endregion

        #region Collision Detection

        /// <summary>
        /// 处理所有碰撞检测（优化版，无 GC 路径）
        /// </summary>
        protected virtual void ProcessAllCollisions(float dt)
        {
            entityManager.ProcessAllCollisions(dt);
            colliderManager.ProcessSolidColliderCollisions(entityManager.Entities, dt);
        }

        #endregion

        #region Knockback System

        /// <summary>
        /// 对指定实体施打击退力
        /// </summary>
        public void ApplyKnockback(TopDownController2D target, Vector2 direction, float force)
        {
            if (force < 0.01f)
                return;

            float actualForce = force * (1f - target.KnockbackResistance);
            if (actualForce < 0.01f)
                return;

            target.AddImpact(direction, actualForce);

            OnKnockbackApplied?.Invoke(new KnockbackEvent
            {
                Source = null,
                Target = target,
                Direction = direction,
                OriginalForce = force,
                ActualForce = actualForce,
                ChainLevel = 0
            });
        }

        #endregion

        protected virtual void OnDrawGizmos()
        {
            // 实体和空间分区网格（仅运行时）
            if (Application.isPlaying)
            {
                if (ShowSpatialHashGrid)
                {
                    colliderManager.DrawSolidSpatialHashGrid(Color.yellow);
                }
            }
            else
            {
                // 编辑模式：显示所有实体的碰撞范围
                if (ShowAllGizmos)
                {
                    DrawEditorModeGizmos();
                }
            }
        }

        /// <summary>
        /// 编辑模式下的 Gizmos 绘制
        /// </summary>
        void DrawEditorModeGizmos()
        {
            // 显示空间分区网格布局预览
            if (ShowSpatialHashGrid)
            {
                DrawGridPreview();
            }
        }

        /// <summary>
        /// 在编辑器模式下预览网格布局
        /// </summary>
        void DrawGridPreview()
        {
            float cellSize = colliderManager.SpatialHashCellSize;
            float gridExtent = 10f; // 显示范围
            Vector3 center = transform.position;

            Gizmos.color = new Color(0f, 1f, 0f, colliderManager.SpatialHashGridAlpha);

            // 绘制一个范围内的网格预览
            for (float x = -gridExtent; x <= gridExtent; x += cellSize)
            {
                Gizmos.DrawLine(
                    new Vector3(x + center.x, center.y - gridExtent, center.z),
                    new Vector3(x + center.x, center.y + gridExtent, center.z)
                );
            }

            for (float y = -gridExtent; y <= gridExtent; y += cellSize)
            {
                Gizmos.DrawLine(
                    new Vector3(center.x - gridExtent, y + center.y, center.z),
                    new Vector3(center.x + gridExtent, y + center.y, center.z)
                );
            }

            // 绘制中心点
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(center, 0.2f);
        }
    }
}
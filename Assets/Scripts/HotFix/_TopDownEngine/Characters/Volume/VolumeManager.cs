using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    /// <summary>
    /// Hash Grid 空间分区
    /// 用于优化碰撞检测，将空间划分成网格，只检测相邻网格中的实体
    /// </summary>
    public class VolumeSpatialHash
    {
        float _cellSize;
        float _invCellSize;
        Dictionary<(int, int), List<TopDownController2D>> _cells = new();
        Dictionary<TopDownController2D, (int, int)> _entityCells = new();
        HashSet<int> _tempNeighborCells = new();

        public VolumeSpatialHash(float cellSize)
        {
            _cellSize = cellSize;
            _invCellSize = 1f / cellSize;
        }

        /// <summary>
        /// 重建整个空间分区（每帧调用）
        /// </summary>
        public void Rebuild(List<TopDownController2D> entities)
        {
            Clear();

            foreach (var entity in entities)
            {
                if (entity == null)
                    continue;

                Insert(entity);
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            foreach (var (key, list) in _cells)
            {
                ListPool<TopDownController2D>.Release(list);
            }

            _cells.Clear();
            _entityCells.Clear();
        }

        /// <summary>
        /// 插入实体到网格
        /// </summary>
        public void Insert(TopDownController2D entity)
        {
            if (entity == null)
                return;

            var cellKey = GetCellKey(entity.Position);
            if (!_cells.TryGetValue(cellKey, out var list))
            {
                list = ListPool<TopDownController2D>.Get();
                _cells[cellKey] = list;
            }

            list.Add(entity);
            _entityCells[entity] = cellKey;
        }

        /// <summary>
        /// 移除实体
        /// </summary>
        public void Remove(TopDownController2D entity)
        {
            if (entity == null)
                return;

            if (_entityCells.TryGetValue(entity, out var cellKey))
            {
                if (_cells.TryGetValue(cellKey, out var list))
                {
                    list.Remove(entity);
                }

                _entityCells.Remove(entity);
            }
        }

        /// <summary>
        /// 更新实体的网格位置
        /// </summary>
        public void UpdatePosition(TopDownController2D entity)
        {
            if (entity == null)
                return;

            var newKey = GetCellKey(entity.Position);
            if (_entityCells.TryGetValue(entity, out var oldKey) && oldKey == newKey)
            {
                return;
            }

            Remove(entity);
            Insert(entity);
        }

        /// <summary>
        /// 获取与指定实体可能发生碰撞的所有实体
        /// </summary>
        public void GetPotentialColliders(TopDownController2D entity, List<TopDownController2D> results)
        {
            results.Clear();

            if (entity == null)
                return;

            int cellX = WorldToCell(entity.Position.x);
            int cellY = WorldToCell(entity.Position.y);

            // 检测周围3x3的网格
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var key = CellToKey(cellX + dx, cellY + dy);
                    if (_cells.TryGetValue(key, out var list))
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var other = list[i];
                            if (other != entity)
                            {
                                results.Add(other);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取圆形区域内所有实体
        /// </summary>
        public void GetEntitiesInCircle(Vector2 center, float radius, List<TopDownController2D> results)
        {
            results.Clear();

            int minX = WorldToCell(center.x - radius);
            int maxX = WorldToCell(center.x + radius);
            int minY = WorldToCell(center.y - radius);
            int maxY = WorldToCell(center.y + radius);

            float radiusSq = radius * radius;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var key = CellToKey(x, y);
                    if (_cells.TryGetValue(key, out var list))
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var entity = list[i];
                            float distSq = (entity.Position - center).sqrMagnitude;
                            if (distSq <= radiusSq)
                            {
                                results.Add(entity);
                            }
                        }
                    }
                }
            }
        }

        public int CellCount => _cells.Count; // 获取网格单元数量
        public int EntityCount => _entityCells.Count; // 获取总实体数量

        /// <summary>
        /// 获取网格单元大小
        /// </summary>
        public float CellSize => _cellSize;

        #region Solid Collider Support

        // 固体碰撞体的空间分区数据
        Dictionary<(int, int), List<VolumeCollider>> _solidCells = new();
        Dictionary<VolumeCollider, int> _solidColliderCells = new();
        Dictionary<VolumeCollider, List<(int, int)>> _solidColliderKeys = new();

        /// <summary>
        /// 重建固体碰撞体空间分区
        /// </summary>
        public void RebuildSolids(List<VolumeCollider> solids)
        {
            ClearSolids();
            foreach (var solid in solids)
            {
                if (solid != null)
                    InsertAsSolid(solid);
            }
        }

        /// <summary>
        /// 清空固体碰撞体数据
        /// </summary>
        public void ClearSolids()
        {
            foreach (var (key, list) in _solidCells)
                ListPool<VolumeCollider>.Release(list);

            _solidCells.Clear();
            _solidColliderCells.Clear();

            foreach (var (key, list) in _solidColliderKeys)
                ListPool<(int, int)>.Release(list);
            _solidColliderKeys.Clear();
        }

        /// <summary>
        /// 插入固体碰撞体到网格（覆盖所有重叠的格子）
        /// </summary>
        public void InsertAsSolid(VolumeCollider solid)
        {
            if (solid == null) return;

            // 已经在列表中，跳过
            if (_solidColliderCells.ContainsKey(solid))
                return;

            solid.RefreshAfterMove();

            // 用包围半径覆盖所有可能涉及的格子
            var bounds = solid.Collider.bounds;
            int minX = WorldToCell(bounds.min.x);
            int maxX = WorldToCell(bounds.max.x);
            int minY = WorldToCell(bounds.min.y);
            int maxY = WorldToCell(bounds.max.y);

            // 记录所有涉及的格子 key（用于 Remove 时完整清理）
            var keys = ListPool<(int, int)>.Get();
            for (int cx = minX; cx <= maxX; cx++)
            {
                for (int cy = minY; cy <= maxY; cy++)
                {
                    var key = CellToKey(cx, cy);
                    keys.Add(key);
                    if (!_solidCells.TryGetValue(key, out var list))
                    {
                        list = ListPool<VolumeCollider>.Get();
                        _solidCells[key] = list;
                    }

                    if (!list.Contains(solid))
                    {
                        list.Add(solid);
                    }
                }
            }

            // 记录 solid 涉及的格子数量（移除时需要全部清理）
            _solidColliderCells[solid] = keys.Count;
            _solidColliderKeys[solid] = keys;
        }

        /// <summary>
        /// 移除固体碰撞体（从所有涉及的格子中删除）
        /// </summary>
        public void RemoveSolid(VolumeCollider solid)
        {
            if (solid == null) return;

            // 获取所有涉及的格子并删除
            if (_solidColliderKeys.TryGetValue(solid, out var keys))
            {
                foreach (var key in keys)
                {
                    if (_solidCells.TryGetValue(key, out var list))
                    {
                        list.Remove(solid);
                    }
                }

                ListPool<(int, int)>.Release(keys);
                _solidColliderKeys.Remove(solid);
            }

            _solidColliderCells.Remove(solid);
        }

        /// <summary>
        /// 获取与指定实体可能发生碰撞的固体碰撞体
        /// </summary>
        public void GetPotentialSolids(TopDownController2D entity, List<VolumeCollider> results)
        {
            results.Clear();
            if (entity == null)
                return;

            int cellX = WorldToCell(entity.Position.x);
            int cellY = WorldToCell(entity.Position.y);

            // 检测周围 3x3 的网格（固体碰撞体可能比较大）
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var key = CellToKey(cellX + dx, cellY + dy);
                    if (_solidCells.TryGetValue(key, out var list))
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var item = list[i];
                            if (!item.IsEnabled())
                                continue;
                            
                            if (results.Contains(item))
                                continue;

                            results.Add(item);
                        }
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        int WorldToCell(float worldPos)
        {
            return Mathf.FloorToInt(worldPos * _invCellSize);
        }

        (int, int) GetCellKey(Vector2 worldPos)
        {
            int x = WorldToCell(worldPos.x);
            int y = WorldToCell(worldPos.y);
            return CellToKey(x, y);
        }

        (int, int) CellToKey(int x, int y)
        {
            return (x, y);
            // 使用较大的质数来减少哈希冲突
            // return x * 73856093 ^ y * 19349663;
        }

        #endregion
    }

    /// <summary>
    /// 2D体积碰撞系统管理器
    /// 处理怪物/玩家之间的体积感、挤压感、链式击退等逻辑
    /// 不使用Unity内置物理系统，纯靠速度、质量和碰撞体大小来计算
    /// </summary>
    public class VolumeManager : MMSingleton<VolumeManager>
    {
        [Header("系统设置")] [Tooltip("是否启用体积碰撞系统")]
        public bool Enabled = true;

        [Tooltip("每帧最大碰撞检测次数（防止性能问题）")] public int MaxCollisionChecksPerFrame = 1000;

        [Tooltip("系统更新频率（秒），设为0表示每帧更新")] public float UpdateInterval;

        [Header("空间分区设置")] [Tooltip("空间分区网格大小（建议设置为最大碰撞半径的2-4倍）")]
        public float SpatialHashCellSize = 2f;

        [Tooltip("是否启用空间分区优化（关闭则使用O(N²)暴力检测）")]
        public bool EnableSpatialHash = true;

        [Header("碰撞参数")] [Tooltip("基础分离力")] public float BaseSeparationForce = 10f;

        [Tooltip("质量差影响系数")] [Range(0f, 2f)] public float MassDifferenceInfluence = 0.5f;

        [Tooltip("速度差影响系数")] [Range(0f, 2f)] public float VelocityDifferenceInfluence = 0.3f;

        [Header("链式击退参数")] [Tooltip("链式击退开关")] public bool EnableChainKnockback = true;

        [Tooltip("链式击退最大传播层级")] [Range(1, 10)] public int MaxChainLevel = 5;

        [Tooltip("每级链式击退的衰减比率（0-1）")] [Range(0f, 1f)]
        public float ChainDecayRatio = 0.6f;

        [Tooltip("链式击退检测半径乘数")] [Range(1f, 3f)]
        public float ChainKnockbackRadiusMultiplier = 1.5f;

        [Tooltip("触发链式击退的最小击退力")] public float MinChainKnockbackForce = 2f;

        [Header("软排斥参数（防止抖动）")] [Tooltip("启用软排斥：当两实体距离小于此距离乘数时，产生柔和的排斥力，避免贴在一起")]
        public bool EnableSoftRepulsion = true;

        [Tooltip("软排斥作用距离（乘以两实体半径和），大于此距离无排斥力")] [Range(1f, 3f)]
        public float SoftRepulsionDistanceRatio = 1.5f;

        [Tooltip("软排斥力强度")] [Range(0f, 20f)] public float SoftRepulsionStrength = 5f;

        [Tooltip("软排斥力作用于速度还是位置（true=位置瞬移，false=力影响速度）")]
        public bool SoftRepulsionAffectsPosition = false;

        [Header("调试")] [Tooltip("显示所有实体的碰撞范围")]
        public bool ShowAllGizmos;

        [Tooltip("显示碰撞连线")] public bool ShowCollisionLines;

        [Tooltip("显示击退方向")] public bool ShowKnockbackDirections;

        [Tooltip("显示软排斥力")] public bool ShowSoftRepulsion;

        [Tooltip("显示空间分区网格")] public bool ShowSpatialHashGrid = true;

        [Tooltip("空间分区网格透明度")] [Range(0.1f, 1f)]
        public float SpatialHashGridAlpha = 0.3f;

        [Header("固体碰撞体（边界/障碍物）")] [Tooltip("启用固体碰撞体碰撞检测")]
        public bool EnableSolidColliders = true;

        [Tooltip("是否自动检测场景中的 VolumeCollider")] public bool AutoDetectVolumeColliders = true;

        // 空间分区
        VolumeSpatialHash _spatialHash;
        VolumeSpatialHash _solidColliderSpatialHash;

        // 运行时数据 - 实体
        List<TopDownController2D> _registeredEntities = new();
        List<TopDownController2D> _potentialColliders = new();
        List<VolumeCollisionResult> _collisionResults = new();
        List<KnockbackChainResult> _knockbackChain = new();
        Queue<TopDownController2D> _entityQueue = new();
        HashSet<TopDownController2D> _visitedSet = new();
        HashSet<int> _processedPairKeys = new();

        // 运行时数据 - 固体碰撞体
        List<VolumeCollider> _solidColliders = new();
        List<VolumeCollider> _potentialSolidColliders = new();
        HashSet<int> _processedSolidPairKeys = new();

        int _collisionCheckCount;
        float _updateTimer;
        int _totalEntitiesLastFrame;

        // 事件
        public event Action<VolumeCollisionEvent> OnCollisionDetected;
        public event Action<KnockbackEvent> OnKnockbackApplied;

        /// <summary>
        /// Statics initialization to support enter play modes
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void InitializeStatics()
        {
            _instance = null;
        }

        protected override void Awake()
        {
            base.Awake();
            InitializeSpatialHash();
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
                _instance = null;
        }

        void InitializeSpatialHash()
        {
            _spatialHash = new(SpatialHashCellSize);
            _solidColliderSpatialHash = new(SpatialHashCellSize);
        }

        protected virtual void Start()
        {
            if (AutoDetectVolumeColliders)
            {
                AutoDetectSolidColliders();
            }
        }

        protected virtual void Update()
        {
            if (!Enabled)
                return;

            var dt = Time.deltaTime;
            _updateTimer += dt;
            if (UpdateInterval > 0 && _updateTimer < UpdateInterval)
                return;

            _updateTimer = 0f;

            // 更新空间分区
            if (EnableSpatialHash)
            {
                UpdateSpatialHash();
            }

            // 实体↔实体碰撞（互相挤）
            ProcessAllCollisions(dt);

            _totalEntitiesLastFrame = _registeredEntities.Count;
        }

        protected virtual void LateUpdate()
        {
            // 击退力应用（在 LateUpdate 中，确保击退应用到当前位置后能立即被纠正）
            ApplyAllKnockbackForces();

            // 实体↔固体碰撞体碰撞（必须最后做，否则怪物下一次 LateUpdate 又会穿回去）
            // 这是最后一道保险：把实体推回表面外，并清理朝向墙的速度
            if (EnableSolidColliders)
            {
                var dt = Time.deltaTime;
                ProcessSolidColliderCollisions(dt);
            }
        }

        #region Spatial Hash

        /// <summary>
        /// 更新空间分区
        /// </summary>
        void UpdateSpatialHash()
        {
            if (_spatialHash == null)
            {
                InitializeSpatialHash();
            }

            // 重建整个网格（实体位置变化了）
            _spatialHash.Rebuild(_registeredEntities);

            // 重建固体碰撞体的空间分区
            if (EnableSolidColliders && _solidColliderSpatialHash != null)
            {
                RebuildSolidColliderSpatialHash();
            }
        }

        /// <summary>
        /// 获取指定实体的潜在碰撞体
        /// </summary>
        void GetPotentialColliders(TopDownController2D entity)
        {
            _potentialColliders.Clear();

            if (EnableSpatialHash && _spatialHash != null)
            {
                // 使用空间分区获取潜在碰撞体
                _spatialHash.GetPotentialColliders(entity, _potentialColliders);
            }
            else
            {
                // 回退到暴力检测
                foreach (var other in _registeredEntities)
                {
                    if (other != entity)
                        _potentialColliders.Add(other);
                }
            }
        }

        #endregion

        #region Solid Collider Registration

        /// <summary>
        /// 自动检测场景中的 VolumeCollider 并注册
        /// </summary>
        [ContextMenu("Auto Detect Solid Colliders")]
        public void AutoDetectSolidColliders()
        {
            _solidColliders.Clear();

            var colliders = FindObjectsByType<VolumeCollider>(FindObjectsSortMode.None);
            foreach (var col in colliders)
            {
                if (col.IsEnabled())
                {
                    RegisterSolidCollider(col);
                }
            }

            Debug.Log($"[VolumeManager] 自动检测到 {_solidColliders.Count} 个固体碰撞体", this);
        }

        /// <summary>
        /// 注册固体碰撞体
        /// </summary>
        public void RegisterSolidCollider(VolumeCollider collider)
        {
            if (collider == null || _solidColliders.Contains(collider))
                return;

            _solidColliders.Add(collider);
            _solidColliderSpatialHash?.InsertAsSolid(collider);
        }

        /// <summary>
        /// 注销固体碰撞体
        /// </summary>
        public void UnregisterSolidCollider(VolumeCollider collider)
        {
            if (collider == null)
                return;

            _solidColliders.Remove(collider);
            _solidColliderSpatialHash?.RemoveSolid(collider);
        }

        /// <summary>
        /// 当固体碰撞体移动时调用（由 VolumeCollider 在 Update 中调用）
        /// </summary>
        public void NotifyColliderMoved(VolumeCollider collider)
        {
            // 空间分区会在下一帧自动重建，不需要手动更新
        }

        /// <summary>
        /// 重建固体碰撞体的空间分区
        /// </summary>
        void RebuildSolidColliderSpatialHash()
        {
            _solidColliderSpatialHash.RebuildSolids(_solidColliders);
        }

        /// <summary>
        /// 获取指定实体的潜在固体碰撞体
        /// </summary>
        void GetPotentialSolidColliders(TopDownController2D entity)
        {
            _potentialSolidColliders.Clear();

            if (EnableSpatialHash && _solidColliderSpatialHash != null)
            {
                _solidColliderSpatialHash.GetPotentialSolids(entity, _potentialSolidColliders);
            }
            else
            {
                _potentialSolidColliders.AddRange(_solidColliders);
            }
        }

        /// <summary>
        /// 处理实体与固体碰撞体的碰撞
        /// </summary>
        protected virtual void ProcessSolidColliderCollisions(float dt)
        {
            if (_solidColliders.Count == 0)
                return;

            foreach (var entity in _registeredEntities)
            {
                if (entity == null)
                    continue;

                GetPotentialSolidColliders(entity);

                foreach (var solid in _potentialSolidColliders)
                {
                    if (solid == null || !solid.IsEnabled())
                        continue;

                    ProcessEntitySolidCollision(entity, solid, dt);
                }
            }
        }

        /// <summary>
        /// 处理单个实体与固体碰撞体的碰撞
        /// </summary>
        protected virtual void ProcessEntitySolidCollision(TopDownController2D entity, VolumeCollider solid, float dt)
        {
            var result = new VolumeColliderCollisionResult(entity, solid);

            if (!result.IsColliding)
                return;

            // 1. 位置分离：把实体推到表面外（重叠量 + 一点点缓冲，避免下一帧又穿透）
            //    必须保证能在一帧内清掉所有重叠，否则会被持续推 → 抖动
            float pushDistance = result.Overlap + 0.001f;
            Vector2 pushDir = result.SurfaceNormal;
            entity.Position += pushDir * pushDistance;
            entity.transform.position = entity.Position;

            // 2. 速度处理：实体朝墙方向的速度分量需要清除
            //    SurfaceNormal 是从墙指向实体的方向，所以沿这个方向的速度是"远离墙"的，
            //    沿 -SurfaceNormal 的速度才是"撞向墙"，要被消除
            Vector2 totalVel = entity.IntentVelocity + entity.KnockbackVelocity;
            float velIntoWall = Vector2.Dot(totalVel, -pushDir);

            if (velIntoWall > 0)
            {
                // 撞墙中，清除指向墙的速度分量（按质量比保留部分动能）
                // 固体质量视为无限大，所以击退速度完全被挡
                float restitution = 0f; // 不反弹
                Vector2 reflectedVel = totalVel - (-pushDir) * (velIntoWall * (1f + restitution));
                // 把反射后的总速度拆分到 IntentVelocity 和 KnockbackVelocity
                // 简单起见，全部作用在 KnockbackVelocity（IntentVelocity 通常较小）
                Vector2 newTotal = reflectedVel;

                // 保留 IntentVelocity 的切向分量，把垂直分量设为 0
                float intentNormal = Vector2.Dot(entity.IntentVelocity, -pushDir);
                if (intentNormal > 0)
                {
                    entity.IntentVelocity += (Vector3)pushDir * intentNormal;
                }

                // KnockbackVelocity 剩余部分补到 total
                Vector2 intentRemaining = entity.IntentVelocity;
                Vector2 neededKnockback = newTotal - intentRemaining;
                entity.KnockbackVelocity = neededKnockback;
            }
        }

        #endregion

        #region Entity Registration

        /// <summary>
        /// 注册实体到碰撞系统
        /// </summary>
        public void Register(TopDownController2D entity)
        {
            if (entity == null || entity.IsRegistered)
                return;

            _registeredEntities.Add(entity);
            entity.IsRegistered = true;

            // 插入到空间分区
            _spatialHash?.Insert(entity);
        }

        /// <summary>
        /// 注销实体
        /// </summary>
        public void Unregister(TopDownController2D entity)
        {
            if (entity == null || !entity.IsRegistered)
                return;

            _registeredEntities.Remove(entity);
            entity.IsRegistered = false;

            // 从空间分区移除
            _spatialHash?.Remove(entity);
        }

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
            foreach (var entity in _registeredEntities)
            {
                if (entity != null)
                    entity.IsRegistered = false;
            }

            _registeredEntities.Clear();
            _spatialHash?.Clear();
        }

        #endregion

        #region Collision Detection

        /// <summary>
        /// 处理所有碰撞检测（优化版）
        /// </summary>
        protected virtual void ProcessAllCollisions(float dt)
        {
            _collisionCheckCount = 0;
            _collisionResults.Clear();

            if (EnableSpatialHash && _spatialHash != null)
            {
                ProcessCollisionsWithSpatialHash(dt);
            }
            else
            {
                ProcessCollisionsBruteForce(dt);
            }
        }

        /// <summary>
        /// 使用空间分区的碰撞检测
        /// </summary>
        void ProcessCollisionsWithSpatialHash(float dt)
        {
            _processedPairKeys.Clear();

            foreach (var entity in _registeredEntities)
            {
                if (entity == null)
                    continue;

                // 获取潜在碰撞体
                GetPotentialColliders(entity);

                foreach (var other in _potentialColliders)
                {
                    if (other == null)
                        continue;

                    // 生成配对唯一键（使用 InstanceID 保证唯一性）
                    int idA = entity.GetInstanceID();
                    int idB = other.GetInstanceID();
                    int pairKey = idA < idB ? idA * 31 + idB : idB * 31 + idA;

                    // 避免重复检测同一对
                    if (_processedPairKeys.Contains(pairKey))
                        continue;

                    _processedPairKeys.Add(pairKey);

                    if (_collisionCheckCount >= MaxCollisionChecksPerFrame)
                        return;

                    ProcessPairCollision(entity, other, dt);
                    _collisionCheckCount++;
                }
            }
        }

        /// <summary>
        /// 暴力碰撞检测（O(N²)，作为回退）
        /// </summary>
        void ProcessCollisionsBruteForce(float dt)
        {
            int count = _registeredEntities.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (_collisionCheckCount >= MaxCollisionChecksPerFrame)
                        return;

                    var entityA = _registeredEntities[i];
                    var entityB = _registeredEntities[j];

                    if (entityA == null || entityB == null)
                        continue;

                    ProcessPairCollision(entityA, entityB, dt);
                    _collisionCheckCount++;
                }
            }
        }

        /// <summary>
        /// 处理一对实体的碰撞
        /// </summary>
        protected virtual void ProcessPairCollision(TopDownController2D a, TopDownController2D b, float dt)
        {
            var result = new VolumeCollisionResult(a, b);

            // 软排斥：在实体未重叠时也施加柔和的排斥力，避免贴在一起导致抖动
            if (EnableSoftRepulsion)
            {
                CalculateSoftRepulsion(a, b, result, dt);
            }

            if (!result.IsColliding)
                return;

            _collisionResults.Add(result);

            // 计算分离
            if (result.IsExceedingMaxOverlap)
            {
                CalculateSeparation(a, b, result, dt);
            }

            // 计算挤压
            CalculateSqueeze(a, b, result, dt);

            // 触发事件
            var evt = new VolumeCollisionEvent
            {
                Self = a,
                Other = b,
                Result = result,
                DeltaTime = dt
            };
            OnCollisionDetected?.Invoke(evt);

            // 反向触发
            var evtB = new VolumeCollisionEvent
            {
                Self = b,
                Other = a,
                Result = new(b, a),
                DeltaTime = dt
            };
            OnCollisionDetected?.Invoke(evtB);
        }

        /// <summary>
        /// 计算软排斥力
        /// 当两实体的距离小于"半径和 × SoftRepulsionDistanceRatio"时，产生柔和的排斥力
        /// 这可以防止实体因为"分离阈值"过低而挤在一起抖动
        /// </summary>
        protected virtual void CalculateSoftRepulsion(TopDownController2D a, TopDownController2D b, VolumeCollisionResult result, float dt)
        {
            float repulsionRadius = (a.Radius + b.Radius) * SoftRepulsionDistanceRatio;

            // 超出软排斥范围，无作用
            if (result.CenterDistance >= repulsionRadius)
                return;

            if (result.CenterDistance < 0.001f)
                return;

            // 计算排斥强度（0-1之间，距离越近越强）
            float strength = 1f - (result.CenterDistance / repulsionRadius);
            strength *= strength; // 平方曲线让近距离排斥更明显

            float totalMass = a.CollisionMass + b.CollisionMass;
            if (totalMass <= 0)
                return;

            // 质量大的排斥小，质量小的排斥大（与分离方向相反）
            float ratioA = b.CollisionMass / totalMass;
            float ratioB = a.CollisionMass / totalMass;

            // 从 A 指向 B 的方向（无论是否重叠都要正确计算）
            Vector2 repelDir = (b.Position - a.Position) / result.CenterDistance;
            float repelForce = SoftRepulsionStrength * strength * dt;

            if (SoftRepulsionAffectsPosition)
            {
                // 直接修改位置
                a.Position -= repelDir * (repelForce * ratioA);
                b.Position += repelDir * (repelForce * ratioB);
                a.transform.position = a.Position;
                b.transform.position = b.Position;
            }
            else
            {
                // 作用在意图速度上（更平滑，符合物理直觉）
                a.IntentVelocity -= (Vector3)repelDir * (repelForce * ratioA);
                b.IntentVelocity += (Vector3)repelDir * (repelForce * ratioB);
            }
        }

        /// <summary>
        /// 计算分离（当重叠超出最大允许时）
        /// </summary>
        protected virtual void CalculateSeparation(TopDownController2D a, TopDownController2D b, VolumeCollisionResult result, float dt)
        {
            float totalMass = a.CollisionMass + b.CollisionMass;
            if (totalMass <= 0) return;

            float ratioA = b.CollisionMass / totalMass;
            float ratioB = a.CollisionMass / totalMass;

            float separationForce = BaseSeparationForce * dt;
            Vector2 separationDir = result.Direction;

            float sepA = result.RequiredSeparation * ratioA * separationForce;
            float sepB = result.RequiredSeparation * ratioB * separationForce;

            a.Position -= separationDir * sepA;
            b.Position += separationDir * sepB;

            a.transform.position = a.Position;
            b.transform.position = b.Position;
        }

        /// <summary>
        /// 计算挤压（基于速度和质量）
        /// </summary>
        protected virtual void CalculateSqueeze(TopDownController2D a, TopDownController2D b, VolumeCollisionResult result, float dt)
        {
            float overlap = result.Overlap;
            if (overlap < 0.01f) return;

            // 挤压作用在"总速度"上（包括意图和击退），因为挤压需要反映真实的相对运动
            Vector2 relativeVel = a.TotalVelocity - b.TotalVelocity;
            float relativeSpeed = relativeVel.magnitude;

            if (relativeSpeed < 0.01f) return;

            float velAlongCollision = Vector2.Dot(relativeVel, result.Direction);

            if (velAlongCollision <= 0) return;

            float totalMass = a.CollisionMass + b.CollisionMass;
            float massRatioA = a.CollisionMass / totalMass;
            float massRatioB = b.CollisionMass / totalMass;

            float squeezeStrength = relativeSpeed * (1f + MassDifferenceInfluence) * (1f + VelocityDifferenceInfluence);

            float squeezeA = velAlongCollision * massRatioB * squeezeStrength * dt;
            float squeezeB = velAlongCollision * massRatioA * squeezeStrength * dt;

            Vector2 squeezeDir = -result.Direction;

            // 挤压结果加在意图速度上（影响后续的 AI 决策）
            a.IntentVelocity += (Vector3)squeezeDir * (squeezeA * 0.5f);
            b.IntentVelocity += (Vector3)squeezeDir * (squeezeB * 0.5f);
        }

        #endregion

        #region Knockback System

        /// <summary>
        /// 对指定实体施打击退力
        /// </summary>
        public void ApplyKnockback(TopDownController2D target, Vector2 direction, float force)
        {
            if (target == null || force < 0.01f)
                return;

            float actualForce = force * (1f - target.KnockbackResistance);
            if (actualForce < 0.01f)
                return;

            target.AddImpact(direction, actualForce);

            if (EnableChainKnockback && force >= MinChainKnockbackForce)
            {
                ProcessChainKnockback(target, direction, force);
            }

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

        /// <summary>
        /// 处理链式击退
        /// 链式击退原理：当A被击退时，A会连带击退在A身后（击退方向相反）的其他实体B
        /// B又会连带击退在B身后的实体C，以此类推
        /// </summary>
        protected virtual void ProcessChainKnockback(TopDownController2D source, Vector2 direction, float force)
        {
            _knockbackChain.Clear();
            _visitedSet.Clear();

            // 链式传播：source(0级) -> 1级 -> 2级 -> ...
            using var _ = new DicScope<TopDownController2D, int>(out var entityLevels);

            // BFS队列，每层记录当前位置
            _entityQueue.Clear();
            _entityQueue.Enqueue(source);
            _visitedSet.Add(source);
            entityLevels[source] = 0;

            int maxIterations = _registeredEntities.Count * 2;
            int iterations = 0;

            while (_entityQueue.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                var current = _entityQueue.Dequeue();
                int currentLevel = entityLevels[current];

                // 超过最大层级，不再传播
                if (currentLevel >= MaxChainLevel) continue;

                // 当前层级的检测范围（考虑层级衰减）
                float levelFactor = 1f - (currentLevel * 0.1f);
                float checkRadius = current.Radius * ChainKnockbackRadiusMultiplier * 2f * levelFactor;

                // 获取当前位置周围的实体
                List<TopDownController2D> neighbors;
                if (EnableSpatialHash && _spatialHash != null)
                {
                    neighbors = GetEntitiesInRadiusInternal(current.Position, checkRadius);
                }
                else
                {
                    neighbors = GetEntitiesInRadiusInternal(current.Position, checkRadius);
                }

                foreach (var other in neighbors)
                {
                    if (other == null || _visitedSet.Contains(other) || other == source)
                        continue;

                    // 检查是否在击退方向的后方（相对于当前实体）
                    Vector2 toOther = other.Position - current.Position;
                    float dist = toOther.magnitude;
                    if (dist < 0.01f)
                        continue;
                    toOther /= dist;

                    // 点积 < 0 表示 angle > 90°，即 other 在 current 的身后方向
                    float dot = Vector2.Dot(direction, toOther);

                    if (dot < -0.3f) // 约107度范围内
                    {
                        _visitedSet.Add(other);
                        int nextLevel = currentLevel + 1;
                        entityLevels[other] = nextLevel;
                        _entityQueue.Enqueue(other);

                        // 计算链式击退力（逐级衰减）
                        float chainForce = CalculateChainForce(force, nextLevel);

                        if (chainForce > 0.01f)
                        {
                            _knockbackChain.Add(new KnockbackChainResult
                            {
                                Target = other,
                                OriginalForce = force,
                                ActualForce = chainForce,
                                Direction = direction,
                                ChainLevel = nextLevel
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定位置周围指定半径内的实体（内部使用，不分配新列表）
        /// </summary>
        List<TopDownController2D> GetEntitiesInRadiusInternal(Vector2 position, float radius)
        {
            _potentialColliders.Clear();
            float radiusSq = radius * radius;

            foreach (var entity in _registeredEntities)
            {
                if (entity == null)
                    continue;

                float distSq = (entity.Position - position).sqrMagnitude;
                if (distSq <= radiusSq)
                {
                    _potentialColliders.Add(entity);
                }
            }

            return _potentialColliders;
        }

        /// <summary>
        /// 计算链式击退力（经过衰减）
        /// </summary>
        protected virtual float CalculateChainForce(float originalForce, int chainLevel)
        {
            if (chainLevel <= 0) return originalForce;
            return originalForce * Mathf.Pow(ChainDecayRatio, chainLevel);
        }

        /// <summary>
        /// 应用所有链式击退
        /// </summary>
        protected virtual void ApplyAllKnockbackForces()
        {
            foreach (var chain in _knockbackChain)
            {
                if (!chain.IsValid)
                    continue;

                chain.Target.AddImpact(chain.Direction, chain.ActualForce);

                OnKnockbackApplied?.Invoke(new KnockbackEvent
                {
                    Source = null,
                    Target = chain.Target,
                    Direction = chain.Direction,
                    OriginalForce = chain.OriginalForce,
                    ActualForce = chain.ActualForce,
                    ChainLevel = chain.ChainLevel
                });
            }
        }

        #endregion

        #region Query

        /// <summary>
        /// 获取指定点周围的所有实体
        /// </summary>
        public void GetEntitiesInRadius(Vector2 position, float radius, ref List<TopDownController2D> result)
        {
            result.Clear();
            if (EnableSpatialHash && _spatialHash != null)
            {
                _spatialHash.GetEntitiesInCircle(position, radius, result);
            }
            else
            {
                float radiusSq = radius * radius;
                foreach (var entity in _registeredEntities)
                {
                    if (entity == null)
                        continue;

                    float distSq = (entity.Position - position).sqrMagnitude;
                    if (distSq <= radiusSq)
                    {
                        result.Add(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 获取最近的可碰撞实体
        /// </summary>
        public TopDownController2D GetNearestEntity(Vector2 position, float maxDistance = float.MaxValue)
        {
            TopDownController2D nearest = null;
            float nearestDistSq = maxDistance * maxDistance;

            foreach (var entity in _registeredEntities)
            {
                if (entity == null)
                    continue;

                float distSq = (entity.Position - position).sqrMagnitude;
                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    nearest = entity;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 圆形碰撞检测
        /// </summary>
        public bool CircleIntersectsCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            float distSq = (centerA - centerB).sqrMagnitude;
            float radiusSum = radiusA + radiusB;
            return distSq <= radiusSum * radiusSum;
        }

        /// <summary>
        /// 点是否在圆内
        /// </summary>
        public bool PointInCircle(Vector2 point, Vector2 center, float radius)
        {
            return (point - center).sqrMagnitude <= radius * radius;
        }

        #endregion

        protected virtual void OnDrawGizmos()
        {
            // 实体和空间分区网格（仅运行时）
            if (Application.isPlaying)
            {
                if (ShowAllGizmos)
                {
                    Gizmos.color = Color.cyan;
                    foreach (var entity in _registeredEntities)
                    {
                        if (entity == null)
                            continue;

                        Gizmos.DrawWireSphere(entity.Position, entity.Radius);
                    }
                }

                if (ShowSpatialHashGrid && _spatialHash != null)
                {
                    DrawSpatialHashGrid(_spatialHash, Color.green);
                }

                if (ShowSpatialHashGrid && EnableSolidColliders && _solidColliderSpatialHash != null)
                {
                    DrawSpatialHashGrid(_solidColliderSpatialHash, Color.yellow);
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
            float cellSize = SpatialHashCellSize;
            float gridExtent = 10f; // 显示范围
            Vector3 center = transform.position;

            Gizmos.color = new Color(0f, 1f, 0f, SpatialHashGridAlpha);

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

        void DrawSpatialHashGrid(VolumeSpatialHash spatialHash, Color color)
        {
            float cellSize = spatialHash.CellSize;
            float alpha = SpatialHashGridAlpha;

            foreach (var entity in _registeredEntities)
            {
                if (entity == null)
                    continue;

                // 获取实体所在的格子坐标
                int cellX = Mathf.FloorToInt(entity.Position.x / cellSize);
                int cellY = Mathf.FloorToInt(entity.Position.y / cellSize);

                // 绘制周围 3x3 的格子
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int cx = cellX + dx;
                        int cy = cellY + dy;

                        Vector3 center = new Vector3(
                            (cx + 0.5f) * cellSize,
                            (cy + 0.5f) * cellSize,
                            0f
                        );

                        Vector3 size = Vector3.one * cellSize;

                        Gizmos.color = new Color(color.r, color.g, color.b, alpha);
                        Gizmos.DrawWireCube(center, size);
                    }
                }
            }

            // 如果有实体，也绘制它们所在格子边框（更明显）
            if (_registeredEntities.Count > 0)
            {
                Gizmos.color = new Color(color.r, color.g, color.b, alpha * 2f);
            }
        }
    }
}
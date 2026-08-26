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
    class VolumeSolidSpatialHash
    {
        public float CellSize => _cellSize;
        float _cellSize;
        float _invCellSize;
        Dictionary<(int, int), List<VolumeCollider>> _cells = new();
        Dictionary<VolumeCollider, int> _solidColliderCells = new();
        Dictionary<VolumeCollider, List<(int, int)>> _solidColliderKeys = new();

        public VolumeSolidSpatialHash(float cellSize)
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
            if (solid == null) return;
            if (_solidColliderCells.ContainsKey(solid)) return;

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
            if (solid == null) return;
            if (!_solidColliderKeys.TryGetValue(solid, out var keys)) return;
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
            if (entity == null) return;
            int cellX = WorldToCell(entity.Position.x);
            int cellY = WorldToCell(entity.Position.y);

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

        [Tooltip("每帧最大碰撞检测次数（防止性能问题）")]
        public int MaxCollisionChecksPerFrame = 1000;

        [Tooltip("系统更新频率（秒），设为0表示每帧更新")]
        public float UpdateInterval;

        [Header("空间分区设置")]
        [Tooltip("空间分区网格大小（建议设置为最大碰撞半径的2-4倍）")]
        public float SpatialHashCellSize = 2f;

        [Tooltip("是否启用空间分区优化（关闭则使用O(N²)暴力检测）")]
        public bool EnableSpatialHash = true;

        [Tooltip("是否使用增量空间哈希更新（仅移动的实体才更新网格，默认开启）")]
        public bool UseIncrementalSpatialHash = true;

        [Header("碰撞参数")]
        [Tooltip("基础分离力")]
        public float BaseSeparationForce = 10f;

        [Tooltip("质量差影响系数")]
        [Range(0f, 2f)]
        public float MassDifferenceInfluence = 0.5f;

        [Tooltip("速度差影响系数")]
        [Range(0f, 2f)]
        public float VelocityDifferenceInfluence = 0.3f;

        [Header("链式击退参数")]
        [Tooltip("链式击退开关")]
        public bool EnableChainKnockback = true;

        [Tooltip("链式击退最大传播层级")]
        [Range(1, 10)]
        public int MaxChainLevel = 5;

        [Tooltip("每级链式击退的衰减比率（0-1）")]
        [Range(0f, 1f)]
        public float ChainDecayRatio = 0.6f;

        [Tooltip("链式击退检测半径乘数")]
        [Range(1f, 3f)]
        public float ChainKnockbackRadiusMultiplier = 1.5f;

        [Tooltip("触发链式击退的最小击退力")]
        public float MinChainKnockbackForce = 2f;

        [Header("软排斥参数（防止抖动）")]
        [Tooltip("启用软排斥：当两实体距离小于此距离乘数时，产生柔和的排斥力，避免贴在一起")]
        public bool EnableSoftRepulsion = true;

        [Tooltip("软排斥作用距离（乘以两实体半径和），大于此距离无排斥力")]
        [Range(1f, 3f)]
        public float SoftRepulsionDistanceRatio = 1.5f;

        [Tooltip("软排斥力强度")]
        [Range(0f, 20f)]
        public float SoftRepulsionStrength = 5f;

        [Tooltip("软排斥力作用于速度还是位置（true=位置瞬移，false=力影响速度）")]
        public bool SoftRepulsionAffectsPosition = false;

        [Header("调试")]
        [Tooltip("显示所有实体的碰撞范围")]
        public bool ShowAllGizmos;

        [Tooltip("显示碰撞连线")]
        public bool ShowCollisionLines;

        [Tooltip("显示击退方向")]
        public bool ShowKnockbackDirections;

        [Tooltip("显示软排斥力")]
        public bool ShowSoftRepulsion;

        [Tooltip("显示空间分区网格")]
        public bool ShowSpatialHashGrid = true;

        [Tooltip("空间分区网格透明度")]
        [Range(0.1f, 1f)]
        public float SpatialHashGridAlpha = 0.3f;

        [Header("固体碰撞体（边界/障碍物）")]
        [Tooltip("启用固体碰撞体碰撞检测")]
        public bool EnableSolidColliders = true;

        [Tooltip("是否自动检测场景中的 VolumeCollider")]
        public bool AutoDetectVolumeColliders = true;

        // 空间分区
        VolumeSpatialHash _spatialHash;
        VolumeSolidSpatialHash _solidSpatialHash;

        // 运行时数据 - 实体
        List<TopDownController2D> _registeredEntities = new();
        List<TopDownController2D> _potentialColliders = new();
        List<KnockbackChainResult> _knockbackChain = new();
        Queue<TopDownController2D> _entityQueue = new();
        HashSet<TopDownController2D> _visitedSet = new();

        // 配对去重：位运算版（避免每帧 new HashSet）
        uint[] _pairKeysBuffer = new uint[4096];
        int _pairKeysCount;

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

        protected override void OnAwake()
        {
            base.OnAwake();
            InitializeSpatialHash();
        }

        void InitializeSpatialHash()
        {
            _spatialHash = new VolumeSpatialHash(SpatialHashCellSize);
            _solidSpatialHash = new VolumeSolidSpatialHash(SpatialHashCellSize);
        }

        protected virtual void Start()
        {
            if (AutoDetectVolumeColliders)
            {
                AutoDetectSolidColliders();
            }
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            if (!Enabled)
                return;

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
        /// 更新空间分区（增量版：仅移动的实体才更新网格）
        /// </summary>
        void UpdateSpatialHash()
        {
            if (_spatialHash == null)
            {
                InitializeSpatialHash();
            }

            if (UseIncrementalSpatialHash)
            {
                // 增量更新：仅当格子变化时才重建
                _spatialHash.IncrementalUpdate(_registeredEntities);
            }
            else
            {
                // 全量重建（实体大幅变化时使用）
                _spatialHash.Rebuild(_registeredEntities);
            }

            // 固体碰撞体通常不移动，保持全量重建
            if (EnableSolidColliders && _solidSpatialHash != null)
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

            var colliders = FindObjectsByType<VolumeCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var col in colliders)
            {
                if (col.IsEnabled() && col.AutoRegister)
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
            _solidSpatialHash?.Insert(collider);
        }

        /// <summary>
        /// 注销固体碰撞体
        /// </summary>
        public void UnregisterSolidCollider(VolumeCollider collider)
        {
            if (collider == null)
                return;

            _solidColliders.Remove(collider);
            _solidSpatialHash?.Remove(collider);
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
            _solidSpatialHash.Rebuild(_solidColliders);
        }

        /// <summary>
        /// 获取指定实体的潜在固体碰撞体
        /// </summary>
        void GetPotentialSolidColliders(TopDownController2D entity)
        {
            _potentialSolidColliders.Clear();

            if (EnableSpatialHash && _solidSpatialHash != null)
            {
                _solidSpatialHash.GetPotentialSolids(entity, _potentialSolidColliders);
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
        /// 处理所有碰撞检测（优化版，无 GC 路径）
        /// </summary>
        protected virtual void ProcessAllCollisions(float dt)
        {
            _collisionCheckCount = 0;
            _pairKeysCount = 0;

            if (EnableSpatialHash && _spatialHash != null)
            {
                ProcessCollisionsOptimized(dt);
            }
            else
            {
                ProcessCollisionsBruteForce(dt);
            }
        }

        /// <summary>
        /// 优化版空间分区碰撞检测。
        /// 关键优化：
        /// 1. 位运算配对去重（无 GC）
        /// 2. 旁路式直接处理，不缓冲结果
        /// 3. 减少 struct 构造函数调用
        /// </summary>
        void ProcessCollisionsOptimized(float dt)
        {
            int entityCount = _registeredEntities.Count;

            for (int i = 0; i < entityCount; i++)
            {
                var entity = _registeredEntities[i];
                if (entity == null) continue;

                _potentialColliders.Clear();
                _spatialHash.GetPotentialColliders(entity, _potentialColliders);

                int otherCount = _potentialColliders.Count;
                for (int j = 0; j < otherCount; j++)
                {
                    var other = _potentialColliders[j];
                    if (other == null) continue;

                    // 位运算配对去重（无 GC）
                    uint idA = (uint)entity.GetInstanceID();
                    uint idB = (uint)other.GetInstanceID();
                    uint pairKey = idA < idB
                        ? (idA << 16) | (idB & 0xFFFF)
                        : (idB << 16) | (idA & 0xFFFF);

                    bool alreadyProcessed = false;
                    for (int k = 0; k < _pairKeysCount; k++)
                    {
                        if (_pairKeysBuffer[k] == pairKey)
                        {
                            alreadyProcessed = true;
                            break;
                        }
                    }
                    if (alreadyProcessed) continue;

                    if (_pairKeysCount < _pairKeysBuffer.Length)
                        _pairKeysBuffer[_pairKeysCount++] = pairKey;

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
                var entityA = _registeredEntities[i];
                if (entityA == null) continue;

                for (int j = i + 1; j < count; j++)
                {
                    var entityB = _registeredEntities[j];
                    if (entityB == null) continue;

                    if (_collisionCheckCount >= MaxCollisionChecksPerFrame)
                        return;

                    ProcessPairCollision(entityA, entityB, dt);
                    _collisionCheckCount++;
                }
            }
        }

        /// <summary>
        /// 处理一对实体的碰撞。
        /// 旁路式：不 new VolumeCollisionResult，用临时变量承载结果。
        /// </summary>
        void ProcessPairCollision(TopDownController2D a, TopDownController2D b, float dt)
        {
            // 计算碰撞结果（直接用 struct，避免 new）
            Vector2 centerA = a.VolumeCenter;
            Vector2 centerB = b.VolumeCenter;
            float centerDist = Vector2.Distance(centerA, centerB);
            float combinedRadius = a.Volume.BoundingRadius + b.Volume.BoundingRadius;
            float overlap = combinedRadius - centerDist;

            if (overlap <= 0f && !EnableSoftRepulsion)
                return;

            // 计算方向（仅在需要时）
            Vector2 dir = overlap > 0f ? (centerB - centerA).normalized : Vector2.zero;
            float softRepulsionRadius = combinedRadius * SoftRepulsionDistanceRatio;

            // 软排斥（如果启用）
            if (EnableSoftRepulsion && centerDist < softRepulsionRadius && centerDist > 0.0001f)
            {
                float strength = 1f - (centerDist / softRepulsionRadius);
                strength *= strength;
                float totalMass = a.CollisionMass + b.CollisionMass;
                if (totalMass > 0f)
                {
                    float ratioA = b.CollisionMass / totalMass;
                    float ratioB = a.CollisionMass / totalMass;
                    Vector2 repelDir = (centerB - centerA) / centerDist;
                    float repelForce = SoftRepulsionStrength * strength * dt;
                    if (SoftRepulsionAffectsPosition)
                    {
                        a.Position -= repelDir * (repelForce * ratioA);
                        b.Position += repelDir * (repelForce * ratioB);
                        a.transform.position = a.Position;
                        b.transform.position = b.Position;
                    }
                    else
                    {
                        a.IntentVelocity -= (Vector3)repelDir * (repelForce * ratioA);
                        b.IntentVelocity += (Vector3)repelDir * (repelForce * ratioB);
                    }
                }
            }

            if (overlap <= 0f)
                return;

            // 计算最大允许重叠与所需分离量
            float otherEffectiveRadius = b.Volume.BoundingRadius * (1f - b.MaxOverlapRatio);
            float maxAllowedOverlap = a.MaxOverlapDistance + b.MaxOverlapDistance;
            float requiredSeparation = Mathf.Max(0f, overlap - maxAllowedOverlap);

            // 分离
            if (requiredSeparation > 0f)
            {
                float totalMass = a.CollisionMass + b.CollisionMass;
                if (totalMass > 0f)
                {
                    float ratioA = b.CollisionMass / totalMass;
                    float ratioB = a.CollisionMass / totalMass;
                    float separationForce = BaseSeparationForce * dt;
                    float sepA = requiredSeparation * ratioA * separationForce;
                    float sepB = requiredSeparation * ratioB * separationForce;
                    a.Position -= dir * sepA;
                    b.Position += dir * sepB;
                    a.transform.position = a.Position;
                    b.transform.position = b.Position;
                }
            }

            // 挤压
            float relativeSpeed = (a.TotalVelocity - b.TotalVelocity).magnitude;
            if (relativeSpeed > 0.01f)
            {
                float velAlongCollision = Vector2.Dot(a.TotalVelocity - b.TotalVelocity, dir);
                if (velAlongCollision > 0f)
                {
                    float totalMass = a.CollisionMass + b.CollisionMass;
                    if (totalMass > 0f)
                    {
                        float massRatioA = a.CollisionMass / totalMass;
                        float massRatioB = b.CollisionMass / totalMass;
                        float squeezeStrength = relativeSpeed * (1f + MassDifferenceInfluence) * (1f + VelocityDifferenceInfluence);
                        float squeezeA = velAlongCollision * massRatioB * squeezeStrength * dt * 0.5f;
                        float squeezeB = velAlongCollision * massRatioA * squeezeStrength * dt * 0.5f;
                        a.IntentVelocity += (Vector3)(-dir) * squeezeA;
                        b.IntentVelocity += (Vector3)(-dir) * squeezeB;
                    }
                }
            }

            // 触发事件（仅在真正重叠且需要分离时）
            if (overlap > 0.001f)
            {
                var evt = new VolumeCollisionEvent
                {
                    Self = a,
                    Other = b,
                    Result = new VolumeCollisionResult(a, b),
                    DeltaTime = dt
                };
                OnCollisionDetected?.Invoke(evt);

                var evtB = new VolumeCollisionEvent
                {
                    Self = b,
                    Other = a,
                    Result = new VolumeCollisionResult(b, a),
                    DeltaTime = dt
                };
                OnCollisionDetected?.Invoke(evtB);
            }
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
                if (currentLevel >= MaxChainLevel)
                    continue;

                // 当前层级的检测范围（考虑层级衰减）
                float levelFactor = 1f - (currentLevel * 0.1f);
                float checkRadius = current.Volume.BoundingRadius * ChainKnockbackRadiusMultiplier * 2f * levelFactor;

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

                float distSq = (entity.VolumeCenter - position).sqrMagnitude;
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

                    float distSq = (entity.VolumeCenter - position).sqrMagnitude;
                    if (distSq <= radiusSq)
                    {
                        result.Add(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 通用形状查询：在指定形状范围内的所有实体。形状参数支持圆形与不旋转的矩形。
        /// </summary>
        public void GetEntitiesInShape(VolumeShape shape, Vector2 center, ref List<TopDownController2D> result)
        {
            result.Clear();
            if (shape == null)
                return;

            float boundingRadius = shape.BoundingRadius;
            if (EnableSpatialHash && _spatialHash != null)
            {
                _spatialHash.GetEntitiesInCircle(center, boundingRadius, result);
            }
            else
            {
                float radiusSq = boundingRadius * boundingRadius;
                foreach (var entity in _registeredEntities)
                {
                    if (entity == null)
                        continue;

                    float distSq = (entity.VolumeCenter - center).sqrMagnitude;
                    if (distSq <= radiusSq)
                    {
                        result.Add(entity);
                    }
                }
            }

            // 粗筛后再用精确形状裁剪
            for (int i = result.Count - 1; i >= 0; i--)
            {
                if (!VolumeUtils.ContainsShape(shape, center, result[i].VolumeCenter, result[i].Volume))
                    result.RemoveAt(i);
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

                        Vector2 center = entity.VolumeCenter;
                        if (entity.Volume.Shape == VolumeShapeType.Rectangle)
                            Gizmos.DrawWireCube(center, entity.Volume.Size);
                        else
                            Gizmos.DrawWireSphere(center, entity.Volume.Radius);
                    }
                }

                if (ShowSpatialHashGrid && _spatialHash != null)
                {
                    // DrawSpatialHashGrid(_spatialHash, Color.green);
                }

                if (ShowSpatialHashGrid && EnableSolidColliders && _solidSpatialHash != null)
                {
                    DrawSolidSpatialHashGrid(_solidSpatialHash, Color.yellow);
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

        void DrawSolidSpatialHashGrid(VolumeSolidSpatialHash solidHash, Color color)
        {
            float cellSize = solidHash.CellSize;
            float alpha = SpatialHashGridAlpha;
            Color c = new Color(color.r, color.g, color.b, alpha);

            foreach (var solid in _solidColliders)
            {
                if (solid == null) continue;
                var bounds = solid.Collider.bounds;
                int minX = Mathf.FloorToInt(bounds.min.x / cellSize);
                int maxX = Mathf.FloorToInt(bounds.max.x / cellSize);
                int minY = Mathf.FloorToInt(bounds.min.y / cellSize);
                int maxY = Mathf.FloorToInt(bounds.max.y / cellSize);
                for (int cx = minX; cx <= maxX; cx++)
                {
                    for (int cy = minY; cy <= maxY; cy++)
                    {
                        Vector3 center = new Vector3((cx + 0.5f) * cellSize, (cy + 0.5f) * cellSize, 0f);
                        Gizmos.color = c;
                        Gizmos.DrawWireCube(center, Vector3.one * cellSize);
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 实体间体积碰撞管理器（重写版）。
    ///
    /// 设计目标：
    /// - 形状：以每个 <see cref="TopDownController2D.Volume"/>（<see cref="VolumeShape"/>）为准，
    ///   支持 Circle 与 Rectangle（轴对齐）。所有相交判定走 <see cref="VolumeShapeIntersection"/>。
    /// - 推开位置：硬推开（一次到位），不靠多帧衰减收敛，从根本上避免"推开 - 被推回"循环抖动。
    /// - 推力：双方按质量比沿分离方向产生"互相挤压"冲量，只写入 <see cref="TopDownController2D.KnockbackVelocity"/>
    ///   （通过 <see cref="TopDownController2D.AddImpact"/>），不污染 AI / 输入的 IntentVelocity。
    /// - 可重叠：全局默认 0（不能重叠）。可逐 entity 用 <see cref="TopDownController2D.MaxOverlapRatio"/> 覆盖，
    ///   也可在 Inspector 调整 <see cref="DefaultMaxOverlapRatio"/>。
    /// - 节流：默认硬推开 + 推力各加 1mm 缓冲，避免浮点误差导致反复触发。
    ///
    /// 不做的事情（旧 VolumeManager 里有，但不属于本系统职责）：
    /// - 不处理 <see cref="VolumeCollider"/>（固体边界）。如需边界推回，请保留旧 VolumeManager 或
    ///   在 <see cref="TopDownController2D"/> 自身做固体检测。
    /// - 不做链式击退、不做软排斥距离场、不做挤压"靠近阻尼"。
    /// </summary>
    [Serializable]
    public class VolumeEntityManager
    {
        [Header("开关")]
        [Tooltip("启用实体间体积碰撞系统")]
        public bool Enabled = true;

        [Header("空间分区设置")]
        [Tooltip("空间分区网格大小（建议设置为最大碰撞半径的2-4倍）")]
        public float SpatialHashCellSize = 1.5f;

        [Tooltip("是否使用增量空间哈希更新（仅移动的实体才更新网格，默认开启）")]
        public bool UseIncrementalSpatialHash = true;

        [Header("可重叠参数")]
        [Tooltip("全局默认最大可重叠比例（0-1）。0 = 任何重叠都会被推开；0.3 = 允许最多 30% 半径/尺寸的重叠。会被实体自身的 MaxOverlapRatio 覆盖。")]
        [Range(0f, 1f)]
        public float DefaultMaxOverlapRatio;

        [Header("推开参数")]
        [Tooltip("硬推开时附加的最小额外距离，避免浮点误差让两实体持续互相推")]
        [Min(0f)]
        public float SeparationEpsilon = 0.001f;

        [Tooltip("推开阈值：分离方向上的 overlap 低于此值时直接跳过推开与推力，避免抖动")]
        [Min(0f)]
        public float MinOverlapToReact = 0.0001f;

        [Tooltip("每帧最大碰撞对数（防止密集场景下计算量爆炸）")]
        [Min(1)]
        public int MaxCollisionChecksPerFrame = 1000;

        [Header("推力参数")]
        [Tooltip("碰撞推力强度系数。0 = 不产生推力；1 = 完全按相对速度转化。")]
        [Range(0f, 2f)]
        public float CollisionPushScale = 1f;

        [Tooltip("推开后的额外冲量（即使两实体本帧无相对速度，也给一次推开冲量提升手感）")]
        [Min(0f)]
        public float CollisionBumpImpulse;

        [Tooltip("相对速度低于此值视为静止，不产生挤压冲量")]
        [Min(0f)]
        public float RelativeSpeedDeadZone = 0.01f;

        [Header("调试")]
        [Tooltip("显示碰撞对数与推开日志")]
        public bool LogVerbose;

        // ===================== 运行时数据 =====================
        VolumeEntitySpatialHash _spatialHash;
        List<TopDownController2D> _registeredEntities = new();
        List<TopDownController2D> _potentialEntities = new();

        public List<TopDownController2D> Entities => _registeredEntities;

        // 配对去重：位运算版（避免每帧 new HashSet）
        uint[] _pairKeysBuffer = new uint[4096];
        int _pairKeysCount;

        int _collisionChecksThisFrame;
        int _collisionCheckCount;

        public void OnAwake()
        {
        }

        public void InitializeSpatialHash()
        {
            _spatialHash = new(SpatialHashCellSize);
        }

        public void UpdateSpatialHash()
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
        }

        // ===================== 注册 / 注销 =====================

        public void Register(TopDownController2D entity)
        {
            if (_registeredEntities.Contains(entity))
                return;

            if (entity.IsRegistered)
                return;

            _registeredEntities.Add(entity);
            entity.IsRegistered = true;
            // 插入到空间分区
            _spatialHash?.Insert(entity);
        }

        public void Unregister(TopDownController2D entity)
        {
            if (!entity.IsRegistered)
                return;

            if (_registeredEntities.Remove(entity))
                entity.IsRegistered = false;

            // 从空间分区移除
            _spatialHash?.Remove(entity);
        }

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

        // ===================== 帧驱动 =====================

        /// <summary>
        /// 帧更新。
        ///
        /// 注意：VolumeEntityManager 的 OnFixedUpdate 与每个 TopDownController2D 的 Update()
        /// 在同一 MonoBehaviour 生命周期中执行，Unity 不保证执行顺序。
        /// 若管理器 GameObject 的 Execution Order 低于实体，则 VEM 在实体 Update() 之后执行，
        /// 当帧只能修改 CurPosition，下一帧 ApplyPosition() 才同步到 transform.position——
        /// 推开效果延迟一帧，通常可接受。
        ///
        /// 若需当帧立即推开（无延迟），请将 VolumeEntityManager GameObject 的
        /// Script Execution Order 设置为比所有实体更早（填写一个负值，如 -100）。
        /// </summary>
        public void ProcessAllCollisions(float dt)
        {
            if (!Enabled)
                return;

            _collisionChecksThisFrame = 0;
            _collisionCheckCount = 0;
            _pairKeysCount = 0;

            ProcessCollisionsOptimized(dt);
        }

        // ===================== 单对处理 =====================

        void ProcessPair(TopDownController2D a, TopDownController2D b, float dt)
        {
            // 1. 用 VolumeShapeIntersection 判定是否相交 + 取精确穿透深度。
            Vector2 centerA = a.VolumeCenter;
            Vector2 centerB = b.VolumeCenter;
            if (!VolumeShapeIntersection.TryGetOverlap(a.Volume, centerA, b.Volume, centerB, out float rawOverlap, scaling: 1.1F))
                return;

            if (rawOverlap < MinOverlapToReact)
                return;

            // 2. 取分离方向：复用 VolumeManager 之前已验证过的 SAT 实现。
            //    该方法已处理 Circle/Circle、Circle/Rectangle、Rectangle/Rectangle 三种组合。
            if (!VolumeUtils.TryGetCollision(a, b, out Vector2 dir, out float satOverlap))
                return;

            // 3. 取本对的有效可重叠比例：实体自身字段优先，缺失时退回全局默认。
            float overlapRatioA = GetMaxOverlapRatio(a);
            float overlapRatioB = GetMaxOverlapRatio(b);
            // 双方各自允许的"不推开比例"，按形状尺寸线性插值：
            //   - Circle：maxAllowed = (rA + rB) * ratio
            //   - Rectangle / 混合：按投影半径的混合值
            float maxAllowed = ComputeMaxAllowedOverlap(a, b, overlapRatioA, overlapRatioB);

            // 4. 沿 SAT 分离方向上的真实可允许重叠量：
            //    maxAllowed 已经在所有方向上都是同值（用 max 半径近似），方向上沿 dir 投影即可。
            float overlap = satOverlap;
            float requiredSeparation = Mathf.Max(0f, overlap - maxAllowed) + SeparationEpsilon;

            // 5. 硬推开：一次到位，按质量比分摊
            ApplyHardSeparation(a, b, dir, requiredSeparation);

            // 6. 推力：把"压入速度"按比例转化为 KnockbackVelocity 冲量
            ApplyPushImpulse(a, b, dir, dt);
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
                _potentialEntities.Clear();
                _spatialHash.GetPotentialColliders(entity, _potentialEntities);

                int otherCount = _potentialEntities.Count;
                for (int j = 0; j < otherCount; j++)
                {
                    var other = _potentialEntities[j];
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

                    if (alreadyProcessed)
                        continue;

                    if (_pairKeysCount < _pairKeysBuffer.Length)
                        _pairKeysBuffer[_pairKeysCount++] = pairKey;

                    if (_collisionCheckCount >= MaxCollisionChecksPerFrame)
                        return;

                    ProcessPair(entity, other, dt);

                    _collisionCheckCount++;
                }
            }
        }

        // ===================== 推开 =====================

        /// <summary>
        /// 硬推开：一次性修正位置，不写 transform.position。
        /// 原因：TopDownController2D.Update() 中的 ApplyPosition() 会把
        /// transform.position Lerp 到 CurPosition，如果 VEM 也在同一帧写了
        /// transform.position = CurPosition（跳过 Lerp），但 LastPosition
        /// 并未同步更新——下帧 ApplyPosition 又从陈旧 LastPosition Lerp，
        /// 视觉上形成"推开 - Lerp 跳回"的循环抖动。
        /// 正确做法：只改 CurPosition，ApplyPosition 自然在下一帧把变化同步过去。
        /// </summary>
        static void ApplyHardSeparation(TopDownController2D a, TopDownController2D b, Vector2 dir, float requiredSeparation)
        {
            if (requiredSeparation <= 0f)
                return;

            float massA = a.CollisionMass;
            float massB = b.CollisionMass;
            float totalMass = massA + massB;
            if (totalMass <= 0f)
                return;

            // 经典牛顿第三定律：质量大者动得少
            float ratioA = massB / totalMass;
            float ratioB = massA / totalMass;
            Vector2 pushA = -dir * (requiredSeparation * ratioA);
            Vector2 pushB = dir * (requiredSeparation * ratioB);

            // 只改 CurPosition，由 ApplyPosition 在下一帧自然同步到 transform.position
            a.MovePositionBy(pushA);
            b.MovePositionBy(pushB);
        }

        // ===================== 推力 =====================

        void ApplyPushImpulse(TopDownController2D a, TopDownController2D b, Vector2 dir, float dt)
        {
            // 相对速度在分离方向上的投影（>0 表示双方正在相对靠近）
            Vector2 velA = a.TotalVelocity;
            Vector2 velB = b.TotalVelocity;
            Vector2 relVel = velA - velB;
            float relAlong = Vector2.Dot(relVel, dir);

            // 计算本帧要施加的冲量（速度量纲）
            float impulse = 0f;
            if (relAlong > RelativeSpeedDeadZone)
            {
                // 把"接近速度"按质量比转化：
                //   - A 的接近速度 = relAlong，质量比 massB / (massA + massB)
                //   - B 的接近速度 = relAlong，质量比 massA / (massA + massB)
                // 双方各按对方质量比吸收一份速度并反推。
                float massA = a.CollisionMass;
                float massB = b.CollisionMass;
                float totalMass = massA + massB;
                if (totalMass > 0f)
                {
                    float ratioA = massB / totalMass;
                    float ratioB = massA / totalMass;
                    // 双方各承担一份"接近速度"，按对方质量比加权；
                    // CollisionPushScale 控制总强度，0.5 让双人的冲量对称。
                    float eachImpulse = relAlong * CollisionPushScale * 0.5f;
                    // A 沿 -dir 推、B 沿 +dir 推：两人互相"弹开"，方向相反
                    Vector3 impulseA = -dir * (eachImpulse * ratioA);
                    Vector3 impulseB = dir * (eachImpulse * ratioB);

                    // 写到 KnockbackVelocity（走 AddImpact，让 TopDownController2D 内部统一处理抗性/衰减）
                    a.AddImpact(impulseA);
                    b.AddImpact(impulseB);
                    impulse = eachImpulse;
                }
            }

            // 额外"碰撞手感"冲量：即使没相对速度也给一点点推开冲量
            if (CollisionBumpImpulse > 0f)
            {
                Vector3 bumpA = -dir * (CollisionBumpImpulse * dt);
                Vector3 bumpB = dir * (CollisionBumpImpulse * dt);
                a.AddImpact(bumpA);
                b.AddImpact(bumpB);
            }

            if (LogVerbose && impulse > 0f)
                Debug.Log($"[VolumeEntityManager] {a.name} <-> {b.name}: relAlong={relAlong:F3}, impulse={impulse:F3}");
        }

        // ===================== 辅助 =====================

        float GetMaxOverlapRatio(TopDownController2D e)
        {
            // 全局 DefaultMaxOverlapRatio 是"上限"——实体的 MaxOverlapRatio 可低于全局，
            // 但不能高于全局。默认 0 时，所有实体的可重叠都会被钳到 0（不能重叠）。
            return Mathf.Min(e.MaxOverlapRatio, DefaultMaxOverlapRatio);
        }

        static float ComputeMaxAllowedOverlap(TopDownController2D a, TopDownController2D b, float ratioA, float ratioB)
        {
            // 用"两形状在中心连线方向上的精确投影半径之和"作为可重叠基准距离，
            // 这样无论圆-圆、圆-矩、矩-矩都给出一致的"沿该方向还能再压入多远"。
            Vector2 axis = (b.VolumeCenter - a.VolumeCenter);
            if (axis.sqrMagnitude < 1e-6f)
                axis = Vector2.right;
            axis.Normalize();

            float rA = a.Volume.GetProjectionRadius(axis);
            float rB = b.Volume.GetProjectionRadius(axis);
            float contact = rA + rB;

            // 可重叠量 = "刚好相切距离" * ratio。ratio 越大，可重叠越大；ratio=0 则完全不允许。
            // 取双方中更严格的一方（较小 ratio）作为本对上限。
            float effectiveRatio = Mathf.Min(ratioA, ratioB);
            return contact * effectiveRatio;
        }
    }
}
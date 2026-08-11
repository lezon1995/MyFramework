using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 经验值物品状态
    /// </summary>
    public enum ExpOrbState
    {
        Idle, // 空闲/未激活
        Dropping, // 掉落动画中（不可拾取）
        Grounded, // 落地静止（可拾取）
        BeingCollected, // 被玩家吸引中（拾取动画中：远离→飞向玩家）
        Collected // 已收集完成
    }

    /// <summary>
    /// 经验值物品实体 - 处理单个经验值物品的掉落动画和拾取动画
    /// 使用轻量级 MonoBehaviour 实现，避免继承 Character 带来的复杂初始化
    ///
    /// 掉落动画基于"倾斜正圆投影"模型（与 Coin 一致）：
    /// 3D空间中经验值物品沿一个倾斜的正圆走一段弧，
    /// 投影到XY平面后形成一段椭圆弧（前半段为上升，后半段为下落）。
    ///
    /// 拾取动画采用两段式（与 Coin 不同的关键点）：
    ///   1) 远离阶段：先朝着远离玩家方向飞行一小段距离（看起来像"被惊动"再被吸走）
    ///   2) 飞向阶段：转向玩家位置加速飞过去，到达玩家位置后消失 → 此时经验值才真正到账
    /// </summary>
    public class ExpOrb : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// 经验值物品当前状态
        /// </summary>
        public ExpOrbState State { get; protected set; } = ExpOrbState.Idle;

        /// <summary>
        /// 经验值物品数值
        /// </summary>
        public int Value { get; protected set; } = 1;

        /// <summary>
        /// 当前世界位置
        /// </summary>
        public Vector2 Position => _transformCache.position;

        /// <summary>
        /// 是否可被拾取
        /// </summary>
        public bool CanBeCollected => State == ExpOrbState.Grounded;

        /// <summary>
        /// 实例ID（用于管理器索引）
        /// </summary>
        public int instanceID => _instanceID;

        protected int _instanceID;

        /// <summary>
        /// 拾取阶段进度（0-1，0=刚开始拾取，1=已到达玩家位置）
        /// 仅在 BeingCollected 状态下有效
        /// </summary>
        public float PickupProgress { get; protected set; }

        /// <summary>
        /// 拾取阶段子阶段（远离阶段 / 飞向阶段）
        /// </summary>
        public PickupPhase pickupPhase { get; protected set; } = PickupPhase.None;

        #endregion

        #region Public Enums

        public enum PickupPhase
        {
            None,
            Flee,        // 远离阶段
            FlyToPlayer  // 飞向玩家阶段
        }

        #endregion

        #region Private Fields - Animation State

        // 掉落动画状态
        float _dropTimer;
        int _currentBounceIndex;

        // 预计算的所有段端点（地面 Y = DropPoint.y）
        // _segmentPositions[0] = DropPoint（起点）
        // _segmentPositions[i] = 第 i 个反弹落点（i >= 1）
        Vector2[] _segmentPositions;

        // 每段的抛物线最大高度（衰减后）
        // _segmentHeights[i] = 第 i 段的高度（从 0 到 BounceCount）
        float[] _segmentHeights;

        Vector2 _segmentStartPos;
        Vector2 _segmentEndPos;
        Vector2 _segmentControlPoint;
        float _segmentDuration;
        float _segmentMaxHeight;
        float _segmentVerticalRadius;

        Vector2 _startPos;
        Vector2 _dropDirection;
        Vector2 _finalLandingPos; // 严格在椭圆上的最终落点
        ExpDropConfig _dropConfig;
        ExpPickupConfig _pickupConfig;

        // 拾取动画状态
        Vector2 _pickupStartPos;
        Vector2 _fleeEndPos;
        Transform _pickupTarget;
        float _fleeTimer;
        float _flyToPlayerTimer;
        float _fleeDuration;
        float _flyToPlayerSpeed;
        float _flyToPlayerTraveled;
        float _flyToPlayerTotalDistance;
        float _fleeDistance;
        float _pickupRotationDegrees;
        float _pickupMinScale;
        MMTween.MMTweenCurve _fleeCurve;
        MMTween.MMTweenCurve _flyToPlayerCurve;
        bool _trackPlayerDuringFly;

        #endregion

        #region Components

        Transform _transformCache;
        public SpriteRenderer _spriteRenderer;

        #endregion

        #region Lifecycle

        void Awake()
        {
            _transformCache = transform;
            if (_spriteRenderer == null)
                TryGetComponent(out _spriteRenderer);
            _instanceID = gameObject.GetInstanceID();
        }

        /// <summary>
        /// 由管理器调用 - 激活经验值物品
        /// </summary>
        public void Acquire()
        {
            _instanceID = gameObject.GetInstanceID();
        }

        /// <summary>
        /// 由管理器调用 - 释放经验值物品（对象池回收）
        /// </summary>
        public void OnRelease()
        {
            State = ExpOrbState.Idle;
            Value = 1;
            _dropTimer = 0f;
            _currentBounceIndex = 0;
            _fleeTimer = 0f;
            _flyToPlayerTimer = 0f;
            _flyToPlayerTraveled = 0f;
            _flyToPlayerTotalDistance = 0f;
            _pickupTarget = null;
            _segmentVerticalRadius = 0f;
            _segmentMaxHeight = 0f;
            _segmentPositions = null;
            _segmentHeights = null;
            _startPos = Vector2.zero;
            _finalLandingPos = Vector2.zero;
            pickupPhase = PickupPhase.None;
            PickupProgress = 0f;

            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }

            if (_transformCache != null)
            {
                _transformCache.localScale = Vector3.one;
                _transformCache.localRotation = Quaternion.identity;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 初始化经验值物品数据
        /// </summary>
        public void Initialize(int value, Vector2 position, Vector2 dropDirection, ExpDropConfig dropConfig, ExpPickupConfig pickupConfig)
        {
            Value = Mathf.Max(1, value);
            _dropConfig = dropConfig ?? ExpDropConfig.Default;
            _pickupConfig = pickupConfig ?? ExpPickupConfig.Default;

            // 重置位置
            var currentPos = _transformCache.position;
            _transformCache.position = new Vector3(position.x, position.y, currentPos.z);

            _transformCache.localScale = Vector3.one;
            _transformCache.localRotation = Quaternion.identity;

            // 归一化方向
            if (dropDirection.sqrMagnitude < 0.0001f)
                dropDirection = Vector2.up;
            _dropDirection = dropDirection.normalized;

            // 缓存 DropPoint
            _startPos = position;

            // 预计算掉落实体路径
            precomputeDropPath(_dropDirection);

            // 第一段动画
            setupCurrentSegment();

            State = ExpOrbState.Dropping;
            pickupPhase = PickupPhase.None;
            PickupProgress = 0f;

            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }
        }

        /// <summary>
        /// 尝试开始拾取动画（两段式：先远离玩家，再飞向玩家）
        /// </summary>
        public bool TryStartPickup(Transform targetTransform)
        {
            if (State != ExpOrbState.Grounded)
                return false;

            initPickupAnimation(_transformCache.position, targetTransform);
            State = ExpOrbState.BeingCollected;
            pickupPhase = PickupPhase.Flee;
            PickupProgress = 0f;
            return true;
        }

        /// <summary>
        /// 更新掉落动画
        /// </summary>
        public void UpdateDropping(float dt)
        {
            if (State != ExpOrbState.Dropping)
                return;

            _dropTimer += dt;
            float t = _segmentDuration > 0f ? Mathf.Clamp01(_dropTimer / _segmentDuration) : 1f;

            // 用二次贝塞尔曲线模拟椭圆弧（在XY平面上的投影）
            Vector2 newPos = evaluateQuadraticBezier(_segmentStartPos, _segmentControlPoint, _segmentEndPos, t);

            // 模拟3D高度的视觉表现：高度在抛物线顶点(t=0.5)最高，落地时接近0
            float heightFactor = Mathf.Sin(t * Mathf.PI);
            float scaleMultiplier = 1f + heightFactor * 0.3f;

            // 保持Z值
            var currentPos = _transformCache.position;
            _transformCache.position = new(newPos.x, newPos.y, currentPos.z);
            _transformCache.localScale = Vector3.one * scaleMultiplier;

            // 当前段动画完成
            if (t >= 1f)
            {
                onSegmentComplete();
            }
        }

        /// <summary>
        /// 更新拾取动画（两段式：Flee + FlyToPlayer）
        ///
        /// 阶段 1 - Flee：
        ///   经验值物品从 _pickupStartPos 沿远离玩家方向飞行 FleeDistance 距离
        ///   曲线由 _fleeCurve 控制（默认 EaseOutQuad）
        ///   期间经验值物品略微缩小并轻微旋转
        ///
        /// 阶段 2 - FlyToPlayer：
        ///   经验值物品从 _fleeEndPos 飞向玩家实时位置
        ///   曲线由 _flyToPlayerCurve 控制（默认 EaseInQuad）
        ///   期间缩放逐渐缩小到 _pickupMinScale，旋转继续累积，并淡出
        ///   到达玩家位置时（t >= 1f）经验值真正到账
        /// </summary>
        public void UpdatePickup(float dt)
        {
            if (State != ExpOrbState.BeingCollected)
                return;

            // 玩家在拾取过程中被销毁时，取消拾取
            if (_pickupTarget == null)
            {
                State = ExpOrbState.Collected;
                pickupPhase = PickupPhase.None;
                return;
            }

            // === 阶段 1：远离阶段 ===
            if (pickupPhase == PickupPhase.Flee)
            {
                _fleeTimer += dt;
                float linearT = _fleeDuration > 0f ? Mathf.Clamp01(_fleeTimer / _fleeDuration) : 1f;
                float t = MMTween.Evaluate(linearT, _fleeCurve);

                Vector2 newPos = Vector2.Lerp(_pickupStartPos, _fleeEndPos, t);

                // 远离阶段：保持大小（甚至略微放大），慢速旋转
                float scaleMultiplier = Mathf.Lerp(1f, 1.05f, t);
                var currentPos = _transformCache.position;
                _transformCache.position = new Vector3(newPos.x, newPos.y, currentPos.z);
                _transformCache.localScale = Vector3.one * scaleMultiplier;

                float rotZ = linearT * _pickupRotationDegrees * 0.3f; // 经验值物品总旋转量的 30% 用于远离阶段
                _transformCache.localRotation = Quaternion.Euler(0f, 0f, rotZ);

                PickupProgress = linearT * 0.3f;

                if (linearT >= 1f)
                {
                    // 远离阶段完成，进入飞向玩家阶段
                    pickupPhase = PickupPhase.FlyToPlayer;
                    _flyToPlayerTimer = 0f;

                    // 快照飞向阶段总距离：以阶段切换瞬间玩家与逃离终点的距离为参考
                    Vector2 initialTargetPos = _pickupTarget != null ? (Vector2)_pickupTarget.position : _fleeEndPos;
                    _flyToPlayerTotalDistance = Vector2.Distance(_fleeEndPos, initialTargetPos);
                    _flyToPlayerTraveled = 0f;
                }
                return;
            }

            // === 阶段 2：飞向玩家阶段（基于速度推进）===
            if (pickupPhase == PickupPhase.FlyToPlayer)
            {
                // 终点：玩家实时位置（可选：跟随玩家移动）
                Vector2 targetPos = _pickupTarget != null ? (Vector2)_pickupTarget.position : _fleeEndPos;

                // 用初始快照距离计算线性进度，保证速度恒定（视觉上由 FlyToPlayerCurve 做缓动）
                float linearT;
                if (_flyToPlayerTotalDistance > 0f)
                {
                    _flyToPlayerTraveled += _flyToPlayerSpeed * dt;
                    linearT = Mathf.Clamp01(_flyToPlayerTraveled / _flyToPlayerTotalDistance);
                }
                else
                {
                    // 玩家已在逃离终点（距离为 0），直接完成
                    linearT = 1f;
                }
                float t = MMTween.Evaluate(linearT, _flyToPlayerCurve);

                // 起点：经验值物品在远离阶段结束时的位置（_fleeEndPos）
                // 终点：玩家实时位置
                Vector2 newPos = Vector2.Lerp(_fleeEndPos, targetPos, t);

                // 缩放：从 1.05 平滑缩到 _pickupMinScale
                float scaleMultiplier = Mathf.Lerp(1.05f, _pickupMinScale, t);

                // 旋转：飞向阶段使用 70% 总旋转量
                float rotZ = (_pickupRotationDegrees * 0.3f) + linearT * (_pickupRotationDegrees * 0.7f);

                var currentPos = _transformCache.position;
                _transformCache.position = new Vector3(newPos.x, newPos.y, currentPos.z);
                _transformCache.localScale = Vector3.one * scaleMultiplier;
                _transformCache.localRotation = Quaternion.Euler(0f, 0f, rotZ);

                // 淡出（最后 50% 进度开始淡出）
                if (_spriteRenderer != null)
                {
                    Color c = _spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, Mathf.InverseLerp(0.5f, 1f, linearT));
                    _spriteRenderer.color = c;
                }

                PickupProgress = 0.3f + linearT * 0.7f;

                // 飞向玩家阶段完成 → 经验值到账
                if (linearT >= 1f)
                {
                    onPickupComplete();
                }
            }
        }

        /// <summary>
        /// 设置经验值物品状态
        /// </summary>
        public void SetState(ExpOrbState newState)
        {
            State = newState;
        }

        /// <summary>
        /// 立即完成拾取
        /// </summary>
        public void ForceCollect()
        {
            if (State == ExpOrbState.Grounded || State == ExpOrbState.BeingCollected)
            {
                _flyToPlayerTraveled = _flyToPlayerTotalDistance;
                onPickupComplete();
            }
        }

        /// <summary>
        /// 设置精灵渲染器（用于外部注入）
        /// </summary>
        public void SetSpriteRenderer(SpriteRenderer renderer)
        {
            _spriteRenderer = renderer;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 预计算整条掉落路径的所有段端点
        /// 与 Coin 一致的椭圆模型：
        /// - 最终落点 P_final = DropPoint + direction × 椭圆交点距离
        /// - 中间反弹点按几何衰减分布在 DropPoint → P_final 的连线上
        /// - 障碍物裁剪：CircleCast 以 _dropConfig.ExpRadius 作为半径
        /// </summary>
        void precomputeDropPath(Vector2 direction)
        {
            int totalSegments = Mathf.Max(0, _dropConfig.BounceCount) + 1;
            int totalPoints = totalSegments + 1;

            _segmentPositions = new Vector2[totalPoints];
            _segmentHeights = new float[totalSegments];

            // === 1. 计算椭圆上的理论最终落点 P_theoretical ===
            float a = _dropConfig.HorizontalSpread;
            float b = _dropConfig.DropHeight;
            float ellipseR = ExpOrbEllipseScatter.RayEllipseIntersectionDistance(direction, a, b);
            Vector2 P_theoretical = _startPos + direction.normalized * ellipseR;

            // === 2. 障碍物裁剪 ===
            Vector2 P_final = clipLandingByObstacle(_startPos, direction, P_theoretical);

            // === 3. 计算总路径方向和长度 ===
            Vector2 totalOffset = P_final - _startPos;
            float totalDistance = totalOffset.magnitude;

            // 如果总距离太小，所有中间点都重叠到 DropPoint
            if (totalDistance < 0.0001f)
            {
                _segmentPositions[0] = _startPos;
                _segmentPositions[totalPoints - 1] = _startPos;
                for (int i = 1; i < totalPoints - 1; i++)
                    _segmentPositions[i] = _startPos;
                for (int i = 0; i < totalSegments; i++)
                    _segmentHeights[i] = _dropConfig.DropHeight;
                _finalLandingPos = _startPos;
                return;
            }

            Vector2 pathDir = totalOffset / totalDistance;

            // === 4. 计算每段高度（按 BounceDecayRatio 衰减）===
            float decay = Mathf.Clamp01(_dropConfig.BounceDecayRatio);
            for (int i = 0; i < totalSegments; i++)
            {
                _segmentHeights[i] = _dropConfig.DropHeight * Mathf.Pow(decay, i);
            }

            // === 5. 计算各段端点在连线上的位置 ===
            _segmentPositions[0] = _startPos;

            float sumRatio = 0f;
            for (int i = 0; i < totalSegments; i++)
                sumRatio += Mathf.Pow(decay, i);
            if (sumRatio < 0.0001f) sumRatio = 1f;

            float segmentLength = totalDistance / sumRatio;

            float accumulated = 0f;
            for (int i = 0; i < totalSegments; i++)
            {
                accumulated += segmentLength * Mathf.Pow(decay, i);
                Vector2 point = _startPos + pathDir * accumulated;
                _segmentPositions[i + 1] = point;
            }

            _segmentPositions[totalPoints - 1] = P_final;
            _finalLandingPos = P_final;
        }

        /// <summary>
        /// 障碍物裁剪：从 startPos 沿 direction 方向 CircleCast（考虑经验值物品半径），
        /// 如果撞到 ObstacleLayers 上的物体，把 P_theoretical 裁剪到碰撞点
        /// </summary>
        Vector2 clipLandingByObstacle(Vector2 startPos, Vector2 direction, Vector2 P_theoretical)
        {
            int obstacleMask = _dropConfig.ObstacleLayers;
            if (obstacleMask == 0)
                return P_theoretical;

            Vector2 normDir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            float theoreticalDistance = (P_theoretical - startPos).magnitude;

            // 探针半径 = 经验值物品碰撞体半径
            float radius = Mathf.Max(0.01f, _dropConfig.ExpRadius);

            // CircleCast 从起点朝理论落点方向检测
            RaycastHit2D hit = Physics2D.CircleCast(
                startPos,
                radius,
                normDir,
                theoreticalDistance,
                obstacleMask);

            // 忽略极近距离的命中
            const float minHitDistance = 0.05f;
            if (hit.collider != null && hit.distance >= minHitDistance && hit.distance < theoreticalDistance)
            {
                Vector2 clipCenter = startPos + normDir * hit.distance;
                return clipCenter;
            }

            return P_theoretical;
        }

        /// <summary>
        /// 设置当前段动画参数（使用预计算的 _segmentPositions / _segmentHeights）
        /// </summary>
        void setupCurrentSegment()
        {
            int segIdx = _currentBounceIndex;

            _segmentStartPos = _segmentPositions[segIdx];
            _segmentEndPos = _segmentPositions[segIdx + 1];

            float verticalRadius = _segmentHeights[segIdx];
            if (verticalRadius < 0.01f) verticalRadius = 0.01f;

            _segmentVerticalRadius = verticalRadius;
            _segmentMaxHeight = verticalRadius;

            // 抛物线顶点：起点和终点的中点 + Vector2.up * verticalRadius
            float midX = (_segmentStartPos.x + _segmentEndPos.x) * 0.5f;
            float baseY = Mathf.Max(_segmentStartPos.y, _segmentEndPos.y);
            _segmentControlPoint = new Vector2(midX, baseY + verticalRadius);

            // 每段动画时间
            int totalSegments = _segmentPositions.Length - 1;
            _segmentDuration = _dropConfig.DropDuration / Mathf.Max(1, totalSegments);
            _dropTimer = 0f;
        }

        /// <summary>
        /// 设置下一段反弹
        /// </summary>
        void setupNextBounce()
        {
            _currentBounceIndex++;
            setupCurrentSegment();
        }

        /// <summary>
        /// 当前段动画完成回调
        /// </summary>
        void onSegmentComplete()
        {
            // 设置最终位置（保持Z值）
            var currentPos = _transformCache.position;
            _transformCache.position = new(_segmentEndPos.x, _segmentEndPos.y, currentPos.z);
            _transformCache.localScale = Vector3.one;

            // 判断是否还有下一段
            if (_segmentPositions != null && _currentBounceIndex + 1 < _segmentPositions.Length - 1)
            {
                setupNextBounce();
            }
            else
            {
                // 全部段完成，把经验值物品精确对齐到最终落点
                var cPos = _transformCache.position;
                _transformCache.position = new Vector3(_finalLandingPos.x, _finalLandingPos.y, cPos.z);
                _transformCache.localScale = Vector3.one;

                // 经验值物品落地
                State = ExpOrbState.Grounded;
                _dropTimer = 0f;
                _currentBounceIndex = 0;
            }
        }

        /// <summary>
        /// 初始化两段式拾取动画
        ///
        /// 阶段 1 - Flee：
        ///   起点 = startPos
        ///   终点 = 沿远离玩家方向偏移 FleeDistance 距离
        ///
        /// 阶段 2 - FlyToPlayer：
        ///   起点 = _fleeEndPos（阶段 1 的终点）
        ///   终点 = 玩家实时位置（每帧更新）
        /// </summary>
        void initPickupAnimation(Vector2 startPos, Transform targetTransform)
        {
            _pickupStartPos = startPos;
            _pickupTarget = targetTransform;

            // 远离方向：从玩家指向经验值物品（远离玩家）
            Vector2 awayDir = startPos - (Vector2)targetTransform.position;
            if (awayDir.sqrMagnitude < 0.0001f)
            {
                // 玩家与经验值物品重叠时，使用一个随机方向
                awayDir = Random.insideUnitCircle.normalized;
            }
            else
            {
                awayDir.Normalize();
            }

            _fleeDistance = Mathf.Max(0f, _pickupConfig.FleeDistance);
            _fleeEndPos = _pickupStartPos + awayDir * _fleeDistance;

            _fleeDuration = Mathf.Max(0.0001f, _pickupConfig.FleeDuration);
            _flyToPlayerSpeed = Mathf.Max(0f, _pickupConfig.FlyToPlayerSpeed);
            _fleeCurve = _pickupConfig.FleeCurve;
            _flyToPlayerCurve = _pickupConfig.FlyToPlayerCurve;
            _pickupRotationDegrees = _pickupConfig.RotationDegrees;
            _pickupMinScale = _pickupConfig.MinScale;
            _trackPlayerDuringFly = _pickupConfig.TrackPlayerDuringFly;

            _fleeTimer = 0f;
            _flyToPlayerTimer = 0f;
        }

        /// <summary>
        /// 拾取动画完成（飞向玩家阶段到达玩家位置）
        /// 经验值此时真正到账
        /// </summary>
        void onPickupComplete()
        {
            State = ExpOrbState.Collected;
            pickupPhase = PickupPhase.None;
            PickupProgress = 1f;
            _flyToPlayerTimer = 0f;
        }

        /// <summary>
        /// 二次贝塞尔曲线求值
        /// B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        /// </summary>
        Vector2 evaluateQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0
                   + 2f * oneMinusT * t * p1
                   + t * t * p2;
        }

        #endregion
    }
}

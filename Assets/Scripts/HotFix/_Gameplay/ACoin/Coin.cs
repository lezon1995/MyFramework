using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 金币实体 - 处理单枚金币的掉落动画和拾取动画
    /// 使用轻量级MonoBehaviour实现，避免继承Character带来的复杂初始化
    ///
    /// 掉落动画基于"倾斜正圆投影"模型：
    /// 3D空间中金币沿一个倾斜的正圆走一段弧，
    /// 投影到XY平面后形成一段椭圆弧（前半段为上升，后半段为下落）。
    /// </summary>
    public class Coin : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// 金币当前状态
        /// </summary>
        public CoinState State { get; protected set; } = CoinState.Idle;

        /// <summary>
        /// 金币价值
        /// </summary>
        public int Value { get; protected set; } = 1;

        /// <summary>
        /// 当前世界位置
        /// </summary>
        public Vector2 Position => _transformCache.position;

        /// <summary>
        /// 是否可被拾取
        /// </summary>
        public bool CanBeCollected => State == CoinState.Grounded;

        /// <summary>
        /// 实例ID（用于管理器索引）
        /// </summary>
        public int instanceID => _instanceID;

        protected int _instanceID;

        #endregion

        #region Private Fields - Animation State

        // 掉落动画状态
        float _dropTimer;
        int _currentBounceIndex;

        // 预计算的所有段端点（地面 Y = DropPoint.y）
        // _segmentPositions[0] = DropPoint（起点）
        // _segmentPositions[i] = 第 i 个反弹落点（i >= 1）
        // 长度 = BounceCount + 2（起 + 反 + 终）
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
        Vector2 _finalLandingPos; // 严格在椭圆上的最终落点（动画结束时对齐到此位置）
        CoinDropConfig _dropConfig;
        CoinPickupConfig _pickupConfig;

        // 拾取动画状态
        Vector2 _pickupStartPos;
        Transform _pickupTarget;
        float _pickupTimer;
        float _pickupDuration;
        float _pickupRotationDegrees;
        float _pickupMinScale;
        MMTween.MMTweenCurve _pickupCurve;

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
        /// 由管理器调用 - 激活金币
        /// </summary>
        public void Acquire()
        {
            _instanceID = gameObject.GetInstanceID();
        }

        /// <summary>
        /// 由管理器调用 - 释放金币（对象池回收）
        /// </summary>
        public void OnRelease()
        {
            State = CoinState.Idle;
            Value = 1;
            _dropTimer = 0f;
            _currentBounceIndex = 0;
            _pickupTimer = 0f;
            _segmentVerticalRadius = 0f;
            _segmentMaxHeight = 0f;
            _segmentPositions = null;
            _segmentHeights = null;
            _startPos = Vector2.zero;
            _finalLandingPos = Vector2.zero;

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
        /// 初始化金币数据
        ///
        /// 椭圆模型：
        /// - 椭圆中心 = DropPoint（掉落起点）
        /// - X 轴半径 = HorizontalSpread，Y 轴半径 = DropHeight
        ///
        /// 动画设计（符合物理直觉）：
        /// - 先确定最终落点 P_final = DropPoint + direction × 椭圆射线交点距离（严格在椭圆边上）
        /// - 计算总路径 = DropPoint → P_final 的连线
        /// - 中间各反弹点都在这条连线上，按几何衰减 (1, decay, decay², ...) 分布
        /// - 每段动画：
        ///   - 起点 = 上一段终点（首段 = DropPoint）
        ///   - 终点 = 连线上预计算的下一个点
        ///   - 高度 = 衰减后的 verticalRadius
        ///
        /// 这保证最终落点严格在椭圆上，所有中间反弹点都在 DropPoint → P_final 的连线上
        /// </summary>
        public void Initialize(int value, Vector2 position, Vector2 dropDirection, CoinDropConfig dropConfig, CoinPickupConfig pickupConfig)
        {
            Value = Mathf.Max(1, value);
            _dropConfig = dropConfig ?? CoinDropConfig.Default;
            _pickupConfig = pickupConfig ?? CoinPickupConfig.Default;

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

            // 预计算所有段的起终点 + 高度（从 startPos 到 P_final 在连线上的几何分布）
            precomputeDropPath(_dropDirection);

            // 第一段动画
            setupCurrentSegment();

            State = CoinState.Dropping;

            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = 1f;
                _spriteRenderer.color = c;
            }
        }

        /// <summary>
        /// 尝试开始拾取动画
        /// </summary>
        public bool TryStartPickup(Transform targetTransform)
        {
            if (State != CoinState.Grounded)
                return false;

            initPickupAnimation(_transformCache.position, targetTransform);
            State = CoinState.BeingCollected;
            return true;
        }

        /// <summary>
        /// 更新掉落动画
        /// </summary>
        public void UpdateDropping(float dt)
        {
            if (State != CoinState.Dropping)
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
        /// 更新拾取动画
        /// </summary>
        public void UpdatePickup(float dt)
        {
            if (State != CoinState.BeingCollected)
                return;

            _pickupTimer += dt;
            float linearT = _pickupDuration > 0f ? Mathf.Clamp01(_pickupTimer / _pickupDuration) : 1f;

            // 应用 PickupCurve 曲线（默认 EaseOutCubic）：让位置、缩放、旋转同步缓动
            float t = MMTween.Evaluate(linearT, _pickupCurve);

            // 单段抛物线：从起始位置飞向目标位置
            Vector2 midPoint = (_pickupStartPos + (Vector2)_pickupTarget.position) * 0.5f;
            Vector2 toTarget = (Vector2)_pickupTarget.position - _pickupStartPos;
            float dist = toTarget.magnitude;

            // 控制点：垂直于运动方向偏移以形成弧线（模拟抛物线）
            if (dist > 0.001f)
            {
                Vector2 perpendicular = new Vector2(-toTarget.y, toTarget.x).normalized;
                // 弧度随距离增加，但有上限
                midPoint += perpendicular * Mathf.Min(0.5f, dist * 0.25f);
                midPoint += Vector2.up * Mathf.Min(1f, dist * 0.4f);
            }
            else
            {
                midPoint += Vector2.up * 0.5f;
            }

            Vector2 newPos = evaluateQuadraticBezier(_pickupStartPos, midPoint, _pickupTarget.position, t);

            // 拾取过程中金币会缩小并旋转
            float scaleMultiplier = Mathf.Lerp(1f, _pickupMinScale, t);
            var currentPos = _transformCache.position;
            _transformCache.position = new Vector3(newPos.x, newPos.y, currentPos.z);
            _transformCache.localScale = Vector3.one * scaleMultiplier;

            // 旋转效果
            float rotationZ = t * _pickupRotationDegrees;
            _transformCache.localRotation = Quaternion.Euler(0f, 0f, rotationZ);

            // 淡出效果
            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                _spriteRenderer.color = c;
            }

            // 拾取动画完成
            if (t >= 0.8f)
            {
                onPickupComplete();
            }
        }

        /// <summary>
        /// 设置金币状态
        /// </summary>
        public void SetState(CoinState newState)
        {
            State = newState;
        }

        /// <summary>
        /// 立即完成拾取
        /// </summary>
        public void ForceCollect()
        {
            if (State == CoinState.Grounded || State == CoinState.BeingCollected)
            {
                _pickupTimer = _pickupDuration;
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
        ///
        /// 关键设计：
        /// - 最终落点 P_final = DropPoint + direction × 椭圆交点距离（严格在椭圆边上）
        /// - 障碍物裁剪：如果 P_final 在障碍物之外（射线方向被墙挡住），用 CircleCast 检测障碍物交点，
        ///   实际 P_final 取 obstacleHit（不超出椭圆边界，但不超过障碍物）
        /// - 总路径 = DropPoint → P_final 连线（裁剪后可能不再是 direction 方向，但仍在 DropPoint → 椭圆方向附近）
        /// - 中间反弹点按几何衰减分布在连线上
        /// - 每段高度 verticalRadius[i] = DropHeight × decay^i
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
            float ellipseR = CoinEllipseScatter.RayEllipseIntersectionDistance(direction, a, b);
            Vector2 P_theoretical = _startPos + direction.normalized * ellipseR;

            // === 2. 障碍物裁剪 ===
            // 沿 direction 方向 CircleCast 从 DropPoint 出发，检测是否撞到 ObstacleLayers 上的物体
            // 如果撞到，用交点作为 P_final（避免金币穿过墙壁）
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
        /// 障碍物裁剪：从 startPos 沿 direction 方向 CircleCast（考虑金币半径），
        /// 如果撞到 ObstacleLayers 上的物体且距离 > epsilon，把 P_theoretical 裁剪到碰撞点
        /// </summary>
        /// <remarks>
        /// 行为说明：
        /// - 起点处的 Collider 不会被命中（Unity CircleCast 规则：origin 在 Collider 内时不注册命中）
        /// - hit.distance = 圆形探针从起点到碰撞点的移动距离
        /// - 探针半径 = CoinRadius，确保金币中心 + 半径 不会穿进障碍物
        /// - 如果 P_theoretical 没有被障碍物挡住（hit = 空 或 距离 > theoreticalDistance），
        ///   返回 P_theoretical（保持原椭圆落点）
        /// </remarks>
        Vector2 clipLandingByObstacle(Vector2 startPos, Vector2 direction, Vector2 P_theoretical)
        {
            int obstacleMask = _dropConfig.ObstacleLayers;
            if (obstacleMask == 0)
                return P_theoretical;

            Vector2 normDir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
            float theoreticalDistance = (P_theoretical - startPos).magnitude;

            // 探针半径 = 金币碰撞体半径
            float radius = Mathf.Max(0.01f, _dropConfig.CoinRadius);

            // CircleCast 从起点朝理论落点方向检测
            RaycastHit2D hit = Physics2D.CircleCast(
                startPos,
                radius,
                normDir,
                theoreticalDistance,
                obstacleMask);

            // 起点处的 Collider 会被 CircleCast 忽略（origin 在 Collider 内时不命中），
            // 但为安全起见，忽略极近距离的命中
            const float minHitDistance = 0.05f;
            if (hit.collider != null && hit.distance >= minHitDistance && hit.distance < theoreticalDistance)
            {
                // 圆心停在 hit.distance 位置（探针外缘刚好接触障碍物表面，金币边缘贴紧障碍物）
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
            int segIdx = _currentBounceIndex; // 0 表示第一段，1 表示第一次反弹，...

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

            // 判断是否还有下一段（还有未播放的端点）
            if (_segmentPositions != null && _currentBounceIndex + 1 < _segmentPositions.Length - 1)
            {
                setupNextBounce();
            }
            else
            {
                // 全部段完成，把金币精确对齐到最终落点（严格在椭圆上）
                var cPos = _transformCache.position;
                _transformCache.position = new Vector3(_finalLandingPos.x, _finalLandingPos.y, cPos.z);
                _transformCache.localScale = Vector3.one;

                // 金币落地
                State = CoinState.Grounded;
                _dropTimer = 0f;
                _currentBounceIndex = 0;
            }
        }

        /// <summary>
        /// 初始化拾取动画
        /// </summary>
        void initPickupAnimation(Vector2 startPos, Transform targetTransform)
        {
            _pickupStartPos = startPos;
            _pickupTarget = targetTransform;
            _pickupTimer = 0f;
            _pickupDuration = _pickupConfig.PickupDuration;
            _pickupRotationDegrees = _pickupConfig.RotationDegrees;
            _pickupMinScale = _pickupConfig.MinScale;
            _pickupCurve = _pickupConfig.PickupCurve;
        }

        /// <summary>
        /// 拾取动画完成
        /// </summary>
        void onPickupComplete()
        {
            State = CoinState.Collected;
            _pickupTimer = 0f;
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
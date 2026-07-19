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
        Vector2 _pickupTargetPos;
        float _pickupTimer;
        float _pickupDuration;
        float _pickupRotationDegrees;
        float _pickupMinScale;
        #endregion

        #region Components
        Transform _transformCache;
        SpriteRenderer _spriteRenderer;
        #endregion

        #region Lifecycle
        void Awake()
        {
            _transformCache = transform;
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
        public bool TryStartPickup(Vector2 targetPos)
        {
            if (State != CoinState.Grounded)
                return false;

            initPickupAnimation(_transformCache.position, targetPos);
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
            float t = _pickupDuration > 0f ? Mathf.Clamp01(_pickupTimer / _pickupDuration) : 1f;

            // 单段抛物线：从起始位置飞向目标位置
            Vector2 midPoint = (_pickupStartPos + _pickupTargetPos) * 0.5f;
            Vector2 toTarget = _pickupTargetPos - _pickupStartPos;
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

            Vector2 newPos = evaluateQuadraticBezier(_pickupStartPos, midPoint, _pickupTargetPos, t);

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
            if (t >= 1f)
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
        /// - 总路径 = DropPoint → P_final 连线
        /// - 中间反弹点按几何衰减分布在连线上（不在 X 轴上）：
        ///     比例 = 1, decay, decay², decay³, ...
        ///     归一化后: r_i = decay^i / (1 + decay + decay² + ...)
        ///     中间点 P_i = DropPoint + pathDir × 累计距离（保持 pathDir 方向，Y 按 pathDir 自然变化）
        /// - 路径总长 = DropPoint 到 P_final 的距离
        /// - 每段高度 verticalRadius[i] = DropHeight × decay^i（按 BounceDecayRatio 衰减）
        /// </summary>
        void precomputeDropPath(Vector2 direction)
        {
            int totalSegments = Mathf.Max(0, _dropConfig.BounceCount) + 1; // 第一段 + N 段反弹
            int totalPoints = totalSegments + 1; // 起 + 中间 + 终

            _segmentPositions = new Vector2[totalPoints];
            _segmentHeights = new float[totalSegments];

            // === 1. 计算最终落点 P_final ===
            float a = _dropConfig.HorizontalSpread;
            float b = _dropConfig.DropHeight;
            float ellipseR = CoinEllipseScatter.RayEllipseIntersectionDistance(direction, a, b);
            Vector2 P_final = _startPos + direction.normalized * ellipseR;

            // === 2. 计算总路径方向和长度 ===
            Vector2 totalOffset = P_final - _startPos;
            float totalDistance = totalOffset.magnitude;

            // 如果总距离太小（direction 落在椭圆内部或者几乎为零），所有中间点都重叠到 DropPoint
            if (totalDistance < 0.0001f)
            {
                _segmentPositions[0] = _startPos;
                _segmentPositions[totalPoints - 1] = _startPos;
                for (int i = 1; i < totalPoints - 1; i++)
                    _segmentPositions[i] = _startPos;
                for (int i = 0; i < totalSegments; i++)
                    _segmentHeights[i] = _dropConfig.DropHeight;
                return;
            }

            Vector2 pathDir = totalOffset / totalDistance; // 从 DropPoint 到 P_final 的单位方向

            // === 3. 计算每段高度（按 BounceDecayRatio 衰减）===
            float decay = Mathf.Clamp01(_dropConfig.BounceDecayRatio);
            for (int i = 0; i < totalSegments; i++)
            {
                _segmentHeights[i] = _dropConfig.DropHeight * Mathf.Pow(decay, i);
            }

            // === 4. 计算各段端点在连线上的位置（严格在 pathDir 方向上，不强制改 Y）===
            _segmentPositions[0] = _startPos;

            // 几何分布：每段水平位移 = d × decay^i / Σdecay^j
            float sumRatio = 0f;
            for (int i = 0; i < totalSegments; i++)
                sumRatio += Mathf.Pow(decay, i);
            if (sumRatio < 0.0001f) sumRatio = 1f;

            float segmentLength = totalDistance / sumRatio;

            // 累计距离，按等比数列计算每段终点
            float accumulated = 0f;
            for (int i = 0; i < totalSegments; i++)
            {
                accumulated += segmentLength * Mathf.Pow(decay, i);
                // 第 i 段终点：在连线上、Y 按 pathDir 自然变化（不再强制改 Y）
                // 即：如果 direction 有 Y 分量，中间点也会沿 Y 推进
                Vector2 point = _startPos + pathDir * accumulated;
                _segmentPositions[i + 1] = point;
            }

            // 强制最后一点 = P_final（确保数值精度下也在椭圆上）
            _segmentPositions[totalPoints - 1] = P_final;

            // 保存最终精确位置（椭圆上）
            _finalLandingPos = P_final;
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
        void initPickupAnimation(Vector2 startPos, Vector2 targetPos)
        {
            _pickupStartPos = startPos;
            _pickupTargetPos = targetPos;
            _pickupTimer = 0f;
            _pickupDuration = _pickupConfig.PickupDuration;
            _pickupRotationDegrees = _pickupConfig.RotationDegrees;
            _pickupMinScale = _pickupConfig.MinScale;
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

using System;
using UnityEngine;

namespace MoreMountains
{
    public class ParabolicProjectile : Projectile
    {
        [Serializable]
        public class ParabolicConfig
        {
            public float Duration = 0.5f;

            /// <summary>
            /// 反弹衰减比例（0-1），值越小衰减越快
            /// 每段反弹后椭圆长半轴 R 乘以此系数（高度随之同步缩小）
            /// </summary>
            public float BounceDecayRatio = 0.6f;

            /// <summary>
            /// 椭圆短半轴 b - 即抛物线在Y方向上的最大高度
            /// 由3D正圆相对XY平面的倾角θ决定：b = R * sin(θ)
            /// 反弹时 R 衰减，高度 b 也按相同比例自动衰减
            /// </summary>
            public float Height = 1.2f;

            /// <summary>
            /// 创建默认配置
            /// </summary>
            public static ParabolicConfig Default = new()
            {
                Duration = 2f,
                BounceDecayRatio = 0.6f,
                Height = 2f,
            };
        }

        public ParabolicConfig Config;

        public enum States
        {
            Idle, // 空闲
            Flying, // 抛物线动画中
            Grounded, // 落地静止
        }

        /// <summary>
        /// 金币当前状态
        /// </summary>
        public States State { get; protected set; } = States.Idle;

        /// <summary>
        /// 当前世界位置
        /// </summary>
        public Vector2 Position => transform.position;

        // 掉落动画状态
        float _timer;

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
        Vector2 _finalLandingPos; // 严格在椭圆上的最终落点（动画结束时对齐到此位置）
        float damageRadius;

        ParabolicConfig _parabolicConfig;
        
        public GameEffect indicatorEffect;
        public CircleRegion circleRegion;
        public Vector2 aimPosition;
        public FxDefine GroundedEffect = FxDefine.EXPLODE_RED;

        public Action<ParabolicProjectile, Vector2, Vector2, float> OnStartFlying;
        public Action<ParabolicProjectile, float> OnFlying;
        public Action<ParabolicProjectile> OnGrounded;

        public override void onAcquire()
        {
            inUse = true;
        }

        public override void onRelease()
        {
            inUse = false;
        }

        protected override void Initialization()
        {
            base.Initialization();
            // _instanceID = gameObject.GetInstanceID();
        }

        public void Initialize(Vector2 position, Vector2 targetPosition, float radius)
        {
            _parabolicConfig = Config ?? ParabolicConfig.Default;

            // 重置位置
            var currentPos = transform.position;
            transform.position = new(position.x, position.y, currentPos.z);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            // 缓存 DropPoint
            _startPos = position;
            damageRadius = radius;

            // 预计算所有段的起终点 + 高度（从 startPos 到 P_final 在连线上的几何分布）
            precomputeDropPath(targetPosition);

            // 第一段动画
            setupCurrentSegment();

            State = States.Flying;
            OnStartFlying?.Invoke(this, position, targetPosition, radius);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            State = States.Idle;
            _timer = 0f;
            _segmentVerticalRadius = 0f;
            _segmentMaxHeight = 0f;
            UN_ARRAY(ref _segmentPositions);
            UN_ARRAY(ref _segmentHeights);
            _startPos = Vector2.zero;
            _finalLandingPos = Vector2.zero;
            OnFlying = null;
            OnGrounded = null;

            if (transform)
            {
                transform.localScale = Vector3.one;
                transform.localRotation = Quaternion.identity;
            }
        }

        public override void OnUpdate(float dt)
        {
            UpdateFlying(dt);
        }

        public override void OnFixedUpdate(float dt)
        {
        }

        public void UpdateFlying(float dt)
        {
            if (State != States.Flying)
                return;

            _timer += dt;
            float t = _segmentDuration > 0f ? Mathf.Clamp01(_timer / _segmentDuration) : 1f;

            OnFlying?.Invoke(this, t);

            // 用二次贝塞尔曲线模拟椭圆弧（在XY平面上的投影）
            Vector3 newPos = evaluateQuadraticBezier(_segmentStartPos, _segmentControlPoint, _segmentEndPos, t);

            // 模拟3D高度的视觉表现：高度在抛物线顶点(t=0.5)最高，落地时接近0
            float heightFactor = Mathf.Sin(t * Mathf.PI);
            float scaleMultiplier = 1f + heightFactor * 0.3f;

            // 保持Z值
            var currentPos = transform.position;
            transform.position = new(newPos.x, newPos.y, currentPos.z);
            transform.localScale = Vector3.one * scaleMultiplier;
            FaceMovementDirection(newPos - currentPos);

            // 当前段动画完成
            if (t >= 1f)
            {
                onSegmentComplete();
            }
        }


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
        void precomputeDropPath(Vector2 targetPosition)
        {
            const int totalSegments = 1;
            const int totalPoints = 2;

            ARRAY_PERSIST(out _segmentHeights, totalSegments);
            ARRAY_PERSIST(out _segmentPositions, totalPoints);

            Vector2 P_final = targetPosition;

            // === 3. 计算总路径方向和长度 ===
            Vector2 totalOffset = P_final - _startPos;
            float totalDistance = totalOffset.magnitude;

            // 如果总距离太小，所有中间点都重叠到 DropPoint
            if (totalDistance < 0.0001f)
            {
                _segmentPositions[0] = _startPos;
                _segmentPositions[totalPoints - 1] = _startPos;
                for (int i = 0; i < totalSegments; i++)
                    _segmentHeights[i] = _parabolicConfig.Height;
                _finalLandingPos = _startPos;
                return;
            }

            Vector2 pathDir = totalOffset / totalDistance;

            // === 4. 计算每段高度（按 BounceDecayRatio 衰减）===
            float decay = Mathf.Clamp01(_parabolicConfig.BounceDecayRatio);
            for (int i = 0; i < totalSegments; i++)
            {
                _segmentHeights[i] = _parabolicConfig.Height * Mathf.Pow(decay, i);
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
        /// 设置当前段动画参数（使用预计算的 _segmentPositions / _segmentHeights）
        /// </summary>
        void setupCurrentSegment()
        {
            _segmentStartPos = _segmentPositions[0];
            _segmentEndPos = _segmentPositions[1];

            float verticalRadius = _segmentHeights[0];
            if (verticalRadius < 0.01f)
                verticalRadius = 0.01f;

            _segmentVerticalRadius = verticalRadius;
            _segmentMaxHeight = verticalRadius;

            // 抛物线顶点：起点和终点的中点 + Vector2.up * verticalRadius
            float midX = (_segmentStartPos.x + _segmentEndPos.x) * 0.5f;
            float baseY = Mathf.Max(_segmentStartPos.y, _segmentEndPos.y);
            _segmentControlPoint = new(midX, baseY + verticalRadius);

            // 每段动画时间
            int totalSegments = _segmentPositions.Length - 1;
            _segmentDuration = _parabolicConfig.Duration / Mathf.Max(1, totalSegments);
            _timer = 0f;
        }

        /// <summary>
        /// 当前段动画完成回调
        /// </summary>
        void onSegmentComplete()
        {
            var cPos = transform.position;
            transform.position = new(_finalLandingPos.x, _finalLandingPos.y, cPos.z);
            transform.localScale = Vector3.one;

            // 金币落地
            State = States.Grounded;
            _timer = 0f;

            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(CHARACTER_LAYER_MASK);
            using var _ = new ListScope<Collider2D>(out var colliders);
            var count = Physics2D.OverlapCircle(_finalLandingPos, damageRadius, filter, colliders);
            for (var i = 0; i < count; i++)
            {
                _damageOnTouch.ForceColliding(colliders[i].gameObject);
            }

            OnGrounded?.Invoke(this);
            _health.Kill();
            fx.play(GroundedEffect, _finalLandingPos);
        }

        /// <summary>
        /// 二次贝塞尔曲线求值
        /// B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        /// </summary>
        static Vector2 evaluateQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
        }
    }
}
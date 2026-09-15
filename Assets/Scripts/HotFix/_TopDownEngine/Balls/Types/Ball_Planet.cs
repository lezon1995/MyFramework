using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 行星球 - 环绕玩家飞行
    /// - 发射时有一段弧线运动飞到轨道上（这段时间不撞击砖块）
    /// - 飞行到轨道后开启砖块碰撞检测
    /// - 碰撞后即刻反向环绕运动
    /// </summary>
    public class Ball_Planet : Ball
    {
        public override BallType BallType => BallType.Planet;

        [Header("Orbit Settings")]
        [Tooltip("环绕轨道半径")]
        public float orbitRadius
        {
            get
            {
                _player.GetStat(Character.Stat.Range, out var stat);
                float multiplier = 1f + (stat.Value / 10F);
                return 2f * multiplier;
            }
        }

        float orbitSign = 1;

        [Tooltip("环绕速度（弧度/秒），正值=逆时针，负值=顺时针")]
        public float orbitSpeed => moveSpeed * orbitSign;

        [Tooltip("弧线飞入速度")]
        public float arcFlySpeed => moveSpeed;

        [Tooltip("弧线飞行完成距离阈值")]
        public float orbitArrivalThreshold = 0.3f;

        // 当前环绕角度（弧度）
        protected float _orbitAngle;

        // 是否处于弧线飞入阶段
        protected bool _isArcFlying = true;

        // 是否已开启砖块碰撞
        protected bool _brickCollisionEnabled;

        // 弧线飞入的目标位置
        protected Vector3 _arcTargetPosition;

        // 弧线飞行起始位置
        protected Vector3 _arcStartPosition;

        // 弧线飞行已用时间
        protected float _arcElapsedTime;

        // 弧线飞行总时长
        protected float _arcDuration;

        // 弧线初始速度方向
        protected Vector3 _arcInitialDirection;

        // 记录弧线飞入前的碰撞层设置
        protected LayerMask _originalBounceLayers;

        public override void reset()
        {
            base.reset();
            _isArcFlying = true;
            _brickCollisionEnabled = false;
            _orbitAngle = 0f;
            _arcElapsedTime = 0f;
            _arcStartPosition = Vector3.zero;
            _arcTargetPosition = Vector3.zero;
            _arcDuration = 0f;
            _arcInitialDirection = Vector3.zero;

            // 恢复原始碰撞层
            if (_originalBounceLayers.value != 0)
            {
                BounceLayers = _originalBounceLayers;
            }
        }

        public override void onPreparedToShoot()
        {
            base.onPreparedToShoot();
            InitArcFly(getWorldPosition(), Direction, Player.getWorldPosition());
        }

        public override void SetOwner(Character newOwner)
        {
            base.SetOwner(newOwner);

            // 记录发射前的初始方向
            _arcInitialDirection = Direction;
        }

        /// <summary>
        /// 初始化弧线飞入阶段
        /// </summary>
        /// <param name="startPos">起始位置</param>
        /// <param name="initialDir">初始发射方向</param>
        /// <param name="playerPos">玩家位置，用于计算轨道目标点</param>
        public void InitArcFly(Vector3 startPos, Vector3 initialDir, Vector3 playerPos)
        {
            _arcStartPosition = startPos;
            _arcInitialDirection = initialDir;
            _arcElapsedTime = 0f;
            _isArcFlying = true;
            _brickCollisionEnabled = false;

            // 保存原始碰撞层设置
            _originalBounceLayers = BounceLayers;

            // 暂时移除砖块层，只与障碍物和边界碰撞
            BounceLayers = LayerManager.Obstacles_Mask | LayerManager.Border_Mask;

            // 计算轨道上的目标位置（在玩家位置的基础上，从球的初始方向确定角度）
            _orbitAngle = Mathf.Atan2(initialDir.y, initialDir.x);
            _arcTargetPosition = playerPos + new Vector3(
                Mathf.Cos(_orbitAngle) * orbitRadius,
                Mathf.Sin(_orbitAngle) * orbitRadius,
                0f
            );

            // 计算弧线飞行时间（基于距离和速度）
            float distanceToTarget = Vector3.Distance(startPos, _arcTargetPosition);
            _arcDuration = distanceToTarget / arcFlySpeed;
            _arcDuration = Mathf.Max(_arcDuration, 0.1f); // 最小时间
        }

        public override void OnFixedUpdate(float dt)
        {
            CheckBrickHitTimerExpiration(dt);
            
            if (!_shouldMove)
                return;

            // 弧线飞入阶段
            if (_isArcFlying)
            {
                UpdateArcFly(dt);
            }
            // 环绕阶段
            else
            {
                UpdateOrbit(dt);
            }

            // 更新位置
            UpdatePosition();

            CheckBallExpiration(dt);
            CheckDashHitExpiration(dt);
        }

        /// <summary>
        /// 更新弧线飞入
        /// </summary>
        protected virtual void UpdateArcFly(float dt)
        {
            _arcElapsedTime += dt;

            // 使用缓动函数让弧线更自然（先快后慢）
            float t = Mathf.Clamp01(_arcElapsedTime / _arcDuration);
            float easedT = EaseOutCubic(t);

            // 贝塞尔曲线控制点 - 向上弯曲形成抛物线
            Vector3 controlPoint = _arcStartPosition + new Vector3(_arcInitialDirection.x, _arcInitialDirection.y, 0f) * (orbitRadius * 1.5f);

            // 计算贝塞尔曲线上的位置
            Vector3 newPos = CalculateQuadraticBezierPoint(_arcStartPosition, controlPoint, _arcTargetPosition, easedT);

            // 更新位置
            curPos = newPos;

            // 检查是否到达轨道
            float distanceToTarget = Vector3.Distance(curPos, _arcTargetPosition);
            if (distanceToTarget < orbitArrivalThreshold || t >= 1f)
            {
                // 到达轨道，切换到环绕模式
                ArriveAtOrbit();
            }
        }

        /// <summary>
        /// 到达轨道，切换到环绕模式
        /// </summary>
        protected virtual void ArriveAtOrbit()
        {
            _isArcFlying = false;
            _brickCollisionEnabled = true;

            // 恢复完整的砖块碰撞层
            BounceLayers = _originalBounceLayers;

            // 修正位置到精确的轨道位置
            curPos = _arcTargetPosition;
        }

        /// <summary>
        /// 更新环绕运动
        /// </summary>
        protected virtual void UpdateOrbit(float dt)
        {
            // 获取玩家位置
            Vector3 playerPos = Player.getWorldPosition();

            // 更新角度（orbitSpeed 为负值时顺时针，正值时逆时针）
            _orbitAngle += orbitSpeed * dt;

            // 计算轨道上的新位置
            Vector3 orbitPos = playerPos + new Vector3(
                Mathf.Cos(_orbitAngle) * orbitRadius,
                Mathf.Sin(_orbitAngle) * orbitRadius,
                0f
            );

            curPos = orbitPos;

            // 更新方向为切线方向，用于碰撞检测
            // 切线方向 = (-sin(angle), cos(angle)) * direction
            float tangentSign = orbitSpeed >= 0 ? 1f : -1f;
            Direction = new Vector3(-Mathf.Sin(_orbitAngle), Mathf.Cos(_orbitAngle), 0f) * tangentSign;
        }

        /// <summary>
        /// 更新实际位置
        /// </summary>
        protected virtual void UpdatePosition()
        {
            prePos = curPos;

            if (_hasRigidBody2D)
            {
                _rigidBody2D.MovePosition(curPos);
            }

            transform.position = curPos;

            // 更新朝向
            if (Direction.magnitude > 0.01f)
            {
                transform.right = Direction;
            }
        }

        /// <summary>
        /// 二阶贝塞尔曲线计算
        /// </summary>
        protected Vector3 CalculateQuadraticBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 +
                   2f * oneMinusT * t * p1 +
                   t * t * p2;
        }

        /// <summary>
        /// 缓动函数 - 二次缓出
        /// </summary>
        protected float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public override bool CollidingWithBrick(Brick brick, Vector2 normal)
        {
            // 在环绕模式下，碰撞后反向
            if (!_isArcFlying && _brickCollisionEnabled)
            {
                // 只有砖块碰撞才反向，障碍物和边界正常反弹
                // 反向环绕
                ReverseOrbitDirection();
                // 修正位置到轨道上
                SnapToOrbit();
            }

            return base.CollidingWithBrick(brick, normal);
        }

        /// <summary>
        /// 反向环绕方向
        /// </summary>
        public virtual void ReverseOrbitDirection()
        {
            orbitSign = -orbitSign;
        }

        /// <summary>
        /// 将球修正到最近的轨道位置
        /// </summary>
        protected virtual void SnapToOrbit()
        {
            Vector3 playerPos = Player.getWorldPosition();

            // 计算当前角度
            Vector3 toBall = curPos - playerPos;
            float angle = Mathf.Atan2(toBall.y, toBall.x);

            // 修正到轨道
            curPos = playerPos + new Vector3(
                Mathf.Cos(angle) * orbitRadius,
                Mathf.Sin(angle) * orbitRadius,
                0f
            );

            // 更新环绕角度
            _orbitAngle = angle;
        }

        /// <summary>
        /// 手动设置轨道角度（用于同步显示等）
        /// </summary>
        public void SetOrbitAngle(float angle)
        {
            _orbitAngle = angle;
            if (!_isArcFlying)
            {
                Vector3 playerPos = Player.getWorldPosition();
                curPos = playerPos + new Vector3(
                    Mathf.Cos(_orbitAngle) * orbitRadius,
                    Mathf.Sin(_orbitAngle) * orbitRadius,
                    0f
                );
            }
        }

        /// <summary>
        /// 获取当前轨道角度
        /// </summary>
        public float GetOrbitAngle()
        {
            return _orbitAngle;
        }

        /// <summary>
        /// 是否在环绕模式
        /// </summary>
        public bool IsOrbiting => !_isArcFlying;

        /// <summary>
        /// 是否已完成弧线飞入
        /// </summary>
        public bool HasArrivedAtOrbit => !_isArcFlying;
    }
}
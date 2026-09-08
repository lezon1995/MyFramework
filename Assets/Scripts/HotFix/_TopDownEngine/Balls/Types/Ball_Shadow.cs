using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 暗影球 - 会进入砖块内部，在碰撞体内边缘反弹
    /// 砖块阵亡后才会弹出
    /// </summary>
    public class Ball_Shadow : Ball, IEvent<OnBrickDeath>
    {
        public override BallType BallType => BallType.Shadow;

        /// <summary>
        /// 暗影球进入的砖块列表
        /// </summary>
        protected List<Brick> _enteredBricks = new();

        /// <summary>
        /// 记录上一次重叠的砖块，用于检测状态变化
        /// </summary>
        protected Brick _lastOverlappingBrick;

        public override void onAcquire()
        {
            base.onAcquire();
            // 暗影球设置为穿透模式，可以进入砖块内部
            setPenetrable(true);
        }

        public override void onRelease()
        {
            // 清理所有监听的砖块事件
            foreach (var brick in _enteredBricks)
            {
                if (brick != null)
                    brick.Event.removeListener<OnBrickDeath>(this);
            }

            _enteredBricks.Clear();
            _lastOverlappingBrick = null;

            base.onRelease();
        }

        /// <summary>
        /// 重写碰撞砖块的逻辑
        /// 暗影球在碰撞体内边缘反弹，不处理外部碰撞
        /// </summary>
        public override bool CollidingWithBrick(Brick brick, Vector2 normal)
        {
            // 暗影球不处理外部碰撞，让球进入砖块内部
            collidingBrick = brick;
            return false;
        }

        protected override void OnFixedUpdateOverlappingBrick(Brick brick, float dt)
        {
            base.OnFixedUpdateOverlappingBrick(brick, dt);

            // 暗影球在砖块内部持续造成伤害
            // 基础逻辑由父类的重叠机制处理
        }

        public override void OnFixedUpdate(float dt)
        {
            // 检测球进入/离开砖块的状态变化
            DetectBrickStateChange();

            // 如果有进入的砖块，检测它们是否还存活
            for (int i = _enteredBricks.Count - 1; i >= 0; i--)
            {
                var brick = _enteredBricks[i];
                if (brick == null || brick.Health.CurrentHealth <= 0)
                {
                    // 砖块已阵亡
                    if (brick != null)
                    {
                        brick.Event.removeListener<OnBrickDeath>(this);
                    }

                    _enteredBricks.RemoveAt(i);
                }
            }

            base.OnFixedUpdate(dt);
        }

        /// <summary>
        /// 重写移动逻辑
        /// 在砖块内部时，使用纯数学计算球与砖块内边的相切点
        /// </summary>
        protected override void Movement(float dt)
        {
            // 如果球在砖块内部，使用暗影球的特殊移动逻辑
            if (isOverlappingBrick && overlappingBrick != null)
            {
                ShadowMovement(dt);
            }
            else
            {
                base.Movement(dt);
            }
        }
        Vector3 tangentBallCenter;
        /// <summary>
        /// 暗影球在砖块内部的移动逻辑
        /// 纯数学计算球沿当前方向运动到砖块内边的相切点
        /// </summary>
        protected void ShadowMovement(float dt)
        {
            hasCorrectPosThisFixedUpdate = false;
            prePos = curPos;

            var range = moveSpeed * dt;
            var dir = Direction.normalized;

            if (curPos == tangentBallCenter)
            {
                DamageOnTouch.Colliding(collidingBrick, lastHitNormal);
                tangentBallCenter = Vector3.zero;
                lastHitNormal = default;
                return;
            }

            // 计算球从当前球心位置沿 dir 方向运动到砖块内边的相切点和相切球心位置
            if (TryCalculateInternalTangent(overlappingBrick, curPos, dir, Radius, out var tangentPos, out var hitNormal, out var hitPoint))
            {
                lastHitNormal = hitNormal;
                // 球心需要到达的位置：相切点 + 法线方向 * 半径
                // 注意：法线指向矩形内部，所以球心到达的位置在矩形边界加上半径的内侧
                tangentBallCenter = tangentPos;

                // 球心移动的距离
                var moveVec = tangentBallCenter - curPos;
                var moveDist = moveVec.magnitude;

                if (moveDist <= range)
                {
                    // 只移动到相切位置，不超出
                    _movement = moveVec;
                    curPos = tangentBallCenter;

                    if (_hasRigidBody2D)
                    {
                        _rigidBody2D.MovePosition(tangentBallCenter);
                    }

                    // 执行反弹
                    // PerformShadowBounce(hitNormal, hitPoint);
                }
                else
                {
                    // 不会撞到边，正常移动
                    _movement = dir * range;
                    curPos = curPos + dir * range;

                    if (_hasRigidBody2D)
                    {
                        _rigidBody2D.MovePosition(curPos);
                    }
                }
            }
            else
            {
                // 没有有效的相切点，正常移动
                _movement = dir * range;
                curPos = curPos + dir * range;

                if (_hasRigidBody2D)
                {
                    _rigidBody2D.MovePosition(curPos);
                }
            }

            Debug.DrawLine(prePos, curPos, Color.red, dt);
            if (tangentBallCenter != Vector3.zero)
            {
                Debug.DrawLine(curPos, tangentBallCenter, Color.green, dt);
            }
        }

        /// <summary>
        /// 纯数学计算球从砖块内部沿 dir 方向运动到内边的相切点
        /// </summary>
        /// <param name="brick">砖块</param>
        /// <param name="ballCenter">球心当前位置</param>
        /// <param name="dir">运动方向（单位向量）</param>
        /// <param name="radius">球半径</param>
        /// <param name="tangentBallCenter">相切时球心应该到达的位置</param>
        /// <param name="hitNormal">击中点的法线（指向矩形内部）</param>
        /// <param name="hitPoint">击中点（位于矩形边线上）</param>
        /// <returns>是否找到有效的相切</returns>
        protected static bool TryCalculateInternalTangent(Brick brick, Vector2 ballCenter, Vector2 dir, float radius,
            out Vector2 tangentBallCenter, out Vector2 hitNormal, out Vector2 hitPoint)
        {
            tangentBallCenter = ballCenter;
            hitNormal = Vector2.zero;
            hitPoint = Vector2.zero;

            var rect = brick.getRect();
            float xMin = rect.xMin;
            float xMax = rect.xMax;
            float yMin = rect.yMin;
            float yMax = rect.yMax;

            // 球在矩形内部时，球心范围: xMin + r < x < xMax - r, yMin + r < y < yMax - r
            // 但球初始可能非常接近边界，先不强制此约束

            // 计算沿 dir 方向运动到四条内边的参数 t（t > 0 表示会撞到该边）
            // 球心到左边的距离：xMin + r - ballCenter.x
            //   沿 dir 方向到达左边对应球心位置的时间 t = (xMin + r - ballCenter.x) / dir.x
            // 类似计算其它三条边

            float tMin = float.PositiveInfinity;
            int hitSide = -1; // 0=左, 1=右, 2=下, 3=上

            // 计算到四条边的时间参数
            // 左内边：球心到达 (xMin + r, ballCenter.y) 的位置
            // 这里球撞到的是 x = xMin 这条边，法线指向右 (+X)
            if (Mathf.Abs(dir.x) > 0F)
            {
                float tx = (xMin + radius - ballCenter.x) / dir.x;
                if (tx > 0F)
                {
                    float yAtT = ballCenter.y + dir.y * tx;
                    // 撞到左边时，球心的 y 应该在 [yMin, yMax] 范围内（球心在线段范围内）
                    if (yAtT >= yMin && yAtT <= yMax)
                    {
                        if (tx < tMin)
                        {
                            tMin = tx;
                            hitSide = 0;
                        }
                    }
                }
            }

            // 右内边
            if (Mathf.Abs(dir.x) > 0F)
            {
                float tx = (xMax - radius - ballCenter.x) / dir.x;
                if (tx > 0F)
                {
                    float yAtT = ballCenter.y + dir.y * tx;
                    if (yAtT >= yMin && yAtT <= yMax)
                    {
                        if (tx < tMin)
                        {
                            tMin = tx;
                            hitSide = 1;
                        }
                    }
                }
            }

            // 下内边
            if (Mathf.Abs(dir.y) > 0F)
            {
                float ty = (yMin + radius - ballCenter.y) / dir.y;
                if (ty > 0F)
                {
                    float xAtT = ballCenter.x + dir.x * ty;
                    if (xAtT >= xMin && xAtT <= xMax)
                    {
                        if (ty < tMin)
                        {
                            tMin = ty;
                            hitSide = 2;
                        }
                    }
                }
            }

            // 上内边
            if (Mathf.Abs(dir.y) > 0F)
            {
                float ty = (yMax - radius - ballCenter.y) / dir.y;
                if (ty > 0F)
                {
                    float xAtT = ballCenter.x + dir.x * ty;
                    if (xAtT >= xMin && xAtT <= xMax)
                    {
                        if (ty < tMin)
                        {
                            tMin = ty;
                            hitSide = 3;
                        }
                    }
                }
            }

            if (hitSide < 0)
                return false;

            // 根据击中边计算相切球心位置和法线
            switch (hitSide)
            {
                case 0: // 左内边
                    tangentBallCenter = new Vector2(xMin + radius, ballCenter.y + dir.y * tMin);
                    hitNormal = Vector2.right; // 指向矩形内部
                    hitPoint = new Vector2(xMin, tangentBallCenter.y);
                    break;
                case 1: // 右内边
                    tangentBallCenter = new Vector2(xMax - radius, ballCenter.y + dir.y * tMin);
                    hitNormal = Vector2.left;
                    hitPoint = new Vector2(xMax, tangentBallCenter.y);
                    break;
                case 2: // 下内边
                    tangentBallCenter = new Vector2(ballCenter.x + dir.x * tMin, yMin + radius);
                    hitNormal = Vector2.up;
                    hitPoint = new Vector2(tangentBallCenter.x, yMin);
                    break;
                case 3: // 上内边
                    tangentBallCenter = new Vector2(ballCenter.x + dir.x * tMin, yMax - radius);
                    hitNormal = Vector2.down;
                    hitPoint = new Vector2(tangentBallCenter.x, yMax);
                    break;
            }

            return true;
        }

        /// <summary>
        /// 执行暗影球的反弹
        /// </summary>
        protected void PerformShadowBounce(Vector2 hitNormal, Vector2 hitPoint)
        {
            // 计算反弹方向
            var reflectDir = Vector2.Reflect(Direction, hitNormal).normalized;

            // 播放反弹反馈
            BounceFeedback.Play();

            // 设置新方向
            SetDirection(reflectDir, Quaternion.identity);

            // 更新反弹次数
            _bouncesLeft--;

            if (_bouncesLeft <= 0)
            {
                _health.Kill();
                _damageOnTouch.HitNonDamageableFeedback.Play();
            }

            onBounceFinished();

            Debug.DrawLine(curPos, curPos + (Vector3)reflectDir, Color.magenta, 1F);
            Debug.Log($"[Ball_Shadow] 在砖块内部反弹，方向: {reflectDir}，击中点: {hitPoint}");
        }

        /// <summary>
        /// 检测球进入/离开砖块的状态变化
        /// </summary>
        protected void DetectBrickStateChange()
        {
            if (isOverlappingBrick && overlappingBrick != null)
            {
                // 球正在砖块内部
                if (_lastOverlappingBrick != overlappingBrick)
                {
                    // 进入了一个新的砖块
                    OnBrickEnter(overlappingBrick);
                    _lastOverlappingBrick = overlappingBrick;
                }
            }
            else
            {
                // 球不在任何砖块内部
                if (_lastOverlappingBrick != null)
                {
                    // 离开了之前的砖块
                    OnBrickExit(_lastOverlappingBrick);
                    _lastOverlappingBrick = null;
                }
            }
        }

        /// <summary>
        /// 当球进入砖块内部时调用
        /// </summary>
        protected void OnBrickEnter(Brick brick)
        {
            if (!_enteredBricks.Contains(brick))
            {
                _enteredBricks.Add(brick);
                brick.Event.addListener<OnBrickDeath>(this);
                Debug.Log($"[Ball_Shadow] 球进入砖块: {brick.name}");
            }
        }

        /// <summary>
        /// 当球离开砖块内部时调用
        /// </summary>
        protected void OnBrickExit(Brick brick)
        {
            if (_enteredBricks.Contains(brick))
            {
                // 只有当砖块还存活时才移除监听（阵亡时会通过事件自动清理）
                if (brick.Health.CurrentHealth > 0)
                {
                    brick.Event.removeListener<OnBrickDeath>(this);
                    _enteredBricks.Remove(brick);
                }

                Debug.Log($"[Ball_Shadow] 球离开砖块: {brick.name}");
            }
        }

        /// <summary>
        /// 从砖块中弹出暗影球
        /// </summary>
        protected void PopOutFromBrick(Brick brick)
        {
            if (brick == null)
                return;

            var rect = brick.getRect();
            var circle = getCircle();

            // 计算从砖块中心到球心的方向
            var popDir = ((Vector2)curPos - rect.center).normalized;

            // 如果方向接近零，使用当前运动方向
            if (popDir.sqrMagnitude < 0.001f)
            {
                popDir = Direction;
            }

            // 确保弹出方向是单位向量
            popDir = popDir.normalized;

            // 找到最近的边缘
            float distToRight = rect.xMax - circle.mCenter.x;
            float distToLeft = circle.mCenter.x - rect.xMin;
            float distToTop = rect.yMax - circle.mCenter.y;
            float distToBottom = circle.mCenter.y - rect.yMin;

            float minDist = Mathf.Min(distToRight, distToLeft, distToTop, distToBottom);
            Vector2 exitDir;

            if (minDist == distToRight)
                exitDir = Vector2.right;
            else if (minDist == distToLeft)
                exitDir = Vector2.left;
            else if (minDist == distToTop)
                exitDir = Vector2.up;
            else
                exitDir = Vector2.down;

            // 设置弹出方向
            setDirection(exitDir);

            Debug.DrawLine(curPos, curPos + (Vector3)exitDir * 2f, Color.yellow, 1F);
            Debug.Log($"[Ball_Shadow] 砖块阵亡，球从边缘 {exitDir} 弹出");
        }

        /// <summary>
        /// 砖块阵亡事件
        /// </summary>
        public void onEvent(OnBrickDeath e)
        {
            e.brick.Event.removeListener<OnBrickDeath>(this);

            _enteredBricks.Remove(e.brick);

            // 砖块阵亡后弹出
            PopOutFromBrick(e.brick);
        }
    }
}
using System.Collections.Generic;
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

        /// <summary>
        /// 上一帧计算的相切球心位置
        /// </summary>
        protected Vector3 _tangentBallCenter;

        /// <summary>
        /// 上一帧计算的击中法线
        /// </summary>
        protected Vector2 _lastHitNormal;

        protected Vector2 _lastHitPoint;

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
                brick.Event.removeListener<OnBrickDeath>(this);
            }

            _enteredBricks.Clear();
            _lastOverlappingBrick = null;
            _tangentBallCenter = Vector3.zero;
            _lastHitNormal = default;

            base.onRelease();
        }

        /// <summary>
        /// 重写碰撞砖块的逻辑
        /// 暗影球不处理外部碰撞，让球进入砖块内部
        /// </summary>
        public override bool CollidingWithBrick(Brick brick, Vector2 normal)
        {
            collidingBrick = brick;
            return false;
        }

        protected override void OnFixedUpdateOverlappingBrick(Brick brick, float dt)
        {
            base.OnFixedUpdateOverlappingBrick(brick, dt);
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
                    brick.Event.removeListener<OnBrickDeath>(this);
                    _enteredBricks.RemoveAt(i);
                }
            }

            base.OnFixedUpdate(dt);
        }

        /// <summary>
        /// 重写移动逻辑
        /// </summary>
        protected override void Movement(float dt)
        {
            if (isOverlappingBrick && overlappingBrick)
            {
                ShadowMovement(dt);
            }
            else
            {
                base.Movement(dt);
            }
        }

        /// <summary>
        /// 暗影球在砖块内部的移动逻辑
        /// 纯数学计算球与砖块矩形四条边（外侧 + 内侧）的相切点
        /// </summary>
        protected void ShadowMovement(float dt)
        {
            hasCorrectPosThisFixedUpdate = false;
            prePos = curPos;

            var range = moveSpeed * dt;
            var dir = Direction.normalized;

            // 如果上一帧已经撞到相切点且停在原地，这一帧触发手动碰撞伤害
            if (_tangentBallCenter != Vector3.zero && curPos == _tangentBallCenter)
            {
                if (collidingBrick)
                {
                    base.CollidingWithBrick(collidingBrick, _lastHitNormal);
                    EvaluateHit2D(collidingBrick.gameObject, _lastHitNormal, _lastHitPoint);
                }

                _tangentBallCenter = Vector3.zero;
                _lastHitNormal = default;
                _lastHitPoint = default;

                return;
            }

            // 计算球沿当前方向运动时，与砖块矩形边相切的第一个点（无论内外）
            if (TryCalculateInternalTangent(overlappingBrick, curPos, dir, Radius, out var tangentBallCenter, out var hitNormal, out var hitPoint))
            {
                _lastHitPoint = hitPoint;
                _lastHitNormal = hitNormal;
                _tangentBallCenter = tangentBallCenter;

                var moveVec = tangentBallCenter - (Vector2)curPos;
                var moveDist = moveVec.sqrMagnitude;

                if (moveDist <= range * range)
                {
                    // 只移动到相切位置
                    _movement = moveVec;
                    curPos = tangentBallCenter;

                    if (_hasRigidBody2D)
                        _rigidBody2D.MovePosition(tangentBallCenter);
                }
                else
                {
                    // 这一帧无法到达相切点，正常移动
                    _movement = dir * range;
                    curPos = curPos + _movement;

                    if (_hasRigidBody2D)
                        _rigidBody2D.MovePosition(curPos);
                }
            }
            else
            {
                // 没有相切点，正常移动
                _movement = dir * range;
                curPos = curPos + _movement;

                if (_hasRigidBody2D)
                    _rigidBody2D.MovePosition(curPos);
            }

            Debug.DrawLine(prePos, curPos, Color.red, dt);
            if (_tangentBallCenter != Vector3.zero)
                Debug.DrawLine(curPos, _tangentBallCenter, Color.green, dt);
        }

        /// <summary>
        /// 纯数学计算：球沿 dir 方向运动，与砖块内边相切的点。
        /// 思路：基于方向分量过滤候选内边，再取 t 最小的有效边作为命中边。
        ///   - dir.x &gt; 0：候选 = { 右内边 }
        ///   - dir.x &lt; 0：候选 = { 左内边 }
        ///   - dir.y &gt; 0：候选 = { 上内边 }
        ///   - dir.y &lt; 0：候选 = { 下内边 }
        /// 斜向时（dir.x 和 dir.y 都非零），候选包含两条边，取 t 最小的命中。
        ///
        /// 例如方向 (-0.889, 0.457)（朝左上）：
        ///   - 候选 = { 左内边, 上内边 }
        ///   - 若球发射点在右上角附近，撞上内边的 t 更小 → 选中上内边
        ///   - 若球发射点在右下角附近，撞左内边的 t 更小 → 选中左内边
        /// </summary>
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

            float tMin = float.PositiveInfinity;
            int hitSide = -1; // 0=左, 1=右, 2=下, 3=上

            // 根据 dir.x 分量过滤左右候选
            if (dir.x > 1e-6f)
            {
                // 候选：右内边（球心 x = xMax - radius）
                float tx = (xMax - radius - ballCenter.x) / dir.x;
                if (tx > 0f)
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
            else if (dir.x < -1e-6f)
            {
                // 候选：左内边（球心 x = xMin + radius）
                float tx = (xMin + radius - ballCenter.x) / dir.x;
                if (tx > 0f)
                {
                    float yAtT = ballCenter.y + dir.y * tx;
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

            // 根据 dir.y 分量过滤上下候选
            if (dir.y > 1e-6f)
            {
                // 候选：上内边（球心 y = yMax - radius）
                float ty = (yMax - radius - ballCenter.y) / dir.y;
                if (ty > 0f)
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
            else if (dir.y < -1e-6f)
            {
                // 候选：下内边（球心 y = yMin + radius）
                float ty = (yMin + radius - ballCenter.y) / dir.y;
                if (ty > 0f)
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

            if (hitSide < 0)
                return false;

            // 根据命中边计算相切球心位置、法线和击中点
            switch (hitSide)
            {
                case 0: // 左内边
                    tangentBallCenter = new Vector2(xMin + radius, ballCenter.y + dir.y * tMin);
                    hitPoint = new Vector2(xMin, tangentBallCenter.y);
                    hitNormal = Vector2.right; // 指向矩形内部
                    break;
                case 1: // 右内边
                    tangentBallCenter = new Vector2(xMax - radius, ballCenter.y + dir.y * tMin);
                    hitPoint = new Vector2(xMax, tangentBallCenter.y);
                    hitNormal = Vector2.left;
                    break;
                case 2: // 下内边
                    tangentBallCenter = new Vector2(ballCenter.x + dir.x * tMin, yMin + radius);
                    hitPoint = new Vector2(tangentBallCenter.x, yMin);
                    hitNormal = Vector2.up;
                    break;
                case 3: // 上内边
                    tangentBallCenter = new Vector2(ballCenter.x + dir.x * tMin, yMax - radius);
                    hitPoint = new Vector2(tangentBallCenter.x, yMax);
                    hitNormal = Vector2.down;
                    break;
            }

            return true;
        }

        /// <summary>
        /// 检测球进入/离开砖块的状态变化
        /// </summary>
        protected void DetectBrickStateChange()
        {
            if (isOverlappingBrick && overlappingBrick)
            {
                if (_lastOverlappingBrick != overlappingBrick)
                {
                    OnBrickEnter(overlappingBrick);
                    _lastOverlappingBrick = overlappingBrick;
                }
            }
            else
            {
                if (_lastOverlappingBrick)
                {
                    OnBrickExit(_lastOverlappingBrick);
                    _lastOverlappingBrick = null;
                }
            }
        }

        protected void OnBrickEnter(Brick brick)
        {
            if (!_enteredBricks.Contains(brick))
            {
                _enteredBricks.Add(brick);
                brick.Event.addListener<OnBrickDeath>(this);
                Debug.Log($"[Ball_Shadow] 球进入砖块: {brick.name}");
            }
        }

        protected void OnBrickExit(Brick brick)
        {
            if (_enteredBricks.Contains(brick))
            {
                brick.Event.removeListener<OnBrickDeath>(this);
                _enteredBricks.Remove(brick);
                Debug.Log($"[Ball_Shadow] 球离开砖块: {brick.name}");
            }
        }

        /// <summary>
        /// 从砖块中弹出暗影球
        /// </summary>
        protected void PopOutFromBrick(Brick brick)
        {
            var rect = brick.getRect();
            var circle = getCircle();

            var popDir = ((Vector2)curPos - rect.center).normalized;

            if (popDir.sqrMagnitude < 0.001f)
                popDir = Direction;

            popDir = popDir.normalized;

            float distToRight = rect.xMax - circle.mCenter.x;
            float distToLeft = circle.mCenter.x - rect.xMin;
            float distToTop = rect.yMax - circle.mCenter.y;
            float distToBottom = circle.mCenter.y - rect.yMin;

            float minDist = Mathf.Min(distToRight, distToLeft, distToTop, distToBottom);
            Vector2 exitDir;

            if (minDist.isEqual(distToRight))
                exitDir = Vector2.right;
            else if (minDist.isEqual(distToLeft))
                exitDir = Vector2.left;
            else if (minDist.isEqual(distToTop))
                exitDir = Vector2.up;
            else
                exitDir = Vector2.down;

            setDirection(exitDir);

            Debug.DrawLine(curPos, curPos + (Vector3)exitDir * 2f, Color.yellow, 1F);
            Debug.Log($"[Ball_Shadow] 砖块阵亡，球从边缘 {exitDir} 弹出");
        }

        public void onEvent(OnBrickDeath e)
        {
            e.brick.Event.removeListener<OnBrickDeath>(this);
            _enteredBricks.Remove(e.brick);
            PopOutFromBrick(e.brick);
        }
    }
}
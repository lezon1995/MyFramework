using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Requires a CharacterMovement ability. Makes the character move up to the specified MinimumDistance in the direction of the target. 
    /// </summary>
    public class EnemyMoveTowardsTargetAction : EnemyAction
    {
        [Tooltip("if this is true, movement will be constrained to not overstep a certain distance to the target on the x axis")]
        public bool UseMinimumXDistance = true;

        [Tooltip("the minimum distance from the target this Character can reach on the x axis.")]
        public float MinimumXDistance = 1f;

        [Tooltip("the movement constraint type (free, 4-dir, 8-dir, etc.)")]
        public Movements MovementConstraint = Movements.Free;

        protected Vector2 _direction;

        public override void Initialization()
        {
            if (!ShouldInitialize)
                return;

            base.Initialization();
        }

        public override void PerformAction(float dt)
        {
            if (_brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Move(dt);
        }

        /// <summary>
        /// Moves the character towards the target if needed
        /// 注意：这里只设置 _movement，CharacterMovement 会处理加速/减速并输出到 CurrentMovement
        /// TopDownController 会使用 CurrentMovement 进行实际移动
        /// </summary>
        protected virtual void Move(float dt)
        {
            if (_enemyBrain.NoTarget)
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            var targetPos = getTargetPosition();
            var selfPos = transform.position;
            if (UseMinimumXDistance)
            {
                _direction = CalculateMinimumDirection(selfPos, targetPos);
            }
            else
            {
                _direction = (targetPos - selfPos).normalized;
            }

            // 根据 MovementConstraint 限制移动方向
            _direction = ConstrainDirection(_direction);

            _movement.SetMovement(_direction);
        }

        Vector2 CalculateMinimumDirection(Vector3 selfPos, Vector3 targetPos)
        {
            var h = selfPos.x < targetPos.x ? 1f : -1f;
            var v = selfPos.y < targetPos.y ? 1f : -1f;

            var minimumXDistance = MinimumXDistance;
            switch (MovementConstraint)
            {
                case Movements.Free:
                    break;
                case Movements.Strict2DirectionsHorizontal:
                    minimumXDistance = 0F;
                    break;
                case Movements.Strict2DirectionsVertical:
                    minimumXDistance = 0F;
                    break;
                case Movements.Strict4Directions:
                    break;
                case Movements.Strict8Directions:
                    break;
            }

            if (Mathf.Abs(selfPos.x - targetPos.x) < minimumXDistance)
                h = 0F;

            if (Mathf.Abs(selfPos.y - targetPos.y) < minimumXDistance)
                v = 0F;

            return new(h, v);
        }

        /// <summary>
        /// 根据 Movements 枚举限制方向向量
        /// </summary>
        protected virtual Vector2 ConstrainDirection(Vector2 dir)
        {
            if (dir == Vector2.zero)
                return dir;

            switch (MovementConstraint)
            {
                case Movements.Free:
                    return dir;

                case Movements.Strict2DirectionsHorizontal:
                    if (dir.x.isZero(0.01F))
                        return new(0F, 0F);

                    return new(Mathf.Sign(dir.x), 0f);

                case Movements.Strict2DirectionsVertical:
                    if (dir.y.isZero(0.01F))
                        return new(0F, 0F);

                    return new(0f, Mathf.Sign(dir.y));

                case Movements.Strict4Directions:
                {
                    // 取 x/y 中绝对值更大的分量作为主方向
                    if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
                        return new(Mathf.Sign(dir.x), 0f);
                    return new(0f, Mathf.Sign(dir.y));
                }

                case Movements.Strict8Directions:
                {
                    // 将方向量化为 8 个方向之一
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    // 归一化到 [-180, 180]
                    angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
                    // 每 45° 一个区间，量化到最近的 8 方向
                    int sector = Mathf.RoundToInt(angle / 45f);
                    float quantizedAngle = sector * 45f * Mathf.Deg2Rad;
                    return new(Mathf.Cos(quantizedAngle), Mathf.Sin(quantizedAngle));
                }

                default:
                    return dir;
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _movement.SetMovement(Vector2.zero);
        }
    }
}
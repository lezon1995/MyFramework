using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Requires a CharacterMovement ability. Makes the character move in a zigzag pattern towards the target.
    /// </summary>
    public class EnemyZigzagMovementAction : EnemyAction
    {
        [Tooltip("the width of the zigzag pattern")]
        public float ZigzagWidth = 2f;

        [Tooltip("the frequency of direction changes")]
        public float ZigzagFrequency = 1f;

        [Tooltip("the speed multiplier during zigzag")]
        public float SpeedMultiplier = 0.8f;

        [Tooltip("the minimum distance to approach before starting zigzag")]
        public float MinDistanceToStart = 5f;

        protected Vector2 _direction;
        protected Vector2 _perpendicularDirection;
        protected float _timer;
        protected bool _zigzagRight = true;

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Zigzag();
        }

        protected virtual void Zigzag()
        {
            if (_brain.Target == null)
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            var targetPos = _brain.Target.position;
            var selfPos = transform.position;
            float distance = Vector3.Distance(selfPos, targetPos);

            // 如果太近，不进行 zigzag
            if (distance < MinDistanceToStart)
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            // 计算朝向目标的方向
            Vector2 toTarget = (targetPos - selfPos).normalized;

            // 更新垂直方向
            _perpendicularDirection = new Vector2(-toTarget.y, toTarget.x);

            // 定时切换方向
            _timer += Time.deltaTime * ZigzagFrequency;
            if (_timer >= 1f)
            {
                _timer = 0f;
                _zigzagRight = !_zigzagRight;
            }

            // 计算最终移动方向
            Vector2 zigzagOffset = _perpendicularDirection * (_zigzagRight ? ZigzagWidth : -ZigzagWidth);
            _direction = (toTarget + zigzagOffset * 0.5f).normalized * SpeedMultiplier;

            // 只通过 CharacterMovement 设置移动
            _movement.SetMovement(_direction);
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _movement.SetMovement(Vector2.zero);
        }
    }
}

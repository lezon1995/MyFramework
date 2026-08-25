using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Requires a CharacterMovement ability. Makes the character strafe around the target (circle strafe).
    /// </summary>
    public class EnemyStrafeAroundTargetAction : EnemyAction
    {
        [Tooltip("the direction to strafe (clockwise or counter-clockwise)")]
        public bool StrafeClockwise = true;

        [Tooltip("the radius to maintain around the target")]
        public float OrbitRadius = 5f;

        [Tooltip("the speed of strafing")]
        public float StrafeSpeed = 1f;

        [Tooltip("if true, will also approach/retreat to maintain orbit radius")]
        public bool MaintainOrbitRadius = true;

        protected Vector2 _direction;

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Strafe();
        }

        protected virtual void Strafe()
        {
            if (_brain.Target == null)
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            var targetPos = _brain.Target.position;
            var selfPos = transform.position;
            var toTarget = (targetPos - selfPos);

            // 计算切向方向（垂直于到目标的方向）
            Vector2 tangent = StrafeClockwise
                ? new Vector2(toTarget.y, -toTarget.x).normalized
                : new Vector2(-toTarget.y, toTarget.x).normalized;

            _direction = tangent * StrafeSpeed;

            // 如果需要保持轨道半径
            if (MaintainOrbitRadius)
            {
                float currentDistance = toTarget.magnitude;
                if (Mathf.Abs(currentDistance - OrbitRadius) > 0.5f)
                {
                    Vector2 radialDirection = toTarget.normalized;
                    float radialSpeed = currentDistance > OrbitRadius ? 1f : -1f;
                    _direction += radialDirection * radialSpeed * 0.5f;
                }
            }

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

using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An action that makes the character orbit around the target while maintaining a set distance.
    /// Useful for ranged enemies that want to keep their distance.
    /// </summary>
    public class EnemyOrbitAction : EnemyAction
    {
        [Tooltip("the radius to maintain around the target")]
        public float OrbitRadius = 5f;

        [Tooltip("the speed of orbiting")]
        public float OrbitSpeed = 1f;

        [Tooltip("the clockwise direction")]
        public bool Clockwise = true;

        [Tooltip("how fast to approach the target if too close")]
        public float ApproachSpeed = 1f;

        [Tooltip("the radius tolerance")]
        public float RadiusTolerance = 0.5f;

        protected Vector2 _direction;

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            Orbit();
        }

        protected virtual void Orbit()
        {
            if (_brain.Target == null)
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            var targetPos = _brain.Target.position;
            var selfPos = transform.position;
            var toTarget = (targetPos - selfPos);
            float currentDistance = toTarget.magnitude;

            // 切向速度（环绕）
            Vector2 tangent = Clockwise
                ? new Vector2(toTarget.y, -toTarget.x).normalized
                : new Vector2(-toTarget.y, toTarget.x).normalized;

            _direction = tangent * OrbitSpeed;

            // 径向速度（靠近/远离以保持距离）
            if (Mathf.Abs(currentDistance - OrbitRadius) > RadiusTolerance)
            {
                Vector2 radialDirection = toTarget.normalized;
                float radialSpeed = currentDistance > OrbitRadius ? ApproachSpeed : -ApproachSpeed;
                _direction += radialDirection * radialSpeed;
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

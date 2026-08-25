using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// This decision will return true if the target is within the character's facing direction (in front).
    /// </summary>
    public class EnemyDetectTargetInFrontDecision : EnemyDecision
    {
        [Tooltip("the angle tolerance in degrees")]
        [Range(0f, 180f)]
        public float AngleTolerance = 60f;

        [Tooltip("the distance to check")]
        public float Distance = 10f;

        protected CharacterOrientation2D _orientation2D;

        public override void Initialization()
        {
            base.Initialization();
            if (brick != null)
            {
                brick.FindAbility(out _orientation2D);
            }
        }

        public override bool Decide()
        {
            if (_brain.Target == null)
                return false;

            return IsTargetInFront();
        }

        protected virtual bool IsTargetInFront()
        {
            if (_orientation2D == null)
                return false;

            Vector2 directionToTarget = (_brain.Target.position - transform.position).normalized;
            Vector2 facingDirection = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;

            float angle = Vector2.Angle(facingDirection, directionToTarget);
            return angle < AngleTolerance;
        }
    }
}

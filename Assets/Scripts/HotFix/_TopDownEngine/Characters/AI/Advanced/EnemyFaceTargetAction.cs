using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An action that makes the character face towards the target without moving.
    /// Useful as a transition action or for turret-like enemies.
    /// </summary>
    public class EnemyFaceTargetAction : EnemyAction
    {
        protected CharacterOrientation2D _orientation2D;
        protected float _rotationSpeed = 10f;

        public override void Initialization()
        {
            base.Initialization();
            if (brick != null)
            {
                brick.FindAbility(out _orientation2D);
            }
        }

        public override void PerformAction(float dt)
        {
            if (brick.IsDead())
                return;

            FaceTarget();
        }

        protected virtual void FaceTarget()
        {
            if (_brain.Target == null || _orientation2D == null)
                return;

            if (transform.position.x > _brain.Target.position.x)
                _orientation2D.FaceDirection(-1);
            else
                _orientation2D.FaceDirection(1);
        }
    }
}

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
            if (_brick != null)
            {
                _brick.FindAbility(out _orientation2D);
            }
        }

        public override void PerformAction(float dt)
        {
            if (_brick.IsDead())
                return;

            FaceTarget();
        }

        protected virtual void FaceTarget()
        {
            if (_brain.NoTarget || _orientation2D == null)
                return;

            if (transform.position.x > getTargetPosition().x)
                _orientation2D.FaceDirection(-1);
            else
                _orientation2D.FaceDirection(1);
        }
    }
}

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// This decision will return true if the Brain's current target is alive, false otherwise
    /// </summary>
    public class EnemyDetectTargetIsAliveDecision : EnemyDecision
    {
        protected Character _character;

        /// <summary>
        /// On Decide we check whether the Target is alive or dead
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
        {
            return CheckIfTargetIsAlive();
        }

        /// <summary>
        /// Returns true if the Brain's Target is alive, false otherwise
        /// </summary>
        /// <returns></returns>
        protected virtual bool CheckIfTargetIsAlive()
        {
            if (_brain.Target == null)
                return false;

            if (_brain.Target.TryGetComponent(out _character))
                return _character.conditionState.Not(Character.Conditions.Dead);

            return false;
        }
    }
}
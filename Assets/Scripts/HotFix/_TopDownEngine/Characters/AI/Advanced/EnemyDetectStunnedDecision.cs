namespace MoreMountains
{
    /// <summary>
    /// This decision will return true if the character is stunned or in a negative condition state.
    /// </summary>
    public class EnemyDetectStunnedDecision : EnemyDecision
    {
        public override bool Decide()
        {
            if (brick == null)
                return false;

            var state = brick.conditionState.CurrentState;
            return state 
                is Character.Conditions.Stunned 
                or Character.Conditions.Frozen 
                or Character.Conditions.Poisoned;
        }
    }
}

using UnityEngine;

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

            return brick.conditionState.CurrentState == Character.Conditions.Stunned ||
                   brick.conditionState.CurrentState == Character.Conditions.Frozen ||
                   brick.conditionState.CurrentState == Character.Conditions.Poisoned;
        }
    }
}

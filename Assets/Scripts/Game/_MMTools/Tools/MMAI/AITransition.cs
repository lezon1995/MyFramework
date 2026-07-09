using System;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Transitions are a combination of one or more decisions and destination states
    /// whether or not these transitions are true or false.
    /// An example of a transition could be "_if an enemy gets in range, transition to the Shooting state_".
    /// </summary>
    [Serializable]
    public class AITransition
    {
        public AIDecision Decision;
        public string TrueState;
        public string FalseState;
    }
}
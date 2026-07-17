namespace MoreMountains
{
    public partial class Buff
    {
        public virtual void NotifyOnCombat(bool inCombat)
        {
            if (_refreshDurationWhileInCombat && inCombat)
            {
                RefreshDuration();
            }
        }

        public struct AfterRemoved
        {
            public Buff Buff;
            public AfterRemoved(Buff buff) => Buff = buff;
        }
    }
}
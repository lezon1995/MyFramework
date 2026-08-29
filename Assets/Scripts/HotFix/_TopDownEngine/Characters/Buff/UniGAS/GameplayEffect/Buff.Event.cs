using System;

namespace MoreMountains
{
    public partial class Buff
    {
        public Action<Character, Dmg> onTakeDmg { get; set; }

        public virtual void NotifyOnCombat(bool inCombat)
        {
            if (isRefreshDurationWhileInCombat && inCombat)
            {
                RefreshDuration();
            }
        }

        public struct AfterRemoved
        {
            public Buff Buff;
            public AfterRemoved(Buff buff) => Buff = buff;
        }

        public virtual void OnTakeDmg(OnDmg e)
        {
            onTakeDmg?.Invoke(e.Source, e.Dmg);
        }
    }
}
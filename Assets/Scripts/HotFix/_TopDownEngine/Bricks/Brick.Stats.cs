namespace MoreMountains
{
    public partial class Brick : IStatsGetter<Brick.Stat>
    {
        public new enum Stat
        {
            HealthMax, //Health Point
            HealthRegen, //Health Point Regen(per 1s)
            AD, //Attack Damage
            AP, //Ability Power
            AR, //Attack Damage Defence
            MR, //Ability Power Defence
            AS, //Attack Speed
            MS, //Move Speed
            CritChance, //Crit Chance
            CritDamage, //Crit Damage
            DmgRate, //DmgRate
            DodgeChance, //Dodge Chance
            KnockbackResistance,
        }

        public UniStats.Stat GetStat(Stat key)
        {
            return _hasStats ? Stats.GetStat(key.Key()) : null;
        }

        public bool GetStat(Stat key, out UniStats.Stat stat)
        {
            if (!_hasStats)
            {
                stat = null;
                return false;
            }

            return Stats.GetStat(key.Key(), out stat);
        }
    }
}
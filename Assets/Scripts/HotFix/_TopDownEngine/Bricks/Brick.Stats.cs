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
            return Stats == null ? null : Stats.GetStat(key.Key());
        }

        public bool GetStat(Stat key, out UniStats.Stat stat)
        {
            if (Stats == null)
            {
                stat = null;
                return false;
            }

            return Stats.GetStat(key.Key(), out stat);
        }
    }
}
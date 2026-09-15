namespace MoreMountains
{
    public partial class APlayer
    {
        public float ad => GetStat(Stat.AD).Value;
        public float ap => GetStat(Stat.AP).Value;
        public float armor => GetStat(Stat.AR).Value;
        public float magicResist => GetStat(Stat.MR).Value;
        public float attackSpeed => GetStat(Stat.AS).Value;
        public float cooldownHaste => GetStat(Stat.CD).Value;

        //已经削减冷却百分比
        public float cooldownPctReduced
        {
            get
            {
                var haste = cooldownHaste;
                var cd = haste / (haste + 100);
                return cd;
            }
        }

        //当前冷却百分比
        public float cooldownPct
        {
            get
            {
                var haste = cooldownHaste;
                var pct = 100 / (haste + 100);
                return pct;
            }
        }

        public float moveSpeed => GetStat(Stat.MS).Value;
        public float triggerChance => GetStat(Stat.TriggerChance).Value;
        public float durationPct
        {
            get
            {
                var value = GetStat(Stat.Duration).Value;
                var pct = 1 + value;
                return pct;
            }
        }
    }
}
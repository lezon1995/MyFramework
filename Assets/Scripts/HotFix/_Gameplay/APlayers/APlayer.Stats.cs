namespace MoreMountains
{
    public partial class APlayer
    {
        public float ad => GetStat(Stat.AD).Value;
        public float ap => GetStat(Stat.AP).Value;
        public float armor => GetStat(Stat.AR).Value;
        public float magicResist => GetStat(Stat.MR).Value;
        public float attackSpeed => GetStat(Stat.AS).Value;
        public float cooldown => GetStat(Stat.CD).Value;
        public float moveSpeed => GetStat(Stat.MS).Value;
    }
}
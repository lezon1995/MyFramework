using System;

namespace MoreMountains
{
    [Serializable]
    public class PlayerStatMod
    {
        public Character.Stat stat;
        public float BonusFlat;
        public float BonusPct;
    }
}
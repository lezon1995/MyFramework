using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/BrickStatsTemplate", fileName = "BrickStatsTemplate")]
    public class BrickStatsTemplate : StatsTemplate
    {
        protected override IEnumerable<string> GetNames()
        {
            var values = (Brick.Stat[])Enum.GetValues(typeof(Brick.Stat));
            return values.Select(stat => stat.Key());
        }
    }

    public static class BrickStatsTemplateExtensions
    {
        public static string Key(this Brick.Stat stat)
        {
            return stat switch
            {
                Brick.Stat.HealthMax => Stats.HealthMax,
                Brick.Stat.HealthRegen => Stats.HealthRegen,
                Brick.Stat.AD => Stats.AD,
                Brick.Stat.AR => Stats.AR,
                Brick.Stat.AP => Stats.AP,
                Brick.Stat.MR => Stats.MR,
                Brick.Stat.AS => Stats.AS,
                Brick.Stat.MS => Stats.MS,
                Brick.Stat.CritChance => Stats.CritChance,
                Brick.Stat.CritDamage => Stats.CritDamage,
                Brick.Stat.DmgRate => Stats.DmgRate,
                Brick.Stat.DodgeChance => Stats.DodgeChance,
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }
    }
}
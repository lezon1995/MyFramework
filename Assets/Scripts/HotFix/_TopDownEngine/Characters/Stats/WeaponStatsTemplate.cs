using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/WeaponStatsTemplate", fileName = "WeaponStatsTemplate")]
    public class WeaponStatsTemplate : StatsTemplate
    {
        protected override IEnumerable<string> GetNames()
        {
            var values = (Weapon.Stat[])Enum.GetValues(typeof(Weapon.Stat));
            return values.Select(stat => stat.Key());
        }
    }
}
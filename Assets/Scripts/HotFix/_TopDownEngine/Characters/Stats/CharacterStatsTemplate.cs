using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/CharacterStatsTemplate", fileName = "CharacterStatsTemplate")]
    public class CharacterStatsTemplate : StatsTemplate
    {
        protected override IEnumerable<string> GetNames()
        {
            var values = (Character.Stat[])Enum.GetValues(typeof(Character.Stat));
            return values.Where(stat => stat != Character.Stat.None).Select(stat => stat.Key());
        }
    }
}
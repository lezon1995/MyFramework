using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/BallStatsTemplate", fileName = "BallStatsTemplate")]
    public class BallStatsTemplate : StatsTemplate
    {
        protected override IEnumerable<string> GetNames()
        {
            var values = (Ball.Stat[])Enum.GetValues(typeof(Ball.Stat));
            return values.Select(stat => stat.Key());
        }
    }
}
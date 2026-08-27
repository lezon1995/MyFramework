using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/BallStatsTemplate", fileName = "BallStatsTemplate")]
    public class BallStatsTemplate : StatsTemplate
    {
        static string[] levelInitialNames =
        {
            Ball.Stat.HitDamage.Key(),
            Ball.Stat.EffectDamage.Key(),
            Ball.Stat.AS.Key(),
            Ball.Stat.BallisticSpeed.Key(),
            Ball.Stat.Duration.Key(),
            Ball.Stat.CritChance.Key(),
            Ball.Stat.CritDamage.Key(),
            Ball.Stat.Knockback.Key(),
            Ball.Stat.Range.Key(),
        };

        protected override IEnumerable<string> GetNames()
        {
            var values = (Ball.Stat[])Enum.GetValues(typeof(Ball.Stat));
            return values.Select(stat => stat.Key());
        }

        protected override void FillLevelInitialValues()
        {
            var initialNames = GetLevelInitialNames();
            if (initialNames is { Length: > 0 })
            {
                names.Clear();
                foreach (var statName in initialNames)
                {
                    names.Add(statName);
                    var values = new float[4];
                    var defaultValue = Configs[statName];
                    values[0] = defaultValue;
                    values[1] = defaultValue;
                    values[2] = defaultValue;
                    values[3] = defaultValue;
                    LevelInitialValues.TryAdd(statName, values);
                }

                foreach (var key in Configs.Keys)
                {
                    if (!names.Contains(key))
                    {
                        LevelInitialValues.Remove(key);
                    }
                }
            }
            else
            {
                LevelInitialValues.Clear();
            }
        }

        protected override string[] GetLevelInitialNames()
        {
            return levelInitialNames;
        }
    }
}
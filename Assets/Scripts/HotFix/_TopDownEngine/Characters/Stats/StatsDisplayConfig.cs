using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(fileName = "StatsDisplayConfig", menuName = "StatsDisplayConfig", order = 0)]
    public class StatsDisplayConfig : ScriptableObject
    {
        public List<DisplayConfig> configs = new();

        public bool TryGetDisplayConfig(string statName, out DisplayConfig c)
        {
            foreach (var config in configs)
            {
                if (config.statName == statName)
                {
                    c = config;
                    return true;
                }
            }

            c = DisplayConfig.Flat;
            return false;
        }
    }
}
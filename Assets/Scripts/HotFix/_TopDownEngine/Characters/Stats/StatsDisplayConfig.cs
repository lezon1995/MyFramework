using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(fileName = "StatsDisplayConfig", menuName = "StatsDisplayConfig", order = 0)]
    public class StatsDisplayConfig : ScriptableObject
    {
        static StatsDisplayConfig sInstance;

        public static StatsDisplayConfig Instance
        {
            get
            {
                if (sInstance == null)
                {
                    string path = $"{GAMEPLAY_PATH}/Characters/Stats/StatsDisplayConfig.asset";
                    sInstance = resource.loadGameResource<StatsDisplayConfig>(path);
                }

                return sInstance;
            }
        }
        
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
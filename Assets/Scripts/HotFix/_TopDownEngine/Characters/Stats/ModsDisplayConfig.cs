using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(fileName = "ModsDisplayConfig", menuName = "ModsDisplayConfig", order = 0)]
    public class ModsDisplayConfig : ScriptableObject
    {
        static ModsDisplayConfig sInstance;

        public static ModsDisplayConfig Instance
        {
            get
            {
                if (sInstance == null)
                {
                    string path = $"{GAMEPLAY_PATH}/Characters/Stats/ModsDisplayConfig.asset";
                    sInstance = resource.loadGameResource<ModsDisplayConfig>(path);
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
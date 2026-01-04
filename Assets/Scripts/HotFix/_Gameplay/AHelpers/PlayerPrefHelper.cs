using UnityEngine;

namespace MarbleHero
{
    public static class PlayerPrefHelper
    {
        public static int GetInt(string priKey, string subKey, int def = default)
        {
            var key = $"{priKey}.{subKey}";
            return PlayerPrefs.GetInt(key, def);
        }

        public static float GetFloat(string priKey, string subKey, float def = default)
        {
            var key = $"{priKey}.{subKey}";
            return PlayerPrefs.GetFloat(key, def);
        }

        public static string GetString(string priKey, string subKey, string def = default)
        {
            var key = $"{priKey}.{subKey}";
            
            return PlayerPrefs.GetString(key, def);
        }
    }
}
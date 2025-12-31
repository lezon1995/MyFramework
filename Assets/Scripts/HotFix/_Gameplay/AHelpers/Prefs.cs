using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace MarbleHero
{
    public class Prefs
    {
        public string FilePath;
        public Dictionary<string, string> data = new();

        public Prefs(string name)
        {
            FilePath = Application.persistentDataPath + "/" + name + ".json";
        }

        public string getString(string key) => data.GetValueOrDefault(key, string.Empty);
        public string getString(string key, string def) => data.GetValueOrDefault(key, def);

        public void putString(string key, string value)
        {
            data[key] = value;
        }

        public int getInteger(string key)
        {
            if (data.TryGetValue(key, out var value))
                return int.Parse(value.Trim());
            return -999;
        }

        public int getInteger(string key, int def)
        {
            if (data.TryGetValue(key, out var value))
                return int.Parse(value.Trim());
            return def;
        }

        public void putInteger(string key, int value)
        {
            data[key] = value.ToString();
        }

        public float getFloat(string key, float def)
        {
            if (data.TryGetValue(key, out var value))
                return float.Parse(value.Trim());
            return def;
        }

        public void putFloat(string key, float value)
        {
            data[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        public long getLong(string key, long def)
        {
            if (data.TryGetValue(key, out var value))
                return long.Parse(value.Trim());
            return def;
        }

        public void putLong(string key, long value)
        {
            data[key] = value.ToString();
        }

        public bool getBoolean(string key, bool def)
        {
            if (data.TryGetValue(key, out var value))
                return bool.Parse(value.Trim());
            return def;
        }

        public bool getBoolean(string key)
        {
            return bool.Parse(data[key].Trim());
        }

        public void putBoolean(string key, bool value)
        {
            data[key] = value.ToString();
        }

        public void flush()
        {
            AsyncSaver.save(FilePath, JsonConvert.SerializeObject(data));
        }

        public Dictionary<string, string> get() => data;
    }
}
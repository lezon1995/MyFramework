using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace MarbleHero
{
    public class BuildSettings
    {
        Prefs prop;
        public static string defaultFilename = "build.properties";

        public BuildSettings(string path)
        {
            prop = new(path);
            var textAsset = Resources.Load<TextAsset>(path);

            var json = textAsset.text;
            prop.data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public string getDistributor()
        {
            var distributor = prop.getString("distributor");
            if (distributor != null)
                return distributor;
            throw new BuildSettingsException("The key 'distributor' is null in file=build.properties");
        }
    }

    public class BuildSettingsException : Exception
    {
        public BuildSettingsException(string message) : base(message)
        {
        }
    }
}
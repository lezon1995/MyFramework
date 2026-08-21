using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace MoreMountains;

public static class LocalizedStats
{
    static StringTable stringTable => LocalizationSettings.StringDatabase.GetTable("Stats");

    public static string getName(string statKey)
    {
        var entry = stringTable.GetEntry(statKey);
        if (entry != null)
        {
            var str = entry.Value;
            return str;
        }

        return null;
    }

    public static string getDodged()
    {
        var entry = stringTable.GetEntry("Dodged");
        if (entry != null)
        {
            var str = entry.Value;
            return str;
        }

        return null;
    }
}
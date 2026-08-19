using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace MoreMountains;

public static class LocalizedStats
{
    static StringTable stringTable => LocalizationSettings.StringDatabase.GetTable("Stats");

    public static string getName(string statKey)
    {
        var str = stringTable.GetEntry(statKey).Value;
        return str;
    }
    public static string getDodged()
    {
        var str = stringTable.GetEntry("Dodged").Value;
        return str;
    }
}
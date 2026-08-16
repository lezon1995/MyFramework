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
}
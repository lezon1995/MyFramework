using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public static class LocalizationExtensions
{
    public static void setInt(this LocalizeStringEvent stringEvent, string arg, int value)
    {
        if (stringEvent.StringReference[arg] is IntVariable variable)
        {
            variable.Value = value;
        }
        else
        {
            var intVariable = new IntVariable
            {
                Value = value
            };
            stringEvent.StringReference[arg] = intVariable;
        }

        stringEvent.RefreshString();
    }
}
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

    public static void setInt(this LocalizeStringEvent stringEvent
        , string arg1, int value1
        , string arg2, int value2
    )
    {
        if (stringEvent.StringReference[arg1] is IntVariable variable1)
        {
            variable1.Value = value1;
        }
        else
        {
            var intVariable = new IntVariable
            {
                Value = value1
            };
            stringEvent.StringReference[arg1] = intVariable;
        }

        if (stringEvent.StringReference[arg2] is IntVariable variable2)
        {
            variable2.Value = value2;
        }
        else
        {
            var intVariable = new IntVariable
            {
                Value = value2
            };
            stringEvent.StringReference[arg2] = intVariable;
        }

        stringEvent.RefreshString();
    }
}
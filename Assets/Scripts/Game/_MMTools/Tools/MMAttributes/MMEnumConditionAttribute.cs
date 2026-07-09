using System;
using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// An attribute to conditionally hide fields based on the current selection in an enum.
    /// Usage :  [MMEnumCondition("rotationMode", (int)RotationMode.LookAtTarget, (int)RotationMode.RotateToAngles)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class MMEnumConditionAttribute : PropertyAttribute
    {
        public string ConditionEnum;
        public bool Hidden;

        BitArray bitArray = new BitArray(32);

        public bool ContainsBitFlag(int enumValue)
        {
            return bitArray.Get(enumValue);
        }

        public MMEnumConditionAttribute(string conditionBoolean, params int[] enumValues)
        {
            ConditionEnum = conditionBoolean;
            Hidden = true;

            for (int i = 0; i < enumValues.Length; i++)
            {
                bitArray.Set(enumValues[i], true);
            }
        }
    }
}
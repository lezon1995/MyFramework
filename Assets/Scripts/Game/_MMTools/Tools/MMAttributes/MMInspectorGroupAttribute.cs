using System;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// An attribute used to group inspector fields under common dropdowns
    /// Implementation inspired by Rodrigo Prinheiro's work, available at https://github.com/RodrigoPrinheiro/unityFoldoutAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class MMInspectorGroupAttribute : PropertyAttribute
    {
        public string GroupName;
        public bool GroupAllFieldsUntilNextGroupAttribute;
        public int GroupColorIndex;
        public bool ClosedByDefault;

        public MMInspectorGroupAttribute(string groupName, int groupColorIndex = -1, bool groupAllFieldsUntilNextGroupAttribute = true, bool closedByDefault = false)
        {
            GroupName = groupName;
            GroupAllFieldsUntilNextGroupAttribute = groupAllFieldsUntilNextGroupAttribute;
            GroupColorIndex = groupColorIndex;
            ClosedByDefault = closedByDefault;
        }

        public int GetColorIndex(int _default)
        {
            if (GroupColorIndex == -1)
                return _default;

            return GroupColorIndex;
        }
    }
}
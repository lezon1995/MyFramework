using System;
using Sirenix.OdinInspector;

namespace MoreMountains.TopDownEngine
{
    public partial class BuffType : SerializedScriptableObject
    {
        public OverflowConfig overflow;
        
        [Serializable, HideLabel]
        [BoxGroup("Overflow", order: 5), ShowIfGroup("Overflow/Toggle", Condition = STACKABLE)]
        public class OverflowConfig
        {
            [ToggleLeft]
            public bool DenyOverflowApplication;

            [ToggleLeft]
            public bool ClearStackOnOverflow;

            public Buff.Data[] OverflowBuffs;
        }
    }
}
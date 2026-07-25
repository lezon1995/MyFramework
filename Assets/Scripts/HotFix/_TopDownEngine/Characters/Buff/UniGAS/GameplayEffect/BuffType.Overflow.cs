using System;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    public partial class Buff
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

            public Data[] OverflowBuffs;
        }
    }
}
using System;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    public partial class BuffType : SerializedScriptableObject
    {
        [Serializable, HideLabel]
        [BoxGroup("Event", order: 9), HideIfGroup("Event/Toggle", Condition = EVENT)]
        public class EventConfig
        {
            public EventItem OnKilling;

            public class EventItem
            {
                [HorizontalGroup, HideLabel]
                public bool On;

                [HorizontalGroup]
                public Buff.Data[] Buffs;
            }
        }
    }
}
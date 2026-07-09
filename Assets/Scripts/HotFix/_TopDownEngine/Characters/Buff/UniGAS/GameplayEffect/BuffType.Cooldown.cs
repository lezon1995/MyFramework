using System;
using Sirenix.OdinInspector;

namespace MoreMountains.TopDownEngine
{
    public partial class BuffType : SerializedScriptableObject
    {
        public CooldownConfig cooldown;

        [Serializable, HideLabel]
        [BoxGroup("Cooldown", order: 8), ShowIfGroup("Cooldown/Toggle", Condition = COOLDOWN)]
        public class CooldownConfig
        {
            [BoxGroup("Duration", false), InlineProperty]
            public Buff.Mag Duration;

            public Buff.Actors CooldownAt;
        }
    }
}